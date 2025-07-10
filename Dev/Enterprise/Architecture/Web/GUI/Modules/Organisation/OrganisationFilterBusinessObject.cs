using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Web.Modules
{
	public class OrganisationFilterBusinessObject : AutoOrganisationFilterBusinessObject
	{
		public OrganisationFilterBusinessObject(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
