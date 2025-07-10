namespace Enterprise.Integration;

public static partial class Customs
{
	public static partial class PL
	{
		public interface IMessageProcessorFactoryLegacy
		{
			object CreateProcessor(IEDIMessage message);
		}
	}
}
