using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageProcessors
{
	public class ExportClearanceEmailObject : IExportClearanceEmailProvider
	{
		public ExportClearanceEmailObject(ZString csvClearance, ZDateTime releaseDate, ZDateTime limitDateOfArrival, ZString clearanceResult)
		{
			CSVClearance = csvClearance;
			ReleaseDate = releaseDate;
			LimitDateOfArrival = limitDateOfArrival;
			ClearanceResult = clearanceResult;
		}

		public ZString CSVClearance { get; }
		public ZDateTime ReleaseDate { get; }
		public ZDateTime LimitDateOfArrival { get; }
		public ZString ClearanceResult { get; }
	}
}
