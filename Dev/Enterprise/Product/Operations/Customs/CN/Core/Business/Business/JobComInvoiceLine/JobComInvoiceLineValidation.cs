using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.CN.Business.Constants;
using NotificationType = CargoWise.EntityFramework.NotificationType;

namespace Enterprise.Customs.CN.Business
{
	public class JobComInvoiceLineValidation : AutoCNJobComInvoiceLineValidation
	{
		public JobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

		internal IValidationModeProvider ValidationModeProvider => Parent.Declaration;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateTradeAgreementCode();
			ValidateCertificateOfOrigin();
			ValidateItemNoOnCertOfOrigin();
			ValidateTradeUnitPrice();
			ValidateCargoAttributesAsString();
			ValidateDangerousGoodsDGSubs();
			ValidateCIQIngredient();
			ValidateManufactureDatesAsString();
		}

		protected override void CheckJI_CEI()
		{
			Parent.JI_CEIInfo.AddNotificationIfNotEntered(ValidationModeProvider);
		}

		protected override void CheckJI_StateOrRegionOfOrigin()
		{
			base.CheckJI_StateOrRegionOfOrigin();

			if (Parent?.Declaration?.WillGenerateEnteringEntry ?? false)
			{
				Parent.JI_StateOrRegionOfOriginInfo.AddNotificationIfInvalidCode(ValidationModeProvider);
			}
		}

		protected override void CheckJI_Tariff()
		{
			base.CheckJI_Tariff();
			var targetInfo = Parent.JI_TariffInfo;
			var tariff = Parent.UniversalTariff;

			if (tariff != null)
			{
				ValidateJI_NameOfGoods();
				ValidateJI_NameOfGoods2();
				ValidateXC_GoodsSpecModel();
				ValidateXC_GoodsSpecModel2();

				if (Parent?.UniversalDutyRate?.ZZ2_RateFormula.Contains(Constants.UniversalReferenceConstants.RateFormulaCountrySpecificValue.CVInUSD, StringComparison.Ordinal) ?? false)
				{
					var usdCurrency = Parent.Factory.GetCachedValue("RefCurrency_USD", () => RefCurrency.LoadFromCurrencyCode(Parent.Factory, Core.Constants.CurrencyCodes.UnitedStates));
					var rate = ((RefCurrencyCurrencyConverter)Parent.CurrencyConverter).GetExchangeRateObject(usdCurrency);
					if (rate == null)
					{
						targetInfo.AddNotification(Res.GetString("49F6B8F2-B5E9-47DF-931E-52C2F85F67C9", "There is no valid customs exchange rate for USD. It's required for the calculation of Duty for this Tariff."), ValidationModeProvider);
					}
				}

				if (Parent.IsImport && tariff.HasCommodityTypeUME() && (Parent.NameOfGoodsSeemsTobeUsedProduct || Parent.CargoAttributes.IsUsedProductAttributeSelected()))
				{
					var supportDocDefinition = CNRefCusCodeListLoader.GetRequiredDocuments(Parent.Factory, Constants.DocumentCodes.ImportLicense, Parent.EffectiveAssessmentDate);
					var hasSupportDoc = Parent.CusSupportingDocuments?.IsDocumentProvided(Constants.DocumentCodes.ImportLicense) ?? false;
					if (supportDocDefinition != null && !hasSupportDoc)
					{
						targetInfo.AddWarning(Res.GetString("0CB31D26-62F1-45BF-9690-9F37BE8D15E9", "The goods seems to be used mechanical and electrical products, supporting document {0} is required.", supportDocDefinition.ZZD_Description));
					}
				}
			}
		}

		protected override ZString UniversalTariffNotExistedError => ListValidation.InvalidCodeMessage.ToString();

		protected override ZString AdditionalCodeCaption => Res.GetString("29F2350C-A08F-4A59-B272-20175F110130", "Preferential code");

		protected override INotificationType NotificationTypeForConditionsCheck => (Parent.EntryInstruction?.IsGeneralTrade ?? ZBool.False) ? ValidationModeProvider.GetNotificationType() : NotificationType.Warning;

		protected override void CheckJI_Description()
		{
			base.CheckJI_Description();
			MandatoryValidation.WarnIfNotEntered(Parent.JI_DescriptionInfo);
		}

		protected override void CheckJI_InvoiceUQ()
		{
			base.CheckJI_InvoiceUQ();
			Parent.JI_InvoiceUQInfo.AddNotificationIfInvalidCode(ValidationModeProvider);
		}

		protected override void CheckJI_CustomsQuantity()
		{
			base.CheckJI_CustomsQuantity();
			Parent.JI_CustomsQuantityInfo.AddNotificationIfNotEntered(ValidationModeProvider);
		}

		protected override void CheckJI_CustomsUnitQty()
		{
			base.CheckJI_CustomsUnitQty();
			Parent.JI_CustomsUnitQtyInfo.AddNotificationIfInvalidCodeOrEmpty(ValidationModeProvider);
		}

		protected override void CheckJI_Procedure()
		{
		}

		protected override void CheckJI_CustomsSecondQuantity()
		{
			base.CheckJI_CustomsSecondQuantity();
			if (!Parent.JI_CustomsSecondUnitQty.IsEmpty)
			{
				Parent.JI_CustomsSecondQuantityInfo.AddNotificationIfNotEntered(ValidationModeProvider);
			}
		}

