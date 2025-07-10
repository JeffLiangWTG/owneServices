using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageProcessors
{
	public class NctsClearanceEmailObject : INctsClearanceEmailProvider
	{
		public NctsClearanceEmailObject(ZString csvClearance, ZString clearanceProcedure, ZDateTime clearanceDate, ZDateTime arrivalLimitDate)
		{
			CSVClearance = csvClearance;
			ClearanceProcedure = clearanceProcedure;
			ClearanceDate = clearanceDate;
			ArrivalLimitDate = arrivalLimitDate;
		}

		public ZString CSVClearance { get; }
		public ZString ClearanceProcedure { get; }
		public ZDateTime ClearanceDate { get; }
		public ZDateTime ArrivalLimitDate { get; }
	}
}
