using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageProcessors
{
	public interface IExportClearanceEmailProvider
	{
		ZString CSVClearance { get; }
		ZDateTime ReleaseDate { get; }
		ZDateTime LimitDateOfArrival { get; }
		ZString ClearanceResult { get; }
	}
}
