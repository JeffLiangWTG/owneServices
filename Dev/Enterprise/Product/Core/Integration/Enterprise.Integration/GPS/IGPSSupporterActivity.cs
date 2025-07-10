using CargoWise.Types;

namespace Enterprise.Integration.GPS
{
	public interface IGPSSupporterActivity
	{
		ZString EN_ActivityType { get; set; }
		ZString EN_ActivityInformation { get; set; }
		ZString EN_ActivityID { get; set; }
		ZDateTime EN_ActivityTime { get; set; }
		ZString EN_AdditionalID { get; set; }
		ZString EN_EventType { get; set; }
		ZString EN_SpeedKmhMph { get; set; }
		ZDecimal EN_Latitude { get; set; }
		ZDecimal EN_Longitude { get; set; }
		ZByte EN_Speed { get; set; }
		ZShort EN_Heading { get; set; }
		ZGuid EN_RQ_Vehicle { get; set; }
		ZGuid EN_JU { get; set; }
	}
}

