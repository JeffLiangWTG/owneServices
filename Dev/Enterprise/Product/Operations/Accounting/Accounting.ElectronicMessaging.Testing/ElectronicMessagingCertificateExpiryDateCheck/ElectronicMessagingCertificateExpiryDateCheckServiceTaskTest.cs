using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.ElectronicMessagingCertificateExpiryDateCheck;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing
{
	[TestedType(typeof(ElectronicMessagingCertificateExpiryDateCheckServiceTask))]
	public class ElectronicMessagingCertificateExpiryDateCheckServiceTaskTest : ServiceTaskTestCase<ElectronicMessagingCertificateExpiryDateCheckServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		[TestDate(2022, 01, 15, 18, 30, 59)]
		public void TestRunServiceTaskEligible()
		{
			AssertRunServiceTask(PasswordTypesList.Codes.EIM
				, isBranchCredentialsRequired: true
				, isCompanyCredentialsRequired: true
				, expectedEligible: true);

			AssertRunServiceTask(PasswordTypesList.Codes.EIM
				, isBranchCredentialsRequired: false
				, isCompanyCredentialsRequired: true
				, expectedEligible: true);

			AssertRunServiceTask(PasswordTypesList.Codes.EIM
				, isBranchCredentialsRequired: true
				, isCompanyCredentialsRequired: false
				, expectedEligible: true);

			AssertRunServiceTask(PasswordTypesList.Codes.EIM
				, isBranchCredentialsRequired: false
				, isCompanyCredentialsRequired: false
				, expectedEligible: true);
		}

		[TestDate(2022, 01, 15, 18, 30, 59)]
		public void TestRunServiceTaskNotEligible()
		{
			foreach (var passwordType in new PasswordTypesList().GetAllCodes().Except(PasswordTypesList.Codes.EIM))
			{
				AssertRunServiceTask(passwordType
					, isBranchCredentialsRequired: true
					, isCompanyCredentialsRequired: true
					, expectedEligible: false);

				AssertRunServiceTask(passwordType
					, isBranchCredentialsRequired: false
					, isCompanyCredentialsRequired: true
					, expectedEligible: false);

				AssertRunServiceTask(passwordType
					, isBranchCredentialsRequired: true
					, isCompanyCredentialsRequired: false
					, expectedEligible: false);

				AssertRunServiceTask(passwordType
					, isBranchCredentialsRequired: false
					, isCompanyCredentialsRequired: false
					, expectedEligible: false);
			}
		}

		void AssertRunServiceTask(string passwordType, bool isBranchCredentialsRequired, bool isCompanyCredentialsRequired, bool expectedEligible)
		{
			var utcNow_20220115_183059 = new ZDateTime(2022, 01, 15, 18, 30, 59);
			AssertEquals("Precondition", utcNow_20220115_183059, ZDateTime.UtcNow);

			var mockProcessor = new Mock<IElectronicMessagingCertificateExpiryDateCheckProcessor>();
			mockProcessor.Setup(x => x.AlertForAlmostExpiryCredentials(It.IsAny<GlbCompany>(), It.IsAny<IEInvoicingCredentialSettings>(), It.IsAny<ZDateTime>(),
				It.IsAny<ILogger>(),
				It.IsAny<IElectronicMessagingNotificationEmailCreator>(),
				It.IsAny<IElectronicMessagingNotificationQueryProvider>()))
				.Callback<GlbCompany, IEInvoicingCredentialSettings, ZDateTime, ILogger, IElectronicMessagingNotificationEmailCreator, IElectronicMessagingNotificationQueryProvider>(MockAlertForAlmostExpiryCredentialsCore);

			using (ObjectFactory.Substitute(mockProcessor.Object))
			{
				var mockCountryFactory = new Mock<ICountryEInvoicingObjectFactory>();
				mockCountryFactory.As<IElectronicMessagingNotificationQueryProvider>();
				var mockCredentials = new Mock<IEInvoicingCredentialSettings>();
				mockCredentials.Setup(x => x.IsCompanyCredentialsRequired).Returns(isCompanyCredentialsRequired);
				mockCredentials.Setup(x => x.IsBranchCredentialsRequired).Returns(isBranchCredentialsRequired);
				mockCredentials.Setup(x => x.PasswordType).Returns(passwordType);
				mockCountryFactory.Setup(x => x.Credentials).Returns(mockCredentials.Object);
				GlobalEInvoicingObjectFactory.TestCountryFactory = mockCountryFactory.Object;

				var serviceTask = new ElectronicMessagingCertificateExpiryDateCheckServiceTask();
				InitialiseAndRunTaskSchedule(serviceTask);

				if (expectedEligible)
				{
					AssertLogs((TestServiceLogger)serviceTask.ServiceLogger
						, "Information|E-Reporting Certificate/Token Expiry Date Check Service is running at 2022-01-15 18:30:59."
						, "Information|Dummy Info for EDI, utcTime:2022-01-15 18:30:59."
						, "Information|Dummy Info for SIN, utcTime:2022-01-15 18:30:59."
						, "Information|E-Reporting Certificate/Token Expiry Date Check Service is completed."
					);

					mockProcessor.Verify(x => x.AlertForAlmostExpiryCredentials(It.IsAny<GlbCompany>(), It.IsAny<IEInvoicingCredentialSettings>(), It.IsAny<ZDateTime>(), It.IsAny<ILogger>(),
						It.IsAny<IElectronicMessagingNotificationEmailCreator>(), It.IsAny<IElectronicMessagingNotificationQueryProvider>())
						, Times.Exactly(2)
					);
					mockProcessor.Verify(x => x.AlertForAlmostExpiryCredentials(It.Is<GlbCompany>(c => c.GC_Code == "EDI"), mockCredentials.Object, utcNow_20220115_183059, serviceTask.ServiceLogger,
						It.IsAny<IElectronicMessagingNotificationEmailCreator>(), It.IsAny<IElectronicMessagingNotificationQueryProvider>())
						, Times.Exactly(1)
					);
					mockProcessor.Verify(x => x.AlertForAlmostExpiryCredentials(It.Is<GlbCompany>(c => c.GC_Code == "SIN"), mockCredentials.Object, utcNow_20220115_183059, serviceTask.ServiceLogger,
						It.IsAny<IElectronicMessagingNotificationEmailCreator>(), It.IsAny<IElectronicMessagingNotificationQueryProvider>())
						, Times.Exactly(1)
					);
				}
				else
				{
					AssertLogs((TestServiceLogger)serviceTask.ServiceLogger
						, "Information|E-Reporting Certificate/Token Expiry Date Check Service is running at 2022-01-15 18:30:59."
						, "Information|E-Reporting Certificate/Token Expiry Date Check Service is completed."
					);

					mockProcessor.Verify(x => x.AlertForAlmostExpiryCredentials(It.IsAny<GlbCompany>(), It.IsAny<IEInvoicingCredentialSettings>(), It.IsAny<ZDateTime>(), It.IsAny<ILogger>(),
						It.IsAny<IElectronicMessagingNotificationEmailCreator>(), It.IsAny<IElectronicMessagingNotificationQueryProvider>())
						, Times.Exactly(0)
					);
				}
			}

			void MockAlertForAlmostExpiryCredentialsCore(GlbCompany company, IEInvoicingCredentialSettings credentialSettings, ZDateTime utcNow, ILogger logger,
				IElectronicMessagingNotificationEmailCreator emailCreator,
				IElectronicMessagingNotificationQueryProvider notificationQueryProvider)
			{
				logger.Information($"Dummy Info for {company.GC_Code}, utcTime:{utcNow:yyyy-MM-dd HH:mm:ss}.");
			}
		}

		[TestDate(2022, 01, 15, 18, 30, 59)]
		public void TestRunServiceTaskWhenNoCountryFactory()
		{
			TestDateAttribute.UseUNLOCO = true;
			var utcNow_20220115_183059 = new ZDateTime(2022, 01, 15, 18, 30, 59);
			AssertEquals("Precondition", utcNow_20220115_183059, ZDateTime.UtcNow);

			var mockProcessor = new Mock<IElectronicMessagingCertificateExpiryDateCheckProcessor>();
			mockProcessor.Setup(x => x.AlertForAlmostExpiryCredentials(It.IsAny<GlbCompany>(), It.IsAny<IEInvoicingCredentialSettings>(), It.IsAny<ZDateTime>(), It.IsAny<ILogger>(),
				It.IsAny<IElectronicMessagingNotificationEmailCreator>(), It.IsAny<IElectronicMessagingNotificationQueryProvider>()));

			using (ObjectFactory.Substitute(mockProcessor.Object))
			{
				GlobalEInvoicingObjectFactory.TestCountryFactory = null;
				var mockCountryFactory = new Mock<ICountryEInvoicingObjectFactory>();
				mockCountryFactory.Setup(x => x.Credentials).Returns((IEInvoicingCredentialSettings)null);
				GlobalEInvoicingObjectFactory.TestCountryFactory = mockCountryFactory.Object;
				var serviceTask = new ElectronicMessagingCertificateExpiryDateCheckServiceTask();

				InitialiseAndRunTaskSchedule(serviceTask);
				AssertLogs((TestServiceLogger)serviceTask.ServiceLogger
					, "Information|E-Reporting Certificate/Token Expiry Date Check Service is running at 2022-01-15 18:30:59."
					, "Information|E-Reporting Certificate/Token Expiry Date Check Service is completed."
				);

				mockProcessor.Verify(x => x.AlertForAlmostExpiryCredentials(It.IsAny<GlbCompany>(), It.IsAny<IEInvoicingCredentialSettings>(), It.IsAny<ZDateTime>(), It.IsAny<ILogger>(),
					It.IsAny<IElectronicMessagingNotificationEmailCreator>(), It.IsAny<IElectronicMessagingNotificationQueryProvider>())
					, Times.Exactly(0)
				);
			}
		}

		[TestDate(2022, 01, 15, 18, 30, 59)]
		public void TestRunServiceTaskWhenNoCredentials()
		{
			TestDateAttribute.UseUNLOCO = true;
			var utcNow_20220115_183059 = new ZDateTime(2022, 01, 15, 18, 30, 59);
			AssertEquals("Precondition", utcNow_20220115_183059, ZDateTime.UtcNow);

			var mockProcessor = new Mock<IElectronicMessagingCertificateExpiryDateCheckProcessor>();
			mockProcessor.Setup(x => x.AlertForAlmostExpiryCredentials(It.IsAny<GlbCompany>(), It.IsAny<IEInvoicingCredentialSettings>(), It.IsAny<ZDateTime>(), It.IsAny<ILogger>(),
				It.IsAny<IElectronicMessagingNotificationEmailCreator>(), It.IsAny<IElectronicMessagingNotificationQueryProvider>()));

			using (ObjectFactory.Substitute(mockProcessor.Object))
			{
				var mockCountryFactory = new Mock<ICountryEInvoicingObjectFactory>();
				mockCountryFactory.Setup(x => x.Credentials).Returns((IEInvoicingCredentialSettings)null);
				GlobalEInvoicingObjectFactory.TestCountryFactory = mockCountryFactory.Object;
				var serviceTask = new ElectronicMessagingCertificateExpiryDateCheckServiceTask();

				InitialiseAndRunTaskSchedule(serviceTask);
				AssertLogs((TestServiceLogger)serviceTask.ServiceLogger
					, "Information|E-Reporting Certificate/Token Expiry Date Check Service is running at 2022-01-15 18:30:59."
					, "Information|E-Reporting Certificate/Token Expiry Date Check Service is completed."
				);

				mockProcessor.Verify(x => x.AlertForAlmostExpiryCredentials(It.IsAny<GlbCompany>(), It.IsAny<IEInvoicingCredentialSettings>(), It.IsAny<ZDateTime>(), It.IsAny<ILogger>(),
					It.IsAny<IElectronicMessagingNotificationEmailCreator>(), It.IsAny<IElectronicMessagingNotificationQueryProvider>())
					, Times.Exactly(0)
				);
			}
		}

		[TestDate(2022, 01, 15, 18, 30, 59)]
		public void TestRunServiceTaskWhenNotCriticalError()
		{
			TestDateAttribute.UseUNLOCO = true;
			var utcNow_20220115_183059 = new ZDateTime(2022, 01, 15, 18, 30, 59);
			AssertEquals("Precondition", utcNow_20220115_183059, ZDateTime.UtcNow);

			var mockProcessor = new Mock<IElectronicMessagingCertificateExpiryDateCheckProcessor>();
			mockProcessor.Setup(x => x.AlertForAlmostExpiryCredentials(It.IsAny<GlbCompany>(), It.IsAny<IEInvoicingCredentialSettings>(), It.IsAny<ZDateTime>(), It.IsAny<ILogger>(),
				It.IsAny<IElectronicMessagingNotificationEmailCreator>(), It.IsAny<IElectronicMessagingNotificationQueryProvider>()))
				.Callback<GlbCompany, IEInvoicingCredentialSettings, ZDateTime, ILogger, IElectronicMessagingNotificationEmailCreator, IElectronicMessagingNotificationQueryProvider>(MockAlertForAlmostExpiryCredentialsCore);

			using (ObjectFactory.Substitute(mockProcessor.Object))
			{
				var mockCountryFactory = CreateEligibleCountryFactory();
				mockCountryFactory.As<IElectronicMessagingNotificationQueryProvider>();
				GlobalEInvoicingObjectFactory.TestCountryFactory = mockCountryFactory.Object;
				var serviceTask = new ElectronicMessagingCertificateExpiryDateCheckServiceTask();

				InitialiseAndRunTaskSchedule(serviceTask);
				AssertLogs((TestServiceLogger)serviceTask.ServiceLogger
					, "Information|E-Reporting Certificate/Token Expiry Date Check Service is running at 2022-01-15 18:30:59."
					, "Error|Fail error for 'EDI' - Dummy Critical Exception."
					, "Error|Fail error for 'SIN' - Dummy Critical Exception."
					, "Information|E-Reporting Certificate/Token Expiry Date Check Service is completed."
				);

				mockProcessor.Verify(x => x.AlertForAlmostExpiryCredentials(It.IsAny<GlbCompany>(), It.IsAny<IEInvoicingCredentialSettings>(), It.IsAny<ZDateTime>(), It.IsAny<ILogger>(),
					It.IsAny<IElectronicMessagingNotificationEmailCreator>(), It.IsAny<IElectronicMessagingNotificationQueryProvider>())
					, Times.Exactly(2)
				);
				mockProcessor.Verify(x => x.AlertForAlmostExpiryCredentials(It.Is<GlbCompany>(c => c.GC_Code == "EDI"), mockCountryFactory.Object.Credentials, utcNow_20220115_183059, serviceTask.ServiceLogger, It.IsAny<IElectronicMessagingNotificationEmailCreator>(), It.IsAny<IElectronicMessagingNotificationQueryProvider>())
					, Times.Exactly(1)
				);
				mockProcessor.Verify(x => x.AlertForAlmostExpiryCredentials(It.Is<GlbCompany>(c => c.GC_Code == "SIN"), mockCountryFactory.Object.Credentials, utcNow_20220115_183059, serviceTask.ServiceLogger, It.IsAny<IElectronicMessagingNotificationEmailCreator>(), It.IsAny<IElectronicMessagingNotificationQueryProvider>())
					, Times.Exactly(1)
				);
			}

			void MockAlertForAlmostExpiryCredentialsCore(GlbCompany company, IEInvoicingCredentialSettings credentialSettings, ZDateTime utcNow, ILogger logger,
				IElectronicMessagingNotificationEmailCreator emailCreator, IElectronicMessagingNotificationQueryProvider notificationQueryProvider)
			{
				throw new Exception("Dummy Critical Exception");
			}
		}

		public void TestRunServiceTaskWhenCriticalError()
		{
			var dummyCriticalException = new OutOfMemoryException("Dummy Critical Exception");
			Assert("PreCondition", dummyCriticalException.IsCriticalException());

			var mockProcessor = new Mock<IElectronicMessagingCertificateExpiryDateCheckProcessor>();
			mockProcessor.Setup(x => x.AlertForAlmostExpiryCredentials(It.IsAny<GlbCompany>(), It.IsAny<IEInvoicingCredentialSettings>(), It.IsAny<ZDateTime>(), It.IsAny<ILogger>(),
				It.IsAny<IElectronicMessagingNotificationEmailCreator>(), It.IsAny<IElectronicMessagingNotificationQueryProvider>()))
				.Callback<GlbCompany, IEInvoicingCredentialSettings, ZDateTime, ILogger, IElectronicMessagingNotificationEmailCreator, IElectronicMessagingNotificationQueryProvider>(MockAlertForAlmostExpiryCredentialsCore);

			using (ObjectFactory.Substitute(mockProcessor.Object))
			{
				GlobalEInvoicingObjectFactory.TestCountryFactory = CreateEligibleCountryFactory().Object;
				var serviceTask = new ElectronicMessagingCertificateExpiryDateCheckServiceTask();
				var exception = AssertExceptionThrown<Exception>(() => InitialiseAndRunTaskSchedule(serviceTask));

				AssertEquals(dummyCriticalException, exception);
			}

			void MockAlertForAlmostExpiryCredentialsCore(GlbCompany company, IEInvoicingCredentialSettings credentialSettings, ZDateTime utcNow, ILogger logger,
				IElectronicMessagingNotificationEmailCreator emailCreator, IElectronicMessagingNotificationQueryProvider notificationQueryProvider)
			{
				throw dummyCriticalException;
			}
		}

		public void TestBasicSettings()
		{
			var attribute = GetHostedServiceAttributes().FirstOrDefault();

			AssertEquals("Period", "1day", attribute.MinimumPeriod);
			AssertEquals("DefaultSchedule.RunEvery", "1day", attribute.DefaultSchedule.RunEvery);
			AssertEquals("Code", "ECE", ElectronicMessagingCertificateExpiryDateCheckServiceTask.Code);
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();

			OriginalTestCountryCode = GlobalEInvoicingObjectFactory.TestCountryCode;
			GlobalEInvoicingObjectFactory.TestCountryCode = GlobalEInvoicingObjectFactory.TestCountryCodeForAllCountry;

			var expectedDefaultActiveCompanies = new[] { "EDI", "SIN" };

			var defaultActiveCompaniesInDB = Factory.Load<GlbCompany>(new ZQuery())
				.Where(company => company.GC_Code != GlbCompany.DemoCompanyCode && company.HasActiveBranch);

			CombineAssertions("PreCondition, default companies in DB", () =>
			{
				AssertEquals("Count", 2, defaultActiveCompaniesInDB.Count());
				AssertContainsExactElementsInAnyOrder("Company Codes"
					, expectedDefaultActiveCompanies
					, defaultActiveCompaniesInDB.Select(x => x.GC_Code)
				);
			});

			var nonActiveCompany = TestObjectCreator.CreateNewCompany("DTR", CountryCodes.Turkey, orgProxy: TestObjectCreator.CreditorTR);
			var nonActiveBranch = TestObjectCreator.CreateBranch("AAA", nonActiveCompany);
			nonActiveBranch.GB_IsActive = false;
			Factory.Save();
		}

		protected override void TearDownCore()
		{
			base.TearDownCore();

			GlobalEInvoicingObjectFactory.TestCountryCode = OriginalTestCountryCode;
			GlobalEInvoicingObjectFactory.TestCountryFactory = null;
		}

		string OriginalTestCountryCode;

		void AssertLogs(TestServiceLogger logger, params string[] messages)
		{
			var index = 0;
			CombineAssertions(() =>
			{
				foreach (var message in messages)
				{
					AssertEquals(message, logger[index++]);
				}
			});
		}

		Mock<ICountryEInvoicingObjectFactory> CreateEligibleCountryFactory()
		{
			var mockCountryFactory = new Mock<ICountryEInvoicingObjectFactory>();
			mockCountryFactory.As<IElectronicMessagingNotificationQueryProvider>();
			var mockCredentials = new Mock<IEInvoicingCredentialSettings>();
			mockCredentials.Setup(x => x.IsCompanyCredentialsRequired).Returns(true);
			mockCredentials.Setup(x => x.IsBranchCredentialsRequired).Returns(false);
			mockCredentials.Setup(x => x.PasswordType).Returns(PasswordTypesList.Codes.EIM);
			mockCountryFactory.Setup(x => x.Credentials).Returns(mockCredentials.Object);

			return mockCountryFactory;
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
