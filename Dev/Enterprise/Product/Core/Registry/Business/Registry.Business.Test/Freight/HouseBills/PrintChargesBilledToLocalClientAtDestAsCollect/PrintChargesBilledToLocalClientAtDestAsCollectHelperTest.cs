using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Registry.Business.Testing
{
	sealed class PrintChargesBilledToLocalClientAtDestAsCollectHelperTest : TestCaseWithFactory
	{
		public void TestIsEnabled()
		{
			var collection = new PrintChargesBilledToLocalClientAtDestAsCollectCollection();
			CreatePrintChargesBilledToLocalClientAtDestAsCollect(collection, Core.Constants.TransportModes.Sea, "AU", "NZ");

			using (FreightDataRegistry.Instance.PrintChargesBilledToLocalClientAtDestAsCollect.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				AssertEquals(true, PrintChargesBilledToLocalClientAtDestAsCollectHelper.IsEnabled("SEA", "AU", "NZ", ""));
				AssertEquals(false, PrintChargesBilledToLocalClientAtDestAsCollectHelper.IsEnabled("SEA", "AU", "", ""));
				AssertEquals(false, PrintChargesBilledToLocalClientAtDestAsCollectHelper.IsEnabled("SEA", "", "NZ", ""));
				AssertEquals(false, PrintChargesBilledToLocalClientAtDestAsCollectHelper.IsEnabled("SEA", "", "", ""));
			}

			collection = new PrintChargesBilledToLocalClientAtDestAsCollectCollection();
			CreatePrintChargesBilledToLocalClientAtDestAsCollect(collection, Core.Constants.TransportModes.Sea, "AU", "");

			using (FreightDataRegistry.Instance.PrintChargesBilledToLocalClientAtDestAsCollect.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				AssertEquals(true, PrintChargesBilledToLocalClientAtDestAsCollectHelper.IsEnabled("SEA", "AU", "NZ", ""));
				AssertEquals(true, PrintChargesBilledToLocalClientAtDestAsCollectHelper.IsEnabled("SEA", "AU", "", ""));
				AssertEquals(false, PrintChargesBilledToLocalClientAtDestAsCollectHelper.IsEnabled("SEA", "", "NZ", ""));
				AssertEquals(false, PrintChargesBilledToLocalClientAtDestAsCollectHelper.IsEnabled("SEA", "", "", ""));
			}

			collection = new PrintChargesBilledToLocalClientAtDestAsCollectCollection();
			CreatePrintChargesBilledToLocalClientAtDestAsCollect(collection, Core.Constants.TransportModes.Sea, "", "NZ");

			using (FreightDataRegistry.Instance.PrintChargesBilledToLocalClientAtDestAsCollect.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				AssertEquals(true, PrintChargesBilledToLocalClientAtDestAsCollectHelper.IsEnabled("SEA", "AU", "NZ", ""));
				AssertEquals(false, PrintChargesBilledToLocalClientAtDestAsCollectHelper.IsEnabled("SEA", "AU", "", ""));
				AssertEquals(true, PrintChargesBilledToLocalClientAtDestAsCollectHelper.IsEnabled("SEA", "", "NZ", ""));
				AssertEquals(false, PrintChargesBilledToLocalClientAtDestAsCollectHelper.IsEnabled("SEA", "", "", ""));
			}

			collection = new PrintChargesBilledToLocalClientAtDestAsCollectCollection();
			CreatePrintChargesBilledToLocalClientAtDestAsCollect(collection, Core.Constants.TransportModes.Sea, "", "");

			using (FreightDataRegistry.Instance.PrintChargesBilledToLocalClientAtDestAsCollect.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				AssertEquals(true, PrintChargesBilledToLocalClientAtDestAsCollectHelper.IsEnabled("SEA", "AU", "NZ", ""));
				AssertEquals(true, PrintChargesBilledToLocalClientAtDestAsCollectHelper.IsEnabled("SEA", "AU", "", ""));
				AssertEquals(true, PrintChargesBilledToLocalClientAtDestAsCollectHelper.IsEnabled("SEA", "", "NZ", ""));
				AssertEquals(true, PrintChargesBilledToLocalClientAtDestAsCollectHelper.IsEnabled("SEA", "", "", ""));
			}
		}

		#region Implementation

		static PrintChargesBilledToLocalClientAtDestAsCollect CreatePrintChargesBilledToLocalClientAtDestAsCollect(PrintChargesBilledToLocalClientAtDestAsCollectCollection collection, ZString transportMode, ZString exportCountry, ZString importCountry)
		{
			var setting = collection.AddNew();

			setting.TransportMode = transportMode;
			setting.ExportCountry = exportCountry;
			setting.ImportCountry = importCountry;

			return setting;
		}

		#endregion
	}
}
