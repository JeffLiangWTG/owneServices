using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using TransportTypeGenericList = Enterprise.Customs.Business.CustomsLists.TransportTypeGenericList;

namespace Enterprise.Customs.DE.Business
{
	public sealed class ImportLineCustomsValueProvider : IImportLineCustomsValue
	{
		public ImportLineCustomsValueProvider(JobDeclaration declaration, IEnumerable<JobComInvoiceLine> invoiceLines)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
			this.invoiceLines = Argument.NotNull(invoiceLines, nameof(invoiceLines)).OrderBy(x => x.JI_SystemCreateTimeUtc).ThenBy(x => x.PK);
			Argument.NotNull(invoiceLines.FirstOrDefault(), nameof(invoiceLines), $"{nameof(invoiceLines)} should have at least one line");
			randomInvoiceLine = invoiceLines.First();
			invoiceHeader = randomInvoiceLine.InvoiceHeader;
		}
		readonly JobDeclaration declaration;
		readonly IEnumerable<JobComInvoiceLine> invoiceLines;
		readonly JobComInvoiceLine randomInvoiceLine;
		readonly JobComInvoiceHeader invoiceHeader;

		public string CustomsValueDepartureAirport => declaration.JE_TransportMode == TransportTypeGenericList.Codes.Air ? declaration.JE_IATALoadPort.ToString() : null;

		public string CustomsValueDestinationPlace => CachedValueHelper.GetValue(ref customsValueDestinationPlace, () =>
		{
			string result = null;
			var destinationCode = declaration.JE_RL_NKPortOfFirstArrival;
			if (!destinationCode.IsEmpty)
			{
				var destination = new RefUNLOCO.Loader(declaration.Factory).Load(destinationCode);
				if (destination != null)
				{
					result = destination.RL_PortName;
				}
			}

			return result;
		});
		CachedValue<string> customsValueDestinationPlace;

		public string CustomsValueAdditionDeductionDescription => CachedValueHelper.GetValue(ref customsValueAdditionDeductionDescription,
			() => invoiceLines.Select(l => l.GetCharge(ImportChargeCodeList.Codes._016)).FirstOrDefault(c => c != null)?.J7_ChargeDescription);
		CachedValue<string> customsValueAdditionDeductionDescription;

		public IImportCosts CustomsValueNetPrice => CachedValueHelper.GetValue(ref customsValueNetPrice,
			() => new ImportCostsProvider(invoiceLines.Sum(l => l.JI_NetPrice), randomInvoiceLine.JI_RX_NKNetPriceCurr, invoiceHeader.JZ_InvoiceCurrExRate, randomInvoiceLine.IsInvoiceCurrExRateUserEnterable));
		CachedValue<IImportCosts> customsValueNetPrice;

		public IImportCosts CustomsValueIndirectPayment => CachedValueHelper.GetValue(ref customsValueIndirectPayment, () =>
		{
			IImportCosts result = null;
			var charges = invoiceLines.Select(l => l.GetCharge(ImportChargeCodeList.Codes.INP)).Where(c => c != null);
			if (charges.Any())
			{
				var charge = charges.First();
				result = new ImportCostsProvider(charges.Sum(c => c.J7_Amount), charge.J7_RX_NKCurrency, charge.J7_ExchangeRate, charge.IsJ7_ExchangeRateUserEnterable);
			}
			return result;
		});
		CachedValue<IImportCosts> customsValueIndirectPayment;

