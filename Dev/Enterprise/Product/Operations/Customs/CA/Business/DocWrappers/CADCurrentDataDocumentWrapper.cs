using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using static Enterprise.Customs.CA.Business.MessageBuilders.B3ImportMessageWrapper;

namespace Enterprise.Customs.CA.Business
{
	public class CADCurrentDataDocumentWrapper : ICADHeader
	{
		public CADCurrentDataDocumentWrapper(CusEntryHeader entryHeader)
		{
			this.declaration = entryHeader.Declaration;
			this.parentRelatedDeclaration = declaration.ParentRelatedDeclaration;
			CaculateDeclarationTotals();
		}
		readonly JobDeclaration declaration;
		readonly JobDeclaration parentRelatedDeclaration;

		ZString ICADHeader.TypeCode_Box1 => declaration.JE_MessageSubType;

		ZString ICADHeader.WsSType_Box2 => ZString.Empty;

		ZDateTime ICADHeader.AccountingDate_Box3 => declaration.B3AcceptedDate;

		ZString ICADHeader.AccountSecurityCode_Box4 => declaration.TransactionNumber.AccountSecurityCode;

		ZString ICADHeader.CADTransactionNo_Box4 => declaration.TransactionNumber.UniqueIdentifier;

		ZString ICADHeader.OfficeNo_Box5 => declaration.JE_CustomsOffice.TrimStart('0');

		ZString ICADHeader.ModeOfTransport_Box6 => TransportTypeList.GetTransportModeNumber(declaration.JE_TransportMode);

		ZDateTime ICADHeader.ReleaseDate_Box7 => declaration.JE_EntryAuthorisationDate;

		ZDecimal ICADHeader.GrossWeightKg_Box8 => new ZWeight(declaration.JE_TotalWeight, declaration.JE_TotalWeightUnit).InKilogramsSafe;

		ZString ICADHeader.CarrierCodeAtImportation_Box9 => declaration.JE_CarrierCode;

		ZString ICADHeader.Pre_CARM_Box10 => parentRelatedDeclaration != null ? "1" : "0";

		ZString ICADHeader.Z1_RPP => ZString.Empty;

		ZString ICADHeader.ImporterBN_Box11
		{
			get
			{
				var result = string.Empty;
				if (declaration.IsConsolidatedLVS)
				{
					result = IB3HeaderHelper.GetLVSBusinessNumber(declaration.Importer);
				}
				else
				{
					result = IB3HeaderHelper.GetBusinessNumber(declaration.IsExistingEffectiveCasualImport, declaration.ImporterOfRecord, declaration.Importer);
				}
				return result;
			}
		}

		ZString ICADHeader.ImporterDetails_Box12
		{
			get
			{
				if (!importerDetails_Box12.HasValue)
				{
					importerDetails_Box12 = ZString.Empty;
					var importerAddress = declaration.ImporterOfRecordAddress.HasRealAddress ? declaration.ImporterOfRecordAddress : (IDocAddress)declaration.Importer?.MainAddress;
					if (importerAddress != null)
					{
						if (importerAddress is OrgAddress address)
						{
							importerDetails_Box12 = AdjustmentDocHelper.AddressForImporterFormatted(address);
						}
						else if (importerAddress is JobDocAddress docAddress)
						{
							importerDetails_Box12 = docAddress == null ? importerAddress.E2_CompanyNameTruncated : AdjustmentDocHelper.AddressForImporterFormatted(docAddress);
						}
					}
				}
				return importerDetails_Box12.Value;
			}
		}
		ZString? importerDetails_Box12;

		ZString ICADHeader.BrokerOrAgentBN_Box13 => declaration.BrokerBusinessNumber;

		ZString ICADHeader.BrokerOrAgentDetails_Box14
		{
			get
			{
				if (!brokerOrAgentDetails_Box14.HasValue)
				{
					brokerOrAgentDetails_Box14 = ZString.Empty;
					var brokerAddress = declaration.GetOrgProxyWithCABusinessNumber().MainAddress;
					brokerOrAgentDetails_Box14 = AdjustmentDocHelper.AddressForImporterFormatted(brokerAddress);
				}
				return brokerOrAgentDetails_Box14.Value;
			}
		}
		ZString? brokerOrAgentDetails_Box14;

		ZString ICADHeader.CargoControlNo_Box15
		{
			get
			{
				var ccns = declaration.CargoControlNumbers.Where(x => !x.CY_CargoControlNumber.IsEmpty).Select(x => x.CY_CargoControlNumber);
				return ccns.Count() == 1 ? ccns.First() : ZString.Empty;
			}
		}

		ZString ICADHeader.RecordOfIntentNo_Box16 => ZString.Empty;

		ZString ICADHeader.PreviousTransactionNo_Box17 => parentRelatedDeclaration?.TransactionNumber.FormattedTransactionNumber ?? ZString.Empty;

		ZDateTime ICADHeader.AcceptedDate_Box18 => declaration.B3AcceptedDate;

		ZString ICADHeader.OriginalTransactionNo_Box19 => parentRelatedDeclaration?.TransactionNumber.FormattedTransactionNumber ?? ZString.Empty;

