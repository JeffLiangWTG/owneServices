using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IExportContainer
	{
		[ID()]
		ZString SequenceNo { get; }
		[DataItemID("D101")]
		ZString ContainerNo { get; }
	}
}
