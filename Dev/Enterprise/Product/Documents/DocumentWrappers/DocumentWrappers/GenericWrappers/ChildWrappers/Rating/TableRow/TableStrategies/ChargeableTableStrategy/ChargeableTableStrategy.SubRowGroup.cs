using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocAmount;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	partial class ChargeableTableStrategy
	{
		[DebuggerDisplay("Count = {Count}")]
		sealed class SubRowGroup : IList<SubRow>
		{
			public SubRowGroup(RateLine rateLine, Column[] columns, DocAmount[] values, bool? isLocalClient = null)
			{
				Array.Sort(columns, values);
				this.columns = Array.AsReadOnly(columns);
				this.subRows = new List<SubRow>() { new SubRow(rateLine.ChargeCode, rateLine.GetMultilingualRateDesc(isLocalClient), rateLine.Currency, rateLine.UseOnlyActualWeightMeasure, rateLine.ConversionFactorForDocumentPrintingOnly, values) };
			}

			public bool TryAbsorb(SubRowGroup group)
			{
				if (group == null || group.columns.Count > columns.Count)
				{
					return false;
				}

				var reMapping = new int[group.columns.Count];
				var allBreaks = new List<int>();

				var i1 = 0;
				var i2 = 0;

				while (i1 < columns.Count && i2 < group.columns.Count)
				{
					var c1 = columns[i1];
					var c2 = group.columns[i2];

					int diff = c1.CompareTo(c2);

					if (diff == 0)
					{
						reMapping[i2] = i1;
						i1++;
						i2++;
					}
					else if (c2.Type == ColumnType.Unit && (c1.Type == ColumnType.Minus || c1.Type == ColumnType.Plus))
					{
						// a "Unit" value expands into all breaks.
						do
						{
							allBreaks.Add(i1);

							i1++;

							if (i1 >= columns.Count)
							{
								break;
							}

							c1 = columns[i1];
						}
						while (c1.Type == ColumnType.Minus || c1.Type == ColumnType.Plus);

						reMapping[i2] = -1;
						i2++;
					}
					else if (diff < 0)
					{
						i1++;
					}
					else
					{
						return false;
					}
				}

				if (i2 < group.columns.Count)
				{
					return false;
				}

				foreach (var row in group.subRows)
				{
					var newLine = DocAmount.Create(count: columns.Count);

					for (var i = 0; i < row.Count; i++)
					{
						var target = reMapping[i];

						if (target < 0)
						{
							for (target = 0; target < allBreaks.Count; target++)
							{
								newLine[allBreaks[target]] = row[i];
							}
						}
						else
						{
							newLine[target] = row[i];
						}
					}

					subRows.Add(new SubRow(row.ChargeCode, row.ChargeDescription, row.Currency, row.UseOnlyActualWeightMeasure, row.ConversionFactor, newLine));
				}

				return true;
			}

			public ReadOnlyCollection<Column> Columns
			{
				[DebuggerStepThrough]
				get { return columns; }
			}

			public SubRow this[int index] => subRows[index];

			#region IList<SubRow> Members

			int IList<SubRow>.IndexOf(SubRow item) => subRows.IndexOf(item);

			void IList<SubRow>.Insert(int index, SubRow item) => throw new NotSupportedException();

			void IList<SubRow>.RemoveAt(int index) => throw new NotImplementedException();

			SubRow IList<SubRow>.this[int index]
			{
				get { return subRows[index]; }
				set { throw new NotSupportedException(); }
			}

			#endregion

			#region ICollection<SubRow> Members

			void ICollection<SubRow>.Add(SubRow item) => throw new NotSupportedException();

			void ICollection<SubRow>.Clear() => throw new NotSupportedException();

			bool ICollection<SubRow>.Contains(SubRow item) => subRows.Contains(item);

			void ICollection<SubRow>.CopyTo(SubRow[] array, int arrayIndex)
			{
				subRows.CopyTo(array, arrayIndex);
			}

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			public int Count => subRows.Count;

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			bool ICollection<SubRow>.IsReadOnly
			{
				[DebuggerStepThrough]
				get { return true; }
			}

			bool ICollection<SubRow>.Remove(SubRow item) => throw new NotSupportedException();

			#endregion

			#region IEnumerable<SubRow> Members

			public IEnumerator<SubRow> GetEnumerator() => subRows.GetEnumerator();

			#endregion

			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

			#endregion

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			readonly ReadOnlyCollection<Column> columns;

			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			readonly List<SubRow> subRows;
		}
	}
}
