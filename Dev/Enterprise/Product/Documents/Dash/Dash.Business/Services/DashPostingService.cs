using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Dash.Integration.Services;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Dash.Business.Services
{
	public class DashPostingService : IDashPostingService
	{
		public ZGuid PostUxml(ZGuid branchId, ZGuid departmentId, ZGuid documentId, string uxml, BusinessObjectFactory factory = null)
		{
			var triggerSave = factory == null;
			factory ??= new BusinessObjectFactory();

			EDIMessage message;

			if (GlbBranch.CurrentBranch == null || GlbDepartment.CurrentDepartment == null)
			{
				using (Env.SetTemporaryUserContext(new UserContext(User.ServiceUserName, branchId.ToGuid(), departmentId.ToGuid())))
				{
					message = factory.New<EDIMessage>();
				}
			}
			else
			{
				message = factory.New<EDIMessage>();
			}
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			message.EM_GB = branchId;
			message.EM_GE = departmentId;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_LinkTable = DashDocument.Schema.TableName;
			message.EM_LinkUniqueID = documentId;
			message.EM_MessageText = uxml;
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			message.EM_SystemCreateUser = DocManagerRegistry.Instance.ShipamaxServiceCode.Value;
			message.EM_SystemLastEditTimeUtc = message.EM_SystemCreateTimeUtc;
			message.EM_SystemLastEditUser = message.EM_SystemCreateUser;

			if (triggerSave)
			{
				factory.Save();
			}

			return message.PK;
		}
	}
}
