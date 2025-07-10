using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public class CollectionSequencer<T>
		where T : BusinessObject
	{
		public CollectionSequencer(ActiveBusinessObjectCollection<T> collection, SchemaNumericColumn sequenceColumn, Action<T> sequenceValidation)
		{
			Collection = Argument.NotNull(collection, "collection");
			SequenceColumn = Argument.NotNull(sequenceColumn, "sequenceColumn");
			SequenceValidation = Argument.NotNull(sequenceValidation, "validation");
		}

		readonly ActiveBusinessObjectCollection<T> Collection;
		readonly SchemaNumericColumn SequenceColumn;
		readonly Action<T> SequenceValidation;

		#region Move

		public void MoveUp(T bizO)
		{
			if (bizO != null)
			{
				SortBySequence();

				var bizOToSwapWith = Collection.LastOrDefault(i => ZInt.ParseSafe(i[SequenceColumn].ToString(), 0) < ZInt.ParseSafe(bizO[SequenceColumn].ToString(), 0));
				if (bizOToSwapWith != null)
				{
					SwapSequence(bizO, bizOToSwapWith);
				}
			}
		}

		public void MoveDown(T bizO)
		{
			if (bizO != null)
			{
				SortBySequence();

				var bizOToSwapWith = Collection.FirstOrDefault(i => ZInt.ParseSafe(i[SequenceColumn].ToString(), 0) > ZInt.ParseSafe(bizO[SequenceColumn].ToString(), 0));
				if (bizOToSwapWith != null)
				{
					SwapSequence(bizO, bizOToSwapWith);
				}
			}
		}

		void SwapSequence(T bizO1, T bizO2)
		{
			Argument.NotNull(bizO1, "bizO1");
			Argument.NotNull(bizO2, "bizO2");

			using (bizO1.GetValidationSuspender())
			using (bizO2.GetValidationSuspender())
			{
				var bizO1_Sequence = bizO1[SequenceColumn];
				bizO1[SequenceColumn] = bizO2[SequenceColumn];
				bizO2[SequenceColumn] = bizO1_Sequence;
			}
		}

		#endregion

		#region Sequence

		public void Sequence()
		{
			SortBySequence();
			var instructions = Collection.Where(i => !i.IsDeleted).ToArray();

			for (int i = 0; i < instructions.Length; i++)
			{
				instructions[i][SequenceColumn] = GetValueInColumnType(SequenceColumn.GetEquivalentZType(), i + 1);
			}

			foreach (var instruction in instructions)
			{
				SequenceValidation(instruction);
			}
		}

		INumericZType GetValueInColumnType(Type columnType, int value)
		{
			INumericZType result;

			if (columnType == typeof(ZInt))
			{
				result = (ZInt)value;
			}
			else if (columnType == typeof(ZByte))
			{
				result = ZByte.ParseSafe(value.ToString(CultureInfo.InvariantCulture), 0);
			}
			else if (columnType == typeof(ZDecimal))
			{
				result = ZDecimal.ParseSafe(value.ToString(CultureInfo.InvariantCulture), 0);
			}
			else if (columnType == typeof(ZShort))
			{
				result = ZShort.ParseSafe(value.ToString(CultureInfo.InvariantCulture), 0);
			}
			else
			{
				throw new NotSupportedException("The column you are sequencing is not supported by the CollectionSequencer: <" + columnType.FullName + ">.");
			}

			return result;
		}

		#endregion

		#region SortBySequence

		public void SortBySequence()
		{
			Collection.ApplySort(SequenceColumn.Name, ListSortDirection.Ascending);
		}

		#endregion
	}
}
