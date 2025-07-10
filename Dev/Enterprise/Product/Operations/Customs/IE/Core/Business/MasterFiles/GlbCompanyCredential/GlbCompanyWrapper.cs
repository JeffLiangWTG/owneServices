using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Customs.IE;

namespace Enterprise.Customs.IE.Business
{
	public class GlbCompanyWrapper : Enterprise.MasterFiles.Business.GlbCompanyWrapper, IIEGlbCompanyWrapper
	{
		protected GlbCompanyWrapper(GlbCompany company)
			: base(company)
		{
		}

		public static GlbCompanyWrapper Get(GlbCompany company) => company?.Factory.GetCachedValue(company.PK.ToString() + "IEGlbCompanyWrapper", () => new GlbCompanyWrapper(company));

		public static ZString GetMessageSenderEORI(IMessageAttachee messageAttachee) => Get(messageAttachee?.Branch?.Company).GetIEGlbExternalPassword()?.GP_MailBoxID ?? ZString.Empty;

		#region GlbExternalPassword

		[ChildEditable]
		public GlbCompanyCredential GlbExternalPassword
		{
			get
			{
				if (glbExternalPassword == null)
				{
					glbExternalPassword = GetGlbExternalPasswordOrCreateNew<GlbCompanyCredential>(PasswordTypesList.Codes.IER);
					RegisterEditableChildObject(glbExternalPassword);
				}

				return glbExternalPassword;
			}
		}
		GlbCompanyCredential glbExternalPassword;

		#endregion

		#region EMCSGlbExternalPasswordCollection

		[ChildEditable]
		public EMCSGlbCompanyCredentialCollection EMCSGlbExternalPasswordCollection
		{
			get
			{
				if (emcsGlbExternalPasswordCollection == null)
				{
					emcsGlbExternalPasswordCollection = new EMCSGlbCompanyCredentialCollection(Company);
					emcsGlbExternalPasswordCollection.Load();
					RegisterEditableChildObject(emcsGlbExternalPasswordCollection);
				}

				return emcsGlbExternalPasswordCollection;
			}
		}
		EMCSGlbCompanyCredentialCollection emcsGlbExternalPasswordCollection;

		#endregion

		IGlbExternalPasswordWithCertificate IIEGlbCompanyWrapper.GetGlbExternalPasswordOrCreateNew() => GlbExternalPassword;
		IGlbExternalPasswordWithCertificate IIEGlbCompanyWrapper.GetGlbExternalPassword() => GetIEGlbExternalPassword();
		GlbCompanyCredential GetIEGlbExternalPassword() => GetGlbExternalPassword<GlbCompanyCredential>(PasswordTypesList.Codes.IER);

		IGlbExternalPasswordCollection_IEEMCS IIEGlbCompanyWrapper.GetEMCSGlbExternalPasswordCollectionOrCreateNew() => EMCSGlbExternalPasswordCollection;

		public override bool IsValidWrapper => true;
	}
}
