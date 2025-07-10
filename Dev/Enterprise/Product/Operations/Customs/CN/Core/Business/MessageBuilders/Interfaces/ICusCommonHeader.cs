using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public interface ICusCommonHeader
	{
		ZDateTime DateOfValuation { get; }
		ZString CustomsOfficeCode { get; }
		ZString OfficeOfEntryOrExitCode { get; }

		ZString TradePartyUSCI { get; }
		ZString TradePartyCCD { get; }
		ZString TradePartyName { get; }

		ZString TransportModeCode { get; }
		ZString CustomsProcedureCode { get; }
		ZString LevyTypeCode { get; }
		ZString ManualNo { get; }
	}
}
