using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class GlbCompanyCredential : GlbExternalPassword, IxTMessageAttributeProvider
	{
		public GlbCompanyCredential(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			GP_PasswordType = PasswordTypesList.Codes.MXB;
		}

		#region Overrided properties

		[MaxLength(13)]
		public override ZString GP_UserID { get => base.GP_UserID; set => base.GP_UserID = value; }

		[MaxLength(128)]
		public override ZString GP_CurrentPassword { get => base.GP_CurrentPassword; set => base.GP_CurrentPassword = value; }

		[ReadOnly(true)]
		public override ZString GP_PasswordStatus { get => base.GP_PasswordStatus; set => base.GP_PasswordStatus = value; }

		#endregion

		#region Password status

		public ZString PasswordStatus => Lookups.PasswordStatusList.GetDescriptionFromCode(base.GP_PasswordStatus) ?? base.GP_PasswordStatus;

		protected override void ChangeStatusWhenUpdateCurrentPassword(ZString oldValue)
		{
			if (oldValue != GP_CurrentPassword)
			{
				GP_PasswordStatus = GetCredentialStatus();
			}
		}

		#endregion

		protected override GlbExternalPasswordValidation GetNewValidation()
		{
			return new GlbCompanyCredentialValidation(this);
		}

		public new GlbCompanyCredentialValidation Validation => (GlbCompanyCredentialValidation)GetNewValidation();

		public Dictionary<string, string> GetMessageAttrDictionary()
		{
			var t = (IxTMessageAttributeProvider)this;
			return t.GetXTMsgAttrProviderForGlbExternalPasswordSendingHttpUserAndPassword(false);
		}

		protected override bool ShouldSendCredential() => false;
	}
}
