using System;
using System.IO;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class CertificateOfOriginXmlUploaderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When entryHeader is null", () => new CertificateOfOriginXmlUploader(entryHeader: null));
	}

	public void TestUploadNullStreamReader()
	{
		AssertExceptionThrown<ArgumentNullException>("When streamReader is null", () => certificateOfOriginXmlUploader.Upload(streamReader: null));
	}

	public void TestUploadInvalidCertificateOfOriginThrowsNotSupportedException()
	{
		using (var streamReader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes("<CERTIFICATO_XXX></CERTIFICATO_XXX>"))))
		{
			AssertExceptionThrown<NotSupportedException>("When an unrecognisable file is uploaded", "The uploaded file is not supported.", () => certificateOfOriginXmlUploader.Upload(streamReader));
		}
	}

	public void TestUploadEUR1()
	{
		AssertEquals("PRE-CONDITION: number of messages", 0, entryHeader.Messages.Count);
		UploadFileAndAssertResult("A21ITQS31T0003580T1_Eur1.xml", expectedMessageSubType: "EUR");
	}

	public void TestUploadATR()
	{
		AssertEquals("PRE-CONDITION: number of messages", 0, entryHeader.Messages.Count);
		UploadFileAndAssertResult("B21ITQS31T0012765E0_Atr.xml", expectedMessageSubType: "ATR");
	}

	public void TestUploadEURMED()
	{
		AssertEquals("PRE-CONDITION: number of messages", 0, entryHeader.Messages.Count);
		UploadFileAndAssertResult("C21ITQVG2T0000060E1_Eur1Med.xml", expectedMessageSubType: "EUM");
	}

	public void TestUploadOverwriteExistingCertificateOfOriginFile()
	{
		AssertEquals("PRE-CONDITION: number of messages", 0, entryHeader.Messages.Count);
		UploadFileAndAssertResult("A21ITQS31T0003580T1_Eur1.xml", expectedMessageSubType: "EUR");

		AssertEquals("PRE-CONDITION: number of messages", 1, entryHeader.Messages.Count);
		UploadFileAndAssertResult("B21ITQS31T0012765E0_Atr.xml", expectedMessageSubType: "ATR");
	}

	protected override void SetUp()
	{
		base.SetUp();
		entryHeader = Factory.New<CusEntryHeader>();
		certificateOfOriginXmlUploader = new CertificateOfOriginXmlUploader(entryHeader);
	}

	CusEntryHeader entryHeader;
	CertificateOfOriginXmlUploader certificateOfOriginXmlUploader;

	void UploadFileAndAssertResult(ZString testFilename, ZString expectedMessageSubType)
	{
		using (var streamReader = ManifestResourceHelper.ManifestResourceContentStream("Enterprise.Customs.IT.Business.Testing.MessageProcessors.CertificateOfOriginXml.TestFiles." + testFilename))
		{
			var expectedFileContent = streamReader.ReadToEnd();

			streamReader.BaseStream.Position = 0;
			streamReader.DiscardBufferedData();
			certificateOfOriginXmlUploader.Upload(streamReader);
			AssertEquals("POST-CONDITION: number of messages", 1, entryHeader.Messages.Count);

			var certificateOfOriginMessage = entryHeader.Messages[0];
			CombineAssertions($"Assertions for EDIMessage {testFilename}", () =>
			{
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.ITCustoms, certificateOfOriginMessage.EM_ApplicationCode);
				AssertEquals("EM_MessageType", "XCO", certificateOfOriginMessage.EM_MessageType);
				AssertEquals("EM_MessageSubType", expectedMessageSubType, certificateOfOriginMessage.EM_MessageSubType);
				AssertEquals("EM_ReceiveTransmit", "RCV", certificateOfOriginMessage.EM_ReceiveTransmit);
				AssertEquals("EM_ApplicationReference", "", certificateOfOriginMessage.EM_ApplicationReference);
				AssertEquals("EM_Status", "MAN", certificateOfOriginMessage.EM_Status);
				AssertEquals("EM_MessageText", expectedFileContent, certificateOfOriginMessage.EM_MessageText);
				AssertEquals("EM_MessageNum", "", certificateOfOriginMessage.EM_MessageNum);
				AssertEquals("EM_LinkTable", "CusEntryHeader", certificateOfOriginMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", entryHeader.PK, certificateOfOriginMessage.EM_LinkUniqueID);
			});
		}
	}
}
