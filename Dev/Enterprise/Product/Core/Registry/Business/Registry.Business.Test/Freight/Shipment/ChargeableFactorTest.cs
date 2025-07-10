using System;
using Enterprise.Environment;
using Enterprise.Registry.Business.Warehouse;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ChargeableFactor))]
	sealed class ChargeableFactorTest : RegistryBusinessObjectTemplateTestCase<ChargeableFactor>
	{
		#region TestGetDefault

		public void TestGetDefault()
		{
			ChargeableFactor domesticAirFactor = new ChargeableFactor();
			ChargeableFactor internationalAirFactor = new ChargeableFactor();
			FreightDataRegistry.Instance.DomesticChargeableFactorAir.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, domesticAirFactor);
			FreightDataRegistry.Instance.InternationalChargeableFactorAir.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, internationalAirFactor);

			ChargeableFactor domesticRoadFactor = new ChargeableFactor();
			ChargeableFactor internationalRoadFactor = new ChargeableFactor();
			FreightDataRegistry.Instance.DomesticChargeableFactorRoad.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, domesticRoadFactor);
			FreightDataRegistry.Instance.InternationalChargeableFactorRoad.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, internationalRoadFactor);

			ChargeableFactor domesticCourierFactor = new ChargeableFactor();
			ChargeableFactor internationalCourierFactor = new ChargeableFactor();
			FreightDataRegistry.Instance.DomesticChargeableFactorCourier.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, domesticCourierFactor);
			FreightDataRegistry.Instance.InternationalChargeableFactorCourier.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, internationalCourierFactor);

			ChargeableFactor domesticRailFactor = new ChargeableFactor();
			ChargeableFactor internationalRailFactor = new ChargeableFactor();
			FreightDataRegistry.Instance.DomesticChargeableFactorRail.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, domesticRailFactor);
			FreightDataRegistry.Instance.InternationalChargeableFactorRail.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, internationalRailFactor);

			ChargeableFactor domesticSeaFactor = new ChargeableFactor();
			ChargeableFactor internationalSeaFactor = new ChargeableFactor();
			FreightDataRegistry.Instance.DomesticChargeableFactorSea.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, domesticSeaFactor);
			FreightDataRegistry.Instance.InternationalChargeableFactorSea.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, internationalSeaFactor);

			var warehouseStorageFactor = new ChargeableFactor();
			var warehouseHandlingFactor = new ChargeableFactor();
			WarehouseDataRegistry.Instance.WarehouseChargeableFactorStorage.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, warehouseStorageFactor);
			WarehouseDataRegistry.Instance.WarehouseChargeableFactorHandling.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, warehouseHandlingFactor);

			var transitWarehouseAirFactor = new ChargeableFactor();
			var transitWarehouseSeaFactor = new ChargeableFactor();
			var transitWarehouseRoadFactor = new ChargeableFactor();
			WarehouseDataRegistry.Instance.TransitChargeableFactorForAir.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, transitWarehouseAirFactor);
			WarehouseDataRegistry.Instance.TransitChargeableFactorForSea.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, transitWarehouseSeaFactor);
			WarehouseDataRegistry.Instance.TransitChargeableFactorForRoad.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, transitWarehouseRoadFactor);

			AssertEquals(domesticAirFactor, ChargeableFactor.GetDefault(ChargeableFactorSource.Domestic, Core.Constants.TransportModes.Air));
			AssertEquals(internationalAirFactor, ChargeableFactor.GetDefault(ChargeableFactorSource.International, Core.Constants.TransportModes.Air));

			AssertEquals(domesticAirFactor, ChargeableFactor.GetDefault(ChargeableFactorSource.Domestic, Core.Constants.TransportModes.AirSea));
			AssertEquals(internationalAirFactor, ChargeableFactor.GetDefault(ChargeableFactorSource.International, Core.Constants.TransportModes.AirSea));

			AssertEquals(domesticRoadFactor, ChargeableFactor.GetDefault(ChargeableFactorSource.Domestic, Core.Constants.TransportModes.Road));
			AssertEquals(internationalRoadFactor, ChargeableFactor.GetDefault(ChargeableFactorSource.International, Core.Constants.TransportModes.Road));

			AssertEquals(domesticCourierFactor, ChargeableFactor.GetDefault(ChargeableFactorSource.Domestic, Core.Constants.TransportModes.Courier));
			AssertEquals(internationalCourierFactor, ChargeableFactor.GetDefault(ChargeableFactorSource.International, Core.Constants.TransportModes.Courier));

			AssertEquals(domesticCourierFactor, ChargeableFactor.GetDefault(ChargeableFactorSource.Domestic, Core.Constants.TransportModes.Mail));
			AssertEquals(internationalCourierFactor, ChargeableFactor.GetDefault(ChargeableFactorSource.International, Core.Constants.TransportModes.Mail));

			AssertEquals(domesticRailFactor, ChargeableFactor.GetDefault(ChargeableFactorSource.Domestic, Core.Constants.TransportModes.Rail));
			AssertEquals(internationalRailFactor, ChargeableFactor.GetDefault(ChargeableFactorSource.International, Core.Constants.TransportModes.Rail));

			AssertEquals(domesticSeaFactor, ChargeableFactor.GetDefault(ChargeableFactorSource.Domestic, Core.Constants.TransportModes.Sea));
			AssertEquals(internationalSeaFactor, ChargeableFactor.GetDefault(ChargeableFactorSource.International, Core.Constants.TransportModes.Sea));

			AssertEquals(warehouseStorageFactor, ChargeableFactor.GetDefault(ChargeableFactorSource.Warehouse, Core.Constants.TransportModes.Storage));
			AssertEquals(warehouseHandlingFactor, ChargeableFactor.GetDefault(ChargeableFactorSource.Warehouse, Core.Constants.TransportModes.WarehouseHandling));

			AssertEquals(transitWarehouseAirFactor, ChargeableFactor.GetDefault(ChargeableFactorSource.TransitWarehouse, Core.Constants.TransportModes.Air));
			AssertEquals(transitWarehouseSeaFactor, ChargeableFactor.GetDefault(ChargeableFactorSource.TransitWarehouse, Core.Constants.TransportModes.Sea));
			AssertEquals(transitWarehouseRoadFactor, ChargeableFactor.GetDefault(ChargeableFactorSource.TransitWarehouse, Core.Constants.TransportModes.Road));
		}

		#endregion

		#region Implementation

		protected override ChargeableFactor GetBusinessObjectToClone()
		{
			return new ChargeableFactor();
		}

		protected override ChargeableFactor GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
