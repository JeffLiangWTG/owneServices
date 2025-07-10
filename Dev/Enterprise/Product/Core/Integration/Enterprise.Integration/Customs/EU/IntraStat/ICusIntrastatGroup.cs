using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class EU
		{
			public interface ICusIntrastatGroup
			{
				ZGuid PK { get; }

				ZString CIG_Flow { get; set; }
			}
		}
	}
}
