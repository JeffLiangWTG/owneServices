using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Customs.MX;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class GlbCompanyWrapper : MasterFiles.Business.GlbCompanyWrapper, IMXGlbCompanyWrapper
	{
		protected GlbCompanyWrapper(GlbCompany company)
			: base(company)
		{
		}

		#region GlbExternalPassword

		[ChildEditable]
		public GlbCompanyCredential GlbExternalPassword
		{
			get
			{
				if (glbExternalPassword == null)
				{
					glbExternalPassword = GetGlbExternalPasswordOrCreateNew<GlbCompanyCredential>(PasswordTypesList.Codes.MXB);
					RegisterEditableChildObject(glbExternalPassword);
				}

				return glbExternalPassword;
			}
		}
		GlbCompanyCredential glbExternalPassword;

		#endregion

		IGlbExternalPassword IMXGlbCompanyWrapper.GlbExternalPassword => GlbExternalPassword;

		public override bool IsValidWrapper => true;
	}
}
