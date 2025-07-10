using System.Collections.Immutable;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using static Enterprise.Customs.ES.Business.ESConstants;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business;

public class ImportH1DocumentRequest(CusEntryHeader entryHeader, ZString certName) : ImportCommonDocumentRequest(entryHeader, certName)
{
	static readonly ImmutableHashSet<string> subStyleAorZ = ImmutableHashSet.Create(EntrySubStyleList.Codes.A, EntrySubStyleList.Codes.Z);

	static readonly ImmutableHashSet<string> subStyleBorC = ImmutableHashSet.Create(EntrySubStyleList.Codes.B, EntrySubStyleList.Codes.C);

	static readonly ImmutableHashSet<string> subStyleXorY = ImmutableHashSet.Create(EntrySubStyleList.Codes.X, EntrySubStyleList.Codes.Y);

	static readonly ImmutableHashSet<string> entryStatusCLPorCLR = ImmutableHashSet.Create(EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, EntryStatusCodes.Cleared);

	protected override bool CAUDocTypeExtraCondition(ZString subStyle) => subStyleAorZ.Contains(subStyle) || subStyleXorY.Contains(subStyle);

	protected override ZInt RequestSpecificDocuments(ZString subStyle)
	{
		var messagesSentCount = ZInt.Zero;

		var entryInstructionIsAorZ = subStyleAorZ.Contains(subStyle);
		var entryInstructionIsBorC = subStyleBorC.Contains(subStyle);
		var entryInstructionIsXorY = subStyleXorY.Contains(subStyle);

		var entryStatusIsCLPorCLR = entryStatusCLPorCLR.Contains(businessObject.CH_EntryStatus);
		var cvsImportCertificate = businessObject.ZG_CSVImportCertificate;

		var clearanceDocMissing = IsDocumentMissing(mrnCode + DocumentCaptureRequestFileNameSuffixes.ImportClearanceDoc);
		var certificateDocMissing = IsDocumentMissing(mrnCode + DocumentCaptureRequestFileNameSuffixes.ImportCertificateDoc);
		var instructionAorZ_or_instructionBorCAndStatusCLPorCLR = entryInstructionIsAorZ || (entryInstructionIsBorC && entryStatusIsCLPorCLR);

		if (!csvClearance.IsEmpty)
		{
			if (clearanceDocMissing && instructionAorZ_or_instructionBorCAndStatusCLPorCLR)
			{
				CreateRequest(boFactory, mrnCode + DocumentCaptureRequestFileNameSuffixes.ImportClearanceDoc, csvClearance);
				messagesSentCount++;
			}
		}

		if (certificateDocMissing && !cvsImportCertificate.IsEmpty && ((!csvClearance.IsEmpty && instructionAorZ_or_instructionBorCAndStatusCLPorCLR) || entryInstructionIsXorY))
		{
			CreateRequest(boFactory, mrnCode + DocumentCaptureRequestFileNameSuffixes.ImportCertificateDoc, cvsImportCertificate);
			messagesSentCount++;
		}

		return messagesSentCount;
	}
}
