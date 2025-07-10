using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using static Enterprise.Customs.ES.Business.ESConstants;

namespace Enterprise.Customs.ES.Business
{
	public class ExportDocumentRequest : ExportCommonDocumentRequest
	{
		public ExportDocumentRequest(CusEntryHeader entryHeader, ZString certName) : base(entryHeader, certName)
		{
			mrnCode = Argument.NotNullOrEmpty(entryHeader.MovementReferenceNumber, nameof(entryHeader.MovementReferenceNumber));
		}

		protected override ZInt RequestSpecificDocuments()
		{
			var messagesSentCount = ZInt.Zero;

			var eadPrintProcedure = businessObject.EUH_EADPrintProcedure;

			var eadDocMissing = IsDocumentMissing(mrnCode + DocumentCaptureRequestFileNameSuffixes.ExportAccompanyingDoc);

			if (eadDocMissing && eadPrintProcedure != EADPrintProcedureCodeList.Codes._0NoEADPrint)
			{
				CreateRequest(boFactory, mrnCode + DocumentCaptureRequestFileNameSuffixes.ExportAccompanyingDoc, mrnCode);
				messagesSentCount++;
			}

			return messagesSentCount;
		}
	}
}
