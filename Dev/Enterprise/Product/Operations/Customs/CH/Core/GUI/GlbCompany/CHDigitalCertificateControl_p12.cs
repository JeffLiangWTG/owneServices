using System;
using CargoWise.Types;

namespace Enterprise.Customs.CH.GUI;

public partial class CHDigitalCertificateControl_p12 : Registry.GUI.DigitalCertificateControl_p12
{
	public CHDigitalCertificateControl_p12()
	{
		InitializeComponent();
	}

	protected override ZDateTime GetCertificateAboutToExpireThreshold(ZDateTime timeToVerifyAgainst) => timeToVerifyAgainst.AddDays(30);

	protected override string GetCertificateAboutToExpireMessage(DateTime expiryDate) => Res.GetString("F6B50B72-42C3-43FF-93CD-FF6B952E1D04", "This certificate will shortly expire.");
}
