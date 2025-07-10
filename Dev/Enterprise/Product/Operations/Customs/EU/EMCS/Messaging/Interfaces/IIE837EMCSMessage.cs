using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Messaging
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This interface will be used in the future.")]
	public interface IIE837EMCSMessage : IEMCSMessage
	{
		IIE837Attributes Attributes { get; set; }

		IExciseMovementEad ExciseMovementEad { get; set; }
	}

	public interface IIE837Attributes
	{
		ZString SubmitterIdentification { get; set; }

		ZString SubmitterType { get; set; }

		ZString ExplanationCode { get; set; }

		ZString ComplementaryInformation { get; set; }

		ZString MessageRole { get; set; }

		ZDateTime DateAndTimeOfValidationOfExplanationOnDelay { get; set; }
	}
}
