using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using static Enterprise.Customs.ES.Business.ESConstants;

namespace Enterprise.Customs.ES.Business;

public abstract class ImportCommonDocumentRequest : EntryHeaderDocumentRequest
{
	public ImportCommonDocumentRequest(CusEntryHeader entryHeader, ZString certName) : base(entryHeader, certName)
	{
		mrnCode = Argument.NotNullOrEmpty(entryHeader.MovementReferenceNumber, nameof(entryHeader.MovementReferenceNumber));
	}

	protected virtual bool CAUDocTypeExtraCondition(ZString subStyle) => true;

	protected override ZInt RequestMissingDocumentsCore()
	{
		var messagesSentCount = ZInt.Zero;

		var paymentLetterAEATDocMissing = IsDocumentMissing(mrnCode + DocumentCaptureRequestFileNameSuffixes.ImportPaymentLetterAEATDoc);
		var paymentLetterATCDocMissing = IsDocumentMissing(mrnCode + DocumentCaptureRequestFileNameSuffixes.ImportPaymentLetterATCDoc);
		var proofOfPaymentAEATDocMissing = IsDocumentMissing(mrnCode + DocumentCaptureRequestFileNameSuffixes.ImportProofOfPaymentAEATDoc);
		var proofOfPaymentATCDocMissing = IsDocumentMissing(mrnCode + DocumentCaptureRequestFileNameSuffixes.ImportProofOfPaymentATCDoc);

		var isCanaryIslands = declaration.DestinationStateIsCanaryIsland;

		var subStyle = businessObject.EntryInstruction?.CEI_SubStyle ?? ZString.Empty;

		if (CAUDocTypeExtraCondition(subStyle))
		{
			if (paymentLetterAEATDocMissing)
			{
				CreateRequest(boFactory, mrnCode + DocumentCaptureRequestFileNameSuffixes.ImportPaymentLetterAEATDoc, mrnCode);
				messagesSentCount++;
			}

			if (paymentLetterATCDocMissing && isCanaryIslands)
			{
				CreateRequest(boFactory, mrnCode + DocumentCaptureRequestFileNameSuffixes.ImportPaymentLetterATCDoc, mrnCode);
				messagesSentCount++;
			}

			if (proofOfPaymentAEATDocMissing)
			{
				CreateRequest(boFactory, mrnCode + DocumentCaptureRequestFileNameSuffixes.ImportProofOfPaymentAEATDoc, mrnCode);
				messagesSentCount++;
			}

			if (proofOfPaymentATCDocMissing && isCanaryIslands)
			{
				CreateRequest(boFactory, mrnCode + DocumentCaptureRequestFileNameSuffixes.ImportProofOfPaymentATCDoc, mrnCode);
				messagesSentCount++;
			}
		}

		messagesSentCount += RequestSpecificDocuments(subStyle);

		return messagesSentCount;
	}

	protected abstract ZInt RequestSpecificDocuments(ZString subStyle);
}