		protected override void CheckJI_CustomsSecondUnitQty()
		{
			base.CheckJI_CustomsSecondUnitQty();
			if (!Parent.JI_CustomsSecondUnitQty.IsEmpty)
			{
				Parent.JI_CustomsSecondUnitQtyInfo.AddNotificationIfInvalidCode(ValidationModeProvider);
			}
		}

		protected override void CheckJI_CountryOfOrigin()
		{
			base.CheckJI_CountryOfOrigin();
			Parent.JI_CountryOfOriginInfo.AddNotificationIfInvalidCodeOrEmpty(ValidationModeProvider);

			if (!Parent.JI_CountryOfOrigin.IsEmpty)
			{
				if (Parent.JI_CIQCountryOfOrigin.IsEmpty)
				{
					Parent.JI_CountryOfOriginInfo.AddNotification(Res.GetString("569A0094-E59D-4926-9F4E-9251E553989E", "Could not find an ISO Alpha-3 Code for '{0}', please check the Country Code you entered.", Parent.JI_CountryOfOrigin), ValidationModeProvider);
				}
				ValidateTradeAgreementCode();
			}
		}

		protected override void CheckJI_RN_NKCountryOfExport()
		{
			base.CheckJI_RN_NKCountryOfExport();
			Parent.JI_RN_NKCountryOfExportInfo.AddNotificationIfInvalidCodeOrEmpty(ValidationModeProvider);
		}

		protected override void CheckJI_OA_ManufacturerAddress()
		{
			base.CheckJI_OA_ManufacturerAddress();

			OrgAddress manufacturerAddress;
			JobDeclaration parentDec = Parent.Declaration;
			if (Parent.CIQRequires && parentDec != null && (manufacturerAddress = Parent.ManufacturerAddress) != null)
			{
				var targetInfo = Parent.JI_OA_ManufacturerAddressInfo;

				if (!parentDec.IsImport)
				{
					ValidationHelper.CheckRegNumbers(manufacturerAddress.Header, OrgCusCode.ChinaCodeTypes.CIQ, targetInfo, ValidationModeProvider.GetNotificationType());
				}

				var chineseLan = Core.Constants.Languages.ChineseSimplified;
				if (manufacturerAddress.Language != chineseLan && !manufacturerAddress.TranslatedAddresses.Any(address => address.Language == chineseLan))
				{
					targetInfo.AddNotification(Res.GetString("3f87d0ee-1483-4b6d-8785-7b891dd5e513", "The Manufacturer Address should have a Simplified Chinese translation."), ValidationModeProvider);
				}
			}
		}

		protected override void CheckJI_Weight()
		{
			base.CheckJI_Weight();
			Parent.JI_WeightInfo.AddNotificationIfLessThanOrEqualToZero(ValidationModeProvider);
		}

		protected override void CheckJI_NetWeight()
		{
			base.CheckJI_NetWeight();
			Parent.JI_NetWeightInfo.AddNotificationIfLessThanOrEqualToZero(ValidationModeProvider);
		}

		protected override void CheckJI_WeightUQ()
		{
			base.CheckJI_WeightUQ();
			Parent.JI_WeightUQInfo.AddNotificationIfInvalidCodeOrEmpty(Parent.Lookups.WeightUQList, ValidationModeProvider);
		}

		protected override void CheckJI_NetWeightUQ()
		{
			base.CheckJI_NetWeightUQ();
			Parent.JI_NetWeightUQInfo.AddNotificationIfInvalidCodeOrEmpty(Parent.Lookups.WeightUQList, ValidationModeProvider);
		}

		protected override void CheckJI_LinePrice()
		{
			base.CheckJI_LinePrice();
			Parent.JI_LinePriceInfo.AddNotificationIfIsZero(ValidationModeProvider);
			Parent.JI_LinePriceInfo.AddNotificationIfIsNegative(ValidationModeProvider);
		}

		protected override void CheckJI_InvoiceQuantity()
		{
			base.CheckJI_InvoiceQuantity();
			Parent.JI_InvoiceQuantityInfo.AddNotificationIfIsNegative(ValidationModeProvider);
		}

		protected override void CheckJI_PrimaryPreference()
		{
			base.CheckJI_PrimaryPreference();
			var targetInfo = Parent.JI_PrimaryPreferenceInfo;

			var isImport = Parent.IsImport;
			if (isImport)
			{
				targetInfo.AddNotificationIfNotEntered(ValidationModeProvider);
			}

			var universalTariff = Parent.UniversalTariff;
			var selectedRate = Parent.UniversalDutyRate;
			if (selectedRate != null && universalTariff != null)
			{
				if (Parent.JI_PrimaryPreference != Constants.PrimaryPreferenceCodes.Normal && Parent.JI_PrimaryPreference != Constants.PrimaryPreferenceCodes.MostFavouredNations)
				{
					if (int.TryParse(selectedRate.ZZ2_RateFormulaDerivedFrom, out var currentRate) && currentRate > 0)
					{
						var mfnRateSelectionCriteria = Parent.GetSpecificRateSelectionCriteria(Universal.Constants.RateTypes.Duty, Constants.UniversalReferenceConstants.RefCusRateCodes.CustomsDuty, Constants.PrimaryPreferenceCodes.MostFavouredNations, ZString.Empty);
						var mfnRateView = universalTariff.GetApplicableRate(mfnRateSelectionCriteria);
						var mfnRateString = mfnRateView?.ZZ2_RateFormulaDerivedFrom ?? ZString.Empty;
						if (int.TryParse(mfnRateString, out var mfnRate) && mfnRate < currentRate)
						{
							targetInfo.AddWarning(Res.GetString(
								"1A2360D3-4881-48DF-B6BB-A4C8435935DA",
								"Most favored nation rate ({0}%) is lower than the applicable rate for the selected Preference. It is recommended to select ‘MFN’.", mfnRateString));
						}
					}
				}
			}

			ValidateCertificateOfOriginType();
			ValidateCertificateOfOrigin();
			ValidateCertificateOfOriginCountry();
			ValidateTradeAgreementCode();
		}

