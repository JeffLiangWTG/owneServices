using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Customs.NZ.FormalEntry
{
	public class DocCusEntryLine : DocBaseCusEntryLine
	{
		#region Static and Normal Constructors
		DocCusEntryLine(CusEntryLine cusEntryLine, BusinessObjectFactory factoryToWrap)
			: base(cusEntryLine, factoryToWrap)
		{
			SetupFlattenedListProperties();
		}

		protected DocCusEntryLine(BusinessObjectFactory factoryToWrap)
			: base(null, factoryToWrap)
		{
		}

		public static DocCusEntryLine New(BusinessObjectFactory factoryToWrap)
		{
			return new DocCusEntryLine(factoryToWrap);
		}

		public static DocCusEntryLine New(CusEntryLine cusEntryLine, BusinessObjectFactory factoryToWrap)
		{
			if (cusEntryLine == null)
			{
				return null;
			}
			else
			{
				return new DocCusEntryLine(cusEntryLine, factoryToWrap);
			}
		}
		#endregion

		#region InvoiceLine Implementation

		protected override DocBaseJobComInvoiceLine CreateJobComInvoiceLine(Enterprise.Customs.Business.BaseJobComInvoiceLine invoiceLineToWrap)
		{
			return DocJobComInvoiceLine.New((JobComInvoiceLine)invoiceLineToWrap, Factory);
		}

		public DocJobComInvoiceLine InvoiceLine
		{
			get { return (DocJobComInvoiceLine)InvoiceLineInternal; }
		}

		#endregion

		#region EntryLine Implementation
		CusEntryLine EntryLine
		{
			get { return (CusEntryLine)WrappedObject; }
		}

		protected bool IsExport
		{
			get { return EntryLine != null && EntryLine.Declaration != null && EntryLine.Declaration.IsExport; }
		}
		#endregion

		#region Common Mapped Fields
		public ZString DetailLineNo
		{
			get { return EntryLine == null ? "" : EntryLine.CL_LineNumber.ToString(); }
		}

		public ZString DescriptionOfGoods
		{
			get { return EntryLine == null ? ZString.Empty : EntryLine.Description; }
		}

		public ZString ProductName
		{
			get
			{
				ZString result = ZString.Empty;
				if (EntryLine != null && EntryLine.RandomLine.Part != null)
				{
					result = EntryLine.RandomLine.Part.OP_Desc;
				}

				return result;
			}
		}

		public ZString ProductBrandName
		{
			get
			{
				ZString result = ZString.Empty;
				if (EntryLine != null && EntryLine.RandomLine.Part != null)
				{
					result = EntryLine.RandomLine.Part.OP_Brand;
				}

				return result;
			}
		}

		public ZString TariffItem
		{
			get { return EntryLine == null ? ZString.Empty : EntryLine.CL_AdValoremTariff; }
		}

		public ZString ConcessionCode
		{
			get { return EntryLine == null ? ZString.Empty : EntryLine.ConcessionCode; }
		}

		public ZString RelationshipIndicator
		{
			get { return EntryLine == null ? ZString.Empty : EntryLine.RelationshipIndicator; }
		}

		public ZString PreferenceIndicator
		{
			get { return EntryLine == null ? ZString.Empty : EntryLine.PreferentialDutyIndicator; }
		}

		public DocCountry CountryOfExport
		{
			get { return EntryLine == null ? null : DocCountry.New(Factory, EntryLine.CountryOfExport); }
		}

		public DocCountry CountryOfOrigin
		{
			get { return EntryLine == null ? null : DocCountry.New(Factory, EntryLine.CountryOfOrigin); }
		}

		public ZString SupplierCode
		{
			get
			{
				if (EntryLine != null)
				{
					OrgHeader supplier = EntryLine.Supplier;
					if (supplier != null && !supplier.IsMiscellaneous)
					{
						return supplier.LocalCustomsSupplierCode;
					}
				}
				return ZString.Empty;
			}
		}

		public ZString SupplierName
		{
			get { return EntryLine != null ? EntryLine.SupplierName : ZString.Empty; }
		}

		public ZString StatisticalUnit
		{
			get { return EntryLine == null ? ZString.Empty : EntryLine.StatisticalUnit; }
		}

		public ZString StatisticalQuantity
		{
			get { return EntryLine == null ? "" : (EntryLine.StatisticalQty.IsEmpty ? "" : EntryLine.StatisticalQty.ToString(JobComInvoiceLine.CustomsQtyMaxDecimalPlaces)); }
		}

		public ZString SupplementaryUnit
		{
			get { return EntryLine == null ? ZString.Empty : EntryLine.SupplementaryUQ; }
		}

		public ZString SupplementaryQuantity
		{
			get { return EntryLine == null ? "" : (EntryLine.SupplementaryQty.IsEmpty ? "" : EntryLine.SupplementaryQty.ToString(JobComInvoiceLine.CustomsQtyMaxDecimalPlaces)); }
		}

		public ZString VFDForeign
		{
			get
			{
				var result = ZString.Empty;
				if (EntryLine != null)
				{
					if (EntryLine.Declaration.Invoices.Count == 1)
					{
						result = EntryLine.OSCustomsValue.ToString(2);
					}
					else
					{
						var accumulatedFOB = ZDecimal.Zero;
						foreach (JobComInvoiceLine invoiceLine in EntryLine.InvoiceLines)
						{
							accumulatedFOB += invoiceLine.JI_Calc_FOB;
						}

						result = accumulatedFOB.ToString(2);
					}
				}

				return result;
			}
		}

		public ZString CurrencyCode
		{
			get { return EntryLine == null ? ZString.Empty : EntryLine.OSCurrencyCode; }
		}

		public ZString VFDWholeNZ
		{
			get { return EntryLine == null ? "" : EntryLine.VFDWholeNZD.ToString(0); }
		}

		public ZString ExchangeRate
		{
			get { return EntryLine == null ? "" : EntryLine.ExchangeRate.ToString(2); }
		}

		public ZString ExchangeRateIndicator
		{
			get { return EntryLine == null ? "" : new ExchangeRateIndicatorList().GetDescriptionFromCode(EntryLine.ExchangeRateIndicator); }
		}

		public ZString InsuranceWholeNZ
		{
			get { return EntryLine == null ? "" : EntryLine.InsuranceWholeNZD.ToString(0); }
		}

		public ZString FreightWholeNZ
		{
			get { return EntryLine == null ? "" : EntryLine.FreightWholeNZD.ToString(0); }
		}

		public ZString MiscReasonCode
		{
			get { return MiscDetails.MiscReasonCode; }
		}

		public ZString MiscAmountNZ
		{
			get { return MiscDetails.MiscAmountNZ.IsEmpty ? "" : MiscDetails.MiscAmountNZ.ToString(2); }
		}

		#region MiscDetails
		MiscDetailsClass MiscDetails
		{
			get
			{
				if (fMiscDetails == null)
				{
					fMiscDetails = new MiscDetailsClass(EntryLine);
				}

				return fMiscDetails;
			}
		}
		MiscDetailsClass fMiscDetails;

		class MiscDetailsClass
		{
			public MiscDetailsClass(CusEntryLine entryLine)
			{
				MiscReasonCode = "";
				MiscAmountNZ = 0m;
				if (entryLine != null)
				{
					if (!entryLine.ALACLevyAmount.IsEmpty)
					{
						MiscReasonCode = "ALAC";
						MiscAmountNZ = entryLine.ALACLevyAmount;
					}
					else if (!entryLine.ALACLevyCreditAmount.IsEmpty)
					{
						MiscReasonCode = "ALAC";
						MiscAmountNZ = entryLine.ALACLevyCreditAmount;
					}
					else if (!entryLine.HERALevyAmount.IsEmpty)
					{
						MiscReasonCode = "HERA";
						MiscAmountNZ = entryLine.HERALevyAmount;
					}
					else if (!entryLine.ACCFuelLevyAmount.IsEmpty)
					{
						MiscReasonCode = "ACC FUEL";
						MiscAmountNZ = entryLine.ACCFuelLevyAmount;
					}
					else if (!entryLine.SyntheticGreenhouseGasesLevyAmount.IsEmpty)
					{
						MiscReasonCode = "SGG";
						MiscAmountNZ = entryLine.SyntheticGreenhouseGasesLevyAmount;
					}
					else if (!entryLine.AntiDumpingDutyAmount.IsEmpty)
					{
						MiscReasonCode = "ANTI DUMP";
						MiscAmountNZ = entryLine.AntiDumpingDutyAmount;
					}
					else if (!entryLine.CountervailingDutyAmount.IsEmpty)
					{
						MiscReasonCode = "COUNTERVAIL";
						MiscAmountNZ = entryLine.CountervailingDutyAmount;
					}
				}
			}
			public readonly ZString MiscReasonCode;
			public readonly ZDecimal MiscAmountNZ;
		}
		#endregion

		public ZString DutyPayableNZ
		{
			get { return EntryLine == null ? "" : IsExport ? EntryLine.DutyCreditAmount.ToString(2) : EntryLine.DutyAmount.ToString(2); }
		}

		public ZString GSTPayableNZ
		{
			get { return EntryLine == null ? "" : IsExport ? EntryLine.GSTCreditAmount.ToString(2) : EntryLine.GSTAmount.ToString(2); }
		}

		public ZString TotalPayableNZ
		{
			get { return EntryLine == null ? "" : EntryLine.TotalAmountPayable.ToString(2); }
		}

		public new ZString LineNumber
		{
			get { return EntryLine == null ? "" : base.LineNumber.ToString(); }
		}

		public ZShort LineNumberAsShort
		{
			get { return EntryLine == null ? ZShort.Zero : base.LineNumber; }
		}

		#endregion

		#region Flattened List Properties used on Entry Print as Documents don't handle indexers

		void SetupFlattenedListProperties()
		{
			SetupFlattenedPermitProperties();
			SetupFlattenedProhibitedProperties();
			SetupFlattenedOtherInfoProperties();
		}

		#region SetupFlattenedProhibitedProperties()
		void SetupFlattenedProhibitedProperties()
		{
			if (EntryLine.ProhibitedCodes.Count > 0)
			{
				fProhibitedCode1 = EntryLine.ProhibitedCodes[0].ZO_Code;
			}
			if (EntryLine.ProhibitedCodes.Count > 1)
			{
				fProhibitedCode2 = EntryLine.ProhibitedCodes[1].ZO_Code;
			}
			if (EntryLine.ProhibitedCodes.Count > 2)
			{
				fProhibitedCode3 = EntryLine.ProhibitedCodes[2].ZO_Code;
			}
		}

		public ZString ProhibitedCode1
		{
			get { return fProhibitedCode1; }
		}
		ZString fProhibitedCode1;

		public ZString ProhibitedCode2
		{
			get { return fProhibitedCode2; }
		}
		ZString fProhibitedCode2;

		public ZString ProhibitedCode3
		{
			get { return fProhibitedCode3; }
		}
		ZString fProhibitedCode3;

		#endregion

		#region OtherInfo

		void SetupFlattenedOtherInfoProperties()
		{
			if (EntryLine.OtherInfos.Count > 0)
			{
				fOtherInfoCode1 = EntryLine.OtherInfos[0].ZO_Code;
				fOtherInfoData1 = EntryLine.OtherInfos[0].ZO_Data;
			}
			if (EntryLine.OtherInfos.Count > 1)
			{
				fOtherInfoCode2 = EntryLine.OtherInfos[1].ZO_Code;
				fOtherInfoData2 = EntryLine.OtherInfos[1].ZO_Data;
			}
			if (EntryLine.OtherInfos.Count > 2)
			{
				fOtherInfoCode3 = EntryLine.OtherInfos[2].ZO_Code;
				fOtherInfoData3 = EntryLine.OtherInfos[2].ZO_Data;
			}
		}

		public ZString OtherInfoCode1
		{
			get { return fOtherInfoCode1; }
		}
		ZString fOtherInfoCode1;

		public ZString OtherInfoData1
		{
			get { return fOtherInfoData1; }
		}
		ZString fOtherInfoData1;

		public ZString OtherInfoCode2
		{
			get { return fOtherInfoCode2; }
		}
		ZString fOtherInfoCode2;

		public ZString OtherInfoData2
		{
			get { return fOtherInfoData2; }
		}
		ZString fOtherInfoData2;

		public ZString OtherInfoCode3
		{
			get { return fOtherInfoCode3; }
		}
		ZString fOtherInfoCode3;

		public ZString OtherInfoData3
		{
			get { return fOtherInfoData3; }
		}
		ZString fOtherInfoData3;
		#endregion

		#region Permit Information

		void SetupFlattenedPermitProperties()
		{
			if (EntryLine.PermitCodes.Count > 0)
			{
				fPermitCode1 = EntryLine.PermitCodes[0].ZO_Code;
				fPermitNumber1 = EntryLine.PermitCodes[0].ZO_Data;
			}
			if (EntryLine.PermitCodes.Count > 1)
			{
				fPermitCode2 = EntryLine.PermitCodes[1].ZO_Code;
				fPermitNumber2 = EntryLine.PermitCodes[1].ZO_Data;
			}
			if (EntryLine.PermitCodes.Count > 2)
			{
				fPermitCode3 = EntryLine.PermitCodes[2].ZO_Code;
				fPermitNumber3 = EntryLine.PermitCodes[2].ZO_Data;
			}
		}

		public ZString PermitCode1
		{
			get { return fPermitCode1; }
		}
		ZString fPermitCode1;

		public ZString PermitCode2
		{
			get { return fPermitCode2; }
		}
		ZString fPermitCode2;

		public ZString PermitCode3
		{
			get { return fPermitCode3; }
		}
		ZString fPermitCode3;

		public ZString PermitNumber1
		{
			get { return fPermitNumber1; }
		}
		ZString fPermitNumber1;

		public ZString PermitNumber2
		{
			get { return fPermitNumber2; }
		}
		ZString fPermitNumber2;

		public ZString PermitNumber3
		{
			get { return fPermitNumber3; }
		}
		ZString fPermitNumber3;
		#endregion

		#endregion

		#region New Properties for Customs Certificate
		public ZString CIFWholeNZ
		{
			get { return EntryLine == null ? "" : new ZDecimal(EntryLine.VFDWholeNZD + EntryLine.FreightWholeNZD + EntryLine.InsuranceWholeNZD).ToString(0); }
		}

		public ZString LevyAmount
		{
			get { return EntryLine == null ? "" : MiscDetails.MiscAmountNZ.ToString(2); }
		}

		public ZString TotalInvoiceLinesValueAndCurrency
		{
			get { return EntryLine == null ? "" : (EntryLine.TotalLinePrice.Currency == null ? "" : EntryLine.TotalLinePrice.Amount.ToString(EntryLine.TotalLinePrice.Currency.Decimals) + " " + EntryLine.TotalLinePrice.Currency.Code); }
		}

		public ZString StatQtyAndUnitIncludingDescription
		{
			get { return EntryLine == null ? "" : EntryLine.StatisticalQty.IsEmpty ? "" : "Stat Qty: " + EntryLine.StatisticalQty.ToString(JobComInvoiceLine.CustomsQtyMaxDecimalPlaces) + " " + EntryLine.StatisticalUnit; }
		}

		public ZString SuppQtyAndUnitIncludingDescription
		{
			get { return EntryLine == null ? "" : EntryLine.SupplementaryQty.IsEmpty ? "" : "Supp Qty: " + EntryLine.SupplementaryQty.ToString(JobComInvoiceLine.CustomsQtyMaxDecimalPlaces) + " " + EntryLine.SupplementaryUQ; }
		}

		public ZString DutyAndLevyRate
		{
			get { return EntryLine != null && !IsExport ? EntryLine.DutyRateDescription : ZString.Empty; }
		}

		public ZString DutyAndLevyRateIncludingDescription
		{
			get
			{
				ZString dutyAndLevyRate = this.DutyAndLevyRate;
				return dutyAndLevyRate.IsEmpty || dutyAndLevyRate == DutyCalculator.FreeDutyRateDescription ? "" : "Duty Rate: " + dutyAndLevyRate;
			}
		}

		#endregion

		#region Modifiable fields

		public ZString HumanConsumption
		{
			get { return ZString.Empty; }
		}

		public ZString PersonalConsumption
		{
			get { return ZString.Empty; }
		}

		public ZString TradeSample
		{
			get { return ZString.Empty; }
		}

		public ZString OtherUse
		{
			get { return ZString.Empty; }
		}

		public ZString SpecifyOtherUse
		{
			get { return ZString.Empty; }
		}

		public ZString ProductClassDeclaration
		{
			get { return ZString.Empty; }
		}

		public ZString BatchLotID
		{
			get { return ZString.Empty; }
		}

		public ZString CertificateNo
		{
			get { return ZString.Empty; }
		}

		public ZString GovtCertificate
		{
			get { return ZString.Empty; }
		}

		public ZString RecognisedAssurance
		{
			get { return ZString.Empty; }
		}

		public ZString SupportingDocOther
		{
			get { return ZString.Empty; }
		}

		public ZString SuppliersInvoice
		{
			get { return ZString.Empty; }
		}

		public ZString BillOfLading
		{
			get { return ZString.Empty; }
		}

		public ZString SupportingDocOtherUse
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region MPI Doc additional fields

		const string CheckBoxTicked = "\u2713";

		public ZString MPIHumanConsumption
		{
			get
			{
				ZString result = ZString.Empty;
				if (EntryLine != null && EntryLine.RandomLine.JI_IntendedUseCode == IntendedUseCodeList.Codes.HC)
				{
					result = CheckBoxTicked;
				}

				return result;
			}
		}

		public ZString MPIPersonalConsumption
		{
			get
			{
				ZString result = ZString.Empty;
				if (EntryLine != null && EntryLine.RandomLine.JI_IntendedUseCode == IntendedUseCodeList.Codes.PU)
				{
					result = CheckBoxTicked;
				}

				return result;
			}
		}

		public ZString MPITradeSample
		{
			get
			{
				ZString result = ZString.Empty;
				if (EntryLine != null && EntryLine.RandomLine.JI_IntendedUseCode == IntendedUseCodeList.Codes.TS)
				{
					result = CheckBoxTicked;
				}

				return result;
			}
		}

		public ZString MPILaboratoryAnalysis
		{
			get
			{
				ZString result = ZString.Empty;
				if (EntryLine != null && EntryLine.RandomLine.JI_IntendedUseCode == IntendedUseCodeList.Codes.LA)
				{
					result = CheckBoxTicked;
				}

				return result;
			}
		}

		public ZString MPIImportForReExport
		{
			get
			{
				ZString result = ZString.Empty;
				if (EntryLine != null && EntryLine.RandomLine.JI_IntendedUseCode == IntendedUseCodeList.Codes.RE)
				{
					result = CheckBoxTicked;
				}

				return result;
			}
		}

		public ZString MPIOtherUse
		{
			get
			{
				ZString result = ZString.Empty;
				if (EntryLine != null && !EntryLine.RandomLine.JI_IntendedUse.IsEmpty)
				{
					result = CheckBoxTicked;
				}

				return result;
			}
		}

		public ZString MPISpecifyOtherUse
		{
			get
			{
				ZString result = ZString.Empty;
				if (EntryLine != null)
				{
					result = EntryLine.RandomLine.JI_IntendedUse;
				}

				return result;
			}
		}

		public ZString MPIBrandName
		{
			get
			{
				ZString result = ZString.Empty;
				if (EntryLine != null)
				{
					result = EntryLine.RandomLine.JI_BrandName;
				}

				return result;
			}
		}

		public ZString MPILotIdentification
		{
			get
			{
				ZString result = ZString.Empty;
				if (EntryLine != null)
				{
					result = EntryLine.RandomLine.JI_LotNumber;
				}

				return result;
			}
		}

		public ZString MPINetWeight
		{
			get
			{
				ZString result = ZString.Empty;
				ZDecimal weightInKG = ZDecimal.Zero;
				if (EntryLine != null)
				{
					weightInKG = EntryLine.EffectiveNetWeight.InKilogramsSafe;
					if (weightInKG > 0)
					{
						result = weightInKG.ToString() + " kg";
					}
				}

				return result;
			}
		}

		public ZString MPIGrowerManufacturerName
		{
			get
			{
				ZString result = ZString.Empty;
				if (EntryLine != null)
				{
					if (!EntryLine.RandomLine.GrowerOrgPK.IsEmpty)
					{
						var grower = EntryLine.Factory.Load<OrgHeader>(EntryLine.RandomLine.GrowerOrgPK);
						if (grower != null)
						{
							result = grower.OH_FullName;
						}
					}
					else if (!EntryLine.RandomLine.ManufacturerOrgPK.IsEmpty)
					{
						var manufacturer = EntryLine.Factory.Load<OrgHeader>(EntryLine.RandomLine.ManufacturerOrgPK);
						if (manufacturer != null)
						{
							result = manufacturer.OH_FullName;
						}
					}
				}

				return result;
			}
		}

		#region MPI Modifiable fields

		public ZInt MPIPackageQty
		{
			get
			{
				ZInt result = ZInt.Zero;
				if (EntryLine != null)
				{
					result = EntryLine.RandomLine.NumberOfPackages1;
				}

				return result;
			}
		}

		public ZString MPIPackageUQ
		{
			get
			{
				ZString result = ZString.Empty;
				if (EntryLine != null)
				{
					result = EntryLine.RandomLine.Packages1UQ;
				}

				return result;
			}
		}

		public ZString MPINumberOfLots
		{
			get
			{
				return ZString.Empty;
			}
		}

		#endregion

		#endregion
	}
}
