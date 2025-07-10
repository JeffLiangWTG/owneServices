using Enterprise.Customs.Forwarding.GUI;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.Forwarding.Module.Testing
{
	[CountrySpecificTest(Core.Constants.CountryCodes.NewZealand)]
	class ForwardingConsolModuleCustomsColumnsProviderNZTest : ForwardingConsolModuleCustomsColumnsProviderAbstractTest
	{
		public void TestBashFetchForView_CustomsCargoStatus()
		{
			BashFetchForView("CustomsCargoStatus", 0);
		}

		public void TestBashFetchForView_AMSBillStatus()
		{
			BashFetchForView("AMSBillStatus", 0);
		}

		public void TestBashFetchForView_AMSBillStatusDescription()
		{
			BashFetchForView("AMSBillStatusDescription", 0);
		}

		public void TestBashFetchForView_AFRBillStatus()
		{
			BashFetchForView("AFRBillStatus", 0);
		}

		public void TestBashFetchForView_AFRBillStatusDescription()
		{
			BashFetchForView("AFRBillStatusDescription", 0);
		}

		public void TestBashFetchForView_OutwardReportStatus()
		{
			// CusEntryNum: 12

			BashFetchForView("OutwardReportStatus", 12);
		}

		public void TestBashFetchForView_OutwardReportStatusDescription()
		{
			// CusEntryNum: 1

			BashFetchForView("OutwardReportStatusDescription", 1);
		}

		public void TestBashFetchForView_OutwardReportEntryNumber()
		{
			// CusEntryNum: 12

			BashFetchForView("OutwardReportEntryNumber", 12);
		}

		public void TestBashFetchForView_LatestAMSDispositionCode()
		{
			BashFetchForView(ForwardingConsolCustomsColumnConstants.Schema.LatestAMSDispositionCode, 0);
		}

		public void TestBashFetchForView_LatestAMSDispositionDesc()
		{
			BashFetchForView(ForwardingConsolCustomsColumnConstants.Schema.LatestAMSDispositionDesc, 0);
		}

		public void TestBashFetchForView_AsycudaRegistrationStatus()
		{
			BashFetchForView(ForwardingConsolCustomsColumnConstants.Schema.AsycudaRegistrationStatus, 0);
		}
	}
}
