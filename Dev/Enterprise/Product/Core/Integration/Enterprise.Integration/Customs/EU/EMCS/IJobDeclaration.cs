using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class EUEMCS
		{
			public interface IJobDeclaration : IBaseJobDeclaration
			{
				ZBool IsMessageStatusSentOrAcknowledged { get; }
			}
		}
	}
}