		ZString ICADHeader.PrevTransNoWarehouse_Box20 => ZString.Empty;

		ZString ICADHeader.PortOfUnlading_Box21 => declaration.CA_UnladingOffice;

		ZString ICADHeader.Notes_Box35 => declaration.CADComments;

		ZDecimal ICADHeader.TotalValueForDuty_Box113 => totalValueForDuty;

		ZDecimal ICADHeader.TotalPSTAndHST_Box114 => totalPSTAndHST;

		ZDecimal ICADHeader.TotalPSTCannabisAmount_Box115 => totalPSTCannabisAmount;

		ZDecimal ICADHeader.TotalProvAlcoholTaxAmount_Box116 => totalProvAlcoholTaxAmount;

		ZDecimal ICADHeader.TotalProvTobaccoAmount_Box117 => totalProvTobaccoAmount;

		ZDecimal ICADHeader.TotalDeclarationRelieved_Box118 => totalDeclarationRelieved;

		ZDecimal ICADHeader.TotalAmount_Box119 => totalAmount;

		ZDecimal ICADHeader.TotalCustomsDuties_Box120 => totalCustomsDuties;

		ZDecimal ICADHeader.TotalExciseDuties_Box121 => totalExciseDuties;

		ZDecimal ICADHeader.TotalExciseTaxes_Box122 => totalExciseTaxes;

		ZDecimal ICADHeader.TotalGST_Box123 => totalGST;

		ZDecimal ICADHeader.TotalAnti_Dumping_Box124 => totalAnti_Dumping;

		ZDecimal ICADHeader.TotalCountervailing_Box125 => totalCountervailing;

		ZDecimal ICADHeader.TotalSurtaxes_Box126 => totalSurtaxes;

		ZDecimal ICADHeader.TotalSafeguards_Box127 => totalSafeguards;

		ZDecimal ICADHeader.TotalInterest_Box128 => totalInterest;

		ZDecimal ICADHeader.TotalDutiesAndTaxesWithInterest_Box129 => totalDutiesAndTaxesWithInterest;

		ZDecimal ICADHeader.TotalDutiesAndTaxes_Box130 => totalDutiesAndTaxes;

		IEnumerable<ICADSubHeader> ICADHeader.CADSubHeaders
		{
			get
			{
				if (cADSubHeaders == null)
				{
					if (declaration.IsLVS)
					{
						var wrapper = new B3ImportMessageWrapper(declaration.B3EntryHeader, true);
						cADSubHeaders = wrapper.GetB3SubHeaders().Select(x => new CADSubHeaderForLVS(x));
					}
					else
					{
						cADSubHeaders = declaration.Invoices.OfType<JobComInvoiceHeader>().Select(x => new CADSubHeader(x));
					}
				}
				return cADSubHeaders;
			}
		}
		IEnumerable<ICADSubHeader> cADSubHeaders;

		void CaculateDeclarationTotals()
		{
			totalPSTAndHST = ZDecimal.Zero;
			totalPSTCannabisAmount = ZDecimal.Zero;
			totalProvAlcoholTaxAmount = ZDecimal.Zero;
			totalProvTobaccoAmount = ZDecimal.Zero;
			totalDeclarationRelieved = ZDecimal.Zero;
			totalAmount = ZDecimal.Zero;
			totalCustomsDuties = ZDecimal.Zero;
			totalExciseDuties = ZDecimal.Zero;
			totalExciseTaxes = ZDecimal.Zero;
			totalGST = ZDecimal.Zero;
			totalAnti_Dumping = ZDecimal.Zero;
			totalCountervailing = ZDecimal.Zero;
			totalSurtaxes = ZDecimal.Zero;
			totalSafeguards = ZDecimal.Zero;
			totalInterest = ZDecimal.Zero;
			totalDutiesAndTaxesWithInterest = ZDecimal.Zero;
			totalDutiesAndTaxes = ZDecimal.Zero;

			foreach (var cadLine in ((ICADHeader)this).CADSubHeaders.SelectMany(x => x.CADLines))
			{
				totalValueForDuty += cadLine.ValueForDuty_Box78;
				totalPSTAndHST += cadLine.PSTAndHSTAmount_Box91;
				totalPSTCannabisAmount += cadLine.ProvincialCannabisExciseDuty_Box95;
				totalProvAlcoholTaxAmount += cadLine.ProvincialAlcoholTax_Box92;
				totalProvTobaccoAmount += cadLine.ProvincialTobaccoAmount_Box93;
				totalCustomsDuties += cadLine.CustomsDuty_Box82;
				totalExciseDuties += cadLine.ExciseDuty_Box84;
				totalExciseTaxes += cadLine.ExciseTax_Box83;
				totalGST += cadLine.GST_Box90;
				totalAnti_Dumping += cadLine.Anti_Dumping_Box86;
				totalSafeguards += cadLine.Safeguard_Box87;
				totalCountervailing += cadLine.Countervailing_Box88;
				totalSurtaxes += cadLine.Surtax_Box85;
				totalDutiesAndTaxes += cadLine.LineTotalDutiesAndTaxes_Box100;
				totalDutiesAndTaxesWithInterest += cadLine.LineTotalDutiesAndTaxes_Box100;
			}
			var deductionChargesOfLine = declaration.InvoiceLines.OfType<JobComInvoiceLine>().SelectMany(x => x.Charges.OfType<InvoiceLineCharge>()).Where(x => x.J7_ChargeType == CustomsChargeTypeList.Codes.DeductionCharge);
			if (deductionChargesOfLine.Any())
			{
				totalDeclarationRelieved = deductionChargesOfLine.Sum(x => x.J7_Amount);
			}
			else
			{
				totalDeclarationRelieved = declaration.Invoices.SelectMany(x => x.Charges).Where(x => x.J7_ChargeType == CustomsChargeTypeList.Codes.DeductionCharge).Sum(x => x.J7_Amount);
			}
			totalAmount = totalCustomsDuties + totalExciseDuties + totalExciseTaxes + totalGST + totalAnti_Dumping + totalCountervailing + totalSurtaxes + totalSafeguards;
		}

