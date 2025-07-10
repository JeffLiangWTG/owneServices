using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

public class ImportH1DocumentRequestTest : ImportCommonDocumentRequestTest<ImportH1DocumentRequest>
{
	public void TestRequestMissingDocuments_CLRDoc_InstructionA_WithBroker()
	{
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var docManagerInfo = ((IDocManagerSupport)businessObject).DocManagerInfo;
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_M031.pdf", "CAU");
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_J031.pdf", "CAU");
		docManagerInfo.Save();

		declaration.Factory.Save();

		declaration.JE_GS_NKCusAgent = staff.GS_Code;

		CombineAssertions(() =>
		{
			AssertEquals(0, businessObject.Messages.Count);

			var documentRequestMessage = RequestMissingDocument();
			AssertEquals("Import H1 Document Capture Request for CLR documents can not be sent when CSV Clearance is empty so nothing will be sent", 0, documentRequestMessage);

			businessObject.SetCSVClearanceNum("ABCDEFGHIJKLMNOP");
			declaration.Factory.Save();

			documentRequestMessage = RequestMissingDocument();
			AssertEquals("Message created successfully", 1, documentRequestMessage);

			AssertDocumentRequestEDIMessage(businessObject, mrnCode + "_I_AEAT_CLR.pdf", "ABCDEFGHIJKLMNOP", certificate.CertificatePK);

			AssertNoDuplicatedDocumentRequests(mrnCode + "_I_AEAT_CLR.pdf", "ABCDEFGHIJKLMNOP", certificate.CertificatePK);
		});
	}

