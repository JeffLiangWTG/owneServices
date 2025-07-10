using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	class ESGuaranteeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckEntryInstructionID()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "A";

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = "B";

			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			CombineAssertions(() =>
			{
				var guarantee = declaration.Guarantees.AddNew();
				guarantee.EntryInstructionID = ZGuid.Invalid;
				AssertHasError(guarantee.EntryInstructionIDInfo, "Enter a valid selection.");

				var guarantee2 = declaration.Guarantees.AddNew();
				guarantee2.EntryInstructionID = entryInstruction1.PK;
				AssertNoError(guarantee2.EntryInstructionIDInfo, "Enter a valid selection.");

				declaration.JE_MessageType = MessageTypeList.Codes.Export;

				var guarantee3 = declaration.Guarantees.AddNew();
				guarantee3.EntryInstructionID = ZGuid.Invalid;
				AssertNoError(guarantee3.EntryInstructionIDInfo, "Enter a valid selection.");
			});
		}

		public void TestCheckPW_BondNumber()
		{
			var declaration = Factory.New<JobDeclaration>();

			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;
			entryInstruction1.CEI_Style = IMPDeclarationTypeList.Codes.H2;

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.B;

			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			CombineAssertions(() =>
			{
				var guarantee = declaration.Guarantees.AddNew();
				guarantee.PW_BondNumber = "22ES000000001";
				guarantee.Validation.ValidatePW_BondFiledPort();
				AssertNoMessageErrorContaining("Assert no message when have reference but is not define entry instruction", guarantee.PW_BondFiledPortInfo, "You have not entered a guarantee Office");

				guarantee.EntryInstructionID = entryInstruction1.PK;
				guarantee.Validation.ValidatePW_BondFiledPort();
				AssertHasMessageErrorContaining("Assert message when have reference but is define entry instruction H2 and Office is empty", guarantee.PW_BondFiledPortInfo, "You have not entered a guarantee Office");

				guarantee.PW_BondFiledPort = "0855";
				guarantee.Validation.ValidatePW_BondFiledPort();
				AssertNoMessageErrorContaining("Assert message when have reference but is define entry instruction H2 and Office is not empty", guarantee.PW_BondFiledPortInfo, "You have not entered a guarantee Office");

				guarantee.EntryInstructionID = entryInstruction2.PK;
				guarantee.Validation.ValidatePW_BondFiledPort();
				AssertNoMessageErrorContaining("Assert message when have reference but is define entry instruction not H2 and Office is not empty", guarantee.PW_BondFiledPortInfo, "You have not entered a guarantee Office");

				guarantee.PW_BondFiledPort = ZString.Empty;
				guarantee.Validation.ValidatePW_BondFiledPort();
				AssertNoMessageErrorContaining("Assert message when have reference but is define entry instruction is not H2 and Office is empty", guarantee.PW_BondFiledPortInfo, "You have not entered a guarantee Office");

				var guarantee1 = declaration.Guarantees.AddNew();
				guarantee1.PW_BondNumber = "22ES000000001";
				guarantee1.Validation.ValidatePW_BondFiledPort();
				AssertNoMessageErrorContaining("Assert no message in second line, when have reference but is not define entry instruction", guarantee1.PW_BondFiledPortInfo, "You have not entered a guarantee Office");

				guarantee1.EntryInstructionID = entryInstruction1.PK;
				guarantee1.Validation.ValidatePW_BondFiledPort();
				AssertHasMessageErrorContaining("Assert message in second line, when have reference but is define entry instruction H2 and Office is empty", guarantee1.PW_BondFiledPortInfo, "You have not entered a guarantee Office");

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				guarantee1.EntryInstructionID = entryInstruction1.PK;
				guarantee1.Validation.ValidatePW_BondFiledPort();
				AssertNoMessageErrorContaining("Assert no message in second line, when have reference but is define entry instruction H2 and Office is empty, for export", guarantee1.PW_BondFiledPortInfo, "You have not entered a guarantee Office");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			Factory.New<ESGuarantee>();
		}
	}
}
