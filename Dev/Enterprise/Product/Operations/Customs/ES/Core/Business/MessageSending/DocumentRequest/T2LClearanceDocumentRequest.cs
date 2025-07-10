using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using static Enterprise.Customs.ES.Business.ESConstants;

namespace Enterprise.Customs.ES.Business
{
	public class T2LClearanceDocumentRequest : EntryHeaderDocumentRequest
	{
		public T2LClearanceDocumentRequest(CusEntryHeader entryHeader, ZString certName) : base(entryHeader, certName)
		{
			mrnCode = Argument.NotNullOrEmpty(entryHeader.T2CMovementReferenceNumber, nameof(entryHeader.T2CMovementReferenceNumber));
		}

		protected override ZInt RequestMissingDocumentsCore()
		{
			var messagesSentCount = ZInt.Zero;

			if (!csvClearance.IsEmpty)
			{
				var clearanceDocMissing = IsDocumentMissing(mrnCode + DocumentCaptureRequestFileNameSuffixes.T2LClearanceDoc);

				if (clearanceDocMissing)
				{
					CreateRequest(boFactory, mrnCode + DocumentCaptureRequestFileNameSuffixes.T2LClearanceDoc, csvClearance);
					messagesSentCount++;
				}
			}

			return messagesSentCount;
		}
	}
}
