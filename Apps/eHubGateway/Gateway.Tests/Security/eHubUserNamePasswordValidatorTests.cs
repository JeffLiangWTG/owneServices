using System;
using System.ServiceModel;
using CargoWise.eHub.Common;
using eServices.eHubDataAccess.Integration;
using CargoWise.eServices.Authentication.ServiceClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Gateway.Tests.Security
{
	[TestClass]
	public class eHubUserNamePasswordValidatorTests
	{
		[TestMethod]
		public void TestValidateCW1System()
		{
			var clientDetails = new ClientDetails("Full Client Name", Guid.Parse("2c5a7734-48e7-49a7-a5cb-76b19cdda67b"), "client@email");
			var partyAccessor1 = MockRepository.GenerateStub<IPartyAccessor>();
			partyAccessor1.Expect(_ => _.ClientExists("ENTXYZSRV")).Return(false);
			partyAccessor1.Expect(_ => _.GetEdiProdClientDetailsForSystem("ENTXYZSRV")).Return(clientDetails);
			partyAccessor1.Expect(_ => _.InsertClientEntry("ENTXYZSRV", "Full Client Name", "client@email", Guid.Parse("2c5a7734-48e7-49a7-a5cb-76b19cdda67b")));

            var authWSApi = MockRepository.GenerateStub<IAuthWebserviceApi>();
            authWSApi.Expect(_ => _.ValidateSystem("ENT", "SRV", "password")).Return(true);

			var validator1 = MockRepository.GeneratePartialMock<eHubUserNamePasswordValidator>();
			validator1.Expect(_ => _.PartyAccessor).Return(partyAccessor1).Repeat.Any();
            validator1.Expect(_ => _.AuthWSApi).Return(authWSApi).Repeat.Any();

			validator1.Validate("ENTXYZSRV", "password");

			var partyAccessor2 = MockRepository.GenerateStub<IPartyAccessor>();
			partyAccessor2.Expect(_ => _.ClientExists("ENT___SRV")).Return(true);
			var validator2 = MockRepository.GeneratePartialMock<eHubUserNamePasswordValidator>();
			validator2.Expect(_ => _.PartyAccessor).Return(partyAccessor2).Repeat.Any();
            validator2.Expect(_ => _.AuthWSApi).Return(authWSApi).Repeat.Any();

			validator2.Validate("ENT___SRV", "password");

            authWSApi.VerifyAllExpectations();
			partyAccessor1.VerifyAllExpectations();
			partyAccessor2.VerifyAllExpectations();
			validator1.VerifyAllExpectations();
			validator2.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestValidateCW1System_UserNameWithAuthWsExtensions_ClientIDDoesNotExists_BecauseItIsNotCW1License()
		{
			var partyAccessor1 = MockRepository.GenerateStub<IPartyAccessor>();
			partyAccessor1.Expect(_ => _.ClientExists("ENTXYZSRV_CSW")).Return(false);
			partyAccessor1.Expect(_ => _.GetEdiProdClientDetailsForSystem("ENTXYZSRV_CSW")).Return(null);
			
			var authWSApi = MockRepository.GenerateStub<IAuthWebserviceApi>();
			authWSApi.Expect(_ => _.ValidateSystem("ENT", "SRV", "password")).Return(true);

			var validator = MockRepository.GeneratePartialMock<eHubUserNamePasswordValidator>();
			validator.Expect(_ => _.PartyAccessor).Return(partyAccessor1).Repeat.Any();
			validator.Expect(_ => _.AuthWSApi).Return(authWSApi).Repeat.Any();

			try
			{
				validator.Validate("ENTXYZSRV_CSW", "password");
				Assert.Fail("FaultException must have been thrown.");
			}
			catch (FaultException e)
			{
				Assert.AreEqual("ClientID or Password invalid.", e.Message);
			}

			authWSApi.VerifyAllExpectations();
			partyAccessor1.VerifyAllExpectations();
			validator.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestValidateCW1System_UserNameWithAuthWsExtensions_CientIDExists()
		{
			var partyAccessor1 = MockRepository.GenerateStub<IPartyAccessor>();
			partyAccessor1.Expect(_ => _.ClientExists("ENTXYZSRV_CSW")).Return(true);

			var authWSApi = MockRepository.GenerateStub<IAuthWebserviceApi>();
			authWSApi.Expect(_ => _.ValidateSystem("ENT", "SRV", "password")).Return(true);

			var validator1 = MockRepository.GeneratePartialMock<eHubUserNamePasswordValidator>();
			validator1.Expect(_ => _.PartyAccessor).Return(partyAccessor1).Repeat.Any();
			validator1.Expect(_ => _.AuthWSApi).Return(authWSApi).Repeat.Any();

			validator1.Validate("ENTXYZSRV_CSW", "password");

			authWSApi.VerifyAllExpectations();
			partyAccessor1.VerifyAllExpectations();
			validator1.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestValidateCW1System_UserNameWithAuthWsExtensions_Branch()
		{
			var partyAccessor1 = MockRepository.GenerateStub<IPartyAccessor>();
			partyAccessor1.Expect(_ => _.ClientExists("ENTXYZSRVBRN_CSW")).Return(true);

			var authWSApi = MockRepository.GenerateStub<IAuthWebserviceApi>();
			authWSApi.Expect(_ => _.ValidateSystem("ENT", "SRV", "password")).Return(true);

			var validator1 = MockRepository.GeneratePartialMock<eHubUserNamePasswordValidator>();
			validator1.Expect(_ => _.PartyAccessor).Return(partyAccessor1).Repeat.Any();
			validator1.Expect(_ => _.AuthWSApi).Return(authWSApi).Repeat.Any();

			validator1.Validate("ENTXYZSRVBRN_CSW", "password");

			authWSApi.VerifyAllExpectations();
			partyAccessor1.VerifyAllExpectations();
			validator1.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestAuthWsExtensions_TCA_Exists()
		{
			var partyAccessor1 = MockRepository.GenerateStub<IPartyAccessor>();
			partyAccessor1.Expect(_ => _.ClientExists("WTLDTWJLI_TCA")).Return(true);

			var authWSApi = MockRepository.GenerateStub<IAuthWebserviceApi>();
			authWSApi.Expect(_ => _.ValidateSystem("WTL", "JLI", "password")).Return(true);

			var validator1 = MockRepository.GeneratePartialMock<eHubUserNamePasswordValidator>();
			validator1.Expect(_ => _.PartyAccessor).Return(partyAccessor1).Repeat.Any();
			validator1.Expect(_ => _.AuthWSApi).Return(authWSApi).Repeat.Any();

			validator1.Validate("WTLDTWJLI_TCA", "password");

			authWSApi.VerifyAllExpectations();
			partyAccessor1.VerifyAllExpectations();
			validator1.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestAuthWsExtensions_TCA_NotExists()
		{
			var partyAccessor1 = MockRepository.GenerateStub<IPartyAccessor>();
			partyAccessor1.Expect(_ => _.ClientExists("WTLDTWJLI_TCA")).Return(false);
			partyAccessor1.Expect(_ => _.GetEdiProdClientDetailsForSystem("WTLDTWJLI_TCA")).Return(null);

			var authWSApi = MockRepository.GenerateStub<IAuthWebserviceApi>();
			authWSApi.Expect(_ => _.ValidateSystem("WTL", "JLI", "password")).Return(true);

			var validator = MockRepository.GeneratePartialMock<eHubUserNamePasswordValidator>();
			validator.Expect(_ => _.PartyAccessor).Return(partyAccessor1).Repeat.Any();
			validator.Expect(_ => _.AuthWSApi).Return(authWSApi).Repeat.Any();

			try
			{
				validator.Validate("WTLDTWJLI_TCA", "password");
				Assert.Fail("FaultException must have been thrown.");
			}
			catch (FaultException e)
			{
				Assert.AreEqual("ClientID or Password invalid.", e.Message);
			}

			authWSApi.VerifyAllExpectations();
			partyAccessor1.VerifyAllExpectations();
			validator.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestAuthWsExtensions_TCA_InsertClientThirdParty()
		{
			var clientDetails = new ClientDetails("Full Client Name", Guid.Parse("2c5a7734-48e7-49a7-a5cb-76b19cdda67b"), "client@email");

			var partyAccessor = MockRepository.GenerateStub<IPartyAccessor>();
			partyAccessor.Expect(_ => _.ClientExists("WTLDTWJLI_TCA")).Return(false);
			partyAccessor.Expect(_ => _.GetEdiProdClientDetailsForSystem("WTLDTWJLI_TCA")).Return(clientDetails);


			var authWSApi = MockRepository.GenerateStub<IAuthWebserviceApi>();
			authWSApi.Expect(_ => _.ValidateSystem("WTL", "JLI", "password")).Return(true);

			var validator = MockRepository.GeneratePartialMock<eHubUserNamePasswordValidator>();
			validator.Expect(_ => _.PartyAccessor).Return(partyAccessor).Repeat.Any();
			validator.Expect(_ => _.AuthWSApi).Return(authWSApi).Repeat.Any();

			validator.Validate("WTLDTWJLI_TCA", "password");

			authWSApi.VerifyAllExpectations();
			partyAccessor.VerifyAllExpectations();
			partyAccessor.AssertWasCalled(x => x.InsertClientAndClientSystem("WTLDTWJLI_TCA", "Full Client Name", "client@email", Guid.Parse("2c5a7734-48e7-49a7-a5cb-76b19cdda67b"), "Third Party"));
			validator.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestAuthWsExtensions_InsertClientDefaultEnterprise()
		{
			var clientDetails = new ClientDetails("Full Client Name", Guid.Parse("2c5a7734-48e7-49a7-a5cb-76b19cdda67b"), "client@email");

			var partyAccessor = MockRepository.GenerateStub<IPartyAccessor>();
			partyAccessor.Expect(_ => _.ClientExists("WTLDTWJLI")).Return(false);
			partyAccessor.Expect(_ => _.GetEdiProdClientDetailsForSystem("WTLDTWJLI")).Return(clientDetails);


			var authWSApi = MockRepository.GenerateStub<IAuthWebserviceApi>();
			authWSApi.Expect(_ => _.ValidateSystem("WTL", "JLI", "password")).Return(true);

			var validator = MockRepository.GeneratePartialMock<eHubUserNamePasswordValidator>();
			validator.Expect(_ => _.PartyAccessor).Return(partyAccessor).Repeat.Any();
			validator.Expect(_ => _.AuthWSApi).Return(authWSApi).Repeat.Any();

			validator.Validate("WTLDTWJLI", "password");

			authWSApi.VerifyAllExpectations();
			partyAccessor.VerifyAllExpectations();
			partyAccessor.AssertWasCalled(x => x.InsertClientAndClientSystem("WTLDTWJLI", "Full Client Name", "client@email", Guid.Parse("2c5a7734-48e7-49a7-a5cb-76b19cdda67b"), "Enterprise"));
			validator.VerifyAllExpectations();
		}
		[TestMethod]
		public void TestValidateCW1System_UserNameWithAuthWsExtensions_NotMatch()
		{
			var partyAccessor = MockRepository.GenerateStub<IPartyAccessor>();
			partyAccessor.Expect(_ => _.IsUnrestrictedClient("ENTXYZSRV_ABC")).Return(false);

			var authWSApi = MockRepository.GenerateStub<IAuthWebserviceApi>();

			var securityAccessor = MockRepository.GenerateStub<ISecurityAccessor>();
			var licensePassword = "Expected empty but having license password";
			securityAccessor.Expect(_ => _.ValidatePassword("ENTXYZSRV_ABC", SHA512Encryptor.Encrypt("ENTXYZSRV_ABC" + licensePassword))).Return(false);

			var validator = MockRepository.GeneratePartialMock<eHubUserNamePasswordValidator>();
			validator.Expect(_ => _.AuthWSApi).Return(authWSApi).Repeat.Any();
			validator.Expect(_ => _.SecurityAccessor).Return(securityAccessor).Repeat.Any();
			validator.Expect(_ => _.PartyAccessor).Return(partyAccessor).Repeat.Any();
			try
			{
				validator.Validate("ENTXYZSRV_ABC", licensePassword);
				Assert.Fail("FaultException must have been thrown.");
			}
			catch (FaultException e)
			{
				Assert.AreEqual("ClientID or Password invalid.", e.Message);
			}

			validator.VerifyAllExpectations();
			authWSApi.VerifyAllExpectations();
			securityAccessor.VerifyAllExpectations();
		}

		[TestMethod]
        public void TestEdiProdDoesNotReturnAClient()
        {
            var clientDetails = new ClientDetails("Full Client Name", Guid.Parse("2c5a7734-48e7-49a7-a5cb-76b19cdda67b"), "client@email");
            
            var securityAccessor = MockRepository.GenerateStub<ISecurityAccessor>();

            var partyAccessor = MockRepository.GenerateStub<IPartyAccessor>();
            partyAccessor.Expect(_ => _.ClientExists("ENTXYZSRV")).Return(false);
            partyAccessor.Expect(_ => _.GetEdiProdClientDetailsForSystem("ENTXYZSRV")).Return(null);

            var authWSApi = MockRepository.GenerateStub<IAuthWebserviceApi>();
            authWSApi.Expect(_ => _.ValidateSystem("ENT", "SRV", "password")).Return(true);

            var validator = MockRepository.GeneratePartialMock<eHubUserNamePasswordValidator>();
            validator.Expect(_ => _.PartyAccessor).Return(partyAccessor).Repeat.Any();
            validator.Expect(_ => _.AuthWSApi).Return(authWSApi).Repeat.Any();
            validator.Expect(_ => _.SecurityAccessor).Return(securityAccessor).Repeat.Never();
			try
            {
                validator.Validate("ENTXYZSRV", "password");
                Assert.Fail("FaultException must have been thrown.");
            }
            catch (FaultException e)
            {
                Assert.AreEqual("ClientID or Password invalid.", e.Message);
            }
            authWSApi.VerifyAllExpectations();
            partyAccessor.VerifyAllExpectations();
            validator.VerifyAllExpectations();
        }

		[TestMethod]
		public void TestValidateEHubClient_DFDPRD()
		{
			var securityAccessor = MockRepository.GenerateStub<ISecurityAccessor>();
			securityAccessor.Expect(_ => _.ValidatePassword("DFD???PRD", SHA512Encryptor.Encrypt("DFD???PRD" + "password"))).Return(true);

			var validator = MockRepository.GeneratePartialMock<eHubUserNamePasswordValidator>();
			validator.Expect(_ => _.SecurityAccessor).Return(securityAccessor).Repeat.Any();

			validator.Validate("DFD???PRD", "password");

			validator.VerifyAllExpectations();
			securityAccessor.VerifyAllExpectations();
		}

		[TestMethod]
        public void TestAuthenticationApiThrowsExceptions()
        {
            var clientDetails = new ClientDetails("Full Client Name", Guid.Parse("2c5a7734-48e7-49a7-a5cb-76b19cdda67b"), "client@email");

			var securityAccessor = MockRepository.GenerateStub<ISecurityAccessor>();
            securityAccessor.Expect(_ => _.ValidatePassword("USC_SHCK_REPO", SHA512Encryptor.Encrypt("USC_SHCK_REPO" + "password"))).Return(true);

            var partyAccessor = MockRepository.GenerateStub<IPartyAccessor>();
			partyAccessor.Expect(_ => _.IsUnrestrictedClient("USC_SHCK_REPO")).Return(true);
			partyAccessor.Expect(_ => _.ClientExists("USC_SHCK_REPO")).Return(false);
            partyAccessor.Expect(_ => _.GetEdiProdClientDetailsForSystem("USC_SHCK_REPO")).Return(clientDetails);
            partyAccessor.Expect(_ => _.InsertClientEntry("USC_SHCK_REPO", "Full Client Name", "client@email", Guid.Parse("2c5a7734-48e7-49a7-a5cb-76b19cdda67b")));

            var validator = MockRepository.GeneratePartialMock<eHubUserNamePasswordValidator>();
            validator.Expect(_ => _.PartyAccessor).Return(partyAccessor).Repeat.Any();
            validator.Expect(_ => _.SecurityAccessor).Return(securityAccessor).Repeat.Any();
			validator.Validate("USC_SHCK_REPO", "password");

            partyAccessor.VerifyAllExpectations();
            validator.VerifyAllExpectations();
        }

		[TestMethod]
		public void TestValidateCW1SystemUnknownUserOrPassword()
		{
			var partyAccessor = MockRepository.GenerateStub<IPartyAccessor>();
			partyAccessor.Expect(_ => _.IsUnrestrictedClient("ENTXYZSRV")).Return(false);

			var authWSApi = MockRepository.GenerateStub<IAuthWebserviceApi>();
            authWSApi.Expect(_ => _.ValidateSystem("ENT", "SRV", "wrong password")).Return(false);

            var securityAccessor = MockRepository.GenerateStub<ISecurityAccessor>();
            securityAccessor.Expect(_ => _.ValidatePassword("ENTXYZSRV", SHA512Encryptor.Encrypt("ENTXYZSRV" + "wrong password"))).Return(false);

			var validator = MockRepository.GeneratePartialMock<eHubUserNamePasswordValidator>();
			validator.Expect(_ => _.AuthWSApi).Return(authWSApi).Repeat.Any();
            validator.Expect(_=>_.SecurityAccessor).Return(securityAccessor).Repeat.Any();
			validator.Expect(_ => _.PartyAccessor).Return(partyAccessor).Repeat.Any();
			try
			{
				validator.Validate("ENTXYZSRV", "wrong password");
				Assert.Fail("FaultException must have been thrown.");
			}
			catch (FaultException e)
			{
				Assert.AreEqual("ClientID or Password invalid.", e.Message);
			}

			validator.VerifyAllExpectations();
		}

        [TestMethod]
        public void TestInserteHubClientSystem()
        {
            var clientDetails1 = new ClientDetails("Full Client Name", Guid.Parse("2c5a7734-48e7-49a7-a5cb-76b19cdda67b"), "client@email");
            var partyAccessor1 = MockRepository.GenerateStub<IPartyAccessor>();
            partyAccessor1.Expect(_ => _.ClientExists("ENTXYZSRV")).Return(false);
            partyAccessor1.Expect(_ => _.GetEdiProdClientDetailsForSystem("ENTXYZSRV")).Return(clientDetails1);
            partyAccessor1.Expect(_ => _.InsertClientEntry("ENTXYZSRV", "Full Client Name", "client@email", Guid.Parse("2c5a7734-48e7-49a7-a5cb-76b19cdda67b")));
           
            var authWSApi = MockRepository.GenerateStub<IAuthWebserviceApi>();
            authWSApi.Expect(_ => _.ValidateSystem("ENT", "SRV", "password")).Return(true);

            var validator1 = MockRepository.GeneratePartialMock<eHubUserNamePasswordValidator>();
            validator1.Expect(_ => _.PartyAccessor).Return(partyAccessor1).Repeat.Any();
            validator1.Expect(_ => _.AuthWSApi).Return(authWSApi).Repeat.Any();
			validator1.Validate("ENTXYZSRV", "password");

            var clientDetails2 = new ClientDetails("Full Client Name 2", Guid.Parse("2c5a7734-48e7-49a7-a5cb-76b19cdda67b"), "client2@email");
            var partyAccessor2 = MockRepository.GenerateStub<IPartyAccessor>();
            partyAccessor2.Expect(_ => _.ClientExists("ENT___SRV")).Return(true);
            partyAccessor1.Expect(_ => _.GetEdiProdClientDetailsForSystem("ENT___SRV")).Return(clientDetails2);
            var validator2 = MockRepository.GeneratePartialMock<eHubUserNamePasswordValidator>();
            validator2.Expect(_ => _.PartyAccessor).Return(partyAccessor2).Repeat.Any();
            validator2.Expect(_ => _.AuthWSApi).Return(authWSApi).Repeat.Any();
			validator2.Validate("ENT___SRV", "password");

            authWSApi.VerifyAllExpectations();
            partyAccessor1.VerifyAllExpectations();
            partyAccessor2.VerifyAllExpectations();
            partyAccessor1.AssertWasCalled(x => x.InsertClientAndClientSystem("ENTXYZSRV", "Full Client Name", "client@email", Guid.Parse("2c5a7734-48e7-49a7-a5cb-76b19cdda67b")));
            partyAccessor2.AssertWasNotCalled(x => x.InsertClientAndClientSystem("ENT___SRV", "Full Client Name 2", "client2@email", Guid.Parse("2c5a7734-48e7-49a7-a5cb-76b19cdda67b")));
            validator1.VerifyAllExpectations();
            validator2.VerifyAllExpectations();
        }

		[TestMethod]
		public void TestDifferentValidationForDifferentSenderID()
		{
			var testCases = new[]
			{
				new { senderId = "AIR_BOOKING_ENGINE", isUnrestrictedClient = true, validateEHubClientCalled = true, validateCW1SystemCalled = false },
				new { senderId = "CONTAINER_TRACKING", isUnrestrictedClient = true, validateEHubClientCalled = true, validateCW1SystemCalled = false },
				new { senderId = "CUSTOMS_DATA_REPO", isUnrestrictedClient = true, validateEHubClientCalled = true, validateCW1SystemCalled = false },
				new { senderId = "EMISSION_CALCULATOR", isUnrestrictedClient = true, validateEHubClientCalled = true, validateCW1SystemCalled = false },
				new { senderId = "FLIGHT_MONITORING_SYSTEM", isUnrestrictedClient = true, validateEHubClientCalled = true, validateCW1SystemCalled = false },
				new { senderId = "GLOWMOB", isUnrestrictedClient = true, validateEHubClientCalled = true, validateCW1SystemCalled = false },
				new { senderId = "LINKTRACK", isUnrestrictedClient = true, validateEHubClientCalled = true, validateCW1SystemCalled = false },
				new { senderId = "SCHEDULE_FEED_SERVICE", isUnrestrictedClient = true, validateEHubClientCalled = true, validateCW1SystemCalled = false },
				new { senderId = "USC_SHCK_REPO", isUnrestrictedClient = true, validateEHubClientCalled = true, validateCW1SystemCalled = false },
				new { senderId = "T_____USC", isUnrestrictedClient = true, validateEHubClientCalled = true, validateCW1SystemCalled = false },
				new { senderId = "NOTINLIST_CSW", isUnrestrictedClient = false, validateEHubClientCalled = false, validateCW1SystemCalled = true }
			};

			foreach (var testCase in testCases)
			{
				var partyAccessor = MockRepository.GenerateStub<IPartyAccessor>();
				partyAccessor.Expect(_ => _.IsUnrestrictedClient(testCase.senderId)).Return(testCase.isUnrestrictedClient);
				var securityAccessor = MockRepository.GenerateStub<ISecurityAccessor>();
				var authWSApi = MockRepository.GenerateStub<IAuthWebserviceApi>();

				var validator = MockRepository.GeneratePartialMock<eHubUserNamePasswordValidator>();
				validator.Expect(_ => _.PartyAccessor).Return(partyAccessor).Repeat.Any();
				validator.Expect(_ => _.SecurityAccessor).Return(securityAccessor).Repeat.Any();
				validator.Expect(_ => _.AuthWSApi).Return(authWSApi).Repeat.Any();

				try
				{
					validator.Validate(testCase.senderId, "ENTXYZSRV_CSW");
				}
				catch (FaultException e)
				{
					Assert.AreEqual("ClientID or Password invalid.", e.Message);
				}

				AssertMethodCall(securityAccessor, _ => _.ValidatePassword(Arg<string>.Is.Anything, Arg<string>.Is.Anything), testCase.validateEHubClientCalled);
				AssertMethodCall(authWSApi, _ => _.ValidateSystem(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything), testCase.validateCW1SystemCalled);

				validator.VerifyAllExpectations();
				securityAccessor.VerifyAllExpectations();
				authWSApi.VerifyAllExpectations();
			}
		}

		private void AssertMethodCall<T>(T mock, Action<T> action, bool shouldBeCalled)
		{
			if (shouldBeCalled)
			{
				mock.AssertWasCalled(action);
			}
			else
			{
				mock.AssertWasNotCalled(action);
			}
		}
	}
}