		public IAirFreightCosts CustomsValueAirFreightCosts => CachedValueHelper.GetValue(ref customsValueAirFreightCosts, () =>
		{
			IAirFreightCosts result = null;
			if (declaration.JE_TransportMode == TransportTypeGenericList.Codes.Air)
			{
				var chargeAmount = decimal.Zero;
				var currency = string.Empty;
				var exchangeRate = decimal.Zero;
				var isExchangeRateIATA = false;
				var exchangeRateDate = ZDate.Empty;
				var isExchangeRateUserEnterable = false;

				var charges = invoiceLines.SelectMany(l => l.GetCustomsValueAirFreightCostsChargesForImport());
				var apportionedCharges = invoiceLines.SelectMany(l => l.GetCustomsValueAirFreightCostsApportionedChargesForImport());

				if (charges.Any())
				{
					var charge = charges.First();
					chargeAmount = charges.Sum(x => x.J7_Amount);
					currency = charge.J7_RX_NKCurrency;
					exchangeRate = charge.J7_ExchangeRate;
					isExchangeRateIATA = charge.IsJ7_ExchangeRateIATA;
					exchangeRateDate = charge.J7_ExchangeRateDate;
					isExchangeRateUserEnterable = charge.IsJ7_ExchangeRateUserEnterable;
				}
				if (apportionedCharges.Any())
				{
					var apportionedCharge = apportionedCharges.First();
					chargeAmount += apportionedCharges.Sum(x => x.J7_Amount);
					currency = apportionedCharge.J7_RX_NKCurrency;
					exchangeRate = apportionedCharge.J7_ExchangeRate;
					isExchangeRateIATA = apportionedCharge.IsJ7_ExchangeRateIATA;
					exchangeRateDate = apportionedCharge.J7_ExchangeRateDate;
					isExchangeRateUserEnterable = apportionedCharge.IsJ7_ExchangeRateUserEnterable;
				}
				if (charges.Any() || apportionedCharges.Any())
				{
					result = new AirFreightCostsProvider(chargeAmount, currency, exchangeRate, isExchangeRateIATA, exchangeRateDate, isExchangeRateUserEnterable);
				}
			}
			return result;
		});
		CachedValue<IAirFreightCosts> customsValueAirFreightCosts;

		public IReadOnlyCollection<IAdditionDeduction> CustomsValueAdditionDeduction => customsValueAdditionDeduction ?? (customsValueAdditionDeduction = GetCustomsValueAdditionDeduction());
		IReadOnlyCollection<IAdditionDeduction> GetCustomsValueAdditionDeduction()
		{
			return invoiceLines.SelectMany(l => l.GetCustomsValueAdditionDeductionChargesForImport())
				.Select(c => new
				{
					c.J7_ChargeType,
					c.J7_Amount,
					c.J7_RX_NKCurrency,
					c.IsJ7_ExchangeRateIATA,
					c.J7_ExchangeRate,
					c.J7_ExchangeRateDate,
					c.IsJ7_ExchangeRateUserEnterable,
					c.J7_Percentage,
				})
				.Concat(
					invoiceLines.SelectMany(l => l.GetCustomsValueAdditionDeductionApportionedChargesForImport())
					.Select(c => new
					{
						c.J7_ChargeType,
						c.J7_Amount,
						c.J7_RX_NKCurrency,
						c.IsJ7_ExchangeRateIATA,
						c.J7_ExchangeRate,
						c.J7_ExchangeRateDate,
						c.IsJ7_ExchangeRateUserEnterable,
						c.J7_Percentage,
					})
				)
				.GroupBy(c => c.J7_ChargeType)
				.Select(g => new
				{
					ChargeType = g.Key,
					Amount = g.Sum(c => c.J7_Amount),
					Charge = g.First()
				})
				.Select(x => new AdditionDeductionProvider(chargeType: x.ChargeType, amount: x.Amount,
							currency: x.Charge.J7_RX_NKCurrency, currencyRateIATA: x.Charge.IsJ7_ExchangeRateIATA, exchangeRate: x.Charge.J7_ExchangeRate,
							exchangeRateDate: x.Charge.J7_ExchangeRateDate, exchangeRateUserEnterable: x.Charge.IsJ7_ExchangeRateUserEnterable, percentage: x.Charge.J7_Percentage)
				).ToArray();
		}
		IReadOnlyCollection<IAdditionDeduction> customsValueAdditionDeduction;
	}
}
