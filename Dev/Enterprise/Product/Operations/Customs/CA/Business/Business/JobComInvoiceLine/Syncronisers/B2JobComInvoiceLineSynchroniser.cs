using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	public class B2JobComInvoiceLineSynchroniser : BusinessObjectSynchroniser
	{
		public B2JobComInvoiceLineSynchroniser(JobComInvoiceLine destination, JobComInvoiceLine source)
			: base(destination, source)
		{ }

		public new JobComInvoiceLine Source
		{
			get { return (JobComInvoiceLine)base.Source; }
		}

		public new JobComInvoiceLine Destination
		{
			get { return (JobComInvoiceLine)base.Destination; }
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			if (Source != null && !Source.IsDeleted && Destination != null && !Destination.IsDeleted)
			{
				Synchronisers.Add(NewFieldSynchroniser(Destination.CA_OriginalLineNoInfo, Source.CA_OriginalLineNoInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.JI_DescriptionInfo, Source.JI_DescriptionInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.CA_CustomsValueInfo, Source.CA_CustomsValueInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.CA_AuthorityNumberInfo, Source.CA_AuthorityNumberInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.JI_TariffInfo, Source.JI_TariffInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.CA_99TariffCodeInfo, Source.CA_99TariffCodeInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.CA_ValueForDutyCodeInfo, Source.CA_ValueForDutyCodeInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.CA_CVforCurrConvInfo, Source.CA_CVforCurrConvInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.JI_CustomsUnitQtyInfo, Source.JI_CustomsUnitQtyInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.JI_CustomsQuantityInfo, Source.JI_CustomsQuantityInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.JI_CustomsSecondUnitQtyInfo, Source.JI_CustomsSecondUnitQtyInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.JI_CustomsSecondQuantityInfo, Source.JI_CustomsSecondQuantityInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.JI_CustomsThirdUnitQtyInfo, Source.JI_CustomsThirdUnitQtyInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.JI_CustomsThirdQuantityInfo, Source.JI_CustomsThirdQuantityInfo));
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
