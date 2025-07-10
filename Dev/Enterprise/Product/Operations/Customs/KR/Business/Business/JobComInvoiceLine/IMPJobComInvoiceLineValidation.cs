using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class IMPJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public IMPJobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}
		bool IsValidationModeSetForDetailedFTA => declaration?.IsValidationModeSetForDetailedFTA ?? false;
		bool IsValidationModeSetForD72 => declaration?.IsValidationModeSetForD72 ?? false;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCertificateOfOriginNo();
			ValidateCertificateOfOriginLineNo();
			ValidateCertificateOfOriginCriteriaCode();
			ValidateCertificateOfOriginIssueDate();
			ValidateCertificateOfOriginIssuingCountry();
			ValidateCertificateOfOriginAgencyName();
			ValidateCertificateOfOriginAreaName();
			ValidateCertificateOfOriginPersonName();
			ValidateCertificateOfOriginUQ();
			ValidateCertificateOfOriginStatus();
			ValidateCriteriaForDeterminingCountryOfOrigin();
			ValidateDutyReductionSeqNumber();
			ValidateDutyReductionItemNumber();
		}

		public void ValidateCertificateOfOriginNo()
		{
			ValidateCalculatedProperty(Parent.CertificateOfOriginNoInfo);
		}
		protected void CheckCertificateOfOriginNo()
		{
			if (IsValidationModeSetForDetailedFTA)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CertificateOfOriginNoInfo);
			}

			if (!Parent.CertificateOfOriginNo.IsEmpty)
			{
				if (Parent.CertificateOfOriginNo.Length < 5)
				{
					Parent.CertificateOfOriginNoInfo.AddMessageError(ReferenceNumberLengthError);
				}
			}
		}

		public void ValidateCertificateOfOriginLineNo()
		{
			ValidateCalculatedProperty(Parent.CertificateOfOriginLineNoInfo);
		}

		protected void CheckCertificateOfOriginLineNo()
		{
			if (IsValidationModeSetForDetailedFTA)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CertificateOfOriginLineNoInfo);
			}
		}

		public void ValidateCertificateOfOriginUQ()
		{
			ValidateCalculatedProperty(Parent.CertificateOfOriginUQInfo);
		}

		protected void CheckCertificateOfOriginUQ()
		{
			if (IsValidationModeSetForDetailedFTA)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CertificateOfOriginUQInfo);
			}
		}

		public void ValidateCertificateOfOriginCriteriaCode()
		{
			ValidateCalculatedProperty(Parent.CertificateOfOriginCriteriaCodeInfo);
			Parent.AddAllNotificationsFromCertificateOfOriginCSI_Procedure();
		}
		protected void CheckCertificateOfOriginCriteriaCode()
		{
			CheckReferenceNumberIsEmpty(Parent.CertificateOfOriginCriteriaCodeInfo);
		}

		public void ValidateCertificateOfOriginIssueDate()
		{
			ValidateCalculatedProperty(Parent.CertificateOfOriginIssueDateInfo);
		}
		protected void CheckCertificateOfOriginIssueDate()
		{
			CheckReferenceNumberIsEmpty(Parent.CertificateOfOriginIssueDateInfo);
			if (!Parent.CertificateOfOriginNo.IsEmpty)
			{
				var declarationDate = Parent.EffectiveAssessmentDate;
				if (declarationDate.IsValid)
				{
					if ((ZDate)Parent.CertificateOfOriginIssueDate > (ZDate)declarationDate)
					{
						Parent.CertificateOfOriginIssueDateInfo.AddMessageError(DeclarationDateError);
					}
				}
			}
		}

		public void ValidateCertificateOfOriginIssuingCountry()
		{
			ValidateCalculatedProperty(Parent.CertificateOfOriginIssuingCountryInfo);
			Parent.AddAllNotificationsFromCertificateOfOriginCSI_RN_NKCountryCode();
		}
		protected void CheckCertificateOfOriginIssuingCountry()
		{
			CheckReferenceNumberIsEmpty(Parent.CertificateOfOriginIssuingCountryInfo);
		}

		public void ValidateCertificateOfOriginAgencyName()
		{
			ValidateCalculatedProperty(Parent.CertificateOfOriginAgencyNameInfo);
		}
		protected void CheckCertificateOfOriginAgencyName()
		{
			CheckReferenceNumberIsEmpty(Parent.CertificateOfOriginAgencyNameInfo);
		}

		public void ValidateCertificateOfOriginAreaName()
		{
			ValidateCalculatedProperty(Parent.CertificateOfOriginAreaNameInfo);
		}
		protected void CheckCertificateOfOriginAreaName()
		{
			CheckReferenceNumberIsEmpty(Parent.CertificateOfOriginAreaNameInfo);
		}

		public void ValidateCertificateOfOriginPersonName()
		{
			ValidateCalculatedProperty(Parent.CertificateOfOriginPersonNameInfo);
		}
		protected void CheckCertificateOfOriginPersonName()
		{
			CheckReferenceNumberIsEmpty(Parent.CertificateOfOriginPersonNameInfo);
		}

		public void ValidateCertificateOfOriginStatus()
		{
			ValidateCalculatedProperty(Parent.CertificateOfOriginStatusInfo);
		}
		protected void CheckCertificateOfOriginStatus()
		{
			CheckReferenceNumberIsEmpty(Parent.CertificateOfOriginStatusInfo);
		}

		public void ValidateCriteriaForDeterminingCountryOfOrigin()
		{
			ValidateCalculatedProperty(Parent.CriteriaForDeterminingCountryOfOriginInfo);
		}
		protected void CheckCriteriaForDeterminingCountryOfOrigin()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CriteriaForDeterminingCountryOfOriginInfo);
		}

		void CheckReferenceNumberIsEmpty(ZPropertyInfo info)
		{
			if (Parent.CertificateOfOriginNo.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfIsEntered(info);
			}
		}

		protected override void CheckJI_CustomsSecondQuantity()
		{
			base.CheckJI_CustomsSecondQuantity();
			if (Parent.JI_CustomsSecondUnitQty.IsEmpty)
			{
				if (!Parent.JI_CustomsSecondQuantity.IsEmpty)
				{
					Parent.JI_CustomsSecondQuantityInfo.AddMessageError(Res.GetString("0B9FAFDC-DA97-4C1E-AA9A-C170122CF22B", "Second Customs Qty must be zero."));
				}
			}
			else
			{
				if (Parent.JI_CustomsSecondQuantity.IsEmpty)
				{
					Parent.JI_CustomsSecondQuantityInfo.AddMessageError(UQRequiredError);
				}
			}
			MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_CustomsSecondQuantityInfo);
		}

		protected override void CheckJI_ParentLine()
		{
			base.CheckJI_ParentLine();
			var procedureType = declaration?.JE_ProcedureType ?? ZString.Empty;
			if ((procedureType == ImportKindCodeList.Codes._29 || procedureType == ImportKindCodeList.Codes._36) && Parent.JI_ProductTypeCode == ProductOrMaterialCodeList.Codes.B)
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.JI_ParentLineInfo);
			}
			else
			{
				if (!Parent.JI_ParentLine.IsEmpty)
				{
					Parent.JI_ParentLineInfo.AddMessageError(Res.GetString("82EBA0E9-1B57-4F8A-9114-2F793B461A40", "Parent Line must be zero."));
				}
			}
			MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_ParentLineInfo);
		}

		protected override void CheckJI_NetWeight()
		{
			base.CheckJI_NetWeight();
			MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_NetWeightInfo);
		}

		protected override void CheckJI_CustomsQuantity()
		{
			base.CheckJI_CustomsQuantity();
			if (Parent.JI_CustomsUnitQty.IsEmpty)
			{
				if (!Parent.JI_CustomsQuantity.IsEmpty)
				{
					Parent.JI_CustomsQuantityInfo.AddMessageError(Res.GetString("755069E5-3690-4C2C-A320-766568BE1D0F", "Customs Quantity must be zero."));
				}
			}
			else if (Parent.JI_CustomsQuantity.IsEmpty)
			{
				Parent.JI_CustomsQuantityInfo.AddMessageError(UQRequiredError);
			}
			MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_CustomsQuantityInfo);
		}

		protected override void CheckJI_CustomsThirdQuantity()
		{
			base.CheckJI_CustomsThirdQuantity();
			if (!Parent.JI_CustomsThirdUnitQty.IsEmpty && Parent.JI_CustomsThirdQuantity.IsEmpty)
			{
				Parent.JI_CustomsThirdQuantityInfo.AddMessageError(UQRequiredError);
			}
			MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_CustomsThirdQuantityInfo);
		}

		protected override void CheckJI_CustomsFourthQuantity()
		{
			base.CheckJI_CustomsFourthQuantity();
			if (!Parent.JI_CustomsFourthUnitQty.IsEmpty && Parent.JI_CustomsFourthQuantity.IsEmpty)
			{
				Parent.JI_CustomsFourthQuantityInfo.AddMessageError(UQRequiredError);
			}
			MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_CustomsFourthQuantityInfo);
		}

		protected override void CheckJI_CountryOfOrigin()
		{
			base.CheckJI_CountryOfOrigin();
			if (Parent.JI_CountryOfOrigin != Core.Constants.CountryCodes.KoreaSouth && Parent.JI_CountryOfOrigin != Core.Constants.CountryCodes.KoreaNorth)
			{
				if (!(declaration?.JE_TradeIndicatorWithKP.IsEmpty ?? true))
				{
					Parent.JI_CountryOfOriginInfo.AddMessageError(Res.GetString("3B8C90BB-8682-467F-9B22-FCCE1A3D3C97", "if Country Of Origin is not 'KR' or 'KP', then South North Trade Y/N must be empty."));
				}
			}
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_CountryOfOriginInfo);
		}

		protected override void CheckJI_BrandName()
		{
			base.CheckJI_BrandName();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_BrandNameInfo);
			if (!Parent.JI_BrandName.IsEmpty)
			{
				ZString replacedBrandName = Parent.JI_BrandName.Replace(" ", "");
				if (replacedBrandName != replacedBrandName.KeepAlphanumericCharacters())
				{
					Parent.JI_BrandNameInfo.AddMessageError(BrandNameCheckError);
				}
			}
		}

		protected override void CheckJI_Model()
		{
			base.CheckJI_Model();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_ModelInfo);
		}

		protected override void CheckJI_InvoiceUQ()
		{
			base.CheckJI_InvoiceUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_InvoiceUQInfo);

			if (IsValidationModeSetForD72)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_InvoiceUQInfo);
			}
		}

		protected override void CheckJI_CustomsFifthQuantity()
		{
			if (IsValidationModeSetForDetailedFTA)
			{
				CompareValidation.MessageErrorIfLessThanOrEqualToZero(Parent.JI_CustomsFifthQuantityInfo);
			}
		}

		protected override void CheckJI_InvoiceQuantity()
		{
			base.CheckJI_InvoiceQuantity();

			MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_InvoiceQuantityInfo);
			if (IsValidationModeSetForD72)
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.JI_InvoiceQuantityInfo);
			}

			if (Parent.JI_InvoiceQuantity > 0 && Parent.JI_InvoiceUQ.IsEmpty)
			{
				Parent.JI_InvoiceQuantityInfo.AddMessageError(Res.GetString("8387EE0B-09A7-4848-AD4F-B9C892A98B17", "You have entered an invoice quantity. Please enter its unit too."));
			}

			if (Parent.JI_LinePrice > 0 && Parent.JI_InvoiceQuantity == 0)
			{
				Parent.JI_InvoiceQuantityInfo.AddMessageError(Res.GetString("71308B91-778E-4874-8120-0A20D896A7FF", "If the amount is greater than 0, the quantity must also be greater than 0."));
			}
		}

		protected override void CheckUnitPrice()
		{
			base.CheckUnitPrice();

			MandatoryValidation.MessageErrorIfIsNegative(Parent.UnitPriceInfo);
			if (IsValidationModeSetForD72)
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.UnitPriceInfo);
			}

			if (Parent.JI_LinePrice > 0 && Parent.UnitPrice == 0)
			{
				Parent.UnitPriceInfo.AddMessageError(Res.GetString("09382438-3D75-4C47-9DAC-B2F55A43938C", "If the amount is greater than 0, the 'Unit Price' must also be greater than 0."));
			}
		}

		protected override void CheckJI_LinePrice()
		{
			base.CheckJI_LinePrice();

			MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_LinePriceInfo);
			if (IsValidationModeSetForD72)
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.JI_LinePriceInfo);
			}
		}

		protected override void CheckJI_PrimaryPreference()
		{
			base.CheckJI_PrimaryPreference();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_PrimaryPreferenceInfo);
			if (Parent.JI_CoveredByCOOExporter)
			{
				var preferenceCode = RemoveTrailingDigits(Parent.JI_PrimaryPreference);
				if (ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Parent.Factory, preferenceCode, Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.KRFTE, Parent.EffectiveAssessmentDate) == null)
				{
					Parent.JI_PrimaryPreferenceInfo.AddMessageError(Res.GetString("7ee676b5-b9c8-49bd-bf9e-415790cf79ef", "The entered FTA is not covered by the COO Certification Exporter system."));
				}

				ZString RemoveTrailingDigits(ZString input)
				{
					var position = input.Length;
					while (position > 0 && char.IsDigit(input[position - 1]))
					{
						position--;
					}
					return input.Substring(0, position);
				}
			}
		}

		protected override void CheckJI_SecondaryPreference()
		{
			base.CheckJI_SecondaryPreference();
			if (DeclarationProcedureTypeCodeList.IsDutyReductionOrInstallmentNotApplicable(declaration?.JE_ProcedureType ?? ZString.Empty) || declaration.JE_PaymentMethod == PaymentMethodCodeList.Codes._33)
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.JI_SecondaryPreferenceInfo);
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.JI_SecondaryPreferenceInfo);

			var query = TariffView.Loader.GetEffectiveTariffFilter(Parent.Factory, Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.TariffTypes.DutyReductionExemption, Parent.JI_SecondaryPreference, Parent.EffectiveAssessmentDate);
			var secondaryPreference = Parent.Factory.Load<TariffView>(query).FirstOrDefault()?.Attributes?.Any(x => x.ZZ3_Name == Constants.ZZ.TariffAttributes.ReImport) ?? false;

			if (Parent.PreviousExpDecLineCollection.Count == 0 && secondaryPreference)
			{
				Parent.JI_SecondaryPreferenceInfo.AddMessageError(Res.GetString("914414A7-7B98-496C-A2CA-70994DED3FDA", "The entered duty reduction/exemption code is for re-Import of previously exported goods. You should enter re-Imported goods information."));
			}
			else if (Parent.PreviousExpDecLineCollection.Count != 0 && !secondaryPreference)
			{
				Parent.JI_SecondaryPreferenceInfo.AddMessageError(Res.GetString("292273B6-763D-40BF-B8D6-F1150852BBD4", "The entered duty reduction/exemption code is not relevant for re-Import of previously exported goods. You should not enter re-Imported goods information."));
			}
		}

		protected override void CheckJI_Tariff()
		{
			base.CheckJI_Tariff();
			if (Parent.UniversalTariff != null)
			{
				var importOGAConditions = Parent.UniversalTariff.Conditions
					.Where(x =>
					x.ZX1_IsImport
					&& x.ConditionType == Constants.ZZ.RefCusConditionType.OGA
					&& x.ZX1_StartDate <= Parent.EffectiveAssessmentDate
					&& x.ZX1_EndDate >= Parent.EffectiveAssessmentDate)
					.Select(x =>
						x.ConditionValues
						.FirstOrDefault(y => y.ValueType == Constants.ZZ.RefCusConditionValueType.OGARegulationNumber).ZX3_Value)
					.ToHashSet();

				var nonGAs = Parent.NonGADetailCollection.Select(x => x.CSI_Procedure).ToHashSet();
				var gaApprovals = Parent.GAApprovalDataCollection.Select(x => x.CSI_Procedure).ToHashSet();
				importOGAConditions.ExceptWith(nonGAs);
				importOGAConditions.ExceptWith(gaApprovals);

				if (importOGAConditions.Count > 0)
				{
					var regulation = importOGAConditions.Count > 1 ? (NoResString)"regulations" : (NoResString)"regulation";
					Parent.JI_TariffInfo.AddMessageError(string.Format(TariffInfoRegulationError, regulation, string.Join(", ", importOGAConditions)));
				}
			}
		}

		protected override void CheckJI_Description()
		{
			if (IsValidationModeSetForD72)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_DescriptionInfo);
			}
			else if (Parent.JI_Description.IsEmpty && Parent.JI_Ingredient.IsEmpty)
			{
				Parent.JI_DescriptionInfo.AddMessageError(Res.GetString("DEE953BF-BBF2-4CB4-97FB-7E64D735E810", "You must enter either 'Goods Description' or 'Ingredient'."));
			}
		}

		protected override void CheckJI_DomesticTaxCode()
		{
			var taxClassification = Parent.DomesticTaxClassification;
			if (Parent.JI_ProductTypeCode == ProductOrMaterialCodeList.Codes.A && taxClassification == ChargeTypeList.Codes.LiquorTax)
			{
				Parent.JI_DomesticTaxCodeInfo.AddMessageError(Res.GetString("33E5F9B3-FA4C-4113-9EB7-581BE3FCE166", "If 'Product Or Material Code' is 'A' then, Domestic Tax Classification can't be Liquor Tax."));
			}
			else if (Parent.JI_ProductTypeCode == ProductOrMaterialCodeList.Codes.B && (taxClassification == ChargeTypeList.Codes.TransportationTax || taxClassification == ChargeTypeList.Codes.SpecialConsumptionTax))
			{
				Parent.JI_DomesticTaxCodeInfo.AddMessageError(Res.GetString("A0B236A6-FADA-4535-AF9F-38E426F43CFA", "If 'Product Or Material Code' is 'B' then, Domestic Tax Classification can't be Special Consumption Tax or Transportation Tax."));
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_DomesticTaxCodeInfo, Parent.Lookups.DomesticTaxCodes);
			base.CheckJI_DomesticTaxCode();
		}

		protected override void CheckJI_DomesticTaxExemptionCode()
		{
			if ((declaration?.JE_PaymentMethod ?? ZString.Empty) == PaymentMethodCodeList.Codes._33)
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.JI_DomesticTaxExemptionCodeInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_DomesticTaxExemptionCodeInfo, Parent.Lookups.TaxExemptionCodes);
		}

		protected override void CheckJI_VATReductionCode()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_VATReductionCodeInfo, Parent.Lookups.VATReductionCodes);
		}

		protected override void CheckJI_COOLabelLocation()
		{
			base.CheckJI_COOLabelLocation();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_COOLabelLocationInfo);
		}

		protected override void CheckJI_COOLabelType()
		{
			base.CheckJI_COOLabelType();
			if (Parent.JI_COOLabelLocation.Contains(CountryOfOriginLabelLocationCodeList.Codes.Y) ||
				Parent.JI_COOLabelLocation.Contains(CountryOfOriginLabelLocationCodeList.Codes.B) ||
				Parent.JI_COOLabelLocation.Contains(CountryOfOriginLabelLocationCodeList.Codes.G))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_COOLabelTypeInfo);
				if (Parent.JI_COOLabelLocation.Contains(CountryOfOriginLabelLocationCodeList.Codes.B) &&
					(Parent.JI_COOLabelType.Contains(CountryOfOriginLabelTypeCodeList.Codes.A) ||
					Parent.JI_COOLabelType.Contains(CountryOfOriginLabelTypeCodeList.Codes.C) ||
					Parent.JI_COOLabelType.Contains(CountryOfOriginLabelTypeCodeList.Codes.E)))
				{
					Parent.JI_COOLabelTypeInfo.AddMessageError(Res.GetString("6BDB20AE-533D-4F06-841D-0AB5BEC86350", "If Country Of Origin Label Location is 'B', then Country Of Origin Label Type cannot be 'A', 'C' or 'E'"));
				}
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JI_COOLabelTypeInfo);
			}
		}

		protected override void CheckJI_BrandCode()
		{
			base.CheckJI_BrandCode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_BrandCodeInfo);
		}

		protected override void CheckJI_SpecificUseProductType()
		{
			base.CheckJI_SpecificUseProductType();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_SpecificUseProductTypeInfo);
			if (IsValidationModeSetFor5FN && IsPostClearanceProcedureRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_SpecificUseProductTypeInfo);
			}
		}

		protected override void CheckJI_ProductTypeCode()
		{
			base.CheckJI_ProductTypeCode();
			var procedureType = declaration?.JE_ProcedureType ?? ZString.Empty;
			if (procedureType == ImportKindCodeList.Codes._29 || procedureType == ImportKindCodeList.Codes._36)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_ProductTypeCodeInfo);
			}
			else
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.JI_ProductTypeCodeInfo);
			}
		}

		protected override void CheckJI_DrawbackUQ()
		{
			base.CheckJI_DrawbackUQ();
			if (Parent.JI_DrawbackQuantity > 0)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_DrawbackUQInfo);
			}
			else
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.JI_DrawbackUQInfo);
			}
		}

		protected override void CheckJI_DrawbackQuantity()
		{
			base.CheckJI_DrawbackQuantity();
			if (Parent.JI_DrawbackUQ.IsEmpty)
			{
				if (!Parent.JI_DrawbackQuantity.IsEmpty)
				{
					Parent.JI_DrawbackQuantityInfo.AddMessageError(Res.GetString("478BE8EC-71BB-4016-BD44-1904DCE5AEAD", "Drawback Quantity must be zero."));
				}
			}
			else
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.JI_DrawbackQuantityInfo);
			}
			MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_DrawbackQuantityInfo);
		}

		protected override void CheckJI_PostClearanceProcedureGA1()
		{
			base.CheckJI_PostClearanceProcedureGA1();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_PostClearanceProcedureGA1Info);
		}

		protected override void CheckJI_PostClearanceProcedureGA2()
		{
			base.CheckJI_PostClearanceProcedureGA2();
			if (Parent.JI_PostClearanceProcedureGA1.IsEmpty && !Parent.JI_PostClearanceProcedureGA2.IsEmpty)
			{
				Parent.JI_PostClearanceProcedureGA2Info.AddMessageError(Res.GetString("91944988-FD3F-4316-B801-6FB3A8C24F09", "You should enter a value here only if you have the first post clearance procedure government agency."));
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_PostClearanceProcedureGA2Info);
		}

		protected override void CheckJI_PostClearanceProcedureGA3()
		{
			base.CheckJI_PostClearanceProcedureGA3();
			if (Parent.JI_PostClearanceProcedureGA2.IsEmpty && !Parent.JI_PostClearanceProcedureGA3.IsEmpty)
			{
				Parent.JI_PostClearanceProcedureGA3Info.AddMessageError(Res.GetString("25419001-ABD8-428F-9524-B7C55F608D39", "You should enter a value here only if you have the second post clearance procedure government agency."));
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_PostClearanceProcedureGA3Info);
		}

		protected override void CheckJI_MightRequireInspection()
		{
			base.CheckJI_MightRequireInspection();
			var customsBrokerCommentCode3 = declaration?.DeclarationRefs?.AsEnumerable().FirstOrDefault(x => x.J3_ReferenceType == Constants.AdditionalInformationStatementCodes_929._259)?.J3_ReferenceNumber ?? ZString.Empty;
			if (CustomsBrokerInspectionOpinionStatementTypeCodeList.IsInspectionRequired(customsBrokerCommentCode3))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_MightRequireInspectionInfo);
			}
			else
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.JI_MightRequireInspectionInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_MightRequireInspectionInfo);
		}

		protected override void CheckJI_COOExemptionReason()
		{
			base.CheckJI_COOExemptionReason();
			if (Parent.JI_COOLabelLocation.Contains(CountryOfOriginLabelLocationCodeList.Codes.E))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_COOExemptionReasonInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JI_COOExemptionReasonInfo);
			}
		}

		protected override void CheckJI_CourierCargoSelectivityIndicator()
		{
			base.CheckJI_CourierCargoSelectivityIndicator();

			var courierCompanyID = declaration?.Forwarder?.GetRegistrationNumber(Constants.IdentificationType.CourierCompanyID) ?? ZString.Empty;

			if (courierCompanyID.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.JI_CourierCargoSelectivityIndicatorInfo);
			}
			else
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_CourierCargoSelectivityIndicatorInfo);
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.JI_CourierCargoSelectivityIndicatorInfo);
		}

		protected override void CheckJI_AdditionalDutyRate()
		{
			base.CheckJI_AdditionalDutyRate();
			if (!Parent.JI_AdditionalDutyType.IsEmpty)
			{
				if (Parent.JI_AdditionalDutyRate <= 0)
				{
					Parent.JI_AdditionalDutyRateInfo.AddMessageError(Res.GetString("CA5FAB1A-AB23-4BA6-B070-D93717D32A1E", "Please enter an 'Additional Duty Rate' greater than 0."));
				}
			}
		}

		protected override void CheckJI_AdditionalDutyType()
		{
			base.CheckJI_AdditionalDutyType();

			ListValidation.MessageErrorIfInvalidCode(Parent.JI_AdditionalDutyTypeInfo);
		}

		protected override void CheckJI_InstallmentCode()
		{
			base.CheckJI_InstallmentCode();
			if (DeclarationProcedureTypeCodeList.IsDutyReductionOrInstallmentNotApplicable(declaration?.JE_ProcedureType ?? ZString.Empty) || declaration.JE_PaymentMethod == PaymentMethodCodeList.Codes._33)
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.JI_InstallmentCodeInfo);
			}
		}

		protected override void CheckJI_Ingredient()
		{
			base.CheckJI_Ingredient();
			if (Parent.JI_Description.IsEmpty && Parent.JI_Ingredient.IsEmpty)
			{
				Parent.JI_IngredientInfo.AddMessageError(Res.GetString("7A545930-CE5C-4C55-92E7-B8C052AD2E7D", "You must enter either 'Goods Description' or 'Ingredient'."));
			}
		}

		protected override void CheckJI_JurisdictionalCusOffice()
		{
			base.CheckJI_JurisdictionalCusOffice();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_JurisdictionalCusOfficeInfo);
			if (IsValidationModeSetFor5FN && IsPostClearanceProcedureRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_JurisdictionalCusOfficeInfo);
			}
		}

		protected override void CheckJI_SpecificUseCodeDescription()
		{
			base.CheckJI_SpecificUseCodeDescription();
			if (IsValidationModeSetFor5FN && IsPostClearanceProcedureRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_SpecificUseCodeDescriptionInfo);
			}
		}

		protected override void CheckJI_DutyRateSelection()
		{
			base.CheckJI_DutyRateSelection();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_DutyRateSelectionInfo);
			if (Parent.Lookups.DutyRateSelectionList.Count > 1)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_DutyRateSelectionInfo);
			}
		}

		protected override void CheckJI_PCProcedure()
		{
			base.CheckJI_PCProcedure();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_PCProcedureInfo);

			if (IsValidationModeSetFor5FN && Parent.IsSubjectTo5FN)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_PCProcedureInfo);

				if (IsPostClearanceProcedureRequired)
				{
					var importer = OrgHeaderWrapper.New(declaration.ImporterAddress?.Header);

					if (importer?.ZO_TypeOfBusiness.IsEmpty ?? true)
					{
						Parent.JI_PCProcedureInfo.AddMessageError(Res.GetString("92FF7CCD-A8AE-4BCC-9C1D-B644C890E254", "If Post Clearance YN is Y, you must enter the Importer's type of business. Please press F3 in the Importer Organization field and set the Type of Business on the Organization form > Details > Config > Korea."));
					}
					var consigneeAddress = Parent.ConsigneeAddress;
					if (consigneeAddress?.OA_Address1.IsEmpty ?? true)
					{
						Parent.JI_PCProcedureInfo.AddMessageError(Res.GetString("44E4100D-07B2-4E07-86CA-AC64473E9F70", "If Post Clearance YN is Y, you must enter the Goods Location's address. Please press F3 in the Goods Location field and enter the address on the Organization form > Addresses."));
					}
					if (consigneeAddress?.OA_PostCode.IsEmpty ?? true)
					{
						Parent.JI_PCProcedureInfo.AddMessageError(Res.GetString("099FE0DB-A29C-4F91-B57F-F2CDB6FE362D", "If Post Clearance YN is Y, you must enter the Goods Location's postcode. Please press F3 in the Goods Location field and enter the postcode on the Organization form > Addresses."));
					}
					if (consigneeAddress?.OA_Phone.IsEmpty ?? true)
					{
						Parent.JI_PCProcedureInfo.AddMessageError(Res.GetString("2E55FA48-E3D9-45B8-9FF7-990D75B425DF", "If Post Clearance YN is Y, you must enter the Goods Location's phone number. Please press F3 in the Goods Location field and enter the phone number on the Organization form > Addresses."));
					}
				}
			}
		}

		protected override void CheckJI_ScheduledReExportCustomsOffice()
		{
			base.CheckJI_ScheduledReExportCustomsOffice();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_ScheduledReExportCustomsOfficeInfo);
		}

		protected override void CheckJI_RN_NKReExportDestinationCountry()
		{
			base.CheckJI_RN_NKReExportDestinationCountry();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_RN_NKReExportDestinationCountryInfo);
		}

		public void ValidateDutyReductionSeqNumber()
		{
			ValidateCalculatedProperty(Parent.DutyReductionSeqNumberInfo);
		}

		protected void CheckDutyReductionSeqNumber()
		{
			if (!Parent.DutyReductionGroupNumber.IsEmpty && IsValidationModeSetFor5FN)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.DutyReductionSeqNumberInfo);
			}
		}

		public void ValidateDutyReductionItemNumber()
		{
			ValidateCalculatedProperty(Parent.DutyReductionItemNumberInfo);
		}

		protected void CheckDutyReductionItemNumber()
		{
			if (!Parent.DutyReductionGroupNumber.IsEmpty && IsValidationModeSetFor5FN)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.DutyReductionItemNumberInfo);
			}
		}

		protected override void CheckJI_CoveredByCOOExporter()
		{
			base.CheckJI_CoveredByCOOExporter();
			var supplier = Parent.InvoiceHeader.Supplier;
			if (supplier != null && Parent.JI_CoveredByCOOExporter && string.IsNullOrEmpty(Parent.CountryOfOriginExporterNumber))
			{
				Parent.JI_CoveredByCOOExporterInfo.AddMessageError(Res.GetString("8658e997-32d2-499f-b559-370d3176f776", "The entered supplier does not have a registration number of type 10. Please press F3 and add a number of type '10' in Config > Registration Numbers/Codes on the Organization form."));
			}
		}

		public static string TariffInfoRegulationError => Res.GetString("B548DF51-373D-4135-BD67-31582D4ED9BD", "This tariff has OGA requirements for the {0} {1} but the details are not in Approval & Re-Import > Approval Document or Non-Approval Document.");
		public static string UQRequiredError => Res.GetString("BA5B937A-96AF-4358-AEDC-5B969ACCCAD7", "Customs Quantity must be entered if UQ is not empty.");
		public static string ReferenceNumberLengthError => Res.GetString("8BE800A2-61D9-4A16-85F6-964CEFBFF137", "Length of the value must be longer than or equal to 5.");
		public static string DeclarationDateError => Res.GetString("C8D33164-4894-4650-AD9B-6AECDCCEEC82", "It must be earlier than or equal to the Declaration Date.");
		public static string BrandNameCheckError => Res.GetString("A4044EF8-2E0F-4EF1-8B59-EC86AA0F73C5", "Brand Name should only contain space and alphanumeric characters.");

		JobDeclaration declaration => Parent.Declaration;
		bool IsValidationModeSetFor5FN => declaration?.IsValidationModeSetFor5FN ?? false;
		bool IsPostClearanceProcedureRequired => Parent.JI_PCProcedure == YesNoList.Codes.Yes;
	}
}
