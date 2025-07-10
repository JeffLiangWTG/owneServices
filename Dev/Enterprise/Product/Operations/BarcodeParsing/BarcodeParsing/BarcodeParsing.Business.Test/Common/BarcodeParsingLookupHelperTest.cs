using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	class BarcodeParsingLookupHelperTest : TransactionedTestCase
	{
		#region TestGetModuleTypes

		public void TestGetModuleTypes()
		{
			var factory = new BusinessObjectFactory();
			AssertContainsExactElementsInAnyOrder(new[] { BarcodeModuleTypes.Codes.ETail, BarcodeModuleTypes.Codes.Warehouse }, BarcodeParsingLookupHelper.ModuleTypes(factory).GetAllCodes());
		}

		#endregion

		#region TestDiagnosticsTypes

		public void TestDiagnosticsTypes()
		{
			var factory = new BusinessObjectFactory();
			AssertContainsExactElementsInAnyOrder(new[] { DiagnosticsTypes.Codes.Parsing, DiagnosticsTypes.Codes.Validation }, BarcodeParsingLookupHelper.DiagnosticsTypes(factory).GetAllCodes());
		}

		#endregion
	}
}
