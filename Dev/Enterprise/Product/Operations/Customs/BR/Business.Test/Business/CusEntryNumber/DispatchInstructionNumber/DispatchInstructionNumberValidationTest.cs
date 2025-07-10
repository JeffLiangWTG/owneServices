using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class DispatchInstructionNumberValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCE_EntryType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var number = declaration.DispatchInstructionNumbers.AddNew();
			number.CE_EntryType = "";
			AssertHasMessageErrorContaining(number.CE_EntryTypeInfo, "entered");
			number.CE_EntryType = "XX";
			AssertNoMessageErrorContaining(number.CE_EntryTypeInfo, "entered");
			AssertHasMessageErrorContaining(number.CE_EntryTypeInfo, ListValidation.InvalidCodeMessageError);
			number.CE_EntryType = DispatchInstructionDocumentTypes.Codes._01;
			AssertNoMessageErrorContaining(number.CE_EntryTypeInfo, "entered");
			AssertNoMessageErrorContaining(number.CE_EntryTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCE_EntryNum()
		{
			var declaration = Factory.New<JobDeclaration>();
			var number = declaration.DispatchInstructionNumbers.AddNew();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(number.CE_EntryNumInfo);
		}
	}
}
