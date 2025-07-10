using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImportNonGADetail
	{
		[ID()]
		ZInt SequenceNo { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.E104)]
		ZString ReasonType { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.E103)]
		ZString RegulationCategoryCode { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.E107)]
		ZString NonGAReasonType { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.E105)]
		ZString Reason { get; }
	}
}