		ZDecimal totalValueForDuty;
		ZDecimal totalPSTAndHST;
		ZDecimal totalPSTCannabisAmount;
		ZDecimal totalProvAlcoholTaxAmount;
		ZDecimal totalProvTobaccoAmount;
		ZDecimal totalDeclarationRelieved;
		ZDecimal totalAmount;
		ZDecimal totalCustomsDuties;
		ZDecimal totalExciseDuties;
		ZDecimal totalExciseTaxes;
		ZDecimal totalGST;
		ZDecimal totalAnti_Dumping;
		ZDecimal totalCountervailing;
		ZDecimal totalSurtaxes;
		ZDecimal totalSafeguards;
		ZDecimal totalInterest;
		ZDecimal totalDutiesAndTaxesWithInterest;
		ZDecimal totalDutiesAndTaxes;

		class CADSubHeader : ICADSubHeader
		{
			public CADSubHeader(JobComInvoiceHeader invoiceHeader)
			{
				this.invoiceHeader = invoiceHeader;
			}
			readonly JobComInvoiceHeader invoiceHeader;

			ZString ICADSubHeader.VendorDetails_Box36
			{
				get
				{
					if (!vendorFormatted.HasValue)
					{
						vendorFormatted = Helper.GetPostalAddressAsASingleLine(invoiceHeader.Factory, ((IEDIInvoiceOGD)this.invoiceHeader).Vendor);
					}
					return vendorFormatted.Value;
				}
			}
			ZString? vendorFormatted;

			ZString ICADSubHeader.PurchaserDetails_Box37
			{
				get
				{
					if (!purchaserFormatted.HasValue)
					{
						purchaserFormatted = Helper.GetPostalAddressAsASingleLine(invoiceHeader.Factory, ((IEDIInvoiceOGD)this.invoiceHeader).Purchaser);
					}
					return purchaserFormatted.Value;
				}
			}
			ZString? purchaserFormatted;

			ZString ICADSubHeader.InvoiceNo_Box38 => invoiceHeader.JZ_InvoiceNumber;

			ZDecimal ICADSubHeader.InvoiceValue_Box39 => invoiceHeader.JZ_InvoiceAmount;

			ZString ICADSubHeader.InvoiceCurrencyCode_Box40 => invoiceHeader.JZ_RX_NKInvoice_Currency;

			ZString ICADSubHeader.PurchaseOrderNo_Box41 => ZString.Empty;

			ZDecimal ICADSubHeader.FreightCharges_Box42
			{
				get
				{
					var result = invoiceHeader.OverseasFreight.Amount;
					if (result == ZDecimal.Zero && invoiceHeader.IsUSCountryOfExport)
					{
						result = invoiceHeader.JobDeclaration?.CalculatedFreightAmount ?? ZDecimal.Zero;
					}
					return result;
				}
			}

			ZString ICADSubHeader.USPortOfExit_Box43 => invoiceHeader.CA_USPortOfExit;

			IEnumerable<ICADLine> ICADSubHeader.CADLines
			{
				get
				{
					if (cADLines == null)
					{
						cADLines = invoiceHeader.InvoiceLines.OfType<JobComInvoiceLine>().Select(x => new CADLine(x)).OrderBy(x => (x as ICADLine).CADLineNo_Box56);
					}
					return cADLines;
				}
			}
			IEnumerable<ICADLine> cADLines;

			B3AndCADDocumentHelper Helper => helper ?? (helper = new B3AndCADDocumentHelper());
			B3AndCADDocumentHelper helper;
		}

		class CADSubHeaderForLVS : ICADSubHeader
		{
			public CADSubHeaderForLVS(B3SubHeader b3SubHeader)
			{
				this.b3SubHeader = b3SubHeader;
				this.invoiceHeader = b3SubHeader.InvoiceHeader;
				this.lines = b3SubHeader.GetB3SubHeaderLines();
			}
			readonly IB3SubHeader b3SubHeader;
			readonly JobComInvoiceHeader invoiceHeader;
			readonly IEnumerable<IClassificationLine1> lines;

