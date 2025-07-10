using System;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class BRDigitalCertificateControl_p12Test : Enterprise.Registry.GUI.Testing.DigitalCertificateControl_p12Test
	{
		public override void TestWarnings_WithDaysBeforeExpiryWarningMessage()
		{
			Assert("Expiry warning controlled via overriding GetCertificateAboutToExpireThreshold() rather than setting DaysBeforeExpiryWarningMessage", true);
		}

		protected override Enterprise.Registry.GUI.DigitalCertificateControl_p12 GetNewControl() => new BRDigitalCertificateControl_p12();

		protected override string ExpectedCertificateAboutToExpireMessage(DateTime expiryDate) => "This certificate will expire soon - the expiry date is within one month.";
	}
}
