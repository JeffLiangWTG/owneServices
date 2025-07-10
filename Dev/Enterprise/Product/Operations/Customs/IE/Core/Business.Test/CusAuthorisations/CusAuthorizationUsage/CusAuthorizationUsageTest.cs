using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(CusAuthorizationUsage))]
	class CusAuthorizationUsageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestICusAuthorizationUsageIsCorrectlySetup()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			authorizationUsage.FillWithValidTestData();
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			AssertType<CusAuthorizationUsage>(newFactory.Load<Integration.Customs.IE.ICusAuthorizationUsage>(authorizationUsage.PK));
			newFactory = new BusinessObjectFactory();
			AssertType<CusAuthorizationUsage>(newFactory.Load<EU.Business.CusAuthorizationUsage>(authorizationUsage.PK));
		}

		public void TestValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var authorizationUsage1 = instruction.CusAuthorizationUsages.AddNew();
			AssertType<ExportCusAuthorizationUsageValidation>(authorizationUsage1.Validation);
			AssertType<ExportEntryInstructionCusAuthorizationUsageValidationStrategy>(((ExportCusAuthorizationUsageValidation)authorizationUsage1.Validation).ValidationStrategy);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			AssertType<ImportCusAuthorizationUsageValidation>(authorizationUsage1.Validation);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var authorizationUsage2 = invoiceLine.CusAuthorizationUsages.AddNew();
			AssertType<CusAuthorizationUsageValidation>(authorizationUsage2.Validation);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			AssertType<ImportCusAuthorizationUsageValidation>(authorizationUsage2.Validation);
		}

		public void TestRefreshWarehouseDataIfNeeded()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var authorizationUsage = instruction.CusAuthorizationUsages.AddNew();
			var isTriggered = false;
			instruction.FromWarehouseTypeInfo.ValueChanged += (sender, e) =>
			{
				isTriggered = true;
			};
			authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			AssertEquals("Triggered when AGC_Code changed", true, isTriggered);

			isTriggered = false;
			authorizationUsage.AGC_OH_Owner = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertEquals("Triggered when AGC_OH_Owner changed", true, isTriggered);

			isTriggered = false;
			using (authorizationUsage.SuspendWarehouseDataRefresh())
			{
				authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2;
				AssertEquals("Not triggered when suspend", false, isTriggered);
			}
		}

		public void TestAuthorizationDefaultsSupportingDocument()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType("EUNAU", "OUT", "Authorisation Codes to Document Type Code", true);
			helper.CreateCusMap("EUNAU", CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "C517", startDate, endDate, "EUN");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			var entryInstruction = declaration.CustomsEntryInstructions.FirstOrAddNew();

			var invoiceHeader = declaration.Invoices.FirstOrAddNew<JobComInvoiceHeader>();
			var invoiceLine = invoiceHeader.InvoiceLines.FirstOrAddNew<JobComInvoiceLine>();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var auth = invoiceLine.CusAuthorizationUsages.AddNew();

			auth.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
			AssertEquals("Supporting Document Count with AGC_Code added", 0, invoiceLine.SupportingDocuments.Count);

			auth.AGC_Number = "1234";
			AssertEquals("Supporting Document Count with AGC_Code & AGC_Number added", 1, invoiceLine.SupportingDocuments.Count);

			var supportingDoc = invoiceLine.SupportingDocuments[0];
			CombineAssertions(() =>
			{
				AssertEquals("CSI_Code", "C517", supportingDoc.CSI_Code);
				AssertEquals("CSI_ReferenceNumber", "1234", supportingDoc.CSI_ReferenceNumber);
			});
		}

		public void TestAuthorizationDefaultsSupportingDocument_NotAdded()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType("EUNAU", "OUT", "Authorisation Codes to Document Type Code", true);
			helper.CreateCusMap("EUNAU", CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "C517", startDate, endDate, "EUN");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;

			CombineAssertions(() =>
			{
				AssertNoSupportingDocs("H1", "44");
				AssertNoSupportingDocs("H3", "53");
				AssertNoSupportingDocs("H4", "51");
			});

			void AssertNoSupportingDocs(string declarationType, string procedureCode)
			{
				var entryInstruction = declaration.CustomsEntryInstructions.FirstOrAddNew();
				entryInstruction.CEI_Style = declarationType;

				var invoiceHeader = declaration.Invoices.FirstOrAddNew<JobComInvoiceHeader>();
				var invoiceLine = invoiceHeader.InvoiceLines.FirstOrAddNew<JobComInvoiceLine>();
				invoiceLine.JI_CEI = entryInstruction.PK;
				invoiceLine.JI_Procedure = procedureCode.PadRight(6, '0');

				var additionalDocument = entryInstruction.AdditionalInfos.AddNew();
				additionalDocument.CSI_SubType = "INF";
				additionalDocument.CSI_Code = "00100";
				additionalDocument.CSI_Description = "Test";

				var auth = entryInstruction.CusAuthorizationUsages.AddNew();

				auth.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
				auth.AGC_Number = "1234";

				AssertEquals($"Supporting Document Count should be 0 for Declaration Type {declarationType} and Procedure Code {procedureCode} with 00100 document", 0, entryInstruction.SupportingDocuments.Count);
			}
		}
	}
}
