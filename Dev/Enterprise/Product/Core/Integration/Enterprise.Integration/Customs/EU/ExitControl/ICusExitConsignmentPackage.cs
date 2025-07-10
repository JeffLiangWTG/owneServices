using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class EUExitControl
		{
			public interface ICusExitConsignmentPackage
			{
				ZGuid PK { get; }

				ZGuid CXP_CXH_Header { get; set; }

				ZShort CXP_Sequence { get; set; }
			}
		}
	}
}
