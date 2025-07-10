using System;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing.Freight.Consol
{
	[TestedType(typeof(BoleroEBLConfiguration))]
	sealed class BoleroEBLConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestGetGalileoEndPointUrlAndGalileoAudienceInTest()
		{
			var registrationMock = GetRegistrationMock(true);
			using (ObjectFactory.Substitute(registrationMock.Object))
			{
				var configuraiton = new BoleroEBLConfiguration();
				AssertEquals(BoleroEBLConfiguration.Constants.GalileoTestEndPointUrl, configuraiton.GetGalileoEndPointUrl());
				AssertEquals(BoleroEBLConfiguration.Constants.GalileoTestAudience, configuraiton.GetGalileoAudience());
			}
		}

		public void TestGetGalileoEndPointUrlAndGalileoAudienceInProduction()
		{
			var registrationMock = GetRegistrationMock(false);
			using (new DisposableAction(() => TestingState.IsRunningTests = false, () => TestingState.IsRunningTests = true))
			using (ObjectFactory.Substitute(registrationMock.Object))
			{
				var configuraiton = new BoleroEBLConfiguration();
				AssertEquals(BoleroEBLConfiguration.Constants.GalileoEndPointUrl, configuraiton.GetGalileoEndPointUrl());
				AssertEquals(BoleroEBLConfiguration.Constants.GalileoAudience, configuraiton.GetGalileoAudience());
			}
		}

		public void TestReadOnly()
		{
			using (EnvProxy.Instance.SetTemporaryUserContext(User.WebUserName, Guid.Empty, Guid.Empty))
			{
				var configuraiton = new BoleroEBLConfiguration();

				Assert(!configuraiton.EnableEBLIntegrationInfo.ReadOnly);
				Assert(configuraiton.GalileoEndPointUrlInfo.ReadOnly);
				Assert(configuraiton.GalileoAudienceInfo.ReadOnly);
				Assert(configuraiton.GalileoTestEndPointUrlInfo.ReadOnly);
				Assert(configuraiton.GalileoTestAudienceInfo.ReadOnly);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				var configuraiton = new BoleroEBLConfiguration();

				Assert(!configuraiton.EnableEBLIntegrationInfo.ReadOnly);
				Assert(!configuraiton.GalileoEndPointUrlInfo.ReadOnly);
				Assert(!configuraiton.GalileoAudienceInfo.ReadOnly);
				Assert(!configuraiton.GalileoTestEndPointUrlInfo.ReadOnly);
				Assert(!configuraiton.GalileoTestAudienceInfo.ReadOnly);
			}
		}
		
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new BoleroEBLConfiguration();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return new BoleroEBLConfiguration();
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
