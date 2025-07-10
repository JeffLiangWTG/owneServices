using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using static Enterprise.Customs.ES.Business.ESConstants;

namespace Enterprise.Customs.ES.Business
{
	public abstract class ExportCommonDocumentRequest : EntryHeaderDocumentRequest
	{
		protected ExportCommonDocumentRequest(CusEntryHeader entryHeader, ZString certName) : base(entryHeader, certName)
		{
		}

		protected override ZInt RequestMissingDocumentsCore()
		{
			var messagesSentCount = ZInt.Zero;

			if (!csvClearance.IsEmpty)
			{
				var clearanceDocMissing = IsDocumentMissing(mrnCode + DocumentCaptureRequestFileNameSuffixes.ExportClearanceDoc);

				if (clearanceDocMissing)
				{
					CreateRequest(boFactory, mrnCode + DocumentCaptureRequestFileNameSuffixes.ExportClearanceDoc, csvClearance);
					messagesSentCount++;
				}

				messagesSentCount += RequestSpecificDocuments();
			}

			messagesSentCount += RequestT2LFDocument();

			return messagesSentCount;
		}

		ZInt RequestT2LFDocument()
		{
			var messagesSentCount = ZInt.Zero;

			var csvT2L = businessObject.ZG_CSVT2L;

			if (!csvT2L.IsEmpty)
			{
				var ctStatus = declaration.ZG_CTStatusID;
				var t2lfDocMissing = IsDocumentMissing(mrnCode + DocumentCaptureRequestFileNameSuffixes.ExportT2LFDoc);

				if (t2lfDocMissing && ctStatus == ExportCommunityTransitStatusList.Codes.T2LF)
				{
					CreateRequest(boFactory, mrnCode + DocumentCaptureRequestFileNameSuffixes.ExportT2LFDoc, csvT2L);
					messagesSentCount++;
				}
			}

			return messagesSentCount;
		}

		protected abstract ZInt RequestSpecificDocuments();
	}
}
