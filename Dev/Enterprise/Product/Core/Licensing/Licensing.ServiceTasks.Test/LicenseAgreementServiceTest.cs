using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.TrustedMessaging.Intergration;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.Licensing.ServiceTasks.Testing
{
	[TestedType(typeof(LicenseAgreementService))]
	sealed class LicenseAgreementServiceTest : ServiceTaskTestCase<SystemLicenceService>
	{
		#region Helpers

		static string RunTask(bool shouldSetCertificates = true)
		{
			if (shouldSetCertificates)
			{
				WebDataRegistry.Instance.TrustedMessagingClientSystemCertificate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, [1, 2, 3]);
				WebDataRegistry.Instance.TrustedMessagingClientSystemCertificatePassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "123");
				WebDataRegistry.Instance.TrustedMessagingCentralSystemCertificate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, [1, 2, 3]);
			}

			var logger = new TestServiceLogger();
			var task = new LicenseAgreementService();
			task.ServiceLogger = logger;
			task.RunTask();
			return logger.ToString();
		}

		static TrustedResponse<GetAcceptancesResponse> GetAcceptances(params LicenseAcceptanceResponse[] acceptances)
		{
			var result = new TrustedResponse<GetAcceptancesResponse>();
			var data = new GetAcceptancesResponse();
			data.Acceptances.AddRange(acceptances);
			result.Success = true;
			result.Response = data;
			return result;
		}

		static TrustedResponse<UserAgreementResponseData> GetUserAgreementResponse(int versionNumber = 999, bool isRequired = true)
		{
			var result = new TrustedResponse<UserAgreementResponseData>();
			var responseData = new UserAgreementResponseData();
			result.Success = true;
			result.Response = responseData;

			responseData.Title = "Animal time";
			responseData.VersionNumber = versionNumber;
			responseData.Content = "🐭🐹🐰🐶🐺🦊\r\nAccept the \r\nanimals\r\ninto\r\nyour heart."; // Testing unicode is fine.
			responseData.Required = isRequired;
			return result;
		}

		static TrustedResponse<UserAgreementResponseData> GetUserAgreementResponse(int majorVersion, int minorVersion, string variant)
		{
			var result = new TrustedResponse<UserAgreementResponseData>();
			var responseData = new UserAgreementResponseData();
			result.Success = true;
			result.Response = responseData;

			responseData.Title = "Animal time";
			responseData.VersionNumber = majorVersion;
			responseData.MinorVersionNumber = minorVersion;
			responseData.Variant = variant;
			responseData.Content = "🐭🐹🐰🐶🐺🦊\r\nAccept the \r\nanimals\r\ninto\r\nyour heart."; // Testing unicode is fine.
			responseData.Required = true;
			return result;
		}

		static TrustedResponse<UserAgreementResponseData> GetUserAgreementNullResponse()
		{
			var result = new TrustedResponse<UserAgreementResponseData>();
			var responseData = new UserAgreementResponseData();
			result.Success = false;
			result.Response = null;
			return result;
		}

		static LicenseAcceptanceResponse MakeAcceptance(int majorVersion, int minorVersion, string variant, string name = "The Gecko", string email = "mmm@foo.goo")
		{
			var enterpriseAcceptance = new LicenseAcceptanceResponse();
			enterpriseAcceptance.EffectiveStartUtc = DateTime.UtcNow;
			enterpriseAcceptance.AcceptedByName = name;
			enterpriseAcceptance.AcceptedByEmail = email;
			enterpriseAcceptance.AcceptedIPAddresss = "10.0.0.22";
			enterpriseAcceptance.AcceptedTimeUtc = DateTime.UtcNow;
			enterpriseAcceptance.Title = "I saw a pie";
			enterpriseAcceptance.Content = "It was pretty";
			enterpriseAcceptance.MajorVersion = majorVersion.ToString();
			enterpriseAcceptance.MinorVersion = minorVersion.ToString();
			enterpriseAcceptance.Variant = variant;
			enterpriseAcceptance.Type = "CWN";
			return enterpriseAcceptance;
		}

		#endregion

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		public void TestNothingToConnectTo()
		{
			var task = new SystemLicenceService();
			AssertNoExceptionThrown(task.RunTask);
		}

		public void TestNewAgreement()
		{
			var response = GetUserAgreementResponse();
			var responseData = response.Response;
			var mock = new Mock<IUserPortalClient>();
			mock.Setup(m => m.GetAcceptancesAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(GetAcceptances()));
			mock.Setup(m => m.GetUserAgreementAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(response));

			using (ObjectFactory.Substitute(nameof(IUserPortalClient), mock.Object))
			{
				RunTask();

				var agreements = Factory.Load<LicenseAgreement>(new ZQuery());
				AssertEquals(1, agreements.Length);
				var agreement = agreements[0];
				AssertEquals(responseData.Title, agreement.LAG_Title);
				AssertEquals(responseData.Content, agreement.LAG_Content);
				AssertEquals(responseData.VersionNumber.ToString(), agreement.LAG_MajorVersion);
				AssertEquals(responseData.VersionNumber + "." + responseData.MinorVersionNumber, agreement.VersionNumber);
				AssertEquals(false, agreement.LAG_EffectiveStartUtc.IsEmpty);
				AssertEquals(true, agreement.LAG_EffectiveEndUtc.IsEmpty);
				AssertEquals(LicenseAgreementStatusList.Codes.Pending, agreement.LAG_Status);
			}
		}

		public void TestNewAgreement_NotRequired()
		{
			var response = GetUserAgreementResponse(versionNumber: 0, isRequired: false);
			var mock = new Mock<IUserPortalClient>();
			mock.Setup(m => m.GetAcceptancesAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(GetAcceptances()));
			mock.Setup(m => m.GetUserAgreementAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(response));

			using (ObjectFactory.Substitute(nameof(IUserPortalClient), mock.Object))
			{
				var logs = RunTask();

				var agreements = Factory.Load<LicenseAgreement>(new ZQuery { ReLoadExistingRows = true });
				AssertEquals("The response should not be added into database", 0, agreements.Length);
				AssertContains("Skipping response since it isn't required", logs);

				mock.Setup(m => m.GetUserAgreementAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(GetUserAgreementResponse(12)));

				RunTask();
				agreements = Factory.Load<LicenseAgreement>(new ZQuery { ReLoadExistingRows = true });
				AssertEquals(1, agreements.Length);
				AssertEquals("12.0", agreements[0].VersionNumber);
			}
		}

		public void TestNewAgreement_CancelOnNewMinorVersionIfMajorVersionAccepted_Case1()
		{
			//case 1: 1.0 cancelled, 1.1 accepted, downloaded 1.2	=> not require to sign

			var acceptancesResponse = GetAcceptances();
			var agreementResponse = GetUserAgreementResponse(majorVersion: 1, minorVersion: 2, variant: "");

			var mock = new Mock<IUserPortalClient>();
			mock.Setup(m => m.GetAcceptancesAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(acceptancesResponse));
			mock.Setup(m => m.GetUserAgreementAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(agreementResponse));

			var localAgreement1_0 = Factory.New<LicenseAgreement>();
			localAgreement1_0.LAG_Status = LicenseAgreementStatusList.Codes.Cancelled;
			localAgreement1_0.LAG_Type = "CWN";
			localAgreement1_0.LAG_MajorVersion = "1";
			localAgreement1_0.LAG_MinorVersion = "0";
			localAgreement1_0.LAG_VariantCode = "";

			var localAgreement1_1 = Factory.New<LicenseAgreement>();
			localAgreement1_1.LAG_Status = LicenseAgreementStatusList.Codes.Accepted;
			localAgreement1_1.LAG_Type = "CWN";
			localAgreement1_1.LAG_MajorVersion = "1";
			localAgreement1_1.LAG_MinorVersion = "1";
			localAgreement1_1.LAG_VariantCode = "";

			Factory.Save();

			using (ObjectFactory.Substitute(nameof(IUserPortalClient), mock.Object))
			{
				RunTask();

				var agreements = Factory.Load<LicenseAgreement>(new ZQuery { ReLoadExistingRows = true });
				AssertEquals(3, agreements.Length);
				var localAgreement1_2 = agreements.Single(l => l.LAG_MinorVersion == "2");
				AssertNotNull(localAgreement1_2);
				AssertEquals(LicenseAgreementStatusList.Codes.Cancelled, localAgreement1_2.LAG_Status);
			}
		}

		public void TestNewAgreement_CancelOnNewMinorVersionIfMajorVersionAccepted_Case2()
		{
			//case 2: 1.0 cancelled, 1.1 cancelled, downloaded 1.2	=> require to sign

			var acceptancesResponse = GetAcceptances();
			var agreementResponse = GetUserAgreementResponse(majorVersion: 1, minorVersion: 2, variant: "");

			var mock = new Mock<IUserPortalClient>();
			mock.Setup(m => m.GetAcceptancesAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(acceptancesResponse));
			mock.Setup(m => m.GetUserAgreementAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(agreementResponse));

			var localAgreement1_0 = Factory.New<LicenseAgreement>();
			localAgreement1_0.LAG_Status = LicenseAgreementStatusList.Codes.Cancelled;
			localAgreement1_0.LAG_Type = "CWN";
			localAgreement1_0.LAG_MajorVersion = "1";
			localAgreement1_0.LAG_MinorVersion = "0";
			localAgreement1_0.LAG_VariantCode = "";

			var localAgreement1_1 = Factory.New<LicenseAgreement>();
			localAgreement1_1.LAG_Status = LicenseAgreementStatusList.Codes.Cancelled;
			localAgreement1_1.LAG_Type = "CWN";
			localAgreement1_1.LAG_MajorVersion = "1";
			localAgreement1_1.LAG_MinorVersion = "1";
			localAgreement1_1.LAG_VariantCode = "";

			Factory.Save();

			using (ObjectFactory.Substitute(nameof(IUserPortalClient), mock.Object))
			{
				RunTask();

				var agreements = Factory.Load<LicenseAgreement>(new ZQuery { ReLoadExistingRows = true });
				AssertEquals(3, agreements.Length);
				var localAgreement1_2 = agreements.Single(l => l.LAG_MinorVersion == "2");
				AssertNotNull(localAgreement1_2);
				AssertEquals(LicenseAgreementStatusList.Codes.Pending, localAgreement1_2.LAG_Status);
			}
		}

		public void TestNewAgreement_CancelOnNewMinorVersionIfMajorVersionAccepted_Case3()
		{
			//case 3: 1.0 accepted, 1.1 cancelled, downloaded 2.0	=> require to sign

			var acceptancesResponse = GetAcceptances();
			var agreementResponse = GetUserAgreementResponse(majorVersion: 2, minorVersion: 0, variant: "");

			var mock = new Mock<IUserPortalClient>();
			mock.Setup(m => m.GetAcceptancesAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(acceptancesResponse));
			mock.Setup(m => m.GetUserAgreementAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(agreementResponse));

			var localAgreement1_0 = Factory.New<LicenseAgreement>();
			localAgreement1_0.LAG_Status = LicenseAgreementStatusList.Codes.Accepted;
			localAgreement1_0.LAG_Type = "CWN";
			localAgreement1_0.LAG_MajorVersion = "1";
			localAgreement1_0.LAG_MinorVersion = "0";
			localAgreement1_0.LAG_VariantCode = "";

			var localAgreement1_1 = Factory.New<LicenseAgreement>();
			localAgreement1_1.LAG_Status = LicenseAgreementStatusList.Codes.Cancelled;
			localAgreement1_1.LAG_Type = "CWN";
			localAgreement1_1.LAG_MajorVersion = "1";
			localAgreement1_1.LAG_MinorVersion = "1";
			localAgreement1_1.LAG_VariantCode = "";

			Factory.Save();

			using (ObjectFactory.Substitute(nameof(IUserPortalClient), mock.Object))
			{
				RunTask();

				var agreements = Factory.Load<LicenseAgreement>(new ZQuery { ReLoadExistingRows = true });
				AssertEquals(3, agreements.Length);
				var localAgreement2_0 = agreements.Single(l => l.LAG_MajorVersion == "2");
				AssertNotNull(localAgreement2_0);
				AssertEquals(LicenseAgreementStatusList.Codes.Pending, localAgreement2_0.LAG_Status);
			}
		}

		public void TestNewAgreement_CancelOnNewMinorVersionIfMajorVersionAccepted_Case4()
		{
			//case 4: 1.0 cancelled, 1.1 queued, downloaded 1.2	=> not require to sign
			//Queued agreement is accepted locally and uploaded before downloading new version
			//If upload fails it should still be considered as accepted

			var acceptancesResponse = GetAcceptances();
			var agreementResponse = GetUserAgreementResponse(majorVersion: 1, minorVersion: 2, variant: "");

			var mock = new Mock<IUserPortalClient>();
			mock.Setup(m => m.GetAcceptancesAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(acceptancesResponse));
			mock.Setup(m => m.GetUserAgreementAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(agreementResponse));

			var localAgreement1_0 = Factory.New<LicenseAgreement>();
			localAgreement1_0.LAG_Status = LicenseAgreementStatusList.Codes.Cancelled;
			localAgreement1_0.LAG_Type = "CWN";
			localAgreement1_0.LAG_MajorVersion = "1";
			localAgreement1_0.LAG_MinorVersion = "0";
			localAgreement1_0.LAG_VariantCode = "";

			var localAgreement1_1 = Factory.New<LicenseAgreement>();
			localAgreement1_1.LAG_Status = LicenseAgreementStatusList.Codes.Queued;
			localAgreement1_1.LAG_Type = "CWN";
			localAgreement1_1.LAG_MajorVersion = "1";
			localAgreement1_1.LAG_MinorVersion = "1";
			localAgreement1_1.LAG_VariantCode = "";
			localAgreement1_1.LAG_AcceptedTimeUtc = DateTime.UtcNow;

			Factory.Save();

			using (ObjectFactory.Substitute(nameof(IUserPortalClient), mock.Object))
			{
				RunTask();

				var agreements = Factory.Load<LicenseAgreement>(new ZQuery { ReLoadExistingRows = true });
				AssertEquals(3, agreements.Length);
				var localAgreement1_2 = agreements.Single(l => l.LAG_MinorVersion == "2");
				AssertNotNull(localAgreement1_2);
				AssertEquals(LicenseAgreementStatusList.Codes.Cancelled, localAgreement1_2.LAG_Status);
			}
		}

		public void TestNewAgreement_NewVariant_Case1()
		{
			//case 1: 1.1 Variant A accepted, downloaded 1.0 Variant B	=> require to sign
			var acceptancesResponse = GetAcceptances();
			var agreementResponse = GetUserAgreementResponse(majorVersion: 1, minorVersion: 0, variant: "B");

			var mock = new Mock<IUserPortalClient>();
			mock.Setup(m => m.GetAcceptancesAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(acceptancesResponse));
			mock.Setup(m => m.GetUserAgreementAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(agreementResponse));

			var localAgreement1_1_A = Factory.New<LicenseAgreement>();
			localAgreement1_1_A.LAG_Status = LicenseAgreementStatusList.Codes.Accepted;
			localAgreement1_1_A.LAG_Type = "CWN";
			localAgreement1_1_A.LAG_MajorVersion = "1";
			localAgreement1_1_A.LAG_MinorVersion = "1";
			localAgreement1_1_A.LAG_VariantCode = "A";

			Factory.Save();

			using (ObjectFactory.Substitute(nameof(IUserPortalClient), mock.Object))
			{
				RunTask();

				var agreements = Factory.Load<LicenseAgreement>(new ZQuery { ReLoadExistingRows = true });
				AssertEquals(2, agreements.Length);
				var localAgreement1_0_B = agreements.Single(l => l.LAG_VariantCode == "B");
				AssertNotNull(localAgreement1_0_B);
				AssertEquals(LicenseAgreementStatusList.Codes.Pending, localAgreement1_0_B.LAG_Status);
			}
		}

		public void TestNewAgreement_NewVariant_Case2()
		{
			//case 2: 1.1 Variant B cancelled, downloaded 1.2 Variant B	=> require to sign
			var acceptancesResponse = GetAcceptances();
			var agreementResponse = GetUserAgreementResponse(majorVersion: 1, minorVersion: 2, variant: "B");

			var mock = new Mock<IUserPortalClient>();
			mock.Setup(m => m.GetAcceptancesAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(acceptancesResponse));
			mock.Setup(m => m.GetUserAgreementAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(agreementResponse));

			var localAgreement1_1_B = Factory.New<LicenseAgreement>();
			localAgreement1_1_B.LAG_Status = LicenseAgreementStatusList.Codes.Cancelled;
			localAgreement1_1_B.LAG_Type = "CWN";
			localAgreement1_1_B.LAG_MajorVersion = "1";
			localAgreement1_1_B.LAG_MinorVersion = "1";
			localAgreement1_1_B.LAG_VariantCode = "B";

			Factory.Save();

			using (ObjectFactory.Substitute(nameof(IUserPortalClient), mock.Object))
			{
				RunTask();

				var agreements = Factory.Load<LicenseAgreement>(new ZQuery { ReLoadExistingRows = true });
				AssertEquals(2, agreements.Length);
				var localAgreement1_2_B = agreements.Single(l => l.LAG_MinorVersion == "2");
				AssertNotNull(localAgreement1_2_B);
				AssertEquals(LicenseAgreementStatusList.Codes.Pending, localAgreement1_2_B.LAG_Status);
			}
		}

		public void TestNewAgreement_NewVariant_Case3()
		{
			//case 3: 1.1 Variant B accepted, downloaded 1.2 Variant B	=> not require to sign
			var acceptancesResponse = GetAcceptances();
			var agreementResponse = GetUserAgreementResponse(majorVersion: 1, minorVersion: 2, variant: "B");

			var mock = new Mock<IUserPortalClient>();
			mock.Setup(m => m.GetAcceptancesAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(acceptancesResponse));
			mock.Setup(m => m.GetUserAgreementAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(agreementResponse));

			var localAgreement1_1_B = Factory.New<LicenseAgreement>();
			localAgreement1_1_B.LAG_Status = LicenseAgreementStatusList.Codes.Accepted;
			localAgreement1_1_B.LAG_Type = "CWN";
			localAgreement1_1_B.LAG_MajorVersion = "1";
			localAgreement1_1_B.LAG_MinorVersion = "1";
			localAgreement1_1_B.LAG_VariantCode = "B";

			Factory.Save();

			using (ObjectFactory.Substitute(nameof(IUserPortalClient), mock.Object))
			{
				RunTask();

				var agreements = Factory.Load<LicenseAgreement>(new ZQuery { ReLoadExistingRows = true });
				AssertEquals(2, agreements.Length);
				var localAgreement1_2_B = agreements.Single(l => l.LAG_MinorVersion == "2");
				AssertNotNull(localAgreement1_2_B);
				AssertEquals(LicenseAgreementStatusList.Codes.Cancelled, localAgreement1_2_B.LAG_Status);
			}
		}

		public void TestAcceptAgreement()
		{
			var mock = new Mock<IUserPortalClient>();
			mock.Setup(m => m.GetAcceptancesAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(GetAcceptances()));
			mock.Setup(m => m.GetUserAgreementAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(GetUserAgreementResponse(1)));

			mock.Setup(m => m.SignAgreementAsync(It.IsAny<IUserAgreementSignerDetails>()))
				.Returns(Task.FromResult(new TrustedResponse<bool>() { Success = true }));
			using (ObjectFactory.Substitute(mock.Object))
			{
				RunTask();
				var agreements1 = Factory.Load<LicenseAgreement>(new ZQuery { ReLoadExistingRows = true });
				AssertEquals(1, agreements1.Length);
				agreements1.Single().LAG_Status = LicenseAgreementStatusList.Codes.Queued;
				agreements1.Single().LAG_AcceptedTimeUtc = ZDateTime.UtcNow;
				Factory.Save();

				RunTask();
				var agreements2 = Factory.Load<LicenseAgreement>(new ZQuery { ReLoadExistingRows = true });
				AssertEquals(1, agreements2.Length);
				AssertEquals(LicenseAgreementStatusList.Codes.Accepted, agreements1.Single().LAG_Status);
			}
		}

		public void TestNoUserAgreementFound()
		{
			var mock = new Mock<IUserPortalClient>();
			mock.SetupSequence(m => m.GetAcceptancesAsync(It.IsAny<string>()))
				.Returns(() => Task.FromResult(GetAcceptances()));
			mock.Setup(m => m.GetUserAgreementAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(GetUserAgreementNullResponse()));

			using (ObjectFactory.Substitute(mock.Object))
			{
				RunTask();
				var agreements1 = Factory.Load<LicenseAgreement>(new ZQuery { ReLoadExistingRows = true });
				AssertEquals(0, agreements1.Length);
			}
		}

		public void TestRunMultipleTimes()
		{
			var mock = new Mock<IUserPortalClient>();
			mock.Setup(m => m.GetAcceptancesAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(GetAcceptances()));
			mock.Setup(m => m.GetUserAgreementAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(GetUserAgreementResponse(1)));

			using (ObjectFactory.Substitute(nameof(IUserPortalClient), mock.Object))
			{
				RunTask();
				RunTask();
				RunTask();

				var agreements = Factory.Load<LicenseAgreement>(new ZQuery());
				AssertEquals(1, agreements.Length);
			}
		}

		public void TestCancelPendingAgreements_OnNewMajorVersion()
		{
			var mock = new Mock<IUserPortalClient>();
			mock.SetupSequence(m => m.GetAcceptancesAsync(It.IsAny<string>()))
				.Returns(() => Task.FromResult(GetAcceptances()));
			mock.SetupSequence(m => m.GetUserAgreementAsync(It.IsAny<string>()))
				.Returns(() => Task.FromResult(GetUserAgreementResponse(1)));

			using (ObjectFactory.Substitute(mock.Object))
			{
				RunTask();

				mock.SetupSequence(m => m.GetAcceptancesAsync(It.IsAny<string>()))
					.Returns(() => Task.FromResult(GetAcceptances()));
				mock.SetupSequence(m => m.GetUserAgreementAsync(It.IsAny<string>()))
					.Returns(() => Task.FromResult(GetUserAgreementResponse(2)));

				var agreements1 = Factory.Load<LicenseAgreement>(new ZQuery { ReLoadExistingRows = true });
				AssertEquals(1, agreements1.Length);

				RunTask();
				var agreements2 = Factory.Load<LicenseAgreement>(new ZQuery { ReLoadExistingRows = true });
				AssertEquals(2, agreements2.Length);

				mock.SetupSequence(m => m.GetAcceptancesAsync(It.IsAny<string>()))
					.Returns(() => Task.FromResult(GetAcceptances()));
				mock.SetupSequence(m => m.GetUserAgreementAsync(It.IsAny<string>()))
					.Returns(() => Task.FromResult(GetUserAgreementResponse(3)));

				RunTask();
				var agreements3 = Factory.Load<LicenseAgreement>(new ZQuery { ReLoadExistingRows = true });
				AssertEquals(3, agreements3.Length);
				AssertEquals(2, agreements2.Count(a => a.LAG_Status == LicenseAgreementStatusList.Codes.Cancelled));
				AssertEquals("3.0", agreements3.Single(a => a.LAG_Status == LicenseAgreementStatusList.Codes.Pending).VersionNumber);
			}
		}

		public void TestCancelPendingAgreements_OnNewVariant()
		{
			//case 1: 1.1 Variant A accepted, 1.0 Variant B pending, downloaded not required	=> 1.0 Variant B cancelled, not require to sign

			var acceptancesResponse = GetAcceptances();
			var agreementResponse = GetUserAgreementResponse(versionNumber: 0, isRequired: false);

			var mock = new Mock<IUserPortalClient>();
			mock.Setup(m => m.GetAcceptancesAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(acceptancesResponse));
			mock.Setup(m => m.GetUserAgreementAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(agreementResponse));

			var localAgreement1_1_A = Factory.New<LicenseAgreement>();
			localAgreement1_1_A.LAG_Status = LicenseAgreementStatusList.Codes.Accepted;
			localAgreement1_1_A.LAG_Type = "CWN";
			localAgreement1_1_A.LAG_MajorVersion = "1";
			localAgreement1_1_A.LAG_MinorVersion = "1";
			localAgreement1_1_A.LAG_VariantCode = "A";

			var localAgreement1_0_B = Factory.New<LicenseAgreement>();
			localAgreement1_0_B.LAG_Status = LicenseAgreementStatusList.Codes.Pending;
			localAgreement1_0_B.LAG_Type = "CWN";
			localAgreement1_0_B.LAG_MajorVersion = "1";
			localAgreement1_0_B.LAG_MinorVersion = "0";
			localAgreement1_0_B.LAG_VariantCode = "B";

			Factory.Save();

			using (ObjectFactory.Substitute(nameof(IUserPortalClient), mock.Object))
			{
				RunTask();

				var agreements = Factory.Load<LicenseAgreement>(new ZQuery { ReLoadExistingRows = true });
				AssertEquals(2, agreements.Length);
				localAgreement1_0_B.Reload();
				AssertEquals(LicenseAgreementStatusList.Codes.Cancelled, localAgreement1_0_B.LAG_Status);
			}
		}

		public void TestResumeCancelledAgreements_OnNewVariant_Case1()
		{
			//case 1: 1.1 Variant A accepted, 1.0 Variant B cancelled, downloaded 1.0 Variant B	=> require to sign 1.0 Variant B

			var acceptancesResponse = GetAcceptances();
			var agreementResponse = GetUserAgreementResponse(majorVersion: 1, minorVersion: 0, variant: "B");
			var agreementResponseData = agreementResponse.Response;

			var mock = new Mock<IUserPortalClient>();
			mock.Setup(m => m.GetAcceptancesAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(acceptancesResponse));
			mock.Setup(m => m.GetUserAgreementAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(agreementResponse));

			var localAgreement1_1_A = Factory.New<LicenseAgreement>();
			localAgreement1_1_A.LAG_Status = LicenseAgreementStatusList.Codes.Accepted;
			localAgreement1_1_A.LAG_Type = "CWN";
			localAgreement1_1_A.LAG_MajorVersion = "1";
			localAgreement1_1_A.LAG_MinorVersion = "1";
			localAgreement1_1_A.LAG_VariantCode = "A";

			var localAgreement1_0_B = Factory.New<LicenseAgreement>();
			localAgreement1_0_B.LAG_Status = LicenseAgreementStatusList.Codes.Cancelled;
			localAgreement1_0_B.LAG_Type = "CWN";
			localAgreement1_0_B.LAG_MajorVersion = "1";
			localAgreement1_0_B.LAG_MinorVersion = "0";
			localAgreement1_0_B.LAG_VariantCode = "B";

			Factory.Save();

			using (ObjectFactory.Substitute(nameof(IUserPortalClient), mock.Object))
			{
				RunTask();

				var agreements = Factory.Load<LicenseAgreement>(new ZQuery { ReLoadExistingRows = true });
				AssertEquals(2, agreements.Length);
				localAgreement1_0_B.Reload();
				AssertEquals(LicenseAgreementStatusList.Codes.Pending, localAgreement1_0_B.LAG_Status);
				AssertEquals(agreementResponseData.Title, localAgreement1_0_B.LAG_Title);
				AssertEquals(agreementResponseData.Content, localAgreement1_0_B.LAG_Content);
			}
		}

		public void TestResumeCancelledAgreements_OnNewVariant_Case2()
		{
			//case 2: 1.1 Variant A accepted, 1.0 Variant B cancelled, downloaded 1.1 Variant B	=> require to sign 1.1 Variant B

			var acceptancesResponse = GetAcceptances();
			var agreementResponse = GetUserAgreementResponse(majorVersion: 1, minorVersion: 1, variant: "B");

			var mock = new Mock<IUserPortalClient>();
			mock.Setup(m => m.GetAcceptancesAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(acceptancesResponse));
			mock.Setup(m => m.GetUserAgreementAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(agreementResponse));

			var localAgreement1_1_A = Factory.New<LicenseAgreement>();
			localAgreement1_1_A.LAG_Status = LicenseAgreementStatusList.Codes.Accepted;
			localAgreement1_1_A.LAG_Type = "CWN";
			localAgreement1_1_A.LAG_MajorVersion = "1";
			localAgreement1_1_A.LAG_MinorVersion = "1";
			localAgreement1_1_A.LAG_VariantCode = "A";

			var localAgreement1_0_B = Factory.New<LicenseAgreement>();
			localAgreement1_0_B.LAG_Status = LicenseAgreementStatusList.Codes.Cancelled;
			localAgreement1_0_B.LAG_Type = "CWN";
			localAgreement1_0_B.LAG_MajorVersion = "1";
			localAgreement1_0_B.LAG_MinorVersion = "0";
			localAgreement1_0_B.LAG_VariantCode = "B";

			Factory.Save();

			using (ObjectFactory.Substitute(nameof(IUserPortalClient), mock.Object))
			{
				RunTask();

				var agreements = Factory.Load<LicenseAgreement>(new ZQuery { ReLoadExistingRows = true });
				AssertEquals(3, agreements.Length);
				var localAgreement1_1_B = agreements.Single(l => l.LAG_MinorVersion == "1" && l.LAG_VariantCode == "B");
				AssertNotNull(localAgreement1_1_B);
				AssertEquals(LicenseAgreementStatusList.Codes.Pending, localAgreement1_1_B.LAG_Status);
			}
		}

		public void TestResumeCancelledAgreements_OnNewVariant_Case3()
		{
			//case 3: 1.1 Variant A accepted, 1.0 Variant B cancelled, 1.1 Variant B cancelled, downloaded 1.1 Variant B	=> require to sign 1.1 Variant B

			var acceptancesResponse = GetAcceptances();
			var agreementResponse = GetUserAgreementResponse(majorVersion: 1, minorVersion: 1, variant: "B");
			var agreementResponseData = agreementResponse.Response;

			var mock = new Mock<IUserPortalClient>();
			mock.Setup(m => m.GetAcceptancesAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(acceptancesResponse));
			mock.Setup(m => m.GetUserAgreementAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(agreementResponse));

			var localAgreement1_1_A = Factory.New<LicenseAgreement>();
			localAgreement1_1_A.LAG_Status = LicenseAgreementStatusList.Codes.Accepted;
			localAgreement1_1_A.LAG_Type = "CWN";
			localAgreement1_1_A.LAG_MajorVersion = "1";
			localAgreement1_1_A.LAG_MinorVersion = "1";
			localAgreement1_1_A.LAG_VariantCode = "A";

			var localAgreement1_0_B = Factory.New<LicenseAgreement>();
			localAgreement1_0_B.LAG_Status = LicenseAgreementStatusList.Codes.Cancelled;
			localAgreement1_0_B.LAG_Type = "CWN";
			localAgreement1_0_B.LAG_MajorVersion = "1";
			localAgreement1_0_B.LAG_MinorVersion = "0";
			localAgreement1_0_B.LAG_VariantCode = "B";

			var localAgreement1_1_B = Factory.New<LicenseAgreement>();
			localAgreement1_1_B.LAG_Status = LicenseAgreementStatusList.Codes.Cancelled;
			localAgreement1_1_B.LAG_Type = "CWN";
			localAgreement1_1_B.LAG_MajorVersion = "1";
			localAgreement1_1_B.LAG_MinorVersion = "1";
			localAgreement1_1_B.LAG_VariantCode = "B";

			Factory.Save();

			using (ObjectFactory.Substitute(nameof(IUserPortalClient), mock.Object))
			{
				RunTask();

				var agreements = Factory.Load<LicenseAgreement>(new ZQuery { ReLoadExistingRows = true });
				AssertEquals(3, agreements.Length);
				localAgreement1_1_B.Reload();
				AssertEquals(LicenseAgreementStatusList.Codes.Pending, localAgreement1_1_B.LAG_Status);
				AssertEquals(agreementResponseData.Title, localAgreement1_1_B.LAG_Title);
				AssertEquals(agreementResponseData.Content, localAgreement1_1_B.LAG_Content);
			}
		}

		public void TestResumeCancelledAgreements_OnNewVariant_Case4()
		{
			//case 4: 1.0 Variant A accepted, 1.1 Variant A cancelled, downloaded 1.1 Variant A	=> not require to sign

			var acceptancesResponse = GetAcceptances();
			var agreementResponse = GetUserAgreementResponse(majorVersion: 1, minorVersion: 1, variant: "A");
			var agreementResponseData = agreementResponse.Response;

			var mock = new Mock<IUserPortalClient>();
			mock.Setup(m => m.GetAcceptancesAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(acceptancesResponse));
			mock.Setup(m => m.GetUserAgreementAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(agreementResponse));

			var localAgreement1_0_A = Factory.New<LicenseAgreement>();
			localAgreement1_0_A.LAG_Status = LicenseAgreementStatusList.Codes.Accepted;
			localAgreement1_0_A.LAG_Type = "CWN";
			localAgreement1_0_A.LAG_MajorVersion = "1";
			localAgreement1_0_A.LAG_MinorVersion = "0";
			localAgreement1_0_A.LAG_VariantCode = "A";

			var localAgreement1_1_A = Factory.New<LicenseAgreement>();
			localAgreement1_1_A.LAG_Status = LicenseAgreementStatusList.Codes.Cancelled;
			localAgreement1_1_A.LAG_Type = "CWN";
			localAgreement1_1_A.LAG_MajorVersion = "1";
			localAgreement1_1_A.LAG_MinorVersion = "1";
			localAgreement1_1_A.LAG_VariantCode = "A";

			Factory.Save();

			using (ObjectFactory.Substitute(nameof(IUserPortalClient), mock.Object))
			{
				RunTask();

				var agreements = Factory.Load<LicenseAgreement>(new ZQuery { ReLoadExistingRows = true });
				AssertEquals(2, agreements.Length);
				localAgreement1_1_A.Reload();
				AssertEquals(LicenseAgreementStatusList.Codes.Cancelled, localAgreement1_1_A.LAG_Status);
			}
		}

		public void TestDontCancelQueuedAgreements()
		{
			var mock = new Mock<IUserPortalClient>();
			mock.SetupSequence(m => m.GetAcceptancesAsync(It.IsAny<string>()))
				.Returns(() => Task.FromResult(GetAcceptances()));
			mock.Setup(m => m.GetUserAgreementAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(GetUserAgreementResponse(1)));

			mock.Setup(m => m.SignAgreementAsync(It.IsAny<IUserAgreementSignerDetails>()))
				.Returns(Task.FromResult(new TrustedResponse<bool>() { Success = true }));

			using (ObjectFactory.Substitute(mock.Object))
			{
				RunTask();
				var agreements1 = Factory.Load<LicenseAgreement>(new ZQuery { ReLoadExistingRows = true });
				AssertEquals(1, agreements1.Length);
				agreements1.Single().LAG_Status = LicenseAgreementStatusList.Codes.Queued;
				agreements1.Single().LAG_AcceptedTimeUtc = ZDateTime.UtcNow;
				Factory.Save();

				mock.SetupSequence(m => m.GetAcceptancesAsync(It.IsAny<string>()))
					.Returns(() => Task.FromResult(GetAcceptances()));
				mock.Setup(m => m.GetUserAgreementAsync(It.IsAny<string>()))
					.Returns(Task.FromResult(GetUserAgreementResponse(2)));

				RunTask();
				var agreements2 = Factory.Load<LicenseAgreement>(new ZQuery { ReLoadExistingRows = true });
				AssertEquals(2, agreements2.Length);
				AssertEquals(LicenseAgreementStatusList.Codes.Accepted, agreements1.Single().LAG_Status);
			}
		}

		public void TestNoCertificates()
		{
			var mock = new Mock<IUserPortalClient>();

			using (ObjectFactory.Substitute(mock.Object))
			{
				var log = new StringBuilder();

				mock.SetupSequence(m => m.GetAcceptancesAsync(It.IsAny<string>()))
					.Returns(() => Task.FromResult(GetAcceptances()));
				mock.SetupSequence(m => m.GetUserAgreementAsync(It.IsAny<string>()))
					.Returns(() => Task.FromResult(GetUserAgreementResponse(1, 2, "A")));
				log.AppendLine(RunTask(false));

				mock.SetupSequence(m => m.GetAcceptancesAsync(It.IsAny<string>()))
					.Returns(() => Task.FromResult(GetAcceptances()));
				mock.SetupSequence(m => m.GetUserAgreementAsync(It.IsAny<string>()))
				.Returns(() => Task.FromResult(GetUserAgreementResponse(1, 2, "A")));
				log.AppendLine(RunTask(true));

				var agreements = Factory.Load<LicenseAgreement>(new ZQuery());
				AssertEquals(1, agreements.Length);

				AssertEquals(@"Debug|Start
Debug|End - The certificates have not yet been downloaded by the TMS service task.

Debug|Start
Debug|GetAcceptances Request succeeded
Debug|GetUserAgreement Request succeeded
Information|Downloaded new license. Version number: 1.2, Variant: A
Debug|End

", log.ToString());
			}
		}

		public void TestCancelPendingAndQueuedAgreements_OnEnterpriseAcceptance()
		{
			var mock = new Mock<IUserPortalClient>();
			mock.SetupSequence(m => m.GetAcceptancesAsync(It.IsAny<string>()))
				.Returns(() => Task.FromResult(GetAcceptances()));
			mock.SetupSequence(m => m.GetUserAgreementAsync(It.IsAny<string>()))
				.Returns(() => Task.FromResult(GetUserAgreementResponse(1, 2, "A")));

			using (ObjectFactory.Substitute(mock.Object))
			{
				RunTask();

				var acceptance = MakeAcceptance(1, 2, "A");
				mock.SetupSequence(m => m.GetAcceptancesAsync(It.IsAny<string>()))
					.Returns(() => Task.FromResult(GetAcceptances(acceptance)));
				mock.SetupSequence(m => m.GetUserAgreementAsync(It.IsAny<string>()))
					.Returns(() => Task.FromResult(GetUserAgreementResponse(1, 2, "A")));

				var agreements1 = Factory.Load<LicenseAgreement>(new ZQuery { ReLoadExistingRows = true });
				AssertEquals(1, agreements1.Length);

				RunTask();
				var agreements2 = Factory.Load<LicenseAgreement>(new ZQuery { ReLoadExistingRows = true });
				AssertEquals(2, agreements2.Length);

				mock.SetupSequence(m => m.GetAcceptancesAsync(It.IsAny<string>()))
					.Returns(() => Task.FromResult(GetAcceptances(acceptance)));
				mock.SetupSequence(m => m.GetUserAgreementAsync(It.IsAny<string>()))
					.Returns(() => Task.FromResult(GetUserAgreementResponse(1, 2, "A")));

				RunTask();
				var agreements3 = Factory.Load<LicenseAgreement>(new ZQuery { ReLoadExistingRows = true });
				AssertEquals(2, agreements3.Length);
				var accepted = agreements3.Single(a => a.LAG_Status == LicenseAgreementStatusList.Codes.Accepted);
				AssertEquals(acceptance.AcceptedByName, accepted.LAG_AcceptedByName);
				AssertEquals(acceptance.Title, accepted.LAG_Title);
				AssertEquals(acceptance.Content, accepted.LAG_Content);
				AssertEquals(acceptance.AcceptedByEmail, accepted.LAG_AcceptedByEmail);
				AssertEquals(acceptance.MajorVersion, accepted.LAG_MajorVersion);
				AssertEquals(acceptance.MinorVersion, accepted.LAG_MinorVersion);

				mock.SetupSequence(m => m.GetAcceptancesAsync(It.IsAny<string>()))
					.Returns(() => Task.FromResult(GetAcceptances(acceptance)));
				mock.SetupSequence(m => m.GetUserAgreementAsync(It.IsAny<string>()))
					.Returns(() => Task.FromResult(GetUserAgreementResponse(versionNumber: 0, isRequired: false)));

				RunTask();
				
				var agreements4 = Factory.Load<LicenseAgreement>(new ZQuery { ReLoadExistingRows = true });
				AssertEquals(1, agreements4.Count(a => a.LAG_Status == LicenseAgreementStatusList.Codes.Cancelled));
			}
		}

		public void TestDuplicateAcceptances()
		{
			var acceptance = MakeAcceptance(1, 2, "A");
			var mock = new Mock<IUserPortalClient>();

			mock.SetupSequence(m => m.GetUserAgreementAsync(It.IsAny<string>()))
				.Returns(() => Task.FromResult(GetUserAgreementResponse(1, 2, "A")));
			mock.SetupSequence(m => m.GetAcceptancesAsync(It.IsAny<string>()))
				.Returns(() => Task.FromResult(GetAcceptances(acceptance)));

			using (ObjectFactory.Substitute(mock.Object))
			{
				RunTask();

				mock.SetupSequence(m => m.GetUserAgreementAsync(It.IsAny<string>()))
					.Returns(() => Task.FromResult(GetUserAgreementResponse(1, 2, "A")));
				mock.SetupSequence(m => m.GetAcceptancesAsync(It.IsAny<string>()))
					.Returns(() => Task.FromResult(GetAcceptances(acceptance)));

				RunTask();

				var agreements1 = Factory.Load<LicenseAgreement>(new ZQuery { ReLoadExistingRows = true });
				AssertEquals(1, agreements1.Length);
				var accepted = agreements1[0];
				AssertEquals(acceptance.AcceptedByName, accepted.LAG_AcceptedByName);
				AssertEquals(acceptance.Title, accepted.LAG_Title);
				AssertEquals(acceptance.Content, accepted.LAG_Content);
				AssertEquals(acceptance.AcceptedByEmail, accepted.LAG_AcceptedByEmail);
				AssertEquals(acceptance.MajorVersion, accepted.LAG_MajorVersion);
				AssertEquals(acceptance.MinorVersion, accepted.LAG_MinorVersion);
			}
		}
	}
}
