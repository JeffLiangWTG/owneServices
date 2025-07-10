using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import008Line : IImport008Line
	{
		public string ItemCategory { get; set; }
		public string ItemCode { get; set; }
		public string InvoiceDescription { get; set; }
		public string BrandName { get; set; }
		public int MonthOfUse { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.Qty)]
		public decimal Quantity { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.QtyOrPrice)]
		public decimal Price { get; set; }
		public string Model { get; set; }

		ZString IImport008Line.ItemCategory => ItemCategory;
		ZString IImport008Line.ItemCode => ItemCode;
		ZString IImport008Line.InvoiceDescription => InvoiceDescription;
		ZString IImport008Line.BrandName => BrandName;
		ZInt IImport008Line.MonthOfUse => MonthOfUse;
		ZDecimal IImport008Line.Quantity => Quantity;
		ZDecimal IImport008Line.Price => Price;
		ZString IImport008Line.Model => Model;
	}
}
