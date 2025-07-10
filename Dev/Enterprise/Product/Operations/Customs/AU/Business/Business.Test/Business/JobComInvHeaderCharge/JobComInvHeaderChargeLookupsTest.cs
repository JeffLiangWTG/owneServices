using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class JobComInvHeaderChargeLookupsTest : TestCaseWithFactory
	{
		public void TestChargeTypeList_Legacy()
		{
			BaseJobComInvHeaderCharge charge = invoice.Charges.AddNew();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			Assert("Import type", charge.Lookups.ChargeTypeList.IndexOfCode(AUChargeCodeList.Codes.BuyingCommission) >= 0);

			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			Assert(charge.Lookups.ChargeTypeList.IndexOfCode(AUChargeCodeList.Codes.BuyingCommission) < 0);
			Assert("Export type", charge.Lookups.ChargeTypeList.IndexOfCode(CustomsChargeTypeList.Codes.Commission) >= 0);
		}

		public void TestChargeTypeList_CMR()
		{
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			BaseJobComInvHeaderCharge charge = invoice.Charges.AddNew();
			Assert("Import type", charge.Lookups.ChargeTypeList.IndexOfCode(AUChargeCodeList.Codes.BuyingCommission) < 0);
			Assert("Import type", charge.Lookups.ChargeTypeList.IndexOfCode(CustomsChargeTypeList.Codes.ExWorks) < 0);
		}

		#region Implementation

		JobDeclaration testDec;
		JobComInvoiceGroupHeader groupHeader;
		JobComInvoiceHeader invoice;

		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<JobDeclaration>();
			groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			invoice = groupHeader.JobComInvoiceHeaders.AddNew();
		}

		#endregion
	}
}
