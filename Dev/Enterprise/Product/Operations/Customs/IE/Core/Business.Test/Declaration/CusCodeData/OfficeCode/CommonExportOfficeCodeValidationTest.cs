using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.Testing
{
	class CommonExportOfficeCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Data_ShouldNotAmendThisValue_OfficeOfExit()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions[0];
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.MovementReferenceNumberSetter("MRN0000001");
			entry.CH_EntryStatus = AESEntryStatusList.Codes.MrnAllocated;
			var officeOfExit = declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "IEATH200");
			instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
			Factory.Save();

			declaration = NewFactory().Load<JobDeclaration>(declaration.PK);
			officeOfExit = declaration.CustomsOffices.GetOfficeOfExit();
			CombineAssertions(() =>
			{
				officeOfExit.CY_Data = "IE000013";
				AssertHasMessageError("NO NoAmend check for EXT offices on EXP jobs.", officeOfExit.CY_DataInfo, CommonResStrings.ShouldNotAmendThisValue);
				officeOfExit.CY_Data = "IEATH200";
				AssertNoMessageError("NO NoAmend check for EXT offices on EXP jobs(validation passes).", officeOfExit.CY_DataInfo, CommonResStrings.ShouldNotAmendThisValue);

				officeOfExit.Parent.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
				officeOfExit.CY_Data = "IE000013";
				AssertHasMessageError("NO NoAmend check for EXT offices on REX jobs.", officeOfExit.CY_DataInfo, CommonResStrings.ShouldNotAmendThisValue);
			});
		}
	}
}
