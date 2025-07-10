using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CusEntryHeaderDocumentSupporter))]
	class CusEntryHeaderDocumentSupporterTest : Customs.Business.Testing.CusEntryHeaderDocumentSupportTest
	{
		public void TestGetBODocDataProviders()
		{
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.ActiveEntryHeaders.AddNew();
			var providers = header.DocumentSupporter.GetBODocDataProviders(new DataContextValue(CusEntryHeaderDocumentSupporter.CustomsDeclarationDocument), null);
			AssertEquals("Provider for CustomsDeclarationDocument", 1, providers.Length);
		}

		public void TestGetDataStateBeforeRun()
		{
			var entryHeader = (CusEntryHeader)GetDocumentSupportableBusinessObject();
			var documentSupporter = entryHeader.DocumentSupporter;
			TestGetDataStateForReleaseNote(documentSupporter);
			TestGetDataStateOther(documentSupporter);
		}

		public override void TestGetDocBusinessObjects()
		{
			var entryHeader = (CusEntryHeader)GetDocumentSupportableBusinessObject();
			DocumentWrapper[] result = entryHeader.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CusEntryHeader, null);
			AssertEquals("Document wrapper for data context of CusEntryHeader is of type CusDataHeaderDocumentWrapper", "Enterprise.Customs.CN.Business.CusDataHeaderDocumentWrapper", result[0].GetType().ToString());
		}

		protected override ZString TestCountryCode => Core.Constants.CountryCodes.China;

		void TestGetDataStateForReleaseNote(DocumentSupporter supporter)
		{
			var instruction = ((CusEntryHeader)supporter.BusinessObject).EntryInstruction;
			var testMenuItem = Factory.New<StmMenuItem>();
			testMenuItem.SU_MenuName = "Customs Release Notice";
			instruction.CEI_DocumentSubmissionType = "L";
			var dataState = supporter.GetDataStateBeforeRun(testMenuItem);
			Assert("Should be invalid", !dataState.IsValid);
			AssertEquals("Should have a correct error message", "Cannot print Customs Release Notice as the entry is not in paperless clearance mode.", dataState.ErrorMessage);
			instruction.CEI_DocumentSubmissionType = "M";
			dataState = supporter.GetDataStateBeforeRun(testMenuItem);
			Assert("Should be invalid", !dataState.IsValid);
			AssertEquals("Should have a correct error message", "Cannot print Customs Release Notice as the entry has not been released.", dataState.ErrorMessage);
			((CusEntryHeader)supporter.BusinessObject).CH_EntryReleaseDate = ZDateTime.Today;
			dataState = supporter.GetDataStateBeforeRun(testMenuItem);
			Assert("Should be valid and empty message.", dataState.IsValid && string.IsNullOrWhiteSpace(dataState.ErrorMessage));
		}

		void TestGetDataStateOther(DocumentSupporter supporter)
		{
			var testMenuItem = Factory.New<StmMenuItem>();
			testMenuItem.SU_MenuName = "A random menu item";
			var dataState = supporter.GetDataStateBeforeRun(testMenuItem);
			Assert("Should be valid and empty message.", dataState.IsValid && string.IsNullOrWhiteSpace(dataState.ErrorMessage));
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			using (GlbCompany.GetCurrentCompany(Factory).TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				declaration.JE_MessageSubType = DecTypeList.Codes.Both;
				var entryInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				var orgHeader0 = Factory.New<OrgHeader>();
				orgHeader0.OH_Code = "TESTORG1";
				var invoice0 = declaration.Invoices.AddNew();
				invoice0.InvoiceLines.AddNew();
				invoice0.JZ_OH_Supplier = orgHeader0.PK;
				var orgHeader1 = Factory.New<OrgHeader>();
				orgHeader1.OH_Code = "TESTORG2";
				var invoice1 = declaration.Invoices.AddNew();
				invoice1.InvoiceLines.AddNew();
				invoice1.JZ_OH_Supplier = orgHeader1.PK;
				var line0 = invoice0.InvoiceLines.AddNew();
				line0.JI_CEI = entryInst.PK;
				new LineMerger(declaration).DoMerge();
				declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
				return declaration.ActiveEntryHeaders[0];
			}
		}
	}
}
