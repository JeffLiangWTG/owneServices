using System.Collections.Generic;
using Enterprise.GraphEngine.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.ServiceTasks;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	USIServiceTask.Code,
	"Universal Schedule Messaging Inbound",
	"ESV",
	typeof(USIServiceTask),
	IsMandatory = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "15minutes"
	)]
	
[assembly: HostedServiceBusinessObjectBinding("USI", EDIMessageSchema.Constants.TableName,
	new[] { EDIMessageSchema.Constants.EM_IsActive + "=Y", EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.UniversalDataMessaging, EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive, EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_MessageType + "=" + EDIMessageTypeList.Codes.XDC, EDIMessageSchema.Constants.EM_MessageSubType + "=" + EDIMessageSubTypeList.Codes.XmlUniversalSchedule }, "XML Universal Schedule")]
namespace Enterprise.UniversalDataBuss.ServiceTasks
{
	public class USIServiceTask : ServiceTask
	{
		public const string Code = "USI";
		public override string MasterServiceTaskCode => Code;
		public override string CurrentServiceTaskCode => MasterServiceTaskCode;
		public override GrEngineServiceSetting ServiceSetting => GrEngineServiceSetting.NonGrengineOnly;

		public override IEnumerable<string> SupportedMessageSubtypes
		{
			get { yield return EDIMessageSubTypeList.Codes.XmlUniversalSchedule; }
		}

		public override IEnumerable<string> ExcludedMessageSubtypes
		{
			get
			{
				foreach (var subType in new UMIServiceTask().SupportedMessageSubtypes)
				{
					yield return subType;
				}
			}
		}
	}
}
