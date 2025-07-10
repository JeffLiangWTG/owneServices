using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IAirCargoReportHeader : ICargoReportHeader
	{
		ZString MasterHouseBill { get; }
		ZString HAWBNum { get; }
		ZString MAWB { get; }
		ZString MatchConsignmentReference { get; }
		ZString FlightNo { get; }
		ZDateTime ArivalDate { get; }
		bool IsMasterHouse { get; }
		bool IsDocuments { get; }
		bool IsPersonalEffects { get; }
		bool IsSelfAssessedClearance { get; }
		bool IsBureau { get; }
		int PackageCount { get; }
		ZString GoodsDescription { get; }
		ZDecimal Weight { get; }
		ZString WeightUQ { get; }
		ZDecimal GoodsValue { get; }
		ZString GoodsValueCurrency { get; }
		bool IsHVLVSpecialReporter { get; }
		bool IsRemailSpecialReporter { get; }
	}
}
