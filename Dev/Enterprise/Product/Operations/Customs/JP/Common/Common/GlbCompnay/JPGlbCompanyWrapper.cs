using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.JP;

namespace Enterprise.Customs.JP.Common
{
	public class JPGlbCompanyWrapper : GlbCompanyWrapper, IJPGlbCompanyWrapper
	{
		protected JPGlbCompanyWrapper(GlbCompany company) : base(company)
		{
		}

		[ChildEditable]
		public GlbExternalPasswordNMC MailboxCredential
		{
			get
			{
				if (mailboxCredential == null)
				{
					mailboxCredential = GetGlbExternalPasswordOrCreateNew<GlbExternalPasswordNMC>(JPPasswordType.Codes.NMC);
					RegisterEditableChildObject(mailboxCredential);
				}
				return mailboxCredential;
			}
		}
		GlbExternalPasswordNMC mailboxCredential;

		public override bool IsValidWrapper => true;
	}
}
