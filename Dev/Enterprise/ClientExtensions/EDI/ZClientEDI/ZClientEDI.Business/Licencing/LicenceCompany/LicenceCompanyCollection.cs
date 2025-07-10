using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class LicenceCompanyCollection : BusinessObjectCollection<LicenceCompany>
	{
		public LicenceCompanyCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public LicenceCompanyCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}

