using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Documents.DocDataObjects
{
	public class EntryLineDataObject : EU.Business.Documents.DocDataObjects.EntryLineDataObject
	{
		public EntryLineDataObject(CusEntryLine entryLine) : base(entryLine)
		{
		}

		protected override ZDecimal InternationalTransportChargesCore => GetAdditionCharges(new ZString[] { ESCustomsChargeTypeList.Codes.InternationalFreight });
		protected override ZDecimal OtherNotElsewhereDeclaredChargesCore
		{
			get
			{
				var otherChargesValue = -EntryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.JI_NegAdj);

				otherChargesValue -= (CostOfTransportOTEU + ConstructionErectionAssemblyCharges + ImportDutiesOrOtherCharges);

				return otherChargesValue;
			}
		}
		protected override ZDecimal CostOfTransportOTEUCore => GetDeductionCharges(new ZString[] { ESCustomsChargeTypeList.Codes.TransportCostsAfterEUEntry });
		protected override ZDecimal MaterialsComponentsPartsChargesCore => base.MaterialsComponentsPartsChargesCore + GetEGVChargeAmountForCalculation();

		protected override ZDecimal TotalBCore => CommissionsCharges + BrokerageCharges + ContainersAndPackingCharges + MaterialsComponentsPartsCharges + ToolsDiesMouldsCharges + MaterialsConsumedCharges +
									 EngineeringDevelopmentArtworkCharges + RoyaltiesAndLicenseCharges + ProceedsOfAnySubsequentResaleCharges + TransportCostsCharges + InsuranceCostsCharges + InternationalTransportCharges;

		protected override ZDecimal TotalCCore => ConstructionErectionAssemblyCharges + OtherNotElsewhereDeclaredCharges + ImportDutiesOrOtherCharges + CostOfTransportOTEU;

		protected override ZDecimal CustomsValueDeclaredCore => decimal.Round(GetESCustomsValue(), 2);

		protected virtual ZDecimal GetEGVChargeAmountForCalculation() => EntryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.ValuationCalculator.GetEGVChargeAmount());

		protected virtual ZDecimal GetESCustomsValue() => EntryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.JI_Calc_ESCustomsValue);

		protected override ZDecimal GetAmountInCorrectCurrency(ZDecimal chargeAmount, RefCurrency currency)
		{
			return currency != null ? EntryLine.RandomLine.InvoiceHeader.CurrencyConverter.ConvertExact(new Money(chargeAmount, currency), EntryLine.Declaration.LocalCurrency).Amount : chargeAmount;
		}
	}
}
