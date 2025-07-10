using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework.Testing
{
	sealed class CodePropertyAttributeTest : TestCaseWithFactory
	{
		public void TestCodeFromBusinessObject()
		{
			var testCode = new ZString("Code");
			var bizO = Factory.New(typeof(DummyBusinessObject)) as DummyBusinessObject;
			bizO.Z0_Code = testCode;
			AssertEquals(testCode, CodePropertyAttribute.CodeFromBusinessObject(bizO));
		}

		public void TestCodeFromBusinessObjectWithMultilingualString()
		{
			var testCode = (NoResString)"Code";
			var bizO = Factory.New(typeof(DummyBusinessObjectWithMultilingualString)) as DummyBusinessObjectWithMultilingualString;
			bizO.Code = testCode;
			AssertEquals(testCode, CodePropertyAttribute.CodeFromBusinessObject(bizO));
		}
	}
}
