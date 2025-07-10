using CargoWise.Integration;

namespace Enterprise.Client.UPE.Business
{
	public class CargoReportQueueCodeDescriptionPairList : AutoCargoReportQueueCodeDescriptionPairList
	{
		public CargoReportQueueCodeDescriptionPairList()
		{
			MoveToLast(Codes.Intervention);
			MoveToLast(Codes.Pending);
			MoveToLast(Codes.Unknown);
			MoveToLast(Codes.Hold);
			MoveToLast(Codes.EIR);
			MoveToLast(Codes.AwaitingDeclaration);
			MoveToLast(Codes.AwaitingEvaluation);
			MoveToLast(Codes.Quarantine);
			MoveToLast(Codes.Completed);
		}

		void MoveToLast(string code)
		{
			ICodeDescription item = this[code];
			Remove(item);
			Add(item);
		}
	}
}
