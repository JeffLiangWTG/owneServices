using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class FieldDefaultingStrategyList : ReadOnlyCodeDescriptionPairList
	{
		public FieldDefaultingStrategyList()
		{
			this.fieldSupporter = null;
		}

		public FieldDefaultingStrategyList(OperationalActionFieldSupporter fieldSupporter, IEnumerable<IFieldDefaultingStrategy> strategies)
		{
			if (fieldSupporter == null)
			{
				throw new ArgumentNullException(nameof(fieldSupporter));
			}

			if (strategies == null)
			{
				throw new ArgumentNullException(nameof(strategies));
			}

			this.fieldSupporter = fieldSupporter;

			foreach (IFieldDefaultingStrategy strategy in strategies)
			{
				Elements.Add(strategy);
			}
		}

		public OperationalActionFieldSupporter FieldSupporter
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fieldSupporter; }
		}

		public new IFieldDefaultingStrategy this[int index]
		{
			get { return (IFieldDefaultingStrategy)base[index]; }
		}

		public IFieldDefaultingStrategy this[string code]
		{
			get
			{
				int index = IndexOfCode(code);
				if (index < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(code), code, "unknown delivery method");
				}

				return this[index];
			}
		}

		public FieldType DetailFieldType(string code)
		{
			int index = IndexOfCode(code);
			return index < 0 ? FieldType.Text : this[index].DetailFieldType;
		}

		public bool UsesDetail(string code)
		{
			return IndexOfCode(code) >= 0;
		}

		public int DetailMaxLength(string code)
		{
			int index = IndexOfCode(code);
			return index < 0 ? 0 : this[index].DetailMaxLength;
		}

		[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
		public IList GetBoundCollection(string code, BusinessObjectFactory factory)
		{
			int index = IndexOfCode(code);
			return index < 0 ? new ReadOnlyCodeDescriptionPairList() : this[index].GetBoundCollection(factory);
		}

		public IZType GetDefaultValue(string code, string detail)
		{
			int index = IndexOfCode(code);
			return index < 0 ? null : this[index].GetDefaultValue(detail);
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly OperationalActionFieldSupporter fieldSupporter;
	}
}