		public void ValidateDangerousGoodsDGSubs()
		{
			ValidateCalculatedProperty(Parent.DangerousGoodsDGSubsInfo);
		}

		protected void CheckDangerousGoodsDGSubs()
		{
			var invoiceLine = Parent;

			if (invoiceLine.DangerousGoodsDGSubs.IsEmpty && invoiceLine.GoodsIsDangerousChemical)
			{
				invoiceLine.DangerousGoodsDGSubsInfo.AddWarning(Res.GetString("9AAD4EEB-71A5-4DEB-B956-13605FFF6F15",
					"UNDG is required for the goods seem to be dangerous chemical."));
			}

			var cargoAttributes = invoiceLine.CargoAttributes;
			if (cargoAttributes.IsProvidedAny(CargoAttributeList.Codes._31, CargoAttributeList.Codes._32) && !invoiceLine.JI_NonDangerousChemicalFlag)
			{
				invoiceLine.DangerousGoodsDGSubsInfo.AddNotificationIfNotEntered(ValidationModeProvider);
			}
		}

		public void ValidateTradeAgreementCode()
		{
			ValidateCalculatedProperty(Parent.TradeAgreementCodeInfo);
		}

		protected void CheckTradeAgreementCode()
		{
			var targetInfo = Parent.TradeAgreementCodeInfo;
			if (Parent.IsCertificateOfOriginRequired || CertificateOfOriginEntered)
			{
				targetInfo.AddNotificationIfInvalidCodeOrEmpty(ValidationModeProvider);
				CheckSelectedTradeAgreementCode();
			}

			void CheckSelectedTradeAgreementCode()
			{
				var selectedTradeAgreementCode = Parent.TradeAgreementCode;
				if (Parent.CertificateOfOriginType == CertificateOfOriginTypeList.Codes.DeclarationOfOrigin
					&& selectedTradeAgreementCode != TradeAgreementCodes.Codes.LDC && !selectedTradeAgreementCode.IsEmpty && !targetInfo.HasNotifications())
				{
					var tradeAgreementCodeList = CNRefCusCodeListTypes.GetTradeAgreementCodesSupportDeclarationOfOrigin(Parent.Factory, Parent.EffectiveAssessmentDate);
					if (!tradeAgreementCodeList.ContainsCode(selectedTradeAgreementCode))
					{
						targetInfo.AddNotification(Res.GetString("D23B2252-1C78-4AB7-B156-86C2F2EB6035", "This Preferential Code does not support Declaration of Origin, please check the COO Type and Preferential Code before submitting."), ValidationModeProvider);
					}
				}
			}
		}

		public void ValidateFormulaPricingRecordNumber()
		{
			ValidateCalculatedProperty(Parent.FormulaPricingRecordNumberInfo);
		}

		protected void CheckFormulaPricingRecordNumber()
		{
			var targetInfo = Parent.FormulaPricingRecordNumberInfo;
			if (Parent.FormulaPricingRecordNumber.ContainsAnyChar(" "))
			{
				targetInfo.AddNotification(Res.GetString("BAF6A421-F0FE-46AF-BA94-7D8A20661280", "Reference Number cannot contain white space."), ValidationModeProvider);
			}
		}

		public void ValidateCargoAttributesAsString()
		{
			ValidateCalculatedProperty(Parent.CargoAttributesAsStringInfo);
		}

		protected void CheckCargoAttributesAsString()
		{
			var targetInfo = Parent.CargoAttributesAsStringInfo;
			if (Parent.CIQRequires)
			{
				targetInfo.AddNotificationIfNotEntered(ValidationModeProvider);
				Parent.CargoAttributes.ValidateSelectedOptionAsString();
			}
		}

		public void ValidateCertificateOfOrigin()
		{
			ValidateCalculatedProperty(Parent.CertificateOfOriginInfo);
		}

		bool CertificateOfOriginEntered => !Parent.CertificateOfOrigin.IsEmpty;

		protected void CheckCertificateOfOrigin()
		{
			var targetInfo = Parent.CertificateOfOriginInfo;
			CusSupportingDocumentValidation.CheckCertificateOfOriginIsRequired(targetInfo, Parent.CertificateOfOrigin, Parent.IsCertificateOfOriginRequired && !Parent.CertificateOfOriginTypeIsX, Parent);
			if (Parent.IsCertificateOfOriginRequired || CertificateOfOriginEntered)
			{
				CusSupportingDocumentValidation.CheckCertificateOfOriginNumber(targetInfo, Parent);
				CusSupportingDocumentValidation.CheckCusSupportingDocumentsAcrossInstruction(Parent.CertificateOfOriginDocument, targetInfo);
			}
		}

