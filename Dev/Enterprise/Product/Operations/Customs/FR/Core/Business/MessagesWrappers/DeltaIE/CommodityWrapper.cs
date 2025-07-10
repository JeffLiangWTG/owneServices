using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class CommodityWrapper : ICommodity
	{
		CommodityWrapper(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
		}

		public static CommodityWrapper New(CusEntryLine entryLine) => entryLine == null ? null : new CommodityWrapper(entryLine);

		public ICalculationOfTaxes CalculationOfTaxes => calculationOfTaxes ?? (calculationOfTaxes = CalculationOfTaxesWrapper.New(entryLine));
		ICalculationOfTaxes calculationOfTaxes;

		public ICommodityCode CommodityCode => commodityCode ?? (commodityCode = CommodityCodeWrapper.New(entryLine.RandomLine));
		ICommodityCode commodityCode;

		public string CusCode => cusCode ?? (cusCode = entryLine.RandomLine.ZG_CusNumber);
		string cusCode;

		public string DescriptionOfGoods => descriptionOfGoods ?? (descriptionOfGoods = entryLine.CL_Description);
		string descriptionOfGoods;

		public IGoodsMeasure GoodsMeasure => goodsMeasure ?? (goodsMeasure = GoodsMeasureWrapper.New(entryLine));
		IGoodsMeasure goodsMeasure;

		public IInvoiceLine InvoiceLine => invoiceLine ?? (invoiceLine = InvoiceLineWrapper.New(entryLine));
		IInvoiceLine invoiceLine;

		public string QuotaOrderNumber => quotaOrderNumber ?? (quotaOrderNumber = entryLine.RandomLine.JI_ConcessionOrder);
		string quotaOrderNumber;

		readonly CusEntryLine entryLine;
	}
}
