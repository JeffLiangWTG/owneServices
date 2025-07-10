using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class EU
		{
			public interface ICusIntrastatLine
			{
				ZGuid PK { get; }

				ZGuid CIL_CIH_Header { get; set; }

				ZGuid CIL_CIM_MergedLine { get; set; }

				ZString CIL_Tariff { get; set; }
			}
		}
	}
}
