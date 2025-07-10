using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Messaging
{
	[CodeAlive("Soon to be used")]
	public interface IImportImmediateDelivery
	{
		[ID()]
		[DataItemID(ImportAmendmentDataItemIDList.Codes.I101)]
		ZInt SequenceNo { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.I102)]
		ZString ImmediateDeliveryNo { get; }
	}
}
