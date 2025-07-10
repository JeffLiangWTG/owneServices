using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class ASYCUDA
		{
			public interface ICusPersonCountry
			{
				bool IsInDatabase { get; }
				ZGuid PK { get; }
				ZGuid CPC_CPN_Person { get; set; }
				ZString CPC_RN_NKCountry { get; set; }
				ZString CPC_Type { get; set; }
				ZString CPC_Value { get; set; }
			}
		}
	}
}
