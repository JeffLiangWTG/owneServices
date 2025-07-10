using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Integration;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	class BusinessObjectFactoryExtensionsTest : BarcodeParsingTestCase
	{
		#region TestGetBarcodeParsingConsumerFromModuleCode

		public void TestGetBarcodeParsingConsumerFromModuleCode()
		{
			AssertEquals(typeof(DummyBarcodeParsingConsumer), Factory.GetBarcodeParsingConsumerFromModuleCode(DummyBarcodeParsingConsumer.Module).GetType());
			AssertEquals(ObjectFactory.Get<IBarcodeParsingConsumer>("ETailBarcodeParsingConsumer", Factory).GetType(), Factory.GetBarcodeParsingConsumerFromModuleCode("ETL").GetType());
			AssertEquals(ObjectFactory.Get<IBarcodeParsingConsumer>("WarehouseBarcodeParsingConsumer", Factory).GetType(), Factory.GetBarcodeParsingConsumerFromModuleCode("WHS").GetType());
			AssertEquals(typeof(DefaultBarcodeParsingConsumer), Factory.GetBarcodeParsingConsumerFromModuleCode("xXx").GetType());
			AssertEquals(typeof(DefaultBarcodeParsingConsumer), Factory.GetBarcodeParsingConsumerFromModuleCode("").GetType());
			AssertEquals(typeof(DefaultBarcodeParsingConsumer), Factory.GetBarcodeParsingConsumerFromModuleCode(null).GetType());

			var moduleTypes = BarcodeParsingLookupHelper.ModuleTypes(Factory);
			ICachedValueManager icachedValueManager = moduleTypes;
			icachedValueManager.IsCacheEnabled = false;

			using (new DisposableAction(() => icachedValueManager.IsCacheEnabled = true))
			{
				moduleTypes.AddPair("RND", "Random");
			}

			AssertExceptionThrown(typeof(InvalidOperationException), "All BarcodeModuleTypes must be mapped to BarcodeParsingConsumers in the application configuration list. Missing Mapping for BarcodeModuleType: RND.",
				() => Factory.GetBarcodeParsingConsumerFromModuleCode("RND"));
		}

		#endregion
	}
}
