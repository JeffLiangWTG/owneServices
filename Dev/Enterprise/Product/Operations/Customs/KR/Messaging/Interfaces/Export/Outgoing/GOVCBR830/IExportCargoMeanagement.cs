using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IExportCargoMeanagement
	{
		[ID()]
		[DataItemID("F101")]
		ZString ImportCargoManagementNumber { get; }
	}
}
