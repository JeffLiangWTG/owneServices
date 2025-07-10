using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class ExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestGetIncoTermChargeFactoryCacheKey_JobDeclaration()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("Import", "", declaration.GetIncoTermChargeFactoryCacheKey());
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertEquals("Export", "EXP", declaration.GetIncoTermChargeFactoryCacheKey());
			});
		}

		public void TestGetIncoTermChargeFactoryCacheKey_JobComInvoiceGroupHeader()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("Import", "", groupInvoice.GetIncoTermChargeFactoryCacheKey());
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertEquals("Export", "EXP", groupInvoice.GetIncoTermChargeFactoryCacheKey());
			});
		}

		public void TestGetIncoTermChargeFactoryCacheKey_JobComInvoiceHeader()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("Import", "", invoice.GetIncoTermChargeFactoryCacheKey());
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertEquals("Export", "EXP", invoice.GetIncoTermChargeFactoryCacheKey());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			groupInvoice = declaration.JobComInvoiceGroupHeaders[0];
			invoice = declaration.Invoices.AddNew();
		}

		JobDeclaration declaration;
		JobComInvoiceGroupHeader groupInvoice;
		JobComInvoiceHeader invoice;
	}
}
