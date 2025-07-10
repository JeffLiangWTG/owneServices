using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using CHPasswordStatusList = Enterprise.Customs.CH.Business.UniversalReferenceConstants.PasswordStatusList;

namespace Enterprise.Customs.CH.Business;

public class GlbCompanyTokenCredentials : GlbExternalPassword, IxTMessageAttributeProvider
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	const string GenericCredentialDescription = "Credential used by the background task that refreshes the Access Token";

	public GlbCompanyTokenCredentials(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	[MaxLength(8000)]
	[ResourceStringData("CH.Business.GlbCompanyTokenCredentials|GP_Certificate", Caption = "Access Token", FullDescription = "Token to access Federal Office for Customs and Border Security B2B services")]
	[ReadOnlyMember(nameof(IsSuspended))]
	public ZString GP_CertificateText
	{
		get => base.GP_Certificate.ToAscii();
		set => base.GP_Certificate = ZBlob.FromAscii(value);
	}

	[ResourceStringData("CH.Business.GlbCompanyTokenCredentials|GP_Certificate", Caption = "Access Token", FullDescription = "Token to access Federal Office for Customs and Border Security B2B services")]
	public override ZBlob GP_Certificate
	{
		get => base.GP_Certificate;
		set => base.GP_Certificate = value;
	}

	public ZPropertyInfo GP_CertificateTextInfo => GetWrappedZPropertyInfo(nameof(GP_CertificateText), x => GP_CertificateInfo);

	[ReadOnly(true)]
	[ResourceStringData("CH.Business.GlbCompanyTokenCredentials|GP_ExpiryDate", Caption = "Expires On (UTC)", FullDescription = "Expiry date and time of the Access Token (UTC)")]
	public override ZDateTime GP_ExpiryDate
	{
		get => base.GP_ExpiryDate;
		set => base.GP_ExpiryDate = value;
	}

	[MaxLength(256)]
	[ResourceStringData("CH.Business.GlbCompanyTokenCredentials|GP_UserID", Caption = "Client Key", FullDescription = GenericCredentialDescription)]
	[ReadOnlyMember(nameof(IsSuspended))]
	public override ZString GP_UserID
	{
		get => base.GP_UserID;
		set => base.GP_UserID = value;
	}

	[ResourceStringData("CH.Business.GlbCompanyTokenCredentials|CurrentDecryptedPassword", Caption = "Client Secret", FullDescription = GenericCredentialDescription)]
	[ReadOnlyMember(nameof(IsSuspended))]
	public override ZString CurrentDecryptedPassword
	{
		get => base.CurrentDecryptedPassword;
		set => base.CurrentDecryptedPassword = value;
	}

	[ResourceStringData("CH.Business.GlbCompanyTokenCredentials|RefreshTokenText", Caption = "Refresh Token", FullDescription = GenericCredentialDescription)]
	[ReadOnlyMember(nameof(IsSuspended))]
	public ZString RefreshTokenText
	{
		get => RefreshToken.GP_CertificateText;
		set => RefreshToken.GP_CertificateText = value;
	}

	public ZPropertyInfo RefreshTokenTextInfo => GetWrappedZPropertyInfo(nameof(RefreshTokenText), x => RefreshToken.GP_CertificateInfo);

	public GlbExternalPassword_CHR RefreshToken => refreshToken ??= LoadOrCreateRefreshToken();
	GlbExternalPassword_CHR refreshToken;

	GlbExternalPassword_CHR LoadOrCreateRefreshToken()
	{
		var refreshToken = LoadRefreshToken() ?? CreateRefreshToken();
		RegisterEditableChildObject(refreshToken);
		return refreshToken;
	}

	GlbExternalPassword_CHR LoadRefreshToken()
	{
		ZQuery zQuery = new ZQuery(GlbExternalPasswordSchema.GP_GC, GP_GC);
		zQuery.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.CHR);
		zQuery.OrderBy = AutoGlbExternalPassword.Schema.GP_SystemCreateTimeUtc;
		return Factory.LoadTop1<GlbExternalPassword_CHR>(zQuery);
	}

	GlbExternalPassword_CHR CreateRefreshToken()
	{
		var val = Factory.New<GlbExternalPassword_CHR>();
		val.GP_GC = GP_GC;
		return val;
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		GP_PasswordType = PasswordTypesList.Codes.CHT;
		LoadOrCreateRefreshToken();
	}

	protected override GlbExternalPasswordValidation GetNewValidation() => new GlbCompanyTokenCredentialsValidation(this);

	public override void OnSaving()
	{
		base.OnSaving();

		if (ShouldUpdateExpiryDateOnSaving)
		{
			UpdateGP_ExpiryDate();
		}
	}

	public bool ShouldUpdateExpiryDateOnSaving { get; set; } = true;

	void UpdateGP_ExpiryDate()
	{
		if (!IsInDatabase || (ZBlob)GP_CertificateInfo.OriginalValue != GP_Certificate)
		{
			GP_ExpiryDate = GP_Certificate.IsEmpty ? ZDateTime.Empty : ZDateTime.UtcNow.AddHours(1);
		}
	}

	public Dictionary<string, string> GetMessageAttrDictionary() => new Dictionary<string, string>
		{
			{ MessagingConstants.CustomMsgAttributes.AccessToken, GP_CertificateText },
		};

	public bool IsSuspended => GP_PasswordStatus == CHPasswordStatusList.Suspended;
}
