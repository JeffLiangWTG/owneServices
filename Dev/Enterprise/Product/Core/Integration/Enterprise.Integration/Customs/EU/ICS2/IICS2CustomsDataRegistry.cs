namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class EUICS2
		{
			public interface IICS2CustomsDataRegistry
			{
				IRegistryItem EnableICS2ErrorsTo { get; }
				IRegistryItem EnableICS2AcknowledgementsTo { get; }
				IRegistryItem EnableICS2RequestsTo { get; }
				IRegistryItem EnableICS2UnmatchedOrNotSentTo { get; }
			}
		}
	}
}
