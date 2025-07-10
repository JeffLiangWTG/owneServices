using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using static Enterprise.Customs.ES.Business.ESConstants;

namespace Enterprise.Customs.ES.Business
{
	public class ExportAESDocumentRequest : ExportCommonDocumentRequest
	{
		public ExportAESDocumentRequest(CusEntryHeader entryHeader, ZString certName) : base(entryHeader, certName)
		{
			mrnCode = Argument.NotNullOrEmpty(entryHeader.MovementReferenceNumber, nameof(entryHeader.MovementReferenceNumber));
		}

		protected override ZInt RequestSpecificDocuments()
		{
			var messagesSentCount = ZInt.Zero;

			var exitCertificateDocMissing = IsDocumentMissing(mrnCode + DocumentCaptureRequestFileNameSuffixes.ExportAESCertificateEffectiveDepartureDoc);

			if (exitCertificateDocMissing && !businessObject.ZG_CSVExitCertificate.IsEmpty)
			{
				CreateRequest(boFactory, mrnCode + DocumentCaptureRequestFileNameSuffixes.ExportAESCertificateEffectiveDepartureDoc, businessObject.ZG_CSVExitCertificate);
				messagesSentCount++;
			}

			var eadDocMissing = IsDocumentMissing(mrnCode + DocumentCaptureRequestFileNameSuffixes.ExportAccompanyingDoc);

			if (eadDocMissing && businessObject.IndirectExport)
			{
				CreateRequest(boFactory, mrnCode + DocumentCaptureRequestFileNameSuffixes.ExportAccompanyingDoc, mrnCode);
				messagesSentCount++;
			}

			return messagesSentCount;
		}
	}
}
