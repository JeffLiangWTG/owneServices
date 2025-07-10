using CargoWise.EntityFramework.Testing;
using Enterprise.PAVE.MENT.Shared;

namespace Enterprise.PAVE.MENT.Business.Test
{
	public class ColumnFunctionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestParameter1Validation()
		{
			var function = new ColumnFunction(MENTConstants.DecimalColumn);
			function.FunctionType = ColumnFunctionTypes.Codes.None;

			function.Parameter1 = 0;

			AssertNoErrors(function.Parameter1Info);

			function.FunctionType = ColumnFunctionTypes.Codes.Round;

			function.Parameter1 = 0;

			AssertHasError(function.Parameter1Info, "Please enter a 'Parameter 1' greater than or equal to 0.001.");
		}

		public void TestFunctionTypeValidation()
		{
			var function = new ColumnFunction(MENTConstants.DecimalColumn);
			function.FunctionType = ColumnFunctionTypes.Codes.None;
			AssertNoErrors(function.FunctionTypeInfo);

			function.FunctionType = ColumnFunctionTypes.Codes.Round;
			AssertNoErrors(function.FunctionTypeInfo);

			var anotherFunction = new ColumnFunction(MENTConstants.StringColumn);
			anotherFunction.FunctionType = ColumnFunctionTypes.Codes.None;
			AssertNoErrors(anotherFunction.FunctionTypeInfo);

			anotherFunction.FunctionType = ColumnFunctionTypes.Codes.Round;
			AssertHasError(anotherFunction.FunctionTypeInfo, "Can only round decimal based columns");
		}

		public void TestFunctionTypeEntered()
		{
			var function = new ColumnFunction(MENTConstants.StringColumn);
			function.FunctionType = "PFT";
			AssertHasError(function.FunctionTypeInfo, "Enter a valid Function Type.");

			function.FunctionType = ColumnFunctionTypes.Codes.None;
			AssertNoErrors(function.FunctionTypeInfo);
		}
	}
}
