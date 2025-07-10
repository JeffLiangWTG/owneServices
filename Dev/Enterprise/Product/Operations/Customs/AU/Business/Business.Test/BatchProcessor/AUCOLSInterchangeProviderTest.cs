using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUCOLSInterchangeProviderTest : InterchangeProviderTestCase
	{
		public override void TestMessagesPopulateNewInterchange()
		{
			var provider = GetInterchangeProvider(messages);
			var interchange = provider.Interchanges[0];
			CombineAssertions(() =>
			{
				AssertEquals("COLSInterchange.EI_ApplicationCode", "COL", interchange.EI_ApplicationCode);
				AssertEquals("COLSInterchange.EI_InterchangeType", "CNL", interchange.EI_InterchangeType);
				AssertEquals("COLSInterchange.EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
				AssertEquals("COLSInterchange.EI_TransportType", "XTT", interchange.EI_TransportType);
				AssertEquals("COLSInterchange.EI_Priority", "HGH", interchange.EI_Priority);
				AssertEquals("COLSInterchange.EI_IsActive", ZBool.True, interchange.EI_IsActive);
				AssertEquals("COLSInterchange.EI_GB", company.Branches[0].PK, interchange.EI_GB);
				AssertEquals("COLSInterchange.EI_From", company.LicenceKeyIdentifier, interchange.EI_From);
				AssertEquals("COLSInterchange.EI_BodyText", "<Greeting>Hello</Greeting>", interchange.EI_BodyText);
				AssertEquals("COLSInterchange.EI_Status", "QUE", interchange.EI_Status);
			});
		}

		public void TestSetInterchangeHeaderTextWithWithInterchangeType_CNL_CME_CRE_LST()
		{
			var expectedHeader = @"{""custom.AU.SubscriptionKey"":""cdfd202a32a044aebd7a4e6348d7b5e2""}";

			AssertInterchangeHeader(expectedHeader, "CNL", ZString.Empty);
			SetupMessages();
			AssertInterchangeHeader(expectedHeader, "CME", ZString.Empty);
			SetupMessages();
			AssertInterchangeHeader(expectedHeader, "CRE", ZString.Empty);
			SetupMessages();
			AssertInterchangeHeader(expectedHeader, "LST", ZString.Empty);
		}

		public void TestSetInterchangeHeaderTextWithWithInterchangeType_CAA_CLS()
		{
			var expectedHeader = @"{""custom.AU.SubscriptionKey"":""cdfd202a32a044aebd7a4e6348d7b5e2"",""custom.Lrn"":""LodgementReferenceNumber""}";
			AssertInterchangeHeader(expectedHeader, "CAA", "LodgementReferenceNumber");
			SetupMessages();
			AssertInterchangeHeader(expectedHeader, "CLS", "LodgementReferenceNumber");
		}

		public void TestSetInterchangeHeaderTextWithInterchangeType_CPS()
		{
			var expectedHeader = @"{""custom.AU.SubscriptionKey"":""cdfd202a32a044aebd7a4e6348d7b5e2"",""custom.ReferenceNumber"":""SomeReferenceNumber""}";
			AssertInterchangeHeader(expectedHeader, "CPS", "SomeReferenceNumber");
		}

		public void TestSetInterchangeHeaderTextWithInterchangeType_CNA()
		{
			var expectedHeader = "{\"custom.AU.SubscriptionKey\":\"cdfd202a32a044aebd7a4e6348d7b5e2\",\"custom.Lrn\":\"LodgementReferenceNumber\"}";
			AssertInterchangeHeader(expectedHeader, "CNA", "LodgementReferenceNumber");
		}

		public void TestSetInterchangeHeaderTextWithInterchangeType_CSA()
		{
			var expectedHeader = "{\"custom.AU.SubscriptionKey\":\"cdfd202a32a044aebd7a4e6348d7b5e2\",\"custom.Lrn\":\"ReferenceNumber\"}";
			AssertInterchangeHeader(expectedHeader, "CSA", "ReferenceNumber");
		}

		void AssertInterchangeHeader(ZString expectedInterchangeHeaderText, ZString messageType, ZString lrn)
		{
			messages[0].EM_MessageType = messageType;
			messages[0].EM_ApplicationReference = lrn;
			var provider = GetInterchangeProvider(messages);
			var interchange = provider.Interchanges[0];
			AssertEquals("COLSInterchange.EI_HeaderText", expectedInterchangeHeaderText, interchange.EI_HeaderText);
		}

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection) => new AUCOLSInterchangeProvider(collection);

		protected override void SetUp()
		{
			base.SetUp();
			CreateCOLSSubscriptionKey();
			company = Factory.New<GlbCompany>();
			company.GC_Code = "GC7";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			_ = company.Branches.AddNew();
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			SetupMessages();
		}

		void CreateCOLSSubscriptionKey()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetExistingRefSysConfigType("COLPRIAPI", "COLS Primary API Subscription Key", "COLS Primary API Subscription Key");
			helper.CreateRefSysConfig("COLPRIAPI", "cdfd202a32a044aebd7a4e6348d7b5e2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}

		void SetupMessages()
		{
			messages = new NonDependentEDIMessageCollection(Factory);
			var message = messages.AddNew();
			message.EM_MessageText = "<Greeting>Hello</Greeting>";
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			message.EM_GB = company.Branches[0].PK;
			message.EM_IsTestMessage = true;
			message.EM_MessageType = "CNL";
			message.EM_MessageOwner = "";
		}

		NonDependentEDIMessageCollection messages;
		GlbCompany company;
	}
}
