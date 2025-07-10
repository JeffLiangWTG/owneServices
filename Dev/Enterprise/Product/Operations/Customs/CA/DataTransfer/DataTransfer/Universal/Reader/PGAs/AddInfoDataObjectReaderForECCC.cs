using System;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.DataTransfer.Universal
{
	public class AddInfoDataObjectReaderForECCC : AddInfoDataObjectReader<ECCCPGAHeader>
	{
		public AddInfoDataObjectReaderForECCC(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper)
			: base(logger, helper, CusAddInfoSchema.B7_AddInfoData, ECCCPGAHeaderAddInfoSchema.Instance)
		{
		}

		protected override void ReadOrganizationAddressCollection(Customs.Business.IAddInfoManager addInfoManager, IOrganizationAddressCollectionParent organizationAddresContainer, Action<ZString, ZString, string> setValue)
		{
			if (organizationAddresContainer.OrganizationAddressCollection != null)
			{
				var ecccPGAHeader = (ECCCPGAHeader)addInfoManager;
				SetAddressPK(setValue, ecccPGAHeader.CA_MachineManufacturerInfo, organizationAddresContainer, Constants.AddressType.ECCCMachineManufacturer, OrganisationTypes.None);
				SetAddressPK(setValue, ecccPGAHeader.CA_OA_EngineLocationInfo, organizationAddresContainer, Constants.AddressType.ECCCEngineLocation, OrganisationTypes.None);
				SetAddressPK(setValue, ecccPGAHeader.CA_OA_EvidenceOfConformityLocationInfo, organizationAddresContainer, Constants.AddressType.ECCCEvidenceOfConfirmityLocation, OrganisationTypes.None);
			}
		}
	}
}
