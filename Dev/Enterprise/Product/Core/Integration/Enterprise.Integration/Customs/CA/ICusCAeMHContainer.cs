using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class CA
		{
			public interface ICusCAeMHContainer
			{
				ZGuid PK { get; }
				ZGuid BQ_BP_Master { get; set; }
			}
		}
	}
}
