using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class CA
		{
			public interface ICusCAeMHHouse
			{
				ZGuid PK { get; }
				ZGuid BW_BP_Master { get; set; }
			}
		}
	}
}
