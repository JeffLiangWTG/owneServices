using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Dash.Integration.BusinessObjects;

namespace Enterprise.Dash.Business
{
	public class DashOrgCandidateName : AutoDashOrgCandidateName, IDashOrgCandidateName
	{
		public DashOrgCandidateName(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
