using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Dash.Integration.BusinessObjects;

namespace Enterprise.Dash.Business
{
	public class DashMatchingConfig : AutoDashMatchingConfig, IDashMatchingConfig
	{
		public DashMatchingConfig(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
