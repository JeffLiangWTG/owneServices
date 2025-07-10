namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class AU
		{
			public interface IFreightDataLayer : IContainerMessagingData
			{
				bool ContainerIsWaitingForResponse { get; }
			}
		}
	}
}
