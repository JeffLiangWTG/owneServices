using System.Collections.Generic;
using Enterprise.GraphEngine.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.UniversalDataBuss.ServiceTasks.UMIServiceTask.CODE,
	"Universal Shipment/Event Messaging Inbound",
	"ESV",
	typeof(Enterprise.UniversalDataBuss.ServiceTasks.UMIServiceTask),
	IsMandatory = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding("UMI", EDIMessageSchema.Constants.TableName,
	new[] { EDIMessageSchema.Constants.EM_IsActive + "=Y", EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.UniversalDataMessaging, EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive, EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_MessageType + "=" + EDIMessageTypeList.Codes.XDC, EDIMessageSchema.Constants.EM_MessageSubType + "=" + EDIMessageSubTypeList.Codes.XmlUniversalShipment }, "XML Universal Shipment")]
[assembly: HostedServiceBusinessObjectBinding("UMI", EDIMessageSchema.Constants.TableName,
	new[] { EDIMessageSchema.Constants.EM_IsActive + "=Y", EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.UniversalDataMessaging, EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive, EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_MessageType + "=" + EDIMessageTypeList.Codes.XDC, EDIMessageSchema.Constants.EM_MessageSubType + "=" + EDIMessageSubTypeList.Codes.XmlUniversalEvent }, "XML Universal Event")]
[assembly: HostedServiceBusinessObjectBinding("UMI", EDIMessageSchema.Constants.TableName,
	new[] { EDIMessageSchema.Constants.EM_IsActive + "=Y", EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.UniversalDataMessaging, EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive, EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_MessageType + "=" + EDIMessageTypeList.Codes.XDC, EDIMessageSchema.Constants.EM_MessageSubType + "=" + EDIMessageSubTypeList.Codes.XmlUniversalTransaction }, "XML Universal Transaction")]
[assembly: HostedServiceBusinessObjectBinding("UMI", EDIMessageSchema.Constants.TableName,
	new[] { EDIMessageSchema.Constants.EM_IsActive + "=Y", EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.UniversalDataMessaging, EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_MessageType + "=" + EDIMessageTypeList.Codes.XDC, EDIMessageSchema.Constants.EM_MessageSubType + "=" + EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch }, "XML Universal Transaction Batch")]
namespace Enterprise.UniversalDataBuss.ServiceTasks
{
	public class UMIServiceTask : ServiceTask
	{
		public const string CODE = Scheduler.GraphEngine.StmQueueStateFactory.ServiceTaskCode;
		public override string MasterServiceTaskCode => CODE;
		public override string CurrentServiceTaskCode => CODE;
		public override GrEngineServiceSetting ServiceSetting => GrEngineServiceSetting.Flipper;

		public override IEnumerable<string> SupportedMessageSubtypes
		{
			get
			{
				yield return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
				yield return EDIMessageSubTypeList.Codes.XmlUniversalEvent;
				yield return EDIMessageSubTypeList.Codes.XmlUniversalTransaction;
				yield return EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch;
				yield return EDIMessageSubTypeList.Codes.XmlUniversalActivity;
				yield return EDIMessageSubTypeList.Codes.XmlUniversalActivityRequest;
				yield return EDIMessageSubTypeList.Codes.XmlUniversalDocumentRequest;
				yield return EDIMessageSubTypeList.Codes.XmlUniversalInterchangeRequeueRequest;
				yield return EDIMessageSubTypeList.Codes.XmlUniversalShipmentRequest;
				yield return EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatchRequest;

				if (IsGrEngineEnabled)
				{
					foreach (var subType in new USIServiceTask().SupportedMessageSubtypes)
					{
						yield return subType;
					}
				}
			}
		}

		public override IEnumerable<string> ExcludedMessageSubtypes
		{
			get
			{
				if (!IsGrEngineEnabled)
				{
					foreach (var subType in new USIServiceTask().SupportedMessageSubtypes)
					{
						yield return subType;
					}
				}
			}
		}
	}
}
