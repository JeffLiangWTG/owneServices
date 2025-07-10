using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class ASYCUDA
		{
			public static partial class SGAccess
			{
				public partial interface IAsycudaBill
				{
					ZDateTime BatchDate { get; set; }
					ZString BatchNumber { get; set; }
					ZDateTime CycleDate { get; set; }
					ZString CycleNumber { get; set; }
					ZString SG_PartyID { get; set; }
					ZString SG_PartyStatus { get; set; }
					ZString SG_PayeeIndicator { get; set; }
				}
			}
		}
	}
}