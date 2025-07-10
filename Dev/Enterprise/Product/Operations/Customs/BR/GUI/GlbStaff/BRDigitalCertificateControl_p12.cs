using System;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI
{
	public partial class BRDigitalCertificateControl_p12 : Enterprise.Registry.GUI.DigitalCertificateControl_p12
	{
		public BRDigitalCertificateControl_p12()
		{
			InitializeComponent();
		}

		protected override ZDateTime GetCertificateAboutToExpireThreshold(ZDateTime timeToVerifyAgainst) => timeToVerifyAgainst.AddMonths(1);

		protected override string GetCertificateAboutToExpireMessage(DateTime expiryDate) => Res.GetString("f2f773af-9298-4a76-be02-76d312134be3", "This certificate will expire soon - the expiry date is within one month.");
	}
}
