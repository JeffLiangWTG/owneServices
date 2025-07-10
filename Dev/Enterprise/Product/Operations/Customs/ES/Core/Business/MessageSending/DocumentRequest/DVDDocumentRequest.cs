using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using static Enterprise.Customs.ES.Business.ESConstants;

namespace Enterprise.Customs.ES.Business
{
	public class DVDDocumentRequest : EntryHeaderDocumentRequest
	{
		public DVDDocumentRequest(CusEntryHeader entryHeader, ZString certName) : base(entryHeader, certName)
		{
			mrnCode = Argument.NotNullOrEmpty(entryHeader.MovementReferenceNumber, nameof(entryHeader.MovementReferenceNumber));
		}

		protected override ZInt RequestMissingDocumentsCore()
		{
			var messagesSentCount = ZInt.Zero;

			if (!csvClearance.IsEmpty)
			{
				var clearanceDocMissing = IsDocumentMissing(mrnCode + DocumentCaptureRequestFileNameSuffixes.DVDClearanceDoc);

				if (clearanceDocMissing)
				{
					CreateRequest(boFactory, mrnCode + DocumentCaptureRequestFileNameSuffixes.DVDClearanceDoc, csvClearance);
					messagesSentCount++;
				}
			}

			return messagesSentCount;
		}
	}
}
