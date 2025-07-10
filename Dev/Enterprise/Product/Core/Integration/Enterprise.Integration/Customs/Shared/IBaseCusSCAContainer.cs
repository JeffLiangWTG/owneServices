using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface IBaseCusSCAContainer
			{
				ZString CN_ContainerNumber { get; set; }
				ZGuid CN_CB { get; set;  }
			}
		}
	}
}
