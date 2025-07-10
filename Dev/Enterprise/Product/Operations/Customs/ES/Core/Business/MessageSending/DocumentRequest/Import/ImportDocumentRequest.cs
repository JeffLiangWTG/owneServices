using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using static Enterprise.Customs.ES.Business.ESConstants;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business;

public class ImportDocumentRequest(CusEntryHeader entryHeader, ZString certName) : ImportCommonDocumentRequest(entryHeader, certName)
{
	protected override ZInt RequestSpecificDocuments(ZString subStyle)
	{
		var messagesSentCount = ZInt.Zero;

		if (csvClearance.IsEmpty)
		{
			return messagesSentCount;
		}

		var entryInstructionIsABXZ = subStyle == EntrySubStyleList.Codes.A || subStyle == EntrySubStyleList.Codes.B || subStyle == EntrySubStyleList.Codes.X || subStyle == EntrySubStyleList.Codes.Z;

		var entryInstructionIsC = subStyle == EntrySubStyleList.Codes.C;

		var entryInstructionIsY = subStyle == EntrySubStyleList.Codes.Y;

		var entryStatus = businessObject.CH_EntryStatus;
		var cvsImportCertificate = businessObject.ZG_CSVImportCertificate;

		var clearanceDocMissing = IsDocumentMissing(mrnCode + DocumentCaptureRequestFileNameSuffixes.ImportClearanceDoc);
		var clearanceComplementaryDocMissing = IsDocumentMissing(mrnCode + DocumentCaptureRequestFileNameSuffixes.ImportClearanceComplementatyDoc);
		var certificateDocMissing = IsDocumentMissing(mrnCode + DocumentCaptureRequestFileNameSuffixes.ImportCertificateDoc);
		var certificateComplementaryDocMissing = IsDocumentMissing(mrnCode + DocumentCaptureRequestFileNameSuffixes.ImportCertificateComplementatyDoc);

		if (clearanceDocMissing && (entryInstructionIsABXZ || (entryInstructionIsC && entryStatus == EntryStatusCodes.ClearedWithPendingComplementaryDeclarations)))
		{
			CreateRequest(boFactory, mrnCode + DocumentCaptureRequestFileNameSuffixes.ImportClearanceDoc, csvClearance);
			messagesSentCount++;
		}

		if (clearanceComplementaryDocMissing && (entryInstructionIsY || (entryInstructionIsC && entryStatus == EntryStatusCodes.Cleared)))
		{
			CreateRequest(boFactory, mrnCode + DocumentCaptureRequestFileNameSuffixes.ImportClearanceComplementatyDoc, csvClearance);
			messagesSentCount++;
		}

		if (certificateDocMissing && !cvsImportCertificate.IsEmpty && (entryInstructionIsABXZ || (entryInstructionIsC && entryStatus == EntryStatusCodes.ClearedWithPendingComplementaryDeclarations)))
		{
			CreateRequest(boFactory, mrnCode + DocumentCaptureRequestFileNameSuffixes.ImportCertificateDoc, cvsImportCertificate);
			messagesSentCount++;
		}

		if (certificateComplementaryDocMissing && !cvsImportCertificate.IsEmpty && (entryInstructionIsY || (entryInstructionIsC && entryStatus == EntryStatusCodes.Cleared)))
		{
			CreateRequest(boFactory, mrnCode + DocumentCaptureRequestFileNameSuffixes.ImportCertificateComplementatyDoc, cvsImportCertificate);
			messagesSentCount++;
		}

		return messagesSentCount;
	}
}
