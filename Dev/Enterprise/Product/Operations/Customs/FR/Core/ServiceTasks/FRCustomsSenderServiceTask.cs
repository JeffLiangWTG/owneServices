using System.Threading;
using CargoWise.ComponentModel;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.Customs.FR.NCTS.Messaging;
using Enterprise.Customs.FR.Registry;
using Enterprise.Customs.FR.ServiceTasks;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(FRCustomsSenderServiceTask.Code,
	FRCustomsSenderServiceTask.Description,
	"FRC",
	typeof(FRCustomsSenderServiceTask),
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

[assembly: HostedServiceBusinessObjectBinding(FRCustomsSenderServiceTask.Code,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.FRCustomsMessage,
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	},
	"FR Customs messages outbound")]

[assembly: HostedServiceBusinessObjectBinding(FRCustomsSenderServiceTask.Code,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.FRPortMessage,
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	},
	"FR Port messages outbound")]

namespace Enterprise.Customs.FR.ServiceTasks
{
	public class FRCustomsSenderServiceTask : CustomsServiceTask
	{
		public const string Code = "FRI";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public const string Description = "FR Customs Interchange Creator";

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
						try
						{
							DeltaGOutgoingMessageProcessor.ProcessMessage(token);

							DeltaIEOutgoingMessageProcessor.ProcessMessage(token);

							DeltaTOutgoingMessageProcessor.ProcessMessage(token);

							CINOutgoingMessageProcessor.ProcessMessage(token);

							ECSOutgoingMessageProcessor.ProcessMessage(token);

							PortsOutgoingMessageProcessor.ProcessMessage(token);

							PNTSOutgoingMessageProcessor.ProcessMessage(token);

							TP5OutgoingMessageProcessor.ProcessMessage(token);
						}
						catch (OnSavingCriticalCheckException ex)
						{
							ServiceLogger.GetTaskNotificationSubscriber().AddError(ex.Message);
						}
					});
				}
			}
		}

		DeltaTOutgoingMessageProcessor DeltaTOutgoingMessageProcessor => deltaTOutgoingMessageProcessor ??= new DeltaTOutgoingMessageProcessor(Logger);
		DeltaTOutgoingMessageProcessor deltaTOutgoingMessageProcessor;

		DeltaGOutgoingMessageProcessor DeltaGOutgoingMessageProcessor => deltaGOutgoingMessageProcessor ??= new DeltaGOutgoingMessageProcessor(Logger);
		DeltaGOutgoingMessageProcessor deltaGOutgoingMessageProcessor;

		DeltaIEOutgoingMessageProcessor DeltaIEOutgoingMessageProcessor => deltaIEOutgoingMessageProcessor ??= new DeltaIEOutgoingMessageProcessor(Logger);
		DeltaIEOutgoingMessageProcessor deltaIEOutgoingMessageProcessor;

		FRCINOutgoingMessageProcessor CINOutgoingMessageProcessor => fCINOutgoingMessageProcessor ??= new FRCINOutgoingMessageProcessor(Logger);
		FRCINOutgoingMessageProcessor fCINOutgoingMessageProcessor;

		FRECSOutgoingMessageProcessor ECSOutgoingMessageProcessor => fECSOutgoingMessageProcessor ??= new FRECSOutgoingMessageProcessor(Logger);
		FRECSOutgoingMessageProcessor fECSOutgoingMessageProcessor;

		FRPortsOutgoingMessageProcessor PortsOutgoingMessageProcessor => portsOutgoingMessageProcessor ??= new FRPortsOutgoingMessageProcessor(Logger);
		FRPortsOutgoingMessageProcessor portsOutgoingMessageProcessor;

		FRPNTSOutgoingMessageProcessor PNTSOutgoingMessageProcessor => fPNTSOutgoingMessageProcessor ??= new FRPNTSOutgoingMessageProcessor(Logger);
		FRPNTSOutgoingMessageProcessor fPNTSOutgoingMessageProcessor;

		TP5OutgoingMessageProcessor TP5OutgoingMessageProcessor => tP5OutgoingMessageProcessor ??= new TP5OutgoingMessageProcessor(Logger);
		TP5OutgoingMessageProcessor tP5OutgoingMessageProcessor;
	}
}
