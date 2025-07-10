using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.JP.Common.Testing
{
	sealed class DefaultBrokerAndCredentialValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateDefaultBrokerCode()
		{
			var targetInfo = defaultBrokerAndCredential.DefaultBrokerCodeInfo;
			var expectedMandatoryError = "Default Broker is required.";
			var expectedCredentialMessageError = "The Default Broker you have selected does not have a valid credential. To add a valid credential, press F3 to visit the Staff screen, navigate to the Credentials tab, and add a CUS – Customs code.";

			defaultBrokerAndCredential.Validation.ValidateDefaultBrokerCode();
			AssertHasErrorContaining(targetInfo, expectedMandatoryError);

			defaultBrokerAndCredential.DefaultBrokerCode = "AN";
			AssertNoErrorContaining(targetInfo, expectedMandatoryError);
			AssertNoMessageError(targetInfo, expectedCredentialMessageError);

			defaultBrokerAndCredential.DefaultBrokerCode = "KA";
			AssertHasMessageError(targetInfo, expectedCredentialMessageError);
		}

		public void TestValidateDefaultCredentialAIR()
		{
			var targetInfo = defaultBrokerAndCredential.DefaultCredentialAIRInfo;
			var expectedError = "Select a valid credential from the list.";

			defaultBrokerAndCredential.DefaultBrokerCode = "AN";
			defaultBrokerAndCredential.DefaultCredentialAIR = "Test1001";
			AssertHasErrorContaining(targetInfo, expectedError);

			defaultBrokerAndCredential.DefaultCredentialAIR = "Test2002";
			AssertNoErrorContaining(targetInfo, expectedError);
		}

		public void TestValidateDefaultCredentialSEA()
		{
			var targetInfo = defaultBrokerAndCredential.DefaultCredentialSEAInfo;
			var expectedError = "Select a valid credential from the list.";

			defaultBrokerAndCredential.DefaultBrokerCode = "AN";
			defaultBrokerAndCredential.DefaultCredentialSEA = "Test2002";
			AssertHasErrorContaining(targetInfo, expectedError);

			defaultBrokerAndCredential.DefaultCredentialSEA = "Test1001";
			AssertNoErrorContaining(targetInfo, expectedError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			new DefaultBrokerAndCredentialTestHelper().CreateBrokerStaff();
			defaultBrokerAndCredential = new DefaultBrokerAndCredential(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);
		}

		DefaultBrokerAndCredential defaultBrokerAndCredential;
	}
}
