using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Dash.Integration.BusinessObjects;

namespace Enterprise.Dash.Business
{
	public class DashOrgCandidateAddress : AutoDashOrgCandidateAddress, IDashOrgCandidateAddress
	{
		public DashOrgCandidateAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
