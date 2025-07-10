using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Customs.Base
{
	public abstract class DocBaseCusEntryLine : DocBaseWrapper
	{
		protected DocBaseCusEntryLine(ICusEntryLine cusEntryLine, BusinessObjectFactory factoryToWrap)
			: base(cusEntryLine, factoryToWrap)
		{
		}

		public override string ToString()
		{
			return LineNumber.ToString();
		}

		#region Abstract

		protected abstract DocBaseJobComInvoiceLine CreateJobComInvoiceLine(BaseJobComInvoiceLine invoiceLineToWrap);

		#endregion

		#region ZDecimal Fields

		public ZDecimal LinePriceInLocalCurrency
		{
			get { return CusEntryLine.TotalLinePriceInLocalCurrency; }
		}

		public ZDecimal CustomsValue
		{
			get { return CusEntryLine.CL_CustomsValue; }
		}

		public virtual ZDecimal CustomsQuantity
		{
			get { return CusEntryLine.CustomsQuantity; }
		}

		public ZDecimal CustomsValueInLocalCurrency
		{
			get { return ReturnMoneyAmountButZeroIfNull(CusEntryLine.CustomsValue); }
		}

		public ZDecimal DutyAmount
		{
			get { return CusEntryLine.DutyAmount; }
		}

		public ZDecimal DutyAmountRounded
		{
			get { return (CusEntryLine.CustomsValue.Currency != null) ? ZArchitecture.Core.Utilities.Round(CusEntryLine.DutyAmount, CusEntryLine.CustomsValue.Currency.Decimals) : 2; }
		}

		public ZDecimal DutyPercent
		{
			get { return CusEntryLine.CL_DutyPercent; }
		}

		public ZDecimal GSTVATAmount
		{
			get { return CusEntryLine.GSTVATAmount; }
		}

		public ZDecimal GSTVATAmountRounded
		{
			get { return ZArchitecture.Core.Utilities.Round(CusEntryLine.GSTVATAmount, CusEntryLine.CustomsValue.Currency.Decimals); }
		}

		public ZDecimal GSTVATDeferred
		{
			get { return CusEntryLine.GSTVATDeferred; }
		}

		public ZDecimal WarehouseUnitValue
		{
			get { return CusEntryLine.CL_WarehouseUnitValue; }
		}

		#endregion

		#region ZShort Fields

		public ZShort LineNumber
		{
			get { return CusEntryLine.CL_LineNumber; }
		}

		#endregion

		#region ZString Fields

		public ZString LinePricesWithCurrency
		{
			get
			{
				if (CusEntryLine.TotalLinePrice != null)
				{
					return CusEntryLine.TotalLinePrice.ToString();
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZString ParentTrailer
		{
			get { return CusEntryLine.CL_ParentTrailer; }
		}

		public ZString CountryOfOriginCode
		{
			get { return CusEntryLine.RandomLine.JI_CountryOfOrigin; }
		}

		public ZString FormattedTariff
		{
			get { return CusEntryLine.FormattedTariff; }
		}

		public ZString Tariff
		{
			get { return CusEntryLine.Tariff; }
		}

		public ZString Description
		{
			get { return CusEntryLine.Description; }
		}

		public ZString TariffAndDescription
		{
			get { return string.Format("{0} {1}", FormattedTariff, Description); }
		}

		public ZString CustomsUnitQty
		{
			get { return CusEntryLine.CustomsUnitQty; }
		}

		public ZString DutyRateDescription
		{
			get { return CusEntryLine.DutyRateDescription; }
		}

		#endregion

		#region Wrapper Fields

		public DocCurrency CustomsValueCurrency
		{
			get
			{
				DocCurrency result = null;

				if (CusEntryLine.CustomsValue != null)
				{
					result = DocCurrency.New(Factory, CusEntryLine.CustomsValue.Currency);
				}

				return result;
			}
		}

		#endregion

		#region Implementation

		ICusEntryLine CusEntryLine
		{
			get { return (ICusEntryLine)WrappedObject; }
		}

		protected DocBaseJobComInvoiceLine InvoiceLineInternal
		{
			get
			{
				if (CusEntryLine.InvoiceLines.Count > 0)
				{
					return CreateJobComInvoiceLine(CusEntryLine.RandomLine);
				}
				else
				{
					return null;
				}
			}
		}

		protected ZDecimal ReturnMoneyAmountButZeroIfNull(Money money)
		{
			return (money != null) ? money.Amount : new ZDecimal(0);
		}
		#endregion
	}
}
