using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Environment.Testing;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MailManager;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.ElectronicMessagingCertificateExpiryDateCheck.Testing
{
	class ElectronicMessagingCertificateExpiryDateCheckProcessorTest : TestCaseWithFactory
	{
		public void TestInjectionType()
		{
			AssertType(typeof(ElectronicMessagingCertificateExpiryDateCheckProcessor), ObjectFactory.Get<IElectronicMessagingCertificateExpiryDateCheckProcessor>());
		}

		public void TestAlertForAlmostExpiryCredentials()
		{
			var utcNow_20220710_083059 = new DateTime(2022, 07, 10, 08, 30, 59);

			var notificationGroupCompany = TestObjectCreator.CreateStaffGroup("GP1");
			var notificationGroupBranchPerth = TestObjectCreator.CreateStaffGroup("GP2");
			Factory.Save();

			AccountingConfigurationRegistry.Instance.EInvoicingCertificateExpiryNotificationGroup.SetValue(TestingCompanyAUSYD_Utc10.PK.ToGuid(), Guid.Empty, Guid.Empty
				, new EInvoicingCertificateExpiryNotificationGroup() { AlertDays = 10, NotificationGroup = notificationGroupCompany.PK });
			AccountingConfigurationRegistry.Instance.EInvoicingCertificateExpiryNotificationGroup.SetValue(Guid.Empty, TestingBranchPERTH_Utc8.PK.ToGuid(), Guid.Empty
				, new EInvoicingCertificateExpiryNotificationGroup() { AlertDays = 15, NotificationGroup = notificationGroupBranchPerth.PK });

			TestObjectCreator.CreateCompanyCertificate(TestingCompanyAUSYD_Utc10, "DummyIssuerTAU_1", "Dummy PAC1", new DateTime(2022, 07, 20, 18, 30, 59));
			TestObjectCreator.CreateCompanyCertificate(TestingCompanyAUSYD_Utc10, "DummyIssuerTAU_2", "Dummy PAC2", new DateTime(2022, 07, 20, 19, 30, 59));
			TestObjectCreator.CreateCompanyCertificate(TestingCompanyAUSYD_Utc10, "DummyIssuerTAU_3", "Dummy PAC3", new DateTime(2022, 07, 15, 19, 30, 59));
			TestObjectCreator.CreateCompanyCertificate(TestingCompanyAUSYD_Utc10, "DummyIssuerTAU_4", "Dummy PAC4", new DateTime(2022, 07, 09, 19, 30, 59));

			TestObjectCreator.CreateBranchCertificate(TestingBranchAUSYD_Utc10, "DummyIssuerAU1_1", "Dummy PAC1", new DateTime(2022, 07, 20, 18, 30, 59));
			TestObjectCreator.CreateBranchCertificate(TestingBranchAUSYD_Utc10, "DummyIssuerAU1_2", "Dummy PAC2", new DateTime(2022, 07, 20, 19, 30, 59));
			TestObjectCreator.CreateBranchCertificate(TestingBranchAUSYD_Utc10, "DummyIssuerAU1_3", "Dummy PAC3", new DateTime(2022, 07, 15, 19, 30, 59));
			TestObjectCreator.CreateBranchCertificate(TestingBranchAUSYD_Utc10, "DummyIssuerAU1_4", "Dummy PAC4", new DateTime(2022, 07, 09, 19, 30, 59));

			TestObjectCreator.CreateBranchCertificate(TestingBranchPERTH_Utc8, "DummyIssuerAU2_1", "Dummy PAC1", new DateTime(2022, 07, 25, 16, 30, 59));
			TestObjectCreator.CreateBranchCertificate(TestingBranchPERTH_Utc8, "DummyIssuerAU2_2", "Dummy PAC2", new DateTime(2022, 07, 25, 17, 30, 59));
			TestObjectCreator.CreateBranchCertificate(TestingBranchPERTH_Utc8, "DummyIssuerAU2_3", "Dummy PAC3", new DateTime(2022, 07, 15, 19, 30, 59));
			TestObjectCreator.CreateBranchCertificate(TestingBranchPERTH_Utc8, "DummyIssuerAU2_4", "Dummy PAC4", new DateTime(2022, 07, 09, 19, 30, 59));

			AssertAlertForAlmostExpiryCredentials(TestingCompanyAUSYD_Utc10, utcNow_20220710_083059
				, isBranchLevel: false, isCompanyLevel: false
				, Array.Empty<(string, Guid, Func<EmailDef, bool>)>()
				, Array.Empty<string>()
			);

			AssertAlertForAlmostExpiryCredentials(TestingCompanyAUSYD_Utc10, utcNow_20220710_083059
				, isBranchLevel: true, isCompanyLevel: false
				, new[] {
					("BranchAUSYD" , notificationGroupCompany.PK.ToGuid()
						, MatchMail("E-Reporting Certificate expiry notification[TAU - AU1]"
							, "Branch [AU1]Dummy Sydney Branch"
							, "1. Dummy PAC1, Issuer: DummyIssuerAU1_1, Expiry Date: 20-Jul-22 18:30"
							, "2. Dummy PAC3, Issuer: DummyIssuerAU1_3, Expiry Date: 15-Jul-22 19:30"))
					, ("BranchPERTH" , notificationGroupBranchPerth.PK.ToGuid()
						, MatchMail("E-Reporting Certificate expiry notification[TAU - AU2]"
							, "Branch [AU2]Dummy Perth Branch"
							, "1. Dummy PAC1, Issuer: DummyIssuerAU2_1, Expiry Date: 25-Jul-22 16:30"
							, "2. Dummy PAC3, Issuer: DummyIssuerAU2_3, Expiry Date: 15-Jul-22 19:30"))
				}
				, new[] {
					"Information|Running alerting for 'TAU - AU1' (2022-07-10 18:30:59), alerting days before expiry:10."
					, "Information|Find almost expiry credential(s) for branch."
					, "Information|Running alerting for 'TAU - AU2' (2022-07-10 16:30:59), alerting days before expiry:15."
					, "Information|Find almost expiry credential(s) for branch."
					, "Information|Running alerting for 'TAU - AU3' (2022-07-10 18:00:59), alerting days before expiry:10."
					, "Information|None of credential is almost expiry for branch."
				}
			);

			AssertAlertForAlmostExpiryCredentials(TestingCompanyAUSYD_Utc10, utcNow_20220710_083059
				, isBranchLevel: false, isCompanyLevel: true
				, new[] {
					("CompanyAUSYD" , notificationGroupCompany.PK.ToGuid()
						, MatchMail("E-Reporting Certificate expiry notification[TAU]"
							, "Company [TAU]Dummy AU Company"
							, "1. Dummy PAC1, Issuer: DummyIssuerTAU_1, Expiry Date: 20-Jul-22 18:30"
							, "2. Dummy PAC3, Issuer: DummyIssuerTAU_3, Expiry Date: 15-Jul-22 19:30"))
				}
				, new[] {
					"Information|Running alerting for 'TAU' (2022-07-10 18:30:59), alerting days before expiry:10."
					, "Information|Find almost expiry credential(s) for company."
				}
			);

			AssertAlertForAlmostExpiryCredentials(TestingCompanyAUSYD_Utc10, utcNow_20220710_083059
				, isBranchLevel: true, isCompanyLevel: true
				, new[] {
					("CompanyAUSYD" , notificationGroupCompany.PK.ToGuid()
						, MatchMail("E-Reporting Certificate expiry notification[TAU]"
							, "Company [TAU]Dummy AU Company"
							, "1. Dummy PAC1, Issuer: DummyIssuerTAU_1, Expiry Date: 20-Jul-22 18:30"
							, "2. Dummy PAC3, Issuer: DummyIssuerTAU_3, Expiry Date: 15-Jul-22 19:30"))
					, ("BranchAUSYD" , notificationGroupCompany.PK.ToGuid()
						, MatchMail("E-Reporting Certificate expiry notification[TAU - AU1]"
							, "Branch [AU1]Dummy Sydney Branch"
							, "1. Dummy PAC1, Issuer: DummyIssuerAU1_1, Expiry Date: 20-Jul-22 18:30"
							, "2. Dummy PAC3, Issuer: DummyIssuerAU1_3, Expiry Date: 15-Jul-22 19:30"))
					, ("BranchPERTH" , notificationGroupBranchPerth.PK.ToGuid()
						, MatchMail("E-Reporting Certificate expiry notification[TAU - AU2]"
							, "Branch [AU2]Dummy Perth Branch"
							, "1. Dummy PAC1, Issuer: DummyIssuerAU2_1, Expiry Date: 25-Jul-22 16:30"
							, "2. Dummy PAC3, Issuer: DummyIssuerAU2_3, Expiry Date: 15-Jul-22 19:30"))
				}
				, new[] {
					"Information|Running alerting for 'TAU' (2022-07-10 18:30:59), alerting days before expiry:10."
					, "Information|Find almost expiry credential(s) for company."
					, "Information|Running alerting for 'TAU - AU1' (2022-07-10 18:30:59), alerting days before expiry:10."
					, "Information|Find almost expiry credential(s) for branch."
					, "Information|Running alerting for 'TAU - AU2' (2022-07-10 16:30:59), alerting days before expiry:15."
					, "Information|Find almost expiry credential(s) for branch."
					, "Information|Running alerting for 'TAU - AU3' (2022-07-10 18:00:59), alerting days before expiry:10."
					, "Information|None of credential is almost expiry for branch."
				}
			);

			Func<EmailDef, bool> MatchMail(string subject, params string[] criticalBodyParts)
			{
				Func<EmailDef, bool> matchLogic = mail =>
				{
					return mail.Subject == subject
						&& criticalBodyParts.All(x => mail.Body.Contains(x));
				};
				return matchLogic;
			}
		}

		public void TestAlertForAlmostExpiryCredentials_EmailCreatorCalled()
		{
			var utcNow_20220710_083059 = new DateTime(2022, 07, 10, 08, 30, 59);

			var notificationGroupCompany = TestObjectCreator.CreateStaffGroup("GP1");
			var notificationGroupBranchPerth = TestObjectCreator.CreateStaffGroup("GP2");
			Factory.Save();

			AccountingConfigurationRegistry.Instance.EInvoicingCertificateExpiryNotificationGroup.SetValue(TestingCompanyAUSYD_Utc10.PK.ToGuid(), Guid.Empty, Guid.Empty
				, new EInvoicingCertificateExpiryNotificationGroup() { AlertDays = 10, NotificationGroup = notificationGroupCompany.PK });

			AccountingConfigurationRegistry.Instance.EInvoicingCertificateExpiryNotificationGroup.SetValue(Guid.Empty, TestingBranchPERTH_Utc8.PK.ToGuid(), Guid.Empty
				, new EInvoicingCertificateExpiryNotificationGroup() { AlertDays = 10, NotificationGroup = notificationGroupBranchPerth.PK });

			TestObjectCreator.CreateCompanyCertificate(TestingCompanyAUSYD_Utc10, "DummyIssuerTAU_1", "Dummy PAC1", new DateTime(2022, 07, 20, 18, 30, 59));
			TestObjectCreator.CreateBranchCertificate(TestingBranchPERTH_Utc8, "DummyIssuerAU2_1", "Dummy PAC1", new DateTime(2022, 07, 20, 16, 30, 59));

			var emailCreatorMock = new Mock<IElectronicMessagingNotificationEmailCreator>();
			emailCreatorMock
				.Setup(x => x.Create(It.IsAny<GlbBranch>(), It.IsAny<GlbCompany>(), It.IsAny<Guid>(), It.IsAny<IEnumerable<EInvoicingCertificateCredential>>()))
				.Returns((ElectronicMessagingCertificateExpiryDateCheckEmail)null);

			// Act
			var mockCredentialSettings = GetMockCredentialSettings(isBranchLevel: true, isCompanyLevel: false);
			new ElectronicMessagingCertificateExpiryDateCheckProcessor().AlertForAlmostExpiryCredentials(TestingCompanyAUSYD_Utc10, mockCredentialSettings, utcNow_20220710_083059, MockLogger.Object, emailCreatorMock.Object, null);

			// Assert
			emailCreatorMock.Verify(
				x => x.Create(It.IsAny<GlbBranch>(), It.IsAny<GlbCompany>(), It.IsAny<Guid>(), It.IsAny<IEnumerable<EInvoicingCertificateCredential>>()),
				Times.Exactly(1));
			emailCreatorMock.Invocations.Clear();

			// Act
			mockCredentialSettings = GetMockCredentialSettings(isBranchLevel: false, isCompanyLevel: true);
			new ElectronicMessagingCertificateExpiryDateCheckProcessor().AlertForAlmostExpiryCredentials(TestingCompanyAUSYD_Utc10, mockCredentialSettings, utcNow_20220710_083059, MockLogger.Object, emailCreatorMock.Object, null);

			// Assert
			emailCreatorMock.Verify(
				x => x.Create(It.IsAny<GlbBranch>(), It.IsAny<GlbCompany>(), It.IsAny<Guid>(), It.IsAny<IEnumerable<EInvoicingCertificateCredential>>()),
				Times.Exactly(1));
		}

		public void TestAlertForAlmostExpiryCredentials_NotificationQueryProviderCalled()
		{
			var utcNow_20220710_083059 = new DateTime(2022, 07, 10, 08, 30, 59);

			var notificationGroupCompany = TestObjectCreator.CreateStaffGroup("GP1");
			var notificationGroupBranchPerth = TestObjectCreator.CreateStaffGroup("GP2");
			Factory.Save();

			AccountingConfigurationRegistry.Instance.EInvoicingCertificateExpiryNotificationGroup.SetValue(TestingCompanyAUSYD_Utc10.PK.ToGuid(), Guid.Empty, Guid.Empty
				, new EInvoicingCertificateExpiryNotificationGroup() { AlertDays = 10, NotificationGroup = notificationGroupCompany.PK });

			AccountingConfigurationRegistry.Instance.EInvoicingCertificateExpiryNotificationGroup.SetValue(Guid.Empty, TestingBranchPERTH_Utc8.PK.ToGuid(), Guid.Empty
				, new EInvoicingCertificateExpiryNotificationGroup() { AlertDays = 10, NotificationGroup = notificationGroupBranchPerth.PK });

			TestObjectCreator.CreateCompanyCertificate(TestingCompanyAUSYD_Utc10, "DummyIssuerTAU_1", "Dummy PAC1", new DateTime(2022, 07, 20, 18, 30, 59));
			TestObjectCreator.CreateBranchCertificate(TestingBranchPERTH_Utc8, "DummyIssuerAU2_1", "Dummy PAC1", new DateTime(2022, 07, 20, 16, 30, 59));

			var notificationQueryProviderMock = new Mock<IElectronicMessagingNotificationQueryProvider>();
			notificationQueryProviderMock
				.Setup(x => x.GetQueryForCompany(It.IsAny<GlbCompany>(), It.IsAny<DateTime>(), It.IsAny<int>()))
				.Returns((ZQuery)null);

			var emailCreatorMock = new Mock<IElectronicMessagingNotificationEmailCreator>();
			emailCreatorMock
				.Setup(x => x.Create(It.IsAny<GlbBranch>(), It.IsAny<GlbCompany>(), It.IsAny<Guid>(), It.IsAny<IEnumerable<EInvoicingCertificateCredential>>()))
				.Returns((ElectronicMessagingCertificateExpiryDateCheckEmail)null);

			// Act
			var mockCredentialSettings = GetMockCredentialSettings(isBranchLevel: true, isCompanyLevel: false);
			new ElectronicMessagingCertificateExpiryDateCheckProcessor().AlertForAlmostExpiryCredentials(TestingCompanyAUSYD_Utc10, mockCredentialSettings, utcNow_20220710_083059, MockLogger.Object, null, notificationQueryProviderMock.Object);

			// Assert
			notificationQueryProviderMock.Verify(
				x => x.GetQueryForBranch(It.IsAny<GlbCompany>()),
				Times.Exactly(1));
			notificationQueryProviderMock.Invocations.Clear();

			// Act
			mockCredentialSettings = GetMockCredentialSettings(isBranchLevel: false, isCompanyLevel: true);
			new ElectronicMessagingCertificateExpiryDateCheckProcessor().AlertForAlmostExpiryCredentials(TestingCompanyAUSYD_Utc10, mockCredentialSettings, utcNow_20220710_083059, MockLogger.Object, null, notificationQueryProviderMock.Object);

			// Assert
			notificationQueryProviderMock.Verify(
				x => x.GetQueryForCompany(It.IsAny<GlbCompany>(), It.IsAny<DateTime>(), It.IsAny<int>()),
				Times.Exactly(1));
		}

		public void TestAlertForAlmostExpiryCredentials_AlertDaysZero()
		{
			var utcNow_202201730_083059 = new DateTime(2022, 07, 30, 08, 30, 59);

			TestObjectCreator.CreateCompanyCertificate(TestingCompanyAUSYD_Utc10, "DummyIssuer", "Dummy PAC", utcNow_202201730_083059.AddDays(1));
			TestObjectCreator.CreateBranchCertificate(TestingBranchPERTH_Utc8, "DummyIssuer2", "Dummy PAC2", utcNow_202201730_083059.AddDays(1));
			Factory.Save();

			var notificationGroupCompany = TestObjectCreator.CreateStaffGroup("GP1");
			var notificationGroupBranchPerth = TestObjectCreator.CreateStaffGroup("GP2");
			Factory.Save();

			var query = new ZQuery(GlbExternalPasswordSchema.GP_GC, TestingCompanyAUSYD_Utc10.PK);
			query.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.EIM);
			Assert("PreCondition", Factory.Exists(typeof(GlbExternalPassword), query));

			AccountingConfigurationRegistry.Instance.EInvoicingCertificateExpiryNotificationGroup.SetValue(TestingCompanyAUSYD_Utc10.PK.ToGuid(), Guid.Empty, Guid.Empty
				, new EInvoicingCertificateExpiryNotificationGroup() { AlertDays = 0, NotificationGroup = notificationGroupCompany.PK });
			AccountingConfigurationRegistry.Instance.EInvoicingCertificateExpiryNotificationGroup.SetValue(Guid.Empty, TestingBranchPERTH_Utc8.PK.ToGuid(), Guid.Empty
				, new EInvoicingCertificateExpiryNotificationGroup() { AlertDays = 0, NotificationGroup = notificationGroupBranchPerth.PK });

			AssertAlertForAlmostExpiryCredentials(TestingCompanyAUSYD_Utc10, utcNow_202201730_083059
				, isBranchLevel: false, isCompanyLevel: false
				, expectedSubjectRecipients: Array.Empty<(string, Guid, Func<EmailDef, bool>)>()
				, expectedLogs: Array.Empty<string>()
			);

			AssertAlertForAlmostExpiryCredentials(TestingCompanyAUSYD_Utc10, utcNow_202201730_083059
				, isBranchLevel: true, isCompanyLevel: false
				, expectedSubjectRecipients: Array.Empty<(string, Guid, Func<EmailDef, bool>)>()
				, expectedLogs: Array.Empty<string>()
			);

			AssertAlertForAlmostExpiryCredentials(TestingCompanyAUSYD_Utc10, utcNow_202201730_083059
				, isBranchLevel: true, isCompanyLevel: true
				, expectedSubjectRecipients: Array.Empty<(string, Guid, Func<EmailDef, bool>)>()
				, expectedLogs: Array.Empty<string>()
			);

			AssertAlertForAlmostExpiryCredentials(TestingCompanyAUSYD_Utc10, utcNow_202201730_083059
				, isBranchLevel: true, isCompanyLevel: true
				, expectedSubjectRecipients: Array.Empty<(string, Guid, Func<EmailDef, bool>)>()
				, expectedLogs: Array.Empty<string>()
			);
		}

		public void TestAlertForAlmostExpiryCredentials_AllCredentialsAreExpired()
		{
			var utcNow_202201730_083059 = new DateTime(2022, 07, 30, 08, 30, 59);

			TestObjectCreator.CreateCompanyCertificate(TestingCompanyAUSYD_Utc10, "DummyIssuer", "Dummy PAC", utcNow_202201730_083059.AddDays(-1));
			TestObjectCreator.CreateBranchCertificate(TestingBranchPERTH_Utc8, "DummyIssuer2", "Dummy PAC2", utcNow_202201730_083059.AddDays(-1));
			Factory.Save();

			var notificationGroupCompany = TestObjectCreator.CreateStaffGroup("GP1");
			var notificationGroupBranchPerth = TestObjectCreator.CreateStaffGroup("GP2");
			var notificationGroupBranchSydney = TestObjectCreator.CreateStaffGroup("GP3");
			Factory.Save();

			var query = new ZQuery(GlbExternalPasswordSchema.GP_GC, TestingCompanyAUSYD_Utc10.PK);
			query.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.EIM);
			Assert("PreCondition", Factory.Exists(typeof(GlbExternalPassword), query));

			AccountingConfigurationRegistry.Instance.EInvoicingCertificateExpiryNotificationGroup.SetValue(TestingCompanyAUSYD_Utc10.PK.ToGuid(), Guid.Empty, Guid.Empty
				, new EInvoicingCertificateExpiryNotificationGroup() { AlertDays = 1, NotificationGroup = notificationGroupCompany.PK });
			AccountingConfigurationRegistry.Instance.EInvoicingCertificateExpiryNotificationGroup.SetValue(Guid.Empty, TestingBranchPERTH_Utc8.PK.ToGuid(), Guid.Empty
				, new EInvoicingCertificateExpiryNotificationGroup() { AlertDays = 1, NotificationGroup = notificationGroupBranchPerth.PK });
			AccountingConfigurationRegistry.Instance.EInvoicingCertificateExpiryNotificationGroup.SetValue(Guid.Empty, TestingBranchAUSYD_Utc10.PK.ToGuid(), Guid.Empty
				, new EInvoicingCertificateExpiryNotificationGroup() { AlertDays = 1, NotificationGroup = notificationGroupBranchSydney.PK });

			AssertAlertForAlmostExpiryCredentials(TestingCompanyAUSYD_Utc10, utcNow_202201730_083059
				, isBranchLevel: false, isCompanyLevel: false
				, expectedSubjectRecipients: Array.Empty<(string, Guid, Func<EmailDef, bool>)>()
				, expectedLogs: Array.Empty<string>()
			);

			AssertAlertForAlmostExpiryCredentials(TestingCompanyAUSYD_Utc10, utcNow_202201730_083059
				, isBranchLevel: true, isCompanyLevel: false
				, expectedSubjectRecipients: Array.Empty<(string, Guid, Func<EmailDef, bool>)>()
				, expectedLogs: new[] {
					"Information|Running alerting for 'TAU - AU1' (2022-07-30 18:30:59), alerting days before expiry:1."
					, "Information|None of credential is almost expiry for branch."
					, "Information|Running alerting for 'TAU - AU2' (2022-07-30 16:30:59), alerting days before expiry:1."
					, "Information|None of credential is almost expiry for branch."
					, "Information|Running alerting for 'TAU - AU3' (2022-07-30 18:00:59), alerting days before expiry:1."
					, "Information|None of credential is almost expiry for branch."
				}
			);

			AssertAlertForAlmostExpiryCredentials(TestingCompanyAUSYD_Utc10, utcNow_202201730_083059
				, isBranchLevel: false, isCompanyLevel: true
				, expectedSubjectRecipients: Array.Empty<(string, Guid, Func<EmailDef, bool>)>()
				, expectedLogs: new[] {
					"Information|Running alerting for 'TAU' (2022-07-30 18:30:59), alerting days before expiry:1."
					, "Information|None of credential is almost expiry for company."
				}
			);

			AssertAlertForAlmostExpiryCredentials(TestingCompanyAUSYD_Utc10, utcNow_202201730_083059
				, isBranchLevel: true, isCompanyLevel: true
				, expectedSubjectRecipients: Array.Empty<(string, Guid, Func<EmailDef, bool>)>()
				, expectedLogs: new[] {
					"Information|Running alerting for 'TAU' (2022-07-30 18:30:59), alerting days before expiry:1."
					, "Information|None of credential is almost expiry for company."
					, "Information|Running alerting for 'TAU - AU1' (2022-07-30 18:30:59), alerting days before expiry:1."
					, "Information|None of credential is almost expiry for branch."
					, "Information|Running alerting for 'TAU - AU2' (2022-07-30 16:30:59), alerting days before expiry:1."
					, "Information|None of credential is almost expiry for branch."
					, "Information|Running alerting for 'TAU - AU3' (2022-07-30 18:00:59), alerting days before expiry:1."
					, "Information|None of credential is almost expiry for branch."
				}
			);
		}

		public void TestAlertForAlmostExpiryCredentials_NoCredentials()
		{
			var utcNow_202201730_083059 = new DateTime(2022, 07, 30, 08, 30, 59);

			var notificationGroupCompany = TestObjectCreator.CreateStaffGroup("GP1");
			var notificationGroupBranchPerth = TestObjectCreator.CreateStaffGroup("GP2");
			var notificationGroupBranchSydney = TestObjectCreator.CreateStaffGroup("GP3");
			Factory.Save();

			AccountingConfigurationRegistry.Instance.EInvoicingCertificateExpiryNotificationGroup.SetValue(TestingCompanyAUSYD_Utc10.PK.ToGuid(), Guid.Empty, Guid.Empty
				, new EInvoicingCertificateExpiryNotificationGroup() { AlertDays = 25, NotificationGroup = notificationGroupCompany.PK });
			AccountingConfigurationRegistry.Instance.EInvoicingCertificateExpiryNotificationGroup.SetValue(Guid.Empty, TestingBranchPERTH_Utc8.PK.ToGuid(), Guid.Empty
				, new EInvoicingCertificateExpiryNotificationGroup() { AlertDays = 25, NotificationGroup = notificationGroupBranchPerth.PK });
			AccountingConfigurationRegistry.Instance.EInvoicingCertificateExpiryNotificationGroup.SetValue(Guid.Empty, TestingBranchAUSYD_Utc10.PK.ToGuid(), Guid.Empty
				, new EInvoicingCertificateExpiryNotificationGroup() { AlertDays = 25, NotificationGroup = notificationGroupBranchSydney.PK });

			var query = new ZQuery(GlbExternalPasswordSchema.GP_GC, TestingCompanyAUSYD_Utc10.PK);
			query.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.EIM);
			AssertEquals("PreCondition", false, Factory.Exists(typeof(GlbExternalPassword), query));

			AssertAlertForAlmostExpiryCredentials(TestingCompanyAUSYD_Utc10, utcNow_202201730_083059
				, isBranchLevel: false, isCompanyLevel: false
				, expectedSubjectRecipients: Array.Empty<(string, Guid, Func<EmailDef, bool>)>()
				, expectedLogs: Array.Empty<string>()
			);

			AssertAlertForAlmostExpiryCredentials(TestingCompanyAUSYD_Utc10, utcNow_202201730_083059
				, isBranchLevel: true, isCompanyLevel: false
				, expectedSubjectRecipients: Array.Empty<(string, Guid, Func<EmailDef, bool>)>()
				, expectedLogs: new[] { "Information|None of credential was saved to company[TAU]." }
			);

			AssertAlertForAlmostExpiryCredentials(TestingCompanyAUSYD_Utc10, utcNow_202201730_083059
				, isBranchLevel: false, isCompanyLevel: true
				, expectedSubjectRecipients: Array.Empty<(string, Guid, Func<EmailDef, bool>)>()
				, expectedLogs: new[] {
					"Information|Running alerting for 'TAU' (2022-07-30 18:30:59), alerting days before expiry:25."
					, "Information|None of credential is almost expiry for company."
				}
			);

			AssertAlertForAlmostExpiryCredentials(TestingCompanyAUSYD_Utc10, utcNow_202201730_083059
				, isBranchLevel: true, isCompanyLevel: true
				, expectedSubjectRecipients: Array.Empty<(string, Guid, Func<EmailDef, bool>)>()
				, expectedLogs: new[] {
					"Information|Running alerting for 'TAU' (2022-07-30 18:30:59), alerting days before expiry:25."
					, "Information|None of credential is almost expiry for company."
					, "Information|None of credential was saved to company[TAU]."
				}
			);
		}

		IEInvoicingCredentialSettings GetMockCredentialSettings(bool isBranchLevel, bool isCompanyLevel)
		{
			var mockCredentialSettings = new Mock<IEInvoicingCredentialSettings>();
			mockCredentialSettings.Setup(x => x.IsBranchCredentialsRequired).Returns(isBranchLevel);
			mockCredentialSettings.Setup(x => x.IsCompanyCredentialsRequired).Returns(isCompanyLevel);

			return mockCredentialSettings.Object;
		}

		void AssertAlertForAlmostExpiryCredentials(GlbCompany company, DateTime utcNow
			, bool isBranchLevel, bool isCompanyLevel
			, IEnumerable<(string Comment, Guid GroupPK, Func<EmailDef, bool> MailMatch)> expectedSubjectRecipients
			, IEnumerable<string> expectedLogs
			, IElectronicMessagingNotificationEmailCreator emailCreator = null
			, IElectronicMessagingNotificationQueryProvider notificationQueryProvider = null)
		{
			MailManagerMock.Invocations.Clear();
			LoggerInfos.Clear();

			var mockCredentialSettings = new Mock<IEInvoicingCredentialSettings>();
			mockCredentialSettings.Setup(x => x.IsBranchCredentialsRequired).Returns(isBranchLevel);
			mockCredentialSettings.Setup(x => x.IsCompanyCredentialsRequired).Returns(isCompanyLevel);

			new ElectronicMessagingCertificateExpiryDateCheckProcessor().AlertForAlmostExpiryCredentials(company, mockCredentialSettings.Object, utcNow, MockLogger.Object, emailCreator, notificationQueryProvider);

			AssertContainsExactElementsInExactOrder("Logs", expectedLogs, LoggerInfos);

			CombineAssertions("Mail Recipient and Subject", () =>
			{
				MailManagerMock.Verify(x => x.CreateAndSave(It.IsAny<EmailDef>(), It.IsAny<Guid>(), It.IsAny<IGroupSourceLocator>())
					, Times.Exactly(expectedSubjectRecipients.Count())
					, "Total mail creation calling times."
				);

				foreach ((string comment, Guid groupPK, Func<EmailDef, bool> mailMatch) in expectedSubjectRecipients)
				{
					MailManagerMock.Verify(x => x.CreateAndSave(It.Is<EmailDef>(p => mailMatch(p)), groupPK, It.Is<IGroupSourceLocator>(p => p.Location == ExpectedGroupLocationMultilingual))
					, Times.Exactly(1)
					, comment
				);
				}
			});
		}

		public void TestAlertForAlmostExpiryCredentials_NullParameter()
		{
			var mockCredentialSettings = new Mock<IEInvoicingCredentialSettings>();
			mockCredentialSettings.Setup(x => x.IsBranchCredentialsRequired).Returns(false);
			mockCredentialSettings.Setup(x => x.IsCompanyCredentialsRequired).Returns(false);

			var expNullCredentialSettings = AssertExceptionThrown<ArgumentNullException>(()
				=> new ElectronicMessagingCertificateExpiryDateCheckProcessor().AlertForAlmostExpiryCredentials(TestingCompanyAUSYD_Utc10, null, ZDateTime.Now, null)
			);
			AssertEquals("Value cannot be null.\r\nParameter name: credentialSettings", expNullCredentialSettings.Message);

			var expNullCompany = AssertExceptionThrown<ArgumentNullException>(
				() => new ElectronicMessagingCertificateExpiryDateCheckProcessor().AlertForAlmostExpiryCredentials(null, mockCredentialSettings.Object, ZDateTime.Now, null)
			);
			AssertEquals("Value cannot be null.\r\nParameter name: company", expNullCompany.Message);

			AssertNoExceptionThrown("Logger could be null."
				, () => new ElectronicMessagingCertificateExpiryDateCheckProcessor().AlertForAlmostExpiryCredentials(TestingCompanyAUSYD_Utc10, mockCredentialSettings.Object, ZDateTime.Now, null)
			);
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestingCompanyAUSYD_Utc10 = TestObjectCreator.CreateNewCompany("TAU", CountryCodes.Australia, TestObjectCreator.ABIGAS);
			TestingBranchAUSYD_Utc10 = TestObjectCreator.CreateBranch("AU1", TestingCompanyAUSYD_Utc10, TestObjectCreator.ABIGAS);
			TestingBranchPERTH_Utc8 = TestObjectCreator.CreateBranch("AU2", TestingCompanyAUSYD_Utc10, TestObjectCreator.ABIGAS);
			TestingBranchAUADO_Utc9point5 = TestObjectCreator.CreateBranch("AU3", TestingCompanyAUSYD_Utc10, TestObjectCreator.ABIGAS);

			TestingCompanyAUSYD_Utc10.GC_Name = "Dummy AU Company";
			TestingBranchAUSYD_Utc10.GB_BranchName = "Dummy Sydney Branch";
			TestingBranchPERTH_Utc8.GB_BranchName = "Dummy Perth Branch";
			TestingBranchAUADO_Utc9point5.GB_BranchName = "Dummy Ado Branch";

			TestObjectCreator.ABIGAS.OH_RL_NKClosestPort = "AUSYD";
			TestingBranchAUSYD_Utc10.GB_RL_NKHomePort = "AUSYD";
			TestingBranchPERTH_Utc8.GB_RL_NKHomePort = "AUEPT";
			TestingBranchAUADO_Utc9point5.GB_RL_NKHomePort = "AUADO";
			Factory.Save();

			MailManagerMock = MockMailCreator.MockInstance;
			MailManagerMock.Setup(x => x.CreateAndSave(It.IsAny<EmailDef>(), It.IsAny<Guid>(), It.IsAny<IGroupSourceLocator>()))
				.Callback<EmailDef, Guid, IGroupSourceLocator>((mail, _, __) => AssertType(typeof(ElectronicMessagingCertificateExpiryDateCheckEmail), mail));

			LoggerInfos = new List<string>();
			MockLogger = new Mock<ILogger>();
			MockLogger.Setup(x => x.Log(It.IsAny<LogType>(), It.IsAny<string>()))
				.Callback<LogType, string>((logType, msg) => LoggerInfos.Add($"{logType}|{msg}"));

			OldProvider = Env.GetCurrentProvider();
			TempProviderForMocking = new NullEnvProvider();
			TempProviderForMocking.Enable();
			TempProviderForMocking.Instance.SetUserContext(OldProvider.Instance.CurrentUserContext);
			ObjectFactory.Substitute<IOutgoingMailManager>(new MockMailCreator());
		}

		protected override void TearDown()
		{
			base.TearDown();
			OldProvider.Enable();
			TempProviderForMocking.Dispose();
			TempProviderForMocking = null;
		}

		GlbCompany TestingCompanyAUSYD_Utc10;

		GlbBranch TestingBranchAUSYD_Utc10;
		GlbBranch TestingBranchPERTH_Utc8;
		GlbBranch TestingBranchAUADO_Utc9point5;
		readonly string ExpectedGroupLocationMultilingual = ((IMultilingualRegistryItem)AccountingConfigurationRegistry.Instance.EInvoicingCertificateExpiryNotificationGroup).LocationMultilingual;

		List<string> LoggerInfos;

		EnvProvider OldProvider;
		EnvProvider TempProviderForMocking;

		Mock<IOutgoingMailManager> MailManagerMock;
		Mock<ILogger> MockLogger;
		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		class MockMailCreator : OutgoingMailCreator
		{
			public static readonly new IOutgoingMailManager Instance = GetInstance();

			public static Mock<IOutgoingMailManager> MockInstance { get; private set; }

			static IOutgoingMailManager GetInstance()
			{
				MockInstance = new Mock<IOutgoingMailManager>();
				return MockInstance.Object;
			}
		}
	}
}
