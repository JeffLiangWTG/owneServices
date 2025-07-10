using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Web.Modules
{
	public class WebRefServiceLevelFilterBusinessObject : AutoWebRefServiceLevelFilterBusinessObject
	{
		public WebRefServiceLevelFilterBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
