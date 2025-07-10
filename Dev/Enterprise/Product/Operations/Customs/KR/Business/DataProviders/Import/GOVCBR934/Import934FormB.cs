using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import934FormB : Import934_5SMFormD, IImport934FormB
	{
		[DecimalPlaces(Constants.DecimalPlacesConstants.CustomsValue)]
		public decimal ExpectedCustomsValue { get; set; }
		public ValuationMethodData Method2_3ValuationData { get; set; }
		public Valuation4_MethodData Method4ValuationData { get; set; }
		public ValuationMethodData Method5_6ValuationData { get; set; }

		ZDecimal IImport934FormB.ExpectedCustomsValue => ExpectedCustomsValue;
		IValuationMethodData IImport934FormB.Method2_3ValuationData => Method2_3ValuationData;
		IValuation4_MethodData IImport934FormB.Method4ValuationData => Method4ValuationData;
		IValuationMethodData IImport934FormB.Method5_6ValuationData => Method5_6ValuationData;
	}
}
