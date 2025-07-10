using Enterprise.GraphEngine.ServiceTasks;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.UniversalDataBuss.ServiceTasks.UMIServiceTaskWorker.CODE,
	"Universal Shipment/Event Messaging Inbound Worker",
	"ESV",
	typeof(Enterprise.UniversalDataBuss.ServiceTasks.UMIServiceTaskWorker),
	AllowsMultipleInstances = true,
	IsMandatory = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "1minute"
	)]
namespace Enterprise.UniversalDataBuss.ServiceTasks
{
	public class UMIServiceTaskWorker : UMIServiceTask
	{
		public new const string CODE = "UMQ";
		public override string CurrentServiceTaskCode => CODE;

		public override GrEngineServiceSetting ServiceSetting => GrEngineServiceSetting.Worker;
	}
}
