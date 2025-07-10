using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Integration;

namespace Enterprise.Messaging.Business.Testing
{
	class EDIMessageInterpreterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckApplicationCode()
		{
			var interpreter = new EDIMessageInterpreter();
			interpreter.ApplicationCode = "$#@";
			AssertHasErrorContaining(interpreter.ApplicationCodeInfo, ListValidation.InvalidCodeError);
			interpreter.ApplicationCode = ZString.Empty;
			AssertNoErrorContaining(interpreter.ApplicationCodeInfo, ListValidation.InvalidCodeError);
			AssertHasErrorContaining(interpreter.ApplicationCodeInfo, MandatoryValidation.MustBeEntered);
			interpreter.ApplicationCode = ApplicationCodeList.Codes.USCustomsImport;
			AssertNoErrors(interpreter.ApplicationCodeInfo);
		}
	}
}
