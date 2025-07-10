using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	[ModuleID("LicenceEnterprise")]
	public class LicenceEnterpriseCollectionForEntCodeFilter : ActiveBusinessObjectCollection<LicEntWithEntCodeAsCodePropertyAttribute>
	{
		public LicenceEnterpriseCollectionForEntCodeFilter(BusinessObjectFactory factory) : base(factory)
		{
		}
	}

	[CodeProperty(LicenceEnterprise.Schema.LE_EnterpriseCode), DescriptionProperty(LicenceEnterprise.Schema.OrganisationName)]
	public class LicEntWithEntCodeAsCodePropertyAttribute : LicenceEnterprise
	{
		public LicEntWithEntCodeAsCodePropertyAttribute(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}


