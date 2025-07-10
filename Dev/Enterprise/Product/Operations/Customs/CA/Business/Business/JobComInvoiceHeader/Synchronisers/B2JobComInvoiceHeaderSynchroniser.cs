using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	public class B2JobComInvoiceHeaderSynchroniser : BusinessObjectSynchroniser
	{
		public B2JobComInvoiceHeaderSynchroniser(JobComInvoiceHeader destination, JobComInvoiceHeader source)
			: base(destination, source)
		{ }

		public new JobComInvoiceHeader Source
		{
			get { return (JobComInvoiceHeader)base.Source; }
		}

		public new JobComInvoiceHeader Destination
		{
			get { return (JobComInvoiceHeader)base.Destination; }
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			if (Source != null && !Source.IsDeleted && Destination != null && !Destination.IsDeleted)
			{
				Synchronisers.Add(NewFieldSynchroniser(Destination.JZ_InvoiceNumberInfo, Source.JZ_InvoiceNumberInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.JZ_RN_NKDefaultOriginInfo, Source.JZ_RN_NKDefaultOriginInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.JZ_RW_NKOriginStateInfo, Source.JZ_RW_NKOriginStateInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.CA_RN_NKExportInfo, Source.CA_RN_NKExportInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.CA_USStateOfExportInfo, Source.CA_USStateOfExportInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.CA_TreatmentCodeInfo, Source.CA_TreatmentCodeInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.JZ_ValuationDateOverrideInfo, Source.JZ_ValuationDateOverrideInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.JZ_RX_NKInvoice_CurrencyInfo, Source.JZ_RX_NKInvoice_CurrencyInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.CA_TimeLimitInfo, Source.CA_TimeLimitInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.CA_TimeLimitCodeInfo, Source.CA_TimeLimitCodeInfo));
				Synchronisers.Add(NewFieldSynchroniser(Destination.CA_TradeZoneInfo, Source.CA_TradeZoneInfo));
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
