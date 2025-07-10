namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface IAdditionalTransportMeansProvider
	{
		IDepartureCusTransportMeansCollection<DepartureCusTransportMeans> AdditionalTransportAtBorderList { get; }

		void ValidateAdditionalTransportAtBorderListCount();
	}
}
