using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Messaging.Integration
{
	public interface IEDIMessage : IBusiness, ISourceInfo
	{
		ZGuid PK { get; }
		ZString EM_MessageText { get; set; }
		ZString EM_MessageType { get; set; }
		ZString EM_MessageSubType { get; set; }
		ZString MessageSubTypeWithDescription { get; }
		ZString EM_ReceiveTransmit { get; set; }
		ZString EM_MessageNum { get; set; }
		ZString EM_Status { get; set; }
		ZByte EM_RetryCount { get; set; }
		ZString EM_LinkTable { get; set; }
		ZGuid EM_LinkUniqueID { get; set; }
		ZGuid EM_EI { get; set; }
		ZGuid EM_GB { get; set; } // branch
		ZGuid EM_GE { get; set; } // department
		ZBool EM_IsTestMessage { get; set; }
		ZBool EM_IsActive { get; set; }
		ZString EM_ApplicationCode { get; set; }
		ZString EM_MessageTextDetail { get; }
		ZDateTime EM_HeldUntilDate { get; set; }
		ZDateTime EM_SystemCreateTimeUtc { get; set; }
		ZDateTime EM_SystemLastEditTimeUtc { get; set; }
		ZString EM_SystemCreateUser { get; set; }
		ZString EM_SystemLastEditUser { get; set; }
		ZGuid EM_ECC_CommunicationPartyConfig { get; set; }
		ZGuid EM_EM_RequestMessage { get; set; }
		ZString EM_ExternalReferenceNumber { get; set; }
		ZString EM_TransportType { get; set; }

		IEDIInterchange Interchange { get; }
		IBranch Branch { get; }

		void SetEM_MessageTextOrDataSource(Stream source);
		TextReader GetEM_MessageTextReader();

		void AssignMessageNumber();
	}
}
