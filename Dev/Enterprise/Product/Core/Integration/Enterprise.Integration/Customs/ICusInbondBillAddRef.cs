using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICusInbondBillAddRef
		{
			ZGuid PK { get; }
			ZString BR_Qualifier { get; set; }
			ZString BR_ReferenceNum { get; set; }
			ZGuid BR_B0 { get; set; }
		}
	}
}
