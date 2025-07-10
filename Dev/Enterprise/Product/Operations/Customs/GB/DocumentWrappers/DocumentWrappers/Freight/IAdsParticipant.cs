using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.DocumentWrappers.Freight
{
	public interface IAdsParticipant
	{
		ZString CtStatus { get; }
		int DeclarationsCount { get; }
		ZString DeclarationUCRs { get; }
		ZString ChiefEntryReferences { get; }
		ZDecimal TotalWeightAds { get; }
		ZString HouseBill { get; }
		ZString Origin { get; }
		ZString FinalDestination { get; }
		OrgHeader Consignor { get; }
		OrgHeader Consignee { get; }
		ZInt TotalNoOfPacks { get; }
		ZString GoodsDescription { get; }
	}

	static class Helpers
	{
		public static ZString GetEntryNumberAndDate(Common.CusEntryNumber cen)
		{
			return cen == null ? ZString.Empty
				: ZString.Format("{0}-{1:dd/MM/yyyy}", cen.CE_EntryNum, cen.CE_IssueDate);
		}

		public static ZString ToIataSafe(this RefUNLOCO loco)
		{
			return loco != null ? loco.RL_IATA : ZString.Empty;
		}

		public const string CStatusGoods = "C STATUS GOODS";
	}
}
