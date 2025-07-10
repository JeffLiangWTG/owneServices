using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.MasterFiles
{
	public class SupervisingOfficeCollection : OrganisationsFindBoxCollection
	{
		public SupervisingOfficeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property1", ZBool.False));
		}
	}
}
