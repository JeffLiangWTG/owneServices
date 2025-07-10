using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.GB.Registry.Business.Testing
{
	[TestedType(typeof(CredentialsSetting))]
	public class CredentialsSettingTest : RegistryBusinessObjectTemplateTestCase<CredentialsSetting>
	{
		class MockedICspDownloadResult : ICspDownloadResult
		{
			public string errorText { get; set; }
			public string[] MessagesArray { get; set; }
			public int batchId { get; set; }
		}

		static class ErrorMessages
		{
			public const string CompanyOrBadgeMustBeThreeCharacters = "Badge/Company code must be 3 characters";
			public const string CompanyRequiredForMcpAndCns = "Company code is required if CSP is MCP or CNS";
			public const string CompanyShouldStartWithTHSForNes = "start with 'THS";
			public const string PrinterMustStartWithCUKForCcsuk = "e.g. 'CUK";
			public const string PrinterShouldStartWithLOCEDCForNes = "start with 'LOCEDC";
			public const string PrinterUsuallyEndsWithMLBXForCns = "ends with 'MLBX'";
			public const string PrinterUsuallyEndsWith9ForMcp = "ends with '9'";
			public const string UsernameCannotBeEmpty = "Username cannot be empty";
			public const string UsernameUsuallyEndsWithCCMIForCns = "end with 'CCMI'";
			public const string UsernameUsuallyEndsWith8OrTForMcp = "end with '8' (live) or 'T' (test)";
			public const string PasswordCannotBeEmpty = "Password cannot be empty";
			public const string PasswordUsuallyIsCCMIPWDForCns = "usually 'CCMIPWD";
		}

		public void TestCrossBranchVisibilityAndValidation()
		{
			var branchA = Factory.NewWithValidTestData<GlbBranch>();
			var branchB = Factory.NewWithValidTestData<GlbBranch>();
			var comp = GlbCompany.CurrentCompany;
			comp.Branches.AddRange(new[] { branchA, branchB });
			branchA.GB_GC = comp.PK;
			branchB.GB_GC = comp.PK;
			Factory.Save();
			var collectionA = GBCustomsDataRegistry.Instance.BadgeCodes.GetValueWithoutFallback(Guid.Empty, branchA.PK.ToGuid(), Guid.Empty);
			var badgeA = collectionA.AddNew();
			badgeA.BadgeCode = "ABC";
			badgeA.CSPCode = GatewayList.Codes.MCP_CUSDECOnly;
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, branchA.PK.ToGuid(), Guid.Empty, collectionA);
			var credentials = GBCustomsDataRegistry.Instance.Credentials.GetValueWithoutFallback(comp.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var cred = credentials.AddNew();
			cred.BadgeCode = "ABC";
			cred.Company = "DEF";
			cred.Username = "X";
			cred.Password = "Y";
			cred.Printer = "Z";
			GBCustomsDataRegistry.Instance.Credentials.SetValue(comp.PK.ToGuid(), Guid.Empty, Guid.Empty, credentials);

			using (DisposableEnvironment.ForBranch(branchB.PK.ToGuid()))
			{
				credentials = GBCustomsDataRegistry.Instance.Credentials.GetValueWithoutFallback(comp.PK.ToGuid(), Guid.Empty, Guid.Empty);
				var credToCheck = credentials[0];
				AssertEquals("MCP", credToCheck.CSP);
				credToCheck.DataTestStatus = "XXX";
				AssertEquals(false, credToCheck.DataTestStatusInfo.Notifications.Any());
			}
		}

		public void TestDataTestStatus()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlbStaff.CurrentUser.GS_Code = user.GS_Code;
			var mockedResultOK = new MockedICspDownloadResult() { MessagesArray = new string[] { "A", "B" } };
			var mockedResultError = new MockedICspDownloadResult() { errorText = "Some Error Meaning You Suck" };
			var mockCns = new Mock<ICspPrintsMailBoxProvider>();
			var objectSubstitutionCns = ObjectFactory.Substitute(mockCns.Object);
			mockCns.SetupSequence(m => m.checkCdsCredentials(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(mockedResultOK)
				.Throws(new Exception("HTTP 401 some unauthorized error"))
				.Returns(mockedResultOK)
				.Returns(mockedResultError);
			var badge = new BadgeCodeSetting();
			badge.ApplicationCode = "CDS";
			badge.BadgeCode = "DAN";
			badge.CSPCode = "CNS";
			var badges = new BadgeCodeSettingCollection();
			badges.Add(badge);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);

			var credential = new CredentialsSetting();
			credential.WebServiceFailureCount = 99;
			AssertEquals(DataTestStatusList.Codes.OldBlankNotTested, credential.DataTestStatus);
			credential.BadgeCode = badge.BadgeCode;
			AssertEquals(DataTestStatusList.Codes.Untested, credential.DataTestStatus);
			var notifier = UnitTestUserNotification.Instance;
			credential.CheckCredentialsAgainstCspWebService(notifier);
			AssertEquals(DataTestStatusList.Codes.Valid, credential.DataTestStatus);
			AssertEquals(0, credential.WebServiceFailureCount);
			credential.WebServiceFailureCount = 99;
			credential.CheckCredentialsAgainstCspWebService(notifier);
			AssertEquals(DataTestStatusList.Codes.Invalid, credential.DataTestStatus);
			AssertEquals(99, credential.WebServiceFailureCount);
			credential.CheckCredentialsAgainstCspWebService(notifier);
			AssertEquals(DataTestStatusList.Codes.Valid, credential.DataTestStatus);
			credential.CheckCredentialsAgainstCspWebService(notifier);
			AssertEquals(DataTestStatusList.Codes.Invalid, credential.DataTestStatus);

			credential.DataTestStatus = DataTestStatusList.Codes.Valid;
			credential.Company = "V";
			AssertEquals(DataTestStatusList.Codes.Untested, credential.DataTestStatus);
			credential.DataTestStatus = DataTestStatusList.Codes.Valid;
			credential.Username = "W";
			AssertEquals(DataTestStatusList.Codes.Untested, credential.DataTestStatus);
			credential.DataTestStatus = DataTestStatusList.Codes.Valid;
			credential.Password = "X";
			AssertEquals(DataTestStatusList.Codes.Untested, credential.DataTestStatus);
			credential.DataTestStatus = DataTestStatusList.Codes.Valid;
			credential.Printer = "Y";
			AssertEquals(DataTestStatusList.Codes.Untested, credential.DataTestStatus);
			credential.DataTestStatus = DataTestStatusList.Codes.Valid;
			credential.FallbackForShed = "Z";
			AssertEquals("This field doesn't mark as dirty", DataTestStatusList.Codes.Valid, credential.DataTestStatus);
			mockCns.VerifyAll();
		}

		public void TestDataTestStatusValidationAndEditing()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlbStaff.CurrentUser.GS_IsDeveloper = false;
			GlbStaff.CurrentUser.GS_LoginName = user.GS_LoginName;
			GlbStaff.CurrentUser.GS_Code = user.GS_Code;
			var badge = new BadgeCodeSetting();
			badge.BadgeCode = "DAN";
			badge.CSPCode = "CNS";
			var badges = new BadgeCodeSettingCollection();
			badges.Add(badge);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
			var credential = new CredentialsSetting();
			credential.BadgeCode = badge.BadgeCode;

			AssertEquals(true, credential.DataTestStatusInfo.ReadOnly);

			credential.DataTestStatus = DataTestStatusList.Codes.Invalid;
			AssertHasErrorContaining(credential.DataTestStatusInfo, "not been proved correct");
			credential.DataTestStatus = DataTestStatusList.Codes.OldBlankNotTested;
			AssertHasErrorContaining(credential.DataTestStatusInfo, "not been proved correct");
			credential.DataTestStatus = DataTestStatusList.Codes.Untested;
			AssertHasErrorContaining(credential.DataTestStatusInfo, "not been proved correct");
			credential.DataTestStatus = DataTestStatusList.Codes.Valid;
			AssertNoErrorContaining(credential.DataTestStatusInfo, "not been proved correct");
			AssertNoErrorContaining(credential.DataTestStatusInfo, "MCP and CNS");

			badge.CSPCode = "MCP";
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
			AssertNoErrorContaining(credential.DataTestStatusInfo, "not been proved correct");
			credential.DataTestStatus = DataTestStatusList.Codes.Invalid;
			AssertHasErrorContaining(credential.DataTestStatusInfo, "not been proved correct");
			credential.DataTestStatus = DataTestStatusList.Codes.OldBlankNotTested;
			AssertHasErrorContaining(credential.DataTestStatusInfo, "not been proved correct");
			credential.DataTestStatus = DataTestStatusList.Codes.Untested;
			AssertHasErrorContaining(credential.DataTestStatusInfo, "not been proved correct");
			credential.DataTestStatus = DataTestStatusList.Codes.Valid;
			AssertNoErrorContaining(credential.DataTestStatusInfo, "not been proved correct");
			AssertNoErrorContaining(credential.DataTestStatusInfo, "MCP and CNS");

			badge.CSPCode = "CCSUK";
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
			AssertNoErrorContaining(credential.DataTestStatusInfo, "not been proved correct");
			credential.DataTestStatus = DataTestStatusList.Codes.Invalid;
			AssertNoErrorContaining(credential.DataTestStatusInfo, "not been proved correct");
			credential.DataTestStatus = DataTestStatusList.Codes.OldBlankNotTested;
			AssertNoErrorContaining(credential.DataTestStatusInfo, "not been proved correct");
			credential.DataTestStatus = DataTestStatusList.Codes.Untested;
			AssertNoErrorContaining(credential.DataTestStatusInfo, "not been proved correct");
			credential.DataTestStatus = DataTestStatusList.Codes.Valid;
			AssertNoErrorContaining(credential.DataTestStatusInfo, "not been proved correct");
			AssertHasErrorContaining(credential.DataTestStatusInfo, "MCP and CNS");

			GlbStaff.CurrentUser.GS_Code = "~E";
			GlbStaff.CurrentUser.GS_LoginName = User.SupportUserName;
			badge.CSPCode = "MCP";
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
			AssertEquals(false, credential.DataTestStatusInfo.ReadOnly);
			credential.DataTestStatus = DataTestStatusList.Codes.Invalid;
			AssertNoErrorContaining(credential.DataTestStatusInfo, "not been proved correct");
			credential.DataTestStatus = DataTestStatusList.Codes.OldBlankNotTested;
			AssertNoErrorContaining(credential.DataTestStatusInfo, "not been proved correct");
			credential.DataTestStatus = DataTestStatusList.Codes.Untested;
			AssertNoErrorContaining(credential.DataTestStatusInfo, "not been proved correct");
			credential.DataTestStatus = DataTestStatusList.Codes.Valid;
			AssertNoErrorContaining(credential.DataTestStatusInfo, "not been proved correct");
		}

		public void TestCheckCredentials()
		{
			var mockedResult = new MockedICspDownloadResult() { MessagesArray = new string[] { "A", "B" } };
			var mockCns = new Mock<ICspPrintsMailBoxProvider>();
			var objectSubstitutionCns = ObjectFactory.Substitute(mockCns.Object);
			mockCns.Setup(m => m.checkCdsCredentials(It.IsAny<string>(), It.IsAny<string>())).Returns(mockedResult);
			var badge = new BadgeCodeSetting();
			badge.BadgeCode = "DAN";
			badge.CSPCode = "CNS";
			badge.ApplicationCode = "CDS";
			var badges = new BadgeCodeSettingCollection();
			badges.Add(badge);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
			var credential = new CredentialsSetting();
			credential.BadgeCode = badge.BadgeCode;
			var notifier = UnitTestUserNotification.Instance;
			credential.CheckCredentialsAgainstCspWebService(notifier);
			AssertContains("CSP confirmed that the credentials are known, and that the topic is registered to: \r\nA", notifier.LastMessage.Text);
			badge.CSPCode = "CCSUK";
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
			credential.CheckCredentialsAgainstCspWebService(notifier);
			AssertContains("Validation is not possible for these credentials", notifier.LastMessage.Text);
			badge.CSPCode = "NES";
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
			credential.CheckCredentialsAgainstCspWebService(notifier);
			AssertContains("Validation is not possible for these credentials", notifier.LastMessage.Text);
			mockCns.VerifyAll();
		}

		public void TestCheckCredentialsCds()
		{
			var mockedResult = new MockedICspDownloadResult() { MessagesArray = new string[] { "https://registered.url" } };
			var mockCns = new Mock<ICspPrintsMailBoxProvider>();
			var objectSubstitutionCns = ObjectFactory.Substitute(mockCns.Object);
			mockCns.Setup(m => m.checkCdsCredentials(It.IsAny<string>(), It.IsAny<string>())).Returns(mockedResult);
			var badge = new BadgeCodeSetting();
			badge.ApplicationCode = "CDS";
			badge.BadgeCode = "JAN";
			badge.CSPCode = "CNS";
			var badges = new BadgeCodeSettingCollection();
			badges.Add(badge);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
			var credential = new CredentialsSetting();
			credential.BadgeCode = badge.BadgeCode;
			var notifier = UnitTestUserNotification.Instance;
			credential.CheckCredentialsAgainstCspWebService(notifier);
			AssertContains("CSP confirmed that the credentials are known, and that the topic is registered to: \r\nhttps://registered.url", notifier.LastMessage.Text);
			mockCns.VerifyAll();
		}

		public void TestPentantEndpoint()
		{
			var badges = GBCustomsDataRegistry.Instance.BadgeCodes.Value;
			var badge = badges.AddNew();
			badge.BadgeCode = "ABC";
			badge.CSPCode = "PNT";
			var badge2 = badges.AddNew();
			badge2.BadgeCode = "DEF";
			badge2.CSPCode = "MCP";

			var creds = GBCustomsDataRegistry.Instance.Credentials.Value;
			var cred = creds.AddNew();
			cred.BadgeCode = badge.BadgeCode;
			cred.Username = "1";
			cred.Password = "2";
			cred.Printer = "wisetechFolderName";
			cred.SenderID = "SSSS";
			cred.ReceiverID = "RRRRR";
			cred.Company = "WTG";
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);
			GBCustomsDataRegistry.Instance.Credentials.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, creds);

			AssertHasMessageError(cred.EndpointInfo, "You have not entered a value.");
			cred.Endpoint = "123";
			AssertHasMessageError(cred.EndpointInfo, "The code you have selected is not in the list.");
			cred.Endpoint = EndpointList.Codes.INV;
			AssertNoMessageErrors(cred.EndpointInfo);

			cred.BadgeCode = badge2.BadgeCode;
			cred.Endpoint = "123";
			AssertHasMessageError(cred.EndpointInfo, "The code you have selected is not in the list.");
			cred.Endpoint = "";
			AssertNoMessageErrors(cred.EndpointInfo);
		}

		public void TestWebServiceFailureCountAndIsLoaderFieldsReadOnly()
		{
			var hostedLocation = EnvProxy.HostedLocation;
			var isDev = GlbStaff.CurrentUser.GS_IsDeveloper;
			var loginName = GlbStaff.CurrentUser.GS_LoginName;
			var lastEdit = GlbStaff.CurrentUser.GS_SystemLastEditTimeUtc;
			var cred = new CredentialsSetting();
			try
			{
				GlbStaff.CurrentUser.GS_IsDeveloper = false;
				GlbStaff.CurrentUser.GS_LoginName = "Daniel";
				EnvProxy.SetHostedLocationForTest("");
				Assert("Not hosted - WebServiceFailureCount is editable", !cred.WebServiceFailureCountInfo.ReadOnly);
				Assert("Ordinary user - IsMaritimeLoader is readonly (hosting not relevant)", cred.IsMaritimeLoaderInfo.ReadOnly);
				EnvProxy.SetHostedLocationForTest("London");
				Assert("Hosted, readnly", cred.WebServiceFailureCountInfo.ReadOnly);
				Assert("Ordinary user - IsMaritimeLoader is readonly (hosting not relevant)", cred.IsMaritimeLoaderInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_LoginName = User.SupportUserName;
				Assert("Hosted but support", !cred.WebServiceFailureCountInfo.ReadOnly);
				Assert("ediSupport user - IsMaritimeLoader is editable", !cred.IsMaritimeLoaderInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsDeveloper = isDev;
				GlbStaff.CurrentUser.GS_LoginName = loginName;
				GlbStaff.CurrentUser.GS_SystemLastEditTimeUtc = lastEdit;
				EnvProxy.SetHostedLocationForTest(hostedLocation);
			}
		}

		public void TestIsCcsukShedIsCcskAgentIsDepOperator()
		{
			var badges = new BadgeCodeSettingCollection();
			var ccsukBadge = new BadgeCodeSetting();
			ccsukBadge.CSPCode = Enterprise.Customs.GB.Registry.GatewayList.Codes.CCSUKviaNTMsgGW;
			ccsukBadge.BadgeCode = "ZPE";
			badges.Add(ccsukBadge);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges);

			var cred = new CredentialsSetting();
			cred.BadgeCode = ccsukBadge.BadgeCode;
			cred.Printer = "CUKAIR98000ZZZ";
			cred.Company = "CAR";
			CredentialsSettingCollection allCreds = new CredentialsSettingCollection();
			allCreds.Add(cred);
			GBCustomsDataRegistry.Instance.Credentials.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allCreds);

			Assert(cred.IsCcskShed);
			Assert(!cred.IsCcskAgent);
			Assert(!cred.IsDEPOperator);

			cred.Printer = "CUKFFW98000YYY";
			Assert(!cred.IsCcskShed);
			Assert(cred.IsCcskAgent);
			Assert(!cred.IsDEPOperator);

			cred.Printer = "CUKAIR98LHRXYY";
			Assert(cred.IsCcskShed);
			Assert(!cred.IsCcskAgent);
			Assert(cred.IsDEPOperator);
		}

		public void TestIsPentant()
		{
			var badges = new BadgeCodeSettingCollection();

			var badgePNT = new BadgeCodeSetting();
			badgePNT.CSPCode = GatewayList.Codes.Pentant;
			badgePNT.BadgeCode = "ZXC";
			badges.Add(badgePNT);
			var badgeNonPNT = new BadgeCodeSetting();
			badgeNonPNT.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			badgeNonPNT.BadgeCode = "VBN";
			badges.Add(badgeNonPNT);
			using (GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badges))
			{
				var credPNT = new CredentialsSetting();
				credPNT.BadgeCode = badgePNT.BadgeCode;
				var credNonPNT = new CredentialsSetting();
				credNonPNT.BadgeCode = badgeNonPNT.BadgeCode;

				Assert(credPNT.IsPentant);
				Assert(!credNonPNT.IsPentant);
			}
		}

		public void TestReadElements()
		{
			CredentialsSetting setting = GetBusinessObjectToSerialise();
			setting.Username = "CAW8";
			setting.Company = "CAW";
			setting.Printer = "CAW9";
			setting.Password = "secret";
			setting.BadgeCode = "FOO";
			setting.FallbackForShed = "BAC";

			IRegistryDataType dummyDataType = new DummyNonPersistentBusinessObjectRegistryDataType(typeof(CredentialsSetting));
			ZBlob serialisedValue = dummyDataType.Serialise(setting);
			CredentialsSetting deserialisedBusinessObject = (CredentialsSetting)dummyDataType.Deserialise(serialisedValue);

			AssertEquals("FOO", deserialisedBusinessObject.BadgeCode);
			AssertEquals("CAW8", deserialisedBusinessObject.Username);
			AssertEquals("CAW9", deserialisedBusinessObject.Printer);
			AssertEquals("CAW", deserialisedBusinessObject.Company);
			AssertEquals("secret", deserialisedBusinessObject.Password);
			AssertEquals("BAC", deserialisedBusinessObject.FallbackForShed);
		}

		public void TestCredentialsValidationForFallback()
		{
			var credential = GetBusinessObjectToSerialise();

			var heathrowShed = ShedTest.CreateShed(Factory, "GB", "LHRBAC", "BRITISH AIRWAYS at Heathrow");
			var gatwickShed = ShedTest.CreateShed(Factory, "GB", "LGWXBB", "ALLPORT CARGO SERVICES LTD at Gatwick");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CustomsAgentCode, "Customs Agent");
			var agent = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, RefCusCodeListTypes.Codes.CustomsAgentCode, "DVG", "Rohlig agent Heefrow", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			agent.Factory.Save();

			credential.PIMA = "XXXXXX";
			credential.FallbackForShed = "BAC";
			AssertHasErrorContaining(credential.FallbackForShedInfo, "Fallback is only for CCSUK");

			credential.PIMA = "CUKAIR98....";
			credential.FallbackForShed = "X";
			AssertHasErrorContaining(credential.FallbackForShedInfo, "3 char");
			credential.FallbackForShed = "ZZZ";
			AssertNoErrorContaining(credential.FallbackForShedInfo, "3 char");
			AssertNoErrorContaining(credential.FallbackForShedInfo, "Fallback is only for CCSUK");

			credential.PIMA = "CUKAIR98LHRDVG";
			credential.FallbackForShed = "XXX";
			AssertHasErrorContaining(credential.FallbackForShedInfo, "Shed");

			credential.PIMA = "CUKAIR98LHRXXX";
			credential.FallbackForShed = "BAC";
			AssertHasErrorContaining(credential.FallbackForShedInfo, "Agent");
			AssertNoErrorContaining(credential.FallbackForShedInfo, "Shed");
			AssertHasErrorContaining(credential.FallbackForShedInfo, "Fallback:");
			AssertNoErrorContaining(credential.FallbackForShedInfo, "Shed");

			credential.PIMA = "CUKAIR98XXXDVG";
			credential.FallbackForShed = "LHR";
			AssertNoErrorContaining(credential.FallbackForShedInfo, "Agent");
			AssertHasErrorContaining(credential.FallbackForShedInfo, "Shed");  // LHR is at LGW

			credential.PIMA = "CUKAIR98LHRDVG";
			credential.FallbackForShed = "LHR";
			AssertNoErrorContaining(credential.FallbackForShedInfo, "Agent");
			AssertHasErrorContaining(credential.FallbackForShedInfo, "Shed");  // LHR is at LGW

			credential.PIMA = "CUKAIR98LHRDVG";
			credential.FallbackForShed = "BAC";
			AssertNoErrorContaining(credential.FallbackForShedInfo, "Fallback:");
			AssertNoErrorContaining(credential.FallbackForShedInfo, "Shed");
			AssertNoErrorContaining(credential.FallbackForShedInfo, "Agent");
		}

		public void TestPrinterValidation_SkippedWhenBadgeApplicationIsCDS()
		{
			var badgeCns1 = new BadgeCodeSetting { CSPCode = GatewayList.Codes.CNS_CUSDECOnly, BadgeCode = "CN1", ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services };
			var badgeCns2 = new BadgeCodeSetting { CSPCode = GatewayList.Codes.CNS_CUSDECOnly, BadgeCode = "CN2", ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF };
			var badgeCollection = new BadgeCodeSettingCollection();
			badgeCollection.AddRange(badgeCns1, badgeCns2);
			using (GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCollection))
			{
				var credential = new CredentialsSetting();
				credential.BadgeCode = "CN2";
				credential.Printer = "XXX";
				AssertHasMessageErrorContaining(credential.PrinterInfo, "CNS output device ('mailbox') usually ends with 'MLBX', e.g. XXXYYYMLBX.");

				credential.BadgeCode = "CC1";
				credential.Printer = "XXX";
				AssertNoNotifications(credential.PrinterInfo);
			}
		}

		public void TestCredentialsValidation()
		{
			var badgeCcs = new BadgeCodeSetting() { CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW, BadgeCode = "CCS", ApplicationCode = "CHF" };
			var badgeCns = new BadgeCodeSetting() { CSPCode = GatewayList.Codes.CNS_CUSDECOnly, BadgeCode = "CNS", ApplicationCode = "CHF" };
			var badgeMcp = new BadgeCodeSetting() { CSPCode = GatewayList.Codes.MCP_CUSDECOnly, BadgeCode = "MCP", ApplicationCode = "CHF" };
			var badgeNes = new BadgeCodeSetting() { CSPCode = GatewayList.Codes.NES, BadgeCode = "NES", Direction = "EXP", ApplicationCode = "CHF" };
			var badgePnt = new BadgeCodeSetting() { CSPCode = GatewayList.Codes.Pentant, BadgeCode = "PNT", ApplicationCode = "CHF" };
			var badgeCdsMcp = new BadgeCodeSetting() { CSPCode = GatewayList.Codes.MCP_CUSDECOnly, BadgeCode = "JIM", ApplicationCode = "CDS" };
			var badgeCdsCns = new BadgeCodeSetting() { CSPCode = GatewayList.Codes.CNS_CUSDECOnly, BadgeCode = "JIC", ApplicationCode = "CDS" };
			var badgeCollection = new BadgeCodeSettingCollection();
			badgeCollection.AddRange(badgeCcs, badgeCns, badgeMcp, badgeNes, badgePnt, badgeCdsMcp, badgeCdsCns);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCollection);

			//CCSUK
			var credential = new CredentialsSetting();
			credential.BadgeCode = "CCS";
			credential.ValidateAll();
			AssertNoMessageErrorContaining(credential.CompanyInfo, ErrorMessages.CompanyOrBadgeMustBeThreeCharacters);
			AssertNoErrorContaining(credential.CompanyInfo, ErrorMessages.CompanyRequiredForMcpAndCns);
			AssertHasMessageErrorContaining(credential.PrinterInfo, ErrorMessages.PrinterMustStartWithCUKForCcsuk);
			AssertNoErrorContaining(credential.UsernameInfo, ErrorMessages.UsernameCannotBeEmpty);
			AssertNoErrorContaining(credential.PasswordInfo, ErrorMessages.PasswordCannotBeEmpty);
			credential.PIMA = "CUKXXXYYYYY";
			AssertNoMessageErrorContaining(credential.PrinterInfo, ErrorMessages.PrinterMustStartWithCUKForCcsuk);

			// CNS
			credential = new CredentialsSetting();
			credential.BadgeCode = "CNS";
			credential.ValidateAll();
			AssertHasMessageErrorContaining(credential.CompanyInfo, ErrorMessages.CompanyOrBadgeMustBeThreeCharacters);
			AssertHasErrorContaining(credential.CompanyInfo, ErrorMessages.CompanyRequiredForMcpAndCns);
			AssertHasMessageErrorContaining(credential.PrinterInfo, ErrorMessages.PrinterUsuallyEndsWithMLBXForCns);
			AssertHasErrorContaining(credential.UsernameInfo, ErrorMessages.UsernameCannotBeEmpty);
			AssertHasErrorContaining(credential.PasswordInfo, ErrorMessages.PasswordCannotBeEmpty);
			credential.Company = "POOP";
			credential.Printer = "POOP";
			credential.Username = "POOP";
			credential.Password = "POOP";
			AssertHasMessageErrorContaining(credential.CompanyInfo, ErrorMessages.CompanyOrBadgeMustBeThreeCharacters);
			AssertNoErrorContaining(credential.CompanyInfo, ErrorMessages.CompanyRequiredForMcpAndCns);
			AssertHasMessageErrorContaining(credential.PrinterInfo, ErrorMessages.PrinterUsuallyEndsWithMLBXForCns);
			AssertHasMessageErrorContaining(credential.UsernameInfo, ErrorMessages.UsernameUsuallyEndsWithCCMIForCns);
			AssertHasMessageErrorContaining(credential.PasswordInfo, ErrorMessages.PasswordUsuallyIsCCMIPWDForCns);
			credential.Company = "CNS";
			credential.Printer = "POOPMLBX";
			credential.Username = "POOPCCMI";
			credential.Password = "CCMIPWD";
			AssertNoMessageErrorContaining(credential.CompanyInfo, ErrorMessages.CompanyOrBadgeMustBeThreeCharacters);
			AssertNoErrorContaining(credential.CompanyInfo, ErrorMessages.CompanyRequiredForMcpAndCns);
			AssertNoMessageErrorContaining(credential.PrinterInfo, ErrorMessages.PrinterUsuallyEndsWithMLBXForCns);
			AssertNoMessageErrorContaining(credential.UsernameInfo, ErrorMessages.UsernameUsuallyEndsWithCCMIForCns);
			AssertNoMessageErrorContaining(credential.PasswordInfo, ErrorMessages.PasswordUsuallyIsCCMIPWDForCns);

			// MCP
			credential = new CredentialsSetting();
			credential.BadgeCode = "MCP";
			credential.ValidateAll();
			AssertHasMessageErrorContaining(credential.CompanyInfo, ErrorMessages.CompanyOrBadgeMustBeThreeCharacters);
			AssertHasErrorContaining(credential.CompanyInfo, ErrorMessages.CompanyRequiredForMcpAndCns);
			AssertHasMessageErrorContaining(credential.PrinterInfo, ErrorMessages.PrinterUsuallyEndsWith9ForMcp);
			AssertHasErrorContaining(credential.UsernameInfo, ErrorMessages.UsernameCannotBeEmpty);
			AssertHasErrorContaining(credential.PasswordInfo, ErrorMessages.PasswordCannotBeEmpty);
			credential.Company = "POOP";
			credential.Printer = "POOP";
			credential.Username = "POOP";
			AssertHasMessageErrorContaining(credential.CompanyInfo, ErrorMessages.CompanyOrBadgeMustBeThreeCharacters);
			AssertNoErrorContaining(credential.CompanyInfo, ErrorMessages.CompanyRequiredForMcpAndCns);
			AssertHasMessageErrorContaining(credential.PrinterInfo, ErrorMessages.PrinterUsuallyEndsWith9ForMcp);
			AssertHasMessageErrorContaining(credential.UsernameInfo, ErrorMessages.UsernameUsuallyEndsWith8OrTForMcp);
			credential.Company = "MCP";
			credential.Printer = "POOP9";
			credential.Username = "POO8";
			AssertNoMessageErrorContaining(credential.CompanyInfo, ErrorMessages.CompanyOrBadgeMustBeThreeCharacters);
			AssertNoErrorContaining(credential.CompanyInfo, ErrorMessages.CompanyRequiredForMcpAndCns);
			AssertNoMessageErrorContaining(credential.PrinterInfo, ErrorMessages.PrinterUsuallyEndsWith9ForMcp);
			AssertNoMessageErrorContaining(credential.UsernameInfo, ErrorMessages.UsernameUsuallyEndsWith8OrTForMcp);
			credential.Username = "POOP8T";
			AssertHasMessageErrorContaining(credential.UsernameInfo, ErrorMessages.UsernameUsuallyEndsWith8OrTForMcp);  //for the fools who enter "ABC8/T" verbatim
			credential.Username = "POOT";
			AssertNoMessageErrorContaining(credential.UsernameInfo, ErrorMessages.UsernameUsuallyEndsWith8OrTForMcp);

			// NES
			credential = new CredentialsSetting();
			credential.BadgeCode = "NES";
			credential.ValidateAll();
			AssertNoMessageErrorContaining(credential.CompanyInfo, ErrorMessages.CompanyOrBadgeMustBeThreeCharacters);
			AssertNoErrorContaining(credential.CompanyInfo, ErrorMessages.CompanyRequiredForMcpAndCns);
			AssertHasMessageErrorContaining(credential.CompanyInfo, ErrorMessages.CompanyShouldStartWithTHSForNes);
			AssertHasMessageErrorContaining(credential.PrinterInfo, ErrorMessages.PrinterShouldStartWithLOCEDCForNes);
			AssertNoErrorContaining(credential.UsernameInfo, ErrorMessages.UsernameCannotBeEmpty);
			AssertNoErrorContaining(credential.PasswordInfo, ErrorMessages.PasswordCannotBeEmpty);
			credential.Company = "POOP";
			credential.Printer = "POOP";
			AssertHasMessageErrorContaining(credential.CompanyInfo, ErrorMessages.CompanyShouldStartWithTHSForNes);
			AssertHasMessageErrorContaining(credential.PrinterInfo, ErrorMessages.PrinterShouldStartWithLOCEDCForNes);
			credential.Company = "THS1XXX";
			credential.Printer = "LOCEDCXXX";
			AssertNoMessageErrorContaining(credential.CompanyInfo, ErrorMessages.CompanyShouldStartWithTHSForNes);
			AssertNoMessageErrorContaining(credential.PrinterInfo, ErrorMessages.PrinterShouldStartWithLOCEDCForNes);
			AssertNoErrorContaining(credential.CompanyInfo, ErrorMessages.CompanyOrBadgeMustBeThreeCharacters);
			AssertNoErrorContaining(credential.CompanyInfo, ErrorMessages.CompanyRequiredForMcpAndCns);

			// PNT
			// TODO: Add asserts for Printer, Username and Pasword?
			credential = new CredentialsSetting();
			credential.BadgeCode = "PNT";
			credential.ValidateAll();
			AssertHasMessageErrorContaining(credential.CompanyInfo, ErrorMessages.CompanyOrBadgeMustBeThreeCharacters);
			AssertNoErrorContaining(credential.CompanyInfo, ErrorMessages.CompanyRequiredForMcpAndCns);
			credential.Company = "POOP";
			AssertHasMessageErrorContaining(credential.CompanyInfo, ErrorMessages.CompanyOrBadgeMustBeThreeCharacters);
			AssertNoErrorContaining(credential.CompanyInfo, ErrorMessages.CompanyRequiredForMcpAndCns);
			credential.Company = "PNT";
			AssertNoMessageErrorContaining(credential.CompanyInfo, ErrorMessages.CompanyOrBadgeMustBeThreeCharacters);
			AssertNoErrorContaining(credential.CompanyInfo, ErrorMessages.CompanyRequiredForMcpAndCns);

			// If CDS, then bypass username validations
			credential = new CredentialsSetting();
			credential.BadgeCode = "JIM";
			credential.Username = "abcd";
			credential.ValidateAll();
			AssertNoMessageErrorContaining(credential.UsernameInfo, ErrorMessages.UsernameUsuallyEndsWith8OrTForMcp);

			credential = new CredentialsSetting();
			credential.BadgeCode = "JIC";
			credential.Username = "abcd";
			credential.ValidateAll();
			AssertNoMessageErrorContaining(credential.UsernameInfo, ErrorMessages.UsernameUsuallyEndsWithCCMIForCns);
		}

		public void TestCredentialsValidationForPreferredAgent()
		{
			var credential = GetBusinessObjectToSerialise();

			credential.PIMA = "XXXXXX";
			credential.PreferredAgent = "DJC";
			AssertHasErrorContaining(credential.PreferredAgentInfo, "This field is only for CCSUK sheds");

			credential.PIMA = "CUKFFW98000XXX";
			credential.PreferredAgent = "DAN";
			AssertHasErrorContaining(credential.PreferredAgentInfo, "This field is only for CCSUK sheds");

			credential.PIMA = "CUKAIR98LHRBAC";
			credential.PreferredAgent = "DJC";
			AssertNoErrorContaining(credential.PreferredAgentInfo, "This field is only for CCSUK sheds");

			credential.PreferredAgent = "X";
			AssertHasErrorContaining(credential.PreferredAgentInfo, "3 char");
			credential.PreferredAgent = "ZZZ";
			AssertNoErrorContaining(credential.PreferredAgentInfo, "3 char");

			credential.PreferredAgent = "";
			AssertNoErrorContaining(credential.PreferredAgentInfo, "3 char");
			AssertNoErrorContaining(credential.PreferredAgentInfo, "This field is only for CCSUK sheds");
		}

		public void TestCredentialsValidationForCcsukFallbackAgentType()
		{
			var credential = GetBusinessObjectToSerialise();

			credential.PIMA = "CUKFFW98000XXX";
			credential.CcsukFallbackAgentType = AgentTypeForExportFallbackList.Codes.Type1;
			AssertNoErrorContaining(credential.CcsukFallbackAgentTypeInfo, "CCSUK agents");
			AssertNoErrorContaining(credential.CcsukFallbackAgentTypeInfo, "valid");

			credential.CcsukFallbackAgentType = "XXX";
			AssertNoErrorContaining(credential.CcsukFallbackAgentTypeInfo, "CCSUK agents");
			AssertHasErrorContaining(credential.CcsukFallbackAgentTypeInfo, "valid");

			credential.CcsukFallbackAgentType = "XXX";
			AssertNoErrorContaining(credential.CcsukFallbackAgentTypeInfo, "CCSUK agents");
			AssertHasErrorContaining(credential.CcsukFallbackAgentTypeInfo, "valid");

			credential.PIMA = "POOP";
			credential.CcsukFallbackAgentType = AgentTypeForExportFallbackList.Codes.Type1;
			AssertHasErrorContaining(credential.CcsukFallbackAgentTypeInfo, "CCSUK agents");
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override CredentialsSetting GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override CredentialsSetting GetBusinessObjectToSerialise()
		{
			fCredentialsSetting = new CredentialsSetting();
			fCredentialsSetting.BadgeCode = "FEY";
			fCredentialsSetting.Password = "password";
			fCredentialsSetting.Printer = "FEY9";
			fCredentialsSetting.Company = "FEY8";
			return fCredentialsSetting;
		}
		CredentialsSetting fCredentialsSetting;

		#endregion
	}
}

