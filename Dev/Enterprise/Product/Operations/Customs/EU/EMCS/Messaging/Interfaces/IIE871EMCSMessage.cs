using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Messaging
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This interface will be used in the future.")]
	public interface IIE871EMCSMessage : IEMCSMessage
	{
		IIE871Attributes Attributes { get; set; }

		IConsigneeTrader ConsigneeTrader { get; set; }

		IExciseMovementEad ExciseMovementEad { get; set; }

		IConsignorTrader ConsignorTrader { get; set; }

		IAnalysis Analysis { get; set; }

		IBodyAnalysis BodyAnalysis { get; set; }

		IOffice DispatchImportOffice { get; set; }
	}

	public interface IIE871Attributes
	{
		ZString SubmitterType { get; set; }

		ZString DateAndTimeOfValidationOfExplanationOnShortage { get; set; }
	}

	public interface IAnalysis
	{
		ZDate DateOfAnalysis { get; set; }

		ZString GlobalExplanation { get; set; }
	}

	public interface IBodyAnalysis
	{
		ZString ExciseProductCode { get; set; }

		ZString BodyRecordUniqueReference { get; set; }

		ZString Explanation { get; set; }

		ZString ActualQuantity { get; set; }
	}
}
