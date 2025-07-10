using System;
using System.IO;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CHEDIInterchange))]
class CHEDIInterchangeTest : EnterpriseBusinessObjectTestCase
{
	public void TestIMessageDataProvider()
	{
		var ediInterchange = Factory.New<CHEDIInterchange>();
		Assert(ediInterchange is IMessageDataProvider);
	}

	public void TestGetMessageDataFromDeclaration()
	{
		var fileContent = Encoding.UTF8.GetBytes("Test Document Content for Declaration");
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();

		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var header = declaration.CustomsEntryHeaders.AddNew();
		header.CH_CEI_Instruction = instruction.PK;

		var interchange = Factory.New<CHEDIInterchange>();
		var message = Factory.New<CHEDIMessage>();
		message.EM_LinkedObject = header;

		var attachment = message.MessageAttachments.AddNew();
		var doc = declaration.DocManagerInfo.AddFileOrDocument(fileContent, "File", "txt");
		attachment.EG_StorageDocsGuid = doc.UniqueKey;

		var key = Convert.ToBase64String(doc.UniqueKey.ToGuid().ToByteArray());

		interchange.ContainedMessages.Add(message);
		interchange.EI_BodyText = expectedNormalText = $"TEST TEXT - {key}";

		interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		interchange.EI_ApplicationCode = ApplicationCodeList.Codes.CHCustomsEdec;
		interchange.EI_InterchangeType = MessageTypeCodeList.Codes.Export;

		using (var reader = interchange.GetMessageData())
		{
			AssertEquals("Should get the original body text.", expectedNormalText, ReadAsString(reader));
		}

		interchange.EI_InterchangeType = MessageTypeCodeList.Codes.Import;

		using (var reader = interchange.GetMessageData())
		{
			AssertEquals("Should get the original body text.", expectedNormalText, ReadAsString(reader));
		}

		interchange.EI_InterchangeType = MessageTypeCodeList.Codes.EBD;

		using (var reader = interchange.GetMessageData())
		{
			AssertEquals("Should get the body with message attachment", $"TEST TEXT - {Convert.ToBase64String(fileContent)}", ReadAsString(reader));
		}
	}

	public void TestGetMessageDataFromShipment()
	{
		var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
		shipment.FillWithValidTestData();

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_JS = shipment.PK;

		var fileContent = Encoding.UTF8.GetBytes("Test Document Content From Shipment");
		var doc = ((IDocManagerSupport)shipment).DocManagerInfo.AddFileOrDocument(fileContent, "TestFile", "txt");

		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var header = declaration.CustomsEntryHeaders.AddNew();
		header.CH_CEI_Instruction = instruction.PK;

		var interchange = Factory.New<CHEDIInterchange>();
		interchange.EI_InterchangeType = MessageTypeCodeList.Codes.EBD;
		var message = Factory.New<CHEDIMessage>();
		message.EM_LinkedObject = header;

		var attachment = message.MessageAttachments.AddNew();
		attachment.EG_StorageDocsGuid = doc.UniqueKey;

		var key = Convert.ToBase64String(doc.UniqueKey.ToGuid().ToByteArray());

		interchange.ContainedMessages.Add(message);
		interchange.EI_BodyText = $"TEST TEXT - {key}";

		using (var reader = interchange.GetMessageData())
		{
			AssertEquals("Should get the parsed body text from the document of shipment.", $"TEST TEXT - {Convert.ToBase64String(fileContent)}", ReadAsString(reader));
		}
	}

	public void TestGetMessageData_ReplaceDocUniqueKey()
	{
		var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
		shipment.FillWithValidTestData();

		var fileOnShipment = Encoding.UTF8.GetBytes("Test Document Content From Shipment");
		var docOnShipment = ((IDocManagerSupport)shipment).DocManagerInfo.AddFileOrDocument(fileOnShipment, "TestFile", "txt");

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_JS = shipment.PK;

		var fileOnDeclaration = Encoding.UTF8.GetBytes("Test Document Content From Declaration");
		var docOnDeclaration = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(fileOnDeclaration, "TestFile", "txt");

		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var header = declaration.CustomsEntryHeaders.AddNew();
		header.CH_CEI_Instruction = instruction.PK;

		var fileOnEntry = Encoding.UTF8.GetBytes("Test Document Content From Entry");
		var docOnEntry = ((IDocManagerSupport)header).DocManagerInfo.AddFileOrDocument(fileOnEntry, "TestFile", "txt");

		var interchange = Factory.New<CHEDIInterchange>();
		var message1 = Factory.New<CHEDIMessage>();
		message1.EM_LinkedObject = header;
		var message2 = Factory.New<CHEDIMessage>();
		message2.EM_LinkedObject = header;

		var attachment1 = message1.MessageAttachments.AddNew();
		attachment1.EG_StorageDocsGuid = docOnShipment.UniqueKey;
		var attachment2 = message1.MessageAttachments.AddNew();
		attachment2.EG_StorageDocsGuid = docOnDeclaration.UniqueKey;
		var attachment3 = message2.MessageAttachments.AddNew();
		attachment3.EG_StorageDocsGuid = docOnEntry.UniqueKey;

		var docKeyOnShipment = Convert.ToBase64String(docOnShipment.UniqueKey.ToGuid().ToByteArray());
		var docKeyOnDeclaration = Convert.ToBase64String(docOnDeclaration.UniqueKey.ToGuid().ToByteArray());
		var docKeyOnEntry = Convert.ToBase64String(docOnEntry.UniqueKey.ToGuid().ToByteArray());

		interchange.ContainedMessages.Add(message1);
		interchange.ContainedMessages.Add(message2);
		interchange.EI_BodyText = expectedNormalText = $"TEST TEXT - {docKeyOnShipment} - {docKeyOnDeclaration} - {docKeyOnEntry}";

		interchange.EI_InterchangeType = MessageTypeCodeList.Codes.Export;

		using (var reader = interchange.GetMessageData())
		{
			AssertEquals("Should get the original body text.", expectedNormalText, ReadAsString(reader));
		}

		interchange.EI_InterchangeType = MessageTypeCodeList.Codes.Import;

		using (var reader = interchange.GetMessageData())
		{
			AssertEquals("Should get the original body text.", expectedNormalText, ReadAsString(reader));
		}

		var expectedBodyText = $"TEST TEXT - {Convert.ToBase64String(fileOnShipment)} - {Convert.ToBase64String(fileOnDeclaration)} - {Convert.ToBase64String(fileOnEntry)}";

		interchange.EI_InterchangeType = MessageTypeCodeList.Codes.EBD;
		using (var reader = interchange.GetMessageData())
		{
			AssertEquals("Should get the parsed body text from the documents on shipment, declaration, entry.", expectedBodyText, ReadAsString(reader));
		}
	}

	string expectedNormalText;

	string ReadAsString(BinaryReader reader)
	{
		var bytes = new byte[reader.BaseStream.Length];
		reader.BaseStream.Read(bytes, 0, bytes.Length);
		return Encoding.UTF8.GetString(bytes);
	}
}