			ZString ICADSubHeader.VendorDetails_Box36
			{
				get
				{
					if (!vendorFormatted.HasValue)
					{
						vendorFormatted = Helper.GetPostalAddressAsASingleLine(invoiceHeader.Factory, b3SubHeader.Vendor);
					}
					return vendorFormatted.Value;
				}
			}
			ZString? vendorFormatted;

			ZString ICADSubHeader.PurchaserDetails_Box37
			{
				get
				{
					if (!purchaserFormatted.HasValue)
					{
						purchaserFormatted = Helper.GetPostalAddressAsASingleLine(invoiceHeader.Factory, ((IEDIInvoiceOGD)this.invoiceHeader).Purchaser);
					}
					return purchaserFormatted.Value;
				}
			}
			ZString? purchaserFormatted;

			ZString ICADSubHeader.InvoiceNo_Box38 => b3SubHeader.B3SubHeaderNumber.ToString();

			ZDecimal ICADSubHeader.InvoiceValue_Box39 => lines.Sum(x => x.ValueForCurrency);

			ZString ICADSubHeader.InvoiceCurrencyCode_Box40 => invoiceHeader.JZ_RX_NKInvoice_Currency;

			ZString ICADSubHeader.PurchaseOrderNo_Box41 => ZString.Empty;

			ZDecimal ICADSubHeader.FreightCharges_Box42 => b3SubHeader.FreightCharges;

			ZString ICADSubHeader.USPortOfExit_Box43 => invoiceHeader.CA_USPortOfExit;

			IEnumerable<ICADLine> ICADSubHeader.CADLines
			{
				get
				{
					if (cADLines == null)
					{
						cADLines = lines.Select(x => new CADLineForLVS(x)).OrderBy(x => (x as ICADLine).CADLineNo_Box56);
					}
					return cADLines;
				}
			}
			IEnumerable<ICADLine> cADLines;

			B3AndCADDocumentHelper Helper => helper ?? (helper = new B3AndCADDocumentHelper());
			B3AndCADDocumentHelper helper;
		}

		internal class CADLine : ICADLine
		{
			public CADLine(JobComInvoiceLine invoiceLine)
			{
				this.invoiceLine = invoiceLine;
				this.entryLine = invoiceLine.CusEntryLine;
				this.b3EntryLine = invoiceLine.B3EntryLine;
				this.invoiceHeader = invoiceLine.InvoiceHeader;
				CaculateDutiesAndTaxes();
			}
			readonly JobComInvoiceLine invoiceLine;
			readonly CusEntryLine entryLine;
			readonly CusEntryLine b3EntryLine;
			readonly JobComInvoiceHeader invoiceHeader;

			ZShort ICADLine.CADLineNo_Box56 => entryLine?.CL_LineNumber ?? ZShort.Zero;

			ZString ICADLine.PreviousLineNoWarehouse_Box57 => ZString.Empty;

			ZString ICADLine.ClassificationNo_Box58 => entryLine?.CL_AdValoremTariff ?? ZString.Empty;

			ZString ICADLine.ClassificationDescription_Box59 => invoiceLine.JI_Description;

			ZString ICADLine.NarrativeDescription_Box60
			{
				get
				{
					var builder = new ZStringBuilder();
					if (!invoiceLine.CA_TRSNumber.IsEmpty)
					{
						builder.Append($"TRS #:{invoiceLine.CA_TRSNumber}");
					}

					builder.AppendIfNotEmpty(!invoiceLine.JI_PartNo.IsEmpty ? invoiceLine.JI_PartNo : (entryLine?.Description ?? ZString.Empty));
					return builder.ToStringWithDelimiterBetweenAppends(";");
				}
			}

