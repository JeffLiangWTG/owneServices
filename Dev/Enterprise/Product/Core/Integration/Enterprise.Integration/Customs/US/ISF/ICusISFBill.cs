using CargoWise.Types;
namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class US
		{
			public static partial class ISF
			{
				public interface ICusISFBill
				{
					ZGuid PK { get; }
					ZGuid BB_BF { get; set; }
					ZString BB_BillNum { get; set; }
					ZString BB_BillType { get; set; }
					ZString BB_CustomsStatus { get; set; }
					ZString BB_CustomsStatusDescription { get; }
					ZDateTime BB_MatchDate { get; }
				}
			}
		}
	}
}
