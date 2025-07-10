using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Interfaces;
using Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.NCTS.Testing
{
	public class NctsMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGetXmlBuilder()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var headerForUnloading = Factory.New<NctsHeader>();
			headerForUnloading.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			_ = headerForUnloading.ArrivalMovementHeader;
			_ = headerForUnloading.UnloadingMovementHeader;
			var errorCollector = new EU.Business.ErrorCollector();
			var builder = new NctsMessageBuilderForTest();

			CombineAssertions("XML Builders", () =>
			{
				AssertEquals("Unsupported Ncts Message Function", null, builder.GetCTCXmlBuilder_Exposed(header, new NctsMessageFunctionSet.XmlNckMessage(), errorCollector));
				AssertType<CC015BXmlMessageBuilder>("NctsMessageFunctionSet.DeclarationDataMessage", builder.GetCTCXmlBuilder_Exposed(header, new NctsMessageFunctionSet.DeclarationDataMessage(), errorCollector));
				AssertType<CC007AXmlMessageBuilder>("NctsMessageFunctionSet.ArrivalNotificationMessage", builder.GetCTCXmlBuilder_Exposed(header, new NctsMessageFunctionSet.ArrivalNotificationMessage(), errorCollector));
				AssertType<CC044AXmlMessageBuilder>("NctsMessageFunctionSet.UnloadingRemarksMessage", builder.GetCTCXmlBuilder_Exposed(headerForUnloading, new NctsMessageFunctionSet.UnloadingRemarksMessage(), errorCollector));
				AssertType<CC014AXmlMessageBuilder>("NctsMessageFunctionSet.DeclarationCancellationRequestMessage", builder.GetCTCXmlBuilder_Exposed(header, new NctsMessageFunctionSet.DeclarationCancellationRequestMessage("", ""), errorCollector));
			});
		}

		public void TestNativeMessage_CTCXml()
		{
			var header = GetNctsHeaderForTest();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.DepartureAndArrival);
			var errorCollector = new EU.Business.ErrorCollector();
			var builder = new NctsMessageBuilderForTest();

			var pwd = SetupPassword(header);

			var message = builder.NativeMessage(header, new NctsMessageFunctionSet.DeclarationDataMessage(), errorCollector);
			AssertStartsWith("No valid token no message", ZString.Empty, message);

			pwd.Status = PasswordStatusList.Codes.Valid;
			pwd.IsTokenForNCTS = true;

			errorCollector.WipeErrors();
			message = builder.NativeMessage(header, new NctsMessageFunctionSet.DeclarationDataMessage(), errorCollector);
			AssertStartsWith("Expecting XML message", "<CC015B>", message);

			pwd.Delete();
		}

		public void TestCheckForValidAccessToken()
		{
			var header = GetNctsHeaderForTest();

			var errorCollector = new EU.Business.ErrorCollector();
			var builder = new NctsMessageBuilderForTest();
			var pwd = SetupPassword(header);

			var result = builder.CheckForValidAccessToken_Exposed(header, errorCollector);
			CombineAssertions("No valid access token", () =>
			{
				AssertEquals("No token", false, result);
				AssertContains("Error logged", builder.NoAccessTokenError, errorCollector.GetErrorsAsString());
			});

			pwd.Status = PasswordStatusList.Codes.Valid;
			pwd.IsTokenForNCTS = true;
			errorCollector.WipeErrors();
			result = builder.CheckForValidAccessToken_Exposed(header, errorCollector);
			CombineAssertions("valid access token", () =>
			{
				AssertEquals("Token found", true, result);
				AssertNotContains("No Error logged", builder.NoAccessTokenError, errorCollector.GetErrorsAsString());
			});
			pwd.Delete();
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

		GlbExternalPassword_GB SetupPassword(NctsHeader nctsHeader)
		{
			var company = nctsHeader.Company;
			var wrapper = GBGlbCompanyWrapper.GetWrapper<GBGlbCompanyWrapper>(company);
			var gbbPasword = wrapper.GBBPasswordCollection.AddNew();
			gbbPasword.Badge = "XXX";
			gbbPasword.EORI = "GB123456789000";
			gbbPasword.Status = PasswordStatusList.Codes.Invalid;

			return gbbPasword;
		}

		class NctsMessageBuilderForTest : NctsMessageBuilder
		{
			public IXmlMessageBuilder GetCTCXmlBuilder_Exposed(NctsHeader header, NctsMessageFunctionSet messageFunction, EU.Business.ErrorCollector errorCollector) => base.GetCTCXmlBuilder(header, messageFunction, errorCollector);
			public bool CheckForValidAccessToken_Exposed(NctsHeader header, EU.Business.ErrorCollector errorCollector) => base.CheckForValidAccessToken(header, errorCollector);
		}
	}
}
