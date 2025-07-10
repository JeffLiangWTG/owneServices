using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Licensing;
using Enterprise.xTMessaging.Shared;
using Enterprise.xTMessaging.Shared.Test;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.xTMessaging.Business.Test
{
	class CW1RegistryConfigurationProviderTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();

			TestUtils.InsertRefSysConfig(Factory);
		}

		public void TestGetConfiguration_Test_Invalid()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "";
			registrationKey.ServerCodeForTest = "";
			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtTest.Code))
			{
				AssertNull(new CW1RegistryConfigurationProvider().GetConfiguration());
			}
		}

		public void TestGetConfiguration_Test_Valid()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "HYE";
			registrationKey.ServerCodeForTest = "CMT";
			registrationKey.PasswordForTest = "xyz123";

			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtTest.Code))
			{
				var config = new CW1RegistryConfigurationProvider().GetConfiguration();
				ConfigurationUtils.StandardizeConfig(config);
				CombineAssertions(() =>
				{
					AssertEquals("Connect", TestUtils.XTServerAddressValue, config.Connect);
					AssertEquals("CA", TestUtils.XTServerCertificateValue, config.CA);
					AssertEquals("Application.URI", "xt-application:HYECMT", config.Application.URI);
					AssertEquals("Application.Password", "CE-86-1F-7F-9B-56-66-F3-66-92-EA-EF-B6-1F-FC-AC-89-7A-C9-08-86-3B-27-C9-DE-C0-D0-95-21-02-EF-DC-4E-B0-DB-04-12-29-22-94-60-F0-3B-27-79-22-7F-8B-00-0F-A4-58-C6-7A-07-86-5F-58-F4-67-84-0A-38-16", config.Application.Password);
				});
			}
		}

		public void TestGetConfiguration_Production_Invalid()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
			registrationKey.EnterpriseCodeForTest = "";
			registrationKey.ServerCodeForTest = "";

			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtProduction.Code))
			{
				AssertNull(new CW1RegistryConfigurationProvider().GetConfiguration());
			}
		}

		public void TestGetConfiguration_Production_Valid()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
			registrationKey.EnterpriseCodeForTest = "HYE";
			registrationKey.ServerCodeForTest = "CMT";
			registrationKey.PasswordForTest = "xyz123";

			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtProduction.Code))
			{
				var config = new CW1RegistryConfigurationProvider().GetConfiguration();
				ConfigurationUtils.StandardizeConfig(config);
				CombineAssertions(() =>
				{
					AssertEquals("Connect", TestUtils.XTServerAddressValue, config.Connect);
					AssertEquals("CA", TestUtils.XTServerCertificateValue, config.CA);
					AssertEquals("Application.URI", "xt-application:HYECMT", config.Application.URI);
					AssertEquals("Application.Password", "CE-86-1F-7F-9B-56-66-F3-66-92-EA-EF-B6-1F-FC-AC-89-7A-C9-08-86-3B-27-C9-DE-C0-D0-95-21-02-EF-DC-4E-B0-DB-04-12-29-22-94-60-F0-3B-27-79-22-7F-8B-00-0F-A4-58-C6-7A-07-86-5F-58-F4-67-84-0A-38-16", config.Application.Password);
				});
			}
		}

		public void TestGetConfiguration_Local_Invalid()
		{
			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtLocal.Code))
			{
				AssertNull("Must configure certificate before Local XT can be used.", new CW1RegistryConfigurationProvider().GetConfiguration());

				using (DirectxTMessagingRegistry.Instance.XTLocalDeveloperCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "A fake certificate"))
				using (DirectxTMessagingRegistry.Instance.XTLocalDeveloperAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
				{
					AssertNull("Empty XT Address should be invalid", new CW1RegistryConfigurationProvider().GetConfiguration());
				}
			}
		}

		public void TestGetConfiguration_Local_ForDefaultAddress()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
			registrationKey.EnterpriseCodeForTest = "WTL";
			registrationKey.ServerCodeForTest = "MUG";
			registrationKey.PasswordForTest = "SuperSecretPassword";
			const string localDeveloperCertificate = @"-----BEGIN CERTIFICATE-----
dGhpcyBpcyBub3QgYSByZWFsIGNlcnRpZmljYXRl
-----END CERTIFICATE-----";

			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtLocal.Code))
			{
				using (DirectxTMessagingRegistry.Instance.XTLocalDeveloperCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, localDeveloperCertificate))
				{
					var config = new CW1RegistryConfigurationProvider().GetConfiguration();
					ConfigurationUtils.StandardizeConfig(config);
					CombineAssertions(() =>
					{
						AssertEquals("Connect", "127.0.0.1:61001", config.Connect);
						AssertEquals("CA", localDeveloperCertificate, config.CA);
						AssertEquals("Application.URI", "xt-application:WTLMUG", config.Application.URI);
						AssertEquals("Application.Password", "C4-3A-67-17-8F-C0-54-56-D0-DA-BE-A8-CE-F7-08-89-F2-4F-E3-03-D1-EB-59-36-ED-CF-B1-9B-10-86-53-E7-E7-25-B4-E8-3B-01-D9-70-A9-10-8B-AD-EF-F4-74-B7-1A-B1-79-3F-48-D2-7C-F1-3D-85-C2-2A-A4-20-83-4A", config.Application.Password);
					});
				}
			}
		}

		public void TestGetConfiguration_Local_ForDifferentAddress()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
			registrationKey.EnterpriseCodeForTest = "WTL";
			registrationKey.ServerCodeForTest = "ABC";
			registrationKey.PasswordForTest = "SomeOtherPassword";
			const string localDeveloperCertificate = @"-----BEGIN CERTIFICATE-----
dGhpcyBpcyBub3QgYSByZWFsIGNlcnRpZmljYXRl
-----END CERTIFICATE-----";

			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtLocal.Code))
			{
				using (DirectxTMessagingRegistry.Instance.XTLocalDeveloperAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "localhost:61008"))
				using (DirectxTMessagingRegistry.Instance.XTLocalDeveloperCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, localDeveloperCertificate))
				{
					var config = new CW1RegistryConfigurationProvider().GetConfiguration();
					ConfigurationUtils.StandardizeConfig(config);
					CombineAssertions(() =>
					{
						AssertEquals("Connect", "localhost:61008", config.Connect);
						AssertEquals("CA", localDeveloperCertificate, config.CA);
						AssertEquals("Application.URI", "xt-application:WTLABC", config.Application.URI);
						AssertEquals("Application.Password", "48-F1-9B-3B-FB-A5-0B-85-CC-F6-4E-50-9F-CB-FE-46-D7-13-4A-64-1C-FE-A8-0E-4A-3A-F2-2F-CB-E7-46-0A-16-B7-6C-CD-9C-A8-76-5E-34-47-F2-74-95-84-3A-3A-14-8C-EE-B4-E4-11-EB-E3-DE-77-7B-B4-96-43-2C-7C", config.Application.Password);
					});
				}
			}
		}
	}
}
