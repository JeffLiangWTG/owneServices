using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	public class B2DutyAndTaxSynchroniser : BusinessObjectSynchroniser
	{
		public B2DutyAndTaxSynchroniser(DutyAndTax destination, DutyAndTax source)
			: base(destination, source)
		{ }

		public new DutyAndTax Source
		{
			get { return (DutyAndTax)base.Source; }
		}

		public new DutyAndTax Destination
		{
			get { return (DutyAndTax)base.Destination; }
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			if (Source != null && !Source.IsDeleted && Destination != null && !Destination.IsDeleted)
			{
				Synchronisers.Add(NewFieldSynchroniser(Destination.C1_CodeInfo, Source.C1_CodeInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.C1_ExemptCodeInfo, Source.C1_ExemptCodeInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.C1_RateTypeInfo, Source.C1_RateTypeInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.C1_RateInfo, Source.C1_RateInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.C1_UnitOfMeasureInfo, Source.C1_UnitOfMeasureInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.C1_PreviousTranLineInfo, Source.C1_PreviousTranLineInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.C1_PreviousTranNumberInfo, Source.C1_PreviousTranNumberInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.C1_NormalValuePerUnitInfo, Source.C1_NormalValuePerUnitInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.C1_NormalValueCurrencyInfo, Source.C1_NormalValueCurrencyInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.C1_ForeignRateInfo, Source.C1_ForeignRateInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.C1_ForeignCurrencyInfo, Source.C1_ForeignCurrencyInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.C1_OverrideInfo, Source.C1_OverrideInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.C1_AmountInfo, Source.C1_AmountInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.C1_TaxTypeInfo, Source.C1_TaxTypeInfo));
			}
		}

		FieldSynchroniser NewFieldSynchroniser(ZPropertyInfo destination, ZPropertyInfo source)
		{
			var lastValue = source.Value;
			return new FieldSynchroniser(destination,
				() =>
				{
					IZType result;
					if (destination.Value.IsDefault || destination.Value.IsEmpty || destination.Value.Equals(lastValue))
					{
						result = source.Value;
					}
					else
					{
						result = destination.Value;
					}
					lastValue = source.Value;
					return result;
				},
				() => { return new[] { source }; },
				true);
		}
	}
}
