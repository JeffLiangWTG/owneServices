using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class JP
		{
			public static partial class AFR
			{
				public interface IJPAFRBills : IBusiness
				{
					ZGuid PK { get; }
					ZString JPB_ReleaseStatus { get; set; }
					ZString JPB_BillNumber { get; set; }
					ZGuid JPB_JPH_Header { get; set; }
				}
			}
		}
	}
}