using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(CommercialInvoiceFilterBusinessObject))]
	class CommercialInvoiceFilterBusinessObjectTest : Customs.Module.Testing.CommercialInvoiceFilterBusinessObjectTest
	{
		public void TestForeignDeclarationNumberFilter()
		{
			var filterBO = new CommercialInvoiceFilterBusinessObject();
			var invoice1 = Factory.New<JobComInvoiceHeader>();
			invoice1.JZ_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			var mercosul1 = invoiceLine1.MercosulForeignDeclarations.AddNew();
			mercosul1.CSI_Description = "00000001";
			mercosul1.CSI_LineNo = 1;

			mercosul1 = invoiceLine1.MercosulForeignDeclarations.AddNew();
			mercosul1.CSI_Description = "99000004";
			mercosul1.CSI_LineNo = 2;

			var invoice2 = Factory.New<JobComInvoiceHeader>();
			invoice2.JZ_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			var mercosul2 = invoiceLine2.MercosulForeignDeclarations.AddNew();
			mercosul2.CSI_Description = "99000002";
			mercosul2.CSI_LineNo = 1;

			var invoice3 = Factory.New<JobComInvoiceHeader>();
			invoice3.JZ_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine3 = invoice3.InvoiceLines.AddNew();
			var mercosul3 = invoiceLine3.MercosulForeignDeclarations.AddNew();
			mercosul3.CSI_Description = "00000003";
			mercosul3.CSI_LineNo = 1;

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<JobComInvoiceHeader>(filterBO, SQLComparisonOperator.Equal, "00000001", new [] { invoice1 }, CommercialInvoiceFilterConstants.ForeignDeclaration);
				ModuleTestHelper.AssertModuleTextFilterResult<JobComInvoiceHeader>(filterBO, SQLComparisonOperator.StartsWith, "99", new [] { invoice1, invoice2 }, CommercialInvoiceFilterConstants.ForeignDeclaration);
				ModuleTestHelper.AssertModuleTextFilterResult<JobComInvoiceHeader>(filterBO, SQLComparisonOperator.Contains, "03", new [] { invoice3 }, CommercialInvoiceFilterConstants.ForeignDeclaration);
			});
		}

		public void TestImportLicenseFilter()
		{
			var filterBO = new CommercialInvoiceFilterBusinessObject();
			var invoice1 = Factory.New<JobComInvoiceHeader>();
			invoice1.JZ_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.ImportLicenseNumber = "00000001";

			var invoiceLine2 = invoice1.InvoiceLines.AddNew();
			invoiceLine2.ImportLicenseNumber = "99000004";

			var invoice2 = Factory.New<JobComInvoiceHeader>();
			invoice2.JZ_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine3 = invoice2.InvoiceLines.AddNew();
			invoiceLine3.ImportLicenseNumber = "99000002";

			var invoice3 = Factory.New<JobComInvoiceHeader>();
			invoice3.JZ_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine4 = invoice3.InvoiceLines.AddNew();
			invoiceLine4.ImportLicenseNumber = "00000003";

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<JobComInvoiceHeader>(filterBO, SQLComparisonOperator.Equal, "00000001", new [] { invoice1 }, CommercialInvoiceFilterConstants.ImportLicense);
				ModuleTestHelper.AssertModuleTextFilterResult<JobComInvoiceHeader>(filterBO, SQLComparisonOperator.StartsWith, "99", new [] { invoice1, invoice2 }, CommercialInvoiceFilterConstants.ImportLicense);
				ModuleTestHelper.AssertModuleTextFilterResult<JobComInvoiceHeader>(filterBO, SQLComparisonOperator.Contains, "03", new [] { invoice3 }, CommercialInvoiceFilterConstants.ImportLicense);
			});
		}

		public void TestLPCOFilter()
		{
			var filterBO = new CommercialInvoiceFilterBusinessObject();
			var invoice1 = Factory.New<JobComInvoiceHeader>();
			invoice1.JZ_MessageType = BRJobMessageTypeList.Codes.Export;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			var lpco1 = invoiceLine1.LPCOJobComInvLineRefsCollection.AddNew();
			lpco1.JG_ReferenceNumber = "00000001";

			lpco1 = invoiceLine1.LPCOJobComInvLineRefsCollection.AddNew();
			lpco1.JG_ReferenceNumber = "99000004";

			var invoice2 = Factory.New<JobComInvoiceHeader>();
			invoice2.JZ_MessageType = BRJobMessageTypeList.Codes.Export;
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			var lpco2 = invoiceLine2.LPCOJobComInvLineRefsCollection.AddNew();
			lpco2.JG_ReferenceNumber = "99000002";

			var invoice3 = Factory.New<JobComInvoiceHeader>();
			invoice3.JZ_MessageType = BRJobMessageTypeList.Codes.Export;
			var invoiceLine3 = invoice3.InvoiceLines.AddNew();
			var lpco3 = invoiceLine3.LPCOJobComInvLineRefsCollection.AddNew();
			lpco3.JG_ReferenceNumber = "00000003";

			Factory.Save();

			CombineAssertions(() =>
			{
				ModuleTestHelper.AssertModuleTextFilterResult<JobComInvoiceHeader>(filterBO, SQLComparisonOperator.Equal, "00000001", new [] { invoice1 }, CommercialInvoiceFilterConstants.Lpco);
				ModuleTestHelper.AssertModuleTextFilterResult<JobComInvoiceHeader>(filterBO, SQLComparisonOperator.StartsWith, "99", new [] { invoice1, invoice2 }, CommercialInvoiceFilterConstants.Lpco);
				ModuleTestHelper.AssertModuleTextFilterResult<JobComInvoiceHeader>(filterBO, SQLComparisonOperator.Contains, "03", new [] { invoice3 }, CommercialInvoiceFilterConstants.Lpco);
				ModuleTestHelper.AssertModuleTextFilterResult<JobComInvoiceHeader>(filterBO, SQLComparisonOperator.Contains, "99,03", new [] { invoice1, invoice2, invoice3 }, CommercialInvoiceFilterConstants.Lpco);
			});
		}

		void AssertFilterName(CommercialInvoiceFilterBusinessObject filterBO, string filterConstants)
		{
			var filter = (ModuleTextFilter)filterBO[filterConstants];
			Assert("No filter find with the name " + filterConstants, filter != null);
		}

		public void TestValidateFiltersNames()
		{
			var filterBO = new CommercialInvoiceFilterBusinessObject();
			string[] filters = { "Invoice Line LPCO", "Invoice Line Import License", "Invoice Line Foreign Dec. DE Number" };
			foreach (var filterName in filters)
			{
				AssertFilterName(filterBO, filterName);
			}
		}
	}
}
