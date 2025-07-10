using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class GlbExternalPassword_NUT : GlbExternalPassword
	{
		public GlbExternalPassword_NUT(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_PasswordType = PasswordTypesList.Codes.NUT;
		}

		public override ZString ConfigurationName => NEXDOCSUserLevel;

		protected override ZString GetCredentialStatus()
		{
			return PasswordStatusList.Codes.Valid;
		}

		public ZBool HasValidCredential
		{
			get
			{
				var credentialStatus = GP_PasswordStatus;
				return credentialStatus == Core.Constants.PasswordOK || credentialStatus == PasswordStatusList.Codes.Valid;
			}
		}

		protected override object[] CreateCredentialItems()
		{
			var currentPassword = CredentialSender.CreateCredential(MasterFiles.Business.Customs.XmlCredential.Constants.CredentialDetails.Current, ZString.Empty, CurrentDecryptedPassword);
			return new object[] { currentPassword };
		}

		public const string NEXDOCSUserLevel = "NEXDOCSUserLevel";
	}
}
