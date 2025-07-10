using System.Collections.Generic;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.ServiceTasks;
using Enterprise.Customs.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly:
	HostedService(
		ServiceTaskApplicationCodeList.Codes.MessageRetriever,
		ServiceTaskApplicationCodeList.Descriptions.MessageRetriever,
		BranchMessagingService.MessageServiceTaskCategory,
		typeof(MessageRetrieverService),
		RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Switzerland,
		CanRunInAnyBranch = true,
		MinimumPeriod = "1minute",
		DefaultScheduleRunEvery = "15minutes",
		ActiveByDefault = true
	)]

[assembly:
	HostedServiceBusinessObjectBinding(
		ServiceTaskApplicationCodeList.Codes.MessageRetriever,
		EDIInterchangeSchema.Constants.TableName,
		new[] {
			EDIInterchangeSchema.Constants.EI_Status + "=" + Enterprise.Messaging.Business.EDIInterchange.Status.Queued,
			EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + Enterprise.Messaging.Business.EDIInterchange.Direction.Receive,
			EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.CHCustomsEdec,
			EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		},
		ServiceTaskApplicationCodeList.Descriptions.MessageRetriever + " " + Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.CHCustomsEdec
	)]

[assembly:
	HostedServiceBusinessObjectBinding(
		ServiceTaskApplicationCodeList.Codes.MessageRetriever,
		EDIInterchangeSchema.Constants.TableName,
		new[] {
			EDIInterchangeSchema.Constants.EI_Status + "=" + Enterprise.Messaging.Business.EDIInterchange.Status.Queued,
			EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + Enterprise.Messaging.Business.EDIInterchange.Direction.Receive,
			EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.CHCustomsPassar,
			EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		},
		ServiceTaskApplicationCodeList.Descriptions.MessageRetriever + " " + Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.CHCustomsPassar
	)]

[assembly:
	HostedServiceBusinessObjectBinding(
		ServiceTaskApplicationCodeList.Codes.MessageRetriever,
		EDIInterchangeSchema.Constants.TableName,
		new[] {
			EDIInterchangeSchema.Constants.EI_Status + "=" + Enterprise.Messaging.Business.EDIInterchange.Status.Queued,
			EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + Enterprise.Messaging.Business.EDIInterchange.Direction.Receive,
			EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.CHCustomsCharteraOutput,
			EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		},
		ServiceTaskApplicationCodeList.Descriptions.MessageRetriever + " " + Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.CHCustomsCharteraOutput
	)]

namespace Enterprise.Customs.CH.ServiceTasks;

public class MessageRetrieverService : BranchInterchangeProcessorService
{
	protected override IEnumerable<string> ApplicationCodes
	{
		get
		{
			yield return Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.CHCustomsEdec;
			yield return Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.CHCustomsPassar;
			yield return Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.CHCustomsCharteraOutput;
		}
	}

	protected override Messaging.Business.BranchInboundInterchangeProcessor GetNewBranchInboundInterchangeProcessor() => new CHCInboundInterchangeProcessor();

	[HostedServiceRequirement]
	public static string IsRequired()
	{
		bool required =
			CertificateRequirementChecker.ExistsCompanyWithCertificate(CountryCodes.Switzerland, PasswordTypesList.Codes.CHT)
			|| CertificateRequirementChecker.ExistsCompanyWithCertificate(CountryCodes.Switzerland, PasswordTypesList.Codes.CHC);
		return required
			? string.Empty
			: (NoResString)"There is no Token Credential / Certificate configured in Switzerland.";
	}
}