		public void ValidateItemNoOnCertOfOrigin()
		{
			ValidateCalculatedProperty(Parent.ItemNoOnCertOfOriginInfo);
		}

		protected void CheckItemNoOnCertOfOrigin()
		{
			var parent = Parent;

			if (Parent.IsCertificateOfOriginRequired || CertificateOfOriginEntered)
			{
				parent.ItemNoOnCertOfOriginInfo.AddNotificationIfIsNegative(ValidationModeProvider);
				if (!Parent.CertificateOfOriginTypeIsX)
				{
					parent.ItemNoOnCertOfOriginInfo.AddNotificationIfIsZero(ValidationModeProvider);
				}
			}
		}

		public void ValidateCertificateOfOriginCountry()
		{
			ValidateCalculatedProperty(Parent.CertificateOfOriginCountryInfo);
		}

		protected void CheckCertificateOfOriginCountry()
		{
			var parent = Parent;
			if (Parent.IsCertificateOfOriginRequired || CertificateOfOriginEntered)
			{
				parent.CertificateOfOriginCountryInfo.AddNotificationIfInvalidCodeOrEmpty(ValidationModeProvider);
			}
		}

		public void ValidateCertificateOfOriginType()
		{
			ValidateCalculatedProperty(Parent.CertificateOfOriginTypeInfo);
		}

		protected void CheckCertificateOfOriginType()
		{
			var parent = Parent;

			if (Parent.IsCertificateOfOriginRequired || CertificateOfOriginEntered)
			{
				parent.CertificateOfOriginTypeInfo.AddNotificationIfInvalidCodeOrEmpty(ValidationModeProvider);
				ValidateCertificateOfOrigin();
			}
		}

		public void ValidateTradeUnitPrice()
		{
			ValidateCalculatedProperty(Parent.TradeUnitPriceInfo);
		}

		protected void CheckTradeUnitPrice()
		{
			Parent.TradeUnitPriceInfo.AddNotificationIfIsNegative(ValidationModeProvider);
			Parent.TradeUnitPriceInfo.AddNotificationIfIsZero(ValidationModeProvider);
		}

		public void ValidateXC_GoodsSpecModel()
		{
			ValidateCalculatedProperty(Parent.XC_GoodsSpecModelInfo);
		}

		protected void CheckXC_GoodsSpecModel()
		{
			var invoiceLine = Parent;
			if (invoiceLine.EntryInstruction == null || invoiceLine.EntryInstruction.CEI_ManualNo.IsEmpty)
			{
				invoiceLine.AdditionalInformationHelper.ValidateGoodsSpecModel();
			}
		}

		public void ValidateXC_GoodsSpecModel2()
		{
			ValidateCalculatedProperty(Parent.XC_GoodsSpecModel2Info);
		}

		protected void CheckXC_GoodsSpecModel2()
		{
			var invoiceLine = Parent;
			if (invoiceLine.ChildInstruction != null && invoiceLine.ChildInstruction.CEI_ManualNo.IsEmpty)
			{
				invoiceLine.AdditionalInformation2Helper.ValidateGoodsSpecModel();
			}
		}

		protected override bool EitherCCOrTariffRequiredForAutoCreationOfProduct => false;

		public void ValidateCIQIngredient()
		{
			ValidateCalculatedProperty(Parent.CIQIngredientInfo);
		}

		protected void CheckCIQIngredient()
		{
			var parent = Parent;
			if (parent.CIQRequires)
			{
				AddWarningWhenNotEqualExtracedCIQDetails(parent.CIQIngredientInfo, parent.CIQIngredient, parent.AdditionalInformationHelper.ExtractedIngredient);
			}
		}

		public void ValidateManufactureDatesAsString()
		{
			ValidateCalculatedProperty(Parent.ManufactureDatesAsStringInfo);
		}

		protected void CheckManufactureDatesAsString()
		{
			var parent = Parent;
			if (parent.CIQRequires)
			{
				AddWarningWhenNotEqualExtracedCIQDetails(parent.ManufactureDatesAsStringInfo, parent.ManufactureDatesAsString, parent.AdditionalInformationHelper.ExtractedManufactureDates.DistinctSortAndJoinForDisplay(format: x => x.ToString("yyyyMMdd")));
			}
		}

		protected override void CheckJI_NDescription()
		{
			base.CheckJI_NDescription();
			var parent = Parent;
			if (parent.CIQRequires)
			{
				AddWarningWhenNotEqualExtracedCIQDetails(parent.JI_NDescriptionInfo, parent.JI_NDescription, parent.AdditionalInformationHelper.ExtractedSpecification);
			}
		}

		protected override void CheckJI_BrandName()
		{
			base.CheckJI_BrandName();
			var parent = Parent;
			if (parent.CIQRequires)
			{
				AddWarningWhenNotEqualExtracedCIQDetails(parent.JI_BrandNameInfo, parent.JI_BrandName, parent.AdditionalInformationHelper.ExtractedBrand);
			}
		}

