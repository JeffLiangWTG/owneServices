using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing
{
	[TestsSubclassesOf(typeof(GlobalEInvoicingCertificateCredentialSettings))]
	abstract class GlobalEInvoicingCertificateCredentialSettingsTest : GlobalEInvoicingCredentialSettingsTest
	{
		protected override ZString ExpectedPasswordType => PasswordTypesList.Codes.EIM;

		protected virtual int ExpectedExpiryWarningDays => 90;

		protected virtual string[] ExpectedHiddenColumns => Array.Empty<string>();

		protected abstract IEInvoicingCertificateCredentialSettings GetCredentialSettings();

		protected sealed override IEInvoicingCredentialSettings GetBaseCredentialSettings() => GetCredentialSettings();

		public void TestExpiryWarningDays()
		{
			var actualValue = GetCredentialSettings().ExpiryWarningDays;
			AssertEquals(ExpectedExpiryWarningDays, actualValue);
		}

		public void TestCredentialsGridListOfHiddenColumns()
		{
			var actualValue = GetCredentialSettings().HiddenColumns;
			Assert(!ExpectedHiddenColumns.Except(actualValue).Any());
		}
	}
}
