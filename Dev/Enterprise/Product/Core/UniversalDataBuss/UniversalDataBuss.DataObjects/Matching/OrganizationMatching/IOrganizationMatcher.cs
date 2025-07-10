using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Management.Matching
{
	public interface IOrganizationAddressMatcher
	{
		IOrgHeader GetMatchingOrgHeader(OrganizationAddress organisationData, BusinessObjectFactory factory);
		ZString GetMatchedOrganisationCode(IOrganizationAddress addressData1, IXmlImportLogger logger);
	}
}
