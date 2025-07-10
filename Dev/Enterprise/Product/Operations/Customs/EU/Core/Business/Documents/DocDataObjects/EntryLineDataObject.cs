using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Customs.EU.Business.Documents.DocDataObjects
{
	public class EntryLineDataObject : DocDataObject
	{
		public EntryLineDataObject(CusEntryLine entryLine)
		{
			EntryLine = entryLine;
		}

		public ZDecimal ExchangeRate => ExchangeRateCore;

		protected virtual ZDecimal ExchangeRateCore
		{
			get
			{
				return EntryLine.RandomLine.InvoiceHeader != null
					? EntryLine.RandomLine.InvoiceHeader.JZ_InvoiceCurrExRate
					: 0.0;
			}
		}

		public ZDecimal TotalA => TotalACore;
		protected virtual ZDecimal TotalACore => decimal.Round((EntryLine.Price.Amount + IndirectAndOtherPaymentsCharges) / (ExchangeRate != 0.0 ? ExchangeRate : 1.0), 2, MidpointRounding.AwayFromZero);
		public ZDecimal TotalB => TotalBCore;
		protected virtual ZDecimal TotalBCore => decimal.Round((CommissionsCharges + BrokerageCharges + ContainersAndPackingCharges + MaterialsComponentsPartsCharges + ToolsDiesMouldsCharges + MaterialsConsumedCharges +
									EngineeringDevelopmentArtworkCharges + RoyaltiesAndLicenseCharges + ProceedsOfAnySubsequentResaleCharges + TransportCostsCharges + InsuranceCostsCharges) / (ExchangeRate != 0.0 ? ExchangeRate : 1.0), 2, MidpointRounding.AwayFromZero);
		public ZDecimal TotalC => TotalCCore;
		protected virtual ZDecimal TotalCCore => decimal.Round((ConstructionErectionAssemblyCharges + OtherNotElsewhereDeclaredCharges + ImportDutiesOrOtherCharges + CostOfTransportOTEU) / (ExchangeRate != 0.0 ? ExchangeRate : 1.0), 2, MidpointRounding.AwayFromZero);
		public ZDecimal CustomsValueDeclared => CustomsValueDeclaredCore;
		protected virtual ZDecimal CustomsValueDeclaredCore => decimal.Round(EntryLine.CustomsValue.Amount, 2, MidpointRounding.AwayFromZero);
		public ZDecimal Price => PriceCore;
		protected virtual ZDecimal PriceCore => decimal.Round(EntryLine?.Price.Amount ?? 0, 2, MidpointRounding.AwayFromZero);
		public ZDecimal RoyaltiesAndLicenseCharges => GetAdditionCharges(new ZString[] { UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge });
		public ZDecimal ContainersAndPackingCharges => GetAdditionCharges(new ZString[] { UCCCustomsChargeTypeList.Codes.ContainersAndPackingCharge });
		public ZDecimal CommissionsCharges => GetAdditionCharges(new ZString[] { UCCCustomsChargeTypeList.Codes.CommissionExceptBuyingCommissionsCharge });
		public ZDecimal BrokerageCharges => GetAdditionCharges(new ZString[] { UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge });
		public ZDecimal ProceedsOfAnySubsequentResaleCharges => GetAdditionCharges(new ZString[] { UCCCustomsChargeTypeList.Codes.ProceedsOfAnySubsequentResaleCharge });
		public ZDecimal EngineeringDevelopmentArtworkCharges => GetAdditionCharges(new ZString[] { UCCCustomsChargeTypeList.Codes.EngineeringDevelopmentArtworkCharge });
		public ZDecimal MaterialsConsumedCharges => GetAdditionCharges(new ZString[] { UCCCustomsChargeTypeList.Codes.MaterialsConsumedCharge });
		public ZDecimal ToolsDiesMouldsCharges => GetAdditionCharges(new ZString[] { UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge });
		public ZDecimal MaterialsComponentsPartsCharges => MaterialsComponentsPartsChargesCore;
		protected virtual ZDecimal MaterialsComponentsPartsChargesCore => GetAdditionCharges(new ZString[] { UCCCustomsChargeTypeList.Codes.MaterialsComponentsPartsCharge });
		public ZDecimal IndirectAndOtherPaymentsCharges => GetAdditionCharges(new ZString[] { UCCCustomsChargeTypeList.Codes.IndirectAndOtherPaymentsCharge });
		public ZDecimal TransportCostsCharges => GetAdditionCharges(new ZString[] { UCCCustomsChargeTypeList.Codes.TransportCostsCharge, UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge });
		public ZDecimal InsuranceCostsCharges => GetAdditionCharges(new ZString[] { UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge });
		public ZDecimal InternationalTransportCharges => InternationalTransportChargesCore;
		protected virtual ZDecimal InternationalTransportChargesCore => ZDecimal.Zero;
		public ZDecimal ConstructionErectionAssemblyCharges => GetDeductionCharges(new ZString[] { UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge });
		public ZDecimal OtherNotElsewhereDeclaredCharges => OtherNotElsewhereDeclaredChargesCore;
		protected virtual ZDecimal OtherNotElsewhereDeclaredChargesCore => GetDeductionCharges(new ZString[] { UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge });
		public ZDecimal ImportDutiesOrOtherCharges => GetDeductionCharges(new ZString[] { UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge });
		public ZDecimal CostOfTransportOTEU => CostOfTransportOTEUCore;
		protected virtual ZDecimal CostOfTransportOTEUCore => GetDeductionCharges(new ZString[] { UCCCustomsChargeTypeList.Codes.AdjustmentCharge });

		public decimal GetAdditionCharges(ZString[] chargeCodes) => GetAdditionChargesCore(chargeCodes);
		protected virtual decimal GetAdditionChargesCore(ZString[] chargeCodes) => decimal.Round(EntryLine.InvoiceLines.Cast<JobComInvoiceLine>()
								.SelectMany(x => x.Charges.Cast<InvoiceLineCharge>())
								.Where(x => chargeCodes.Contains(x.J7_ChargeType) && (x.J7_IsDutiable && !x.J7_IsIncludedInITOT))
								.Select(x => GetAmountInCorrectCurrency(x.J7_Amount, x.Currency))
						.Concat(EntryLine.InvoiceLines.Cast<JobComInvoiceLine>()
								.SelectMany(x => x.ApportionedCharges).Cast<InvoiceLineApportionCharge>()
								.Where(x => chargeCodes.Contains(x.J7_ChargeType) && (x.J7_IsDutiable && !x.J7_IsIncludedInITOT))
								.Select(x => GetAmountInCorrectCurrency(x.J7_Amount, x.Currency)))
						.Sum(x => x), 2, MidpointRounding.AwayFromZero);

		public decimal GetDeductionCharges(ZString[] chargeCodes) => GetDeductionChargesCore(chargeCodes);
		protected virtual decimal GetDeductionChargesCore(ZString[] chargeCodes) => decimal.Round(EntryLine.InvoiceLines.Cast<JobComInvoiceLine>()
								.SelectMany(x => x.Charges.Cast<InvoiceLineCharge>())
								.Where(x => chargeCodes.Contains(x.J7_ChargeType) && (!x.J7_IsDutiable && x.J7_IsIncludedInITOT))
								.Select(x => GetAmountInCorrectCurrency(x.J7_Amount, x.Currency))
						.Concat(EntryLine.InvoiceLines.Cast<JobComInvoiceLine>()
								.SelectMany(x => x.ApportionedCharges).Cast<InvoiceLineApportionCharge>()
								.Where(x => chargeCodes.Contains(x.J7_ChargeType) && (!x.J7_IsDutiable && x.J7_IsIncludedInITOT))
								.Select(x => GetAmountInCorrectCurrency(x.J7_Amount, x.Currency)))
						.Sum(x => x), 2, MidpointRounding.AwayFromZero);

		protected virtual ZDecimal GetAmountInCorrectCurrency(ZDecimal chargeAmount, Enterprise.MasterFiles.Business.RefCurrency currency)
		{
			return currency?.ConvertUsingCustomsRate(ZDateTime.Today, chargeAmount, EntryLine.Declaration?.LocalCurrency) ?? chargeAmount;
		}

		public ZString EntryHeaderReference => EntryHeaderReferenceCore;
		protected virtual ZString EntryHeaderReferenceCore => EntryLine.Header.CH_BGMReference;

		public ZInt LineNumber => EntryLine.CL_LineNumber;

		public CusEntryLine EntryLine { get; set; }
	}
}
