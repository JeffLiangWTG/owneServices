using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business;

public class SafeFoodLicenseCollection : CusCodeDataCollection<SafeFoodLicense>
{
	public SafeFoodLicenseCollection(BusinessObject master) : base(master, CusCodeDataTypeList.Codes.SafeFoodLicense)
	{
	}
}
