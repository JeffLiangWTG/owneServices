using System;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Customs.GB.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.NCTS;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GovernmentGateway.NCTS.Testing
{
	public class GovernmentGatewayExtensionsTest : TestCaseWithFactory
	{
		public void TestSerializeGBCustomsRequest()
		{
			var req = new GBCustomsRequest
			{
				Credentials = new Credentials { Key = "ABC.1234567890.QWE" },
				Provider = ProviderType.CTCGB,
				Service = ServiceType.Depart,
				ServiceReference = "SVC123",
				JobNumber = "Job123",
				ContentType = "XML",
				Version = "1.0"
			};

			var headerText = @"<GBCustomsRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Provider>CTCGB</Provider>
  <Service>Depart</Service>
  <Credentials Key=""ABC.1234567890.QWE"" />
  <JobNumber>Job123</JobNumber>
  <ServiceReference>SVC123</ServiceReference>
  <Version>1.0</Version>
  <ContentType>XML</ContentType>
</GBCustomsRequest>";

			AssertXmlEquals(headerText, req.Serialize());

			req = new GBCustomsRequest
			{
				Credentials = new Credentials { Key = "ABC.1234567890.QWE" },
				Provider = ProviderType.CTCGB,
				Service = ServiceType.Depart,
				ServiceReference = "SVC123",
				JobNumber = "Job123",
				ContentType = "XML",
				Version = "2.0",
				Accept = "application/vnd.hmrc.2.0+json",
				LargeFile = "true"
			};

			headerText = @"<GBCustomsRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <Provider>CTCGB</Provider>
  <Service>Depart</Service>
  <Credentials Key=""ABC.1234567890.QWE"" />
  <JobNumber>Job123</JobNumber>
  <ServiceReference>SVC123</ServiceReference>
  <Version>2.0</Version>
  <ContentType>XML</ContentType>
  <Accept>application/vnd.hmrc.2.0+json</Accept>
  <LargeFile>true</LargeFile>
</GBCustomsRequest>";

			AssertXmlEquals(headerText, req.Serialize());
		}

		[TestDate(2021, 10, 1)]
		public void TestGetValidAccessTokenFromNctsHeader()
		{
			var header = GetNctsHeaderForTest();

			CombineAssertions(() =>
			{
				AssertNull("None Exist", header.GetValidAccessToken());

				var pwd1 = CreatePassword(header.Company, "GB123456789000", "AAA", PasswordStatusList.Codes.Invalid, new ZDateTime(2021, 11, 1));
				AssertNull("Not valid", header.GetValidAccessToken());
				var pwd2 = CreatePassword(header.Company, "EORI", "BBB", PasswordStatusList.Codes.Valid, new ZDateTime(2021, 9, 1));
				AssertNull("Not a match for EORI", header.GetValidAccessToken());
				var pwd3 = CreatePassword(header.Company, "GB123456789000", "BBB", PasswordStatusList.Codes.Valid, new ZDateTime(2021, 9, 1));
				AssertNull("Expired", header.GetValidAccessToken());
				var pwd6 = CreatePassword(header.Company, "GB123456789000", "EEE", PasswordStatusList.Codes.Valid, new ZDateTime(2021, 11, 1), false);
				AssertNull("No token for NCTS", header.GetValidAccessToken());
				var pwd4 = CreatePassword(header.Company, "GB123456789000", "CCC", PasswordStatusList.Codes.Valid, new ZDateTime(2021, 11, 1));
				AssertEquals("First Valid", pwd4.PK, header.GetValidAccessToken().PK);
				var pwd5 = CreatePassword(header.Company, "GB123456789000", "DDD", PasswordStatusList.Codes.Valid, new ZDateTime(2021, 12, 1));
				AssertEquals("First Valid as ordering by badge", pwd4.PK, header.GetValidAccessToken().PK);
			});
		}

		[TestDate(2021, 10, 1)]
		public void TestHasValidAccessTokenFromNctsHeader()
		{
			var header = GetNctsHeaderForTest();

			CombineAssertions(() =>
			{
				AssertEquals("No valid token", expected: false, header.HasValidAccessToken());
				CreatePassword(header.Company, "GB123456789000", "CCC", PasswordStatusList.Codes.Valid, new ZDateTime(2021, 11, 1));
				AssertEquals("Has valid token", expected: true, header.HasValidAccessToken());
			});
		}

		public void TestIsInFinalState()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
			var message = Factory.New<NCTSOutboundEDIMessage>();
			message.EM_LinkedObject = header;
			var movement = header.MovementHeader;
			var message2 = Factory.New<NCTSOutboundEDIMessage>();
			message2.EM_LinkedObject = movement;
			var message3 = Factory.New<NCTSOutboundEDIMessage>();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NCTSTransitionPeriod, GlbCompany.CurrentCompany.Country.Code, ZDate.Today, value: true))
			{
				AssertEquals("Functionality is enabled => !IsInFinalState, parent is NctsHeader", expected: false, GovernmentGatewayExtensions.IsInFinalState(message));
				AssertEquals("Functionality is enabled => !IsInFinalState, parent is NctsDepartureMovementHeader", expected: false, GovernmentGatewayExtensions.IsInFinalState(message2));
				AssertEquals("Functionality is enabled => !IsInFinalState, parent is null", expected: true, GovernmentGatewayExtensions.IsInFinalState(message3));
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NCTSTransitionPeriod, GlbCompany.CurrentCompany.Country.Code, ZDate.Today, value: false))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, value: false))
			{
				AssertEquals("Functionality is disabled => IsInFinalState, parent is NctsHeader", expected: true, GovernmentGatewayExtensions.IsInFinalState(message));
				AssertEquals("Functionality is disabled => IsInFinalState, parent is NctsDepartureMovementHeader", expected: true, GovernmentGatewayExtensions.IsInFinalState(message2));
				AssertEquals("Functionality is disabled => IsInFinalState, parent is null", expected: true, GovernmentGatewayExtensions.IsInFinalState(message3));
			}
		}

		public void TestIsUseVersion21ApiForAccept()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.Ncts5UseApi21, Core.Constants.CountryCodes.UnitedKingdom, ZDate.Today, value: false))
			{
				AssertEquals("Functionality is disabled", expected: false, GovernmentGatewayExtensions.IsUseVersion21ApiForAccept);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.Ncts5UseApi21, Core.Constants.CountryCodes.UnitedKingdom, ZDate.Today, value: true))
			{
				AssertEquals("Functionality is enabled", expected: true, GovernmentGatewayExtensions.IsUseVersion21ApiForAccept);
			}
		}

		public void TestNewGBCustomsRequest_NCTS5ApiVersion()
		{
			var header = GetNctsHeaderForTest();
			var message = Factory.New<NCTSOutboundEDIMessage>();
			message.EM_MessageType = "015";
			message.EM_LinkedObject = header;

			CombineAssertions(() =>
			{
				for (var testcase = 0; testcase < 4; ++testcase)
				{
					var finalState = (testcase & 2) == 2;
					var useApi21 = (testcase & 1) == 1;
					var expectedAccept = finalState && useApi21 ? "application/vnd.hmrc.2.1+json" : "application/vnd.hmrc.2.0+json";

					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NCTSTransitionPeriod, GlbCompany.CurrentCompany.Country.Code, ZDate.Today, !finalState))
					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, !finalState))
					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.Ncts5UseApi21, Core.Constants.CountryCodes.UnitedKingdom, ZDate.Today, useApi21))
					{
						var request = GovernmentGatewayExtensions.NewGBCustomsRequest(message);
						AssertEquals($"Case {testcase}: Accept", expectedAccept, request.Accept);
						AssertEquals($"Case {testcase}: Version", "2.0", request.Version);
					}
				}
			});
		}

		GlbExternalPassword_GB CreatePassword(GlbCompany company, ZString eori, ZString badge, ZString status, ZDateTime expiryDate, bool isTokenForNCTS = true)
		{
			var collection = GBGlbCompanyWrapper.GetWrapper<GBGlbCompanyWrapper>(company).GBBPasswordCollection;
			var pwd = collection.AddNew();

			pwd.Badge = badge;
			pwd.EORI = eori;
			pwd.Status = status;
			pwd.GP_ExpiryDate = expiryDate;
			pwd.IsTokenForNCTS = isTokenForNCTS;

			return pwd;
		}

		NctsHeader GetNctsHeaderForTest()
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ORG";
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789000", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var aaaBranch = Factory.New<GlbBranch>();
			aaaBranch.GB_Code = "ABC";
			aaaBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			aaaBranch.GB_OH_OrgProxy = org1.PK;
			header.Declarant.E2_OA_Address = org1.MainAddress.PK;
			header.BH_GB = aaaBranch.PK;
			return header;
		}

		static void AssertXmlEquals(string expectedXml, string actualXml)
		{
			var expected = NormalizeNamespaces(XElement.Parse(expectedXml));
			var actual = NormalizeNamespaces(XElement.Parse(actualXml));

			if (!XNode.DeepEquals(expected, actual))
			{
				throw new Exception("XML content does not match.\n\nExpected:\n" + expected + "\n\nActual:\n" + actual);
			}

			Assert(true);
		}

		static XElement NormalizeNamespaces(XElement element, XNamespace defaultNamespace = null)
		{
			var currentNs = element.Name.Namespace;
			var effectiveNs = currentNs != XNamespace.None ? currentNs : defaultNamespace ?? XNamespace.None;

			return new XElement(
				effectiveNs + element.Name.LocalName,
				element.Attributes().Where(a => !a.IsNamespaceDeclaration),
				element.Nodes().Select(n =>
				{
					if (n is XElement child)
					{
						return NormalizeNamespaces(child, effectiveNs);
					}
					else if (n is XText text)
					{
						return new XText(text.Value.Trim());
					}
					else
					{
						return n;
					}
				})
			);
		}
	}
}
