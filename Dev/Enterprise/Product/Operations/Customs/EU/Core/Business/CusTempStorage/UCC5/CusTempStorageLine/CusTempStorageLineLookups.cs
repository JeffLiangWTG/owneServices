using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class CusTempStorageLineLookups : AutoCusTempStorageLineLookups
	{
		public CusTempStorageLineLookups(AutoCusTempStorageLine parent)
			: base(parent)
		{
		}

		public virtual OrganisationsFindBoxCollection OrganizationsFindBoxList => new OrganisationsFindBoxCollection(Factory);
	}
}
