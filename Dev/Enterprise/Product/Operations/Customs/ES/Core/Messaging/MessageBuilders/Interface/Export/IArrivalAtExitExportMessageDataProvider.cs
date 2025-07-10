using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IArrivalAtExitExportMessageDataProvider : IEDIFACTMessageDataProvider
	{
		#region Fields For BGM

		ZString LocalReferenceNumber { get; }

		#endregion

		#region Fields For CST

		ZString CustomsProcedureCategory5 { get; }

		#endregion

		#region Fields For LOC

		ZString CustomsOfficeofExitCountryCode { get; }
		ZString CustomsOfficeofExit { get; }
		ZString LocationOfGoodsExamCustomsOffice { get; }
		ZString LocationOfGoodsExam { get; }

		#endregion

		#region Fields For DTM

		ZDateTime DateOfArrival { get; }

		#endregion

		#region Fields For NAD

		IExportDeclarantPartyIdProvider Declarant { get; }

		#endregion
	}
}
