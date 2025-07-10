using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Client.EDI.MasterFiles
{
	public class BorderWiseOrgLicencesHelper : IOrgLicencesHelper
	{
		public IEnumerable<IOrgLicenceInfo> GetOrgLicences(ZGuid orgPk)
		{
			var org = new BusinessObjectFactory(Db.Connection) { RefreshEnabled = false }.Load<EDIOrgHeader>(orgPk);
			return GetOrgLicences(org);
		}

		public IEnumerable<IOrgLicenceInfo> GetOrgLicences(EDIOrgHeader org)
		{
			if (org != null && org.LicCompany != null)
			{
				var licenceDatabases = org.LicCompany.LicDatabases.OfType<LicenceDatabase>()
					.Where(ld => ld.LD_IsActive
						&& (ld.LD_Product.EqualsIgnoringCase(ProductTypes.Codes.BorderWise)
						|| ld.LD_Product.EqualsIgnoringCase(ProductTypes.Codes.CargoWiseOne)
						|| ld.LD_Product.EqualsIgnoringCase(ProductTypes.Codes.CargoWiseNext)
						|| ld.LD_Product.EqualsIgnoringCase(ProductTypes.Codes.CargoWise))).ToList();
				if (licenceDatabases.Count > 0)
				{
					var companyNumber = org.LicCompany.LC_CompanyNumber;
					return licenceDatabases.Select(ld => new OrgLicenceInfo { CompanyNumber = companyNumber, DatabaseNumber = ld.LD_DatabaseNumber, Product = ld.LD_Product, LicenseType = ld.LD_LicenceType });
				}
			}

			return Enumerable.Empty<IOrgLicenceInfo>();
		}

		public class OrgLicenceInfo : IOrgLicenceInfo
		{
			public int CompanyNumber { get; set; }
			public int DatabaseNumber { get; set; }
			public string Product { get; set; }
			public string LicenseType { get; set; }
		}
	}
}
