using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class ExportCusEntryLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCL_StatisticalValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B1;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();

			var message = "Statistical Value must be greater than 0.";
			entryLine.Validation.ValidateCL_StatisticalValue();
			var targetInfo = entryLine.CL_StatisticalValueInfo;
			AssertHasMessageErrorContaining("EXP/B1, Statistical Value required.", targetInfo, message);

			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B2;
			entryLine.Validation.ValidateCL_StatisticalValue();
			AssertHasMessageErrorContaining("EXP/B2, Statistical Value required.", targetInfo, message);

			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B3;
			entryLine.Validation.ValidateCL_StatisticalValue();
			AssertNoMessageErrorContaining("EXP/B3, Statistical Value not required.", targetInfo, message);
		}
	}
}
