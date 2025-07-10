using System.Threading;
using Enterprise.Customs.IE.ServiceTasks;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using EDIInterchange = Enterprise.Messaging.Business.EDIInterchange;

[assembly: HostedService(
	code: Enterprise.Customs.IE.EMCS.ServiceTasks.TransactionIDManagerServiceTask.ServiceTaskApplicationCode,
	description: Enterprise.Customs.IE.EMCS.ServiceTasks.TransactionIDManagerServiceTask.ServiceTaskApplicationDescription,
	category: Enterprise.Customs.IE.ServiceTasks.Constants.ServiceTaskCategory,
	type: typeof(Enterprise.Customs.IE.EMCS.ServiceTasks.TransactionIDManagerServiceTask),
	MinimumPeriod = "60Seconds",
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Ireland,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: Enterprise.Customs.IE.EMCS.ServiceTasks.TransactionIDManagerServiceTask.ServiceTaskApplicationCode,
	table: EDIMessageSchema.Constants.TableName,
	predicates: new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIInterchange.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIInterchange.Direction.Transmit,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationReference + "=",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.IECustomsEMCS,
	},
	queueName: "IE EMCS Customs TransactionID"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: Enterprise.Customs.IE.EMCS.ServiceTasks.TransactionIDManagerServiceTask.ServiceTaskApplicationCode,
	table: EDIInterchangeSchema.Constants.TableName,
	predicates: new[] {
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.IECustomsEMCS,
		EDIInterchangeSchema.Constants.EI_TransportType + "=" + EDIInterchangeTransportTypeList.Codes.xT,
		EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + Enterprise.Customs.IE.Messaging.CommonInterchangeTypeList.Codes.TransactionID
	},
	queueName: "IE EMCS Customs TransactionID Request"
)]
namespace Enterprise.Customs.IE.EMCS.ServiceTasks
{
	public class TransactionIDManagerServiceTask : NonBranchSpecificServiceTask
	{
		public const string ServiceTaskApplicationCode = "IEE";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Task description")]
		public const string ServiceTaskApplicationDescription = "IE EMCS Customs Transaction ID Manager";

		[HostedServiceRequirement]
		public static string IsRequired() => ServiceTaskHelper.GetEMCSCertificateMessageError();

		protected override void RunTaskCore(CancellationToken youMustReactToThisToken)
		{
			Business.TransactionIDManager.New(ServiceLogger, true).Process(youMustReactToThisToken);
		}
	}
}
