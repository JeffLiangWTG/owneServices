using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class ImportTaxTreatmentsBatchMessageSenderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("lines is null", () => new ImportTaxTreatmentsBatchMessageSender(null));
			AssertExceptionThrown<ArgumentException>("lines is empty", () => new ImportTaxTreatmentsBatchMessageSender(new List<JobComInvoiceLine>()));
		}

		public void TestSendMessagesAndSave()
		{
			var codes = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("BR", "105"),
				new KeyValuePair<string, string>("CL", "158")
			};
			ReferenceTestDataHelper.CreateRefCusMap(Factory, RefCusMapTypeList.Codes.Country, "Country Codes Mapping", codes);

			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "BR1";

			var password = BRGlbStaffWrapper.Get(broker).CCTPassword;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(1);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;

			var exchangeDate = new ZDateTime(2024, 12, 11);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_GS_NKCusAgent = broker.GS_Code;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.ExchangeRateDate = exchangeDate;

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Brazil;
			invoiceLine.JI_Tariff = "01010101";

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.Brazil;
			invoiceLine2.JI_Tariff = "01010102";

			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_CountryOfOrigin = Core.Constants.CountryCodes.Brazil;
			invoiceLine3.JI_Tariff = "01010103";

			var invoiceLine4 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "01010104";

			var invoiceLine5 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine5.JI_CountryOfOrigin = Core.Constants.CountryCodes.Brazil;
			invoiceLine5.JI_Tariff = "01010101";

			var invoiceLine6 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine6.JI_CountryOfOrigin = Core.Constants.CountryCodes.Chile;
			invoiceLine6.JI_Tariff = "01010101";

			var invoiceLine7 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine7.JI_CountryOfOrigin = Core.Constants.CountryCodes.Chile;

			var messageSender = new ImportTaxTreatmentsBatchMessageSender(declaration.InvoiceLines.Cast<JobComInvoiceLine>());

			AssertEquals("Messages count should be", 4, messageSender.SendMessagesAndSave());
			AssertEquals("Messages count should be", 4, declaration.Messages.Count);

			var orderedMessages = declaration.Messages.Cast<EDIMessage>().OrderBy(x => x.EM_MessageText).ToArray();
			AssertGenerateMessage(orderedMessages[0], GetMessageText("01010101", "105"));
			AssertGenerateMessage(orderedMessages[1], GetMessageText("01010101", "158"));
			AssertGenerateMessage(orderedMessages[2], GetMessageText("01010102", "105"));
			AssertGenerateMessage(orderedMessages[3], GetMessageText("01010103", "105"));

			string GetMessageText(string tariffCode, string countryCode) => string.Format(@"{{
  ""ncm"": ""{0}"",
  ""codigoPais"": {1},
  ""dataFatoGerador"": ""2024-12-11"",
  ""tipoOperacao"": ""I""
}}", tariffCode, countryCode);

			void AssertGenerateMessage(EDIMessage message, string expectedMessageText)
			{
				CombineAssertions(() =>
				{
					AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.BRCustoms, message.EM_ApplicationCode);
					AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
					AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("EM_MessageType", MessageTypeList.Codes.RTT, message.EM_MessageType);
					AssertEquals("EM_MessageSubType", EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment, message.EM_MessageSubType);
					AssertEquals("EM_LinkUniqueID", declaration.PK, message.EM_LinkUniqueID);
					AssertEquals("EM_LinkTable", JobDeclarationSchema.Constants.TableName, message.EM_LinkTable);
					AssertEquals("EM_ApplicationReference", ZString.Empty, message.EM_ApplicationReference);
					AssertEquals("EM_MessageOwner", ZString.Empty, message.EM_MessageOwner);
					AssertEquals("EM_MessageText", expectedMessageText, message.EM_MessageText);
					AssertEquals("EM_GP", password.PK, message.EM_GP);
					AssertEquals("IsInDatabase", true, message.IsInDatabase);
				});
			}
		}

		public void TestSendMessagesAndSave_HandleSaveException()
		{
			var codes = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("BR", "105")
			};
			ReferenceTestDataHelper.CreateRefCusMap(Factory, RefCusMapTypeList.Codes.Country, "Country Codes Mapping", codes);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = ZGuid.Invalid;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			invoiceHeader.ExchangeRateDate = new ZDateTime(2024, 12, 11);
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Brazil;
			invoiceLine.JI_Tariff = "01010101";

			var messageSender = new ImportTaxTreatmentsBatchMessageSender(invoiceHeader.InvoiceLines.Cast<JobComInvoiceLine>());
			AssertEquals("No Message sent", 0, messageSender.SendMessagesAndSave());
			AssertEquals("No Message saved", 0, declaration.Messages.Count);
		}
	}
}
