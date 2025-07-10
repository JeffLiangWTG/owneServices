using System;
using Enterprise.Core;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OceanCarrierDataRegistry))]
	sealed class OceanCarrierDataRegistryTest : RegistryItemSetTestCaseWithFactory<OceanCarrierDataRegistry>
	{
		public void TestEnableNewCarrierSolution()
		{
			TestRegistryItem(
				item: ItemSet.EnableOceanCarrierSolution,
				expectedName: nameof(ItemSet.EnableOceanCarrierSolution),
				expectedCategory: OceanCarrierDataRegistry.Categories.OceanCarrier,
				expectedCaption: "Enable Ocean Carrier Solution",
				expectedHint: @"Set to true to enable Ocean Carrier Solution.",
				expectedStorage: RegistryStorageFlags.System,
				expectedOptions: RegistryOptions.IsOnlyForSupport,
				expectedDefaultValue: false);
		}

		public void TestOceanCarrierShipmentReferenceNumberFormat()
		{
			TestGenericRegistryItem(
				item: ItemSet.OceanCarrierShipmentReferenceNumberFormat,
				expectedName: nameof(ItemSet.OceanCarrierShipmentReferenceNumberFormat),
				expectedCategory: OceanCarrierDataRegistry.Categories.OceanCarrier,
				expectedCaption: "Shipment Reference Number Customization",
				expectedHint: "Override this value to customize how the ocean carrier shipment reference numbers are formatted.",
				expectedStorage: RegistryStorageFlags.System,
				expectedOptions: RegistryOptions.IsOnlyForSupport);
		}

		public void TestOceanCarrierShippingInstructionReferenceNumberFormat()
		{
			TestGenericRegistryItem(
				item: ItemSet.OceanCarrierShippingInstructionReferenceNumberFormat,
				expectedName: nameof(ItemSet.OceanCarrierShippingInstructionReferenceNumberFormat),
				expectedCategory: OceanCarrierDataRegistry.Categories.OceanCarrier,
				expectedCaption: "Shipping Instruction Number Customization",
				expectedHint: "Override this value to customize how the ocean carrier shipping instruction reference numbers are formatted.",
				expectedStorage: RegistryStorageFlags.System,
				expectedOptions: RegistryOptions.IsOnlyForSupport);
		}

		public void TestDefaultDimensionUnitForOceanCarrier()
		{
			TestGenericRegistryItem(
				item: ItemSet.DefaultDimensionUnitForOceanCarrier,
				expectedName: nameof(ItemSet.DefaultDimensionUnitForOceanCarrier),
				expectedCategory: OceanCarrierDataRegistry.Categories.OceanCarrier_DefaultUnits,
				expectedCaption: "Default Dimension Unit",
				expectedHint: "The default unit of dimension to use.",
				expectedStorage: RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				expectedOptions: RegistryOptions.Default,
				expectedDefaultValue: Constants.Dimension.Centimetres);
		}

		public void TestDefaultDistanceUnitForOceanCarrier()
		{
			TestGenericRegistryItem(
				item: ItemSet.DefaultDistanceUnitForOceanCarrier,
				expectedName: nameof(ItemSet.DefaultDistanceUnitForOceanCarrier),
				expectedCategory: OceanCarrierDataRegistry.Categories.OceanCarrier_DefaultUnits,
				expectedCaption: "Default Distance Unit",
				expectedHint: "The default unit of distance to use.",
				expectedStorage: RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				expectedOptions: RegistryOptions.Default,
				expectedDefaultValue: Constants.Distance.Kilometres);
		}

		public void TestDefaultAreaUnitForOceanCarrier()
		{
			TestGenericRegistryItem(
				item: ItemSet.DefaultAreaUnitForOceanCarrier,
				expectedName: nameof(ItemSet.DefaultAreaUnitForOceanCarrier),
				expectedCategory: OceanCarrierDataRegistry.Categories.OceanCarrier_DefaultUnits,
				expectedCaption: "Default Area Unit",
				expectedHint: "The default unit of area to use.",
				expectedStorage: RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				expectedOptions: RegistryOptions.Default,
				expectedDefaultValue: Constants.Area.SquareMetre);
		}

		public void TestDefaultVolumeUnitForOceanCarrier()
		{
			TestGenericRegistryItem(
				item: ItemSet.DefaultVolumeUnitForOceanCarrier,
				expectedName: nameof(ItemSet.DefaultVolumeUnitForOceanCarrier),
				expectedCategory: OceanCarrierDataRegistry.Categories.OceanCarrier_DefaultUnits,
				expectedCaption: "Default Volume Unit",
				expectedHint: "The default unit of volume to use.",
				expectedStorage: RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				expectedOptions: RegistryOptions.Default,
				expectedDefaultValue: Constants.Volume.CubicMetres);
		}

		public void TestDefaultWeightUnitForOceanCarrier()
		{
			TestGenericRegistryItem(
				item: ItemSet.DefaultWeightUnitForOceanCarrier,
				expectedName: nameof(ItemSet.DefaultWeightUnitForOceanCarrier),
				expectedCategory: OceanCarrierDataRegistry.Categories.OceanCarrier_DefaultUnits,
				expectedCaption: "Default Weight Unit",
				expectedHint: "The default unit of weight to use.",
				expectedStorage: RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				expectedOptions: RegistryOptions.Default,
				expectedDefaultValue: Constants.Weight.Kilograms);
		}

		public void TestRangeInDaysForPreviousAndNextPortCalls()
		{
			TestRegistryItem(
				item: ItemSet.RangeInDaysForPreviousAndNextPortCalls,
				expectedName: nameof(ItemSet.RangeInDaysForPreviousAndNextPortCalls),
				expectedCategory: OceanCarrierDataRegistry.Categories.OceanCarrier_Voyages,
				expectedCaption: "Range For Displaying Previous And Next Port Calls",
				expectedHint: "The range in days for which previous and next port calls of a voyage are displayed.",
				expectedStorage: RegistryStorageFlags.System | RegistryStorageFlags.Company,
				expectedOptions: RegistryOptions.Default,
				expectedDefaultValue: 30,
				expectedMinValue: 0,
				expectedMaxValue: Int32.MaxValue);
		}

		public void TestTariffMatchingModeForCost()
		{
			TestGenericRegistryItem(
				item: ItemSet.TariffMatchingModeForCost,
				expectedName: nameof(ItemSet.TariffMatchingModeForCost),
				expectedCategory: OceanCarrierDataRegistry.Categories.OceanCarrier_TariffMatchingMode,
				expectedCaption: "Tariff Matching Mode For Cost",
				expectedHint: "The tariff matching mode selected here will be used as the calculation mode for the 'Cost' component of Rating process.",
				expectedStorage: RegistryStorageFlags.System | RegistryStorageFlags.Company,
				expectedOptions: RegistryOptions.Default,
				expectedDefaultValue: TariffMatchingModeCodeList.Codes.LEG);
		}

		public void TestTariffMatchingModeForRevenue()
		{
			TestGenericRegistryItem(
				item: ItemSet.TariffMatchingModeForRevenue,
				expectedName: nameof(ItemSet.TariffMatchingModeForRevenue),
				expectedCategory: OceanCarrierDataRegistry.Categories.OceanCarrier_TariffMatchingMode,
				expectedCaption: "Tariff Matching Mode For Revenue",
				expectedHint: "The tariff matching mode selected here will be used as the calculation mode for the 'Revenue' component of Rating process.",
				expectedStorage: RegistryStorageFlags.System | RegistryStorageFlags.Company,
				expectedOptions: RegistryOptions.Default,
				expectedDefaultValue: TariffMatchingModeCodeList.Codes.LEG);
		}

		public void TestVgmThresholdPercentage()
		{
			TestRegistryItem(
				item: ItemSet.VgmThresholdPercentage,
				expectedName: nameof(ItemSet.VgmThresholdPercentage),
				expectedCategory: OceanCarrierDataRegistry.Categories.OceanCarrier,
				expectedCaption: "VGM Threshold Percentage",
				expectedHint: @"Threshold in percentage for comparisons between VGM gross weight, sum of container and package weights, dunnage and tare. If both percentage and weight in Kg threshold are defined, the lowest resulting weight difference is used.",
				expectedStorage: RegistryStorageFlags.System,
				expectedOptions: RegistryOptions.IsOnlyForController,
				expectedDefaultValue: 0m,
				expectedMinValue: 0m,
				expectedMaxValue: 100m);
		}

		public void TestVgmThresholdWeightInKg()
		{
			TestRegistryItem(
				item: ItemSet.VgmThresholdWeightInKg,
				expectedName: nameof(ItemSet.VgmThresholdWeightInKg),
				expectedCategory: OceanCarrierDataRegistry.Categories.OceanCarrier,
				expectedCaption: "VGM Threshold Weight",
				expectedHint: @"Threshold Weight in Kg for comparisons between VGM gross weight, sum of container and package weights, dunnage and tare. If both percentage and weight in Kg threshold are defined, the lowest resulting weight difference is used.",
				expectedStorage: RegistryStorageFlags.System,
				expectedOptions: RegistryOptions.IsOnlyForController,
				expectedDefaultValue: 0m,
				expectedMinValue: 0m,
				expectedMaxValue: 1000000000000m);
		}

		public void TestDefaultMinimumStayAndTravelTimes()
		{
			TestGenericRegistryItem(ItemSet.DefaultMinimumStayAndTravelTimes,
				"DefaultMinimumStayAndTravelTimes",
				ItemSet.DefaultMinimumStayAndTravelTimes.Category,
				"Default Stay And Travel Times",
				"The default minimum stay and travel times per transport mode.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company);
		}

		public void TestTransportModeCombinationBufferTimeList()
		{
			TestGenericRegistryItem(
				item: ItemSet.TransportModeCombinationBufferTimeList,
				expectedName: nameof(ItemSet.TransportModeCombinationBufferTimeList),
				expectedCategory: OceanCarrierDataRegistry.Categories.OceanCarrier_VoyageMapper,
				expectedCaption: "Default Buffer Times",
				expectedHint: "Defines the default buffer time in hours for each combination of load and unload transport modes.",
				expectedStorage: RegistryStorageFlags.System | RegistryStorageFlags.Company,
				expectedOptions: RegistryOptions.Default);
		}

		public void TestMaximumOffsetForEarliestStartDate()
		{
			TestRegistryItem(
				item: ItemSet.MaximumOffsetForEarliestStartDate,
				expectedName: nameof(ItemSet.MaximumOffsetForEarliestStartDate),
				expectedCategory: OceanCarrierDataRegistry.Categories.OceanCarrier_VoyageMapper,
				expectedCaption: "Maximum Offset For Earliest Start Date",
				expectedHint: "Defines the maximum offset related to the earliest start date in days.",
				expectedStorage: RegistryStorageFlags.System | RegistryStorageFlags.Company,
				expectedOptions: RegistryOptions.Default,
				expectedDefaultValue: 60,
				expectedMinValue: 0,
				expectedMaxValue: Int32.MaxValue);
		}
	}
}
