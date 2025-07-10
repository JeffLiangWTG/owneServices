using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Dash.Integration.BusinessObjects;

namespace Enterprise.Dash.Business
{
	public class DashOrgCandidate : AutoDashOrgCandidate, IDashOrgCandidate
	{
		public DashOrgCandidate(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
