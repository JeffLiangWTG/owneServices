using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class ASYCUDA
		{
			public interface ICusPerson
			{
				bool IsInDatabase { get; }
				ZGuid PK { get; }
				ZBool CPN_IsPassenger { get; set; }
				ZGuid CPN_ParentID { get; set; }
				ZString CPN_ParentTableCode { get; set; }
				ZGuid CPN_PER_Person { get; set; }
			}
		}
	}
}
