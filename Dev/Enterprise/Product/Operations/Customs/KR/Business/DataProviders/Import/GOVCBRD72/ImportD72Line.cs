using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ImportD72Line : IImportD72Line
	{
		public int EntryLineNo { get; set; }
		public int DetailLineNo { get; set; }
		public string HSDescription { get; set; }
		public string ItemDescription { get; set; }
		public string QuantityUnit { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.QtyOrWeight)]
		public decimal Quantity { get; set; }
		public string AmountCurrency { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.Amount)]
		public decimal Amount { get; set; }
		public string Remark { get; set; }

		ZInt IImportD72Line.EntryLineNo => EntryLineNo;
		ZInt IImportD72Line.DetailLineNo => DetailLineNo;
		ZString IImportD72Line.HSDescription => HSDescription;
		ZString IImportD72Line.ItemDescription => ItemDescription;
		ZString IImportD72Line.QuantityUnit => QuantityUnit;
		ZDecimal IImportD72Line.Quantity => Quantity;
		ZString IImportD72Line.AmountCurrency => AmountCurrency;
		ZDecimal IImportD72Line.Amount => Amount;
		ZString IImportD72Line.Remark => Remark;
	}
}
