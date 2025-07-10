using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Customs.IL;

namespace Enterprise.Customs.IL.Business
{
	public class GlbCompanyWrapper : MasterFiles.Business.GlbCompanyWrapper, IILGlbCompanyWrapper
	{
		public GlbCompanyWrapper(GlbCompany company) : base(company)
		{
		}

		#region GlbExternalPassword

		[ChildEditable]
		public GlbILExternalPassword GlbExternalPassword
		{
			get
			{
				if (glbExternalPassword == null)
				{
					glbExternalPassword = GetGlbExternalPasswordOrCreateNew<GlbILExternalPassword>(PasswordTypesList.Codes.ILC);
					RegisterEditableChildObject(glbExternalPassword);
				}

				return glbExternalPassword;
			}
		}
		GlbILExternalPassword glbExternalPassword;

		#endregion

		public override bool IsValidWrapper => true;

		IGlbExternalPasswordWithCertificate IILGlbCompanyWrapper.GetGlbExternalPasswordOrCreateNew() => GlbExternalPassword;
	}
}
