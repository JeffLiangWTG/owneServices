using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class EU
		{
			public interface ICusIntrastatMergedLine
			{
				ZGuid PK { get; }

				ZGuid CIM_CIG_Group { get; set; }

				ZGuid CIM_CIH_Header { get; set; }

				ZGuid CIM_OH_Trader { get; set; }

				ZInt CIM_ClusterKey { get; set; }

				ZDecimal CIM_InvoiceValue { get; set; }

				ZInt CIM_MassInKilograms { get; set; }

				ZString CIM_MemberState { get; set; }

				ZString CIM_Tariff { get; set; }
			}
		}
	}
}
