using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	sealed class RadionuclideComponentTest : TestCaseWithFactory
	{
		public void TestCFRRadionuclideComponent_Curie()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			var dataItem = packline.UNDGs.AddNew();
			dataItem.DI_RadionuclideElement = RadionuclideElementConstants.Codes.Silver;
			dataItem.DI_RadionuclideElementSuffix = RadionuclideElementSuffixConstants.Silver108ma;
			dataItem.DI_RadioactiveMaximumActivity = 8m;
			dataItem.DI_RadioactiveMaximumActivityUnit = Core.Constants.RadioactiveUnits.Millicurie;

			var wrapper = new UNDGSubstanceWrapper(dataItem, Factory);
			var component = new RadionuclideComponent() as IUNDGSummaryWriterComponent;
			var expectedResult = "Ag-108m (a), 0.30 GBq (8 mCi)";
			AssertEquals(expectedResult, component.Write(wrapper));
		}

		public void TestCFRRadionuclideComponent_Becquerel()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			var dataItem = packline.UNDGs.AddNew();
			dataItem.DI_RadionuclideElement = RadionuclideElementConstants.Codes.Silver;
			dataItem.DI_RadionuclideElementSuffix = RadionuclideElementSuffixConstants.Silver108ma;
			dataItem.DI_RadioactiveMaximumActivity = 300m;
			dataItem.DI_RadioactiveMaximumActivityUnit = Core.Constants.RadioactiveUnits.Megabecquerel;

			var wrapper = new UNDGSubstanceWrapper(dataItem, Factory);
			var component = new RadionuclideComponent() as IUNDGSummaryWriterComponent;
			var expectedResult = "Ag-108m (a), 0.3 GBq (8.11 mCi)";
			AssertEquals(expectedResult, component.Write(wrapper));
		}

		public void TestCFRRadionuclideComponent_NoHyphen()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			var dataItem = packline.UNDGs.AddNew();
			dataItem.DI_RadionuclideElement = RadionuclideElementConstants.Codes.Uranium;
			dataItem.DI_RadionuclideElementSuffix = RadionuclideElementSuffixConstants.UraniumNat;
			dataItem.DI_RadioactiveMaximumActivity = 0.3m;
			dataItem.DI_RadioactiveMaximumActivityUnit = Core.Constants.RadioactiveUnits.Gigabecquerel;

			var wrapper = new UNDGSubstanceWrapper(dataItem, Factory);
			var component = new RadionuclideComponent() as IUNDGSummaryWriterComponent;
			var expectedResult = "U (nat), 0.3 GBq (8.11 mCi)";
			AssertEquals(expectedResult, component.Write(wrapper));
		}

		public void TestCFRRadionuclideComponent_Conversion()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			var dataItem = packline.UNDGs.AddNew();
			dataItem.DI_RadionuclideElement = RadionuclideElementConstants.Codes.Uranium;
			dataItem.DI_RadionuclideElementSuffix = RadionuclideElementSuffixConstants.UraniumNat;
			var wrapper = new UNDGSubstanceWrapper(dataItem, Factory);
			var component = new RadionuclideComponent() as IUNDGSummaryWriterComponent;

			dataItem.DI_RadioactiveMaximumActivity = 30m;
			dataItem.DI_RadioactiveMaximumActivityUnit = Core.Constants.RadioactiveUnits.Megabecquerel;
			AssertContains("30 MBq", component.Write(wrapper));

			dataItem.DI_RadioactiveMaximumActivity = 300m;
			dataItem.DI_RadioactiveMaximumActivityUnit = Core.Constants.RadioactiveUnits.Megabecquerel;
			AssertContains("0.3 GBq", component.Write(wrapper));

			dataItem.DI_RadioactiveMaximumActivity = 300000m;
			dataItem.DI_RadioactiveMaximumActivityUnit = Core.Constants.RadioactiveUnits.Megabecquerel;
			AssertContains("0.3 TBq", component.Write(wrapper));

			dataItem.DI_RadioactiveMaximumActivity = 0.01m;
			dataItem.DI_RadioactiveMaximumActivityUnit = Core.Constants.RadioactiveUnits.Millicurie;
			AssertContains("(10 uCi)", component.Write(wrapper));

			dataItem.DI_RadioactiveMaximumActivity = 10m;
			dataItem.DI_RadioactiveMaximumActivityUnit = Core.Constants.RadioactiveUnits.Millicurie;
			AssertContains("(10 mCi)", component.Write(wrapper));

			dataItem.DI_RadioactiveMaximumActivity = 100m;
			dataItem.DI_RadioactiveMaximumActivityUnit = Core.Constants.RadioactiveUnits.Millicurie;
			AssertContains("(0.1 Ci)", component.Write(wrapper));
		}
	}
}
