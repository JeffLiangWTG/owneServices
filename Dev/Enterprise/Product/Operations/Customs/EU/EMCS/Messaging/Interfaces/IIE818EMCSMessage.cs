using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Messaging
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This interface will be used in the future.")]
	public interface IIE818EMCSMessage : IEMCSMessage
	{
		IIE818Attributes Attributes { get; set; }

		IConsigneeTrader ConsigneeTrader { get; set; }

		IExciseMovementEad ExciseMovementEad { get; set; }

		IDeliveryPlaceTrader DeliveryPlaceTrader { get; set; }

		IOffice DestinationOffice { get; set; }

		IReportOfReceiptExport ReportOfReceiptExport { get; set; }

		IBodyReportOfReceiptExport BodyReportOfReceiptExport { get; set; }
	}

	public interface IIE818Attributes
	{
		ZString DateAndTimeOfValidationOfReportOfReceiptExport { get; set; }
	}

	public interface IReportOfReceiptExport
	{
		ZDate DateOfArrivalOfExciseProducts { get; set; }

		ZString GlobalConclusionOfReceipt { get; set; }

		ZString ComplementaryInformation { get; set; }
	}

	public interface IBodyReportOfReceiptExport
	{
		ZString BodyRecordUniqueReference { get; set; }

		ZString IndicatorOfShortageOrExcess { get; set; }

		ZString ObservedShortageOrExcess { get; set; }

		ZString ExciseProductCode { get; set; }

		ZString RefusedQuantity { get; set; }

		IUnsatisfactoryReason UnsatisfactoryReason { get; set; }
	}

	public interface IUnsatisfactoryReason
	{
		ZString UnsatisfactoryReasonCode { get; set; }

		ZString ComplementaryInformation { get; set; }
	}
}
