using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImportGAApprovalDocument
	{
		[ID()]
		ZInt SequenceNo { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.D106)]
		ZString RequirementDocumentType { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.D102)]
		ZString RequirementApprovalNumber { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.D103)]
		ZString RegulationCategoryCode { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.D108)]
		ZString DocumentName { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.D107)]
		ZDate ApprovalDate { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.D104)]
		ZString UseCode { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.D105)]
		ZString UniqueItemID { get; }
	}
}
