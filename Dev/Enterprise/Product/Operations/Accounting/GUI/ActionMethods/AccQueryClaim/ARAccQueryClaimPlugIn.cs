using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GUI
{
	public class ARAccQueryClaimPlugIn : AccQueryClaimPlugIn
	{
		public ARAccQueryClaimPlugIn(IBusiness hostBusinessEntity)
			: base(hostBusinessEntity)
		{
		}

		#region Overrides

		public override string Name
		{
			get { return (NoResString)"Receivables Claims and Queries"; } // Hard-coded constant
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return QueryClaims;
		}

		#endregion

		OrgARQueryClaimDependentCollection fQueryClaims;
		public OrgARQueryClaimDependentCollection QueryClaims
		{
			get
			{
				if (fQueryClaims == null)
				{
					fQueryClaims = new OrgARQueryClaimDependentCollection(Organisation, fFactory);
					fQueryClaims.Load();
				}
				return fQueryClaims;
			}
		}
	}
}
