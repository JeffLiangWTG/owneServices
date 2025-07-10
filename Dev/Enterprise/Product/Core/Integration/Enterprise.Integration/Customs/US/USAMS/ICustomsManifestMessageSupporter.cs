using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class US
		{
			public static partial class USAMS
			{
				public interface ICustomsManifestMessageSupporter : IBaseAutoSendingMessageSupporter
				{
					IProcessor CreateManifestMessageProcessor();
				}
			}
		}
	}
}
