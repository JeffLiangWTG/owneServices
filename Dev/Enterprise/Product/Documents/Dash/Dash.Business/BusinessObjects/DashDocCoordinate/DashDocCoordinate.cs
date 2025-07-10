using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Dash.Integration.BusinessObjects;

namespace Enterprise.Dash.Business
{
	public class DashDocCoordinate : AutoDashDocCoordinate, IDashDocCoordinate
	{
		public DashDocCoordinate(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
