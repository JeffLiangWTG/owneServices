using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business
{
	public class DrawbackImportLicenseCollection : CusSupportingInfoCollection<DrawbackImportLicense>
	{
		public DrawbackImportLicenseCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.Drawback)
		{
		}
	}
}