	public void TestRequestMissingDocuments_CLRDoc_InstructionZ()
	{
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Z;

		var docManagerInfo = ((IDocManagerSupport)businessObject).DocManagerInfo;
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_M031.pdf", "CAU");
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_J031.pdf", "CAU");
		docManagerInfo.Save();

		declaration.Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals(0, businessObject.Messages.Count);

			var documentRequestMessage = RequestMissingDocument();
			AssertEquals("Import H1 Document Capture Request for CLR documents can not be sent when CSV Clearance is empty so nothing will be sent", 0, documentRequestMessage);

			businessObject.SetCSVClearanceNum("ABCDEFGHIJKLMNOP");
			declaration.Factory.Save();

			documentRequestMessage = RequestMissingDocument();
			AssertEquals("Message created successfully", 1, documentRequestMessage);

			AssertDocumentRequestEDIMessage(businessObject, mrnCode + "_I_AEAT_CLR.pdf", "ABCDEFGHIJKLMNOP", ZGuid.Empty);

			AssertNoDuplicatedDocumentRequests(mrnCode + "_I_AEAT_CLR.pdf", "ABCDEFGHIJKLMNOP", ZGuid.Empty);
		});
	}

	public void TestRequestMissingDocuments_CLRDoc_InstructionB()
	{
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;

		var docManagerInfo = ((IDocManagerSupport)businessObject).DocManagerInfo;
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_M031.pdf", "CAU");
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_J031.pdf", "CAU");
		docManagerInfo.Save();

		declaration.Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals(0, businessObject.Messages.Count);

			var documentRequestMessage = RequestMissingDocument();
			AssertEquals("Import H1 Document Capture Request for CLR documents can not be sent when CSV Clearance is empty so nothing will be sent", 0, documentRequestMessage);

			businessObject.SetCSVClearanceNum("ABCDEFGHIJKLMNOP");
			declaration.Factory.Save();

			documentRequestMessage = RequestMissingDocument();
			AssertEquals("Import H1 Document Capture Request for CLR document with entry instruction B can not be sent when entry status is not CLP nor CLR so nothing will be sent", 0, documentRequestMessage);

			businessObject.CH_EntryStatus = "CLR";
			declaration.Factory.Save();

			documentRequestMessage = RequestMissingDocument();
			AssertEquals("Message created successfully", 1, documentRequestMessage);

			AssertDocumentRequestEDIMessage(businessObject, mrnCode + "_I_AEAT_CLR.pdf", "ABCDEFGHIJKLMNOP", ZGuid.Empty);

			AssertNoDuplicatedDocumentRequests(mrnCode + "_I_AEAT_CLR.pdf", "ABCDEFGHIJKLMNOP", ZGuid.Empty);
		});
	}

	public void TestRequestMissingDocuments_CLRDoc_InstructionC()
	{
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;

		var docManagerInfo = ((IDocManagerSupport)businessObject).DocManagerInfo;
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_M031.pdf", "CAU");
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_J031.pdf", "CAU");
		docManagerInfo.Save();

		declaration.Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals(0, businessObject.Messages.Count);

			var documentRequestMessage = RequestMissingDocument();
			AssertEquals("Import H1 Document Capture Request for CLR documents can not be sent when CSV Clearance is empty so nothing will be sent", 0, documentRequestMessage);

			businessObject.SetCSVClearanceNum("ABCDEFGHIJKLMNOP");
			declaration.Factory.Save();

			documentRequestMessage = RequestMissingDocument();
			AssertEquals("Import H1 Document Capture Request for CLR document with entry instruction C can not be sent when entry status is not CLP nor CLR so nothing will be sent", 0, documentRequestMessage);

			businessObject.CH_EntryStatus = "CLP";
			declaration.Factory.Save();

			documentRequestMessage = RequestMissingDocument();
			AssertEquals("Message created successfully", 1, documentRequestMessage);

			AssertDocumentRequestEDIMessage(businessObject, mrnCode + "_I_AEAT_CLR.pdf", "ABCDEFGHIJKLMNOP", ZGuid.Empty);

			AssertNoDuplicatedDocumentRequests(mrnCode + "_I_AEAT_CLR.pdf", "ABCDEFGHIJKLMNOP", ZGuid.Empty);
		});
	}

	public void TestRequestMissingDocuments_CERDoc_InstructionA()
	{
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var docManagerInfo = ((IDocManagerSupport)businessObject).DocManagerInfo;
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_aeat_M031.PDF", "CAU");
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_J031.pdf", "CAU");
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_aeat_CLR.PDF", "CLR");
		docManagerInfo.Save();

		declaration.Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals(0, businessObject.Messages.Count);

			var documentRequestMessage = RequestMissingDocument();
			AssertEquals("Import H1 Document Capture Request for CER documents can not be sent when CSV Clearance is empty so nothing will be sent", 0, documentRequestMessage);

			businessObject.SetCSVClearanceNum("ABCDEFGHIJKLMNOP");
			declaration.Factory.Save();

			documentRequestMessage = RequestMissingDocument();
			AssertEquals("Import H1 Document Capture Request for CER document with can not be sent when CSV Import Certificate is empty so nothing will be sent", 0, documentRequestMessage);

			businessObject.ZG_CSVImportCertificate = "ZYXWVUTSRQPONMLK";
			declaration.Factory.Save();

			documentRequestMessage = RequestMissingDocument();
			AssertEquals("Message created successfully", 1, documentRequestMessage);

			AssertDocumentRequestEDIMessage(businessObject, mrnCode + "_I_AEAT_CER.pdf", "ZYXWVUTSRQPONMLK", ZGuid.Empty);

			AssertNoDuplicatedDocumentRequests(mrnCode + "_I_AEAT_CER.pdf", "ZYXWVUTSRQPONMLK", ZGuid.Empty);
		});
	}

	public void TestRequestMissingDocuments_CERDoc_InstructionZ()
	{
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Z;

		var docManagerInfo = ((IDocManagerSupport)businessObject).DocManagerInfo;
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_aeat_M031.PDF", "CAU");
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_J031.pdf", "CAU");
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_aeat_CLR.PDF", "CLR");
		docManagerInfo.Save();

		declaration.Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals(0, businessObject.Messages.Count);

			var documentRequestMessage = RequestMissingDocument();
			AssertEquals("Import H1 Document Capture Request for CER documents can not be sent when CSV Clearance is empty so nothing will be sent", 0, documentRequestMessage);

			businessObject.SetCSVClearanceNum("ABCDEFGHIJKLMNOP");
			declaration.Factory.Save();

			documentRequestMessage = RequestMissingDocument();
			AssertEquals("Import H1 Document Capture Request for CER document with can not be sent when CSV Import Certificate is empty so nothing will be sent", 0, documentRequestMessage);

			businessObject.ZG_CSVImportCertificate = "ZYXWVUTSRQPONMLK";
			declaration.Factory.Save();

			documentRequestMessage = RequestMissingDocument();
			AssertEquals("Message created successfully", 1, documentRequestMessage);

			AssertDocumentRequestEDIMessage(businessObject, mrnCode + "_I_AEAT_CER.pdf", "ZYXWVUTSRQPONMLK", ZGuid.Empty);

			AssertNoDuplicatedDocumentRequests(mrnCode + "_I_AEAT_CER.pdf", "ZYXWVUTSRQPONMLK", ZGuid.Empty);
		});
	}

	public void TestRequestMissingDocuments_CERDoc_InstructionB()
	{
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;

		var docManagerInfo = ((IDocManagerSupport)businessObject).DocManagerInfo;
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_M031.pdf", "CAU");
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_J031.pdf", "CAU");
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_CLR.pdf", "CLR");
		docManagerInfo.Save();

		declaration.Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals(0, businessObject.Messages.Count);

			var documentRequestMessage = RequestMissingDocument();
			AssertEquals("Import H1 Document Capture Request for CER documents can not be sent when CSV Clearance is empty so nothing will be sent", 0, documentRequestMessage);

			businessObject.SetCSVClearanceNum("ABCDEFGHIJKLMNOP");
			declaration.Factory.Save();

			documentRequestMessage = RequestMissingDocument();
			AssertEquals("Import H1 Document Capture Request for CER document with can not be sent when CSV Import Certificate is empty so nothing will be sent", 0, documentRequestMessage);

			businessObject.ZG_CSVImportCertificate = "ZYXWVUTSRQPONMLK";
			declaration.Factory.Save();

			documentRequestMessage = RequestMissingDocument();
			AssertEquals("Import H1 Document Capture Request for CER document with entry instruction B can not be sent when entry status is not CLP nor CLR so nothing will be sent", 0, documentRequestMessage);

			businessObject.CH_EntryStatus = "CLR";
			declaration.Factory.Save();

			documentRequestMessage = RequestMissingDocument();
			AssertEquals("Message created successfully", 1, documentRequestMessage);

			AssertDocumentRequestEDIMessage(businessObject, mrnCode + "_I_AEAT_CER.pdf", "ZYXWVUTSRQPONMLK", ZGuid.Empty);

			AssertNoDuplicatedDocumentRequests(mrnCode + "_I_AEAT_CER.pdf", "ZYXWVUTSRQPONMLK", ZGuid.Empty);
		});
	}

	public void TestRequestMissingDocuments_CERDoc_InstructionC()
	{
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;

		var docManagerInfo = ((IDocManagerSupport)businessObject).DocManagerInfo;
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_M031.pdf", "CAU");
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_J031.pdf", "CAU");
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_CLR.pdf", "CLR");
		docManagerInfo.Save();

		declaration.Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals(0, businessObject.Messages.Count);

			var documentRequestMessage = RequestMissingDocument();
			AssertEquals("Import H1 Document Capture Request for CER documents can not be sent when CSV Clearance is empty so nothing will be sent", 0, documentRequestMessage);

			businessObject.SetCSVClearanceNum("ABCDEFGHIJKLMNOP");
			declaration.Factory.Save();

			documentRequestMessage = RequestMissingDocument();
			AssertEquals("Import H1 Document Capture Request for CER document with can not be sent when CSV Import Certificate is empty so nothing will be sent", 0, documentRequestMessage);

			businessObject.ZG_CSVImportCertificate = "ZYXWVUTSRQPONMLK";
			declaration.Factory.Save();

			documentRequestMessage = RequestMissingDocument();
			AssertEquals("Import H1 Document Capture Request for CER document with entry instruction C can not be sent when entry status is not CLP nor CLR so nothing will be sent", 0, documentRequestMessage);

			businessObject.CH_EntryStatus = "CLP";
			declaration.Factory.Save();

			documentRequestMessage = RequestMissingDocument();
			AssertEquals("Message created successfully", 1, documentRequestMessage);

			AssertDocumentRequestEDIMessage(businessObject, mrnCode + "_I_AEAT_CER.pdf", "ZYXWVUTSRQPONMLK", ZGuid.Empty);

			AssertNoDuplicatedDocumentRequests(mrnCode + "_I_AEAT_CER.pdf", "ZYXWVUTSRQPONMLK", ZGuid.Empty);
		});
	}

	public void TestRequestMissingDocuments_CERDoc_InstructionX_CsvClearanceEmpty()
	{
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.X;

		var docManagerInfo = ((IDocManagerSupport)businessObject).DocManagerInfo;
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_M031.pdf", "CAU");
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_J031.pdf", "CAU");
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_CLR.pdf", "CLR");
		docManagerInfo.Save();

		declaration.Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals(0, businessObject.Messages.Count);

			var documentRequestMessage = RequestMissingDocument();
			AssertEquals("Import H1 Document Capture Request for CER document with can not be sent when CSV Import Certificate is empty so nothing will be sent", 0, documentRequestMessage);

			businessObject.ZG_CSVImportCertificate = "ZYXWVUTSRQPONMLK";
			declaration.Factory.Save();

			documentRequestMessage = RequestMissingDocument();
			AssertEquals("Message created successfully", 1, documentRequestMessage);

			AssertDocumentRequestEDIMessage(businessObject, mrnCode + "_I_AEAT_CER.pdf", "ZYXWVUTSRQPONMLK", ZGuid.Empty);

			AssertNoDuplicatedDocumentRequests(mrnCode + "_I_AEAT_CER.pdf", "ZYXWVUTSRQPONMLK", ZGuid.Empty);
		});
	}

	public void TestRequestMissingDocuments_CERDoc_InstructionY_CsvClearanceEmpty()
	{
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Y;

		var docManagerInfo = ((IDocManagerSupport)businessObject).DocManagerInfo;
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_M031.pdf", "CAU");
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_J031.pdf", "CAU");
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_CLR.pdf", "CLR");
		docManagerInfo.Save();

		declaration.Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals(0, businessObject.Messages.Count);

			var documentRequestMessage = RequestMissingDocument();
			AssertEquals("Import H1 Document Capture Request for CER document with can not be sent when CSV Import Certificate is empty so nothing will be sent", 0, documentRequestMessage);

			businessObject.ZG_CSVImportCertificate = "ZYXWVUTSRQPONMLK";
			declaration.Factory.Save();

			documentRequestMessage = RequestMissingDocument();
			AssertEquals("Message created successfully", 1, documentRequestMessage);

			AssertDocumentRequestEDIMessage(businessObject, mrnCode + "_I_AEAT_CER.pdf", "ZYXWVUTSRQPONMLK", ZGuid.Empty);

			AssertNoDuplicatedDocumentRequests(mrnCode + "_I_AEAT_CER.pdf", "ZYXWVUTSRQPONMLK", ZGuid.Empty);
		});
	}

	public override void TestRequestMissingDocuments_M031Doc()
	{
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		var docManagerInfo = ((IDocManagerSupport)businessObject).DocManagerInfo;
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_J031.pdf", "CAU");
		docManagerInfo.Save();

		CombineAssertions(() =>
		{
			AssertEquals(0, businessObject.Messages.Count);

			var documentRequestMessage = RequestMissingDocument();
			AssertEquals("Message created successfully", 1, documentRequestMessage);

			AssertDocumentRequestEDIMessage(businessObject, mrnCode + "_I_AEAT_M031.pdf", mrnCode, ZGuid.Empty);

			AssertNoDuplicatedDocumentRequests(mrnCode + "_I_AEAT_M031.pdf", mrnCode, ZGuid.Empty);
		});
	}

	public override void TestRequestMissingDocuments_M032Doc()
	{
		CanaryIslandSetUp();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Z;
		declaration.ZG_DestinationState = CanaryIslandCode;
		businessObject.Factory.Save();

		var docManagerInfo = ((IDocManagerSupport)businessObject).DocManagerInfo;
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_M031.pdf", "CAU");
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_J031.pdf", "CAU");
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_J032.pdf", "CAU");
		docManagerInfo.Save();

		CombineAssertions(() =>
		{
			AssertEquals(0, businessObject.Messages.Count);

			var documentRequestMessage = RequestMissingDocument();
			AssertEquals("Message created successfully", 1, documentRequestMessage);

			AssertDocumentRequestEDIMessage(businessObject, mrnCode + "_I_AEAT_M032.pdf", mrnCode, ZGuid.Empty);

			AssertNoDuplicatedDocumentRequests(mrnCode + "_I_AEAT_M032.pdf", mrnCode, ZGuid.Empty);
		});
	}

	public override void TestRequestMissingDocuments_J031Doc()
	{
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.X;
		var docManagerInfo = ((IDocManagerSupport)businessObject).DocManagerInfo;
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_M031.pdf", "CAU");
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_M032.pdf", "CAU");
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_J032.pdf", "CAU");
		docManagerInfo.Save();

		CombineAssertions(() =>
		{
			AssertEquals(0, businessObject.Messages.Count);

			var documentRequestMessage = RequestMissingDocument();
			AssertEquals("Message created successfully", 1, documentRequestMessage);

			AssertDocumentRequestEDIMessage(businessObject, mrnCode + "_I_AEAT_J031.pdf", mrnCode, ZGuid.Empty);

			AssertNoDuplicatedDocumentRequests(mrnCode + "_I_AEAT_J031.pdf", mrnCode, ZGuid.Empty);
		});
	}

	public override void TestRequestMissingDocuments_J032Doc()
	{
		CanaryIslandSetUp();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Y;
		declaration.ZG_DestinationState = CanaryIslandCode;
		businessObject.Factory.Save();

		var docManagerInfo = ((IDocManagerSupport)businessObject).DocManagerInfo;
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_M031.pdf", "CAU");
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_M032.pdf", "CAU");
		docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_I_AEAT_J031.pdf", "CAU");
		docManagerInfo.Save();

		CombineAssertions(() =>
		{
			AssertEquals(0, businessObject.Messages.Count);

			var documentRequestMessage = RequestMissingDocument();
			AssertEquals("Message created successfully", 1, documentRequestMessage);

			AssertDocumentRequestEDIMessage(businessObject, mrnCode + "_I_AEAT_J032.pdf", mrnCode, ZGuid.Empty);

			AssertNoDuplicatedDocumentRequests(mrnCode + "_I_AEAT_J032.pdf", mrnCode, ZGuid.Empty);
		});
	}

	protected override ImportH1DocumentRequest GetDocumentRequestClass(CusEntryHeader businessObject, ZString certName) => new(businessObject, certName);
}
