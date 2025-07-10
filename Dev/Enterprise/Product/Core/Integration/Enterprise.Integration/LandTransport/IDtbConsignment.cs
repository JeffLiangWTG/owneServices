using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Integration.Customs;

namespace Enterprise.Integration.LandTransport
{
	public interface IDtbConsignment : IBusiness
	{
		ZGuid PK { get; }
		ZGuid JobHeaderPK { get; }
		ZGuid LTC_KM_Booking { get; set; }
		ICusEntryNumAdditionalReferenceCollection AdditionalReferenceNumbers { get; }
		ZString LTC_Direction { get; set; }
		ZString LTC_Status { get; set; }
		ZString LTC_ConsignmentType { get; set; }
		ZBool LTC_IsActive { get; set; }
	}
}
