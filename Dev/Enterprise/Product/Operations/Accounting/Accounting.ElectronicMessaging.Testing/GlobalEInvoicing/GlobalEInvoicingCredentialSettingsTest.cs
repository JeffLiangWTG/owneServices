using CargoWise.Types;
using Enterprise.Integration.Accounting;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing
{
	abstract class GlobalEInvoicingCredentialSettingsTest : TransactionedTestCase
	{
		protected abstract IEInvoicingCredentialSettings GetBaseCredentialSettings();

		protected abstract ZString ExpectedPasswordType { get; }
		protected virtual bool ExpectedIsCompanyCredentialsRequired => false;
		protected virtual bool ExpectedIsBranchCredentialsRequired => false;
		protected virtual bool ExpectedIsBranchRegistrationRequired => false;

		public void TestExpectedPasswordType()
		{
			var actualValue = GetBaseCredentialSettings().PasswordType;
			AssertEquals(ExpectedPasswordType, actualValue);
		}

		public void TestExpectedIsCompanyCredentialsRequired()
		{
			var actualValue = GetBaseCredentialSettings().IsCompanyCredentialsRequired;
			AssertEquals(ExpectedIsCompanyCredentialsRequired, actualValue);
		}

		public void TestExpectedIsBranchCredentialsRequired()
		{
			var actualValue = GetBaseCredentialSettings().IsBranchCredentialsRequired;
			AssertEquals(ExpectedIsBranchCredentialsRequired, actualValue);
		}

		public void TestExpectedIsBranchReigstrationRequired()
		{
			var actualValue = GetBaseCredentialSettings().IsBranchRegistrationRequired;
			AssertEquals(ExpectedIsBranchRegistrationRequired, actualValue);
		}
	}
}
