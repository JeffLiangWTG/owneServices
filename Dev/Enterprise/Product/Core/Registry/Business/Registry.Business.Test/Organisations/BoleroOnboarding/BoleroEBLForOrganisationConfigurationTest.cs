using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing.Organisations
{
	[TestedType(typeof(BoleroEBLForOrganisationConfiguration))]
	sealed class BoleroEBLForOrganisationConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestGetGalileoEndPointUrlAndGalileoAudienceInTest()
		{
			var registrationMock = GetRegistrationMock(true);
			using (ObjectFactory.Substitute(registrationMock.Object))
			{
				var configuraiton = new BoleroEBLForOrganisationConfiguration();
				AssertEquals(BoleroEBLForOrganisationConfiguration.Constants.GalileoTestEndPointUrl, configuraiton.GetGalileoEndPointUrl());
				AssertEquals(BoleroEBLForOrganisationConfiguration.Constants.GalileoTestAudience, configuraiton.GetGalileoAudience());
			}
		}

		public void TestGetGalileoEndPointUrlAndGalileoAudienceInProduction()
		{
			var registrationMock = GetRegistrationMock(false);
			using (new DisposableAction(() => TestingState.IsRunningTests = false, () => TestingState.IsRunningTests = true))
			using (ObjectFactory.Substitute(registrationMock.Object))
			{
				var configuraiton = new BoleroEBLForOrganisationConfiguration();
				AssertEquals(BoleroEBLForOrganisationConfiguration.Constants.GalileoEndPointUrl, configuraiton.GetGalileoEndPointUrl());
				AssertEquals(BoleroEBLForOrganisationConfiguration.Constants.GalileoAudience, configuraiton.GetGalileoAudience());
			}
		}

		public void TestGetTimeout()
		{
			var registrationMock = GetRegistrationMock(false);
			using (new DisposableAction(() => TestingState.IsRunningTests = false, () => TestingState.IsRunningTests = true))
			using (ObjectFactory.Substitute(registrationMock.Object))
			{
				var configuraiton = new BoleroEBLForOrganisationConfiguration();
				AssertEquals(BoleroEBLForOrganisationConfiguration.Constants.Timeout, configuraiton.GetTimeout());

				configuraiton = new BoleroEBLForOrganisationConfiguration
				{
					Timeout = 30
				};
				AssertEquals(30, configuraiton.GetTimeout());
			}
		}

		public void TestReadOnly()
		{
			using (EnvProxy.Instance.SetTemporaryUserContext(User.WebUserName, Guid.Empty, Guid.Empty))
			{
				var configuraiton = new BoleroEBLForOrganisationConfiguration();

				Assert(!configuraiton.EnableEBLIntegrationInfo.ReadOnly);
				Assert(configuraiton.GalileoEndPointUrlInfo.ReadOnly);
				Assert(configuraiton.GalileoAudienceInfo.ReadOnly);
				Assert(configuraiton.GalileoTestEndPointUrlInfo.ReadOnly);
				Assert(configuraiton.GalileoTestAudienceInfo.ReadOnly);
				Assert(configuraiton.TimeoutInfo.ReadOnly);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				var configuraiton = new BoleroEBLForOrganisationConfiguration();

				Assert(!configuraiton.EnableEBLIntegrationInfo.ReadOnly);
				Assert(!configuraiton.GalileoEndPointUrlInfo.ReadOnly);
				Assert(!configuraiton.GalileoAudienceInfo.ReadOnly);
				Assert(!configuraiton.GalileoTestEndPointUrlInfo.ReadOnly);
				Assert(!configuraiton.GalileoTestAudienceInfo.ReadOnly);
				Assert(!configuraiton.TimeoutInfo.ReadOnly);
			}
		}

		public void TestValidateTimeout()
		{
			var bob = new BoleroEBLForOrganisationConfiguration();
			bob.Timeout = 5;
			var error = bob.TimeoutInfo.GetErrors().GetFirstMessage();
			Assert("Timeout's value need in the range from 30 to 600", error == "Value must be greater than or equal to the minimum 30");
			bob.Timeout = 800;
			error = bob.TimeoutInfo.GetErrors().GetFirstMessage();
			Assert("Timeout's value need in the range from 30 to 600", error == "Value must be less than or equal to the maximum 600");
		}

		public void TestValidateUrl()
		{
			var bob = new BoleroEBLForOrganisationConfiguration();
			bob.GalileoEndPointUrl = "abc";
			var error = bob.GalileoEndPointUrlInfo.GetErrors().GetFirstMessage();
			Assert("URL need have correct format", error == "Please enter the correct URL.");
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new BoleroEBLForOrganisationConfiguration();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return new BoleroEBLForOrganisationConfiguration();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		Mock<IProductRegistration> GetRegistrationMock(bool isTest)
		{
			var registrationMock = new Mock<IProductRegistration>();
			registrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(isTest);
			if (isTest)
			{
				registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Test);
			}
			else
			{
				registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Production);
			}

			return registrationMock;
		}
	}
}
