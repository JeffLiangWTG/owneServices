using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration.RollUpSort;
using Enterprise.DocumentWrappers.Accounting.DocRollUpSort.InvoiceDocLineRollUpper;
using Enterprise.DocumentWrappers.HelperClasses.DocRollUpSort;

namespace Enterprise.DocumentWrappers.Accounting.DocRollUpSort.RollUpper
{
	public abstract partial class BaseInvoiceDocRollUpper<TID, TDescription> : BaseDocRollUpper<TID, TDescription, DocARInvoiceLineCollection>
		where TID : IZType
		where TDescription : IZType
	{
		public BaseInvoiceDocRollUpper(BaseInvoiceDocLineRollUpper docLineRollUpper, DocARBaseInvoice docARInvoice, BusinessObjectFactory factory, DocARInvoiceLineCollection lines)
			: base(docARInvoice, factory, lines)
		{
			DocLineRollUpper = docLineRollUpper;
		}

		protected bool IsGroupedByChargeCode { get; set; }

		protected DocARBaseInvoice DocARBaseInvoice => (DocARBaseInvoice)DocHeader;

		protected override IRolledUpDocLine RollUpGroup(DocARInvoiceLineCollection group, TID groupId)
			=> RollUpGroupCore(group, groupId);

		protected virtual bool IsRollUpGrouperByChargeAndTax => false;

		protected IRolledUpDocLine RollUpGroupCore(DocARInvoiceLineCollection group, TID groupId)
		{
			var lineGroup = group;

			DocARInvoiceLineForRollUp result = null;

			if (group.Count > 0)
			{
				var exchangeRates = new DocARInvoiceLine.AmountWithExchangeRate();

				if (lineGroup.OnlyOneCurrency)
				{
					var currencyExRateAmount = ((DocARInvoiceLine)lineGroup[0]).CurrencyExRateAmount;
					exchangeRates.Currency = currencyExRateAmount.Currency;
					exchangeRates.ExchangeRate = lineGroup.OneCurrencyExchangeRate;
					exchangeRates.Amount = lineGroup.ConvertedTotalOSAmount;
				}

				DocChargeCode chargeCode = null;

				if (IsRollUpGrouperByChargeAndTax)
				{
					chargeCode = lineGroup[0].ChargeCode;
				}

				var desription1 = GetDescriptionForRolledUpLine(lineGroup, groupId);

				result = DocLineRollUpper.CreateWrapperForRolledUpLine
				(
					desription1,
					lineGroup.TotalOSExTaxamount,
					lineGroup.TotalOSTaxAmount,
					lineGroup.TotalLineAmount,
					lineGroup.TotalGSTVAT,
					exchangeRates,
					lineGroup,
					chargeCode
				);

				if (lineGroup.OnlyOneJob)
				{
					result.JobHeader = DocJobHeader.New(lineGroup[0].JobHeader.JobHeader, Factory);
				}

				if (IsGroupedByChargeCode)
				{
					AttachChargeCodeToRolledDoc(lineGroup, result);
				}

				result.IsRollUpLine = true;
			}

			return result;
		}

		BaseInvoiceDocLineRollUpper DocLineRollUpper { get; }

		static void AttachChargeCodeToRolledDoc(DocARInvoiceLineCollection group, DocARInvoiceLineForRollUp result)
		{
			if (group.Count > 0)
			{
				result.ChargeCode = group[0].ChargeCode;
			}
		}

		public override DocARInvoiceLineCollection GetNewLines(BusinessObjectFactory factory)
			=> DocARInvoiceLineCollection.New(factory);

		protected override BaseDocRollUpGroupList<TID, DocARInvoiceLineCollection> GetDocRollUpGroupList()
			=> new InvoiceDocRollUpGroupList<TID>();
	}
}
