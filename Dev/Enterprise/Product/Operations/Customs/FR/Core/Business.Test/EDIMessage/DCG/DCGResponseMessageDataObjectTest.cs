using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class DCGResponseMessageDataObjectTest : TestCaseWithFactory
	{
		public void TestMessageStatusDescription()
		{
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustoms.xml");
			var message = Factory.New<DCGResponseFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.DCG;
			message.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
			message.EM_MessageText = importMessageText;
			var dataProvider = (IDCGResponseDataProvider)message.MessageDataObject;
			Factory.Save();

			AssertEquals("MessageStatusDescription should match message.", StatementStatusList.Descriptions.Complete, dataProvider.MessageStatusDescription);

			importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustomsWithErrors.xml");
			var message2 = Factory.New<DCGResponseFREDIMessage>();
			message2.EM_MessageType = MessageTypeList.Codes.DCG;
			message2.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
			message2.EM_MessageText = importMessageText;
			var dataProvider2 = (IDCGResponseDataProvider)message2.MessageDataObject;
			Factory.Save();

			AssertEquals("MessageStatusDescription should match message.", StatementStatusList.Descriptions.Incomplete, dataProvider2.MessageStatusDescription);

			importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustomsWithAnomalies.xml");
			var message3 = Factory.New<DCGResponseFREDIMessage>();
			message3.EM_MessageType = MessageTypeList.Codes.DCG;
			message3.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
			message3.EM_MessageText = importMessageText;
			var dataProvider3 = (IDCGResponseDataProvider)message3.MessageDataObject;
			Factory.Save();

			AssertEquals("MessageStatusDescription should match message.", StatementStatusList.Descriptions.Incomplete, dataProvider3.MessageStatusDescription);
		}

		public void TestFrequency()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var orgCusAccount = Factory.New<OrgCusAccount>();
			orgCusAccount.CZ_OH = importer.PK;
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
			orgCusAccount.CZ_Account = "TESTACC";
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G2;
			orgCusAccount.CZ_ReportingPeriod = ReportingPeriodList.Codes.MON;

			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			dec1.JE_OH_Importer = importer.PK;
			dec1.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			dec1.JE_DeclarationReference = "0738/19";

			var entry1 = dec1.CustomsEntryHeaders.AddNew();
			var entryNumber1 = CusEntryNumber.New(entry1, EU.Business.MessageTypeList.Codes.Import, Core.Constants.CountryCodes.France);
			entryNumber1.CE_EntryNum = "1906142283";

			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustoms.xml");
			var message = Factory.New<DCGResponseFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.DCG;
			message.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
			message.EM_MessageText = importMessageText;

			var dataProvider = (IDCGResponseDataProvider)message.MessageDataObject;
			Factory.Save();

			dec1.JE_CustomsProfile = "";
			AssertEquals("Frequency should be defaulted to Unknown when it can not be retrieved from entries listed in the message.", StatementPeriodicityList.Codes.Unknown, dataProvider.Frequency);

			dec1.JE_CustomsProfile = "TESTACC";
			AssertEquals("Frequency should be retrieved from entries listed in the message.", StatementPeriodicityList.Codes.Month, dataProvider.Frequency);
		}

		public void TestDirection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "0738/19";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryNumber = CusEntryNumber.New(entry, EU.Business.MessageTypeList.Codes.Import, Core.Constants.CountryCodes.France);
			entryNumber.CE_EntryNum = "1906142283";

			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustoms.xml");
			var message = Factory.New<DCGResponseFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.DCG;
			message.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
			message.EM_MessageText = importMessageText;
			var dataProvider = (IDCGResponseDataProvider)message.MessageDataObject;

			Factory.Save();

			AssertEquals("Direction should match DCG first entry.", StatementEntryTypeImpExpList.Codes.Import, dataProvider.Direction);

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			AssertEquals("Direction should match DCG first entry.", StatementEntryTypeImpExpList.Codes.Export, dataProvider.Direction);
		}

		public void TestPeriodStartDate()
		{
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustoms.xml");
			var message = Factory.New<DCGResponseFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.DCG;
			message.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
			message.EM_MessageText = importMessageText;
			var dataProvider = (IDCGResponseDataProvider)message.MessageDataObject;
			AssertEquals("01-Jul-19", dataProvider.PeriodStartDate.ToString());
		}

		public void TestPeriodEndDate()
		{
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustoms.xml");
			var message = Factory.New<DCGResponseFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.DCG;
			message.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
			message.EM_MessageText = importMessageText;
			var dataProvider = (IDCGResponseDataProvider)message.MessageDataObject;
			AssertEquals("31-Jul-19", dataProvider.PeriodEndDate.ToString());
		}

		public void TestGetReadableErrorList()
		{
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustomsWithErrors.xml");
			var message = Factory.New<DCGResponseFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.DCG;
			message.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
			message.EM_MessageText = importMessageText;
			var dataProvider = (IDCGResponseDataProvider)message.MessageDataObject;
			AssertEquals("Readable Error List", "\r\n\r\nFollowing errors were reported by Customs:\r\n- DELTA_FONCTL/Erreur 1\r\n- DELTA_FONCTL/Erreur 2", dataProvider.GetReadableErrorList());
		}

		public void TestGetReadableAnomalyList()
		{
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustomsWithAnomalies.xml");
			var message = Factory.New<DCGResponseFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.DCG;
			message.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
			message.EM_MessageText = importMessageText;
			var dataProvider = (IDCGResponseDataProvider)message.MessageDataObject;
			AssertEquals("Readable Anomaly List", "\r\n\r\nFollowing anomalies were reported by Customs:\r\n- DELTA_FONCTL/Anomalie 1 (1906096292)\r\n- DELTA_FONCTL/Anomalie 2 (1906100402)", dataProvider.GetReadableAnomalyList());
		}

		public void TestGetReadableTaxList()
		{
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustoms.xml");
			var message = Factory.New<DCGResponseFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.DCG;
			message.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
			message.EM_MessageText = importMessageText;
			var dataProvider = (IDCGResponseDataProvider)message.MessageDataObject;
			AssertEquals("Readable Tax List", "\r\n\r\nResponse lists following taxes:\r\n- A445/B00 : 101079 EUR\r\n- U165/A00 : 160254 EUR\r\n- A445/B00 : 597310 EUR", dataProvider.GetReadableTaxList());
		}

		public void TestHasErrors()
		{
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustoms.xml");
			var message = Factory.New<DCGResponseFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.DCG;
			message.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
			message.EM_MessageText = importMessageText;
			var dataProvider = (IDCGResponseDataProvider)message.MessageDataObject;
			AssertEquals("HasErrors", false, dataProvider.HasErrors);

			var importMessageText2 = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustomsWithErrors.xml");
			var message2 = Factory.New<DCGResponseFREDIMessage>();
			message2.EM_MessageType = MessageTypeList.Codes.DCG;
			message2.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
			message2.EM_MessageText = importMessageText2;
			var dataProvider2 = (IDCGResponseDataProvider)message2.MessageDataObject;
			AssertEquals("HasErrors", true, dataProvider2.HasErrors);
		}

		public void TestHasAnomalies()
		{
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustoms.xml");
			var message = Factory.New<DCGResponseFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.DCG;
			message.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
			message.EM_MessageText = importMessageText;
			var dataProvider = (IDCGResponseDataProvider)message.MessageDataObject;
			AssertEquals("HasAnomalies", false, dataProvider.HasAnomalies);

			var importMessageText2 = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustomsWithAnomalies.xml");
			var message2 = Factory.New<DCGResponseFREDIMessage>();
			message2.EM_MessageType = MessageTypeList.Codes.DCG;
			message2.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
			message2.EM_MessageText = importMessageText2;
			var dataProvider2 = (IDCGResponseDataProvider)message2.MessageDataObject;
			AssertEquals("HasAnomalies", true, dataProvider2.HasAnomalies);
		}

		public void TestHasTaxes()
		{
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustoms.xml");
			var message = Factory.New<DCGResponseFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.DCG;
			message.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
			message.EM_MessageText = importMessageText;
			var dataProvider = (IDCGResponseDataProvider)message.MessageDataObject;
			AssertEquals("HasTaxes", true, dataProvider.HasTaxes);

			var importMessageText2 = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustomsWithErrors.xml");
			var message2 = Factory.New<DCGResponseFREDIMessage>();
			message2.EM_MessageType = MessageTypeList.Codes.DCG;
			message2.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
			message2.EM_MessageText = importMessageText2;
			var dataProvider2 = (IDCGResponseDataProvider)message2.MessageDataObject;
			AssertEquals("HasTaxes", false, dataProvider2.HasTaxes);
		}

		public void TestHasEntries()
		{
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustoms.xml");
			var message = Factory.New<DCGResponseFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.DCG;
			message.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
			message.EM_MessageText = importMessageText;
			var dataProvider = (IDCGResponseDataProvider)message.MessageDataObject;
			AssertEquals("HasEntries", true, dataProvider.HasEntries);

			var importMessageText2 = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustomsWithAnomalies.xml");
			var message2 = Factory.New<DCGResponseFREDIMessage>();
			message2.EM_MessageType = MessageTypeList.Codes.DCG;
			message2.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
			message2.EM_MessageText = importMessageText2;
			var dataProvider2 = (IDCGResponseDataProvider)message2.MessageDataObject;
			AssertEquals("HasEntries", false, dataProvider2.HasEntries);
		}

		public void TestIDCGResponseDataProviderWhenAnomalies()
		{
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustomsWithAnomalies.xml");

			var message = Factory.New<DCGResponseFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.DCG;
			message.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
			message.EM_MessageText = importMessageText;
			var dataProvider = (IDCGResponseDataProvider)message.MessageDataObject;
			AssertEquals("Anomalies", 2, dataProvider.Anomalies.Count);
		}

		public void TestIDCGResponseDataProviderWhenErrors()
		{
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustomsWithErrors.xml");

			var message = Factory.New<DCGResponseFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.DCG;
			message.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
			message.EM_MessageText = importMessageText;
			var dataProvider = (IDCGResponseDataProvider)message.MessageDataObject;
			AssertEquals("Errors", 2, dataProvider.Errors.Count);
		}

		public void TestIDCGResponseDataProvider()
		{
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustoms.xml");

			var message = Factory.New<DCGResponseFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.DCG;
			message.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
			message.EM_MessageText = importMessageText;
			AssertType<DCGResponseMessageDataObject>("DCG response message", message.MessageDataObject);

			var dataProvider = (IDCGResponseDataProvider)message.MessageDataObject;

			CombineAssertions(() =>
			{
				AssertEquals("DateFrom", "01/07/2019", dataProvider.DateFrom);
				AssertEquals("DateTo", "31/07/2019", dataProvider.DateTo);
				AssertEquals("EntryNum", "19014110", dataProvider.EntryNum);
				AssertEquals("DCGReference", "DCG_REFERENCE", dataProvider.DCGReference);
				AssertEquals("Taxes", 3, dataProvider.Taxes.Count);
				AssertEquals("Entries", 5, dataProvider.Entries.Count);
			});
		}

		public void TestPrettier()
		{
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustoms.xml");
			var message = Factory.New<DCGResponseFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.DCG;
			message.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
			message.EM_MessageText = importMessageText;

			AssertType<DCGResponsePrettier>("DCGResponsePrettier", message.MessageDataObject.Prettier);
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
