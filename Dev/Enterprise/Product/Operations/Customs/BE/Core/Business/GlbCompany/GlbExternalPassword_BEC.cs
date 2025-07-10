using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.BE;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BE.Business;

public class GlbExternalPassword_BEC : GlbExternalPassword, IGlbExternalPassword_BEC, IxTMessageAttributeProvider
{
	public GlbExternalPassword_BEC(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();

		GP_PasswordType = PasswordTypesList.Codes.BEC;
	}

	public Dictionary<string, string> GetMessageAttrDictionary() => new Dictionary<string, string>
	{
		{ xTMessaging.Shared.Constants.xTMsgAttributes.Oauth2ClientID, GP_UserID },
		{ xTMessaging.Shared.Constants.xTMsgAttributes.Oauth2ClientSecret, CurrentDecryptedPassword },
	};

	#region Overrided properties

	[ResourceStringData("B1715DFC-9F5C-464A-90A7-8BBAB184C941", Caption = "ID")]
	public override ZString GP_UserID { get => base.GP_UserID; set => base.GP_UserID = value; }

	[ResourceStringData("6C12CAC7-5F06-473A-B599-4E692FCECD40", Caption = "Secret")]
	public override ZString CurrentDecryptedPassword { get => base.CurrentDecryptedPassword; set => base.CurrentDecryptedPassword = value; }

	#endregion
}
