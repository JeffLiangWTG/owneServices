using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class CA
		{
			public interface ICusCAeMHHouseContainerPivot
			{
				ZGuid PK { get; }
				ZGuid BPA_BQ_Container { get; set; }
				ZGuid BPA_BW_House { get; set; }
			}
		}
	}
}
