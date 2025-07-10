using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Messaging
{
	[CodeAlive("Soon to be used")]
	public interface IImportOnlineOrder
	{
		[DataItemID(ImportAmendmentDataItemIDList.Codes.J102)]
		ZString OnlineOrderNo { get; }
		[ID()]
		[DataItemID(ImportAmendmentDataItemIDList.Codes.J101)]
		ZInt SequenceNo { get; }
	}
}
