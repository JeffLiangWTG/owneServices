using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public partial class TokenCredentialsUserControl : ZUserControl
{
	public TokenCredentialsUserControl()
	{
		InitializeComponent();
		SetControlsCaptions();
	}

	void SetControlsCaptions()
	{
		var noTranslateFormat = Res.GetData("EB2A8D22-E931-42F3-A25E-ADB78248C0C4", "{0}");
		this.AccessTokenTextBox.CaptionResourceString = noTranslateFormat.Format((NoResString)"Access Token");
		this.CertificatePassPhraseTextBox.CaptionResourceString = noTranslateFormat.Format((NoResString)"Refresh Token");
		this.UserIdTextBox.CaptionResourceString = noTranslateFormat.Format((NoResString)"Customer Key");
		this.ClientSecretTextBox.CaptionResourceString = noTranslateFormat.Format((NoResString)"Customer Secret");
	}
}
