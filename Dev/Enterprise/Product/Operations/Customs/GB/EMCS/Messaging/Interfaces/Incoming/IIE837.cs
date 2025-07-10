using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging
{
	public interface IIE837 : IEMCSInboundProvider
	{
		IEMCSEvent ExciseMovement { get; }

		ZString AdministrativeReferenceCode { get; }

		ZDateTime DateAndTimeOfValidationOfExplanationOnDelay { get; }

		ZString ExplanationCode { get; }

		ZString MessageRole { get; }

		ZString SubmitterIdentification { get; }

		ZString SubmitterType { get; }

		ZString ComplementaryInformation { get; }
	}
}
