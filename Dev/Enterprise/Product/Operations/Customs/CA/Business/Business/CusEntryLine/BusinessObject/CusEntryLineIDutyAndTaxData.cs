namespace Enterprise.Customs.CA.Business
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.CA.Registry;

	partial class CusEntryLine : IDutyAndTaxData
	{
		#region Implementation of IDutyAndTaxData

		JobDeclaration IDutyAndTaxData.Declaration
		{
			get { return Declaration; }
		}

		ZBool IDutyAndTaxData.DDPDeductDutyOnly
		{
			get { return LineData.DDPDeductDutyOnly; }
		}

		ZString IDutyAndTaxData.CalculationMethod
		{
			get { return LineData.CalculationMethod; }
		}

		ZDecimal IDutyAndTaxData.ExchangeRate
		{
			get { return LineData.ExchangeRate; }
		}

		ZDateTime IDutyAndTaxData.EffectiveDutyDate
		{
			get { return LineData.EffectiveDutyDate; }
		}

		ZString IDutyAndTaxData.ClassificationNumber
		{
			get { return LineData.ClassificationNumber; }
		}

		ZString IDutyAndTaxData.TariffCode
		{
			get { return LineData.TariffCode; }
		}

		ZString IDutyAndTaxData.TreatmentCode
		{
			get { return LineData.TreatmentCode; }
		}

		ZDecimal IDutyAndTaxData.FOBValue
		{
			get { throw new NotSupportedException(); }
		}

		ZString IDutyAndTaxData.AdjustmentCode
		{
			get { throw new NotSupportedException(); }
		}

		ZDecimal IDutyAndTaxData.AdjustmentValue
		{
			get { throw new NotSupportedException(); }
		}

		ZDecimal IDutyAndTaxData.ValueForCurrencyConversion
		{
			get
			{
				return (from JobComInvoiceLine line in InvoiceLines
						select (decimal)(line.CA_CVforCurrConvOvr ? line.CA_CVforCurrConv
											: line.DutyAndTaxManager.GetValueForCurrencyConversionFromFOB())).Sum();
			}
		}

		ZDecimal IDutyAndTaxData.CustomsValue
		{
			get { return DutyAndTaxManager.GetCustomsValueForDuty(); }
			set { CL_CustomsValue = value; }
		}

		ZBool IDutyAndTaxData.CalculateCustomsValueWithoutDutyIfDutyPaid
		{
			get { return true; }
		}

		ZDecimal IDutyAndTaxData.NormalDutyPaidValue
		{
			get { return DutyAndTaxManager.NormalDutyPaidValue; }
		}

		ZDecimal IDutyAndTaxData.NormalValueForTax
		{
			get { return DutyAndTaxManager.NormalValueForTax; }
		}

		ZInt IDutyAndTaxData.MonthlyTimeLimit
		{
			get { return LineData.MonthlyTimeLimit; }
		}

		ZDecimal IDutyAndTaxData.CustomsQuantity
		{
			get { return CustomsQuantity; }
		}

		ZString IDutyAndTaxData.CustomsUnits
		{
			get { return CustomsUnitQty; }
		}

		ZDecimal IDutyAndTaxData.CustomsQuantity2
		{
			get { return (from JobComInvoiceLine line in InvoiceLines select (decimal)line.JI_CustomsSecondQuantity).Sum(); }
		}

		ZString IDutyAndTaxData.CustomsUnits2
		{
			get { return LineData.CustomsUnits2; }
			set { LineData.CustomsUnits2 = value; }
		}

		ZDecimal IDutyAndTaxData.CustomsQuantity3
		{
			get { return (from JobComInvoiceLine line in InvoiceLines select (decimal)line.JI_CustomsThirdQuantity).Sum(); }
		}

		ZString IDutyAndTaxData.CustomsUnits3
		{
			get { return LineData.CustomsUnits3; }
			set { LineData.CustomsUnits3 = value; }
		}

		ZString IDutyAndTaxData.DefaultGSTStatusCode
		{
			get { return LineData.DefaultGSTStatusCode; }
		}

		ZString IDutyAndTaxData.DefaultETRateCode
		{
			get { return LineData.DefaultETRateCode; }
		}

		ZString IDutyAndTaxData.DefaultETExemptionCode
		{
			get { return LineData.DefaultETExemptionCode; }
		}

		IDutyAndTaxData LineData
		{
			get { return RandomLine; }
		}

		ZBool IDutyAndTaxData.IsWarehouseOrSupplementaryEntry
		{
			get { return Declaration.IsWarehouseOrSupplementaryEntry; }
		}

		ZBool IDutyAndTaxData.IsB3ValidationRequired
		{
			get { return Declaration.IsB3ValidationRequired; }
		}

		ZBool IDutyAndTaxData.IsSIMADutyRequired
		{
			get { return LineData.IsSIMADutyRequired; }
		}

		ZString IDutyAndTaxData.AuthorityNumber
		{
			get { return LineData.AuthorityNumber; }
		}

		ZString IDutyAndTaxData.PreviousTransactionNumber
		{
			get { return ZString.Empty; }
		}

		ZInt IDutyAndTaxData.PreviousLineNumber
		{
			get { return ZInt.Zero; }
		}

		ZString IDutyAndTaxData.CountryOfOrigin
		{
			get { return LineData.CountryOfOrigin; }
		}

		ZString IDutyAndTaxData.CountryOfExport
		{
			get { return LineData.CountryOfExport; }
		}

		ZString IDutyAndTaxData.DumpingCaseNumber
		{
			get { return LineData.DumpingCaseNumber; }
		}

		DutyAndTaxUnitConverter IDutyAndTaxData.UnitConverter
		{
			get { return fUnitConverter ?? (fUnitConverter = new DutyAndTaxUnitConverter()); }
		}
		DutyAndTaxUnitConverter fUnitConverter;

		bool IDutyAndTaxData.IsSettingSIMAExemptCodeInProgress { get; set; }

		ZDecimal IDutyAndTaxData.ExchangeRateForSIMA => 1m;

		IEnumerable<DutyAndTax> IDutyAndTaxData.SIMADuties
		{
			get { return LineData.SIMADuties; }
		}

		IEnumerable<CACusRulingConfig> IDutyAndTaxData.RulingConfigs => LineData.RulingConfigs;

		event EventHandler IDutyAndTaxData.QuantityChanged
		{
			remove { }
			add { }
		}

		#endregion

		#region PopulateDutiesAndTaxesReturningManager

		internal ZDecimal GetValueForCurrencyConversion()
		{
			var data = ((IDutyAndTaxData)this);
			return data.CalculationMethod == CalculationMethods.Codes.DeliveredDutyPaid
					? DutyAndTaxManager.GetValueForCurrencyConversionFromCalculatedCustomsValue()
					: data.ValueForCurrencyConversion;
		}

		internal DutyAndTaxManager PopulateDutiesAndTaxesReturningManager()
		{
			dutyAndTaxManager = null;
			dutiesAndTaxes = new DutyAndTaxCollection(new BusinessObjectFactory(), this);
			DefaultDutiesAndTaxesFromInvoiceLines();
			DutyAndTaxManager.PopulateDutiesAndTaxes();
			return DutyAndTaxManager;
		}

		void DefaultDutiesAndTaxesFromInvoiceLines()
		{
			var simaDuties = from JobComInvoiceLine line in InvoiceLines
							 from duty in line.DutiesAndTaxes
							 where DutyAndTaxTypes.IsSIMATaxCodeIncludingSIMAType(duty.C1_TaxType)
							 select duty;

			if (simaDuties.Any())
			{
				var simaDuty = dutiesAndTaxes.AddNew();
				simaDuty.C1_Override = true;
				simaDuty.C1_TaxType = DutyAndTaxTypes.Codes.SIMADuty;
				simaDuty.C1_Amount = simaDuties.Sum(d => d.C1_Amount);
				simaDuty.C1_ExemptCode = simaDuties.First().C1_ExemptCode;
			}

			var randomLineDutiesAndTaxes = RandomLine.DutiesAndTaxes;
			if (randomLineDutiesAndTaxes != null && randomLineDutiesAndTaxes.Any())
			{
				foreach (var lineTax in randomLineDutiesAndTaxes.Where(d => d.IsTax))
				{
					var tax = dutiesAndTaxes.AddNew();
					tax.C1_TaxType = lineTax.C1_TaxType;
					tax.C1_Code = lineTax.C1_Code;
					tax.C1_ExemptCode = lineTax.C1_ExemptCode;
				}
			}
			return;
		}

		DutyAndTaxManager DutyAndTaxManager
		{
			get { return dutyAndTaxManager ?? (dutyAndTaxManager = new DutyAndTaxManager(dutiesAndTaxes, this)); }
		}

		internal void RefreshCalculatedFieldsForEntryLines()
		{
			InvoiceLines.Cast<JobComInvoiceLine>().ForEach(line => line.RefreshAggregatedFields());
		}

		DutyAndTaxManager dutyAndTaxManager;
		DutyAndTaxCollection dutiesAndTaxes;

		#endregion

		#region ApportionRoundingAmountsOverLines

		internal void ApportionRoundingAmountsOverLines()
		{
			DutyAndTaxManager.ApportionTotalValueOverLinesIfDifferentToLinesTotal(
				CL_CustomsValue,
				InvoiceLines.Cast<JobComInvoiceLine>(),
				line => line.CA_CustomsValue,
				(line, cents) => line.CA_CustomsValue += cents
				);

			ApportionDutyOrTaxRoundingOverLines(EntryChargeTypeList.Codes.TotalDutyAmount, DutyAndTaxTypes.Codes.CustomsDuty);
			ApportionDutyOrTaxRoundingOverLines(EntryChargeTypeList.Codes.TotalExciseTaxAmount, DutyAndTaxTypes.Codes.ExciseTax);
			if (Declaration.IsGSTDirectPayment)
			{
				ApportionDutyOrTaxRoundingOverLines(EntryChargeTypeList.Codes.TotalGSTDirectAmount, DutyAndTaxTypes.Codes.GST);
			}
			else
			{
				ApportionDutyOrTaxRoundingOverLines(EntryChargeTypeList.Codes.TotalGSTAmount, DutyAndTaxTypes.Codes.GST);
			}
		}

		void ApportionDutyOrTaxRoundingOverLines(string entryFeeType, string dutyOrTaxType)
		{
			DutyAndTaxManager.ApportionTotalValueOverLinesIfDifferentToLinesTotal(
				Fees.GetAmount(entryFeeType),
				InvoiceLines.Cast<JobComInvoiceLine>(),
				line => line.DutyAndTaxManager.GetTotalAmount(dutyOrTaxType),
				(line, cents) => AddCentsToInvoiceLineDutyOrTax(line, cents, dutyOrTaxType));
		}

		static void AddCentsToInvoiceLineDutyOrTax(JobComInvoiceLine line, decimal cents, string dutyOrTaxType)
		{
			DutyAndTaxManager.ApportionTotalValueOverLinesIfDifferentToLinesTotal(
				line.DutyAndTaxManager.GetTotalAmount(dutyOrTaxType) + cents,
				line.DutyAndTaxManager.GetDutiesOrTaxesByType(dutyOrTaxType),
				dutyOrTax => dutyOrTax.C1_Amount,
				(dutyOrTax, cents1) => dutyOrTax.C1_Amount += cents1);
		}

		ZDecimal IDutyAndTaxData.ValueForTax
		{
			get;
			set;
		}

		#endregion

		DutyAndTaxManager IDutyAndTaxData.DutyAndTaxManager
		{
			get { return DutyAndTaxManager; }
		}
	}
}
