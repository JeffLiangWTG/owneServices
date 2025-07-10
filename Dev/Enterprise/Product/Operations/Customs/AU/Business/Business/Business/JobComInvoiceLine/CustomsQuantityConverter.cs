using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CustomsQuantityConverter : BaseCustomsQuantityConverter
	{
		public CustomsQuantityConverter(JobComInvoiceLine invoiceLine, ZPropertyInfo customsQuantityInfo, ZPropertyInfo customsUnitOfQuantityInfo)
			: base(invoiceLine, customsQuantityInfo, customsUnitOfQuantityInfo)
		{
		}

		new JobComInvoiceLine InvoiceLine
		{
			get { return (JobComInvoiceLine)base.InvoiceLine; }
		}

		#region Implementation

		protected AUAddInfo AddInfo
		{
			get { return InvoiceLine.AddInfo; }
		}

		protected override void CalculateCountrySpecificQuantity()
		{
			if ((ZString)customsUnitOfQuantityInfo.Value == "LA" && AddInfo.ZA_UQ2 == "L" && Part != null)
			{
				decimal iSS = UnitConverter.ConversionFactor("L", "LA") * 100;
				if (iSS > 0)
				{
					AddInfo.ZA_ISS = iSS;
				}
			}
		}

		protected override bool IsPartSpecificConversion
		{
			get
			{
				return Part != null
					&& (ZString)customsUnitOfQuantityInfo.Value == "LA"
					&& InvoiceLine.NonDutiableComponent > 0;
			}
		}

		protected override ZDecimal CalculatePartSpecificConversionFactor()
		{
			ZDecimal result = 0m;
			ZDecimal directConversion = UnitConverter.ConversionFactor(InvoiceLine.JI_InvoiceUQ, (ZString)customsUnitOfQuantityInfo.Value);
			if (!directConversion.IsEmpty)
			{
				ZDecimal invoiceToLitres = UnitConverter.ConversionFactor(InvoiceLine.JI_InvoiceUQ, "L");
				ZDecimal litresToLitresOfAlcohol = UnitConverter.ConversionFactor("L", "LA");
				ZDecimal dutiablePercent = litresToLitresOfAlcohol - (InvoiceLine.NonDutiableComponent / 100);
				ZDecimal dutiableLitresConversionFactor = invoiceToLitres * dutiablePercent;
				result = dutiableLitresConversionFactor;
			}
			return result;
		}

		#endregion

	}
}
