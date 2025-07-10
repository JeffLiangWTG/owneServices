using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class EU
		{
			public interface ICusTempStorageJobHeader : IBusiness
			{
				ZGuid PK { get; }
				object this[string propertyName] { get; set; }
				ZString SJH_TransportRegNo { get; set; }
				ZString SJH_TransportMode { get; set; }
				ZString SJH_TransportMeansDescription { get; set; }
				ZString SJH_TransportMeansCode { get; set; }
				ZString SJH_SystemLastEditUser { get; set; }
				ZDateTime SJH_SystemLastEditTimeUtc { get; set; }
				ZString SJH_SystemCreateUser { get; set; }
				ZDateTime SJH_SystemCreateTimeUtc { get; set; }
				ZString SJH_RL_NKLoading { get; set; }
				ZString SJH_ReferenceNumber { get; set; }
				ZString SJH_PreviousReferenceType { get; set; }
				ZString SJH_PreviousReferenceNumber { get; set; }
				ZDateTime SJH_PresentationDate { get; set; }
				ZGuid SJH_OH_Customer { get; set; }
				ZGuid SJH_OA_Representative { get; set; }
				ZGuid SJH_OA_Presenter { get; set; }
				ZString SJH_AdditionalInformation { get; set; }
				ZString SJH_AppCode { get; set; }
				ZDate SJH_ArrivalDate { get; set; }
				ZInt SJH_ContainerCount { get; set; }
				ZString SJH_ContainerMode { get; set; }
				ZString SJH_CustomsOffice { get; set; }
				ZString SJH_CustomsProfile { get; set; }
				ZDate SJH_DepartureDate { get; set; }
				ZGuid SJH_GB { get; set; }
				ZString SJH_GS_NKAssignedStaff { get; set; }
				ZString SJH_JobReference { get; set; }
				ZBool SJH_NCTSFlag { get; set; }
				ZString SJH_CustomsOfficeOfEntryIntoEU { get; set; }
			}
		}
	}
}
