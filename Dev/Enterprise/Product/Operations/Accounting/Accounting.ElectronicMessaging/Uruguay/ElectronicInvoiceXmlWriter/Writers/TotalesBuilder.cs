using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Uruguay
{
	public interface ITotalesBuilder
	{
		Totales BuildTotales(TransactionInfo transaction, Item_Det_Fact[] itemDetFacts);
	}

	class TotalesBuilder : ITotalesBuilder
	{
		public TotalesBuilder()
		{
			factory_InitializedOnly = new BusinessObjectFactory();
		}

		Totales ITotalesBuilder.BuildTotales(TransactionInfo transaction, Item_Det_Fact[] itemDetFacts)
		{
			var totales = new Totales();

			if (Enum.TryParse(transaction?.OSCurrency?.Code, out TipMonType tipoMonEnumValue))
			{
				totales.TpoMoneda = tipoMonEnumValue;

				if (tipoMonEnumValue != TipMonType.UYU && transaction.ExchangeRate.HasValue)
				{
					totales.TpoCambioSpecified = true;
					totales.TpoCambio = transaction.ExchangeRate.Value;
				}

				CalculateTotales(totales, itemDetFacts);
			}

			(totales.IVATasaMin, totales.IVATasaBasica) = GetRefAccTaxRate();
			totales.IVATasaMinSpecified = totales.IVATasaBasicaSpecified = true;

			if ((transaction?.TransactionType).HasValue)
			{
				var sign = (transaction.TransactionType.Value == TransactionType.CRD ? -1 : 1);

				var postingJournalCollection = transaction?.PostingJournalCollection ?? new List<PostingJournal>();

				postingJournalCollection.Where(x => x.VATTaxID != null && x.VATTaxID.TaxCode.HasValue && x.OSGSTVATAmount.HasValue).ForEach(x =>
				{
					if (x.VATTaxID.TaxCode.Value == "IVA10")
					{
						totales.MntIVATasaMin += x.OSGSTVATAmount.Value;
						totales.MntIVATasaMinSpecified = true;
					}

					if (GetTaxCodeList().Contains(x.VATTaxID.TaxCode.Value))
					{
						totales.MntIVATasaBasica += x.OSGSTVATAmount.Value;
						totales.MntIVATasaBasicaSpecified = true;
					}
				});

				if (totales.MntIVATasaMinSpecified && totales.MntIVATasaMin != 0)
				{
					totales.MntIVATasaMin = totales.MntIVATasaMin * sign;
				}

				if (totales.MntIVATasaBasicaSpecified && totales.MntIVATasaBasica != 0)
				{
					totales.MntIVATasaBasica = totales.MntIVATasaBasica * sign;
				}
			}

			totales.MntTotal = totales.MntNoGrv + totales.MntExpoyAsim + totales.MntNetoIvaTasaMin + totales.MntNetoIVATasaBasica + FormatDecimals(totales.MntIVATasaMin) + FormatDecimals(totales.MntIVATasaBasica);
			totales.MntPagar = totales.MntTotal + totales.MontoNF;

			return totales;
		}

		(ZDecimal minRate, ZDecimal basicRate) GetRefAccTaxRate()
		{
			var basicRate = ZDecimal.Zero;
			var minRate = ZDecimal.Zero;

			var query = new ZQuery(RefAccTaxRateSchema.ZAT_RN_NKCountry, CountryCodes.Uruguay)
							.AddToFilter(RefAccTaxRateSchema.ZAT_StartDate, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.Now)
							.AddToFilter(RefAccTaxRateSchema.ZAT_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Now);
			RefAccTaxRate[] rates = Factory.Load<RefAccTaxRate>(query);

			if (rates.Length > 0)
			{
				basicRate = rates.Where(x => x.ZAT_ReferenceRateType == "STD").Select(x => x.ZAT_Rate).FirstOrDefault();
				minRate = rates.Where(x => x.ZAT_ReferenceRateType == "MID").Select(x => x.ZAT_Rate).FirstOrDefault();
			}

			return (minRate, basicRate);
		}

		static HashSet<string> GetTaxCodeList()
		{
			return new HashSet<string> { UruguayConstants.IVA, UruguayConstants.CAPIVA, UruguayConstants.IVA66, UruguayConstants.IVA77, UruguayConstants.IVA396 };
		}

		void CalculateTotales(Totales totales, Item_Det_Fact[] items)
		{
			if (items != null)
			{
				var itemsInd1 = items.Where(x => x.IndFact == Item_Det_FactIndFact.Item1).ToArray();
				if (itemsInd1.Any())
				{
					totales.MntNoGrv = itemsInd1.Sum(x => FormatDecimals(x.MontoItem));
					totales.MntNoGrvSpecified = true;
				}

				var itemsInd10 = items.Where(x => x.IndFact == Item_Det_FactIndFact.Item10).ToArray();
				if (itemsInd10.Any())
				{
					totales.MntExpoyAsim = itemsInd10.Sum(x => FormatDecimals(x.MontoItem));
					totales.MntExpoyAsimSpecified = true;
				}

				var itemsInd2 = items.Where(x => x.IndFact == Item_Det_FactIndFact.Item2).ToArray();
				if (itemsInd2.Any())
				{
					totales.MntNetoIvaTasaMin = itemsInd2.Sum(x => FormatDecimals(x.MontoItem));
					totales.MntNetoIvaTasaMinSpecified = true;
				}

				var itemsInd3 = items.Where(x => x.IndFact == Item_Det_FactIndFact.Item3).ToArray();
				if (itemsInd3.Any())
				{
					totales.MntNetoIVATasaBasica = itemsInd3.Sum(x => FormatDecimals(x.MontoItem));
					totales.MntNetoIVATasaBasicaSpecified = true;
				}

				var itemsInd6 = items.Where(x => x.IndFact == Item_Det_FactIndFact.Item6).ToArray();
				var itemsInd7 = items.Where(x => x.IndFact == Item_Det_FactIndFact.Item7).ToArray();
				if (itemsInd6.Any() || itemsInd7.Any())
				{
					totales.MontoNF = FormatDecimals(itemsInd6.Sum(x => x.MontoItem)) - FormatDecimals(itemsInd7.Sum(x => x.MontoItem));
					totales.MontoNFSpecified = true;
				}

				totales.CantLinDet = items.Length.ToString("D3");
			}
		}

		decimal FormatDecimals(decimal dm)
		{
			return Utilities.Round(dm, 2);
		}

		BusinessObjectFactory Factory => factory_InitializedOnly;
		BusinessObjectFactory factory_InitializedOnly;

#if DEBUG
		public void SubstituteFactory_ForTestOnly(BusinessObjectFactory replacement) => factory_InitializedOnly = replacement;
#endif
	}
}