		protected override void CheckJI_Model()
		{
			base.CheckJI_Model();
			var parent = Parent;
			if (parent.CIQRequires)
			{
				AddWarningWhenNotEqualExtracedCIQDetails(parent.JI_ModelInfo, parent.JI_Model, parent.AdditionalInformationHelper.ExtractedModel);
			}
		}

		void AddWarningWhenNotEqualExtracedCIQDetails(ZPropertyInfo propertyInfo, ZString userEnteredValue, ZString extacedValue)
		{
			if (!extacedValue.IsEmpty && !extacedValue.EqualsIgnoringCase(userEnteredValue))
			{
				propertyInfo.AddWarning(Res.GetString("EC9A6F52-FB36-48CB-989C-09963E83C290", "The value is different to Additional Information Value entered in Line Details > Specification & Model ('{0}').", extacedValue));
			}
		}

		#region AddInfo Properties

		protected override void CheckJI_NonDangerousChemicalFlag()
		{
			base.CheckJI_NonDangerousChemicalFlag();

			var parent = Parent;
			var targetInfo = parent.JI_NonDangerousChemicalFlagInfo;

			if (parent.JI_NonDangerousChemicalFlag && parent.GoodsIsDangerousChemical)
			{
				targetInfo.AddWarning(Res.GetString("41C36D3B-C5AD-4353-9C39-8A032B2FC70C",
					"Non Dangerous Chemical should not be ticked for the goods seem to be dangerous chemical."));
			}

			if (parent.CargoAttributes.ContainsCode(CargoAttributeList.Codes._33) && !parent.JI_NonDangerousChemicalFlag)
			{
				targetInfo.AddNotification(
					Res.GetString(
						"5CCC1878-0FD9-4CA3-A55F-794F7BFD9AD9",
						"Non Dangerous Goods should be ticked due to Cargo Attribute '{0}' selected.",
						CargoAttributeList.Descriptions._33
				), ValidationModeProvider);
			}
		}

		protected override void CheckJI_CIQOriginState()
		{
			base.CheckJI_CIQOriginState();

			var parent = Parent;
			if (parent.Declaration?.WillGenerateEnteringEntry ?? false)
			{
				parent.JI_CIQOriginStateInfo.AddNotificationIfInvalidCode(ValidationModeProvider);

				if (!parent.JI_CIQOriginState.IsEmpty && parent.CountryOfOrigin != null)
				{
					if (!parent.JI_CIQOriginState.StartsWith(parent.CountryOfOrigin.RN_IsoNumericUNM49Code, StringComparison.OrdinalIgnoreCase))
					{
						parent.JI_CIQOriginStateInfo.AddNotification(StateShouldBelongToGoodsOrigin, ValidationModeProvider);
					}
				}
			}
		}

		internal static string StateShouldBelongToGoodsOrigin => Res.GetString("a3a013e2-d8d8-454a-ac18-237a3cc7b33b", "The state does not belong to the country of Goods Origin.");

		protected override void CheckJI_DestinationDistrict()
		{
			base.CheckJI_DestinationDistrict();

			var parent = Parent;
			if (parent.Declaration?.WillGenerateEnteringEntry ?? false)
			{
				parent.JI_DestinationDistrictInfo.AddNotificationIfInvalidCodeOrEmpty(ValidationModeProvider);
			}
			CheckLevyTypeAgainstDistrictCode(parent.JI_DestinationDistrict, parent.JI_DestinationDistrictInfo);
		}

		protected override void CheckJI_OriginDistrict()
		{
			base.CheckJI_OriginDistrict();

			var parent = Parent;
			if (parent.Declaration?.WillGenerateExitingEntry ?? false)
			{
				parent.JI_OriginDistrictInfo.AddNotificationIfInvalidCodeOrEmpty(ValidationModeProvider);
			}
			CheckLevyTypeAgainstDistrictCode(parent.JI_OriginDistrict, parent.JI_OriginDistrictInfo);
		}

		void CheckLevyTypeAgainstDistrictCode(ZString districtCode, ZPropertyInfo info)
		{
			var parent = Parent;
			var levyType = parent.EntryInstruction?.CEI_LevyType ?? ZString.Empty;
			if (levyType == LevyTypeList.Codes._307 || levyType == LevyTypeList.Codes._399)
			{
				var isEntering = parent.Declaration.WillGenerateEnteringEntry;

				if (!districtCode.IsEmpty)
				{
					switch (levyType)
					{
						case LevyTypeList.Codes._307:
							if (!Regex.IsMatch(districtCode, @"^.{4}4"))
							{
								info.AddNotification(GetDistrictPattern307NotMatchMessage(isEntering), ValidationModeProvider);
							}
							break;
						case LevyTypeList.Codes._399:
							if (!Regex.IsMatch(districtCode, @"^.{4}[56]|^6509A$|^3723W$"))
							{
								info.AddNotification(GetDistrictPattern399NotMatchMessage(isEntering), ValidationModeProvider);
							}
							break;
					}
				}
			}
		}

		static string GetDistrictPattern307NotMatchMessage(bool isEntering)
		{
			return isEntering ? Res.GetString("7E4756B1-0440-4866-AE60-4AF27F3170CA", "When Levy Type is 307, the fifth digit of Destination District should be 4.")
											: Res.GetString("E8300023-F106-45D2-9A71-D1B61B0BFB86", "When Levy Type is 307, the fifth digit of Origin District should be 4.");
		}

