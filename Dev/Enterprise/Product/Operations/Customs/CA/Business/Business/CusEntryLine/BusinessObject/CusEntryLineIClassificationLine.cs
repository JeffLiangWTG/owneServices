using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	partial class CusEntryLine : IClassificationLine1
	{
		#region IClassificationLine1 Members

		ZShort IClassificationLine1.B3LineNumber
		{
			get { return CL_LineNumber; }
		}

		ZShort IClassificationLine1.SequenceNumber
		{
			get { return CL_CommoditySequence; }
		}

		MessageSubTypes IClassificationLine1.MessageSubType => MessageSubTypes.Create;

		//TODO: Fix it.
		//There is definitely bug with negative amounts:
		//We decide if CVforCurrConv < 0 then class line NEG else POS.
		//But if we make CV < 0 then all V rates will be negative but S - positive and AllTotal will be rubbish.
		//I think that it should be some check box to indicate class line is POS or NEG. Leon will check.
		ZString IClassificationLine1.RecordIdentifier
		{
			get { return ((IClassificationLine1)this).ValueForCurrency < 0 ? MessageConstants.B3RecordIdentifiers.Negative : MessageConstants.B3RecordIdentifiers.Positive; }
		}

		ZInt IClassificationLine1.B3SubHeaderNumber
		{
			get { return RandomLine.CA_B3SubHeaderNumber; }
		}

		ZInt IClassificationLine1.B3SubHeaderNumberForLVX
		{
			get { return RandomLine.B3SubHeaderNumberForLVX; }
		}

		ZString IClassificationLine1.ClassificationNumber
		{
			get { return CL_AdValoremTariff; }
		}

		ZString IClassificationLine1.ValueForDutyCode
		{
			get { return RandomLine.EffectiveValueForDutyCode; }
		}

		ZString IClassificationLine1.TariffCode
		{
			get { return RandomLine.CA_99TariffCode; }
		}

		ZDecimal IClassificationLine1.ValueForCurrency
		{
			get { return Fees.GetAmount(EntryChargeTypeList.Codes.CustomsValueInInvoiceCurrency); }
		}

		ZDecimal IClassificationLine1.ValueForDuty
		{
			get { return CL_CustomsValue; }
		}

		ZDecimal IClassificationLine1.ValueForTax
		{
			get { return Fees.GetAmount(EntryChargeTypeList.Codes.CustomsValueForTax); }
		}

		ZString IClassificationLine1.AuthorityNumber
		{
			get { return RandomLine.CA_AuthorityNumber; }
		}

		ZString IClassificationLine1.TRSNumber
		{
			get { return RandomLine.CA_TRSNumber; }
		}

		ZString[] IClassificationLine1.PartNumberDescriptions
		{
			get { return new[] { Description }; }
		}

		IEnumerable<IInvoiceCrossReference> IClassificationLine1.InvoiceCrossReferences
		{
			get
			{
				return from JobComInvoiceLine invoiceLine in InvoiceLines
							 orderby invoiceLine.CA_PageNumber, invoiceLine.InvoiceCrossReferencePageLineNumber
							 group invoiceLine by new { invoiceLine.CA_PageNumber, invoiceLine.InvoiceCrossReferencePageLineNumber } into invoiceLines
							 select GetInvoiceCrossReference(invoiceLines);
			}
		}

		InvoiceCrossReference GetInvoiceCrossReference(IEnumerable<JobComInvoiceLine> lines)
		{
			var invoicePageNumber = ZInt.Zero;
			var invoiceLineNumber = ZInt.Zero;
			var invoiceValue = ZDecimal.Zero;
			var randomLine = lines.FirstOrDefault();
			if (randomLine != null)
			{
				invoicePageNumber = randomLine.CA_PageNumber;
				invoiceLineNumber = randomLine.InvoiceCrossReferencePageLineNumber;
				invoiceValue = randomLine.IsLuxuryTaxInvoiceLine ? new ZDecimal(0.01) : (ZDecimal)lines.Sum(l => l.JI_LinePrice);
			}
			return new InvoiceCrossReference(invoicePageNumber, invoiceLineNumber, invoiceValue);
		}

		ZDecimal IClassificationLine1.CustomsQuantity => CustomsQuantity;

		ZDecimal IClassificationLine1.CountOfInvoice => InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.InvoiceHeader.PK).Distinct().Count();

		ZString IClassificationLine1.CustomsUnitQty => CustomsUnitQty;

		ZDecimal IClassificationLine1.InvoiceQuantity => InvoiceQuantity;

		ZString IClassificationLine1.InvoiceUQ => InvoiceUQ;

		Money IClassificationLine1.TotalLinePrice => TotalLinePrice;

		Money IClassificationLine1.CustomsValue => CustomsValue;

		Money IClassificationLine1.FOB => FOB;

		ZDecimal IClassificationLine1.SalesTaxAmount => GetTotalAmountFromDutiesAndTaxes(DutyAndTaxTypes.Codes.CPT);

		ZDecimal IClassificationLine1.CTAAmount => GetTotalAmountFromDutiesAndTaxes(DutyAndTaxTypes.Codes.CTA);

		(ZDecimal Amount, RefCurrency Currency) IClassificationLine1.DeductionChargeAmountAndCurrency
		{
			get
			{
				if (!deductionChargeAmountAndCurrency.HasValue)
				{
					deductionChargeAmountAndCurrency = (0, null);
					var deductionChargesOfLine = InvoiceLines.OfType<JobComInvoiceLine>().SelectMany(x => x.Charges.OfType<InvoiceLineCharge>()).Where(x => x.J7_ChargeType == CustomsChargeTypeList.Codes.DeductionCharge);
					if (deductionChargesOfLine.Any())
					{
						var totalAmount = deductionChargesOfLine.Sum(x => x.J7_Amount);
						var currency = deductionChargesOfLine.First().Currency;
						deductionChargeAmountAndCurrency = (totalAmount, currency);
					}
					else
					{
						var deductionApportionedChargesOfLine = InvoiceLines.OfType<JobComInvoiceLine>().SelectMany(x => x.ApportionedCharges.OfType<InvoiceLineApportionCharge>()).Where(x => x.J7_ChargeType == CustomsChargeTypeList.Codes.DeductionCharge);
						if (deductionApportionedChargesOfLine.Any())
						{
							var totalAmount = deductionApportionedChargesOfLine.Sum(x => x.J7_Amount);
							var currency = deductionApportionedChargesOfLine.First().Currency;
							deductionChargeAmountAndCurrency = (totalAmount, currency);
						}
						else
						{
							var deductionChargesOfHeader = RandomLine.InvoiceHeader.Charges.Where(x => x.J7_ChargeType == CustomsChargeTypeList.Codes.DeductionCharge);
							if (deductionChargesOfHeader.Any())
							{
								var totalAmount = deductionChargesOfHeader.Sum(x => x.J7_Amount);
								var currency = deductionChargesOfHeader.First().Currency;
								deductionChargeAmountAndCurrency = (totalAmount, currency);
							}
						}
					}
				}
				return deductionChargeAmountAndCurrency.Value;
			}
		}
		(ZDecimal Amount, RefCurrency Currency)? deductionChargeAmountAndCurrency;

		ZString IClassificationLine1.CustomsDutyCode => RandomLine.DutyAndTaxManager.GetCode(DutyAndTaxTypes.Codes.CustomsDuty);

		ZDecimal IClassificationLine1.SurtaxAmount => GetTotalAmountFromDutiesAndTaxes(DutyAndTaxTypes.Codes.SUR);

		ZDecimal IClassificationLine1.SurtaxQuantity => GetTotalQuantityFromDutiesAndTaxes(DutyAndTaxTypes.Codes.SUR);

		ZString IClassificationLine1.SurtaxUnitOfMeasure => RandomLine.DutyAndTaxManager.GetUnitOfMeasure(DutyAndTaxTypes.Codes.SUR, getSpecifiedTypeForSIMA: true);

		ZString IClassificationLine1.SurtaxCode => RandomLine.DutyAndTaxManager.GetCode(DutyAndTaxTypes.Codes.SUR, getSpecifiedTypeForSIMA: true);

		ZString IClassificationLine1.SurtaxStatementCode => GetStatementCodeViaExemptCode(RandomLine.DutyAndTaxManager.GetExemptCode(DutyAndTaxTypes.Codes.SUR, getSpecifiedTypeForSIMA: true));

		ZBool IClassificationLine1.SurtaxIsOverride => RandomLine.DutyAndTaxManager.GetIsOverride(DutyAndTaxTypes.Codes.SUR, getSpecifiedTypeForSIMA: true);

		ZBool IClassificationLine1.HasSurtax => HasDutiesOrTaxes(DutyAndTaxTypes.Codes.SUR);

		ZDecimal IClassificationLine1.ADDAmount => GetTotalAmountFromDutiesAndTaxes(DutyAndTaxTypes.Codes.ADD);

		ZDecimal IClassificationLine1.ADDQuantity => GetTotalQuantityFromDutiesAndTaxes(DutyAndTaxTypes.Codes.ADD);

		ZString IClassificationLine1.ADDUnitOfMeasure => RandomLine.DutyAndTaxManager.GetUnitOfMeasure(DutyAndTaxTypes.Codes.ADD, getSpecifiedTypeForSIMA: true);

		ZString IClassificationLine1.ADDCode => RandomLine.DutyAndTaxManager.GetCode(DutyAndTaxTypes.Codes.ADD, getSpecifiedTypeForSIMA: true);

		ZBool IClassificationLine1.ADDIsOverride => RandomLine.DutyAndTaxManager.GetIsOverride(DutyAndTaxTypes.Codes.ADD, getSpecifiedTypeForSIMA: true);

		ZBool IClassificationLine1.HasADD => HasDutiesOrTaxes(DutyAndTaxTypes.Codes.ADD);

		ZDecimal IClassificationLine1.CVDAmount => GetTotalAmountFromDutiesAndTaxes(DutyAndTaxTypes.Codes.CVD);

		ZDecimal IClassificationLine1.CVDQuantity => GetTotalQuantityFromDutiesAndTaxes(DutyAndTaxTypes.Codes.CVD);

		ZString IClassificationLine1.CVDUnitOfMeasure => RandomLine.DutyAndTaxManager.GetUnitOfMeasure(DutyAndTaxTypes.Codes.CVD, getSpecifiedTypeForSIMA: true);

		ZString IClassificationLine1.CVDCode => RandomLine.DutyAndTaxManager.GetCode(DutyAndTaxTypes.Codes.CVD, getSpecifiedTypeForSIMA: true);

		ZBool IClassificationLine1.CVDIsOverride => RandomLine.DutyAndTaxManager.GetIsOverride(DutyAndTaxTypes.Codes.CVD, getSpecifiedTypeForSIMA: true);

		ZBool IClassificationLine1.HasCVD => HasDutiesOrTaxes(DutyAndTaxTypes.Codes.CVD);

		ZDecimal IClassificationLine1.SafeguardAmount => GetTotalAmountFromDutiesAndTaxes(DutyAndTaxTypes.Codes.SAF);

		ZString IClassificationLine1.SafeguardCode => RandomLine.DutyAndTaxManager.GetCode(DutyAndTaxTypes.Codes.SAF);

		ZBool IClassificationLine1.SafeguardIsOverride => RandomLine.DutyAndTaxManager.GetIsOverride(DutyAndTaxTypes.Codes.SAF);

		ZBool IClassificationLine1.HasSafeguard => HasDutiesOrTaxes(DutyAndTaxTypes.Codes.SAF);

		ZString IClassificationLine1.SafeguardStatementCode => GetStatementCodeViaExemptCode(RandomLine.DutyAndTaxManager.GetExemptCode(DutyAndTaxTypes.Codes.SAF));

		ZString IClassificationLine1.SIMACode
		{
			get { return SIMACode; }
		}

		ZString SIMACode
		{
			get { return RandomLine.DutyAndTaxManager.GetExemptCode(DutyAndTaxTypes.Codes.SIMADuty); }
		}

		ZString IClassificationLine1.SIMAStatementCode => GetStatementCodeViaExemptCode(SIMACode);

		ZDecimal IClassificationLine1.SIMAAssessment
		{
			get { return SIMAAssessment; }
		}

		ZDecimal SIMAAssessment
		{
			get { return Fees.GetAmount(EntryChargeTypeList.Codes.TotalSIMAAmount) + Fees.GetAmount(EntryChargeTypeList.Codes.TotalNonBillableSIMAAmount); }
		}

		ZDecimal IClassificationLine1.ExciseDutyAmount
		{
			get
			{
				return InvoiceLines.OfType<JobComInvoiceLine>().
				SelectMany(x => x.DutiesAndTaxes).
				Where(x => x.IsEXDDuty).
				Sum(x => x.C1_Amount);
			}
		}

		ZString IClassificationLine1.ExciseExemptionCode
		{
			get { return RandomLine.DutyAndTaxManager.GetExemptCode(DutyAndTaxTypes.Codes.ExciseTax); }
		}

		ZString IClassificationLine1.ExciseCode
		{
			get { return RandomLine.DutyAndTaxManager.GetCode(DutyAndTaxTypes.Codes.ExciseTax); }
		}

		ZDecimal ExciseTaxRateCore
		{
			get { return RandomLine.DutyAndTaxManager.GetRate(DutyAndTaxTypes.Codes.ExciseTax); }
		}

		ZDecimal IClassificationLine1.ExciseTaxRate
		{
			get { return ExciseTaxRateCore; }
		}

		ZDecimal IClassificationLine1.ExciseTaxRateToPrint
		{
			get { return ExciseTaxAmountCore == ZDecimal.Zero ? ZDecimal.Zero : ExciseTaxRateCore; }
		}

		ZString IClassificationLine1.ExciseTaxRateType
		{
			get { return RandomLine.DutyAndTaxManager.GetRateType(DutyAndTaxTypes.Codes.ExciseTax); }
		}

		ZDecimal ExciseTaxAmountCore
		{
			get { return Fees.GetAmount(EntryChargeTypeList.Codes.TotalExciseTaxAmount); }
		}

		ZDecimal IClassificationLine1.ExciseTaxAmount
		{
			get { return ExciseTaxAmountCore; }
		}

		bool IClassificationLine1.IsDummyExciseTaxRate
		{
			get { return false; }
		}

		ZBool IClassificationLine1.HasExcise => HasDutiesOrTaxes(DutyAndTaxTypes.Codes.ExciseTax);

		ZString IClassificationLine1.GSTExemptionCode => gstExemptCode;

		ZString gstExemptCode => RandomLine.DutyAndTaxManager.GetExemptCode(DutyAndTaxTypes.Codes.GST);

		ZString IClassificationLine1.GSTCode
		{
			get { return RandomLine.DutyAndTaxManager.GetCode(DutyAndTaxTypes.Codes.GST); }
		}

		ZDecimal IClassificationLine1.RateOfGST
		{
			get { return RandomLine.DutyAndTaxManager.GetRate(DutyAndTaxTypes.Codes.GST); }
		}

		ZString IClassificationLine1.GSTRateType
		{
			get { return RandomLine.DutyAndTaxManager.GetRateType(DutyAndTaxTypes.Codes.GST); }
		}

		ZDecimal IClassificationLine1.GSTAmount => gstAmount;

		ZDecimal gstAmount => Fees.GetAmount(EntryChargeTypeList.Codes.TotalGSTAmount) + Fees.GetAmount(EntryChargeTypeList.Codes.TotalGSTDirectAmount);

		ZDecimal IClassificationLine1.CUDAmount => Fees.GetAmount(EntryChargeTypeList.Codes.TotalDutyAmount);

		bool IClassificationLine1.HasGSTDetails
		{
			get { return RandomLine.DutyAndTaxManager.GSTaxes.Any() && (gstAmount > 0 || !gstExemptCode.IsEmpty); }
		}

		IEnumerable<IClassificationLine2> IClassificationLine1.ClassificationLines
		{
			get { return ClassificationLines; }
		}

		IEnumerable<IClassificationLine2> ClassificationLines
		{
			get { return ClassificationLine2.GetClassificationLines(this); }
		}

		ZInt IClassificationLine1.CountOfConsolidatedLines
		{
			get { return InvoiceLines.Count; }
		}

		BusinessObject IClassificationLine1.RelevantLine => RandomLine;

		ZDecimal GetTotalAmountFromDutiesAndTaxes(params ZString[] taxTypes)
		{
			return GetTotalValueFromDutiesAndTaxes((x) => x.C1_Amount, taxTypes);
		}

		ZDecimal GetTotalQuantityFromDutiesAndTaxes(params ZString[] taxTypes)
		{
			return GetTotalValueFromDutiesAndTaxes((x) => x.Quantity, taxTypes);
		}

		ZBool HasDutiesOrTaxes(params ZString[] taxTypes)
		{
			return InvoiceLines.OfType<JobComInvoiceLine>().
				SelectMany(x => x.DutiesAndTaxes).
				Any(x => taxTypes.Contains(x.C1_TaxType));
		}

		ZDecimal GetTotalValueFromDutiesAndTaxes(Func<DutyAndTax, ZDecimal> getValue, params ZString[] taxTypes)
		{
			return InvoiceLines.OfType<JobComInvoiceLine>().
				SelectMany(x => x.DutiesAndTaxes).
				Where(x => taxTypes.Contains(x.C1_TaxType)).
				Sum(x => getValue(x));
		}

		internal static ZString GetStatementCodeViaExemptCode(ZString exemptCode)
		{
			var result = ZString.Empty;

			if (exemptCode == SIMACodes.Codes.C10)
			{
				result = "N";
			}
			else if (exemptCode == SIMACodes.Codes.C20)
			{
				result = "U";
			}
			else if (!exemptCode.IsEmpty)
			{
				result = "S";
			}

			return result;
		}

		#endregion

		#region ClassificationLine2

		class ClassificationLine2 : IClassificationLine2
		{
			ClassificationLine2() { }

			public static IEnumerable<IClassificationLine2> GetClassificationLines(CusEntryLine entryLine)
			{
				var classLines = new List<ClassificationLine2>();
				var firstLine = true;
				ZDecimal exciseQtyTotal = 0m;
				var exciseUnits = ZString.Empty;
				foreach (JobComInvoiceLine line in entryLine.InvoiceLines)
				{
					if (line.DutyAndTaxManager.Duties.Any())
					{
						var i = 0;
						foreach (var tax in line.DutyAndTaxManager.Duties.OrderBy(a => a, new DutyAndTaxComparer()))
						{
							var customsUnits = line.Tariff != null ? line.Tariff.TariffUnits : line.JI_CustomsUnitQty;
							if (firstLine || classLines.Count < i + 1)
							{
								if (classLines.Count < i + 1)
								{
									classLines.Add(new ClassificationLine2 { B3LineNumber = entryLine.CL_LineNumber });
								}

								classLines[i].UnitOfMeasureCode = tax.C1_UnitOfMeasure.IsEmpty && i == 0 ? customsUnits : tax.C1_UnitOfMeasure;
								classLines[i].CustomsDutyRate = tax.C1_Rate;
								classLines[i].CustomsDutyRateType = tax.C1_RateType;
								classLines[i].PreviousTransactionNumber = tax.C1_PreviousTranNumber;
								classLines[i].PreviousLineNumber = tax.C1_PreviousTranLine;
								classLines[i].CustomsDutyAmount = entryLine.Fees.GetAmount(new ZString(EntryChargeTypeList.Codes.TotalDutyAmount).Left(2) + (i + 1));
							}

							var customsQuantity = customsUnits.IsEmpty ? ZDecimal.Zero : line.JI_CustomsQuantity;
							classLines[i].ClassificationLineQuantity += tax.Quantity.IsEmpty && i == 0 ? customsQuantity : tax.Quantity;
							i++;
						}

						if (entryLine.Header.IsB3GrossWeightSet)
						{
							classLines[0].WeightInKGM = GetB3GrossWeight(entryLine.Header);
						}
						entryLine.Header.IsB3GrossWeightSet = false;
						if (!line.JI_InvoiceUQ.IsEmpty)
						{
							classLines[0].InvoiceUnitOfMeasureCode = line.JI_InvoiceUQ;
						}

						classLines[0].ClassificationLineInvoiceQuantity += line.JI_InvoiceQuantity;
						firstLine = false;
					}

					foreach (var tax in line.DutyAndTaxManager.ExciseTaxes.Where(x => x.C1_RateType == RateTypes.Codes.Specific))
					{
						exciseQtyTotal += tax.Quantity;
						exciseUnits = tax.C1_UnitOfMeasure;
					}
				}

				if (!exciseUnits.IsEmpty && exciseQtyTotal > 0m)
				{
					bool exciseQuantityAlreadyIncluded = false;
					ClassificationLine2 emptyClassLine = null;
					foreach (var classLine in classLines)
					{
						if (classLine.UnitOfMeasureCode == exciseUnits)
						{
							exciseQuantityAlreadyIncluded = true;
							break;
						}
						if (emptyClassLine == null && classLine.UnitOfMeasureCode.IsEmpty && classLine.ClassificationLineQuantity == 0m)
						{
							emptyClassLine = classLine;
						}
					}
					if (!exciseQuantityAlreadyIncluded)
					{
						if (emptyClassLine == null)
						{
							emptyClassLine = new ClassificationLine2 { B3LineNumber = entryLine.CL_LineNumber };
							classLines.Add(emptyClassLine);
						}
						emptyClassLine.UnitOfMeasureCode = exciseUnits;
						emptyClassLine.ClassificationLineQuantity = exciseQtyTotal;
					}
				}

				return classLines.Cast<IClassificationLine2>();
			}

			static ZDecimal GetB3GrossWeight(CusEntryHeader header)
			{
				if (header.Declaration.IsLVS || header.GrossWeight.InKilogramsSafe >= 0 && header.GrossWeight.InKilogramsSafe <= 1)
				{
					return 1;
				}
				else
				{
					return header.GrossWeight.InKilogramsSafe.Round(0);
				}
			}

			#region Implementation of IClassificationLine2

			public ZInt B3LineNumber { get; private set; }
			public ZString UnitOfMeasureCode { get; private set; }
			public ZDecimal ClassificationLineQuantity { get; private set; }
			public ZDecimal WeightInKGM { get; private set; }
			public ZDecimal CustomsDutyRate { get; private set; }
			public ZString CustomsDutyRateType { get; private set; }
			public ZDecimal CustomsDutyAmount { get; private set; }
			public ZString PreviousTransactionNumber { get; private set; }
			public ZInt PreviousLineNumber { get; private set; }
			public ZString InvoiceUnitOfMeasureCode { get; private set; }
			public ZDecimal ClassificationLineInvoiceQuantity { get; private set; }

			#endregion
		}

		#endregion
	}
}