			ZDecimal ICADLine.Quantity_Box61
			{
				get
				{
					var result = ZDecimal.Zero;
					if (b3EntryLine is IClassificationLine1 classificationLine)
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

			ZString ICADLine.UnitOfMeasure_Box62 => b3EntryLine is IClassificationLine1 classificationLine && !classificationLine.CustomsQuantity.IsEmpty ? classificationLine.CustomsUnitQty : ZString.Empty;

			ZString ICADLine.TimeLimitType_Box63 => invoiceHeader.CA_TimeLimitCode;

			ZDateTime ICADLine.ExtensionDate_Box64 => ZDateTime.Empty;

			ZString ICADLine.CountryOfOrigin_Box65 => invoiceLine.JI_CountryOfOrigin;

			ZString ICADLine.USState_Box66 => invoiceLine.JI_StateOrRegionOfOrigin;

			ZString ICADLine.PlaceOfExport_Box67 => invoiceHeader.CA_RN_NKExport;

			ZString ICADLine.PlaceOfExportCodeState_Box68 => invoiceHeader.CA_USStateOfExport;

			ZDateTime ICADLine.DirectShipmentDate_Box69 => invoiceHeader.JZ_ValuationDateOverride;

			ZString ICADLine.TariffTreatment_Box70 => invoiceLine.CA_TreatmentCode;

			ZString ICADLine.TariffCode_Box71 => invoiceLine.CA_99TariffCode;

			ZString ICADLine.TimeLimitFrom_Box72 => DateTimeToDateString(TimeLimitRange.TimeLimitStart);

			ZString ICADLine.TimeLimitTo_Box73 => DateTimeToDateString(TimeLimitRange.TimeLimitEnd);

			(ZDateTime TimeLimitStart, ZDateTime TimeLimitEnd) TimeLimitRange => timeLimitRange ??= invoiceHeader.TimeLimitRange;
			(ZDateTime TimeLimitStart, ZDateTime TimeLimitEnd)? timeLimitRange;

			ZString ICADLine.DestinationProvince_Box74 => invoiceLine.CA_CasualImportDestinationProvince;

			ZDecimal ICADLine.ValueForCurrencyConversion_Box75 => invoiceLine.CA_CVforCurrConv;

			ZString ICADLine.Currency_Box76 => invoiceHeader.JZ_RX_NKInvoice_Currency;

			ZDecimal ICADLine.ExchangeRate_Box77 => invoiceHeader.JZ_InvoiceCurrExRate;

			ZDecimal ICADLine.ValueForDuty_Box78 => invoiceLine.CA_CustomsValue;

			ZString ICADLine.DRPLicense_Box79 => QueryAuthorityNumber(invoiceLine, RemissionTypeList.Codes.DutiesReliefProgramLicense);

			ZString ICADLine.SpecialAuthOIC_Box80 => QueryAuthorityNumber(invoiceLine, RemissionTypeList.Codes.OrderInCouncil);

			ZString ICADLine.SpecialAuthorityPermit_Box81 => QueryAuthorityNumber(invoiceLine, RemissionTypeList.Codes.Permit);

			ZDecimal ICADLine.CustomsDuty_Box82 => customsDuty;

			ZDecimal ICADLine.ExciseTax_Box83 => exciseTax;

			ZDecimal ICADLine.ExciseDuty_Box84 => exciseDuty;

			ZDecimal ICADLine.Surtax_Box85 => surtax;

			ZDecimal ICADLine.Anti_Dumping_Box86 => anti_Dumping;

			ZDecimal ICADLine.Safeguard_Box87 => safeguard;

			ZDecimal ICADLine.Countervailing_Box88 => countervailing;

			ZDecimal ICADLine.ValueForTax_Box89 => valueForTax;

			ZDecimal ICADLine.GST_Box90 => gST;

			ZDecimal ICADLine.PSTAndHSTAmount_Box91 => pSTAndHSTAmount;

			ZDecimal ICADLine.ProvincialAlcoholTax_Box92 => provincialAlcoholTax;

			ZDecimal ICADLine.ProvincialTobaccoAmount_Box93 => provincialTobaccoAmount;

			ZDecimal ICADLine.AlcohosPercent_Box94 => invoiceLine.AlcoholPercentByVolume;

			ZDecimal ICADLine.ProvincialCannabisExciseDuty_Box95 => provincialCannabisExciseDuty;

			ZString ICADLine.CBSACaseNo_Box96 => ZString.Empty;

			ZString ICADLine.RulingNo_Box97 => ZString.Empty;

			ZString ICADLine.AppealsCaseNo_Box98 => ZString.Empty;

			ZString ICADLine.ComplianceCaseNo_Box99 => ZString.Empty;

			ZDecimal ICADLine.LineTotalDutiesAndTaxes_Box100 => totalDutiesAndTaxes;

			ZString ICADLine.CommodityReason1_Box101 => AmendmentInfo[0].reason;

			ZString ICADLine.Authority1_Box102 => AmendmentInfo[0].appeal;

			ZString ICADLine.CommodityRemark1_Box103 => AmendmentInfo[0].description;

			ZString ICADLine.CommodityReason2_Box105 => AmendmentInfo[1].reason;

			ZString ICADLine.Authority2_Box106 => AmendmentInfo[1].appeal;

			ZString ICADLine.CommodityRemark2_Box107 => AmendmentInfo[1].description;

			ZString ICADLine.CommodityReason3_Box109 => AmendmentInfo[2].reason;

			ZString ICADLine.Authority3_Box110 => AmendmentInfo[2].appeal;

			ZString ICADLine.CommodityRemark3_Box111 => AmendmentInfo[2].description;

			(ZString reason, ZString appeal, ZString description)[] AmendmentInfo => amendmentInfo ??= QueryTopThreeAmendmentInfo(b3EntryLine);
			(ZString reason, ZString appeal, ZString description)[] amendmentInfo;

			void CaculateDutiesAndTaxes()
			{
				customsDuty = ZDecimal.Zero;
				exciseTax = ZDecimal.Zero;
				exciseDuty = ZDecimal.Zero;
				surtax = ZDecimal.Zero;
				anti_Dumping = ZDecimal.Zero;
				safeguard = ZDecimal.Zero;
				countervailing = ZDecimal.Zero;
				valueForTax = ZDecimal.Zero;
				gST = ZDecimal.Zero;
				pSTAndHSTAmount = ZDecimal.Zero;
				provincialAlcoholTax = ZDecimal.Zero;
				provincialTobaccoAmount = ZDecimal.Zero;
				provincialCannabisExciseDuty = ZDecimal.Zero;
				totalDutiesAndTaxes = ZDecimal.Zero;

				foreach (var dutyAndTax in invoiceLine.DutiesAndTaxes)
				{
					var amount = dutyAndTax.C1_Amount;
					totalDutiesAndTaxes += amount;
					switch (dutyAndTax.C1_TaxType)
					{
						case DutyAndTaxTypes.Codes.CPT:
							pSTAndHSTAmount += amount;
							break;
						case DutyAndTaxTypes.Codes.CTA:
							var dutyTypeForCAD = Helper.ConvertDutyAndTaxType(dutyAndTax.C1_TaxType, invoiceLine);
							switch (dutyTypeForCAD)
							{
								case CADDutyTaxFeeTypeCodes.Codes.PAT:
									provincialCannabisExciseDuty += amount;
									break;
								case CADDutyTaxFeeTypeCodes.Codes.TAC:
									provincialAlcoholTax += amount;
									break;
								case CADDutyTaxFeeTypeCodes.Codes.AAD:
									provincialTobaccoAmount += amount;
									break;
								default:
									break;
							}
							break;
						case DutyAndTaxTypes.Codes.CustomsDuty:
							if (dutyAndTax.IsEXDDuty)
							{
								exciseDuty += amount;
							}
							else
							{
								customsDuty += amount;
							}
							break;
						case DutyAndTaxTypes.Codes.ExciseTax:
							exciseTax += amount;
							break;
						case DutyAndTaxTypes.Codes.GST:
							gST += amount;
							break;
						case DutyAndTaxTypes.Codes.ADD:
							anti_Dumping += amount;
							break;
						case DutyAndTaxTypes.Codes.CVD:
							countervailing += amount;
							break;
						case DutyAndTaxTypes.Codes.SAF:
							safeguard += amount;
							break;
						case DutyAndTaxTypes.Codes.SUR:
							surtax += amount;
							break;
						default:
							break;
					}
				}
			}
			ZDecimal customsDuty;
			ZDecimal exciseTax;
			ZDecimal exciseDuty;
			ZDecimal surtax;
			ZDecimal anti_Dumping;
			ZDecimal safeguard;
			ZDecimal countervailing;
			ZDecimal valueForTax;
			ZDecimal gST;
			ZDecimal pSTAndHSTAmount;
			ZDecimal provincialAlcoholTax;
			ZDecimal provincialTobaccoAmount;
			ZDecimal provincialCannabisExciseDuty;
			ZDecimal totalDutiesAndTaxes;

			B3AndCADDocumentHelper Helper => helper ?? (helper = new B3AndCADDocumentHelper());
			B3AndCADDocumentHelper helper;
		}

		class CADLineForLVS : ICADLine
		{
			public CADLineForLVS(IClassificationLine1 classificationLine)
			{
				this.classificationLine = classificationLine;
				this.entryLine = classificationLine as CusEntryLine;
				if (entryLine != null)
				{
					this.invoiceLine = entryLine.RandomLine;
					this.invoiceHeader = invoiceLine.InvoiceHeader;
					this.invoiceLines = entryLine.InvoiceLines.OfType<JobComInvoiceLine>();
					CalculateDutiesAndTaxesEtc();
				}
				else
				{
					throw new InvalidOperationException("There is no valid Entry Line.");
				}
			}
			readonly IClassificationLine1 classificationLine;
			readonly JobComInvoiceLine invoiceLine;
			readonly JobComInvoiceHeader invoiceHeader;
			readonly IEnumerable<JobComInvoiceLine> invoiceLines;
			readonly CusEntryLine entryLine;

			ZShort ICADLine.CADLineNo_Box56 => classificationLine.B3LineNumber;

			ZString ICADLine.PreviousLineNoWarehouse_Box57 => ZString.Empty;

			ZString ICADLine.ClassificationNo_Box58 => classificationLine.ClassificationNumber;

			ZString ICADLine.ClassificationDescription_Box59 => invoiceLine.JI_Description;

			ZString ICADLine.NarrativeDescription_Box60 => Helper.GetDescriptionFromIClassificationLine1(classificationLine);

			ZDecimal ICADLine.Quantity_Box61
			{
				get
				{
					var result = ZDecimal.Zero;
					if (classificationLine.AuthorityNumber.IsEmpty)
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

			ZString ICADLine.UnitOfMeasure_Box62 => classificationLine.CustomsQuantity.IsEmpty || classificationLine.AuthorityNumber.IsEmpty ? string.Empty : classificationLine.CustomsUnitQty;

			ZString ICADLine.TimeLimitType_Box63 => invoiceHeader.CA_TimeLimitCode;

			ZDateTime ICADLine.ExtensionDate_Box64 => ZDateTime.Empty;

			ZString ICADLine.CountryOfOrigin_Box65 => invoiceLine.JI_CountryOfOrigin;

			ZString ICADLine.USState_Box66 => invoiceLine.JI_StateOrRegionOfOrigin;

			ZString ICADLine.PlaceOfExport_Box67 => invoiceHeader.CA_RN_NKExport;

			ZString ICADLine.PlaceOfExportCodeState_Box68 => invoiceHeader.CA_USStateOfExport;

			ZDateTime ICADLine.DirectShipmentDate_Box69 => invoiceHeader.JZ_ValuationDateOverride;

			ZString ICADLine.TariffTreatment_Box70 => invoiceLine.CA_TreatmentCode;

			ZString ICADLine.TariffCode_Box71 => invoiceLine.CA_99TariffCode;

			ZString ICADLine.TimeLimitFrom_Box72 => DateTimeToDateString(TimeLimitRange.TimeLimitStart);

			ZString ICADLine.TimeLimitTo_Box73 => DateTimeToDateString(TimeLimitRange.TimeLimitEnd);

			(ZDateTime TimeLimitStart, ZDateTime TimeLimitEnd) TimeLimitRange => timeLimitRange ??= invoiceHeader.TimeLimitRange;
			(ZDateTime TimeLimitStart, ZDateTime TimeLimitEnd)? timeLimitRange;

			ZString ICADLine.DestinationProvince_Box74 => invoiceLine.CA_CasualImportDestinationProvince;

			ZDecimal ICADLine.ValueForCurrencyConversion_Box75 => classificationLine.ValueForCurrency;

			ZString ICADLine.Currency_Box76 => invoiceHeader.JZ_RX_NKInvoice_Currency;

			ZDecimal ICADLine.ExchangeRate_Box77 => invoiceHeader.JZ_InvoiceCurrExRate;

			ZDecimal ICADLine.ValueForDuty_Box78 => classificationLine.ValueForDuty;

			ZString ICADLine.DRPLicense_Box79 => QueryAuthorityNumber(invoiceLine, RemissionTypeList.Codes.DutiesReliefProgramLicense);

			ZString ICADLine.SpecialAuthOIC_Box80 => QueryAuthorityNumber(invoiceLine, RemissionTypeList.Codes.OrderInCouncil);

			ZString ICADLine.SpecialAuthorityPermit_Box81 => QueryAuthorityNumber(invoiceLine, RemissionTypeList.Codes.Permit);

			ZDecimal ICADLine.CustomsDuty_Box82 => totalCustomsDuty;

			ZDecimal ICADLine.ExciseTax_Box83 => totalExciseTax;

			ZDecimal ICADLine.ExciseDuty_Box84 => totalExciseDuty;

			ZDecimal ICADLine.Surtax_Box85 => totalSurtax;

			ZDecimal ICADLine.Anti_Dumping_Box86 => totalAnti_Dumping;

			ZDecimal ICADLine.Safeguard_Box87 => totalSafeguard;

			ZDecimal ICADLine.Countervailing_Box88 => totalCountervailing;

			ZDecimal ICADLine.ValueForTax_Box89 => totalValueForTax;

			ZDecimal ICADLine.GST_Box90 => totalGST;

			ZDecimal ICADLine.PSTAndHSTAmount_Box91 => totalPSTAndHSTAmount;

			ZDecimal ICADLine.ProvincialAlcoholTax_Box92 => totalProvincialAlcoholTax;

			ZDecimal ICADLine.ProvincialTobaccoAmount_Box93 => totalProvincialTobaccoAmount;

			ZDecimal ICADLine.AlcohosPercent_Box94 => invoiceLine.AlcoholPercentByVolume;

			ZDecimal ICADLine.ProvincialCannabisExciseDuty_Box95 => totalProvincialCannabisExciseDuty;

			ZString ICADLine.CBSACaseNo_Box96 => ZString.Empty;

			ZString ICADLine.RulingNo_Box97 => ZString.Empty;

			ZString ICADLine.AppealsCaseNo_Box98 => ZString.Empty;

			ZString ICADLine.ComplianceCaseNo_Box99 => ZString.Empty;

			ZDecimal ICADLine.LineTotalDutiesAndTaxes_Box100 => totalTotalDutiesAndTaxes;

			ZString ICADLine.CommodityReason1_Box101 => AmendmentInfo[0].reason;

			ZString ICADLine.Authority1_Box102 => AmendmentInfo[0].appeal;

			ZString ICADLine.CommodityRemark1_Box103 => AmendmentInfo[0].description;

			ZString ICADLine.CommodityReason2_Box105 => AmendmentInfo[1].reason;

			ZString ICADLine.Authority2_Box106 => AmendmentInfo[1].appeal;

			ZString ICADLine.CommodityRemark2_Box107 => AmendmentInfo[1].description;

			ZString ICADLine.CommodityReason3_Box109 => AmendmentInfo[2].reason;

			ZString ICADLine.Authority3_Box110 => AmendmentInfo[2].appeal;

			ZString ICADLine.CommodityRemark3_Box111 => AmendmentInfo[2].description;

			(ZString reason, ZString appeal, ZString description)[] AmendmentInfo => amendmentInfo ??= QueryTopThreeAmendmentInfo(entryLine);
			(ZString reason, ZString appeal, ZString description)[] amendmentInfo;

			void CalculateDutiesAndTaxesEtc()
			{
				totalCustomsDuty = ZDecimal.Zero;
				totalExciseTax = ZDecimal.Zero;
				totalExciseDuty = ZDecimal.Zero;
				totalSurtax = ZDecimal.Zero;
				totalAnti_Dumping = ZDecimal.Zero;
				totalSafeguard = ZDecimal.Zero;
				totalCountervailing = ZDecimal.Zero;
				totalValueForTax = ZDecimal.Zero;
				totalGST = ZDecimal.Zero;
				totalPSTAndHSTAmount = ZDecimal.Zero;
				totalProvincialAlcoholTax = ZDecimal.Zero;
				totalProvincialTobaccoAmount = ZDecimal.Zero;
				totalProvincialCannabisExciseDuty = ZDecimal.Zero;
				totalTotalDutiesAndTaxes = ZDecimal.Zero;

				foreach (var line in invoiceLines)
				{
					foreach (var dutyAndTax in line.DutiesAndTaxes)
					{
						var amount = dutyAndTax.C1_Amount;
						totalTotalDutiesAndTaxes += amount;
						switch (dutyAndTax.C1_TaxType)
						{
							case DutyAndTaxTypes.Codes.CPT:
								totalPSTAndHSTAmount += amount;
								break;
							case DutyAndTaxTypes.Codes.CTA:
								var dutyTypeForCAD = Helper.ConvertDutyAndTaxType(dutyAndTax.C1_TaxType, invoiceLine);
								switch (dutyTypeForCAD)
								{
									case CADDutyTaxFeeTypeCodes.Codes.PAT:
										totalProvincialCannabisExciseDuty += amount;
										break;
									case CADDutyTaxFeeTypeCodes.Codes.TAC:
										totalProvincialAlcoholTax += amount;
										break;
									case CADDutyTaxFeeTypeCodes.Codes.AAD:
										totalProvincialTobaccoAmount += amount;
										break;
									default:
										break;
								}
								break;
							case DutyAndTaxTypes.Codes.CustomsDuty:
								if (dutyAndTax.IsEXDDuty)
								{
									totalExciseDuty += amount;
								}
								else
								{
									totalCustomsDuty += amount;
								}
								break;
							case DutyAndTaxTypes.Codes.ExciseTax:
								totalExciseTax += amount;
								break;
							case DutyAndTaxTypes.Codes.GST:
								totalGST += amount;
								break;
							case DutyAndTaxTypes.Codes.ADD:
								totalAnti_Dumping += amount;
								break;
							case DutyAndTaxTypes.Codes.CVD:
								totalCountervailing += amount;
								break;
							case DutyAndTaxTypes.Codes.SAF:
								totalSafeguard += amount;
								break;
							case DutyAndTaxTypes.Codes.SUR:
								totalSurtax += amount;
								break;
							default:
								break;
						}
					}
				}
			}
			ZDecimal totalCustomsDuty;
			ZDecimal totalExciseTax;
			ZDecimal totalExciseDuty;
			ZDecimal totalSurtax;
			ZDecimal totalAnti_Dumping;
			ZDecimal totalSafeguard;
			ZDecimal totalCountervailing;
			ZDecimal totalValueForTax;
			ZDecimal totalGST;
			ZDecimal totalPSTAndHSTAmount;
			ZDecimal totalProvincialAlcoholTax;
			ZDecimal totalProvincialTobaccoAmount;
			ZDecimal totalProvincialCannabisExciseDuty;
			ZDecimal totalTotalDutiesAndTaxes;

			B3AndCADDocumentHelper Helper => helper ?? (helper = new B3AndCADDocumentHelper());
			B3AndCADDocumentHelper helper;
		}

		public static (ZString reason, ZString appeal, ZString description)[] QueryTopThreeAmendmentInfo(CusEntryLine b3EntryLine)
		{
			if (b3EntryLine == null || b3EntryLine.AmendmentDetails == null || !b3EntryLine.AmendmentDetails.Any())
			{
				return new[]
				{
						(ZString.Empty, ZString.Empty, ZString.Empty),
						(ZString.Empty, ZString.Empty, ZString.Empty),
						(ZString.Empty, ZString.Empty, ZString.Empty)
					};
			}
			var requireCount = 3;
			var actions = b3EntryLine.AmendmentDetails.Cast<CADCorrectionMessageSendingAction>();
			var maxLineNO = actions.Max(x => x.CSI_LineNo);
			return actions.Where(w => w.CSI_LineNo == maxLineNO)
				.GroupBy(action => new { action.CSI_Code, action.CSI_SubType })
				.Select(g =>
				{
					var action = g.First();
					return (
						new ZString($"{action.CSI_Code} {action.ReasonCodeDescription}"),
						new ZString($"{action.CSI_SubType} {action.AppealsProgramCodeDescription}"),
						action.CSI_Description
					);
				})
				.Concat(Enumerable.Repeat((ZString.Empty, ZString.Empty, ZString.Empty), requireCount))
				.Take(requireCount)
				.ToArray();
		}

		static ZString DateTimeToDateString(ZDateTime dateTime) => dateTime.IsEmpty ? ZString.Empty : dateTime.ToString("yyyyMMdd");

		static ZString QueryAuthorityNumber(JobComInvoiceLine invoiceLine, string targetCode) => invoiceLine.CA_RemissionType == targetCode ? invoiceLine.CA_AuthorityNumber : ZString.Empty;
	}
}

