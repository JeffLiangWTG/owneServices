using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.IT.NCTS.DataTransfer;

public class NctsHeaderDataObjectReader : EU.NCTS.DataTransfer.Phase4.NctsHeaderDataObjectReader, IOrganisationDataObjectReaderSupporter
{
	public NctsHeaderDataObjectReader(UniversalShipment headerDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
		: base(headerDataObject, logger, factory)
	{
	}

	protected override EU.NCTS.Business.NctsHeader GetNewBusinessObject()
	{
		return factory.New<NctsHeader>();
	}

	protected override void FillCountryData(EU.NCTS.Business.NctsHeader baseHeader)
	{
		base.FillCountryData(baseHeader);
		var header = (NctsHeader)baseHeader;
		if (dataObject.DeclarantType != null)
		{
			header.RepresentationType = dataObject.DeclarantType.GetCodeAsUpperCase();
		}
		var declarantAddressPk = GetAddressPK(header, AddressTypes.Declarant, OrganisationTypes.Consignee);
		if (declarantAddressPk.HasValue)
		{
			header.DeclarantAddressPK = declarantAddressPk.Value;
		}
	}

	ZGuid? GetAddressPK(NctsHeader header, ZString addressType, OrganisationTypes orgCategory, string orgType = null)
	{
		ZGuid? result = null;
		OrgAddress addressBO;
		if (this.TryGetMatchedOrganisation(out addressBO, dataObject, header, addressType, orgCategory, orgType) && addressBO != null)
		{
			var addressRow = (IColumnIndexer)((IBusinessObjectInternals)addressBO).Row;
			result = addressRow.GetValue(OrgAddressSchema.PK);
		}
		return result;
	}

	OrganisationDataObjectReader IOrganisationDataObjectReaderSupporter.CreateNewReader(OrganizationAddress addressData)
	{
		return new OrganisationDataObjectReader(addressData, logger, factory);
	}
}
