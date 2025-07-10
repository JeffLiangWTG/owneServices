using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IExportGAApprovalDocument
	{
		[ID()]
		[DataItemID("G101")]
		ZString SequenceNo { get; }
		[DataItemID("G102")]
		ZString RequirementApprovalNumber { get; }
		[DataItemID("G103")]
		ZString RequirementDocumentType { get; }
		[DataItemID("G104")]
		ZString DocumentName { get; }
		[DataItemID("G105")]
		ZDate ApprovalDate { get; }
		[DataItemID("G106")]
		ZString RegulationCategoryCode { get; }
		[DataItemID("G107")]
		ZString RequirementType { get; }
		[DataItemID("G108")]
		ZString ReasonForMissingApprovalNumber { get; }
		[DataItemID("G109")]
		ZString UniqueItemID { get; }
		[DataItemID("G110")]
		ZString NonGAReasonType { get; }
	}
}
