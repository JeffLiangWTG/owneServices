using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.DocumentWrappers
{
	public class DocTaxEstimatorWrapper : DocSADH, Chief.ITotalDutyAndTotalVatProvider
	{
		DocTaxEstimatorWrapper(CusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap)
			: base(entryHeader, factoryToWrap)
		{ }

		public static DocTaxEstimatorWrapper New(CusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap)
		{
			return new DocTaxEstimatorWrapper(entryHeader, factoryToWrap);
		}

		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		public ZString Box62 => ConvertToGBP(Declaration.ZG_OSAirTransportAmount, Declaration.ZG_RX_NKFrtChg, 2);

		public ZString Box63 => ConvertToGBP(Declaration.ZG_FrtChgAmt, Declaration.ZG_RX_NKFrtChg, 2);

		public ZString Box64 => Declaration.ZG_ApportionByWeight ? "Yes" : "No";

		public ZString Box65 => ConvertToGBP(Declaration.ZG_DiscAmt, Declaration.ZG_RX_NKDisc, 2);

		public ZString Box66 => ConvertToGBP(Declaration.ZG_InsAmt, Declaration.ZG_RX_NKIns, 2);

		public ZString Box67 => ConvertToGBP(Declaration.ZG_OthChgAmt, Declaration.ZG_RX_NKOthChg, 2);

		public ZDecimal Box68 => ConvertToGBP(Declaration.ZG_VATAdjAmt, Declaration.ZG_RX_NKVATAdj);

		public ZString TotalDuties
		{
			get
			{
				var entryTotalDuty = ZDecimal.Zero;
				foreach (LineForTax line in LinesForTax)
				{
					entryTotalDuty += line.TotalDutyDue;
				}
				return entryTotalDuty.ToStringTrimZeros(2);
			}
		}

		public ZString TotalVat
		{
			get
			{
				var entryTotalVat = ZDecimal.Zero;
				foreach (LineForTax line in LinesForTax)
				{
					entryTotalVat += line.VatDue;
				}
				return entryTotalVat.ToStringTrimZeros(2);
			}
		}

		ZDecimal Chief.ITotalDutyAndTotalVatProvider.TotalDuty
		{
			get { return ZDecimal.ParseSafe(TotalDuties, 0m); }
		}

		ZDecimal Chief.ITotalDutyAndTotalVatProvider.TotalVAT
		{
			get { return ZDecimal.ParseSafe(TotalVat, 0m); }
		}

		public LineForTaxCollection LinesForTax
		{
			get { return linesForTax ?? (linesForTax = new LineForTaxCollection(EntryHeader.MergedLines, Factory)); }
		}
		LineForTaxCollection linesForTax;

		ZString ConvertToGBP(ZDecimal amount, ZString sourceCurrencyCode, ZInt decimalPlaces)
			=> ConvertToGBP(amount, sourceCurrencyCode).ToStringTrimZeros(decimalPlaces);

		ZDecimal ConvertToGBP(ZDecimal amount, ZString currencyCode)
		{
			var result = amount;
			if (!amount.IsEmpty && !currencyCode.IsEmpty && currencyCode != Core.Constants.CurrencyCodes.UnitedKingdom)
			{
				var sourceMoney = new Money(amount, new Currency(currencyCode));
				result = EntryHeader.CurrencyConverter.ConvertExact(sourceMoney, BritishPound).Amount;
			}
			return result;
		}

		Currency britishPound;
		Currency BritishPound => britishPound ?? (britishPound = new Currency(Core.Constants.CurrencyCodes.UnitedKingdom));
	}
}
