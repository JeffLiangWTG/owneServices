using Enterprise.Customs.CA.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.CA.DataTransfer.Universal
{
	public static class AddInfoGroupOrganizationAddressCollectionUpdator
	{
		internal static void UpdateForDFO(IOrganizationAddressCollectionParent parent, DFOPGAHeader dfoPGAHeader, IDataWritingManager writeManager)
		{
			if (dfoPGAHeader != null)
			{
				parent.AddOrgAddress(writeManager, dfoPGAHeader.HarvestingParty, Constants.AddressType.HarvestingParty);
				parent.AddOrgAddress(writeManager, dfoPGAHeader.Processor, Constants.AddressType.FoodProcessor);
			}
		}

		internal static void UpdateForLPCO(IOrganizationAddressCollectionParent parent, LPCOView lpco, IDataWritingManager writeManager)
		{
			if (lpco != null)
			{
				if (lpco.CLP_ApplicantType == LPCOHolderPartyTypeCodes.Codes.Other)
				{
					var addOrgAddress = parent.AddOrgAddress(writeManager, lpco.OthLPCOApplicant, Constants.AddressType.LPCOApplicant);
					if (addOrgAddress != null)
					{
						addOrgAddress.AddressOverride = lpco.CLP_IsApplicantOverridden;
					}
				}
				if (lpco.CLP_HolderType == LPCOHolderPartyTypeCodes.Codes.Other)
				{
					var addOrgAddress = parent.AddOrgAddress(writeManager, lpco.OthLPCOHolder, Constants.AddressType.LPCOHolder);
					if (addOrgAddress != null)
					{
						addOrgAddress.AddressOverride = lpco.CLP_IsHolderOverridden;
					}
				}
			}
		}

		internal static void UpdateForECCC(IOrganizationAddressCollectionParent parent, ECCCPGAHeader eccc, IDataWritingManager writeManager)
		{
			if (eccc != null)
			{
				parent.AddOrgAddress(writeManager, eccc.MachineManufacturer, Constants.AddressType.ECCCMachineManufacturer);
				parent.AddOrgAddress(writeManager, eccc.EngineLocation, Constants.AddressType.ECCCEngineLocation);
				parent.AddOrgAddress(writeManager, eccc.EvidenceOfConformityLocation, Constants.AddressType.ECCCEvidenceOfConfirmityLocation);
			}
		}
	}
}
