namespace Enterprise.Customs.EU.NCTS.Business
{
	public class ArrivalCusTransportMeansConfiguration
	{
		public IArrivalCusTransportMeansValidationDecider GetValidationDecider(ArrivalCusTransportMeans arrivalCusTransportMeans) => ArrivalCusTransportMeansValidationDeciderCore(arrivalCusTransportMeans);

		protected virtual IArrivalCusTransportMeansValidationDecider ArrivalCusTransportMeansValidationDeciderCore(ArrivalCusTransportMeans arrivalCusTransportMeans)
		{
			return arrivalCusTransportMeans.MovementHeader switch
			{
				NctsArrivalMovementHeader { IsPhase5Arrival: true } => GetArrivalPhase5ValidationDecider(),
				_ => null
			};
		}

		protected virtual IArrivalCusTransportMeansPhase5ValidationDecider GetArrivalPhase5ValidationDecider() => new ArrivalCusTransportMeansPhase5ValidationDecider();
	}
}
