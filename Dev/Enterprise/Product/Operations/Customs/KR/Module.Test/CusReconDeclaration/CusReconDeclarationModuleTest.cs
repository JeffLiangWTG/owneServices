using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(CusReconDeclarationModule))]
	sealed class CusReconDeclarationModuleTest : ZModuleBasherWithFetchHintsTest
	{
		protected override ZFilterModule CreateModuleForFetchHintsTest() => new CusReconDeclarationModule();
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.KR.CusReconDeclaration;
		protected override void SetupDataForFetchHintsTest()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSOF", "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType("CUSDP", "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, "CUSOF", "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, "CUSDP", "20", "내륙기지통관과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			for (int idx = 0; idx < 20; idx++)
			{
				CreateCusReconDeclarationForFetchHintTest(Factory, idx);
			}
			Factory.Save();
		}
		void CreateCusReconDeclarationForFetchHintTest(BusinessObjectFactory factory, int idx)
		{
			var orgHeader = factory.New<OrgHeader>();
			orgHeader.OH_Category = "BUS";
			orgHeader.OH_Code = "RK" + idx.ToString();
			orgHeader.OH_FullName = "RK Test" + idx.ToString();

			var declaration = factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;

			var reconDeclaration = factory.New<CusReconDeclaration>();
			reconDeclaration.CRD_MessageStatus = CustomsMessageStatusTypeList.Codes.OriginalSent;
			reconDeclaration.CRD_CustomsStatus = CustomsEntryStatusTypeList.Codes.NDC;
			reconDeclaration.CRD_ApplicationCode = "KRC";
			reconDeclaration.CRD_CustomsOffice = "010";
			reconDeclaration.CRD_JobReferenceNumber = "MSC000000" + idx.ToString("00");
			reconDeclaration.CRD_OA_DeclarantAddress = orgHeader.MainAddress.PK;

			var reconEntryLine = reconDeclaration.CusReconEntryLines.AddNew();
			reconEntryLine.CRL_LineNumber = 1;
			reconEntryLine.CRL_CustomsStatus = "AA";
			reconEntryLine.CRL_Description = "AA";
			reconEntryLine.CRL_OriginalEntryLineNumber = 1;

			var reconEntry = reconDeclaration.CusReconEntries[0];
			reconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			reconEntry.CRE_EntryDate = ZDate.Today;
			reconEntry.CRE_EntryType = "AA";
			reconEntry.CRE_OA_DeclarantAddress = orgHeader.MainAddress.PK;

			var cusReconCustomsCharge = reconEntryLine.CusReconCharges.AddNew();
			cusReconCustomsCharge.CRC_Amount = 1 + idx;
			cusReconCustomsCharge.CRC_ChargeType = "AA";

			var message5UL = reconDeclaration.Messages.AddNew();
			message5UL.EM_MessageType = ElectronicDocumentTypeList.Codes._5UL;

			var entryNum5UL = factory.New<CusEntryNumber>();
			entryNum5UL.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNum5UL.CE_EntryNum = "123452212345" + idx.ToString("00");
			entryNum5UL.CE_IssueDate = ZDateTime.Today;
			entryNum5UL.CE_ParentID = reconDeclaration.PK;
			entryNum5UL.CE_RN_NKCountryCode = "KR";
			entryNum5UL.CE_ParentTable = CusReconDeclaration.Schema.TableName;
			entryNum5UL.CE_EntryIsSystemGenerated = true;

			var entryNum5UO = factory.New<CusEntryNumber>();
			entryNum5UO.CE_EntryType = ElectronicDocumentTypeList.Codes._5UO;
			entryNum5UO.CE_EntryNum = "123452312345" + idx.ToString("00");
			entryNum5UO.CE_IssueDate = ZDateTime.Today;
			entryNum5UO.CE_ParentID = reconDeclaration.PK;
			entryNum5UO.CE_RN_NKCountryCode = "KR";
			entryNum5UO.CE_ParentTable = CusReconDeclaration.Schema.TableName;
			entryNum5UO.CE_EntryIsSystemGenerated = true;
		}

		protected override bool ShouldTestFormIsFullyTranslatable => false;

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			var factory = collection.Factory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSOF", "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType("CUSDP", "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, "CUSOF", "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, "CUSDP", "20", "내륙기지통관과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			CreateCusReconDeclarationForFetchHintTest(factory, 0);
			factory.Save();
		}
	}
}
