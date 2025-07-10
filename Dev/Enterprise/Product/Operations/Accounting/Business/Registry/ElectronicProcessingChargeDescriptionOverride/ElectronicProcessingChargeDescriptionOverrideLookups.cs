using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Registry.Business
{
	public class ElectronicProcessingChargeDescriptionOverrideLookups
	{
		public ElectronicProcessingChargeDescriptionOverrideLookups(ElectronicProcessingChargeDescriptionOverride electronicProcessingChargeDescriptionOverride)
		{
			Parent = electronicProcessingChargeDescriptionOverride;
			ReadOnlyFactory = new ReadOnlyBusinessObjectFactory();
		}

		readonly ElectronicProcessingChargeDescriptionOverride Parent;

		public CodeDescriptionPairList Transports
		{
			get
			{
				return ReadOnlyFactory.GetCachedValue("FreightCodePairLists.JS_TransportModeList",
					() =>
					{
						var transportModeList = new CodeDescriptionPairList();
						transportModeList.AddPair(AccountingMasterFilesConstants.ApportionmentMethod.AllCode, AccountingMasterFilesConstants.ApportionmentMethod.AllTransportModeDescription);
						transportModeList.AddRange(FreightCodePairLists.JS_TransportModeList());
						return transportModeList;
					});
			}
		}

		public CodeDescriptionPairList Containers
		{
			get
			{
				return ReadOnlyFactory.GetCachedValue("ElectronicProcessingChargeDescriptionOverride.ContainersList_" + Parent.Transport,
					() =>
					{
						var containerList = new CodeDescriptionPairList();
						containerList.AddPair(Core.Constants.ContainerModes.All, Core.Constants.ContainerModeDescriptions.All);
						if (Parent.Transport == AccountingMasterFilesConstants.ApportionmentMethod.AllCode)
						{
							containerList.AddRange(FreightCodePairLists.JS_PackingModeList(string.Empty));
						}
						else
						{
							containerList.AddRange(FreightCodePairLists.JS_PackingModeList(Parent.Transport));
						}
						return containerList;
					});
			}
		}

		public CodeDescriptionPairList ShipmentTypes
		{
			get
			{
				if (shipmentTypes == null)
				{
					shipmentTypes = new CodeDescriptionPairList();
					shipmentTypes.AddPair(AccountingMasterFilesConstants.ApportionmentMethod.AllCode, AccountingConstants.ShipmentTypeDescriptions.All);
					shipmentTypes.AddRange(FreightCodePairLists.JS_ShipmentTypeList());
				}

				return shipmentTypes;
			}
		}
		CodeDescriptionPairList shipmentTypes;

		public CodeDescriptionPairList Origins
		{
			get
			{
				if (origins == null)
				{
					origins = AccountingConstants.ElectronicProcessingChargeDescriptionOverrideOriginDestinationList;
				}

				return origins;
			}
		}
		CodeDescriptionPairList origins;

		public CodeDescriptionPairList Destinations
		{
			get
			{
				if (destinations == null)
				{
					destinations = AccountingConstants.ElectronicProcessingChargeDescriptionOverrideOriginDestinationList;
				}

				return destinations;
			}
		}
		CodeDescriptionPairList destinations;

		public CodeDescriptionPairList PrefixSuffix
		{
			get
			{
				if (prefixSuffix == null)
				{
					prefixSuffix = AccountingConstants.ElectronicProcessingChargeDescriptionOverridePrefixSuffixList;
				}

				return prefixSuffix;
			}
		}
		CodeDescriptionPairList prefixSuffix;

		ReadOnlyBusinessObjectFactory ReadOnlyFactory { get; set; }
	}
}