		static string GetDistrictPattern399NotMatchMessage(bool isEntering)
		{
			return isEntering ? Res.GetString("004DB93E-6F90-4024-9FD6-C79C2134E859", "When Levy Type is 399, the fifth digit of Destination District should be 5,6, or 6509A, 3723W.")
											: Res.GetString("1528B0FD-383F-43F5-AC65-8CE43C874ADC", "When Levy Type is 399, the fifth digit of Origin District should be 5,6, or 6509A, 3723W.");
		}

		protected override void CheckJI_OriginRegion()
		{
			base.CheckJI_OriginRegion();
			var parent = Parent;
			if (parent.Declaration?.WillGenerateExitingEntry ?? false)
			{
				parent.JI_OriginRegionInfo.AddNotificationIfInvalidCode(ValidationModeProvider);
				if (parent.CIQRequires)
				{
					parent.JI_OriginRegionInfo.AddNotificationIfNotEntered(ValidationModeProvider);
				}
			}
		}

		protected override void CheckJI_DestinationRegion()
		{
			base.CheckJI_DestinationRegion();
			var parent = Parent;
			if (parent.Declaration?.WillGenerateEnteringEntry ?? false)
			{
				parent.JI_DestinationRegionInfo.AddNotificationIfInvalidCodeOrEmpty(ValidationModeProvider);
			}
		}

		protected override void CheckJI_DutyMode()
		{
			base.CheckJI_DutyMode();
			var parent = Parent;
			var dutyModeFullList = parent.Factory.GetCachedValue<DutyModeList>();
			parent.JI_DutyModeInfo.AddNotificationIfInvalidCodeOrEmpty(dutyModeFullList, ValidationModeProvider);

			if (dutyModeFullList.ContainsCode(parent.JI_DutyMode) && !parent.Lookups.DutyModes.ContainsCode(parent.JI_DutyMode))
			{
				parent.JI_DutyModeInfo.AddNotification(Res.GetString("4F64D68E-B42F-4493-9E61-AEDA73B5D4B0", "The selected Duty Mode is not suitable for Levy Type {0}.",
					parent.EntryInstruction?.CEI_LevyType), ValidationModeProvider);
			}

			var declaration = parent.Declaration;
			if (declaration?.IsTwoStepDeclaration ?? false)
			{
				if (parent.JI_DutyMode != DutyModeList.Codes._3 && !declaration.JE_TaxInvolved)
				{
					parent.JI_DutyModeInfo.AddNotification(Res.GetString("3F8E0381-1FA6-4169-9AB3-C38B19EB484A", "Duty Mode should be {0} - {1} when the declaration is not tax involved.", DutyModeList.Codes._3, DutyModeList.Descriptions._3), ValidationModeProvider);
				}
			}
		}

		protected override void CheckJI_CIQTariff()
		{
			base.CheckJI_CIQTariff();
			var parent = Parent;
			if (parent.CIQRequires)
			{
				parent.JI_CIQTariffInfo.AddNotificationIfNotEntered(ValidationModeProvider);

				var ciqTariff = parent.CIQTariff;
				if (ciqTariff == null)
				{
					parent.JI_CIQTariffInfo.AddNotificationIfInvalidCode(ValidationModeProvider);
				}
				else if (!parent.JI_Tariff.IsEmpty && !parent.JI_CIQTariff.IsEmpty)
				{
					if (!ciqTariff.RelatedTariffs.Any(x => parent.JI_Tariff.StartsWith(x.ZZH_TariffCode, StringComparison.OrdinalIgnoreCase)))
					{
						parent.JI_CIQTariffInfo.AddNotification(Res.GetString("1F33DBE1-0DFD-4557-8452-5F437359D79F", "The CIQ Tariff Code '{0}' is not valid for the Customs Tariff Code '{1}'.", parent.JI_CIQTariff, parent.JI_Tariff), ValidationModeProvider);
					}
					else
					{
						var requiredCargoAttributes = GetRequiredCargoAttributesByCIQTariff(parent.JI_CIQTariff);
						if ((requiredCargoAttributes?.Any() ?? false) && !parent.CargoAttributes.IsProvidedAny(requiredCargoAttributes))
						{
							parent.JI_CIQTariffInfo.AddNotification(Res.GetString("24F21540-72E8-4838-8901-5D2CC4696FE4", "Cargo Attribute {0} is required.", string.Join("/", requiredCargoAttributes)), ValidationModeProvider);
						}

						var requiredProductQualifications = GetRequiredProductQualificationByCIQTariff(parent, parent.JI_CIQTariff);
						if ((requiredProductQualifications?.Any() ?? false) && !parent.CIQProductQualifications.IsProvidedAny(requiredProductQualifications))
						{
							parent.JI_CIQTariffInfo.AddNotification(Res.GetString("AD0C30E3-CA8B-4D7F-B625-6132C49FCE1E", "Product Qualification {0} is required for the selected tariff.", string.Join("/", requiredProductQualifications)), ValidationModeProvider);
						}
					}
				}
			}
			else
			{
				if (!parent.JI_CIQTariff.IsEmpty)
				{
					parent.JI_CIQTariffInfo.AddWarning(Res.GetString("AD1C18C1-C004-469D-B866-77AFF1DACCD9", "If you want to submit CIQ data when sending to Customs, please tick 'CIQ Requires' on the Entry Instruction."));
				}
			}
		}

