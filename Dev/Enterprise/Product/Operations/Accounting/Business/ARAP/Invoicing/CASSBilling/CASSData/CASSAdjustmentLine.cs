using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class CASSAdjustmentLine : CASSData
	{
		public CASSAdjustmentLine() : base(null)
		{ }

		public ZString AirlinePrefix { get; set; }
		public ZString AgentCode { get; set; }
		public ZString AWBSerialNumber { get; set; }
		public ZString CCADCMNumber { get; set; }
		public ZDecimal WeightChargePP { get; set; }
		public ZDecimal ValuationChargePP { get; set; }
		public ZDecimal ChargesDueCarrierPP { get; set; }
		public ZDecimal ChargesDueAgentCC { get; set; }
		public ZDecimal Commission { get; set; }
		public ZDecimal Incentive { get; set; }
		public ZDecimal Weight { get; set; }
		public ZString WeightUnit { get; set; }
		public ZInt ReasonCode { get; set; }
		public ZString Comment { get; set; }
	}
}
