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
	public class AddInfoDataObjectReaderForDFO : AddInfoDataObjectReader<DFOPGAHeader>
	{
		public AddInfoDataObjectReaderForDFO(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper)
			: base(logger, helper, CusAddInfoSchema.B7_AddInfoData, DFOPGAHeaderAddInfoSchema.Instance)
		{
		}

		protected override void ReadOrganizationAddressCollection(Customs.Business.IAddInfoManager addInfoManager, IOrganizationAddressCollectionParent organizationAddresContainer, Action<ZString, ZString, string> setValue)
		{
			if (organizationAddresContainer.OrganizationAddressCollection != null)
			{
				var dfoPGAHeader = (DFOPGAHeader)addInfoManager;
				SetAddressPK(setValue, dfoPGAHeader.CA_OA_HarvestingPartyInfo, organizationAddresContainer, Constants.AddressType.HarvestingParty, OrganisationTypes.None);
				SetAddressPK(setValue, dfoPGAHeader.CA_OA_ProcessorInfo, organizationAddresContainer, Constants.AddressType.FoodProcessor, OrganisationTypes.None);
			}
		}
	}
}
