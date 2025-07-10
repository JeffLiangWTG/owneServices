using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class EdiLicenceDatabaseOrgSuggestion : AutoEdiLicenceDatabaseOrgSuggestion
	{
		public EdiLicenceDatabaseOrgSuggestion(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new EDIOrgHeader Header => (EDIOrgHeader)base.Header;

		public ZString OrgName => Header?.MainAddress?.CompanyName ?? Header?.OH_FullName ?? ZString.Empty;
	}
}
