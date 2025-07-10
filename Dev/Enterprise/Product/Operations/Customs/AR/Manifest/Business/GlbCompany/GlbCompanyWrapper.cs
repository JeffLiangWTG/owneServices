using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Customs.AR;

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class GlbCompanyWrapper : MasterFiles.Business.GlbCompanyWrapper, IARGlbCompanyWrapper
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
					glbExternalPassword = GetGlbExternalPasswordOrCreateNew<GlbCompanyCredential>(PasswordTypesList.Codes.ARB);
					RegisterEditableChildObject(glbExternalPassword);
				}

				return glbExternalPassword;
			}
		}
		GlbCompanyCredential glbExternalPassword;

		#endregion

		IGlbExternalPassword IARGlbCompanyWrapper.GlbExternalPassword => GlbExternalPassword;

		public override bool IsValidWrapper => true;
	}
}