		public static string[] GetRequiredCargoAttributesByCIQTariff(string ciqTariff)
		{
			string[] cargoAttributes = null;

			if (Regex.IsMatch(ciqTariff, @"^(440[1346789]|441[0-3]|450[12]).*"))
			{
				cargoAttributes = new[] { CargoAttributeList.Codes._23, CargoAttributeList.Codes._24 };
			}

			return cargoAttributes;
		}

		public static string[] GetRequiredProductQualificationByCIQTariff(JobComInvoiceLine invoiceLine, string ciqTariff)
		{
			string[] qualifications = null;

			if (invoiceLine.IsImport)
			{
				if (Regex.IsMatch(ciqTariff, @"^(870[1-5]|8711|8716([^9]|$)).*"))
				{
					qualifications = new[] { ProductQualificationCodeList.Codes._408, ProductQualificationCodeList.Codes._409, ProductQualificationCodeList.Codes._603 };
				}
				else if (Regex.IsMatch(ciqTariff, @"^(320[89]|3210).*"))
				{
					qualifications = new[] { ProductQualificationCodeList.Codes._412 };
				}
				else if (Regex.IsMatch(ciqTariff, @"^(330[3457]).*"))
				{
					qualifications = new[] { ProductQualificationCodeList.Codes._516, ProductQualificationCodeList.Codes._523 };
				}
			}

			return qualifications;
		}

		protected override void CheckJI_ProductManualNo()
		{
			base.CheckJI_ProductManualNo();

			var parent = Parent;
			var targetInfo = parent.JI_ProductManualNoInfo;
			targetInfo.AddNotificationIfIsNegative(ValidationModeProvider);

			var entryInstruction = parent.EntryInstruction;
			if (entryInstruction != null)
			{
				var instructionManualNo = entryInstruction.CEI_ManualNo;
				var invoiceLineManualNo = targetInfo.Value;

				if (instructionManualNo.IsEmpty && !invoiceLineManualNo.IsEmpty)
				{
					targetInfo.AddNotification(GetProductManualNoShouldBeZeroMessage(targetInfo), ValidationModeProvider);
				}
				if (!instructionManualNo.IsEmpty && invoiceLineManualNo.IsEmpty)
				{
					targetInfo.AddNotification(GetProductManualNoCannotBeZeroMessage(targetInfo), ValidationModeProvider);
				}
			}
		}

		protected override void CheckJI_ProductManualNo2()
		{
			base.CheckJI_ProductManualNo2();

			var parent = Parent;
			var targetInfo = parent.JI_ProductManualNo2Info;
			targetInfo.AddNotificationIfIsNegative(ValidationModeProvider);

			var childInstruction = parent.ChildInstruction;
			if ((childInstruction == null || childInstruction.CEI_ManualNo.IsEmpty) && !targetInfo.Value.IsEmpty)
			{
				targetInfo.AddNotification(GetProductManualNoShouldBeZeroMessage(targetInfo), ValidationModeProvider);
			}
			else if (childInstruction != null && !childInstruction.CEI_ManualNo.IsEmpty && targetInfo.Value.IsEmpty)
			{
				targetInfo.AddNotification(GetProductManualNoCannotBeZeroMessage(targetInfo), ValidationModeProvider);
			}
		}

		ZString GetProductManualNoCannotBeZeroMessage(ZPropertyInfo targetInfo)
		{
			return Res.GetString("3276F7A0-EA8C-4675-97C6-120B8B5E9000", "{0} cannot be zero when the Manual Number is entered.", targetInfo.HumanReadableName);
		}

		ZString GetProductManualNoShouldBeZeroMessage(ZPropertyInfo targetInfo)
		{
			return Res.GetString("4B4F2CFD-03E6-4FCF-AF2A-A5607BD1D61C", "{0} should be zero when the Manual Number is not entered.", targetInfo.HumanReadableName);
		}

		protected override void CheckJI_CIQQualityGuaranteePeriod()
		{
			base.CheckJI_CIQQualityGuaranteePeriod();
			Parent.JI_CIQQualityGuaranteePeriodInfo.AddNotificationIfIsNegative(ValidationModeProvider);
		}

		protected override void CheckJI_CIQExpiryDate()
		{
			base.CheckJI_CIQExpiryDate();
			var parent = Parent;
			if (parent.JI_CIQExpiryDate.IsValid)
			{
				if (!parent.ProductionBatch.Cast<ProductionBatch>().Any(x => x.CY_Date.IsValid))
				{
					if (parent.JI_CIQExpiryDate < ZDateTime.Today)
					{
						parent.JI_CIQExpiryDateInfo.AddWarning(Res.GetString("C99E2FAB-9FB0-4BAF-A79E-0CF5B378780F", "The Expiry Date cannot be a past date"));
					}
				}
				else
				{
					if (parent.ProductionBatch.Cast<ProductionBatch>().Any(x => x.CY_Date.IsValid && parent.JI_CIQExpiryDate < x.CY_Date))
					{
						parent.JI_CIQExpiryDateInfo.AddWarning(Res.GetString("4AB7F271-1278-4F24-9D07-25DE27B1486C", "The Expiry Date cannot before any Manufacture Dates"));
					}
				}
			}
		}

