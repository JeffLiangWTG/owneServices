using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IOutturnReportLineInformation
	{
		bool DamageIndicator { get; }
		bool PillageIndicator { get; }
		ZInt NumberOfPackages { get; }
		ZString GoodsDescription { get; }
		ZString OutturnResultType { get; }
		ZDateTime LastMessageDate { get; }
	}
}
