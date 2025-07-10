using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business
{
	public class GlbExternalPasswordAuthorisationCollection
		: DependentBusinessObjectCollection<GlbExternalPasswordAuthorisation, GlbExternalPassword>
	{
		public GlbExternalPasswordAuthorisationCollection(GlbExternalPassword password) : base(password)
		{
		}

		protected override bool AllowNewCore => base.AllowNewCore && IsStaffCurrentUser;

		protected override bool AllowRemoveCore => base.AllowRemoveCore && (IsStaffCurrentUser || GlbStaff.CurrentUser.GS_IsController);

		bool IsStaffCurrentUser => Master.Staff?.IsCurrentUser ?? false;
	}
}
