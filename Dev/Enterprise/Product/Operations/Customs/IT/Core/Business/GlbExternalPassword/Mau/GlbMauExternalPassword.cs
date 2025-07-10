using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IT.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.IT.Business;

public class GlbMauExternalPassword : GlbExternalPassword
	, IxTMessageAttributeProvider
	, IGlbMauExternalPassword
{
	public GlbMauExternalPassword(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : GlbExternalPasswordWithCertificate.Schema
	{
		public new const int GP_UserIDMaxLength = 20;
		public new const int GP_MailBoxIDMaxLength = 20;
	}

	[MaxLength(Schema.GP_UserIDMaxLength)]
	[ResourceStringData("Enterprise.Customs.IT.Business.GlbMauExternalPassword|GP_UserID", Caption = "Internal Code", ShortCaption = "Int. Code")]
	public override ZString GP_UserID { get => base.GP_UserID; set => base.GP_UserID = value; }

	[List(nameof(Lookups) + "." + nameof(GlbMauExternalPasswordLookups.Declarants))]
	[ResourceStringData("Enterprise.Customs.IT.Business.GlbMauExternalPassword|GP_Name", Caption = "Declarant")]
	public override ZString GP_Name { get => base.GP_Name; set => base.GP_Name = value; }

	[MaxLength(Schema.GP_MailBoxIDMaxLength)]
	[ResourceStringData("Enterprise.Customs.IT.Business.GlbMauExternalPassword|GP_MailBoxID", Caption = "Authorized User", ShortCaption = "Auth. User")]
	public override ZString GP_MailBoxID { get => base.GP_MailBoxID; set => base.GP_MailBoxID = value; }

	ZString IGlbMauExternalPassword.DeclarantTaxNumber => AccountHelper.SplitCodeBySeparator(GP_MailBoxID).DeclarantTaxNumber;

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		GP_GS = ZGuid.Empty;
	}

	public new GlbMauExternalPasswordLookups Lookups => (GlbMauExternalPasswordLookups)base.Lookups;
	protected override MasterFiles.Business.GlbExternalPasswordLookups GetNewLookups() => new GlbMauExternalPasswordLookups(this);

	protected override MasterFiles.Business.GlbExternalPasswordValidation GetNewValidation() => new GlbMauExternalPasswordValidation(this);

	protected override ZString PasswordType => PasswordTypesList.Codes.ITM;

	protected override bool ShouldSendCredential() => false;

	Dictionary<string, string> IxTMessageAttributeProvider.GetMessageAttrDictionary()
	{
		var t = (IxTMessageAttributeProvider)this;
		return t.GetXTMsgAttrProviderForGlbExternalPasswordSendingCertificatePEM();
	}
}
