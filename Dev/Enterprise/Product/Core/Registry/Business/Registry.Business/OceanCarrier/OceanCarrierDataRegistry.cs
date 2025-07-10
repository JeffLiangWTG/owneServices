using System;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	public sealed class OceanCarrierDataRegistry : RegistryItemSet
	{
		#region Instance

		OceanCarrierDataRegistry()
		{
		}

		public static OceanCarrierDataRegistry Instance => instance ?? (instance = new OceanCarrierDataRegistry());

		[ThreadStatic]
		static OceanCarrierDataRegistry instance;

		#endregion

		public override bool IsForProductivityWise => false;

		public sealed class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString OceanCarrier => ResString.GetMultilingualString("2c18174d-d807-43b6-a10a-17f5c612b8fa", "Ocean Carrier");
			public static MultilingualString OceanCarrier_DefaultUnits => CombineCategories(OceanCarrier, ResString.GetMultilingualString("0a494a4a-5ded-11ee-8c99-0242ac120002", "Default Units"));
			public static MultilingualString OceanCarrier_TariffMatchingMode => CombineCategories(OceanCarrier, ResString.GetMultilingualString("7c78645c-ba18-49ee-a1b3-762d7a81340e", "Tariff Matching Mode"));
			public static MultilingualString OceanCarrier_Voyages => CombineCategories(OceanCarrier, ResString.GetMultilingualString("507d111a-4bb7-4740-aca6-581d7952509e", "Voyages"));
			public static MultilingualString OceanCarrier_VoyageMapper => CombineCategories(OceanCarrier_Voyages, ResString.GetMultilingualString("9fe7c8a3-72c1-4dca-810c-9cf8b7932599", "Voyage Mapper"));
		}

		public CodePairRegistryItem DefaultDimensionUnitForOceanCarrier
		{
			get
			{
				const string registryItemName = "DefaultDimensionUnitForOceanCarrier";

				return GetItem(registryItemName, () =>
					new CodePairRegistryItem(new OceanCarrier.RegistryItemImplementationWithDefaultDelegate<string>(
						registryItemName,
						Categories.OceanCarrier_DefaultUnits,
						ResString.GetMultilingualString("4a5e3367-9291-4e15-b715-639df2fdd79a", "Default Dimension Unit"),
						ResString.GetMultilingualString("a76f33c2-8e30-4c69-93c2-b8d99b647470", "The default unit of dimension to use."),
						new CodePairRegistryDataType(OLookUpEditType.Dimension, false, true),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						(c, b, d) => Constants.Dimension.Centimetres)));
			}
		}

		public CodePairRegistryItem DefaultDistanceUnitForOceanCarrier
		{
			get
			{
				const string registryItemName = "DefaultDistanceUnitForOceanCarrier";

				return GetItem(registryItemName, () =>
					new CodePairRegistryItem(new OceanCarrier.RegistryItemImplementationWithDefaultDelegate<string>(
						registryItemName,
						Categories.OceanCarrier_DefaultUnits,
						ResString.GetMultilingualString("f3a46d33-cfb8-4082-9bf0-840e86015b3d", "Default Distance Unit"),
						ResString.GetMultilingualString("b0a9d63a-fc6f-4d1a-8e15-d1474cfc6e83", "The default unit of distance to use."),
						new CodePairRegistryDataType(OLookUpEditType.Distance, false, true),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						(c, b, d) => Constants.Distance.Kilometres)));
			}
		}

		public CodePairRegistryItem DefaultAreaUnitForOceanCarrier
		{
			get
			{
				const string registryItemName = "DefaultAreaUnitForOceanCarrier";

				return GetItem(registryItemName, () =>
					new CodePairRegistryItem(new OceanCarrier.RegistryItemImplementationWithDefaultDelegate<string>(
						registryItemName,
						Categories.OceanCarrier_DefaultUnits,
						ResString.GetMultilingualString("98bc7a96-4a86-49ea-af3e-791a3f598512", "Default Area Unit"),
						ResString.GetMultilingualString("2c39361e-114e-42d4-b270-caf94fc8a71a", "The default unit of area to use."),
						new CodePairRegistryDataType(OLookUpEditType.Area, false, true),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						(c, b, d) => Constants.Area.SquareMetre)));
			}
		}

		public CodePairRegistryItem DefaultVolumeUnitForOceanCarrier
		{
			get
			{
				const string registryItemName = "DefaultVolumeUnitForOceanCarrier";

				return GetItem(registryItemName, () =>
					new CodePairRegistryItem(new OceanCarrier.RegistryItemImplementationWithDefaultDelegate<string>(
						registryItemName,
						Categories.OceanCarrier_DefaultUnits,
						ResString.GetMultilingualString("791f6a51-e0b3-432a-a2ae-66a27d5f6b2f", "Default Volume Unit"),
						ResString.GetMultilingualString("fb41eb0a-b119-43b0-88c6-b5202ee07171", "The default unit of volume to use."),
						new CodePairRegistryDataType(OLookUpEditType.Volume, false, true),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						(c, b, d) => Constants.Volume.CubicMetres)));
			}
		}

		public CodePairRegistryItem DefaultWeightUnitForOceanCarrier
		{
			get
			{
				const string registryItemName = "DefaultWeightUnitForOceanCarrier";

				return GetItem(registryItemName, () =>
					new CodePairRegistryItem(new OceanCarrier.RegistryItemImplementationWithDefaultDelegate<string>(
						registryItemName,
						Categories.OceanCarrier_DefaultUnits,
						ResString.GetMultilingualString("4fcf0cca-f139-4fe4-b33e-11dc852da815", "Default Weight Unit"),
						ResString.GetMultilingualString("84a1dbf2-4d5f-40b4-b762-485e62737720", "The default unit of weight to use."),
						new CodePairRegistryDataType(OLookUpEditType.Weight, false, true),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						(c, b, d) => Constants.Weight.Kilograms)));
			}
		}

		public BooleanRegistryItem EnableOceanCarrierSolution
		{
			get
			{
				const string registryItemName = "EnableOceanCarrierSolution";

				return GetItem(registryItemName, () =>
					new BooleanRegistryItem(
						registryItemName,
						Categories.OceanCarrier,
						(NoResString)"Enable Ocean Carrier Solution",
						(NoResString)"Set to true to enable Ocean Carrier Solution.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false));
			}
		}

		public BillCustomisationRegistryItem OceanCarrierShipmentReferenceNumberFormat
		{
			get
			{
				const string registryItemName = "OceanCarrierShipmentReferenceNumberFormat";

				return GetItem(registryItemName, () =>
				{
					var dataType = new BillCustomisationRegistryDataType();
					dataType.FountainPrefix = "CA";
					dataType.GeneratedNumberName = ResString.GetMultilingualString("15B40CAE-47F4-4445-B2A6-B83A2619BED2", "Shipping Reference Number");
					dataType.SequenceNumberName = ResString.GetMultilingualString("BE0058CF-D46E-466D-85DC-0B2BFD87C2C1", "Shipment");
					dataType.MaxLength = CarrierShipmentHeaderSchema.CSH_CarrierShipmentReference.MaxLength;
					dataType.Categories |= NumberCustomisationElementCategories.Standard;
					dataType.AllowNonAlphanumericCharacters = true;
					dataType.EnableMacroInsertion = false;

					return new BillCustomisationRegistryItem(
						name: registryItemName,
						category: Categories.OceanCarrier,
						caption: (NoResString)"Shipment Reference Number Customization",
						hint: (NoResString)"Override this value to customize how the ocean carrier shipment reference numbers are formatted.",
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.IsOnlyForSupport,
						dataType);
				});
			}
		}

		public BillCustomisationRegistryItem OceanCarrierShippingInstructionReferenceNumberFormat
		{
			get
			{
				const string registryItemName = "OceanCarrierShippingInstructionReferenceNumberFormat";

				return GetItem(registryItemName, () =>
				{
					var dataType = new BillCustomisationRegistryDataType();
					dataType.FountainPrefix = "SI";
					dataType.GeneratedNumberName = ResString.GetMultilingualString("499d075f-e9e6-4b08-91a6-e99535ad0d6d", "Shipping Instruction Number");
					dataType.SequenceNumberName = ResString.GetMultilingualString("dfa9fdc5-325a-4eed-8a95-f3088adc4f1a", "Shipment");
					dataType.MaxLength = CarrierShipmentHeaderSchema.CSH_CarrierShipmentReference.MaxLength;
					dataType.Categories |= NumberCustomisationElementCategories.OceanCarrier;
					dataType.AllowNonAlphanumericCharacters = true;
					dataType.EnableMacroInsertion = false;

					return new BillCustomisationRegistryItem(
						name: registryItemName,
						category: Categories.OceanCarrier,
						caption: (NoResString)"Shipping Instruction Number Customization",
						hint: (NoResString)"Override this value to customize how the ocean carrier shipping instruction reference numbers are formatted.",
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.IsOnlyForSupport,
						dataType);
				});
			}
		}

		public IntRegistryItem RangeInDaysForPreviousAndNextPortCalls
		{
			get
			{
				const string registryItemName = "RangeInDaysForPreviousAndNextPortCalls";

				return GetItem(registryItemName, () =>
					new IntRegistryItem(
						registryItemName,
						Categories.OceanCarrier_Voyages,
						ResString.GetMultilingualString("18afaaea-62ad-4f8a-8936-ebc8391b0f0b", "Range For Displaying Previous And Next Port Calls"),
						ResString.GetMultilingualString("bf76d741-8a81-41bd-8f63-eb2abad59dd3", "The range in days for which previous and next port calls of a voyage are displayed."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						30,
						0,
						Int32.MaxValue
					));
			}
		}

		public CodePairRegistryItem TariffMatchingModeForCost
		{
			get
			{
				return GetItem("TariffMatchingModeForCost", () =>
					new CodePairRegistryItem(
						name: "TariffMatchingModeForCost",
						category: Categories.OceanCarrier_TariffMatchingMode,
						caption: ResString.GetMultilingualString("936b548c-0cc1-4434-bb61-040a24b8cc7d", "Tariff Matching Mode For Cost"),
						hint: ResString.GetMultilingualString("92b12ab3-f0b5-45c5-b67f-958a5a1eec2e", "The tariff matching mode selected here will be used as the calculation mode for the 'Cost' component of Rating process."),
						lookUpList: new CodeDescriptionPairListProvider(() => new TariffMatchingModeCodeList()),
						storage: RegistryStorageFlags.System | RegistryStorageFlags.Company,
						options: RegistryOptions.Default,
						defaultValue: TariffMatchingModeCodeList.Codes.LEG));
			}
		}

		public CodePairRegistryItem TariffMatchingModeForRevenue
		{
			get
			{
				return GetItem("TariffMatchingModeForRevenue", () =>
					new CodePairRegistryItem(
						name: "TariffMatchingModeForRevenue",
						category: Categories.OceanCarrier_TariffMatchingMode,
						caption: ResString.GetMultilingualString("c6d6453d-a820-49f3-a98f-88180410598f", "Tariff Matching Mode For Revenue"),
						hint: ResString.GetMultilingualString("b0017448-f7d9-4459-af49-a863c7cec60d", "The tariff matching mode selected here will be used as the calculation mode for the 'Revenue' component of Rating process."),
						lookUpList: new CodeDescriptionPairListProvider(() => new TariffMatchingModeCodeList()),
						storage: RegistryStorageFlags.System | RegistryStorageFlags.Company,
						options: RegistryOptions.Default,
						defaultValue: TariffMatchingModeCodeList.Codes.LEG));
			}
		}

		public DecimalRegistryItem VgmThresholdPercentage
		{
			get
			{
				const string registryItemName = "VgmThresholdPercentage";

				return GetItem(registryItemName, () =>
					new DecimalRegistryItem(
						registryItemName,
						Categories.OceanCarrier,
						ResString.GetMultilingualString("722f50ad-be71-492a-994d-84693026fbda", "VGM Threshold Percentage"),
						ResString.GetMultilingualString("979eb310-7a7e-4db8-aa0e-96d3c3df062f", "Threshold in percentage for comparisons between VGM gross weight, sum of container and package weights, dunnage and tare. If both percentage and weight in Kg threshold are defined, the lowest resulting weight difference is used."),
						new NumericRegistryEditorInfo(2),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						0m,
						0,
						100
					));
			}
		}

		public DecimalRegistryItem VgmThresholdWeightInKg
		{
			get
			{
				const string registryItemName = "VgmThresholdWeightInKg";

				return GetItem(registryItemName, () =>
					new DecimalRegistryItem(
						registryItemName,
						Categories.OceanCarrier,
						ResString.GetMultilingualString("b4c777a4-ab14-4d34-ad4b-5aadc67b35cb", "VGM Threshold Weight"),
						ResString.GetMultilingualString("2ab1e86f-a7b1-417a-bf3c-7daa43ac5013", "Threshold Weight in Kg for comparisons between VGM gross weight, sum of container and package weights, dunnage and tare. If both percentage and weight in Kg threshold are defined, the lowest resulting weight difference is used."),
						new NumericRegistryEditorInfo(1),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						0m,
						0,
						1000000000000
					));
			}
		}

		public DefaultMinimumStayAndTravelTimeCollectionRegistryItem DefaultMinimumStayAndTravelTimes
		{
			get
			{
				const string registryItemName = "DefaultMinimumStayAndTravelTimes";

				return GetItem(registryItemName, () =>
					new DefaultMinimumStayAndTravelTimeCollectionRegistryItem(
						name: registryItemName,
						category: Categories.OceanCarrier_Voyages,
						caption: ResString.GetMultilingualString("ab615654-eef8-4764-b1b3-cf54a9cc5271", "Default Stay And Travel Times"),
						hint: ResString.GetMultilingualString("4befe6c7-3361-454f-a910-d7633a15269e", "The default minimum stay and travel times per transport mode."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						defaultValue: DefaultMinimumStayAndTravelTimeCollection.DefaultValue));
			}
		}

		public TransportModeCombinationBufferTimeRegistryItem TransportModeCombinationBufferTimeList
		{
			get
			{
				const string registryItemName = "TransportModeCombinationBufferTimeList";

				return GetItem(registryItemName, () =>
					new TransportModeCombinationBufferTimeRegistryItem(
						name: registryItemName,
						category: Categories.OceanCarrier_VoyageMapper,
						caption: ResString.GetMultilingualString("5b755faa-658d-4b5a-938e-bde09bc40a79", "Default Buffer Times"),
						hint: ResString.GetMultilingualString("2162266c-9ddd-45a9-8d42-1587d93c6005", "Defines the default buffer time in hours for each combination of load and unload transport modes."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						defaultValue: TransportModeCombinationBufferTimeCollection.DefaultValue
				));
			}
		}

		public IntRegistryItem MaximumOffsetForEarliestStartDate
		{
			get
			{
				return GetItem("MaximumOffsetForEarliestStartDate", delegate
				{
					return new IntRegistryItem(
						"MaximumOffsetForEarliestStartDate",
						Categories.OceanCarrier_VoyageMapper,
						ResString.GetMultilingualString("03deac33-9ee8-4976-b266-3d437513bc59", "Maximum Offset For Earliest Start Date"),
						ResString.GetMultilingualString("f9756d1b-d74a-4389-a317-3d05621506bf", "Defines the maximum offset related to the earliest start date in days."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						defaultValue: 60, minValue: 0, maxValue: Int32.MaxValue);
				});
			}
		}
	}
}
