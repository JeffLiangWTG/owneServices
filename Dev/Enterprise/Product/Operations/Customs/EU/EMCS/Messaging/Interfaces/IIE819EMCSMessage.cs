using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Messaging
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This interface will be used in the future.")]
	public interface IIE819EMCSMessage : IEMCSMessage
	{
		IIE819Attributes Attributes { get; set; }

		IConsigneeTrader ConsigneeTrader { get; set; }

		IExciseMovementEad ExciseMovementEad { get; set; }

		IOffice DestinationOffice { get; set; }

		IAlertOrRejection AlertOrRejection { get; set; }

		IAlertOrRejectionOfEadReason AlertOrRejectionOfEadReason { get; set; }
	}

	public interface IIE819Attributes
	{
		ZDateTime DateAndTimeOfValidationOfAlertRejection { get; set; }
	}

	public interface IAlertOrRejection
	{
		ZDate DateOfAlertOrRejection { get; set; }

		ZString EadRejectedFlag { get; set; }
	}

	public interface IAlertOrRejectionOfEadReason
	{
		ZString AlertOrRejectionOfEadReasonCode { get; set; }

		ZString ComplementaryInformation { get; set; }
	}
}
