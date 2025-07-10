using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class XTradeInterchangeProviderTest : InterchangeProviderTestCase
{
	public override void TestMessagesPopulateNewInterchange()
	{
		var company = Factory.NewWithValidTestData<GlbCompany>();
		var companyWrapper = GlbCompanyWrapper.Get(company);
		var companyPassword = companyWrapper.PasswordCollection.AddNew();

		var message = Factory.New<EDIMessage>();
		message.EM_ApplicationCode = "ITH";
		message.EM_MessageText = "message text";
		message.EM_GP = companyPassword.PK;
		message.MessageNumberStrategy = new FixedMessageNumberStrategy("123456");
		message.EM_ApplicationReference = "ABC";

		var interchangeCollection = PackCollatedMessagesIntoInterchanges(message);
		Factory.Save();
		message.Reload();

		AssertEquals("PRE-CONDITION: Number of created interchanges", 1, interchangeCollection.Length);
		CombineAssertions(() =>
		{
			var expectedInterchangeFrom = FormattableString.Invariant($"Hello{GlbCompany.CurrentCompany.GC_Code}World");
			var interchange = interchangeCollection[0];
			AssertEquals("EM_EI", interchange.PK, message.EM_EI);
			AssertEquals("EI_ApplicationCode", "ITH", interchange.EI_ApplicationCode);
			AssertEquals("EI_InterchangeType", "ABC", interchange.EI_InterchangeType);
			AssertEquals("EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
			AssertEquals("EI_From", expectedInterchangeFrom, interchange.EI_From);
			AssertEquals("EI_To", "ITCustoms", interchange.EI_To);
			AssertEquals("EI_Status", "QUE", interchange.EI_Status);
			AssertEquals("EI_HeaderText", "", interchange.EI_HeaderText);
			AssertEquals("EI_BodyText", "message text", interchange.EI_BodyText);
			AssertEquals("EI_TransportType", "XTT", interchange.EI_TransportType);
			AssertEquals("EI_GP", companyPassword.PK, interchange.EI_GP);
			AssertEquals("EI_FooterText", "", interchange.EI_FooterText);
			AssertNotEquals("EI_SessionGUID", ZGuid.Empty, interchange.EI_SessionGUID);
		});
	}

	public void TestCreateOneInterchangeEachMessage()
	{
		var message1 = Factory.New<EDIMessage>();
		var message2 = Factory.New<EDIMessage>();

		var interchangeCollection = PackCollatedMessagesIntoInterchanges(message1, message2);
		AssertEquals("PRE-CONDITION: Number of created interchanges", 2, interchangeCollection.Length);
		CombineAssertions(() =>
		{
			AssertNotEquals("message1.EM_EI", ZGuid.Empty, message1.EM_EI);
			AssertNotEquals("message2.EM_EI", ZGuid.Empty, message2.EM_EI);
			AssertNotEquals("message1 and message2 are linked to different interchanges (no collation)", message1.EM_EI, message2.EM_EI);
		});
	}

	public void TestInstructionHowToSetInterchangeSenderID()
	{
		var interchangeProvider = new XTradeInterchangeProviderForTest(new NonDependentEDIMessageCollection(Factory));
		AssertEquals("InstructionHowToSetInterchangeSenderID", ZString.Empty, interchangeProvider.InstructionHowToSetInterchangeSenderIDExposed);
	}

	public void TestGetCollationKey()
	{
		var interchangeProvider = new XTradeInterchangeProviderForTest(new NonDependentEDIMessageCollection(Factory));
		AssertEquals("Collation Key", "DONOTCOLLATE", interchangeProvider.GetCollationKeyExposed(message: null));
	}

	public void TestGetFooterText()
	{
		var interchangeProvider = new XTradeInterchangeProviderForTest(new NonDependentEDIMessageCollection(Factory));
		AssertEquals("Footer Text", ZString.Empty, interchangeProvider.GetFooterTextExposed());
	}

	public void TestMessagePopulateNewInterchangeForEFQMessage()
	{
		var company = Factory.NewWithValidTestData<GlbCompany>();
		var companyWrapper = GlbCompanyWrapper.Get(company);
		var companyPassword = companyWrapper.PasswordCollection.AddNew();

		var message = Factory.New<EDIMessage>();
		message.EM_ApplicationCode = "ITH";
		message.EM_MessageText = "EFQ Message";
		message.EM_GP = companyPassword.PK;
		message.MessageNumberStrategy = new FixedMessageNumberStrategy("123456");
		message.EM_ApplicationReference = ZString.Empty;
		message.EM_MessageType = EDIMessageTypeList.Codes.ElectronicFolderQuery;

		var interchangeCollection = PackCollatedMessagesIntoInterchanges(message);
		Factory.Save();
		message.Reload();

		AssertEquals("PRE-CONDITION: Number of created interchanges", 1, interchangeCollection.Length);
		CombineAssertions(() =>
		{
			var expectedInterchangeFrom = FormattableString.Invariant($"Hello{GlbCompany.CurrentCompany.GC_Code}World");
			var interchange = interchangeCollection[0];
			AssertEquals("EM_EI", interchange.PK, message.EM_EI);
			AssertEquals("EI_ApplicationCode", "ITH", interchange.EI_ApplicationCode);
			AssertEquals("EI_InterchangeType", "EFQ", interchange.EI_InterchangeType);
			AssertEquals("EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
			AssertEquals("EI_From", expectedInterchangeFrom, interchange.EI_From);
			AssertEquals("EI_To", "ITCustomsSOAP", interchange.EI_To);
			AssertEquals("EI_Status", "QUE", interchange.EI_Status);
			AssertEquals("EI_HeaderText", ZString.Empty, interchange.EI_HeaderText);
			AssertEquals("EI_BodyText", "EFQ Message", interchange.EI_BodyText);
			AssertEquals("EI_TransportType", "XTT", interchange.EI_TransportType);
			AssertEquals("EI_GP", companyPassword.PK, interchange.EI_GP);
			AssertEquals("EI_FooterText", ZString.Empty, interchange.EI_FooterText);
			AssertNotEquals("EI_SessionGUID", ZGuid.Empty, interchange.EI_SessionGUID);
		});
	}

	public void TestMessagePopulateNewInterchangeForIUTMessage()
	{
		var company = Factory.NewWithValidTestData<GlbCompany>();
		var companyWrapper = GlbCompanyWrapper.Get(company);
		var companyPassword = companyWrapper.PasswordCollection.AddNew();

		var parent = Factory.New<DummyBusinessObject>();
		var message = Factory.New<EDIMessage>();
		message.EM_ApplicationCode = "ITH";
		message.EM_MessageType = "IUT";
		message.EM_MessageSubType = "XXX";
		message.EM_ReceiveTransmit = "TRX";
		message.EM_Status = "QUE";
		message.EM_MessageText = "IUT SOAP Message Text";
		message.EM_LinkedObject = parent;
		message.EM_GP = companyPassword.PK;
		message.EM_ApplicationReference = "IUT";
		message.MessageNumberStrategy = new FixedMessageNumberStrategy("");

		var interchangeCollection = PackCollatedMessagesIntoInterchanges(message);
		Factory.Save();
		message.Reload();

		AssertEquals("PRE-CONDITION: Number of created interchanges", 1, interchangeCollection.Length);
		CombineAssertions(() =>
		{
			var expectedInterchangeFrom = FormattableString.Invariant($"Hello{GlbCompany.CurrentCompany.GC_Code}World");
			var interchange = interchangeCollection[0];
			AssertEquals("EM_EI", interchange.PK, message.EM_EI);
			AssertNotEquals("EI_SessionGUID", ZGuid.Empty, interchange.EI_SessionGUID);
			AssertEquals("EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
			AssertEquals("EI_ApplicationCode", "ITH", interchange.EI_ApplicationCode);
			AssertEquals("EI_InterchangeType", "IUT", interchange.EI_InterchangeType);
			AssertEquals("EI_From", expectedInterchangeFrom, interchange.EI_From);
			AssertEquals("EI_To", "ITCustoms", interchange.EI_To);
			AssertEquals("EI_Status", "QUE", interchange.EI_Status);
			AssertEquals("EI_BodyText", "IUT SOAP Message Text", interchange.EI_BodyText);
			AssertEquals("EI_GP", companyPassword.PK, interchange.EI_GP);
			AssertEquals("EI_TransportType", "XTT", interchange.EI_TransportType);
		});
	}

	public void TestMessagePopulateNewInterchangeHeaderTextForCANMessage()
	{
		var parent = Factory.New<DummyBusinessObject>();
		var message = Factory.New<EDIMessage>();
		message.EM_MessageType = "CAN";
		message.EM_LinkedObject = parent;
		message.MessageNumberStrategy = new FixedMessageNumberStrategy("");

		var interchangeCollection = PackCollatedMessagesIntoInterchanges(message);
		Factory.Save();
		message.Reload();

		AssertEquals("PRE-CONDITION: Number of created interchanges", 1, interchangeCollection.Length);

		var interchange = interchangeCollection[0];
		AssertEquals("EI_HeaderText", "{\"custom.MessageSubType\":\"CAN\"}", interchange.EI_HeaderText);
	}

	public void TestMessagePopulateNewInterchangeHeaderTextForAMDMessage()
	{
		var parent = Factory.New<DummyBusinessObject>();
		var message = Factory.New<EDIMessage>();
		message.EM_MessageType = "AMD";
		message.EM_LinkedObject = parent;
		message.MessageNumberStrategy = new FixedMessageNumberStrategy("");
		message.EM_ApplicationReference = EDIMessageApplicationReferenceList.Codes.TemporaryStorage;

		var interchangeCollection = PackCollatedMessagesIntoInterchanges(message);
		Factory.Save();
		message.Reload();

		AssertEquals("PRE-CONDITION: Number of created interchanges", 1, interchangeCollection.Length);

		var interchange = interchangeCollection[0];
		AssertEquals("EI_HeaderText", "{\"custom.MessageSubType\":\"AMD\"}", interchange.EI_HeaderText);
	}

	public void TestMessagePopulateNewInterchangeForIVIMessage()
	{
		var company = Factory.NewWithValidTestData<GlbCompany>();
		var companyWrapper = GlbCompanyWrapper.Get(company);
		var companyPassword = companyWrapper.PasswordCollection.AddNew();

		var parent = Factory.New<DummyBusinessObject>();
		var message = Factory.New<EDIMessage>();
		message.EM_ApplicationCode = "ITH";
		message.EM_MessageType = "IVI";
		message.EM_MessageSubType = "XXX";
		message.EM_ReceiveTransmit = "TRX";
		message.EM_Status = "QUE";
		message.EM_MessageText = "IVI SOAP Message Text";
		message.EM_LinkedObject = parent;
		message.EM_GP = companyPassword.PK;
		message.EM_ApplicationReference = "";
		message.MessageNumberStrategy = new FixedMessageNumberStrategy("");

		var interchangeCollection = PackCollatedMessagesIntoInterchanges(message);
		Factory.Save();
		message.Reload();

		AssertEquals("PRE-CONDITION: Number of created interchanges", 1, interchangeCollection.Length);
		CombineAssertions(() =>
		{
			var expectedInterchangeFrom = FormattableString.Invariant($"Hello{GlbCompany.CurrentCompany.GC_Code}World");
			var interchange = interchangeCollection[0];
			AssertEquals("EM_EI", interchange.PK, message.EM_EI);
			AssertNotEquals("EI_SessionGUID", ZGuid.Empty, interchange.EI_SessionGUID);
			AssertEquals("EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
			AssertEquals("EI_ApplicationCode", "ITH", interchange.EI_ApplicationCode);
			AssertEquals("EI_InterchangeType", "IVI", interchange.EI_InterchangeType);
			AssertEquals("EI_From", expectedInterchangeFrom, interchange.EI_From);
			AssertEquals("EI_To", "ITCustoms", interchange.EI_To);
			AssertEquals("EI_Status", "QUE", interchange.EI_Status);
			AssertEquals("EI_BodyText", "IVI SOAP Message Text", interchange.EI_BodyText);
			AssertEquals("EI_GP", companyPassword.PK, interchange.EI_GP);
			AssertEquals("EI_TransportType", "XTT", interchange.EI_TransportType);
		});
	}

	public void TestMessagePopulateNewInterchangeForSVIMessage()
	{
		var company = Factory.NewWithValidTestData<GlbCompany>();
		var companyWrapper = GlbCompanyWrapper.Get(company);
		var companyPassword = companyWrapper.PasswordCollection.AddNew();

		var parent = Factory.New<DummyBusinessObject>();
		var message = Factory.New<EDIMessage>();
		message.EM_ApplicationCode = "ITH";
		message.EM_MessageType = "SVI";
		message.EM_MessageSubType = "XXX";
		message.EM_ReceiveTransmit = "TRX";
		message.EM_Status = "QUE";
		message.EM_MessageText = "SVI SOAP Message Text";
		message.EM_LinkedObject = parent;
		message.EM_GP = companyPassword.PK;
		message.EM_ApplicationReference = "";
		message.MessageNumberStrategy = new FixedMessageNumberStrategy("");

		var interchangeCollection = PackCollatedMessagesIntoInterchanges(message);
		Factory.Save();
		message.Reload();

		AssertEquals("PRE-CONDITION: Number of created interchanges", 1, interchangeCollection.Length);
		CombineAssertions(() =>
		{
			var expectedInterchangeFrom = FormattableString.Invariant($"Hello{GlbCompany.CurrentCompany.GC_Code}World");
			var interchange = interchangeCollection[0];
			AssertEquals("EM_EI", interchange.PK, message.EM_EI);
			AssertNotEquals("EI_SessionGUID", ZGuid.Empty, interchange.EI_SessionGUID);
			AssertEquals("EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
			AssertEquals("EI_ApplicationCode", "ITH", interchange.EI_ApplicationCode);
			AssertEquals("EI_InterchangeType", "SVI", interchange.EI_InterchangeType);
			AssertEquals("EI_From", expectedInterchangeFrom, interchange.EI_From);
			AssertEquals("EI_To", "ITCustoms", interchange.EI_To);
			AssertEquals("EI_Status", "QUE", interchange.EI_Status);
			AssertEquals("EI_BodyText", "SVI SOAP Message Text", interchange.EI_BodyText);
			AssertEquals("EI_GP", companyPassword.PK, interchange.EI_GP);
			AssertEquals("EI_TransportType", "XTT", interchange.EI_TransportType);
		});
	}

	public void TestMessagePopulateNewInterchangeForPRRMessage()
	{
		var company = Factory.NewWithValidTestData<GlbCompany>();
		var companyWrapper = GlbCompanyWrapper.Get(company);
		var companyPassword = companyWrapper.PasswordCollection.AddNew();

		var parent = Factory.New<DummyBusinessObject>();
		var message = Factory.New<EDIMessage>();
		message.EM_ApplicationCode = "ITH";
		message.EM_MessageType = "PRR";
		message.EM_MessageSubType = "XXX";
		message.EM_ReceiveTransmit = "TRX";
		message.EM_Status = "QUE";
		message.EM_MessageText = "PRR SOAP Message Text";
		message.EM_LinkedObject = parent;
		message.EM_GP = companyPassword.PK;
		message.EM_ApplicationReference = "";
		message.MessageNumberStrategy = new FixedMessageNumberStrategy("");

		var interchangeCollection = PackCollatedMessagesIntoInterchanges(message);
		Factory.Save();
		message.Reload();

		AssertEquals("PRE-CONDITION: Number of created interchanges", 1, interchangeCollection.Length);
		CombineAssertions(() =>
		{
			var expectedInterchangeFrom = FormattableString.Invariant($"Hello{GlbCompany.CurrentCompany.GC_Code}World");
			var interchange = interchangeCollection[0];
			AssertEquals("EM_EI", interchange.PK, message.EM_EI);
			AssertNotEquals("EI_SessionGUID", ZGuid.Empty, interchange.EI_SessionGUID);
			AssertEquals("EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
			AssertEquals("EI_ApplicationCode", "ITH", interchange.EI_ApplicationCode);
			AssertEquals("EI_InterchangeType", "PRR", interchange.EI_InterchangeType);
			AssertEquals("EI_From", expectedInterchangeFrom, interchange.EI_From);
			AssertEquals("EI_To", "ITCustoms", interchange.EI_To);
			AssertEquals("EI_Status", "QUE", interchange.EI_Status);
			AssertEquals("EI_BodyText", "PRR SOAP Message Text", interchange.EI_BodyText);
			AssertEquals("EI_GP", companyPassword.PK, interchange.EI_GP);
			AssertEquals("EI_TransportType", "XTT", interchange.EI_TransportType);
		});
	}

	public void TestMessagePopulateNewInterchangeForEU1Message()
	{
		var company = Factory.NewWithValidTestData<GlbCompany>();
		var companyWrapper = GlbCompanyWrapper.Get(company);
		var companyPassword = companyWrapper.PasswordCollection.AddNew();

		var parent = Factory.New<DummyBusinessObject>();
		var message = Factory.New<EDIMessage>();
		message.EM_ApplicationCode = "ITH";
		message.EM_MessageType = "EU1";
		message.EM_MessageSubType = "XXX";
		message.EM_ReceiveTransmit = "TRX";
		message.EM_Status = "QUE";
		message.EM_MessageText = "EU1 SOAP Message Text";
		message.EM_LinkedObject = parent;
		message.EM_GP = companyPassword.PK;
		message.EM_ApplicationReference = "";
		message.MessageNumberStrategy = new FixedMessageNumberStrategy("");

		var interchangeCollection = PackCollatedMessagesIntoInterchanges(message);
		Factory.Save();
		message.Reload();

		AssertEquals("PRE-CONDITION: Number of created interchanges", 1, interchangeCollection.Length);
		CombineAssertions(() =>
		{
			var expectedInterchangeFrom = FormattableString.Invariant($"Hello{GlbCompany.CurrentCompany.GC_Code}World");
			var interchange = interchangeCollection[0];
			AssertEquals("EM_EI", interchange.PK, message.EM_EI);
			AssertNotEquals("EI_SessionGUID", ZGuid.Empty, interchange.EI_SessionGUID);
			AssertEquals("EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
			AssertEquals("EI_ApplicationCode", "ITH", interchange.EI_ApplicationCode);
			AssertEquals("EI_InterchangeType", "EU1", interchange.EI_InterchangeType);
			AssertEquals("EI_From", expectedInterchangeFrom, interchange.EI_From);
			AssertEquals("EI_To", "ITCustoms", interchange.EI_To);
			AssertEquals("EI_Status", "QUE", interchange.EI_Status);
			AssertEquals("EI_BodyText", "EU1 SOAP Message Text", interchange.EI_BodyText);
			AssertEquals("EI_GP", companyPassword.PK, interchange.EI_GP);
			AssertEquals("EI_TransportType", "XTT", interchange.EI_TransportType);
		});
	}

	public void TestMessagePopulateNewInterchangeForTadMessage()
	{
		var company = Factory.NewWithValidTestData<GlbCompany>();
		var companyWrapper = GlbCompanyWrapper.Get(company);
		var companyPassword = companyWrapper.PasswordCollection.AddNew();

		var parent = Factory.New<DummyBusinessObject>();
		var message = Factory.New<EDIMessage>();
		message.EM_ApplicationCode = "ITH";
		message.EM_MessageType = "TAD";
		message.EM_MessageSubType = "TAD";
		message.EM_ReceiveTransmit = "TRX";
		message.EM_Status = "QUE";
		message.EM_MessageText = "TAD SOAP Message Text";
		message.EM_LinkedObject = parent;
		message.EM_GP = companyPassword.PK;
		message.EM_ApplicationReference = "";
		message.MessageNumberStrategy = new FixedMessageNumberStrategy("");

		var interchangeCollection = PackCollatedMessagesIntoInterchanges(message);
		Factory.Save();
		message.Reload();

		AssertEquals("PRE-CONDITION: Number of created interchanges", 1, interchangeCollection.Length);
		CombineAssertions(() =>
		{
			var expectedInterchangeFrom = FormattableString.Invariant($"Hello{GlbCompany.CurrentCompany.GC_Code}World");
			var interchange = interchangeCollection[0];
			AssertEquals("EM_EI", interchange.PK, message.EM_EI);
			AssertNotEquals("EI_SessionGUID", ZGuid.Empty, interchange.EI_SessionGUID);
			AssertEquals("EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
			AssertEquals("EI_ApplicationCode", "ITH", interchange.EI_ApplicationCode);
			AssertEquals("EI_InterchangeType", "TAD", interchange.EI_InterchangeType);
			AssertEquals("EI_From", expectedInterchangeFrom, interchange.EI_From);
			AssertEquals("EI_To", "ITCustoms", interchange.EI_To);
			AssertEquals("EI_Status", "QUE", interchange.EI_Status);
			AssertEquals("EI_BodyText", "TAD SOAP Message Text", interchange.EI_BodyText);
			AssertEquals("EI_GP", companyPassword.PK, interchange.EI_GP);
			AssertEquals("EI_TransportType", "XTT", interchange.EI_TransportType);
		});
	}

	public void TestMessagePopulateNewInterchangeForEADMessage()
	{
		var company = Factory.NewWithValidTestData<GlbCompany>();
		var companyWrapper = GlbCompanyWrapper.Get(company);
		var companyPassword = companyWrapper.PasswordCollection.AddNew();

		var parent = Factory.New<DummyBusinessObject>();
		var message = Factory.New<EDIMessage>();
		message.EM_ApplicationCode = "ITH";
		message.EM_MessageType = "EAD";
		message.EM_MessageSubType = "XXX";
		message.EM_ReceiveTransmit = "TRX";
		message.EM_Status = "QUE";
		message.EM_MessageText = "EAD SOAP Message Text";
		message.EM_LinkedObject = parent;
		message.EM_GP = companyPassword.PK;
		message.EM_ApplicationReference = "";
		message.MessageNumberStrategy = new FixedMessageNumberStrategy("");

		var interchangeCollection = PackCollatedMessagesIntoInterchanges(message);
		Factory.Save();
		message.Reload();

		AssertEquals("PRE-CONDITION: Number of created interchanges", 1, interchangeCollection.Length);
		CombineAssertions(() =>
		{
			var expectedInterchangeFrom = FormattableString.Invariant($"Hello{GlbCompany.CurrentCompany.GC_Code}World");
			var interchange = interchangeCollection[0];
			AssertEquals("EM_EI", interchange.PK, message.EM_EI);
			AssertNotEquals("EI_SessionGUID", ZGuid.Empty, interchange.EI_SessionGUID);
			AssertEquals("EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
			AssertEquals("EI_ApplicationCode", "ITH", interchange.EI_ApplicationCode);
			AssertEquals("EI_InterchangeType", "EAD", interchange.EI_InterchangeType);
			AssertEquals("EI_From", expectedInterchangeFrom, interchange.EI_From);
			AssertEquals("EI_To", "ITCustoms", interchange.EI_To);
			AssertEquals("EI_Status", "QUE", interchange.EI_Status);
			AssertEquals("EI_BodyText", "EAD SOAP Message Text", interchange.EI_BodyText);
			AssertEquals("EI_GP", companyPassword.PK, interchange.EI_GP);
			AssertEquals("EI_TransportType", "XTT", interchange.EI_TransportType);
		});
	}

	public void TestMessagePopulateNewInterchangeForIRIMessage()
	{
		var company = Factory.NewWithValidTestData<GlbCompany>();
		var companyWrapper = GlbCompanyWrapper.Get(company);
		var companyPassword = companyWrapper.PasswordCollection.AddNew();

		var parent = Factory.New<DummyBusinessObject>();
		var message = Factory.New<EDIMessage>();
		message.EM_ApplicationCode = "ITH";
		message.EM_MessageType = "IRI";
		message.EM_MessageSubType = "XXX";
		message.EM_ReceiveTransmit = "TRX";
		message.EM_Status = "QUE";
		message.EM_MessageText = "IRI SOAP Message Text";
		message.EM_LinkedObject = parent;
		message.EM_GP = companyPassword.PK;
		message.EM_ApplicationReference = "";
		message.MessageNumberStrategy = new FixedMessageNumberStrategy("");

		var interchangeCollection = PackCollatedMessagesIntoInterchanges(message);
		Factory.Save();
		message.Reload();

		AssertEquals("PRE-CONDITION: Number of created interchanges", 1, interchangeCollection.Length);
		CombineAssertions(() =>
		{
			var expectedInterchangeFrom = FormattableString.Invariant($"Hello{GlbCompany.CurrentCompany.GC_Code}World");
			var interchange = interchangeCollection[0];
			AssertEquals("EM_EI", interchange.PK, message.EM_EI);
			AssertNotEquals("EI_SessionGUID", ZGuid.Empty, interchange.EI_SessionGUID);
			AssertEquals("EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
			AssertEquals("EI_ApplicationCode", "ITH", interchange.EI_ApplicationCode);
			AssertEquals("EI_InterchangeType", "IRI", interchange.EI_InterchangeType);
			AssertEquals("EI_From", expectedInterchangeFrom, interchange.EI_From);
			AssertEquals("EI_To", "ITCustoms", interchange.EI_To);
			AssertEquals("EI_Status", "QUE", interchange.EI_Status);
			AssertEquals("EI_BodyText", "IRI SOAP Message Text", interchange.EI_BodyText);
			AssertEquals("EI_GP", companyPassword.PK, interchange.EI_GP);
			AssertEquals("EI_TransportType", "XTT", interchange.EI_TransportType);
		});
	}

	public void TestMessagePopulateNewInterchangeForSPRMessage()
	{
		var company = Factory.NewWithValidTestData<GlbCompany>();
		var companyWrapper = GlbCompanyWrapper.Get(company);
		var companyPassword = companyWrapper.PasswordCollection.AddNew();

		var parent = Factory.New<DummyBusinessObject>();
		var message = Factory.New<EDIMessage>();
		message.EM_ApplicationCode = "ITH";
		message.EM_MessageType = "SPR";
		message.EM_MessageSubType = "SPR";
		message.EM_ReceiveTransmit = "TRX";
		message.EM_Status = "QUE";
		message.EM_MessageText = "SPR SOAP Message Text";
		message.EM_LinkedObject = parent;
		message.EM_GP = companyPassword.PK;
		message.EM_ApplicationReference = "";
		message.MessageNumberStrategy = new FixedMessageNumberStrategy("");

		var interchangeCollection = PackCollatedMessagesIntoInterchanges(message);
		Factory.Save();
		message.Reload();

		AssertEquals("PRE-CONDITION: Number of created interchanges", 1, interchangeCollection.Length);
		CombineAssertions(() =>
		{
			var expectedInterchangeFrom = FormattableString.Invariant($"Hello{GlbCompany.CurrentCompany.GC_Code}World");
			var interchange = interchangeCollection[0];
			AssertEquals("EM_EI", interchange.PK, message.EM_EI);
			AssertNotEquals("EI_SessionGUID", ZGuid.Empty, interchange.EI_SessionGUID);
			AssertEquals("EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
			AssertEquals("EI_ApplicationCode", "ITH", interchange.EI_ApplicationCode);
			AssertEquals("EI_InterchangeType", "SPR", interchange.EI_InterchangeType);
			AssertEquals("EI_From", expectedInterchangeFrom, interchange.EI_From);
			AssertEquals("EI_To", "ITCustoms", interchange.EI_To);
			AssertEquals("EI_Status", "QUE", interchange.EI_Status);
			AssertEquals("EI_BodyText", "SPR SOAP Message Text", interchange.EI_BodyText);
			AssertEquals("EI_GP", companyPassword.PK, interchange.EI_GP);
			AssertEquals("EI_TransportType", "XTT", interchange.EI_TransportType);
		});
	}

	public void TestMessagePopulateNewInterchangeForPRDMessage()
	{
		var company = Factory.NewWithValidTestData<GlbCompany>();
		var companyWrapper = GlbCompanyWrapper.Get(company);
		var companyPassword = companyWrapper.PasswordCollection.AddNew();

		var parent = Factory.New<DummyBusinessObject>();
		var message = Factory.New<EDIMessage>();
		message.EM_ApplicationCode = "ITH";
		message.EM_MessageType = "PRD";
		message.EM_MessageSubType = "XXX";
		message.EM_ReceiveTransmit = "TRX";
		message.EM_Status = "QUE";
		message.EM_MessageText = "PRD SOAP Message Text";
		message.EM_LinkedObject = parent;
		message.EM_GP = companyPassword.PK;
		message.EM_ApplicationReference = "";
		message.MessageNumberStrategy = new FixedMessageNumberStrategy("");

		var interchangeCollection = PackCollatedMessagesIntoInterchanges(message);
		Factory.Save();
		message.Reload();

		AssertEquals("PRE-CONDITION: Number of created interchanges", 1, interchangeCollection.Length);
		CombineAssertions(() =>
		{
			var expectedInterchangeFrom = FormattableString.Invariant($"Hello{GlbCompany.CurrentCompany.GC_Code}World");
			var interchange = interchangeCollection[0];
			AssertEquals("EM_EI", interchange.PK, message.EM_EI);
			AssertNotEquals("EI_SessionGUID", ZGuid.Empty, interchange.EI_SessionGUID);
			AssertEquals("EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
			AssertEquals("EI_ApplicationCode", "ITH", interchange.EI_ApplicationCode);
			AssertEquals("EI_InterchangeType", "PRD", interchange.EI_InterchangeType);
			AssertEquals("EI_From", expectedInterchangeFrom, interchange.EI_From);
			AssertEquals("EI_To", "ITCustoms", interchange.EI_To);
			AssertEquals("EI_Status", "QUE", interchange.EI_Status);
			AssertEquals("EI_BodyText", "PRD SOAP Message Text", interchange.EI_BodyText);
			AssertEquals("EI_GP", companyPassword.PK, interchange.EI_GP);
			AssertEquals("EI_TransportType", "XTT", interchange.EI_TransportType);
		});
	}

	public void TestMessagePopulateNewInterchangeForSPDMessage()
	{
		var company = Factory.NewWithValidTestData<GlbCompany>();
		var companyWrapper = GlbCompanyWrapper.Get(company);
		var companyPassword = companyWrapper.PasswordCollection.AddNew();

		var parent = Factory.New<DummyBusinessObject>();
		var message = Factory.New<EDIMessage>();
		message.EM_ApplicationCode = "ITH";
		message.EM_MessageType = "SPD";
		message.EM_MessageSubType = "XXX";
		message.EM_ReceiveTransmit = "TRX";
		message.EM_Status = "QUE";
		message.EM_MessageText = "SPD SOAP Message Text";
		message.EM_LinkedObject = parent;
		message.EM_GP = companyPassword.PK;
		message.EM_ApplicationReference = "";
		message.MessageNumberStrategy = new FixedMessageNumberStrategy("");

		var interchangeCollection = PackCollatedMessagesIntoInterchanges(message);
		Factory.Save();
		message.Reload();

		AssertEquals("PRE-CONDITION: Number of created interchanges", 1, interchangeCollection.Length);
		CombineAssertions(() =>
		{
			var expectedInterchangeFrom = FormattableString.Invariant($"Hello{GlbCompany.CurrentCompany.GC_Code}World");
			var interchange = interchangeCollection[0];
			AssertEquals("EM_EI", interchange.PK, message.EM_EI);
			AssertNotEquals("EI_SessionGUID", ZGuid.Empty, interchange.EI_SessionGUID);
			AssertEquals("EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
			AssertEquals("EI_ApplicationCode", "ITH", interchange.EI_ApplicationCode);
			AssertEquals("EI_InterchangeType", "SPD", interchange.EI_InterchangeType);
			AssertEquals("EI_From", expectedInterchangeFrom, interchange.EI_From);
			AssertEquals("EI_To", "ITCustoms", interchange.EI_To);
			AssertEquals("EI_Status", "QUE", interchange.EI_Status);
			AssertEquals("EI_BodyText", "SPD SOAP Message Text", interchange.EI_BodyText);
			AssertEquals("EI_GP", companyPassword.PK, interchange.EI_GP);
			AssertEquals("EI_TransportType", "XTT", interchange.EI_TransportType);
		});
	}

	public void TestMessagesPopulateNewInterchange_WhenUserHasAutomaticSignaturePassword()
	{
		AutomaticSignatureExternalPasswordLookupsTest.SetUpRefSysConfigs(Factory);
		var staff = Factory.NewWithValidTestData<GlbStaff>();
		var staffWrapper = GlbStaffWrapper.Get(staff);
		var automaticSignature = staffWrapper.AutomaticSignaturePasswordCollection.AddNew();
		automaticSignature.GP_MailBoxID = "ITARDSDL02";
		automaticSignature.GP_UserID = "UserName";
		automaticSignature.IsConfigurationActive = true;

		var parent = Factory.New<DummyBusinessObject>();
		var message = Factory.New<EDIMessage>();
		message.EM_SystemCreateUser = staff.GS_Code;
		message.EM_LinkedObject = parent;
		message.MessageNumberStrategy = new FixedMessageNumberStrategy("");
		message.EM_Status = "QUE";

		var interchangeCollection = PackCollatedMessagesIntoInterchanges(message);
		Factory.Save();
		message.Reload();

		const string expectedHeaderText = "{" +
			"\"custom.IT.Signature\":\"Y\"," +
			"\"custom.IT.SignatureDelegatedUser\":\"angela.stecca\"," +
			"\"custom.IT.SignatureUser\":\"UserName\"" +
			"}";

		AssertEquals("PRE-CONDITION: Number of created interchanges", 1, interchangeCollection.Length);
		var interchange = interchangeCollection[0];
		AssertEquals("EI_HeaderText", expectedHeaderText, interchange.EI_HeaderText);
	}

	public void TestMessagesPopulateNewInterchange_IvistoRequestDoesNotContainAutomaticSignatureMetadata()
	{
		SetupMessageAndAssertInterchangeDoesNotIncludeAutomaticSignatureMetadata(EDIMessageTypeList.Codes.IvistoRequest);
	}

	public void TestMessagesPopulateNewInterchange_IrildesRequestDoesNotContainAutomaticSignatureMetadata()
	{
		SetupMessageAndAssertInterchangeDoesNotIncludeAutomaticSignatureMetadata(EDIMessageTypeList.Codes.IrildesRequest);
	}

	public void TestMessagesPopulateNewInterchange_IutRequestDoesNotContainAutomaticSignatureMetadata()
	{
		SetupMessageAndAssertInterchangeDoesNotIncludeAutomaticSignatureMetadata(EDIMessageTypeList.Codes.UniqueTransactionId);
	}

	public void TestCancellationMessagePopulateNewInterchange_WhenUserHasAutomaticSignaturePassword()
	{
		AutomaticSignatureExternalPasswordLookupsTest.SetUpRefSysConfigs(Factory);
		var staff = Factory.NewWithValidTestData<GlbStaff>();
		var staffWrapper = GlbStaffWrapper.Get(staff);
		var automaticSignature = staffWrapper.AutomaticSignaturePasswordCollection.AddNew();
		automaticSignature.GP_MailBoxID = "ITARDSDL02";
		automaticSignature.GP_UserID = "UserName";
		automaticSignature.IsConfigurationActive = true;

		var parent = Factory.New<DummyBusinessObject>();
		var message = Factory.New<EDIMessage>();
		message.EM_MessageType = "CAN";
		message.EM_SystemCreateUser = staff.GS_Code;
		message.EM_LinkedObject = parent;
		message.MessageNumberStrategy = new FixedMessageNumberStrategy("");
		message.EM_Status = "QUE";

		var interchangeCollection = PackCollatedMessagesIntoInterchanges(message);
		Factory.Save();
		message.Reload();

		const string expectedHeaderText = "{" +
			"\"custom.MessageSubType\":\"CAN\"," +
			"\"custom.IT.Signature\":\"Y\"," +
			"\"custom.IT.SignatureDelegatedUser\":\"angela.stecca\"," +
			"\"custom.IT.SignatureUser\":\"UserName\"" +
			"}";

		AssertEquals("PRE-CONDITION: Number of created interchanges", 1, interchangeCollection.Length);
		var interchange = interchangeCollection[0];
		AssertEquals("EI_HeaderText", expectedHeaderText, interchange.EI_HeaderText);
	}

	public void TestAmendmentMessagePopulateNewInterchange_WhenUserHasAutomaticSignaturePassword()
	{
		AutomaticSignatureExternalPasswordLookupsTest.SetUpRefSysConfigs(Factory);
		var staff = Factory.NewWithValidTestData<GlbStaff>();
		var staffWrapper = GlbStaffWrapper.Get(staff);
		var automaticSignature = staffWrapper.AutomaticSignaturePasswordCollection.AddNew();
		automaticSignature.GP_MailBoxID = "ITARDSDL02";
		automaticSignature.GP_UserID = "UserName";
		automaticSignature.IsConfigurationActive = true;

		var parent = Factory.New<DummyBusinessObject>();
		var message = Factory.New<EDIMessage>();
		message.EM_MessageType = "AMD";
		message.EM_SystemCreateUser = staff.GS_Code;
		message.EM_LinkedObject = parent;
		message.EM_ApplicationReference = EDIMessageApplicationReferenceList.Codes.TemporaryStorage;
		message.MessageNumberStrategy = new FixedMessageNumberStrategy("");
		message.EM_Status = "QUE";

		var interchangeCollection = PackCollatedMessagesIntoInterchanges(message);
		Factory.Save();
		message.Reload();

		const string expectedHeaderText = "{" +
			"\"custom.MessageSubType\":\"AMD\"," +
			"\"custom.IT.Signature\":\"Y\"," +
			"\"custom.IT.SignatureDelegatedUser\":\"angela.stecca\"," +
			"\"custom.IT.SignatureUser\":\"UserName\"" +
			"}";

		AssertEquals("PRE-CONDITION: Number of created interchanges", 1, interchangeCollection.Length);
		var interchange = interchangeCollection[0];
		AssertEquals("EI_HeaderText", expectedHeaderText, interchange.EI_HeaderText);
	}

	public void TestMessagesPopulateNewInterchange_WhenUserIsInvalid()
	{
		var parent = Factory.New<DummyBusinessObject>();
		var message = Factory.New<EDIMessage>();
		message.EM_SystemCreateUser = "XZ";
		message.EM_LinkedObject = parent;
		message.MessageNumberStrategy = new FixedMessageNumberStrategy("");
		message.EM_Status = "QUE";

		var interchangeCollection = PackCollatedMessagesIntoInterchanges(message);
		Factory.Save();
		message.Reload();

		AssertEquals("PRE-CONDITION: Number of created interchanges", 1, interchangeCollection.Length);
		var interchange = interchangeCollection[0];
		AssertEquals("EI_HeaderText", "", interchange.EI_HeaderText);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var productRegKeyMock = new Mock<IProductRegistrationKey>();
		productRegKeyMock.Setup(k => k.EnterpriseCode).Returns("Hello");
		productRegKeyMock.Setup(k => k.ServerCode).Returns("World");

		var productRegistrationMock = new Mock<IProductRegistration>();
		productRegistrationMock.Setup(p => p.Key).Returns(productRegKeyMock.Object);

		ObjectFactory.Substitute(productRegistrationMock.Object);
	}

	protected override void TearDown()
	{
		base.TearDown();
		ObjectFactory.DisposeSubstitutions();
	}

	protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection) => new XTradeInterchangeProvider(collection);

	EDIInterchange[] PackCollatedMessagesIntoInterchanges(params EDIMessage[] messages)
	{
		var ediMessageCollection = new NonDependentEDIMessageCollection(Factory);
		ediMessageCollection.AddRange(messages);

		var interchangeProvider = GetInterchangeProvider(ediMessageCollection);
		interchangeProvider.PackCollatedMessagesIntoInterchanges();
		return interchangeProvider.Interchanges;
	}

	void SetupMessageAndAssertInterchangeDoesNotIncludeAutomaticSignatureMetadata(string messageType)
	{
		AutomaticSignatureExternalPasswordLookupsTest.SetUpRefSysConfigs(Factory);
		var staff = Factory.NewWithValidTestData<GlbStaff>();
		var staffWrapper = GlbStaffWrapper.Get(staff);
		var automaticSignature = staffWrapper.AutomaticSignaturePasswordCollection.AddNew();
		automaticSignature.GP_MailBoxID = "ITARDSDL02";
		automaticSignature.GP_UserID = "UserName";
		automaticSignature.IsConfigurationActive = true;

		var parent = Factory.New<DummyBusinessObject>();
		var message = Factory.New<EDIMessage>();
		message.EM_SystemCreateUser = staff.GS_Code;
		message.EM_LinkedObject = parent;
		message.MessageNumberStrategy = new FixedMessageNumberStrategy("");
		message.EM_Status = "QUE";
		message.EM_MessageType = messageType;

		var interchangeCollection = PackCollatedMessagesIntoInterchanges(message);
		Factory.Save();
		message.Reload();

		AssertEquals("PRE-CONDITION: Number of created interchanges", 1, interchangeCollection.Length);
		var interchange = interchangeCollection[0];
		AssertEquals("EI_HeaderText", "", interchange.EI_HeaderText);
	}
}

class XTradeInterchangeProviderForTest : XTradeInterchangeProvider
{
	public XTradeInterchangeProviderForTest(NonDependentEDIMessageCollection messages) : base(messages)
	{
	}

	public string InstructionHowToSetInterchangeSenderIDExposed => InstructionHowToSetInterchangeSenderID;

	public string GetCollationKeyExposed(EDIMessage message) => GetCollationKey(message);

	public ZString GetFooterTextExposed() => GetFooterText(interchange: null, messages: null);
}
