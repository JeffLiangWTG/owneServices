using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GUI
{
	public class APAccQueryClaimPlugIn : AccQueryClaimPlugIn
	{
		public APAccQueryClaimPlugIn(IBusiness hostBusinessEntity)
			: base(hostBusinessEntity)
		{
		}

		#region Overrides

		public override string Name
		{
			get { return (NoResString)"Payables Claims and Queries"; } // Hard-coded constant
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return QueryClaims;
		}

		#endregion

		OrgAPQueryClaimDependentCollection fQueryClaims;
		public OrgAPQueryClaimDependentCollection QueryClaims
		{
			get
			{
				if (fQueryClaims == null)
				{
					fQueryClaims = new OrgAPQueryClaimDependentCollection(Organisation, fFactory);
					fQueryClaims.Load();
				}
				return fQueryClaims;
			}
		}
	}
}
