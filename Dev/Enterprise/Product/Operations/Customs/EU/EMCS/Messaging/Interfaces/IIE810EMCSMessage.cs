using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Messaging
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This interface will be used in the future.")]
	public interface IIE810EMCSMessage : IEMCSMessage
	{
		IIE810Attributes Attributes { get; set; }

		IIE810ExciseMovementEad ExciseMovementEad { get; set; }

		ICancellation Cancellation { get; set; }
	}

	public interface IIE810Attributes
	{
		ZDateTime DateAndTimeOfValidationOfCancellation { get; set; }
	}

	public interface IIE810ExciseMovementEad
	{
		ZString AdministrativeReferenceCode { get; set; }
	}

	public interface ICancellation
	{
		ZString CancellationReasonCode { get; set; }

		ZString ComplementaryInformation { get; set; }
	}
}
