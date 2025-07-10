using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAddInfoInvLineValidation : AUAddInfoLineValidation
	{
		public const string InstrumentTypeAndNoNumberMessageError = "You have entered an instrument type, but not the number.";
		public const string InstrumentNumberAndNoTypeMessageError = "You have entered an instrument number, but not the type.";

		public CMRAddInfoInvLineValidation(AUAddInfo parent)
			: base(parent)
		{
		}

		public const string EnterAWarehouseMessageError = "This line is on a Nature 20/30 or WEA entry. Please enter a warehouse and its address either on this line or the declaration.";
		public const string NoCCPMessageError = "The bonded warehouse address doesn't have a warehouse code (CCP) entered.";
		public const string WarehouseNotNeededMessageError = "Warehouse is only required for Nature 20, Nature 30 and Warehoused by External Agent invoice lines.";
		public const string MultipleWarehousesNotSupported = "You have entered multiple warehouses on this declaration. This is not supported, please make a separate declaration for each warehouse.";

		protected override void CheckZA_OA_WarehouseAddress_Hidden()
		{
			base.CheckZA_OA_WarehouseAddress_Hidden();
			var declaration = JobDeclaration;
			if (declaration != null && declaration.IsWHSUniversalXMLActive)
			{
				if (declaration.IsExWarehouse)
				{
					var invoiceLinesWithDifferentWarehouseAddress = declaration.GetInvoiceLinesMarkedForBondedWarehousingWithDifferentWarehouseAddressToDeclaration();
					if (invoiceLinesWithDifferentWarehouseAddress.Contains(InvoiceLine))
					{
						AddInfo.ZA_OA_WarehouseAddress_HiddenInfo.AddMessageError(JobDeclaration.WarehouseAddressIsDifferentToDeclarationLevelForBondedWarehousing);
					}

					var warehouseAddress = AddInfo.WarehouseAddress;
					if (warehouseAddress != null && warehouseAddress.LocalControlledPremisesID.IsEmpty)
					{
						AddInfo.ZA_OA_WarehouseAddress_HiddenInfo.AddMessageError(NoCCPMessageError);
					}
				}
			}
			else
			{
				var needWarehouse = InvoiceLine.JI_IsPackToBondForLine || declaration.IsNature30;

				if (needWarehouse && AddInfo.WarehouseAddress == null && declaration.WarehouseAddress == null)
				{
					AddInfo.ZA_OA_WarehouseAddress_HiddenInfo.AddMessageError(EnterAWarehouseMessageError);
				}
				else if (needWarehouse && AddInfo.WarehouseAddress != null && AddInfo.WarehouseAddress.LocalControlledPremisesID.IsEmpty)
				{
					AddInfo.ZA_OA_WarehouseAddress_HiddenInfo.AddMessageError(NoCCPMessageError);
				}
				else if (!needWarehouse && AddInfo.WarehouseAddress != null)
				{
					AddInfo.ZA_OA_WarehouseAddress_HiddenInfo.AddMessageError(WarehouseNotNeededMessageError);
				}
				else if (InvoiceLine.JI_IsPackToBondForLine && declaration.HasMultipleWarehouses)
				{
					AddInfo.ZA_OA_WarehouseAddress_HiddenInfo.AddMessageError(MultipleWarehousesNotSupported);
				}
			}
		}

		#region Public methods for Calculated fields

		protected override void CheckTCI_InstrumentType()
		{
			base.CheckTCI_InstrumentType();
			CheckAddInfoInstrumentType(AddInfo.TCI_InstrumentTypeInfo, AddInfo.TCI_InstrumentNoInfo);

			ListValidation.MessageErrorIfInvalidCode(AddInfo.TCI_InstrumentTypeInfo, AddInfo.Lookups.ZA_InstrumentType_List);

			ValidateTCI_InstrumentNo();
		}

		protected override void CheckTCI_InstrumentNo()
		{
			base.CheckTI2_InstrumentNo();
			CheckAddInfoInstrumentNumber(AddInfo.TCI_InstrumentTypeInfo, AddInfo.TCI_InstrumentNoInfo);

			ValidateTCI_InstrumentType();
		}

		protected override void CheckPRI_InstrumentType()
		{
			base.CheckPRI_InstrumentType();
			CheckAddInfoInstrumentType(AddInfo.PRI_InstrumentTypeInfo, AddInfo.PRI_InstrumentNoInfo);
			ListValidation.MessageErrorIfInvalidCode(AddInfo.PRI_InstrumentTypeInfo, AddInfo.Lookups.ZA_InstrumentType_List);

			ValidatePRI_InstrumentNo();
		}

		protected override void CheckPRI_InstrumentNo()
		{
			base.CheckPRI_InstrumentNo();
			CheckAddInfoInstrumentNumber(AddInfo.PRI_InstrumentTypeInfo, AddInfo.PRI_InstrumentNoInfo);

			ValidatePRI_InstrumentType();
		}

		protected override void CheckTI2_InstrumentType()
		{
			base.CheckTI2_InstrumentType();
			CheckAddInfoInstrumentType(AddInfo.TI2_InstrumentTypeInfo, AddInfo.TI2_InstrumentNoInfo);

			ListValidation.MessageErrorIfInvalidCode(AddInfo.TI2_InstrumentTypeInfo, AddInfo.Lookups.ZA_InstrumentType_List);

			ValidateTI2_InstrumentNo();
		}

		protected override void CheckTI2_InstrumentNo()
		{
			base.CheckTI2_InstrumentNo();
			CheckAddInfoInstrumentNumber(AddInfo.TI2_InstrumentTypeInfo, AddInfo.TI2_InstrumentNoInfo);

			ValidateTI2_InstrumentType();
		}

		protected void CheckAddInfoInstrumentType(ZPropertyInfo typeInfo, ZPropertyInfo numberInfo)
		{
			ZString type = (ZString)typeInfo.Value;
			ZString number = (ZString)numberInfo.Value;

			if (!type.IsEmpty && number.IsEmpty)
			{
				typeInfo.AddMessageError(InstrumentTypeAndNoNumberMessageError);
			}
		}

		protected void CheckAddInfoInstrumentNumber(ZPropertyInfo typeInfo, ZPropertyInfo numberInfo)
		{
			ZString type = (ZString)typeInfo.Value;
			ZString number = (ZString)numberInfo.Value;

			if (type.IsEmpty && !number.IsEmpty)
			{
				numberInfo.AddMessageError(InstrumentNumberAndNoTypeMessageError);
			}
		}

		#endregion

		protected override void CheckZA_TreatmentCode_Hidden()
		{
			base.CheckZA_TreatmentCode_Hidden();
			if (AddInfo.ZA_TreatmentCode_Hidden.Trim() == "444" && AddInfo.ZA_RNO.Trim() != "044")
			{
				AddInfo.ZA_TreatmentCode_HiddenInfo.AddMessageError("Treatment Code '444' can only be used when Tariff Rate Number is '044'.");
			}

			if (!AddInfo.ZA_TreatmentCode_Hidden.IsEmpty)
			{
				CMRDutyWrapperForInvoiceLine dutyWrapperWithTreamentCode = new CMRDutyWrapperForInvoiceLine(AddInfo.InvoiceLine, false);
				CMRDutyWrapperForInvoiceLine dutyWrapperWithoutTreamentCode = new CMRDutyWrapperForInvoiceLine(AddInfo.InvoiceLine, true);
				CMRTreatmentRatePeriodSnapshot treatmentRate = CMRTreatmentRatePeriodSnapshot.Load(dutyWrapperWithTreamentCode, AddInfo.ZA_TreatmentCode_Hidden);

				if (treatmentRate != null && treatmentRate.IsCalculable)
				{
					CMRDutyCalculator dutyCalculatorWithTreatmentCode = new CMRDutyCalculator(dutyWrapperWithTreamentCode);
					CMRDutyCalculator dutyCalculatorWithoutTteatmentCode = new CMRDutyCalculator(dutyWrapperWithoutTreamentCode);
					if (dutyCalculatorWithTreatmentCode.Duty.Amount.Amount >= dutyCalculatorWithoutTteatmentCode.Duty.Amount.Amount)
					{
						ZStringBuilder warningMessage = new ZStringBuilder();
						warningMessage.Append("Duty should be LESS with the treatment code. But the duty WITH treatment code is ");
						warningMessage.Append("$" + dutyCalculatorWithTreatmentCode.Duty.Amount);
						warningMessage.Append(". The duty WITHOUT treatment code is ");
						warningMessage.Append("$" + dutyCalculatorWithoutTteatmentCode.Duty.Amount);
						warningMessage.Append(".");
						AddInfo.ZA_TreatmentCode_HiddenInfo.AddWarning(warningMessage.ToString());
					}
				}
			}

			ValidateZA_InstrumentType_Hidden();
		}
		protected override void CheckZA_RNO()
		{
			base.CheckZA_RNO();
			CodeDescriptionPairList rateNumbers = AddInfo.Lookups.ZA_RNO_List;
			if (rateNumbers.Count > 1 && AddInfo.ZA_RNO.IsEmpty)
			{
				AddInfo.ZA_RNOInfo.AddMessageError("Tariff Rate Number is required for this Tariff number / Preference Scheme Type combination");
			}

			if (AddInfo.ZA_RNO.Trim() == "044" && AddInfo.ZA_TreatmentCode_Hidden.Trim() != "444")
			{
				AddInfo.ZA_RNOInfo.AddMessageError("Tariff Rate Number '044' can only be used when Treatment Code is '444'.");
			}

			ListValidation.MessageErrorIfInvalidCode(AddInfo.ZA_RNOInfo, rateNumbers);

			if (AddInfo.ZA_RNO.Length > 3)
			{
				AddInfo.ZA_RNOInfo.AddMessageError("The length of Tariff Rate Number cannot be greater than 3.");
			}

			ValidateAddInfoLine();
		}
		protected override void CheckZA_TRN()
		{
			base.CheckZA_TRN();
			ListValidation.MessageErrorIfInvalidCode(AddInfo.ZA_TRNInfo, AddInfo.Lookups.ZA_TRN_List);
		}
		protected override void CheckZA_ISS()
		{
			base.CheckZA_ISS();
			if (JobDeclaration.IsNature30)
			{
				if (InvoiceLine != null && InvoiceLine.JI_CustomsUnitQty == LitresOfAlcohol || AddInfo.ZA_UQ2 == LitresOfAlcohol)
				{
					MandatoryValidation.MessageErrorIfNotEntered(AddInfo.ZA_ISSInfo);
				}
			}
			else
			{
				if (AddInfo.ZA_ISS != 0)
				{
					AddInfo.ZA_ISSInfo.AddMessageError("Allowed for Nature 30 only.");
				}
			}
		}
		protected override void CheckZA_WRU()
		{
			base.CheckZA_WRU();
			if (JobDeclaration.IsNature30 || InvoiceLine.IsNature20)
			{
				if (AddInfo.ZA_WRU.IsEmpty)
				{
					if (InvoiceLine.JI_CustomsUnitQty.IsEmpty)
					{
						AddInfo.ZA_WRUInfo.AddMessageError("Warehouse reference unit of quantity is required");
					}
					else if (AddInfo.ZA_WRQ > 0)
					{
						AddInfo.ZA_WRUInfo.AddMessageError("Warehouse reference unit of quantity is required when warehouse reference quantity is entered");
					}
				}
			}
			else if (!AddInfo.ZA_WRU.IsEmpty)
			{
				AddInfo.ZA_WRUInfo.AddMessageError("Warehouse reference unit of quantity is only required for a nature 20 or 30");
			}
		}
		protected override void CheckZA_WRQ()
		{
			base.CheckZA_WRQ();
			JobDeclaration cachedDeclaration = JobDeclaration;
			if ((cachedDeclaration != null && cachedDeclaration.IsNature30) || InvoiceLine.IsNature20)
			{
				if (AddInfo.ZA_WRQ == 0)
				{
					if (InvoiceLine.JI_CustomsQuantity == 0)
					{
						AddInfo.ZA_WRQInfo.AddMessageError("Warehouse reference quantity is required");
					}
					else if (!AddInfo.ZA_WRU.IsEmpty)
					{
						AddInfo.ZA_WRQInfo.AddMessageError("Warehouse reference quantity is required when warehouse reference unit of quantity is entered");
					}
				}
			}
			else if (AddInfo.ZA_WRQ > 0)
			{
				AddInfo.ZA_WRQInfo.AddMessageError("Warehouse reference quantity is only required for a nature 20 or 30");
			}
		}
		protected override void CheckZA_DCX()
		{
			base.CheckZA_DCX();
			if (!AddInfo.ZA_DCX.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(AddInfo.ZA_DCXInfo, AddInfo.Lookups.CountryCode_List);
			}
		}
		protected override void CheckZA_DXT()
		{
			base.CheckZA_DXT();
			if (!AddInfo.ZA_DXT.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(AddInfo.ZA_DXTInfo, AddInfo.Lookups.ZA_DXT_List);

				if (!AddInfo.ZA_DSN.IsEmpty)
				{
					AddInfo.ZA_DXTInfo.AddMessageError("Dumping Exemption Type cannot be used with Dumping Specification Number. Please delete one of these values.");
				}
			}

			ValidateZA_DRE();
			ValidateZA_DSN();
		}

		protected override void CheckZA_FOD()
		{
			base.CheckZA_FOD();
			if (!AddInfo.ZA_FOD.IsEmpty && AddInfo.FOD.IsEmpty)
			{
				AddInfo.ZA_FODInfo.AddMessageError(AddInfo.ZA_FOD + " is not in a valid format. The Expected format is day, month, year.");
			}

			ValidateZA_ISC();
		}
		protected override void CheckZA_LCTI()
		{
			base.CheckZA_LCTI();
			ValidateIndicatorValue(AddInfo.ZA_LCTI, AddInfo.ZA_LCTIInfo, "Luxury Car Tax Payable Indicator");
		}
		protected override void CheckZA_MLPI()
		{
			base.CheckZA_MLPI();
			ValidateIndicatorValue(AddInfo.ZA_MLPI, AddInfo.ZA_MLPIInfo, "Manual Line Processing Indicator");
		}
		protected override void CheckZA_SEC()
		{
			base.CheckZA_SEC();
			ValidateIndicatorValue(AddInfo.ZA_SEC, AddInfo.ZA_SECInfo, "Security Calculate Indicator");

			if (!Parent.ZA_SEC.IsEmpty)
			{
				Parent.ZA_SECInfo.AddWarning("This value is only required when you send a Pre-Lodge Declaration");
			}
		}
		protected override void CheckZA_SCN()
		{
			if (!Parent.ZA_SCN.IsEmpty)
			{
				if (Parent.ZA_SCN.Length != 9)
				{
					Parent.ZA_SCNInfo.AddMessageError("Security Number must be 9 alpha-numeric characters");
				}

				Parent.ZA_SCNInfo.AddWarning("This value will only be sent for a Formal Declaration. It will not be sent in Pre-Lodge Declaration.");
			}
		}

		#region CheckZA_TILV

		public const string EmptyTransportAndInsuranceForExWarehouseMessageError = "Transport and Insurance must be supplied for Ex-Warehouse. You can enter it as TILV on the AddInfo or you can enter OFT or ONS in Charges grid.";
		protected override void CheckZA_TILV()
		{
			base.CheckZA_TILV();

			JobDeclaration declaration = JobDeclaration;

			if (declaration != null && declaration.IsNature30)
			{
				if (InvoiceLine.TransportAndInsurance.IsEmpty)
				{
					AddInfo.ZA_TILVInfo.AddMessageError(EmptyTransportAndInsuranceForExWarehouseMessageError);
				}
			}

			if (!AddInfo.ZA_TILV.IsEmpty)
			{
				if (AddInfo.ZA_TILV.StartsWith("-"))
				{
					AddInfo.ZA_TILVInfo.AddError(NoNegativeTILVAllowed);
				}
			}
		}

		public const string NoNegativeTILVAllowed = "You have entered a negative amount of TILV.";

		#endregion

		protected override void CheckZA_PUP()
		{
			base.CheckZA_PUP();
			ValidateIndicatorValue(AddInfo.ZA_PUP, AddInfo.ZA_PUPInfo, "Paid Under Protest Indicator");
		}

		protected override void CheckZA_ELA()
		{
			base.CheckZA_ELA();
			if (!AddInfo.ZA_ELA.IsEmpty)
			{
				ZString[] splitELACValues = AddInfo.ZA_ELA.Split(',');
				foreach (ZString currentELACValue in splitELACValues)
				{
					if (currentELACValue.Trim().Length != 8)
					{
						AddInfo.ZA_ELAInfo.AddMessageError("Each ELAC number must be at least 8 characters long.");
						break;
					}
				}
			}
		}
		protected override void CheckZA_CL2()
		{
			base.CheckZA_CL2();
			ZString cL2 = AddInfo.ZA_CL2;
			if (!cL2.IsEmpty && cL2.Replace(".", "").Trim().Length != 8)
			{
				AddInfo.ZA_CL2Info.AddMessageError("Second Tariff Item must be 8 characters long.");
			}
		}

		#region Warehouse Fields

		protected override void CheckZA_WMC()
		{
			base.CheckZA_WMC();
			JobDeclaration cachedDeclaration = JobDeclaration;

			if (cachedDeclaration != null && cachedDeclaration.IsExWarehouse && !AddInfo.ZA_WMC.IsEmpty)
			{
				if (AddInfo.ZA_WMC.Length > 9)
				{
					AddInfo.ZA_WMCInfo.AddMessageError("Multiple Clearance Code can only be 9 characters long. If this is a COMPILE N20 Entry Number please enter this into the Warehouse reference number field.");
				}

				if (!AddInfo.AggregatedValue(AUAddInfo.Schema.ZA_WRL).IsEmpty || !AddInfo.AggregatedValue(AUAddInfo.Schema.ZA_WRN).IsEmpty)
				{
					AddInfo.ZA_WMCInfo.AddMessageError("You can only enter either the Multiple Clearance Code or the Warehouse reference number and line. You cannot enter both.");
				}
			}
		}

		protected override void CheckZA_WRL()
		{
			base.CheckZA_WRL();

			JobDeclaration cachedDeclaration = JobDeclaration;

			if (cachedDeclaration != null && (cachedDeclaration.IsExWarehouse || cachedDeclaration.IsWarehousedByExternalAgent))
			{
				if (AddInfo.AggregatedValue(AUAddInfo.Schema.ZA_WRL).IsEmpty)
				{
					if (AddInfo.ZA_WMC.IsEmpty)
					{
						AddInfo.ZA_WRLInfo.AddMessageError("Please enter a warehouse reference line either on the invoice or on the line.");
					}
				}
				else if (!AddInfo.ZA_WMC.IsEmpty)
				{
					AddInfo.ZA_WRLInfo.AddMessageError("You can only enter either the Multiple Clearance Code or the Warehouse reference line. You cannot enter both. Please delete this value from the invoice or the line.");
				}

				if (!AddInfo.ZA_WRN.IsEmpty && cachedDeclaration.IsWarehousedByExternalAgent && HasSameEntryLineWithDifferentMergingRatio(InvoiceLine))
				{
					AddInfo.ZA_WRLInfo.AddError("Line with same WRN and WRL cannot have different ratio of Invoice Qty to Customs Qty or different WUV or Product.");
				}
			}
		}

		bool HasSameEntryLineWithDifferentMergingRatio(JobComInvoiceLine invoiceLine)
		{
			var result = false;
			var wrn = invoiceLine.AddInfo.ZA_WRN;
			var wrl = invoiceLine.AddInfo.ZA_WRL;
			var ratio = GetRatio(invoiceLine);
			var productPK = invoiceLine.JI_OP;
			var wuv = invoiceLine.WUV;
			var lowerWUV = wuv - 0.0001m;
			var upperWUV = wuv + 0.0001m;
			foreach (JobComInvoiceLine line in JobDeclaration.InvoiceLines)
			{
				if (line != invoiceLine && line.AddInfo.ZA_WRL == wrl && line.AddInfo.ZA_WRN == wrn && (line.JI_OP != productPK || GetRatio(line) != ratio || !IsBetween(line.WUV, lowerWUV, upperWUV)))
				{
					result = true;
					break;
				}
			}
			return result;
		}

		bool IsBetween(ZDecimal value, ZDecimal lowerValue, ZDecimal upperValue)
		{
			return lowerValue <= value && value <= upperValue;
		}

		decimal GetRatio(JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.JI_CustomsQuantity.IsEmpty ? -1m : decimal.Round(invoiceLine.JI_InvoiceQuantity / invoiceLine.JI_CustomsQuantity, 6);
		}

		protected override void CheckZA_WUV()
		{
			base.CheckZA_WUV();
			ValidateZA_WRL();
		}

		protected override void CheckZA_WRN()
		{
			base.CheckZA_WRN();
			JobDeclaration cachedDeclaration = JobDeclaration;

			if (cachedDeclaration != null && (cachedDeclaration.IsExWarehouse || cachedDeclaration.IsWarehousedByExternalAgent))
			{
				ZString aggregatedWarehouseValue = AddInfo.AggregatedValue(AUAddInfo.Schema.ZA_WRN).ToString();

				if (aggregatedWarehouseValue.IsEmpty)
				{
					if (AddInfo.ZA_WMC.IsEmpty)
					{
						AddInfo.ZA_WRNInfo.AddMessageError("Please enter a warehouse reference number either on the invoice or on the line.");
					}
				}
				else
				{
					if (aggregatedWarehouseValue.Length != 9 && aggregatedWarehouseValue.Length != 11)
					{
						AddInfo.ZA_WRNInfo.AddMessageError("Warehouse reference number must be either 9 or 11 characters long.");
					}

					if (!AddInfo.ZA_WMC.IsEmpty)
					{
						AddInfo.ZA_WRNInfo.AddMessageError("You can only enter either the Multiple Clearance Code or the Warehouse reference number. You cannot enter both. Please delete this value from the invoice or the line.");
					}
				}
			}
			ValidateZA_WRL();
		}

		#endregion

		protected override void CheckZA_PST()
		{
			base.CheckZA_PST();
			if (AddInfo.AggregatedValue(AUAddInfoSchema.ZA_PST.Name).IsEmpty && !AddInfo.IsGeneralRate)
			{
				AddInfo.ZA_PSTInfo.AddMessageError("A preference scheme should be entered at Invoice Line level. Otherwise the General Duty rate will be calculated.");
			}

			ValidateZA_TreatmentCode_Hidden();
		}
		protected override void CheckZA_DRE()
		{
			base.CheckZA_DRE();
			if (!AddInfo.ZA_DRE.IsEmpty && AddInfo.ZA_DXT.IsEmpty && AddInfo.ZA_DSN.IsEmpty)
			{
				AddInfo.ZA_DREInfo.AddMessageError("Either Dumping Exemption Type (DXT) or the Dumping Specification Number (DSN) is required when entered.");
			}
		}

		protected override void CheckZA_DSN()
		{
			base.CheckZA_DSN();

			if (!AddInfo.ZA_DSN.IsEmpty && !AddInfo.ZA_DXT.IsEmpty)
			{
				AddInfo.ZA_DSNInfo.AddMessageError("Dumping Specification Number cannot be used with Dumping Exemption Type. Please delete one of these values.");
			}

			ValidateZA_DRE();
			ValidateZA_DXP();
		}
		protected override void CheckZA_DXP()
		{
			base.CheckZA_DXP();
			if (!AddInfo.ZA_DSN.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(AddInfo.ZA_DXPInfo, "A Dumping Export Price (DXP) is required if a Dumping Specification Number (DSN) is entered.");
			}
		}
		protected override void CheckZA_ISC()
		{
			base.CheckZA_ISC();
			if (!AddInfo.ZA_FOD.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(AddInfo.ZA_ISCInfo, "Instrument Security Code (ISC) is required when a Firm Order Date (FOD) is entered.");
			}
		}
		protected override void CheckZA_POC()
		{
			if (AddInfo.AggregatedZA_POC.IsEmpty
				&& !AddInfo.IsGeneralRate
				&& !AddInfo.AggregatedZA_ORG.IsEmpty)
			{
				AddInfo.ZA_POCInfo.AddWarning("You have not entered Preference Origin and Goods origin, " + AddInfo.AggregatedZA_ORG + " is used instead.");
				ValidateAddInfoLine();
			}
			else
			{
				base.CheckZA_POC();
			}
		}
		protected override void CheckZA_VALB_Hidden()
		{
			base.CheckZA_VALB_Hidden();
			if (AddInfo.AggregatedZA_VALB_Hidden.IsEmpty)
			{
				AddInfo.ZA_VALB_HiddenInfo.AddMessageError("The valuation basis must be set at either the header or line level. If you set at header level then that value will default on lines, and so you do not need to set on lines.");
			}
		}
		protected override void CheckZA_REL_Hidden()
		{
			base.CheckZA_REL_Hidden();
			ListValidation.MessageErrorIfInvalidCode(AddInfo.ZA_REL_HiddenInfo, Lookups.ZA_REL_List);
			if (AggregatedREL.IsEmpty)
			{
				AddInfo.ZA_REL_HiddenInfo.AddMessageError("The transaction (relationship) basis must be set at either the header or line level. If you set at header level then that value will default on lines, and so you do not need to set on lines.");
			}
		}
		protected override void CheckZA_ICN()
		{
			base.CheckZA_ICN();
			if (!AddInfo.ZA_ICN.IsEmpty)
			{
				if (AddInfo.ZA_ICN.Trim().Length > 7)
				{
					AddInfo.ZA_ICNInfo.AddMessageError("Import Credit Number can only be 7 characters long.");
				}

				if (InvoiceLine != null)
				{
					CMRDutyWrapperForInvoiceLine invoiceLineDutyWrapper = new CMRDutyWrapperForInvoiceLine(InvoiceLine, false);
					CMRDutyCalculator dutyCalculator = new CMRDutyCalculator(invoiceLineDutyWrapper);
					if (!dutyCalculator.Duty.HasDuty)
					{
						AddInfo.ZA_ICNInfo.AddMessageError("ICN only required when there is duty payable.");
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public const string GSTERedundantMessage = "The Schedule 4 Tariff Item or Treatment Code you have entered is automated for GST Exemption by Australian Customs according to information in the CMR Reference Files. AU Customs will reject a message if the GST Exemption code is sent as it is redundant. CargoWise One will ignore the GSTE Code and won't build this code in the message.";
		protected override void CheckZA_GSTE()
		{
			base.CheckZA_GSTE();
			if (AddInfo.ZA_GSTE != "")
			{
				JobComInvoiceLine invoiceLine = AddInfo.InvoiceLine;
				if (invoiceLine != null && invoiceLine.DoesTariffRateOrTreatmentCodeDeemGSTExemption)
				{
					AddInfo.ZA_GSTEInfo.AddWarning(GSTERedundantMessage);
				}
			}
		}

		protected override void CheckZA_WAR()
		{
			base.CheckZA_WAR();
			if (!AddInfo.ZA_WAR.IsEmpty)
			{
				if (AddInfo.InvoiceLine?.JI_IsPackToBondForLine ?? false)
				{
					AddInfo.ZA_WARInfo.AddMessageError("Warehouse Establishment Code should not be entered for N20.");
				}
				else if (new EstablishmentCodeValidation(Parent).ValidateEstablishmentCode(AddInfo.ZA_WARInfo))
				{
					ListValidation.MessageErrorIfInvalidCode(AddInfo.ZA_WARInfo, AddInfo.Lookups.EstablishmentCodes);
				}
			}
		}

		#region Instrument Type

		protected override void CheckZA_InstrumentType_Hidden()
		{
			base.CheckZA_InstrumentType_Hidden();
			ListValidation.MessageErrorIfInvalidCode(AddInfo.ZA_InstrumentType_HiddenInfo, AddInfo.Lookups.ZA_InstrumentType_List);
			if (!AddInfo.ZA_InstrumentType_HiddenInfo.HasNotifications()
				&& InvoiceLine != null
				&& InvoiceLine.TreatmentCode == "505"
				&& InvoiceLine.InstrumentType != CMRInstrumentTypeList.Codes.TariffConcessionOrder)
			{
				AddInfo.ZA_InstrumentType_HiddenInfo.AddMessageError("As Treatment code is '505', Instrument Type must be 'TC'.");
			}
		}

		protected override bool DoesInstrumentCodeExist
		{
			get { return CMRInstrument != null; }
		}

		#endregion

		#region Implementation
		void ValidateIndicatorValue(ZString value, ZPropertyInfo property, ZString errorMessageValue)
		{
			if (!value.IsEmpty && value != "Y")
			{
				property.AddMessageError("If you wish to select " + errorMessageValue + ", please enter 'Y'");
			}
		}

		ZString AggregatedREL
		{
			get
			{
				ZString result = ZString.Empty;
				if ((AddInfo.ZA_REL_Hidden.IsEmpty || AddInfo.ZA_REL_Hidden == CMRRelatedTransaction.Default.Code) && AddInfo.InvoiceLine.InvoiceHeader != null)
				{
					result = AddInfo.InvoiceLine.InvoiceHeader.AddInfo.ZA_HeaderREL_Hidden;
				}
				else
				{
					result = AddInfo.ZA_REL_Hidden == CMRRelatedTransaction.Default.Code ? ZString.Empty : AddInfo.ZA_REL_Hidden;
				}
				return result;
			}
		}

		#endregion
	}
}
