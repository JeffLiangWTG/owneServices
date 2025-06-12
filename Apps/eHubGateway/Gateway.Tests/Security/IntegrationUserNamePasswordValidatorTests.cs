using System;
using System.ServiceModel;
using CargoWise.eHub.Common;
using eServices.eHubDataAccess.Integration;
using NUnit.Framework;
using Rhino.Mocks;

namespace CargoWise.eHub.Gateway.Tests.Security
{
	[TestFixture]
	public class IntegrationUserNamePasswordValidatorTests
	{
		[Test]
        public void TestValidate_CallSecurityAccessorValidatePassword()
        {
            var securityAccessor = MockRepository.GenerateStub<ISecurityAccessor>();
            securityAccessor.Expect(_ => _.ValidatePassword("ENTXYZSRV", SHA512Encryptor.Encrypt("ENTXYZSRV" + "password"))).Return(true);

            var validator = MockRepository.GeneratePartialMock<Gateway.IntegrationUserNamePasswordValidator>();
            validator.Expect(_ => _.SecurityAccessor).Return(securityAccessor).Repeat.Any();
            validator.Validate("ENTXYZSRV", "password");

            securityAccessor.VerifyAllExpectations();
            validator.VerifyAllExpectations();
        }

        [Test]
        public void TestValidate_FailValidatePassword_ThrowFaultException()
        {
	        var securityAccessor = MockRepository.GenerateStub<ISecurityAccessor>();
	        securityAccessor.Expect(_ => _.ValidatePassword("ENTXYZSRV", SHA512Encryptor.Encrypt("ENTXYZSRV" + "wrong password"))).Return(false);

	        var validator = MockRepository.GeneratePartialMock<Gateway.IntegrationUserNamePasswordValidator>();
	        validator.Expect(_ => _.SecurityAccessor).Return(securityAccessor).Repeat.Any();

			var ex = Assert.Throws<FaultException>(() => validator.Validate("ENTXYZSRV", "wrong password"));
			Assert.That(ex.Message, Is.EqualTo("ClientID or Password invalid."));

            securityAccessor.VerifyAllExpectations();
	        validator.VerifyAllExpectations();
        }
	}
}