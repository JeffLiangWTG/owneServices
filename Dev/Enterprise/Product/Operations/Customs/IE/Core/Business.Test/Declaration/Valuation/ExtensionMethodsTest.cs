using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.Testing
{
	public class ExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestGetIncoTermChargeFactoryCacheKey_JobDeclaration()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				AssertEquals("Import", "IMP", declaration.GetIncoTermChargeFactoryCacheKey());
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
				AssertEquals("Export", "EXP", declaration.GetIncoTermChargeFactoryCacheKey());
			});
		}

		public void TestGetIncoTermChargeFactoryCacheKey_JobComInvoiceGroupHeader()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				AssertEquals("Import", "IMP", groupInvoice.GetIncoTermChargeFactoryCacheKey());
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
				AssertEquals("Export", "EXP", groupInvoice.GetIncoTermChargeFactoryCacheKey());
			});
		}

		public void TestGetIncoTermChargeFactoryCacheKey_JobComInvoiceHeader()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				AssertEquals("Import", "IMP", invoice.GetIncoTermChargeFactoryCacheKey());
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
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
