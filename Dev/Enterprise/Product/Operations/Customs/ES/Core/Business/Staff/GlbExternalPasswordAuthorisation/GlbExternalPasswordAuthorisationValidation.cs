using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business
{
	public class GlbExternalPasswordAuthorisationValidation : Enterprise.MasterFiles.Business.GlbExternalPasswordAuthorisationValidation
	{
		public GlbExternalPasswordAuthorisationValidation(AutoGlbExternalPasswordAuthorisation parent) : base(parent)
		{
		}
		protected new GlbExternalPasswordAuthorisation Parent => (GlbExternalPasswordAuthorisation)base.Parent;

		protected override void CheckGEA_GS_AuthorisedStaff()
		{
			base.CheckGEA_GS_AuthorisedStaff();

			var authorisedStaffPk = Parent.GEA_GS_AuthorisedStaff;
			if (Parent.ExternalPassword?.Authorisations.Cast<GlbExternalPasswordAuthorisation>().Any(sub => sub.PK != Parent.PK && sub.GEA_GS_AuthorisedStaff == authorisedStaffPk) ?? false)
			{
				Parent.GEA_GS_AuthorisedStaffInfo.AddError(Res.GetString("998F2066-800D-4B06-9F46-4E32C846A913", "This user has already been registered, it must be unique."));
			}
		}
	}
}
