using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Customs.AU.ServiceTasks.InterchangeSenderService.Code,
	Enterprise.Customs.AU.ServiceTasks.InterchangeSenderService.Name,
	"AUC",
	typeof(Enterprise.Customs.AU.ServiceTasks.InterchangeSenderService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Australia,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1Minute",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.AU.ServiceTasks.InterchangeSenderService.Code,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=TRX",
		EDIMessageSchema.Constants.EM_Status          + "=QUE",
		EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=CMR",
		EDIMessageSchema.Constants.EM_MessageType     + "!=ACR",
		EDIMessageSchema.Constants.EM_MessageType     + "!=SCR"
	},
	"AU Customs CMR exclude ACR And SCR messages outbound")]
[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.AU.ServiceTasks.InterchangeSenderService.Code,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=TRX",
		EDIMessageSchema.Constants.EM_Status          + "=QUE",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=CMR",
		EDIMessageSchema.Constants.EM_MessageType     + "=ACR",
		EDIMessageSchema.Constants.EM_MessageType     + "=SCR"
	},
	"AU Customs CMR ACR And SCR messages outbound")]
[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.AU.ServiceTasks.InterchangeSenderService.Code,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=TRX",
		EDIMessageSchema.Constants.EM_Status          + "=QUE",
		EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=PRA"
	},
	"AU Customs PRA messages outbound")]
[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.AU.ServiceTasks.InterchangeSenderService.Code,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=TRX",
		EDIMessageSchema.Constants.EM_Status          + "=QUE",
		EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=EXD"
	},
	"AU Customs ExDocs messages outbound")]
[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.AU.ServiceTasks.InterchangeSenderService.Code,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=TRX",
		EDIMessageSchema.Constants.EM_Status          + "=QUE",
		EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=NEX"
	},
	"AU Customs NEXDocs messages outbound")]
[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.AU.ServiceTasks.InterchangeSenderService.Code,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=TRX",
		EDIMessageSchema.Constants.EM_Status          + "=QUE",
		EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=COL"
	},
	"AU Customs COLS messages outbound")]

namespace Enterprise.Customs.AU.ServiceTasks
{
	public class InterchangeSenderService : MultiCompanyCustomsMessagingService
	{
		protected override ZString LegacyBatchProcessorCode => ZString.Empty;

		protected override ICustomsServiceTaskProcess GetNewProcess()
		{
			var senderTypeToCreate = TypeDecider.GetTypeForBinding(typeof(AUCInterchangeSender));
			var aUCMessageSenderProcess = (AUCInterchangeSender)Activator.CreateInstance(senderTypeToCreate);
			return aUCMessageSenderProcess;
		}

		protected override string RequiredCountry => Enterprise.Core.Constants.CountryCodes.Australia;

		public const string Code = "AUS";
		public const string Name = "Australian Customs Interchange Sender";
	}
}


