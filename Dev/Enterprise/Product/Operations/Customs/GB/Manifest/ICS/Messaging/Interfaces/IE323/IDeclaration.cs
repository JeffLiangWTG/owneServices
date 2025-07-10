using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.GB.ICS.Messaging.IE323
{
	[CodeAlive("Will be used in subsequent WI.")]
	public interface IDeclaration
	{
		ZString MessageSender { get; }
		ZString MessageRecipient { get; }
		ZString DateOfPreparation { get; }
		ZString TimeOfPreparation { get; }
		ZString Priority { get; }
		ZBool TestIndicator { get; }
		ZString MessageIdentification { get; }
		ZString MessageType { get; }
		ZString CorrelationIdentifier { get; }

		IHeader Header { get; }

		ICustomsOffice ActualArrivalCustomsOffice { get; }

		ICustomsOffice FirstEntryCustomsOffice { get; }

		ITrader RequestingDiversionTrader { get; }

		ICusEntryNum ImportOperation { get; }
	}
}
