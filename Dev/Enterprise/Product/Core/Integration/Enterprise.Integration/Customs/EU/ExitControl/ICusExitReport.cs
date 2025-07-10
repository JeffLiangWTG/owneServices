using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class EUExitControl
		{
			public interface ICusExitReport : IBusiness
			{
				ZGuid PK { get; }
				object this[string propertyName] { get; set; }

				ZString CER_AdditionalDeclarationType { get; set; }
				ZString CER_Behavior { get; set; }
				ZGuid CER_CER_ExitReport { get; set; }
				ZInt CER_ClusterKey { get; set; }
				ZGuid CER_CXC_Consignment { get; set; }
				ZGuid CER_CXH_Header { get; set; }
				ZDateTimeOffset CER_DateTime { get; set; }
				ZString CER_DeclarantType { get; set; }
				ZString CER_EnquiryInformationCode { get; set; }
				ZBool CER_IsFinalized { get; set; }
				ZString CER_Location { get; set; }
				ZString CER_MessageStatus { get; set; }
				ZString CER_OfficeOfExit { get; set; }
				ZString CER_OfficeOfExport { get; set; }
				ZString CER_RN_NKTransportNationality { get; set; }
				ZString CER_Status { get; set; }
				ZDateTime CER_SystemCreateTimeUtc { get; set; }
				ZString CER_SystemCreateUser { get; set; }
				ZDateTime CER_SystemLastEditTimeUtc { get; set; }
				ZString CER_SystemLastEditUser { get; set; }
				ZString CER_TransportID { get; set; }
				ZString CER_TransportMode { get; set; }
				ZString CER_TransportType { get; set; }
				ZString CER_Type { get; set; }
				ICusExitConsignment Consignment { get; }
				IActiveBusinessObjectCollection<ICusExitReportItem> CusExitReportItems { get; }
			}
		}
	}
}
