using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICusInBondEvent
		{
			ZGuid PK { get; }
			ZGuid BN_BH { get; set; }
			ZString BN_Type { get; set; }
			ZString BN_EventPlace { get; set; }
			ZString BN_EventCountryCode { get; set; }
			ZDateTime BN_EndorsementDate { get; set; }
			ZString BN_EndorsementAuthority { get; set; }
			ZString BN_EndorsementPlace { get; set; }
			ZString BN_EndorsementCountryCode { get; set; }
			ZString BN_TransportID { get; set; }
			ZString BN_TransportCountryCode { get; set; }
			ZString BN_Information { get; set; }
			ZByte BN_NoOfSeals { get; set; }
			ZString BN_CustomsStatus { get; set; }
		}
	}
}
