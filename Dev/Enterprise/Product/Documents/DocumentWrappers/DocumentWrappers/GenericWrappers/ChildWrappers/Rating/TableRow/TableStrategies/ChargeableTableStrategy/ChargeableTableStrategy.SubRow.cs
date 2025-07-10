using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocAmount;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	partial class ChargeableTableStrategy
	{
		[DebuggerDisplay("Count = {Count}")]
		sealed class SubRow : IList<DocAmount>
		{
			public SubRow(AccChargeCode charge, ZString chargeDescription, RefCurrency currency, bool useOnlyActualWeightMeasure, ConversionFactor conversionFactor, DocAmount[] values)
			{
				this.ChargeCode = charge;
				this.ChargeDescription = chargeDescription;
				this.Currency = currency;
				this.UseOnlyActualWeightMeasure = useOnlyActualWeightMeasure;
				this.ConversionFactor = conversionFactor;
				this.values = values;
			}

			public AccChargeCode ChargeCode { get; private set; }
			public ZString ChargeDescription { get; private set; }
			public RefCurrency Currency { get; private set; }
			public bool UseOnlyActualWeightMeasure { get; private set; }
			public ConversionFactor ConversionFactor { get; private set; }

			public DocAmount this[int index] => values[index];

			#region IList<ZString> Members

			int IList<DocAmount>.IndexOf(DocAmount item) => Array.IndexOf(values, item);

			void IList<DocAmount>.Insert(int index, DocAmount item) => throw new NotSupportedException();

			void IList<DocAmount>.RemoveAt(int index) => throw new NotSupportedException();

			DocAmount IList<DocAmount>.this[int index]
			{
				get { return values[index]; }
				set { throw new NotSupportedException(); }
			}

			#endregion

			#region ICollection<ZString> Members

			void ICollection<DocAmount>.Add(DocAmount item) => throw new NotSupportedException();

			void ICollection<DocAmount>.Clear() => throw new NotSupportedException();

			bool ICollection<DocAmount>.Contains(DocAmount item) => Array.IndexOf(values, item) >= 0;

			void ICollection<DocAmount>.CopyTo(DocAmount[] array, int arrayIndex)
			{
				Array.Copy(values, array, values.Length);
			}

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			public int Count
			{
				[DebuggerStepThrough]
				get { return values.Length; }
			}

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			bool ICollection<DocAmount>.IsReadOnly
			{
				[DebuggerStepThrough]
				get { return true; }
			}

			bool ICollection<DocAmount>.Remove(DocAmount item) => throw new NotSupportedException();

			#endregion

			#region IEnumerable<ZString> Members

			public IEnumerator<DocAmount> GetEnumerator() => ((IEnumerable<DocAmount>)values).GetEnumerator();

			#endregion

			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

			#endregion

			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			readonly DocAmount[] values;
		}
	}
}
