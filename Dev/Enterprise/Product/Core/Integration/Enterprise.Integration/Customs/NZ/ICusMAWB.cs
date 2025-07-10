using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class NZ
		{
			public interface ICusMAWB : Shared.ICusMAWB
			{
				ZInt ConsignmentCount { get; }
			}
		}
	}
}
