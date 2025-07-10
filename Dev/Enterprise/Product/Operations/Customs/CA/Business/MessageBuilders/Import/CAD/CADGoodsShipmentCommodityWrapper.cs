using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CA.MessageContracts.CAD;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	public class CADGoodsShipmentCommodityWrapper : ICADDeclarationGoodsShipmentCommodity
	{
		public CADGoodsShipmentCommodityWrapper(IClassificationLine1 entryLine, MessageSubTypes messageSubType, bool isWarehouse)
		{
			this.classificationLine = entryLine;
			this.invoiceLine = entryLine.RelevantLine as JobComInvoiceLine;
			this.invoiceHeader = invoiceLine.InvoiceHeader;
			this.declaration = invoiceLine.Declaration;
			this.messageSubType = messageSubType;
			this.isWarehouse = isWarehouse;
			this.isLVS = declaration.IsLVS;
		}

		readonly IClassificationLine1 classificationLine;
		readonly JobComInvoiceLine invoiceLine;
		readonly JobComInvoiceHeader invoiceHeader;
		readonly JobDeclaration declaration;
		readonly MessageSubTypes messageSubType;
		readonly bool isWarehouse;
		readonly bool isLVS;

		#region ICADMessageDeclarationGoodsShipmentCommodity Members

		string ICADDeclarationGoodsShipmentCommodity.ExitDateTime
		{
			get
			{
				if (!(isLVS || declaration.IsWarehouseNotInType) && (messageSubType == MessageSubTypes.Change || messageSubType == MessageSubTypes.Amend))
				{
					return invoiceLine.InvoiceHeader.JZ_ValuationDateOverride.ToString("yyyyMMdd");
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		uint ICADDeclarationGoodsShipmentCommodity.SequenceNumeric
		{
			get
			{
				return (uint)classificationLine.B3LineNumber;
			}
		}

		string ICADDeclarationGoodsShipmentCommodity.Description
		{
			get
			{
				var result = ZString.Empty;
				if (isLVS && classificationLine.AuthorityNumber.IsEmpty && !HasSpecialTaxes)
				{
					result = JobMessageTypeList.Codes.LowValueShipments;
				}
				else
				{
					result = CADMessageHelper.StripOutInvalidCharacters(ZString.Join(",", classificationLine.PartNumberDescriptions)).SubstringSafe(0, 132);
				}
				return result;
			}
		}

		decimal ICADDeclarationGoodsShipmentCommodity.CountQuantity
		{
			get
			{
				var result = ZDecimal.Zero;
				if (isLVS && classificationLine.AuthorityNumber.IsEmpty && !HasSpecialTaxes)
				{
					result = classificationLine.CountOfInvoice;
				}
				else
				{
					result = classificationLine.CustomsQuantity;
					if (result.IsEmpty)
					{
						result = classificationLine.InvoiceQuantity;
					}
				}
				return ZArchitecture.Core.Utilities.Round(result, 3);
			}
		}

		string ICADDeclarationGoodsShipmentCommodity.CountQtyUnit => classificationLine.CustomsQuantity.IsEmpty || isLVS && classificationLine.AuthorityNumber.IsEmpty && !HasSpecialTaxes ? string.Empty : classificationLine.CustomsUnitQty;

		string ICADDeclarationGoodsShipmentCommodity.AcquisitionDateTime => ZString.Empty;

		IEnumerable<ICADMessageDeclarationAdditionalDocument> ICADDeclarationGoodsShipmentCommodity.AdditionalDocument
		{
			get
			{
				if (!invoiceLine.CA_AuthorityNumber.IsEmpty)
				{
					return new CADDeclarationAdditionalDocument[] { new CADDeclarationAdditionalDocument(invoiceLine.CA_AuthorityNumber, invoiceLine.CA_RemissionType) };
				}
				return Array.Empty<CADDeclarationAdditionalDocument>();
			}
		}

		bool ICADDeclarationGoodsShipmentCommodity.AdditionalDocumentSpecified => false;

		IEnumerable<ICADMessageDeclarationAdditionalInformation> ICADDeclarationGoodsShipmentCommodity.AdditionalInformation
		{
			get
			{
				var list = new List<ICADMessageDeclarationAdditionalInformation>();

				if (classificationLine.HasADD || classificationLine.HasCVD)
				{
					list.Add(new CADDeclarationAdditionalInformation(statementCode: classificationLine.SIMAStatementCode, statementTypeCode: AdditionalInformationTypeCodes.Codes.ASJ));

					var simaStatementCode = ZString.Empty;
					if (!invoiceLine.CA_SIMADumpingDesc.IsEmpty)
					{
						simaStatementCode = ExtractBracketsText(invoiceLine.CA_SIMADumpingDesc);
					}
					if (!simaStatementCode.IsEmpty)
					{
						list.Add(new CADDeclarationAdditionalInformation(statementCode: simaStatementCode, statementTypeCode: AdditionalInformationTypeCodes.Codes.MIF));
					}
					else
					{
						var addOrCVDCode = classificationLine.ADDCode;
						if (addOrCVDCode.IsEmpty)
						{
							addOrCVDCode = classificationLine.CVDCode;
						}
						if (!addOrCVDCode.IsEmpty)
						{
							list.Add(new CADDeclarationAdditionalInformation(statementCode: addOrCVDCode, statementTypeCode: AdditionalInformationTypeCodes.Codes.MIF));
						}
					}
				}

				if (classificationLine.HasSurtax)
				{
					list.Add(new CADDeclarationAdditionalInformation(statementCode: classificationLine.SurtaxStatementCode, statementTypeCode: AdditionalInformationTypeCodes.Codes.BSJ));
				}

				if (classificationLine.HasSafeguard)
				{
					list.Add(new CADDeclarationAdditionalInformation(statementCode: classificationLine.SafeguardStatementCode, statementTypeCode: AdditionalInformationTypeCodes.Codes.CSJ));
				}

				if (isLVS)
				{
					list.Add(new CADDeclarationAdditionalInformation(ValueForDutyCodes.Codes.UnrelatedFirmsPaidPayableWithoutAdjustments.PadLeft(3, '0'), AdditionalInformationTypeCodes.Codes.VDC));
				}
				else
				{
					if (!invoiceLine.CA_ValueForDutyCode.IsEmpty)
					{
						list.Add(new CADDeclarationAdditionalInformation(invoiceLine.CA_ValueForDutyCode.PadLeft(3, '0'), AdditionalInformationTypeCodes.Codes.VDC));
					}

					var timeLimitRange = invoiceLine.InvoiceHeader.TimeLimitRange;
					if (!timeLimitRange.TimeLimitStart.IsEmpty && !timeLimitRange.TimeLimitEnd.IsEmpty)
					{
						if (isWarehouse)
						{
							list.Add(new CADDeclarationAdditionalInformation(statementTypeCode: AdditionalInformationTypeCodes.Codes.TLS, limitDateTime: timeLimitRange.TimeLimitStart.ToString("yyyyMMdd")));
							list.Add(new CADDeclarationAdditionalInformation(statementTypeCode: AdditionalInformationTypeCodes.Codes.TLE, limitDateTime: timeLimitRange.TimeLimitEnd.ToString("yyyyMMdd")));
							list.Add(new CADDeclarationAdditionalInformation(statementTypeCode: AdditionalInformationTypeCodes.Codes.TLT, statementCode: "2"));
						}
						else if (declaration.JE_MessageSubType == B3EntryTypeList.Codes.Confirming && CalculationMethods.IsTemporaryImport(invoiceLine.CA_CalculationMethod))
						{
							list.Add(new CADDeclarationAdditionalInformation(statementTypeCode: AdditionalInformationTypeCodes.Codes.TLS, limitDateTime: timeLimitRange.TimeLimitStart.ToString("yyyyMMdd")));
							list.Add(new CADDeclarationAdditionalInformation(statementTypeCode: AdditionalInformationTypeCodes.Codes.TLE, limitDateTime: timeLimitRange.TimeLimitEnd.ToString("yyyyMMdd")));
						}
					}
				}
				return list.ToArray();
			}
		}

		ZString ExtractBracketsText(ZString input)
		{
			var result = ZString.Empty;
			var texts = input.Split("(");
			foreach (var text in texts)
			{
				if (text.Contains(')'))
				{
					result = text.Substring(0, text.IndexOf(')'));
					break;
				}
			}
			return result.ToUpper();
		}

		decimal ICADDeclarationGoodsShipmentCommodity.ConstituentElementPercentNumeric => invoiceLine.AlcoholPercentByVolume;

		IEnumerable<ICADMessageDeclarationGoodsShipmentCommodityClassification> ICADDeclarationGoodsShipmentCommodity.Classification
		{
			get
			{
				var list = new List<CADDeclarationGoodsShipmentCommodityClassification>();
				if (isLVS && classificationLine.AuthorityNumber.IsEmpty && !HasSpecialTaxes)
				{
					list.Add(new CADDeclarationGoodsShipmentCommodityClassification("0000999900"));
				}
				else
				{
					list.Add(new CADDeclarationGoodsShipmentCommodityClassification(classificationLine.ClassificationNumber));
					if (!invoiceLine.CA_99TariffCode.IsEmpty)
					{
						list.Add(new CADDeclarationGoodsShipmentCommodityClassification(invoiceLine.CA_99TariffCode, false));
					}
				}
				return list.ToArray();
			}
		}

		ZBool HasSpecialTaxes
		{
			get { return classificationLine.HasSurtax || classificationLine.HasCVD || classificationLine.HasADD || classificationLine.HasExcise || classificationLine.HasSafeguard; }
		}

		string ICADDeclarationGoodsShipmentCommodity.DestinationRegionID
		{
			get
			{
				return invoiceLine.CA_IsCasualImport ? invoiceLine.CA_CasualImportDestinationProvince : ZString.Empty;
			}
		}

		ICADMessageRegion ICADDeclarationGoodsShipmentCommodity.ExportCountry
		{
			get
			{
				if (exportCountry == null)
				{
					exportCountry = new CachedProperty<CADRegion>(classificationLine.Factory, delegate
					{
						CADRegion result = null;
						var randomLine = invoiceLine;
						var tariffTreatmentCode = randomLine.EffectiveTreatmentCode;
						if (isLVS && classificationLine.AuthorityNumber.IsEmpty && !HasSpecialTaxes &&
							(tariffTreatmentCode == TariffTreatmentCodes.Codes.MostFavouredNation ||
							tariffTreatmentCode == TariffTreatmentCodes.Codes.UnitedStates ||
							tariffTreatmentCode == TariffTreatmentCodes.Codes.CanadaIsraelAgreement))
						{
							result = new CADRegion(Core.Constants.CountryCodes.UnitedStates, USStatesList.Codes.NewYork);
						}
						else
						{
							var header = randomLine.InvoiceHeader;
							var countryCode = isLVS ? randomLine.CA_RN_NKExport : header.CA_RN_NKExport;
							if (!countryCode.IsEmpty)
							{
								result = new CADRegion(countryCode, countryCode == Core.Constants.CountryCodes.UnitedStates ? (randomLine.CA_USStateOfExport.IsEmpty ? header.CA_USStateOfExport : randomLine.CA_USStateOfExport) : ZString.Empty);
							}
						}
						return result;
					});
				}
				return exportCountry.Value;
			}
		}

		CachedProperty<CADRegion> exportCountry;

		string ICADDeclarationGoodsShipmentCommodity.ExporterID
		{
			get
			{
				var exportID = ZString.Empty;
				if (classificationLine.HasADD || classificationLine.HasCVD)
				{
					var exporter = invoiceHeader.ExporterDocumentaryAddress?.Organisation;
					exportID = exporter == null ? ZString.Empty : exporter.GetCABusinessNumber();
				}
				return exportID;
			}
		}

		ICADMessageMoney ICADDeclarationGoodsShipmentCommodity.InvoiceLineCharge
		{
			get
			{
				CADMessageMoney result = null;
				if (!isLVS && (classificationLine.HasADD || classificationLine.HasCVD || classificationLine.HasSurtax || classificationLine.HasSafeguard))
				{
					var money = classificationLine.FOB;
					result = new CADMessageMoney(money.Amount, money.Currency);
				}
				return result;
			}
		}

		ICADMessageRegion ICADDeclarationGoodsShipmentCommodity.Origin
		{
			get
			{
				if (origin == null)
				{
					origin = new CachedProperty<CADRegion>(classificationLine.Factory, () =>
					{
						CADRegion result = null;
						var randomLine = invoiceLine;
						var tariffTreatmentCode = randomLine.EffectiveTreatmentCode;
						if (isLVS && classificationLine.AuthorityNumber.IsEmpty && !HasSpecialTaxes &&
							(tariffTreatmentCode == TariffTreatmentCodes.Codes.MostFavouredNation ||
							tariffTreatmentCode == TariffTreatmentCodes.Codes.UnitedStates ||
							tariffTreatmentCode == TariffTreatmentCodes.Codes.CanadaIsraelAgreement))
						{
							result = new CADRegion(Core.Constants.CountryCodes.UnitedStates, USStatesList.Codes.NewYork);
						}
						else if (invoiceLine.CountryOfOrigin is RefCountry countryOfOrigin)
						{
							result = new CADRegion(countryOfOrigin.Code, countryOfOrigin.Code == Core.Constants.CountryCodes.UnitedStates ? invoiceLine.JI_StateOrRegionOfOrigin : ZString.Empty);
						}
						return result;
					});
				}
				return origin.Value;
			}
		}
		CachedProperty<CADRegion> origin;

		ICADMessageDeclarationGoodsShipmentCommodityPreviousDocument ICADDeclarationGoodsShipmentCommodity.PreviousDocument
		{
			get
			{
				if (!isLVS && isWarehouse)
				{
					var lineNumeric = invoiceLine.DutiesAndTaxes.Select(x => x.C1_PreviousTranLine).FirstOrDefault(y => !y.IsEmpty);
					if (!lineNumeric.IsEmpty)
					{
						var numericFormatted = (declaration.IsWarehouseEntry && declaration.JE_MessageSubType != B3EntryTypeList.Codes.ExWarehouse22) ? (lineNumeric % 100000).ToString().PadLeft(5, '0') : (lineNumeric % 1000000).ToString().PadLeft(6, '0');
						return new CADDeclarationGoodsShipmentCommodityPreviousDocument(numericFormatted, "632");
					}
				}
				return null;
			}
		}

		string ICADDeclarationGoodsShipmentCommodity.ProductID => !isLVS && (classificationLine.HasADD || classificationLine.HasCVD) ? invoiceLine.JI_Model : ZString.Empty;

		string ICADDeclarationGoodsShipmentCommodity.TradeTermsConditionCode
		{
			get
			{
				var result = ZString.Empty;
				if (!isLVS && (classificationLine.HasADD || classificationLine.HasCVD || classificationLine.HasSurtax))
				{
					result = invoiceHeader.JZ_IncoTerm;
				}
				return result;
			}
		}

		IEnumerable<ICADMessageDeclarationGoodsShipmentCommodityDutyTaxFee> ICADDeclarationGoodsShipmentCommodity.DutyTaxFee
		{
			get
			{
				var isPRECARM = invoiceLine.Declaration.ParentRelatedDeclaration != null;
				var currency = RefCurrency.LoadFromCurrencyCode(classificationLine.Factory, Core.Constants.CurrencyCodes.Canada);
				var totPayment = ZDecimal.Zero;

				if (invoiceLine.CA_IsCasualImport)
				{
					var salesTaxAmount = classificationLine.SalesTaxAmount;
					if (!salesTaxAmount.IsEmpty)
					{
						yield return new CADDeclarationGoodsShipmentCommodityDutyTaxFee(Helper.ConvertDutyAndTaxType(DutyAndTaxTypes.Codes.CPT, invoiceLine), (null, new CADMessageMoney(salesTaxAmount, currency)));
					}

					var ctaAmount = classificationLine.CTAAmount;
					if (!ctaAmount.IsEmpty)
					{
						yield return new CADDeclarationGoodsShipmentCommodityDutyTaxFee(Helper.ConvertDutyAndTaxType(DutyAndTaxTypes.Codes.CTA, invoiceLine), (null, new CADMessageMoney(ctaAmount, currency)));
					}
				}

				var deductionChargeAmount = classificationLine.DeductionChargeAmountAndCurrency.Amount;
				if (!deductionChargeAmount.IsEmpty)
				{
					yield return new CADDeclarationGoodsShipmentCommodityDutyTaxFee(CADDutyTaxFeeTypeCodes.Codes.EXD, new CADMessageMoney(deductionChargeAmount, classificationLine.DeductionChargeAmountAndCurrency.Currency));
				}

				var customsDutyCode = classificationLine.CustomsDutyCode;
				if (!customsDutyCode.IsEmpty && customsDutyCode != DutyAndTaxTypes.Constant.NO)
				{
					yield return new CADDeclarationGoodsShipmentCommodityDutyTaxFee(CADDutyTaxFeeTypeCodes.Codes.FET, customsDutyCode);
				}

				var exciseCode = classificationLine.ExciseCode;
				var exciseAmount = classificationLine.ExciseTaxAmount;
				var exciseTaxRateType = classificationLine.ExciseTaxRateType;
				if (!exciseAmount.IsEmpty || (!exciseCode.IsEmpty && exciseCode != DutyAndTaxTypes.Constant.NO))
				{
					totPayment += exciseAmount;
					if (exciseTaxRateType == RateTypes.Codes.AcceptT || exciseTaxRateType == RateTypes.Codes.AcceptX)
					{
						yield return new CADDeclarationGoodsShipmentCommodityDutyTaxFee(CADDutyTaxFeeTypeCodes.Codes.FET, exciseCode);
						var amount = exciseAmount + classificationLine.ExciseDutyAmount;
						yield return new CADDeclarationGoodsShipmentCommodityDutyTaxFee(CADDutyTaxFeeTypeCodes.Codes.EXC,
								payment: (null, new CADMessageMoney(amount, currency)));
					}
					else
					{
						if (UniversalReferenceConstants.SLFCALCEXSIsValid)
						{
							yield return new CADDeclarationGoodsShipmentCommodityDutyTaxFee(CADDutyTaxFeeTypeCodes.Codes.FET,
								payment: (null, !exciseAmount.IsEmpty && UniversalReferenceConstants.ExciseTaxCodesRequireSelfCalculation.Contains<string>(exciseCode) ? new CADMessageMoney(classificationLine.ExciseTaxAmount, currency) : null),
								dutyRegimeCode: exciseCode);
						}
						else
						{
							yield return new CADDeclarationGoodsShipmentCommodityDutyTaxFee(CADDutyTaxFeeTypeCodes.Codes.FET, dutyTaxFeeAssessmentBasis: exciseAmount.IsEmpty ? null : new[] { new CADMessageMoney(classificationLine.ExciseTaxAmount, currency) }, dutyRegimeCode: exciseCode);
						}
					}
				}

				if (!classificationLine.ValueForTax.IsEmpty)
				{
					yield return new CADDeclarationGoodsShipmentCommodityDutyTaxFee(CADDutyTaxFeeTypeCodes.Codes.VFT, (new CADMessageMoney(classificationLine.ValueForTax, currency), null));
				}

				var gstExemptionCode = classificationLine.GSTExemptionCode;
				if (!gstExemptionCode.IsEmpty)
				{
					yield return new CADDeclarationGoodsShipmentCommodityDutyTaxFee(Helper.ConvertDutyAndTaxType(DutyAndTaxTypes.Codes.GST, invoiceLine), gstExemptionCode);
				}
				else
				{
					var gstAmount = classificationLine.GSTAmount;
					if (!gstAmount.IsEmpty)
					{
						totPayment += gstAmount;
						var dutyRegimeCode = classificationLine.GSTCode.Length > 2 ? classificationLine.GSTCode.Right(2) : classificationLine.GSTCode;
						yield return new CADDeclarationGoodsShipmentCommodityDutyTaxFee(Helper.ConvertDutyAndTaxType(DutyAndTaxTypes.Codes.GST, invoiceLine), (null, new CADMessageMoney(gstAmount, currency)), dutyRegimeCode);
					}
				}

				if (classificationLine.HasSurtax && classificationLine.SurtaxCode is ZString surtaxCode && !surtaxCode.IsEmpty)
				{
					var surPayment = isLVS || isPRECARM ? new CADMessageMoney(classificationLine.SurtaxAmount, currency) : null;
					var requestOverrideCode = (isLVS || isPRECARM) && classificationLine.SurtaxIsOverride ? CheckOverrideBox : string.Empty;
					yield return new CADDeclarationGoodsShipmentCommodityDutyTaxFee(Helper.ConvertDutyAndTaxType(DutyAndTaxTypes.Codes.SUR, invoiceLine), (null, surPayment), surtaxCode, requestOverrideCode);
				}

				if (classificationLine.HasADD && classificationLine.ADDUnitOfMeasure is ZString addUnitOfMeasure && !addUnitOfMeasure.IsEmpty)
				{
					var requestOverrideCode = classificationLine.ADDIsOverride ? CheckOverrideBox : string.Empty;
					yield return new CADDeclarationGoodsShipmentCommodityDutyTaxFee(Helper.ConvertDutyAndTaxType(DutyAndTaxTypes.Codes.ADD, invoiceLine), (null, new CADMessageMoney(classificationLine.ADDAmount, currency)), (classificationLine.ADDQuantity, addUnitOfMeasure), dutyTaxFeeAssessmentBasis: null, dutyRegimeCode: classificationLine.ADDCode, requestOverrideCode: requestOverrideCode);
				}

				if (classificationLine.HasCVD && classificationLine.CVDUnitOfMeasure is ZString cvdUnitOfMeasure && !cvdUnitOfMeasure.IsEmpty)
				{
					var requestOverrideCode = classificationLine.CVDIsOverride ? CheckOverrideBox : string.Empty;
					yield return new CADDeclarationGoodsShipmentCommodityDutyTaxFee(Helper.ConvertDutyAndTaxType(DutyAndTaxTypes.Codes.CVD, invoiceLine), (null, new CADMessageMoney(classificationLine.CVDAmount, currency)), (classificationLine.CVDQuantity, cvdUnitOfMeasure), dutyTaxFeeAssessmentBasis: null, dutyRegimeCode: classificationLine.CVDCode, requestOverrideCode: requestOverrideCode);
				}

				if (classificationLine.HasSafeguard && classificationLine.SafeguardCode is ZString safeguardCode && !safeguardCode.IsEmpty)
				{
					var safPayment = isLVS || isPRECARM ? new CADMessageMoney(classificationLine.SafeguardAmount, currency) : null;
					var requestOverrideCode = (isLVS || isPRECARM) && classificationLine.SafeguardIsOverride ? CheckOverrideBox : string.Empty;
					yield return new CADDeclarationGoodsShipmentCommodityDutyTaxFee(Helper.ConvertDutyAndTaxType(DutyAndTaxTypes.Codes.SAF, invoiceLine), (null, safPayment), safeguardCode, requestOverrideCode);
				}

				totPayment += classificationLine.CUDAmount;
				var cudPayment = isLVS || isPRECARM ? new CADMessageMoney(classificationLine.CUDAmount, currency) : null;
				yield return new CADDeclarationGoodsShipmentCommodityDutyTaxFee(CADDutyTaxFeeTypeCodes.Codes.CUD, (null, cudPayment), invoiceLine.CA_TreatmentCode);

				if (isPRECARM || isLVS || (declaration.IsWarehouseEntry && declaration.JE_MessageSubType != B3EntryTypeList.Codes.ExWarehouse22))
				{
					var valueForCurrency = invoiceHeader.CurrencyConverter.ConvertExact(new Money(classificationLine.ValueForCurrency, invoiceHeader.Invoice_Currency), currency);
					var valueForDuty = new Money(classificationLine.ValueForDuty, currency);

					yield return new CADDeclarationGoodsShipmentCommodityDutyTaxFee(CADDutyTaxFeeTypeCodes.Codes.TOT, (null, new CADMessageMoney(totPayment, currency)), new[] { new CADMessageMoney(valueForCurrency.Amount, currency), new CADMessageMoney(valueForDuty.Amount, currency) });
				}
				else
				{
					var valueForDuty = new Money(classificationLine.ValueForDuty, currency);
					var valueForCurrency = invoiceHeader.CurrencyConverter.ConvertExact(valueForDuty, invoiceHeader.Invoice_Currency);
					yield return new CADDeclarationGoodsShipmentCommodityDutyTaxFee(CADDutyTaxFeeTypeCodes.Codes.TOT, new[] { new CADMessageMoney(valueForCurrency.Amount, valueForCurrency.Currency) });
				}
			}
		}

		#endregion

		B3AndCADDocumentHelper Helper => helper ?? (helper = new B3AndCADDocumentHelper());
		B3AndCADDocumentHelper helper;

		const string CheckOverrideBox = "X";
	}
}
