using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	/// <summary>
	/// Module Controller for AccQueryClaim.
	/// </summary>
	public abstract class AccQueryClaimController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new AccQueryClaimForm((AccQueryClaimBase)businessEntity);
		}
	}
}
