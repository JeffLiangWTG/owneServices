using System.Collections.Generic;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Integration.Accounting.Test
{
	sealed class IEInvoicingCredentialSettingsExtensionsTest : TestCase
	{
		public void TestIsCertificate()
		{
			AssertEquals("Implementor of IEInvoicingCertificateCredentialSettings returns true", true, new CertificateSettingsForTest().IsCertificate());
			AssertEquals("Implementor of IEInvoicingPasswordCredentialSettings returns false", false, new PasswordSettingsForTest().IsCertificate());
			AssertEquals("Implementor of IEInvoicingCredentialSettings ONLY returns false", false, new NullSettingsForTest().IsCertificate());
			AssertEquals("Null returns false", false, ((IEInvoicingCredentialSettings)null).IsCertificate());
		}

		public void TestIsPassword()
		{
			AssertEquals("Implementor of IEInvoicingCertificateCredentialSettings returns false", false, new CertificateSettingsForTest().IsPassword());
			AssertEquals("Implementor of IEInvoicingPasswordCredentialSettings returns true", true, new PasswordSettingsForTest().IsPassword());
			AssertEquals("Implementor of IEInvoicingCredentialSettings ONLY returns false", false, new NullSettingsForTest().IsPassword());
			AssertEquals("Null returns false", false, ((IEInvoicingCredentialSettings)null).IsPassword());
		}

		public void TestIsNull()
		{
			AssertEquals("Implementor of IEInvoicingCertificateCredentialSettings returns false", false, new CertificateSettingsForTest().IsNoCredential());
			AssertEquals("Implementor of IEInvoicingPasswordCredentialSettings returns false", false, new PasswordSettingsForTest().IsNoCredential());
			AssertEquals("Implementor of IEInvoicingCredentialSettings ONLY returns true", true, new NullSettingsForTest().IsNoCredential());
			AssertEquals("Null returns true", true, ((IEInvoicingCredentialSettings)null).IsNoCredential());
		}

		#region Implementation

		class CertificateSettingsForTest : IEInvoicingCertificateCredentialSettings
		{
			public bool UserEnteredMailbox => throw new System.NotImplementedException();
			public int ExpiryWarningDays => throw new System.NotImplementedException();
			public ZString PasswordType => throw new System.NotImplementedException();
			public bool IsCompanyCredentialsRequired => throw new System.NotImplementedException();
			public bool IsBranchCredentialsRequired => throw new System.NotImplementedException();
			public string[] HiddenColumns => throw new System.NotImplementedException();
			public bool IsBranchRegistrationRequired => throw new System.NotImplementedException();
		}

		class PasswordSettingsForTest : IEInvoicingPasswordCredentialSettings
		{
			public ZString PasswordType => throw new System.NotImplementedException();
			public bool IsCompanyCredentialsRequired => throw new System.NotImplementedException();
			public bool IsBranchCredentialsRequired => throw new System.NotImplementedException();
			public bool IsBranchRegistrationRequired => throw new System.NotImplementedException();
			public IReadOnlyCollection<IEInvoicingPasswordCredentialDefinition> PasswordDefinitions => throw new System.NotImplementedException();
		}

		class NullSettingsForTest : IEInvoicingCredentialSettings
		{
			public ZString PasswordType => throw new System.NotImplementedException();
			public bool IsCompanyCredentialsRequired => throw new System.NotImplementedException();
			public bool IsBranchCredentialsRequired => throw new System.NotImplementedException();
			public bool IsBranchRegistrationRequired => throw new System.NotImplementedException();
		}

		#endregion
	}
}
