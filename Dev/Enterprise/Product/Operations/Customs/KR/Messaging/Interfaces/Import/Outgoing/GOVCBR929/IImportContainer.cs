using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Messaging
{
	[CodeAlive("Soon to be used")]
	public interface IImportContainer
	{
		[ID()]
		[DataItemID(ImportAmendmentDataItemIDList.Codes.H102)]
		ZInt SequenceNo { get; }
		[DataItemID(ImportAmendmentDataItemIDList.Codes.H101)]
		ZString ContainerNo { get; }
	}
}
