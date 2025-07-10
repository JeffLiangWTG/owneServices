using CargoWise.Types;

namespace Enterprise.Integration.Freight
{
	public interface IContainerPenalty
	{
		ZString RefContainerCode { get; }

		ZString CPY_ProcessType { get; set; }
		ZString CPY_PenaltyType { get; set; }
		ZString CPY_CreditorType { get; set; }
		ZDateTime CPY_FreeTime { get; set; }
		ZString CPY_TimeUnit { get; set; }
		ZDecimal CPY_PerUnitCost { get; set; }
		ZString CPY_RX_NKCurrency { get; set; }
	}
}
