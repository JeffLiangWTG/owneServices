using System.Threading;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.Customs.FR.Registry;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.FR.ServiceTasks.FRCustomsRetrieverServiceTask.Code,
	Enterprise.Customs.FR.ServiceTasks.FRCustomsRetrieverServiceTask.Description,
	"FRC",
	typeof(Enterprise.Customs.FR.ServiceTasks.FRCustomsRetrieverServiceTask),
	MinimumPeriod = "60Seconds",
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.France
	+ "," + Enterprise.Core.Constants.CountryCodes.FrenchGuyana
	+ "," + Enterprise.Core.Constants.CountryCodes.Guadeloupe
	+ "," + Enterprise.Core.Constants.CountryCodes.Martinique
	+ "," + Enterprise.Core.Constants.CountryCodes.Mayotte
	+ "," + Enterprise.Core.Constants.CountryCodes.Reunion
	+ "," + Enterprise.Core.Constants.CountryCodes.SaintMartin
	+ "," + Enterprise.Core.Constants.CountryCodes.SaintBarthelemy,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.FR.ServiceTasks.FRCustomsRetrieverServiceTask.Code,
	EDIInterchangeSchema.Constants.TableName,
	new[]
	{
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchangeStatusList.Codes.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms,
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchangeTypeList.Codes.GenericMessageDelivery
	},
	"FR Customs interchanges inbound")]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.FR.ServiceTasks.FRCustomsRetrieverServiceTask.Code,
	EDIInterchangeSchema.Constants.TableName,
	new[]
	{
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchangeStatusList.Codes.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsCIN,
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchangeTypeList.Codes.GenericMessageDelivery
	},
	"FR Customs CIN interchanges inbound")]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.FR.ServiceTasks.FRCustomsRetrieverServiceTask.Code,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.FRCustomsMessage,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	},
	"FR Customs messages inbound")]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.FR.ServiceTasks.FRCustomsRetrieverServiceTask.Code,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.FRPortMessage,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	},
	"FR Port messages inbound")]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.FR.ServiceTasks.FRCustomsRetrieverServiceTask.Code,
	EDIInterchangeSchema.Constants.TableName,
	new[]
	{
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchangeStatusList.Codes.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + GenericMessageDeliveryInterchangeTypeList.Codes.FRPorts,
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchangeTypeList.Codes.GenericMessageDelivery
	},
	"FR Ports interchanges inbound")]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.FR.ServiceTasks.FRCustomsRetrieverServiceTask.Code,
	EDIInterchangeSchema.Constants.TableName,
	new[]
	{
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchangeStatusList.Codes.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsDeltaIE,
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchangeTypeList.Codes.GenericMessageDelivery
	},
	"FR Delta IE interchanges inbound")]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.FR.ServiceTasks.FRCustomsRetrieverServiceTask.Code,
	EDIInterchangeSchema.Constants.TableName,
	new[]
	{
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchangeStatusList.Codes.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsPNTS,
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchangeTypeList.Codes.GenericMessageDelivery
	},
	"FR PNTS interchanges inbound")]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.FR.ServiceTasks.FRCustomsRetrieverServiceTask.Code,
	EDIInterchangeSchema.Constants.TableName,
	new[]
	{
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchangeStatusList.Codes.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsTP5,
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchangeTypeList.Codes.GenericMessageDelivery
	},
	"FR TP5 interchanges inbound")]

namespace Enterprise.Customs.FR.ServiceTasks
{
	public class FRCustomsRetrieverServiceTask : CustomsServiceTask
	{
		public const string Code = "FRR";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public const string Description = "FR Customs Message Retriever";

		[HostedServiceRequirement]
		public static string CheckRecipientIDRegistrySetting() => HostedServiceRequirementAttribute.CheckValueIsNotNullOrEmptyString(FRCustomsDataRegistry.Instance.RecipientID);

		protected override void RunTaskCore(CancellationToken token)
		{
			foreach (var branch in ServiceTaskHelper.GetOneActiveBranchPerCompanies())
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					RunTaskHandleEmailSendFailure(() =>
					{
						using (var processor = new DeltaGIncomingMessageProcessor(Logger))
						{
							processor.ProcessInterchangesAndExecuteBatch(token);
						}

						using (var processor = new FRCINImportIncomingMessageProcessor(Logger))
						{
							processor.ProcessInterchangesAndExecuteBatch(token);
						}

						using (var processor = new FRPortsIncomingMessageProcessor(Logger))
						{
							processor.ProcessInterchangesAndExecuteBatch(token);
						}

						using (var processor = new FRPNTSIncomingMessageProcessor(Logger))
						{
							processor.ProcessInterchangesAndExecuteBatch(token);
						}

						using (var processor = new DeltaIEIncomingMessageProcessor(Logger))
						{
							processor.ProcessInterchangesAndExecuteBatch(token);
						}

						using (var processor = new TP5IncomingMessageProcessor(Logger))
						{
							processor.ProcessInterchangesAndExecuteBatch(token);
						}
					});
				}
			}
		}
	}
}
