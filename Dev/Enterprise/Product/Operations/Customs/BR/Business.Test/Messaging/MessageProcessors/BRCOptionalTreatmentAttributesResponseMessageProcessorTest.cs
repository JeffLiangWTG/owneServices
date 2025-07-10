using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCOptionalTreatmentAttributesResponseMessageProcessorTest : BRCResponseMessageProcessorAbstractTest<BRCOptionalTreatmentAttributesResponseMessageProcessor>
	{
		protected override IReadOnlyList<string> MessageFilterTypes => new[] { "RTT" };

		protected override IReadOnlyList<string> MessageFilterSubTypes => new[] { "OTA" };

		public void TestMessageFriendlyName()
		{
			var processor = new BRCOptionalTreatmentAttributesResponseMessageProcessor(new LoggingInformation());
			AssertEquals("MessageFriendlyName", "Optional Treatment Attributes Response Message", processor.MessageFriendlyName);
		}

		public void TestProcessResponseMessage()
		{
			var codes = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("CN", "160")
			};
			ReferenceTestDataHelper.CreateRefCusMap(Factory, RefCusMapTypeList.Codes.Country, "Country Codes Mapping", codes);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.ExchangeRateDate = new ZDateTime(2023, 04, 17);

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "01010101";
			invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "01010101";
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(declaration, MessageTypeList.Codes.RTT, EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes, requestMessageSubType: EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes).ResponseMessage;
			responseMessage.EM_MessageText = BRMessageTestHelper.GetEmbeddedResource(ResponseMessageOTA);
			Factory.Save();
			ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_LinkUniqueID", declaration.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", declaration.TableName, responseMessage.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);
				AssertEquals("EM_ApplicationReference", "01010101|CN|20230417", responseMessage.EM_ApplicationReference);
			});
		}

		public void TestProcessResponseMessage_HasNoCountryMapped()
		{
			var codes = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("CN", "150")
			};
			ReferenceTestDataHelper.CreateRefCusMap(Factory, RefCusMapTypeList.Codes.Country, "Country Codes Mapping", codes);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.ExchangeRateDate = new ZDateTime(2023, 04, 17);

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "01010101";
			invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(declaration, MessageTypeList.Codes.RTT, EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes, requestMessageSubType: EDIMessageSubTypeList.Codes.OptionalTreatmentAttributes).ResponseMessage;
			responseMessage.EM_MessageText = BRMessageTestHelper.GetEmbeddedResource(ResponseMessageOTA);
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);
			AssertEquals("EM_GB", declaration.Branch.PK, responseMessage.EM_GB);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
			AssertEquals("Logger", "Error: \tMessage #1: Code '160' has not Country mapped.\r\n", logger.LogMessages.ToString());
		}

		public void TestProcessResponseMessage_LogErrorWithDeserializationFailed()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.ExchangeRateDate = new ZDateTime(2023, 04, 17);

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "01010101";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(declaration, MessageTypeList.Codes.RTT, EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment).ResponseMessage;
			responseMessage.EM_MessageText = @"";
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);
			AssertEquals("EM_GB", declaration.Branch.PK, responseMessage.EM_GB);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
			AssertEquals("Logger", "Error: \tMessage #1: Message deserialization was failed.\r\n", logger.LogMessages.ToString());
		}

		public const string ResponseMessageOTA = "ResponseMessageOTA.json";
	}
}
