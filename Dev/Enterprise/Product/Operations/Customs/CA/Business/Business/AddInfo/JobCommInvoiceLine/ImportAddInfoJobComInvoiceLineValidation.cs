using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class ImportAddInfoJobComInvoiceLineValidation : AddInfoJobComInvoiceLineValidation
	{
		public ImportAddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		JobComInvoiceLine InvoiceLine
		{
			get { return Parent.Parent; }
		}

		CADeclarationValidator DeclarationValidator
		{
			get { return InvoiceLine.Declaration?.DeclarationValidator; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateBlankODGCFIA();
		}

		#region CheckCheckCA_ADJValue

		protected override void CheckCA_ADJValue()
		{
			base.CheckCA_ADJValue();
			var invoiceLine = InvoiceLine;
			if (invoiceLine.IsWarrantyRepairLine && invoiceLine.CA_ADJValue != 0m)
			{
				Parent.CA_ADJValueInfo.AddMessageError(ImportJobComInvoiceLineValidation.ValueShouldBeZero);
			}
		}

		#endregion

		#region CheckCA_CVforCurrConv

		protected override void CheckCA_CVforCurrConv()
		{
			base.CheckCA_CVforCurrConv();

			var invoiceLine = InvoiceLine;
			if (invoiceLine.IsWarrantyRepairLine)
			{
				if (Parent.CA_CVforCurrConv != 0m)
				{
					Parent.CA_CVforCurrConvInfo.AddMessageError(ImportJobComInvoiceLineValidation.ValueShouldBeZero);
				}
			}
			else if ((DeclarationValidator?.IsValidationRequired(ValidateForMessageType.B3CUSDEC) ?? false) && !invoiceLine.IsLuxuryTaxInvoiceLine)
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.CA_CVforCurrConvInfo);
				MandatoryValidation.MessageErrorIfIsNegative(Parent.CA_CVforCurrConvInfo);
			}
		}

		#endregion

		#region CheckCA_AuthorityNumber

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void CheckCA_AuthorityNumber()
		{
			var invoiceLine = InvoiceLine;
			if (!invoiceLine.IsRemissionRepairLine)
			{
				base.CheckCA_AuthorityNumber();
				ValidateRemissionEffectiveDate();

				var invoiceHeader = invoiceLine.InvoiceHeader;
				var skipAuthCheck = invoiceHeader?.IsRemissionAllOrMexicoAndUSDutyOnlyLVXInvoiceHeader ?? ZBool.False;

				if (!skipAuthCheck && invoiceHeader != null && invoiceHeader.IsAttachedToPersistentConsolidatedLVS
				   && invoiceHeader.FirstAdditionalOrOnlyDeclaration.JE_MessageSubType == LowValueShipmentsTypes.Codes.TotalConsolidation
				   && !invoiceLine.CA_AuthorityNumber.IsEmpty)
				{
					Parent.CA_AuthorityNumberInfo.AddError(Res.GetString("8c7afec7-782b-4ee2-a0ad-36df1511ae1e", "You should not enter a Special Authority (OIC) on a Total Consolidation (VAR) Type F Declaration"));
				}

				if (invoiceLine.CA_AuthorityNumber.IsEmpty && !skipAuthCheck && invoiceHeader != null && invoiceHeader.IsAttachedToPersistentDeclaration && invoiceHeader.FirstAdditionalOrOnlyDeclaration.JE_MessageSubType != LowValueShipmentsTypes.Codes.TotalConsolidation)
				{
					if ((DeclarationValidator?.IsValidationRequired(ValidateForMessageType.B3CUSDEC) ?? false) && invoiceLine.CA_SIMExemptCode == SIMACodes.Codes.C50)
					{
						Parent.CA_AuthorityNumberInfo.AddMessageError(Res.GetString("5e66e068-7016-4fc2-a363-76c97c2f310e", "Special Authority Number required with SIMA Code 50"));
					}

					if (invoiceLine.Declaration?.IsMergeDone ?? false)
					{
						ZDecimal totalValueForDuty = 0.0m;

						if (invoiceHeader.IsAttachedToPersistentLVSDeclaration)
						{
							totalValueForDuty = invoiceHeader.TotalValueForDuty;
						}
						else
						{
							totalValueForDuty = invoiceLine.Declaration.TotalCustomsValueInLocalCurrency;
						}

						if (totalValueForDuty < CACustomsDataRegistry.Instance.MinimumVFDForDutyAndTax.Value)
						{
							Parent.CA_AuthorityNumberInfo.AddMessageError(Res.GetString("2257cb77-6148-4868-a422-cd057514dbc4", "The total Value for Duty of this job is less than the minimum but you have not entered an OIC regular remission."));
						}
					}
				}
				if (!skipAuthCheck && invoiceLine.CA_AuthorityNumber.IsEmpty &&
					!invoiceLine.CA_CalculationMethod.IsEmpty &&
					invoiceLine.CA_CalculationMethod != CalculationMethods.Codes.NoRemission &&
					!invoiceLine.IsChildLine &&
					invoiceLine.CA_CalculationMethod != CalculationMethods.Codes.DeliveredDutyPaid)
				{
					Parent.CA_AuthorityNumberInfo.AddMessageError(Res.GetString("4588d19b-8519-43e2-ac0f-bdee8ec93e0e", "A Special Authority (OIC) code is required when remission is claimed"));
				}

				if (!Parent.CA_AuthorityNumberInfo.HasErrors())
				{
					var declaration = invoiceLine.Declaration;
					var org = declaration != null ? declaration.ImporterOfRecord ?? declaration.Importer : invoiceHeader?.Buyer;
					ZZRefCusRulingValidator.ValidateSpecialAuthorityNumber(Parent.Factory, Parent.CA_AuthorityNumberInfo, org != null ? org.PK : ZGuid.Empty);
					ZZRefCusRulingValidator.ValidateAuthorityNumberMatchesRemissionType(Parent.CA_AuthorityNumberInfo, Parent.CA_CalculationMethod, invoiceLine.Ruling);
				}
			}
		}

		void ValidateRemissionEffectiveDate()
		{
			var invoiceLine = InvoiceLine;

			if (!invoiceLine.CA_AuthorityNumber.IsEmpty && invoiceLine.Ruling is Universal.ZZRefCusRulingCombined invoiceLineRuling)
			{
				Func<Universal.CusRulingConfigCombined, ZBool> filterCategoryDAT = (x) => x.ZZY_Category == Universal.RefCusRulingConfigCategories.Codes.DAT;

				var config = invoiceLine.RulingConfigurations.Cast<Universal.CusRulingConfigCombined>().FirstOrDefault(x => filterCategoryDAT(x))
					?? invoiceLineRuling.Configurations.Cast<Universal.CusRulingConfigCombined>().FirstOrDefault(x => filterCategoryDAT(x));

				var effectiveDate = new ZDate();
				var useDefautREL = config == null;
				if (useDefautREL || config.ZZY_Type.ToString() == Universal.RefCusRulingConfigTypes.Codes.REL)
				{
					effectiveDate = invoiceLine.EffectiveDateForDutyRate;
				}
				else
				{
					effectiveDate = invoiceLine.InvoiceHeader.JZ_ValuationDateOverride.Date;
				}

				if (effectiveDate < invoiceLineRuling.ZZX_StartDate || effectiveDate > invoiceLineRuling.ZZX_EndDate)
				{
					Parent.CA_AuthorityNumberInfo.AddMessageError(ResString.GetMultilingualString("34EA826C-8090-4C17-8092-8F8709481CC0", "Remission Number is on file but is not valid for {0}.", effectiveDate.ToShortDateString()));
				}
			}
		}

		#endregion

		#region CheckCA_ImportReasonCode

		protected override void CheckCA_ImportReasonCode()
		{
			base.CheckCA_ImportReasonCode();

			if (InvoiceLine.Validation is ImportJobComInvoiceLineValidation lineValidation)
			{
				lineValidation.ValidateCA_ImportReasonCodeTC();
				lineValidation.ValidateCA_ImportReasonCodeNR();
				lineValidation.ValidateCA_ImportReasonCodeSITT();
			}
		}

		#endregion

		#region CheckCA_DestinationProvince

		protected override void CheckCA_DestinationProvince()
		{
			if (IsOkaCFIA)
			{
				return;
			}

			base.CheckCA_DestinationProvince();
			var invoiceLine = InvoiceLine;
			if (IsCFIAValidationRequiredForThisLine)
			{
				ListValidation.MessageErrorIfInvalidCode(invoiceLine.CA_DestinationProvinceInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(invoiceLine.CA_DestinationProvinceInfo);
			}
		}

		#endregion

		#region CheckCA_RN_NKCFIAOrigin

		protected override void CheckCA_RN_NKCFIAOrigin()
		{
			if (IsOkaCFIA)
			{
				return;
			}

			base.CheckCA_RN_NKCFIAOrigin();
			ListValidation.MessageErrorIfInvalidCode(InvoiceLine.CA_RN_NKCFIAOriginInfo);
		}

		#endregion

		#region CheckCA_CFIAUSStateOfOrigin

		protected override void CheckCA_CFIAUSStateOfOrigin()
		{
			if (IsOkaCFIA)
			{
				return;
			}

			base.CheckCA_CFIAUSStateOfOrigin();
			var invoiceLine = InvoiceLine;
			if (invoiceLine.CA_RN_NKCFIAOrigin == Constants.CountryCodes.UnitedStates)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(invoiceLine.CA_CFIAUSStateOfOriginInfo, invoiceLine.AddInfoLookups.CFIAStatesOfOrigin);
			}
		}

		#endregion

		#region CheckCA_RN_NKExport

		protected override void CheckCA_RN_NKExport()
		{
			base.CheckCA_RN_NKExport();
			var invoiceLine = InvoiceLine;
			var invoiceHeader = invoiceLine.InvoiceHeader;
			if (invoiceHeader != null && invoiceHeader.IsAttachedToPersistentLVSDeclaration && invoiceLine.EffectiveCountryOfExport.IsEmpty
				&& TariffTreatmentCodes.IsCountryOfOriginAndExportRequiredForLVS(invoiceLine.EffectiveTreatmentCode))
			{
				Parent.CA_RN_NKExportInfo.AddMessageError(Res.GetString("15f470b1-08f8-4f07-9407-e3c3e2aed6f5",
					"Country/Region of Export must be specified for LVS when tariff treatment code other than 02 and 10."));
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_RN_NKExportInfo, Lookups.Exports);
		}

		#endregion

		#region CheckCA_USStateOfExport

		protected override void CheckCA_USStateOfExport()
		{
			base.CheckCA_USStateOfExport();
			if (InvoiceLine.CA_RN_NKExport == Constants.CountryCodes.UnitedStates)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_USStateOfExportInfo, Lookups.StatesOfExport);
			}
		}

		#endregion

		#region CheckCA_TIIN

		protected override void CheckCA_TIIN()
		{
			base.CheckCA_TIIN();
			var invoiceLine = InvoiceLine;
			if (ImportJobComInvoiceLineValidation.IsTCValidationRequiredForThisLine(invoiceLine)
				&& invoiceLine.CA_ImportReasonCode == ImportReasonCodes.Codes.Retread
				&& invoiceLine.EffectiveCountryOfOrigin != Constants.CountryCodes.UnitedStates)
			{
				MandatoryValidation.MessageErrorIfNotEntered(invoiceLine.CA_TIINInfo);
			}
		}

		#endregion

		#region CheckCA_TreatmentCode

		protected override void CheckCA_TreatmentCode()
		{
			base.CheckCA_TreatmentCode();
			var invoiceLine = this.InvoiceLine;
			var invoice = invoiceLine.InvoiceHeader;
			var treatmentCode = invoiceLine.CA_TreatmentCode;

			invoiceLine.Validation.MatchToProductValidation(invoiceLine.CA_TreatmentCodeInfo, () => invoiceLine.Pivot?.CCA_TreatmentCode ?? ZString.Empty);

			if (invoice != null && (invoice.FirstAdditionalOrOnlyDeclaration?.IsLVSTotalConsolidation ?? false)
				&& treatmentCode == TariffTreatmentCodes.Codes.General
				&& invoiceLine.JI_CountryOfOrigin == Core.Constants.CountryCodes.UnitedStates)
			{
				Parent.CA_TreatmentCodeInfo.AddMessageError(Res.GetString("931308b8-0b8e-4b73-a089-4f46ff1bf621", "General Rate of Duty (TT 03) is not applicable to US origin shipments"));
			}
			if (invoice != null && invoice.CA_TreatmentCode.IsEmpty && (DeclarationValidator?.IsValidationRequired(ValidateForMessageType.B3CUSDEC) ?? false))
			{
				MandatoryValidation.CheckEntered(Parent.CA_TreatmentCodeInfo);
				ListValidation.MessageErrorIfInvalidCode(Parent.CA_TreatmentCodeInfo, Lookups.TreatmentCodes);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CA_TreatmentCodeInfo, Lookups.TreatmentCodes);
			}
			if (!treatmentCode.IsEmpty && (DeclarationValidator?.IsValidationRequired(ValidateForMessageType.B3CUSDEC) ?? false))
			{
				if (!invoiceLine.JI_Tariff.IsEmpty)
				{
					var rate = CACRate.Load(invoiceLine);
					if (rate == null)
					{
						DutyAndTaxManager.AddNoDutyRateFoundMessageError(Parent.CA_TreatmentCodeInfo, invoiceLine.JI_FormattedTariff, treatmentCode, invoiceLine.EffectiveDateForDutyRate);
					}
				}
			}
			if (TariffTreatmentCodes.IsNeedCheckForValidCertificateOfOrigin(treatmentCode) && Lookups.TreatmentCodes.ContainsCode(treatmentCode))
			{
				CheckForValidCertificateOfOrigin();
			}
		}

		void CheckForValidCertificateOfOrigin()
		{
			var action = CACustomsDataRegistry.Instance.SeverityLevelForCertificateOfOriginValidations.Value;

			if (ShouldCheckForValidCertificateOfOrigin(action))
			{
				var ior = InvoiceLine.Declaration?.ImporterOfRecord;
				var importer = InvoiceLine.Declaration?.Importer;
				var part = InvoiceLine.Part;

				if (ior != null && HasValidCertificateOfOrigin(ior.RequiredDocuments))
				{
					return;
				}
				else if (importer != null && HasValidCertificateOfOrigin(importer.RequiredDocuments))
				{
					return;
				}
				else if (part != null && HasValidCertificateOfOrigin(((IHaveRequiredDocuments)part).RequiredDocuments))
				{
					return;
				}

				if (action == ProductAuditActions.Codes.AddMessageErrorValidation)
				{
					InvoiceLine.CA_TreatmentCodeInfo.AddMessageError(CheckCertificateOfOrigin);
				}
				else if (action == ProductAuditActions.Codes.AddWarningValidation)
				{
					InvoiceLine.CA_TreatmentCodeInfo.AddWarning(CheckCertificateOfOrigin);
				}
			}
		}

		bool ShouldCheckForValidCertificateOfOrigin(string action)
		{
			return action == ProductAuditActions.Codes.AddMessageErrorValidation || action == ProductAuditActions.Codes.AddWarningValidation;
		}

		bool HasValidCertificateOfOrigin(JobRequiredDocumentDependentCollection requiredDocuments)
		{
			return requiredDocuments.OfType<JobRequiredDocument>().Any(x =>
				x.EQ_DocType == Core.Constants.RefDocTypes.CertificateOfOrigin &&
				x.Attributes[JobRequiredDocAttribTypeList.Codes.TradePreferenceCode, InvoiceLine.CA_TreatmentCode] != null &&
				(x.EQ_ValidToDate.IsEmpty || InvoiceLine.EffectiveDateForDutyRate <= x.EQ_ValidToDate));
		}

		internal static string CheckCertificateOfOrigin
		{
			get { return Res.GetString("f1dae597-769a-4d0d-be63-02f493a0f2e2", "The Certificate of Origin in eDocs of the Importer of Record Organization or Product Code is not valid or is expired."); }
		}

		#endregion

		#region CheckCA_Model

		protected override void CheckCA_Model()
		{
			base.CheckCA_Model();

			if (InvoiceLine.Validation is ImportJobComInvoiceLineValidation lineValidation)
			{
				lineValidation.ValidateCA_ModelNR();
				lineValidation.ValidateCA_ModelSITT();
			}
		}

		#endregion

		#region CheckCA_ModelNumber

		protected override void CheckCA_ModelNumber()
		{
			base.CheckCA_ModelNumber();

			if (InvoiceLine.Validation is ImportJobComInvoiceLineValidation lineValidation)
			{
				lineValidation.ValidateCA_ModelNumberNR();
				lineValidation.ValidateCA_ModelNumberSITT();
			}
		}

		#endregion

		PGAInvoiceLineValidator PGAInvoiceLineValidator
		{
			get => fPGAInvoiceLineValidator ?? (fPGAInvoiceLineValidator = new PGAInvoiceLineValidator(InvoiceLine));
		}
		PGAInvoiceLineValidator fPGAInvoiceLineValidator;

		#region CheckCA_TypeSize
		protected override void CheckCA_TypeSize()
		{
			base.CheckCA_TypeSize();

			if (InvoiceLine.Validation is ImportJobComInvoiceLineValidation lineValidation)
			{
				lineValidation.ValidateCA_TypeSizeTC();
				lineValidation.ValidateCA_TypeSizeNR();
			}
		}

		#endregion

		#region CheckCA_RequirementID

		protected override void CheckCA_RequirementID()
		{
			if (IsOkaCFIA)
			{
				return;
			}

			base.CheckCA_RequirementID();
			if (!Parent.CA_RequirementID.IsNumbersOnlyOrEmpty)
			{
				Parent.CA_RequirementIDInfo.AddError(Res.GetString("a2905c43-eccc-4926-95db-2ee891eeeec8", "Requirement ID should be composed of digits."));
			}
		}

		#endregion

		#region CheckCA_RequirementVer

		protected override void CheckCA_RequirementVer()
		{
			if (IsOkaCFIA)
			{
				return;
			}

			base.CheckCA_RequirementVer();
			if (!Parent.CA_RequirementVer.IsNumbersOnlyOrEmpty)
			{
				Parent.CA_RequirementVerInfo.AddError(Res.GetString("d6b9354e-32ab-43e2-92b0-9ad8950b1e5c", "Requirement Version should be composed of digits."));
			}
		}

		#endregion

		#region CheckCA_AirsCode

		protected override void CheckCA_AirsCode()
		{
			if (IsOkaCFIA)
			{
				return;
			}

			base.CheckCA_AirsCode();
			if (!Parent.CA_AirsCode.IsNumbersOnlyOrEmpty)
			{
				Parent.CA_AirsCodeInfo.AddError(Res.GetString("ee4c5535-ddd1-43c1-937d-07f0a223e986", "Airs (OGD) Code should be composed of digits."));
			}
			var invoiceLine = InvoiceLine;
			if (invoiceLine.Declaration.IsOGD && invoiceLine.Declaration.CA_OGDCFIA && !invoiceLine.JI_Tariff.IsEmpty && (DeclarationValidator?.IsValidationRequired(ValidateForMessageType.ACROSS) ?? false))
			{
				if (!invoiceLine.IsHSCodeContainsOGDCFIA)
				{
					if (!invoiceLine.CA_AirsCode.IsEmpty)
					{
						invoiceLine.CA_AirsCodeInfo.AddMessageError(NoCFIATariffMessage);
					}
				}
			}
		}

		#endregion

		#region CheckCA_EndUse

		protected override void CheckCA_EndUse()
		{
			if (IsOkaCFIA)
			{
				return;
			}

			base.CheckCA_EndUse();
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_EndUseInfo, Parent.Lookups.CFIAEndUseCodes);
		}

		#endregion

		#region CheckCA_MiscID

		protected override void CheckCA_MiscID()
		{
			if (IsOkaCFIA)
			{
				return;
			}

			base.CheckCA_MiscID();
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_MiscIDInfo, (ICodeDescriptionPairList)Parent.Lookups.CFIAMiscIDCodes);
		}

		#endregion

		#region CheckCA_CustomsValue

		protected override void CheckCA_CustomsValue()
		{
			base.CheckCA_CustomsValue();
			Parent.Parent.DutyAndTaxManager.ValidateDetailsWereFound(Parent.CA_CustomsValueInfo);

			if (!Parent.CA_CustomsValueInfo.HasMessageErrors())
			{
				var declaration = Parent.Declaration;
				if (declaration != null)
				{
					ZString[] list;
					if (declaration.IsCADEnabled)
					{
						list = new ZString[] { CADEntryTypeList.Codes.Warehouse101, CADEntryTypeList.Codes.Warehouse102, CADEntryTypeList.Codes.ReWarehouse131, CADEntryTypeList.Codes.ReWarehouse132, CADEntryTypeList.Codes.ExWarehouse201 };
					}
					else
					{
						list = new ZString[] { B3EntryTypeList.Codes.Warehouse10, B3EntryTypeList.Codes.ReWarehouse13, B3EntryTypeList.Codes.ExWarehouse20 };
					}

					if (list.Contains(Parent.Declaration.JE_MessageSubType) && !Parent.Parent.DutyAndTaxManager.Duties.Any())
					{
						Parent.CA_CustomsValueInfo.AddMessageError(Res.GetString("c45d1ceb-e5af-4947-b395-a0511fb4b10c", "At least one Customs Duty Rate must be specified."));
					}
				}
			}

			if (InvoiceLine.IsWarrantyRepairLine && Parent.CA_CustomsValue != 0m)
			{
				Parent.CA_CustomsValueInfo.AddMessageError(ImportJobComInvoiceLineValidation.ValueShouldBeZero);
			}
		}

		#endregion

		#region CheckCA_99TariffCode

		protected override void CheckCA_99TariffCode()
		{
			base.CheckCA_99TariffCode();
			if (Parent.CA_CalculationMethod == CalculationMethods.Codes.OneSixtiethRemission && Parent.CA_99TariffCode.IsEmpty)
			{
				Parent.CA_99TariffCodeInfo.AddWarning(Res.GetString("1081d4b8-b99e-479e-ae9c-a8393a7f463c", "A Tariff Code may be required with 1/60 Remission calculation method."));
			}

			if (Parent.CA_CalculationMethod == CalculationMethods.Codes.OneOneTwentiethRemission && Parent.CA_99TariffCode.IsEmpty)
			{
				Parent.CA_99TariffCodeInfo.AddWarning(Res.GetString("deca246e-fdda-4c98-87d4-149cd2dd9622", "A Tariff Code is required with 1/120 Remission calculation method."));
			}

			if (!(Parent.InvoiceHeader?.IsAttachedToPersistentLVXDeclaration ?? ZBool.False))
			{
				new TariffValidator(Parent.Factory).ValidateFourDigitTariff(Parent.CA_99TariffCodeInfo);
			}
		}

		#endregion

		#region CFIA, SITT, NRCAN And Tires Validation

		bool IsCFIAValidationRequiredForThisLine
		{
			get { return InvoiceLine.Declaration.IsOGD && InvoiceLine.Declaration.CA_OGDCFIA && (DeclarationValidator?.IsValidationRequired(ValidateForMessageType.ACROSS) ?? false) && InvoiceLine.IsHSCodeContainsOGDCFIA; }
		}

		bool IsOkaCFIA
		{
			get { return InvoiceLine.Declaration.IsOGD && InvoiceLine.CA_OGDStatus == AVSStatusList.Codes.WillBeApproved; }
		}

		public void ValidateCFIA()
		{
			ValidateBlankODGCFIA();
			ValidateCA_RequirementID();
			ValidateCA_RequirementVer();
			ValidateCA_AirsCode();
			ValidateCA_DestinationProvince();
			ValidateCA_EndUse();
			ValidateCA_MiscID();
			ValidateCA_RN_NKCFIAOrigin();
			ValidateCA_CFIAUSStateOfOrigin();
		}

		void ValidateBlankODGCFIA()
		{
			Parent.InvoiceLine.RemoveRowMessageError(NoCFIAData);
			if (IsCFIAValidationRequiredForThisLine)
			{
				if (InvoiceLine.IsOGDCFIABlank)
				{
					Parent.InvoiceLine.AddRowMessageError(NoCFIAData);
				}
			}
		}
		internal static string NoCFIAData => Res.GetString("f7b6c317-8f18-423f-8b37-ea47e4c15207", "No CFIA data has been entered.");

		public void ValidateSITT()
		{
			ValidateCA_ImportReasonCode();
			ValidateCA_Model();
			ValidateCA_ModelNumber();
		}

		public void ValidateNRCAN()
		{
			ValidateCA_ImportReasonCode();
			ValidateCA_Model();
			ValidateCA_ModelNumber();
			ValidateCA_TypeSize();
		}

		public void ValidateTires()
		{
			ValidateCA_ImportReasonCode();
			ValidateCA_TIIN();
			ValidateCA_CompliantCompletion();
			ValidateCA_CompliantImportDate();
			ValidateCA_TypeSize();
		}

		#endregion

		#region CheckCA_ValueForDutyCode (B3)

		protected override void CheckCA_ValueForDutyCode()
		{
			base.CheckCA_ValueForDutyCode();
			var invoiceLine = InvoiceLine;
			if (!invoiceLine.IsLuxuryTaxInvoiceLine)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CA_ValueForDutyCodeInfo, Parent.Lookups.ValueForDutyCodes);
				if (invoiceLine.InvoiceHeader != null && invoiceLine.InvoiceHeader.CA_ValueForDutyCode.IsEmpty && !invoiceLine.InvoiceHeader.IsAttachedToPersistentLVSDeclaration)
				{
					DeclarationValidator?.MessageErrorIfNotEntered(Parent.CA_ValueForDutyCodeInfo, string.Empty, ValidateForMessageType.B3CUSDEC);
				}
				if (invoiceLine.IsWarrantyRepairLine)
				{
					var parentLine = (JobComInvoiceLine)invoiceLine.ParentTariffLine;
					if (ValueForDutyCodes.IsRelatedFirms(parentLine.CA_ValueForDutyCode) && Parent.CA_ValueForDutyCode != ValueForDutyCodes.Codes.RelatedFirmsResidualMethodValue)
					{
						Parent.CA_ValueForDutyCodeInfo.AddMessageError(VFDCodeShouldBe29);
					}
					else if (ValueForDutyCodes.IsUnrelatedFirms(parentLine.CA_ValueForDutyCode) && Parent.CA_ValueForDutyCode != ValueForDutyCodes.Codes.UnrelatedFirmsResidualMethodValue)
					{
						Parent.CA_ValueForDutyCodeInfo.AddMessageError(VFDCodeShouldBe19);
					}
				}
			}
		}

		internal static string VFDCodeShouldBe19 => Res.GetString("f8bd538d-f3a2-43f4-8d6b-c56a8a6ec9aa", "The VFD code should be 19 if the importer and vendor are not related companies.");
		internal static string VFDCodeShouldBe29 => Res.GetString("74f578ee-1935-4bbb-8fd6-7c9dc5259e07", "The VFD code should be 29 if the importer and vendor are related companies.");

		#endregion

		#region CheckCA_PageNumber (B3)

		protected override void CheckCA_PageNumber()
		{
			base.CheckCA_PageNumber();
			var invoiceLine = InvoiceLine;
			var invoiceHeader = invoiceLine.InvoiceHeader;
			if (invoiceHeader != null && !invoiceHeader.IsAttachedToPersistentLVSDeclaration && !invoiceLine.IsLuxuryTaxInvoiceLine)
			{
				DeclarationValidator?.MessageErrorIfNotEntered(Parent.CA_PageNumberInfo, string.Empty, ValidateForMessageType.B3CUSDEC);
			}
		}

		#endregion

		#region CheckCA_CasualImportCommodity

		protected override void CheckCA_CasualImportCommodity()
		{
			base.CheckCA_CasualImportCommodity();
			var invoiceLine = InvoiceLine;
			if (!invoiceLine.CA_CasualImportCommodity.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(invoiceLine.CA_CasualImportCommodityInfo);
				if (!IsAppropriateUnit(invoiceLine.JI_CustomsUnitQty) &&
					!IsAppropriateUnit(invoiceLine.JI_CustomsSecondUnitQty) &&
					!IsAppropriateUnit(invoiceLine.JI_CustomsThirdUnitQty))
				{
					invoiceLine.CA_CasualImportCommodityInfo.AddWarning(Res.GetString("7b54cc5a-6833-4e41-884e-ee3226f157c7", "There are no appropriate units for this commodity on Duty & Tax Tab page."));
				}
			}
		}

		ZBool IsAppropriateUnit(ZString unit)
		{
			return IsAppropriateUnitForCommodity(unit, InvoiceLine.CA_CasualImportCommodity.Trim());
		}

		ZBool IsAppropriateUnitForCommodity(ZString unit, ZString commodity)
		{
			var result = ZBool.False;

			var commodityType = LookupsHelper.GetCasualImportCommodityType(Parent.Factory, commodity);
			if (commodityType == CasualImportConstants.CasualImpCommodityType.Alcohol)
			{
				result = IsUnitOfVolume(unit) || IsUnitOfWeight(unit);
			}
			else if (commodityType == CasualImportConstants.CasualImpCommodityType.Tobacco)
			{
				result = IsUnitOfQuantity(unit) || IsUnitOfWeight(unit);
			}

			return result;
		}

		ZBool IsUnitOfWeight(ZString unit)
		{
			return
				unit == CustomsUnitOfMeasureList.Codes.Milligram ||
				unit == CustomsUnitOfMeasureList.Codes.Gram ||
				unit == CustomsUnitOfMeasureList.Codes.Hectogram ||
				unit == CustomsUnitOfMeasureList.Codes.Kilogram ||
				unit == CustomsUnitOfMeasureList.Codes.Deciton ||
				unit == CustomsUnitOfMeasureList.Codes.MetricTon ||
				unit == CustomsUnitOfMeasureList.Codes.Kiloton;
		}

		ZBool IsUnitOfVolume(ZString unit)
		{
			return
				unit == CustomsUnitOfMeasureList.Codes.Ounce ||
				unit == CustomsUnitOfMeasureList.Codes.CubicMillimetre ||
				unit == CustomsUnitOfMeasureList.Codes.CubicCentimetre ||
				unit == CustomsUnitOfMeasureList.Codes.Millilitre ||
				unit == CustomsUnitOfMeasureList.Codes.Centilitre ||
				unit == CustomsUnitOfMeasureList.Codes.Decilitre ||
				unit == CustomsUnitOfMeasureList.Codes.CubicDecimetre ||
				unit == CustomsUnitOfMeasureList.Codes.Litre ||
				unit == CustomsUnitOfMeasureList.Codes.Hectolitre ||
				unit == CustomsUnitOfMeasureList.Codes.CubicMetre ||
				unit == CustomsUnitOfMeasureList.Codes.ThousandCubicMetres ||
				unit == CustomsUnitOfMeasureList.Codes.Megalitre ||
				unit == CustomsUnitOfMeasureList.Codes.MillionCubicMetres;
		}

		ZBool IsUnitOfQuantity(ZString unit)
		{
			return
				unit == CustomsUnitOfMeasureList.Codes.Number ||
				unit == CustomsUnitOfMeasureList.Codes.Pair ||
				unit == CustomsUnitOfMeasureList.Codes.Dozen ||
				unit == CustomsUnitOfMeasureList.Codes.Score ||
				unit == CustomsUnitOfMeasureList.Codes.DozenPairs ||
				unit == CustomsUnitOfMeasureList.Codes.Gross ||
				unit == CustomsUnitOfMeasureList.Codes.GreatGross ||
				unit == CustomsUnitOfMeasureList.Codes.Hundred ||
				unit == CustomsUnitOfMeasureList.Codes.Thousand ||
				unit == CustomsUnitOfMeasureList.Codes.Million;
		}

		#endregion

		#region CheckCA_CalculationMethod

		protected override void CheckCA_CalculationMethod()
		{
			base.CheckCA_CalculationMethod();
			var invoiceLine = InvoiceLine;
			var invoiceHeader = invoiceLine.InvoiceHeader;
			var skipCheck = invoiceHeader?.IsRemissionAllOrMexicoAndUSDutyOnlyLVXInvoiceHeader ?? ZBool.False;

			if (!skipCheck && invoiceHeader != null && invoiceHeader.IsAttachedToPersistentConsolidatedLVS &&
				invoiceHeader.FirstAdditionalOrOnlyDeclaration.JE_MessageSubType == LowValueShipmentsTypes.Codes.TotalConsolidation &&
				!invoiceLine.CA_CalculationMethod.IsEmpty &&
				invoiceLine.CA_CalculationMethod != CalculationMethods.Codes.NoRemission &&
				invoiceLine.CA_CalculationMethod != CalculationMethods.Codes.DeliveredDutyPaid)
			{
				Parent.CA_CalculationMethodInfo.AddError(Res.GetString("def705bc-ebd2-421e-a559-bfa36fb4dbbb", "You may not claim remission on a Total Consolidation (VAR) Type F Declaration"));
			}
			Parent.Validation.ValidateCA_AuthorityNumber();
		}

		#endregion

		#region CheckCA_CasualImportDestinationProvince

		protected override void CheckCA_CasualImportDestinationProvince()
		{
			base.CheckCA_CasualImportDestinationProvince();

			var invoiceLine = InvoiceLine;
			if (!invoiceLine.CA_CasualImportDestinationProvince.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(invoiceLine.CA_CasualImportDestinationProvinceInfo);
			}

			if (invoiceLine.IsEffectiveCasualImport && invoiceLine.EffectiveCasualImportDestinationProvince.IsEmpty)
			{
				invoiceLine.CA_CasualImportDestinationProvinceInfo.AddError(NoCasualImportDestinationProvinceMessage);
			}

			if (invoiceLine.CA_IsCasualImport)
			{
				var defaultCasualImportDestinationProvince = invoiceLine.DefaultCasualImportDestinationProvince;
				if ((!defaultCasualImportDestinationProvince.IsEmpty && invoiceLine.CA_CasualImportDestinationProvince != defaultCasualImportDestinationProvince)
					|| (defaultCasualImportDestinationProvince.IsEmpty && invoiceLine.CA_CasualImportDestinationProvince != invoiceLine.InvoiceHeader.CA_CasualImportDestinationProvince))
				{
					invoiceLine.CA_CasualImportDestinationProvinceInfo.AddWarning(ImportAddInfoJobComInvoiceHeaderValidation.DestinationProvinceDoesNotMatch);
				}
			}
		}

		#endregion

		#region CheckCA_CFIACountryOfSource

		protected override void CheckCA_CFIACountryOfSource()
		{
			base.CheckCA_CFIACountryOfSource();
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_CFIACountryOfSourceInfo, Parent.Lookups.CFIACountryOfSourceList);
		}

		#endregion

		#region CheckCA_CFIAStateOfSource

		protected override void CheckCA_CFIAStateOfSource()
		{
			base.CheckCA_CFIAStateOfSource();
			if (Parent.CA_CFIACountryOfSource == Constants.CountryCodes.UnitedStates)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_CFIAStateOfSourceInfo, Parent.Lookups.CFIAStateOfSourceList);
			}
			else
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.CA_CFIAStateOfSourceInfo);
			}
		}

		#endregion

		#region CheckCA_OGDStatus

		protected override void CheckCA_OGDStatus()
		{
			base.CheckCA_OGDStatus();

			if (InvoiceLine.Validation is ImportJobComInvoiceLineValidation lineValidation)
			{
				lineValidation.ValidateCA_OGDStatus();
			}
		}

		#endregion

		#region CheckCA_TradeName

		protected override void CheckCA_TradeName()
		{
			base.CheckCA_TradeName();

			if (PGAInvoiceLineValidator.IsTradeNameRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_TradeNameInfo);
			}

			if (!Parent.CA_TradeName.IsEmpty)
			{
				var nrcanHeader = Parent.Parent?.NRCanPGAHeader;
				if (nrcanHeader != null
					&& nrcanHeader.CA_EXPProgramInd == YesNoList.Codes.Yes
					&& nrcanHeader.CA_AuthorizedParty.IsEmpty)
				{
					Parent.CA_TradeNameInfo.AddMessageError(Res.GetString("12718771-bf06-4ed9-b7b9-447ff8a8c31b", "Trade Name should keep empty as MFG/Authorized Party is not entered in PGA: {0}.", PGACodes.Descriptions.NRCan));
				}
			}

			var header = Parent.Parent?.HCPGAHeader;
			if (header != null && header.IsProgramEnabled(HCPGADepartmentCodes.Codes.CPR))
			{
				if (Parent.CA_TradeName.IsEmpty)
				{
					Parent.CA_TradeNameInfo.AddWarning(CPR_TradeNameShouldNotBeEmptyMessage);
				}
			}
		}

		#endregion

		#region CheckCA_ExpiryDate

		protected override void CheckCA_ExpiryDate()
		{
			base.CheckCA_ExpiryDate();

			var header = Parent.Parent?.HCPGAHeader;
			if (header != null && header.CA_CTOProgramInd == YesNoList.Codes.Yes)
			{
				if (header.CA_CategoryCTO == HCCategories.Codes.HC27 && Parent.CA_ExpiryDate.IsEmpty)
				{
					Parent.CA_ExpiryDateInfo.AddWarning(Res.GetString("489159cb-c006-4ba0-944c-b92e20c9fbd3", "It is strongly recommended to provide the Expiry Date for Tissues."));
				}

				if (Parent.CA_ExpiryDate.IsValid
					&& InvoiceLine.Declaration.JE_EntryAuthorisationDate.IsValid
					&& Parent.CA_ExpiryDate < InvoiceLine.Declaration.JE_EntryAuthorisationDate)
				{
					Parent.CA_ExpiryDateInfo.AddMessageError(Res.GetString("0e3f3f5c-bb3c-46c6-9ed1-1d4f44c42067", "Expiry Date is in the past before release is obtained."));
				}
			}
		}

		#endregion

		#region CheckCA_RN_NKSource

		protected override void CheckCA_RN_NKSource()
		{
			base.CheckCA_RN_NKSource();
			ListValidation.MessageErrorIfInvalidCode(Parent.CA_RN_NKSourceInfo, Parent.Lookups.DefaultOrigins);
		}

		#endregion

		#region CheckCA_StateOfSource

		protected override void CheckCA_StateOfSource()
		{
			base.CheckCA_StateOfSource();

			if (Parent.Parent?.CFIAPGAHeader is CFIAPGAHeader cfiaHeader && cfiaHeader.CA_AllProgramInd == YesNoList.Codes.Yes)
			{
				if (Parent.CA_RN_NKSource == Constants.CountryCodes.UnitedStates)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_StateOfSourceInfo);
				}
				ListValidation.MessageErrorIfInvalidCode(Parent.CA_StateOfSourceInfo, Parent.Lookups.StatesOfExport);
			}
		}

		#endregion

		#region CheckCA_ProductionDate

		protected override void CheckCA_ProductionDate()
		{
			base.CheckCA_ProductionDate();

			var header = Parent.Parent?.HCPGAHeader;
			if (header != null
				&& (header.IsProgramEnabled(HCPGADepartmentCodes.Codes.API)
					|| header.IsProgramEnabled(HCPGADepartmentCodes.Codes.HDR)
					|| header.IsProgramEnabled(HCPGADepartmentCodes.Codes.NHP)
					|| header.IsProgramEnabled(HCPGADepartmentCodes.Codes.VET)))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_ProductionDateInfo);
			}

			if (header != null && header.IsProgramEnabled(HCPGADepartmentCodes.Codes.CPR))
			{
				if (Parent.CA_ProductionDate.IsEmpty)
				{
					Parent.CA_ProductionDateInfo.AddWarning(CPR_ProductionDateShouldNotBeEmptyMessage);
				}
			}
		}

		#endregion

		#region CheckCA_RemissionType

		protected override void CheckCA_RemissionType()
		{
			base.CheckCA_RemissionType();
			ListValidation.MessageErrorIfInvalidCode(Parent.InvoiceLine.CA_RemissionTypeInfo);
			if (!Parent.CA_AuthorityNumber.IsEmpty && InvoiceLine.Declaration is JobDeclaration declaration && declaration.IsLVS)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_RemissionTypeInfo);
			}
		}

		#endregion

		internal static string NoCFIATariffMessage
		{
			get { return Res.GetString("f9606d78-3e7b-4dff-9b30-26c5baabf2f3", "CFIA details are NOT required with this HS code."); }
		}

		internal static string NoCasualImportDestinationProvinceMessage
		{
			get { return Res.GetString("c96cd06e-ecfd-4989-b8e6-f562e0047ca8", "The destination of casual import should not be empty. Please specify the destination province in corresponding Invoice Header or choose a value here to override."); }
		}

		internal static string ExistingManualDummyCasualImportMessage
		{
			get { return Res.GetString("0be23a4b-5238-4656-8f70-bc87490177c2", "There are some manual dummy casual import lines in the Invoice Lines Tab Page. Please remove them if you want to use the automatic calculation."); }
		}

		internal static string CPR_ProductionDateShouldNotBeEmptyMessage
		{
			get { return Res.GetString("560bfc8e-dd22-4946-be97-6ae748e3cfdb", "It is strongly recommended that the date on which the commodity was manufactured be provided to help trace products in case of recall."); }
		}

		internal static string CPR_TradeNameShouldNotBeEmptyMessage
		{
			get { return Res.GetString("e267431e-6d59-441e-a07a-b3ecfa0a7166", "It is strongly recommended that the Product Name of the commodity being imported be provided. While not required, this information may help to facilitate communication in case of a referral."); }
		}

		#region CheckCA_ModelYear

		protected override void CheckCA_ModelYear()
		{
			base.CheckCA_ModelYear();

			if (PGAInvoiceLineValidator.IsCA_ModelYearRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_ModelYearInfo);
			}

			if (PGAInvoiceLineValidator.IsValidCA_ModelYearRequired)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CA_ModelYearInfo, Parent.Lookups.ModelYearList);
			}
		}

		#endregion

		#region CheckCA_VINNumber

		protected override void CheckCA_VINNumber()
		{
			base.CheckCA_VINNumber();

			if (PGAInvoiceLineValidator.IsCA_VINNumberRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_VINNumberInfo);
			}
			CheckVINFormat();
		}

		void CheckVINFormat()
		{
			var parent = Parent;
			if (!parent.CA_VINNumber.IsEmpty)
			{
				if (!ZInt.TryParse(parent.CA_ModelYear, out ZInt modelYear) || modelYear >= 1981)
				{
					if (!Regex.IsMatch(parent.CA_VINNumber, @"^[a-zA-Z0-9]{17}$"))
					{
						parent.CA_VINNumberInfo.AddMessageError(Res.GetString("343e45f6-3f5a-4f37-b647-dc6c7c91c149", "VIN should be 17 alphanumeric."));
					}
				}
				else
				{
					if (!Regex.IsMatch(parent.CA_VINNumber, @"^[a-zA-Z0-9]{5,13}$"))
					{
						parent.CA_VINNumberInfo.AddMessageError(Res.GetString("9B3A8D99-2847-402D-887E-EFC454831B7C", "VIN should be 5-13 alphanumeric."));
					}
				}
			}
		}

		#endregion
	}
}
