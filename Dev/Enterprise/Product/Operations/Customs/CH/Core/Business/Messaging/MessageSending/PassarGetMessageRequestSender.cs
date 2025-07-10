using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.CH.Business;

internal sealed class PassarGetMessageRequestSender : BaseGetMessageRequestSender
{
	public PassarGetMessageRequestSender(LoggingInformation logger) : base(logger)
	{
	}

	protected override ZString ApplicationCode => ApplicationCodes.CHCustomsPassar;

	protected override string FriendlyName => (NoResString)"Passar Get Message Request";
}
