using System;

namespace Enterprise.Customs.CH.GUI.Testing;

class CHDigitalCertificateControl_p12Test : Registry.GUI.Testing.DigitalCertificateControl_p12Test
{
	public override void TestWarnings_WithDaysBeforeExpiryWarningMessage()
	{
		Assert("Expiry warning controlled via overriding GetCertificateAboutToExpireThreshold() rather than setting DaysBeforeExpiryWarningMessage", true);
	}

	protected override Registry.GUI.DigitalCertificateControl_p12 GetNewControl() => new CHDigitalCertificateControl_p12();

	protected override string ExpectedCertificateAboutToExpireMessage(DateTime expiryDate) => "This certificate will shortly expire.";
}
