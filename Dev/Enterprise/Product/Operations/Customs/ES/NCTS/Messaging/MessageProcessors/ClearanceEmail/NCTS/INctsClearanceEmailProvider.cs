using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageProcessors
{
	public interface INctsClearanceEmailProvider
	{
		ZString CSVClearance { get; }
		ZString ClearanceProcedure { get; }
		ZDateTime ClearanceDate { get; }
		ZDateTime ArrivalLimitDate { get; }
	}
}
