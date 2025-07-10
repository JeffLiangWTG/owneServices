using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using static Enterprise.Customs.ES.Business.ESConstants;

namespace Enterprise.Customs.ES.Business
{
	public class EXSDocumentRequest : EntryHeaderDocumentRequest
	{
		public EXSDocumentRequest(CusEntryHeader entryHeader, ZString certName) : base(entryHeader, certName)
		{
			mrnCode = Argument.NotNullOrEmpty(entryHeader.MovementReferenceNumber, nameof(entryHeader.MovementReferenceNumber));
		}

		protected override ZInt RequestMissingDocumentsCore()
		{
			var messagesSentCount = ZInt.Zero;

			if (!csvClearance.IsEmpty)
			{
				var clearanceDocMissing = IsDocumentMissing(mrnCode + DocumentCaptureRequestFileNameSuffixes.EXSClearanceDoc);

				if (clearanceDocMissing)
				{
					CreateRequest(boFactory, mrnCode + DocumentCaptureRequestFileNameSuffixes.EXSClearanceDoc, csvClearance);
					messagesSentCount++;
				}
			}

			return messagesSentCount;
		}
	}
}
