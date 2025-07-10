
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public class CrossTradeDebtorConfigurationLookups : JobConfigurationSelectorLookups
	{
		public CrossTradeDebtorConfigurationLookups(CrossTradeDebtorConfiguration parent)
			: base(parent)
		{
		}

		#region JobTypeList

		protected override CodeDescriptionPairList GetJobTypeList()
		{
			if (jobTypeList == null)
			{
				jobTypeList = new CodeDescriptionPairList();
				jobTypeList.AddPair(JobTypeAdditionalCodes.All, ResString.GetMultilingualString("CrossTradeJobTypeList.ALL", "Shipment, Quick Booking"));
				jobTypeList.Add(JobInvoicingConsumerTypes.Shipment);
				jobTypeList.Add(JobInvoicingConsumerTypes.QuotedBooking);
			}
			return jobTypeList;
		}

		CodeDescriptionPairList jobTypeList;

		#endregion

		#region ChargePaymentTypeList

		public CodeDescriptionPairList ChargePaymentTypeList
		{
			get
			{
				if (chargePaymentTypeList == null)
				{
					chargePaymentTypeList = new CodeDescriptionPairList();
					chargePaymentTypeList.AddPair(PrepaidCollectFreightForwardingList.Codes.All, ResString.GetMultilingualString("ChargePaymentTypeList.ALL", "Both Prepaid and Collect"));
					chargePaymentTypeList.AddPair(PrepaidCollectFreightForwardingList.Codes.PPD, ResString.GetMultilingualString("ChargePaymentTypeList.PPD", "Prepaid"));
					chargePaymentTypeList.AddPair(PrepaidCollectFreightForwardingList.Codes.CCX, ResString.GetMultilingualString("ChargePaymentTypeList.CCX", "Collect"));
					return chargePaymentTypeList;
				}
				return chargePaymentTypeList;
			}
		}
		
		CodeDescriptionPairList chargePaymentTypeList;

		#endregion

		#region DebtorOptionList

		public CodeDescriptionPairList DebtorOptionList
		{
			get
			{
				if (debtorOptionList == null)
				{
					debtorOptionList = new DefaultDebtorList();
					return debtorOptionList;
				}
				return debtorOptionList;
			}
		}
		
		CodeDescriptionPairList debtorOptionList;

		#endregion
	}
}
