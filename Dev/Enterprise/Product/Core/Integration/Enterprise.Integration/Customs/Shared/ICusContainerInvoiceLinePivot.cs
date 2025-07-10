using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface ICusContainerInvoiceLinePivot
			{
				ZGuid PK { get; }
				ZGuid C2_CO { get; set; }
				ZDecimal C2_GrossWeight { get; set; }
				ZDecimal C2_NetWeight { get; set; }
				ZInt C2_PackQty { get; set; }
				ZDecimal C2_SplitValue { get; set; }
			}
		}
	}
}