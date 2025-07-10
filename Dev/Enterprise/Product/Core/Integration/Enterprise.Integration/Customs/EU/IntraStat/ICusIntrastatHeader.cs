using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class EU
		{
			public interface ICusIntrastatHeader
			{
				ZGuid PK { get; }

				ZGuid CIH_CIG_MergedGroup { get; set; }
			}
		}
	}
}
