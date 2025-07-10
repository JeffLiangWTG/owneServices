using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.Testing
{
	class ExportOfficeCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Data_ShouldNotAmendThisValue_OfficeOfPresentation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions[0];
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.MovementReferenceNumberSetter("MRN0000001");
			entry.CH_EntryStatus = AESEntryStatusList.Codes.MrnAllocated;
			var officeOfPresentation = declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfPresentation, "AT100000");
			instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
			Factory.Save();

			declaration = NewFactory().Load<JobDeclaration>(declaration.PK);
			officeOfPresentation = declaration.CustomsOffices.GetPresentationOffice();
			CombineAssertions(() =>
			{
				officeOfPresentation.CY_Data = "IE000012";
				AssertHasMessageError("NoAmend check for PRE offices on EXP jobs.", officeOfPresentation.CY_DataInfo, CommonResStrings.ShouldNotAmendThisValue);
				officeOfPresentation.CY_Data = "AT100000";
				AssertNoMessageError("NoAmend check for PRE offices on EXP jobs(validation passes).", officeOfPresentation.CY_DataInfo, CommonResStrings.ShouldNotAmendThisValue);
			});
		}

		public void TestCheckCY_Data_ShouldNotAmendThisValue_SupervisingOffice()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions[0];
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.MovementReferenceNumberSetter("MRN0000001");
			entry.CH_EntryStatus = AESEntryStatusList.Codes.MrnAllocated;
			var supervisingOffice = declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.SupervisingOffice, "AT100000");
			instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
			Factory.Save();

			declaration = NewFactory().Load<JobDeclaration>(declaration.PK);
			supervisingOffice = declaration.CustomsOffices.GetSupervisingOffice();
			CombineAssertions(() =>
			{
				supervisingOffice.CY_Data = "IE000012";
				AssertHasMessageError("NoAmend check for PRE offices on EXP jobs.", supervisingOffice.CY_DataInfo, CommonResStrings.ShouldNotAmendThisValue);
				supervisingOffice.CY_Data = "AT100000";
				AssertNoMessageError("NoAmend check for PRE offices on EXP jobs(validation passes).", supervisingOffice.CY_DataInfo, CommonResStrings.ShouldNotAmendThisValue);
			});
		}

		public void TestCheckCY_Data_SupervisingOffice()
		{
			var errorMessage = "Supervising Customs Office cannot be the same as Office of Export";

			var declaration = Factory.New<JobDeclaration>();
			var supervisingOffice = declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.SupervisingOffice);

			CombineAssertions(() =>
			{
				AssertNoMessageError(supervisingOffice.CY_DataInfo, errorMessage);
				declaration.JE_CustomsOffice = "IEDUB100";
				supervisingOffice.CY_Data = "IEDUB100";
				AssertHasMessageError(supervisingOffice.CY_DataInfo, errorMessage);
				supervisingOffice.CY_Data = "IEARK100";
				AssertNoMessageError(supervisingOffice.CY_DataInfo, errorMessage);
			});
		}
	}
}