		protected override void CheckJI_CIQEndUse()
		{
			base.CheckJI_CIQEndUse();

			var parent = Parent;
			var targetInfo = parent.JI_CIQEndUseInfo;
			if (parent.CIQRequires)
			{
				targetInfo.AddNotificationIfInvalidCodeOrEmpty(ValidationModeProvider);

				var requiredProductQualifications = GetRequiredProductQualifications(parent, parent.JI_CIQEndUse);
				if ((requiredProductQualifications?.Any() ?? false) && !parent.CIQProductQualifications.IsProvidedAny(requiredProductQualifications))
				{
					targetInfo.AddNotification(Res.GetString("DFE4B1D9-21AF-4648-9BCA-AE96FAA03112", "Product Qualification {0} is required for the selected End Use.", string.Join(",", requiredProductQualifications)), ValidationModeProvider);
				}

				if (parent.IsImport && parent.JI_CIQEndUse != EndUseList.Codes.Cosmetics && Regex.IsMatch(parent.JI_CIQTariff, @"^(330[3457]).*"))
				{
					targetInfo.AddNotification(Res.GetString("0A86428D-41EB-41A2-8802-E0AF6FD8A691", "End Use should be 27 for CIQ Tariff ‘{0}’.", parent.JI_CIQTariff), ValidationModeProvider);
				}
			}
		}

		static string[] GetRequiredProductQualifications(JobComInvoiceLine invoiceLine, string ciqEndUse)
		{
			string[] qualifications = null;

			if (invoiceLine.IsImport)
			{
				switch (ciqEndUse)
				{
					case EndUseList.Codes.Cosmetics:
						qualifications = new[] { ProductQualificationCodeList.Codes._516, ProductQualificationCodeList.Codes._523 };
						break;
				}
			}

			return qualifications;
		}

		protected override void CheckJI_TradeQuantity()
		{
			base.CheckJI_TradeQuantity();
			var infoDesp = Res.GetString("691F80E8-E68A-420A-BBD0-7FFA0C08027C", "Trade Qty");
			Parent.JI_TradeQuantityInfo.AddNotificationIfIsZero(infoDesp, ValidationModeProvider);
			Parent.JI_TradeQuantityInfo.AddNotificationIfIsNegative(infoDesp, ValidationModeProvider);
		}

		protected override void CheckJI_TradeUnitQty()
		{
			base.CheckJI_TradeUnitQty();

			var targetInfo = Parent.JI_TradeUnitQtyInfo;
			targetInfo.AddNotificationIfInvalidCodeOrEmpty(ValidationModeProvider);

			if (Parent.Pivot is CusClassPartPivot pivot && !pivot.CNC_TradeUnitQty.IsEmpty && Parent.JI_TradeUnitQty != pivot.CNC_TradeUnitQty)
			{
				targetInfo.AddWarning(Res.GetString("9064C052-CBDA-43FC-9F8A-A582B8208F14", "The value entered is different from Trade Unit Price on Product ({0}), please confirm that this is correct.", pivot.CNC_TradeUnitQty));
			}
		}

		protected override void CheckJI_OrigContainerFlag()
		{
			base.CheckJI_OrigContainerFlag();
			Parent.JI_OrigContainerFlagInfo.AddNotificationIfInvalidCode(ValidationModeProvider);
		}

		#region Validate Name of Goods & Goods Spec Model

		protected override void CheckJI_NameOfGoods()
		{
			Parent.JI_NameOfGoodsInfo.AddNotificationIfNotEntered(ValidationModeProvider);
		}

		protected override void CheckJI_NameOfGoods2()
		{
			if (Parent.ChildInstruction != null)
			{
				Parent.JI_NameOfGoods2Info.AddNotificationIfNotEntered(ValidationModeProvider);
			}
		}

		#endregion

		protected override void CheckJI_PackageTypeOfUNDG()
		{
			base.CheckJI_PackageTypeOfUNDG();

			var invoiceLine = Parent;
			var packageTypeOfUNDG = Parent.JI_PackageTypeOfUNDG;
			var targetInfo = Parent.JI_PackageTypeOfUNDGInfo;

			if (invoiceLine.CargoAttributes.ContainsCode(CargoAttributeList.Codes._32) && !invoiceLine.JI_NonDangerousChemicalFlag)
			{
				targetInfo.AddNotificationIfNotEntered(ValidationModeProvider);
			}

			if (!packageTypeOfUNDG.IsEmpty)
			{
				if (InvalidCharactersRegex.IsMatch(packageTypeOfUNDG))
				{
					targetInfo.AddNotification(Res.GetString("147DB7BB-B69B-42F6-ACFE-C26B979B3596", "{0} can only contain alphanumeric characters and '/', ' ', '-', '.'.", targetInfo.HumanReadableName), ValidationModeProvider);
				}
				if (!packageTypeOfUNDG.Contains('/'))
				{
					targetInfo.AddNotification(Res.GetString("EC2F7076-C9A7-4F83-8047-0A333F083FB7", "{0} must contain '/'.", targetInfo.HumanReadableName), ValidationModeProvider);
				}
			}
		}

		static readonly Regex InvalidCharactersRegex = new((NoResString)@"[^a-zA-Z0-9\/ \-\.]");

		#endregion
	}
}
