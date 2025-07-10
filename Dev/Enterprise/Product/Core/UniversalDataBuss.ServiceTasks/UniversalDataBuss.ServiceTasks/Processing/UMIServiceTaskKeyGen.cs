using Enterprise.GraphEngine.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.ServiceTasks;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	UMIServiceTaskKeyGen.CODE,
	"Universal Shipment/Event Messaging Inbound Key Generator",
	"ESV",
	typeof(UMIServiceTaskKeyGen),
	AllowsMultipleInstances = true,
	IsMandatory = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding("UMK", EDIMessageSchema.Constants.TableName,
	new[] { EDIMessageSchema.Constants.EM_IsActive + "=Y", EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.UniversalDataMessaging, EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive, EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_MessageType + "=" + EDIMessageTypeList.Codes.XDC, EDIMessageSchema.Constants.EM_MessageSubType + "=" + EDIMessageSubTypeList.Codes.XmlUniversalShipment }, "Universal Shipment Key Generator")]
[assembly: HostedServiceBusinessObjectBinding("UMK", EDIMessageSchema.Constants.TableName,
	new[] { EDIMessageSchema.Constants.EM_IsActive + "=Y", EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.UniversalDataMessaging, EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive, EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_MessageType + "=" + EDIMessageTypeList.Codes.XDC, EDIMessageSchema.Constants.EM_MessageSubType + "=" + EDIMessageSubTypeList.Codes.XmlUniversalEvent }, "Universal Event Key Generator")]
[assembly: HostedServiceBusinessObjectBinding("UMK", EDIMessageSchema.Constants.TableName,
	new[] { EDIMessageSchema.Constants.EM_IsActive + "=Y", EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.UniversalDataMessaging, EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive, EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_MessageType + "=" + EDIMessageTypeList.Codes.XDC, EDIMessageSchema.Constants.EM_MessageSubType + "=" + EDIMessageSubTypeList.Codes.XmlUniversalTransaction }, "Universal Transaction Key Generator")]
[assembly: HostedServiceBusinessObjectBinding("UMK", EDIMessageSchema.Constants.TableName,
	new[] { EDIMessageSchema.Constants.EM_IsActive + "=Y", EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.UniversalDataMessaging, EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive, EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_MessageType + "=" + EDIMessageTypeList.Codes.XDC, EDIMessageSchema.Constants.EM_MessageSubType + "=" + EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch }, "Universal Transaction Batch Key Generator")]
namespace Enterprise.UniversalDataBuss.ServiceTasks
{
	public class UMIServiceTaskKeyGen : UMIServiceTask
	{
		public new const string CODE = "UMK";
		public override string CurrentServiceTaskCode => CODE;

		public override GrEngineServiceSetting ServiceSetting => GrEngineServiceSetting.KeyGen;
	}
}
