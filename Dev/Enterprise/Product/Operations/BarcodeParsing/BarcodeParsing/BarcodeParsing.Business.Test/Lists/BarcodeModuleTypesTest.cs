using CargoWise.EntityFramework.Testing;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	class BarcodeModuleTypesTest : TestCaseWithFactory
	{
		#region TestAllBarcodeModulesHaveMappedBarcodeParsingConsumer

		public void TestAllBarcodeModulesHaveMappedBarcodeParsingConsumer()
		{
			foreach (var module in new BarcodeModuleTypes().ToArray())
			{
				var consumer = Factory.GetBarcodeParsingConsumerFromModuleCode(module.Code);
				AssertNotNull("All BarcodeModuleTypes should be mapped to a Barcode Parsing Consumer, missing mapping for: " + module, consumer);
				AssertNotEquals("All BarcodeModuleTypes should be mapped to a Barcode Parsing Consumer, missing mapping for: " + module, typeof(DefaultBarcodeParsingConsumer), consumer.GetType());
			}
		}

		#endregion
	}
}
