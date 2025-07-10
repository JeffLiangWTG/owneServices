using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import5SMLine : IImport5SMLine
	{
		public ZInt EntryLineNo { get; set; }

		public ZString HSCode { get; set; }

		public ZString HSDescription { get; set; }

		public ZString InvoiceDescription { get; set; }

		public ZString BrandName { get; set; }

		public ZString ItemDescription { get; set; }

		public ZString Ingredient { get; set; }

		ZInt IImport5SMLine.EntryLineNo => EntryLineNo;

		ZString IImport5SMLine.HSCode => HSCode;

		ZString IImport5SMLine.HSDescription => HSDescription;

		ZString IImport5SMLine.InvoiceDescription => InvoiceDescription;

		ZString IImport5SMLine.BrandName => BrandName;

		ZString IImport5SMLine.ItemDescription => ItemDescription;

		ZString IImport5SMLine.Ingredient => Ingredient;
	}
}
