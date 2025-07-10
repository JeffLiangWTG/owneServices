using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(OfficeCode))]
	class OfficeCodeTest : CusCodeDataTest<OfficeCode>
	{
		public void TestNew()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CY_Code", EuOfficeCodesTypes.Codes.OfficeOfExit, office.CY_Code);
				AssertEquals("Parent", declaration, office.Parent);
			});
		}

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest) => new OfficeCodeLightValidationTester(bizObjToTest);

		class OfficeCodeLightValidationTester : LightValidationTester
		{
			public OfficeCodeLightValidationTester(BusinessObject bo) : base(bo) { }

			protected override bool ShouldTestProperty(ZPropertyInfo info) => base.ShouldTestProperty(info) && info.Name != JobDeclarationSchema.Constants.JE_EntryStatus;
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(CusCodeDataTypeList.Codes.OfficeCode, office.CY_Type);
		}

		public void TestLookups()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var office = declaration.CustomsOffices.AddNew();

			AssertType<OfficeCodeLookups>("OfficeCodeLookups for EXP jobs.", office.Lookups);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			office = declaration.CustomsOffices.AddNew();
			AssertType<ImportUCC5OfficeCodeLookups>("OfficeCodeLookups for IMP UCC5 jobs.", office.Lookups);

			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
			office = declaration.CustomsOffices.AddNew();
			AssertType<ImportOfficeCodeLookups>("OfficeCodeLookups for IMP jobs.", office.Lookups);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			office = declaration.CustomsOffices.AddNew();
			AssertType<ExitSummaryOfficeCodeLookups>("OfficeCodeLookups for EXS jobs.", office.Lookups);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
			office = declaration.CustomsOffices.AddNew();
			AssertType<OfficeCodeLookups>("OfficeCodeLookups for REX jobs.", office.Lookups);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.MiscellaneousCustoms;
			office = declaration.CustomsOffices.AddNew();
			AssertType<OfficeCodeLookups>("OfficeCodeLookups for MSC jobs.", office.Lookups);
		}

		public void TestValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var office = declaration.CustomsOffices.AddNew();

			AssertType<ExportOfficeCodeValidation>("ExportOfficeCodeValidation for EXP jobs.", office.Validation);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			office = declaration.CustomsOffices.AddNew();
			AssertType<OfficeCodeValidation>("OfficeCodeValidation for IMP jobs.", office.Validation);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			office = declaration.CustomsOffices.AddNew();
			AssertType<ExitSummaryOfficeCodeValidation>("ExitSummaryOfficeCodeValidation for Exit Summary jobs.", office.Validation);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
			office = declaration.CustomsOffices.AddNew();
			AssertType<ReExportOfficeCodeValidation>("ReExportOfficeCodeValidation for ReExport jobs.", office.Validation);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Customs Office EXT(Office of Exit)", office.HumanReadableName);

			var declarationIMP = Factory.New<JobDeclaration>();
			declarationIMP.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			var officeIMP = declarationIMP.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.SupervisingOffice);
			AssertEquals("Customs Office SVO(Supervising Office)", officeIMP.HumanReadableName);
		}

		public void TestGetWarningBeforeBeingDeleted_OfficeOfPresentation()
		{
			var instruction = declaration.CustomsEntryInstructions[0];
			(declaration.CustomsOffices.GetPresentationOffice() ?? declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfPresentation)).CY_Data = "IEATH200";
			instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.MovementReferenceNumberSetter("MRN0000001");
			entry.CH_EntryStatus = AESEntryStatusList.Codes.MrnAllocated;
			Factory.Save();

			declaration = NewFactory().Load<JobDeclaration>(declaration.PK);
			var officeOfPresentation = declaration.CustomsOffices.GetOffice(EuOfficeCodesTypes.Codes.OfficeOfPresentation);
			officeOfPresentation.CY_Data = "A";

			AssertContains("Office Of Presentation, should have a warning.", CommonResStrings.ShouldNotDeleteDeclaredDataString, officeOfPresentation.GetWarningBeforeBeingDeleted());
		}

		public void TestGetWarningBeforeBeingDeleted_OfficeOfExit()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			(declaration.CustomsOffices.GetOfficeOfExit() ?? declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit)).CY_Data = "IEATH200";
			instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.MovementReferenceNumberSetter("MRN0000001");
			entry.CH_EntryStatus = AESEntryStatusList.Codes.MrnAllocated;
			Factory.Save();

			declaration = NewFactory().Load<JobDeclaration>(declaration.PK);
			var officeOfExit = declaration.CustomsOffices.GetOffice(EuOfficeCodesTypes.Codes.OfficeOfExit);
			officeOfExit.CY_Data = "A";
			AssertContains("Office Of Exit, should have a warning.", CommonResStrings.ShouldNotDeleteDeclaredDataString, officeOfExit.GetWarningBeforeBeingDeleted());
		}

		public void TestGetWarningBeforeBeingDeleted_SupervisingOffice()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			(declaration.CustomsOffices.GetSupervisingOffice() ?? declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.SupervisingOffice)).CY_Data = "IEORK200";
			instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.MovementReferenceNumberSetter("MRN0000001");
			entry.CH_EntryStatus = AESEntryStatusList.Codes.MrnAllocated;
			Factory.Save();

			declaration = NewFactory().Load<JobDeclaration>(declaration.PK);
			var supervisingOffice = declaration.CustomsOffices.GetOffice(EuOfficeCodesTypes.Codes.SupervisingOffice);
			supervisingOffice.CY_Data = "A";
			AssertContains("Office Of Exit, should have a warning.", CommonResStrings.ShouldNotDeleteDeclaredDataString, supervisingOffice.GetWarningBeforeBeingDeleted());
		}

		public void TestPopulateAuthorisations()
		{
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			office.CY_Code = EuOfficeCodesTypes.Codes.SupervisingOffice;

			var entry1 = declaration.CustomsEntryInstructions.AddNew();
			entry1.CEI_Style = ExportDeclarationTypeList.Codes.B1;
			var authorization1 = entry1.CusAuthorizationUsages.AddNew();
			authorization1.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CentralizedClearance;

			var entry2 = declaration.CustomsEntryInstructions.AddNew();
			entry2.CEI_Style = ExportDeclarationTypeList.Codes.B1;

			var entry3 = declaration.CustomsEntryInstructions.AddNew();
			entry3.CEI_Style = ExportDeclarationTypeList.Codes.B2;
			office.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;

			var entry4 = declaration.CustomsEntryInstructions.AddNew();
			entry3.CEI_Style = ExportDeclarationTypeList.Codes.B2;
			((IBusinessObjectInternals)office).IsCopying = true;
			office.CY_Code = EuOfficeCodesTypes.Codes.SupervisingOffice;
			office.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;

			CombineAssertions(() =>
			{
				AssertEquals("CusAuthorizationUsages be repeated multiple times", 1, entry1.CusAuthorizationUsages.Count);
				AssertEquals("Should populate CusAuthorizationUsages", 1, entry2.CusAuthorizationUsages.Count);
				AssertEquals("Should populate CentralizedClearance CusAuthorizationUsage", CusAuthorizationHeaderTypeList.Codes.CentralizedClearance, entry2.CusAuthorizationUsages[0].AGC_Code);
				AssertEquals("Should populate CusAuthorizationUsages", 1, entry3.CusAuthorizationUsages.Count);
				AssertEquals("Should not populate CusAuthorizationUsages when copy happens (e.g. importing from USXML)", 0, entry4.CusAuthorizationUsages.Count);
			});
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport);

		protected override BusinessObject GetNewBusinessObject() => office;

		protected override IEnumerable<OfficeCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory) => new[] { factory.New<JobDeclaration>().CustomsOffices.AddNew() };

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			office = declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit);
		}
		JobDeclaration declaration;
		OfficeCode office;
	}
}
