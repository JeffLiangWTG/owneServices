using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class JobComInvoiceLineValidation : BaseJobComInvoiceLineValidation
	{
		public JobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public new JobComInvoiceLine Parent
		{
			get { return (JobComInvoiceLine)base.Parent; }
		}
		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		public IInvoiceLineValidationDecider ValidationDecider => Parent.Factory.GetValue(ref validationDeciderCached, GetValidationDecider);
		CachedProperty<IInvoiceLineValidationDecider> validationDeciderCached;

		IInvoiceLineValidationDecider GetValidationDecider() => Declaration?.Configuration.InvoiceLineConfiguration.GetValidationDecider(Parent);

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidateAdditionalProcedureCodesAsString();
			ValidateNotAllowCreateNewEntryLine();
			ValidateJI_TaxOrFeeDetail();
			ValidateJI_SupplementaryCode1();
		}

		protected override void CheckJI_RelatedIndicator()
		{
			base.CheckJI_RelatedIndicator();
			CheckRuleC0624_InvoiceLine(Parent, Parent.RelatedIndicatorInfo);
		}

		internal void CheckRuleC0624_InvoiceLine(JobComInvoiceLine invoiceLine, ZPropertyInfo relatedIndicatorInfo)
		{
			if (invoiceLine is JobComInvoiceLine line
				&& (ValidationDecider is IImportInvoiceLineValidationDecider importValidationDecider && importValidationDecider.IsRuleC0624Active)
				&& (ZBool)relatedIndicatorInfo.Value
				&& EntryInstruction_FallIntoRuleC0624(line.EntryInstruction))
			{
				relatedIndicatorInfo.AddMessageError(errorStringC0624);
			}
		}
		#pragma warning disable IDE0044 // conflicting warnings ( conflict between IDE0044 and Res.GetString ) suppressing the latest one, i.e., IDE0044
		string errorStringC0624 = Res.GetString("B4007499-0B62-4158-A8D4-BCDF32A5CCAA", "[C0624] Value indicator should not be entered for the provided Declaration Type and Requested Procedure.");
		#pragma warning restore IDE0044 // Restoring IDE0044
		bool EntryInstruction_FallIntoRuleC0624(CusEntryInstruction entryInstruction)
		{
			return entryInstruction != null && (entryInstruction.IsSimplifiedOrPreliminaryUnderCodeC || entryInstruction.CEI_Procedure == Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration || entryInstruction.CEI_Procedure == Core.Constants.Customs.Universal.RefCusProcedure.Codes._53);
		}

		public void ValidateJI_TaxOrFeeDetail()
		{
			ValidateCalculatedProperty(Parent.JI_TaxOrFeeDetailInfo);
		}

		protected void CheckJI_TaxOrFeeDetail()
		{
			var parent = Parent;

			if (ShouldWarnVAT && parent.JI_TaxOrFeeDetailEntity != null)
			{
				var message = Res.GetString("0FFEB552-31FA-4131-8673-6846F3C12B77", "Procedure {0} indicates that VAT does not apply, but value {1} in this field means that VAT is calculated by CW1; To disable the calculation set this field's value to blank.", parent.ProcedureIndicatesVATNotApply, parent.JI_TaxOrFeeDetailEntity.VATCode);
				parent.JI_TaxOrFeeDetailInfo.AddWarning(message);
			}
		}

		protected override void CheckJI_ValuationCode()
		{
			base.CheckJI_ValuationCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_ValuationCodeInfo, Parent.Lookups.ValuationCodeList);

			CheckRuleC0627();
		}

		void CheckRuleC0627()
		{
			var parent = Parent;
			if (ValidationDecider is IImportInvoiceLineValidationDecider importValidationDecider && importValidationDecider.IsRuleC0627Active)
			{
				if (parent.EntryInstruction?.IsSimplifiedOrPreliminaryUnderCodeC ?? false)
				{
					if (!parent.JI_ValuationCode.IsEmpty)
					{
						parent.JI_ValuationCodeInfo.AddMessageError(Res.GetString("33D7B82A-7630-4D88-82A8-E3B219C0A341", "[C0627] This field must be empty in case of Declaration Sub Type C or F."));
					}
				}
				else
				{
					if (parent.JI_ValuationCode.IsEmpty)
					{
						parent.JI_ValuationCodeInfo.AddMessageError(Res.GetString("10E38E96-96A3-4AAB-84F6-5BF96272679A", "[C0627] This field is mandatory for this declaration Sub Type."));
					}
				}
			}
		}

		protected override void CheckJI_WeightUQ()
		{
			base.CheckJI_WeightUQ();
			var entryLine = Parent.CusEntryLine;
			if (entryLine != null && entryLine.EffectiveGrossWeightIsApplicable && Parent.JI_WeightUQ.IsEmpty)
			{
				Parent.JI_WeightUQInfo.AddMessageError(Res.GetString("7474454A-DE35-4A35-AE02-4C6743DBACE9", "Effective gross mass is applicable, please select a weight unit (UQ)"));
			}
		}

		protected override void CheckJI_Weight()
		{
			base.CheckJI_Weight();
			if (!Parent.JI_Weight.IsEmpty)
			{
				if ((ValidationDecider?.IsRuleR0222Active ?? false) && Parent.PackagesForInvoiceLinesForBindingOnly.Cast<BaseCusLinkPackage>().Any(x => x.PackQty == 0 && !x.Package.CW_PackType.In(BulkPackageTypesR0222) && x.IsLinked))
				{
					Parent.JI_WeightInfo.AddMessageError(Res.GetString("B2F0A1D-3C8E-4F5B-9A6B-7D3F0A1D3C8E", "[R0222] If package quantity = 0 then Gross Weight must be 0 as well."));
				}
				if ((ValidationDecider?.IsRuleR0223Active ?? false) && !Parent.JI_NetWeight.IsEmpty && (Parent.GrossWeightInKG < Parent.NetWeightInKG))
				{
					Parent.JI_WeightInfo.AddMessageError(Res.GetString("3EE6B524-4EAE-4AEA-BF64-E76E40DADDFF", "[R0223] Gross weight must be greater than or equal to net weight"));
				}

				if ((ValidationDecider?.IsRuleR0224Active ?? false) && Parent.EntryInstruction is CusEntryInstruction entryInstruction && entryInstruction.InvoiceLines.IsCountMoreThan(0))
				{
					var totalGrossWeightKG = Parent.EntryInstruction.InvoiceLines.Sum(line => line.GrossWeightInKG);
					var totalNetWeightKG = Parent.EntryInstruction.InvoiceLines.Sum(line => line.NetWeightInKG);

					if (totalGrossWeightKG < totalNetWeightKG)
					{
						Parent.JI_WeightInfo.AddMessageError(Res.GetString("A4D1E5F2-0C3B-4F7A-8E6C-9D1A0B2F5C3D", "[R0224] Sum of gross weight must be greater than or equal to sum of net weight"));
					}
				}
			}
		}

		protected override void CheckJI_Tariff()
		{
			base.CheckJI_Tariff();

			CheckJI_Tariff_NoPackage();
			CheckAdditionalCode();
			ValidateJI_Procedure();
			CheckJI_TariffRuleC0820();
			var(controlConditionCheck, informationConditionCheck) = GetCheckClassConditionsResults();
			CheckClassConditions(controlConditionCheck, informationConditionCheck);
		}

		protected internal (ZString, ZString) GetCheckClassConditionsResults()
		{
			var controlConditionCheck = ZString.Empty;
			var informationConditionCheck = ZString.Empty;
			if (Parent?.Declaration != null && Parent.UseUniversalTariff && Parent.GetUniversalConditionCheck())
			{
				(controlConditionCheck, informationConditionCheck) = Parent.UniversalTariff?.CheckConditionsAreMetForConditionClass(Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Class, Parent.EvaluateConditionValue, Parent.GetFriendlyConditionValue, Parent.CalcDataForConditionFormula) ?? (ZString.Empty, ZString.Empty);
			}
			return (controlConditionCheck, informationConditionCheck);
		}

		void CheckClassConditions(ZString controlConditionCheck, ZString informationConditionCheck)
		{
			var targetInfo = Parent.JI_TariffInfo;
			if (!controlConditionCheck.IsEmpty)
			{
				targetInfo.AddNotification(NotificationTypeForConditionsCheck, controlConditionCheck);
			}

			if (!informationConditionCheck.IsEmpty)
			{
				targetInfo.AddNotification(CargoWise.EntityFramework.NotificationType.Warning, informationConditionCheck);
			}
		}

		void CheckJI_TariffRuleC0820()
		{
			var parent = Parent;

			if ((ValidationDecider is IImportInvoiceLineValidationDecider importValidationDecider && importValidationDecider.IsRuleC0820ActiveForJI_Tariff)
				&& parent.JI_Calc_Concession != Core.Constants.Customs.Universal.RefCusProcedure.Concession.F15
				&& parent.JI_Tariff.KeepNumericCharacters().Length < 10)
			{
				AddMessageError_RuleC0820(parent.JI_TariffInfo);
			}
		}

		protected override void CheckJI_OA_ExporterAddress()
		{
			base.CheckJI_OA_ExporterAddress();

			CheckRuleC0002();
		}

		protected override void RunCountrySpecificRateValidation(ZPropertyInfo targetInfo, IZZRateSelectionCriteria criteria, IEnumerable<RateView> applicableRates)
		{
			if (criteria.RateType == Universal.Constants.RateTypes.Duty)
			{
				if (!applicableRates.Any(x => x.RateCode == UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts))
				{
					targetInfo.AddMessageError(Res.GetString("51BCAE2F-E96E-413E-AF01-03A0E9736C89", "No DUTY rate with Rate Code A00 exists."));
				}
			}
		}

		protected override IEnumerable<IZZRateSelectionCriteria> RateSelectionCriteriaLists => new List<IZZRateSelectionCriteria>() { Parent.DutyRateSelectionCriteria, Parent.ADDRateSelectionCriteria, Parent.CVDRateSelectionCriteria }.Union(Parent.NationalRateSelectionCriteria);

		protected override ZString OrderNumberCaption => Res.GetString("B28EDBE6-BA91-4767-996D-2BFEB5C90A45", "Quota");

		protected override ZString AdditionalCodeCaption => Res.GetString("DD6321D4-EB6B-4B62-AB90-D78071D3E7BD", "Supplementary Code");

		protected virtual void CheckAdditionalCode()
		{
			CheckTariffNoAdditionalCodes();
			CheckTariffCodesForMeursingDuty();
		}

		protected virtual void CheckTariffNoAdditionalCodes()
		{
			var parent = Parent;

			if (!parent.SupplementaryCodes.Any() && parent.Lookups.AdditionalCodesList.Count > 0)
			{
				parent.JI_TariffInfo.AddNotification(TariffNoAdditionalCodesNotificationType, TariffNoAdditionalCodesMessage);
			}
		}

		protected virtual void CheckTariffCodesForMeursingDuty()
		{
			var parent = Parent;

			var applicableRates = parent.UniversalTariff?.GetApplicableRates(Parent.DutyRateSelectionCriteria);
			if (applicableRates != null)
			{
				var meursingIsApplicable = applicableRates.Any(x => EURateFormulaHelper.ContainsMursingPattern(x.ZZ2_RateFormula));
				var supplementaryCodesNumberForMeursing = parent.SupplementaryCodes.Count(x =>
				{
					var code = x?.CY_Code ?? ZString.Empty;
					return code.IsNumbersOnlyOrEmpty && code.Length == 4 && code.StartsWith("7", StringComparison.OrdinalIgnoreCase);
				});

				if (meursingIsApplicable && supplementaryCodesNumberForMeursing != 1)
				{
					parent.JI_TariffInfo.AddMessageError(Res.GetString("564CDD7A-E66C-433E-AECA-E8C530E56489", "Exactly one supplementary code matching pattern '7NNN' e.g. 7086, should exist for Meursing duty calculation purposes."));
				}

				if (ShouldCheckNo7NNNSupplementaryCodeForMeursingIsNotApplicable && !meursingIsApplicable && supplementaryCodesNumberForMeursing > 0)
				{
					parent.JI_TariffInfo.AddMessageError(Res.GetString("3A623973-90BC-4338-8667-557CD7926AB9", "No supplementary code matching pattern '7NNN' should exist when Meursing is not applicable."));
				}
			}
		}

		protected virtual ZString TariffNoAdditionalCodesMessage => Res.GetString("863ED4C6-874B-400B-8D8B-F68282208F00", "Supplementary Codes exist for this tariff.");

		protected virtual INotificationType TariffNoAdditionalCodesNotificationType => CargoWise.EntityFramework.NotificationType.Warning;

		protected virtual bool ShouldCheckNo7NNNSupplementaryCodeForMeursingIsNotApplicable => true;

		protected override void CheckJI_LineNo()
		{
			base.CheckJI_LineNo();
			var parent = Parent;
			if (parent.ShouldCheckMissingPreviousDocuments)
			{
				CheckMissingPreviousDocuments(parent);
			}
		}

		void CheckMissingPreviousDocuments(JobComInvoiceLine parent)
		{
			var declaration = Declaration;

			if (declaration != null && declaration.Configuration.InvoiceLineConfiguration.PreviousDocumentsSupport(declaration))
			{
				var invoice = parent.InvoiceHeader;
				var invoiceHasDocs = (invoice?.PreviousDocuments.Count ?? 0) > 0;
				var invoiceSupportPrevDocs = declaration.Configuration.InvoiceHeaderConfiguration.PreviousDocumentsSupport(declaration);
				var declarationHasDocs = declaration.PreviousDocuments.Count > 0;
				var declarationSupportPrevDocs = declaration.Configuration.MiscPreviousDocumentsSupport(declaration);
				if (parent.CusEntryLine != null)
				{
					var entryInstructionStyles = declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().Where(x => !x.CEI_Style.IsEmpty).Select(x => x.CEI_Style).ToArray();
					if (!parent.CusEntryLine.PreviousDocuments.Any() && (!PreviousDocumentsCheckExceptedDeclarationTypes.Any() || entryInstructionStyles.Except(PreviousDocumentsCheckExceptedDeclarationTypes).Any()))
					{
						var message = GetMergedLineMissingPreviousDocumentsMessage(invoiceHasDocs, invoiceSupportPrevDocs, declarationHasDocs, declarationSupportPrevDocs);
						if (!string.IsNullOrEmpty(message))
						{
							parent.JI_LineNoInfo.AddWarning(message);
						}
					}
				}
				else
				{
					if (parent.PreviousDocuments.Count == 0
						&& (!invoiceSupportPrevDocs || !invoiceHasDocs)
						&& (!declarationSupportPrevDocs || !declarationHasDocs))
					{
						var initialMessage = Res.GetString("4A3E1E6C-4C8F-46B7-AC71-A48B722B17FE", "This line has no previous documents. Without a previous document the entry may be rejected. Please enter one at this line");
						var message = GetMissingPreviousDocumentsMessage(initialMessage, invoiceHasDocs, invoiceSupportPrevDocs, declarationHasDocs, declarationSupportPrevDocs);
						if (!string.IsNullOrEmpty(message))
						{
							parent.JI_LineNoInfo.AddWarning(message);
						}
					}
				}
			}
		}

		protected virtual string GetMergedLineMissingPreviousDocumentsMessage(bool invoiceHasDocs, bool invoiceSupportPrevDocs, bool declarationHasDocs, bool declarationSupportPrevDocs)
		{
			var initialMessage = Res.GetString("BA9BC1C4-C6F5-463C-A4D0-210A50D1B83E", "This invoice line’s merged entry line has no previous documents. Without a previous document the entry may be rejected. Add one to any of the entry line’s invoice line");
			return GetMissingPreviousDocumentsMessage(initialMessage, invoiceHasDocs, invoiceSupportPrevDocs, declarationHasDocs, declarationSupportPrevDocs);
		}

		static string GetMissingPreviousDocumentsMessage(string initialMessage, bool invoiceHasDocs, bool invoiceSupportPrevDocs, bool declarationHasDocs, bool declarationSupportPrevDocs)
		{
			var result = new ZStringBuilder();

			if ((!invoiceSupportPrevDocs || !invoiceHasDocs) && (!declarationSupportPrevDocs || !declarationHasDocs))
			{
				result.Append(initialMessage);

				if (invoiceSupportPrevDocs)
				{
					result.Append(" " + Res.GetString("6C929942-29D1-4DDC-8C76-2DB2BF288101", "or at its invoice"));
				}
				if (declarationSupportPrevDocs)
				{
					result.Append(" " + Res.GetString("2FC2CC0E-569C-4864-8001-5A8309497D6F", "or at declaration"));
				}
				result.Append(".");
			}
			return result.ToString();
		}

		protected virtual IEnumerable<ZString> PreviousDocumentsCheckExceptedDeclarationTypes
		{
			get { return Array.Empty<ZString>(); }
		}

		protected virtual void CheckJI_Tariff_NoPackage()
		{
			var dec = Declaration;
			if (dec != null)
			{
				if (dec.JE_MessageType == MessageTypeList.Codes.Import)
				{
					CheckJI_Tariff_NoPackage_Import(dec);
				}
				else if (dec.JE_MessageType == MessageTypeList.Codes.Export)
				{
					CheckJI_Tariff_NoPackage_Export(dec);
				}
			}
		}

		protected virtual void CheckJI_Tariff_NoPackage_Import(JobDeclaration dec)
		{
			var parent = Parent;
			parent.ClearRowNotificationsContaining(Res.GetString("6e5a935a-ab35-4e1a-b120-2749d61e1951", "This line has no packaging details"));
			if (!HasValidPackagePivots)
			{
				parent.AddRowMessageError(TariffInfoMessageErrorNoPackageDetails);
			}
		}

		protected virtual void CheckJI_Tariff_NoPackage_Export(JobDeclaration dec)
		{
			var parent = Parent;
			parent.ClearRowNotificationsContaining(Res.GetString("771dddbc-5c6b-428b-9a74-8ae92db39051", "This line has no packaging details"));
			if (!HasValidPackagePivots)
			{
				var seaSuffix = dec.IsSea ? Res.GetString("32CB4128-2928-48DE-BAF0-CFF7EB8A4ADB", "For export sea declarations without any bill information, a placeholder bill number may be furnished, e.g. 'TBA'.") + (dec.RelevantConsol != null ? Res.GetString("EA032E18-14FB-438F-87BE-E62111FD3A98", " Enter this on the consol's master bill field.") : "") : "";
				parent.AddRowMessageError(TariffInfoMessageErrorNoPackageDetails + seaSuffix);
			}
		}

		protected virtual bool HasValidPackagePivots => Parent.PackagesPivot.Count != 0;

		protected virtual string GetPackagesTabName() => Res.GetString("52B23046-54E9-47F7-8B03-E8C3F3B38C6A", "Packages");

		protected override void CheckJI_CustomsSecondQuantity()
		{
			base.CheckJI_CustomsSecondQuantity();
			var parent = Parent;
			var cusProcedure = Parent.CusProcedure;
			var customsSecondQuantity = parent.JI_CustomsSecondQuantity;
			var targetInfo = parent.JI_CustomsSecondQuantityInfo;

			var procAllowsZeroSupp = cusProcedure != null && cusProcedure.HasAttribute(Universal.AttributeNames.Codes.ZeroSupplementaryQty);
			if (!parent.JI_CustomsSecondUnitQty.IsEmpty && customsSecondQuantity == 0 && !procAllowsZeroSupp && parent.JI_CustomsSecondUnitQty != QuantityUnitList.WeightAcesulfamePotatssium119 && !IsFollowingRuleC0936(parent))
			{
				targetInfo.AddMessageError(SupplementaryQtyShouldNotBeEmptyWhenWeHaveUnitsWarning);
			}

			CheckQuantityLengthValidation(customsSecondQuantity, targetInfo);
		}

		protected bool IsFollowingRuleC0936(JobComInvoiceLine parent)
		{
			var substyle = parent.EntryInstruction?.CEI_SubStyle ?? ZString.Empty;
			return (ValidationDecider is IImportInvoiceLineValidationDecider importValidationDecider && importValidationDecider.IsRuleC0936Active) && (parent.JI_Calc_Concession == Core.Constants.Customs.Universal.RefCusProcedure.Concession.F15 || ListOfEntrySubStyleListForOptionalCustomsSecondUnitQty.Contains(substyle));
		}

		ZString[] ListOfEntrySubStyleListForOptionalCustomsSecondUnitQty => listOfEntrySubStyleListForOptionalCustomsSecondUnitQty ?? (listOfEntrySubStyleListForOptionalCustomsSecondUnitQty = new ZString[] { EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC, EntrySubStyleList.Codes.SimplifiedDeclaration });
		ZString[] listOfEntrySubStyleListForOptionalCustomsSecondUnitQty;

		protected override void CheckJI_CustomsQuantity()
		{
			var targetInfo = Parent.JI_CustomsQuantityInfo;
			var customsQuantity = Parent.JI_CustomsQuantity;
			if ((ValidationDecider is IImportInvoiceLineValidationDecider importValidationDecider && importValidationDecider.IsRuleCD9102Active)
				&& customsQuantity == ZDecimal.Zero && Parent.EntryInstruction is CusEntryInstruction instruction
				&& (instruction.CEI_Style == EUCommonConstants.ImportDeclarationTypeList.H1 || instruction.CEI_Style == EUCommonConstants.ImportDeclarationTypeList.H4))
			{
				targetInfo.AddMessageError(messageTextCD9102);
			}
			else
			{
				base.CheckJI_CustomsQuantity();
			}

			CheckQuantityLengthValidation(customsQuantity, targetInfo);
		}
		#pragma warning disable IDE0044 // conflicting warnings ( conflict between IDE0044 and Res.GetString ) suppressing the latest one, i.e., IDE0044
		string messageTextCD9102 = Res.GetString("7B9631F6-A221-47FE-AF47-707B121F2B75", "[CD9102] Net Weight in KG is required.");
		#pragma warning restore IDE0044 // Restoring IDE0044
		protected override void CheckJI_CustomsUnitQty()
		{
			base.CheckJI_CustomsUnitQty();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_CustomsUnitQtyInfo);
			CheckQuantityQtyPresentWhenPositive(Parent.JI_CustomsQuantity, Parent.JI_CustomsUnitQtyInfo);
		}

		protected override void CheckJI_CustomsSecondUnitQty()
		{
			base.CheckJI_CustomsSecondUnitQty();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_CustomsSecondUnitQtyInfo);
			CheckQuantityQtyPresentWhenPositive(Parent.JI_CustomsSecondQuantity, Parent.JI_CustomsSecondUnitQtyInfo);
		}

		protected override void CheckJI_CustomsThirdQuantity()
		{
			base.CheckJI_CustomsThirdQuantity();

			var customsThirdQuantity = Parent.JI_CustomsThirdQuantity;
			var targetInfo = Parent.JI_CustomsThirdQuantityInfo;
			if (!Parent.JI_CustomsThirdUnitQty.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}

			CheckQuantityLengthValidation(customsThirdQuantity, targetInfo);
		}

		protected override void CheckJI_CustomsThirdUnitQty()
		{
			base.CheckJI_CustomsThirdUnitQty();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_CustomsThirdUnitQtyInfo);
			CheckQuantityQtyPresentWhenPositive(Parent.JI_CustomsThirdQuantity, Parent.JI_CustomsThirdUnitQtyInfo);
		}

		protected override void CheckJI_CustomsFourthQuantity()
		{
			base.CheckJI_CustomsFourthQuantity();

			CheckQuantityLengthValidation(Parent.JI_CustomsFourthQuantity, Parent.JI_CustomsFourthQuantityInfo);
		}

		protected override void CheckJI_CustomsFourthUnitQty()
		{
			base.CheckJI_CustomsFourthUnitQty();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_CustomsFourthUnitQtyInfo);
			CheckQuantityQtyPresentWhenPositive(Parent.JI_CustomsFourthQuantity, Parent.JI_CustomsFourthUnitQtyInfo);
		}

		protected override void CheckJI_CustomsFifthUnitQty()
		{
			base.CheckJI_CustomsFifthUnitQty();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_CustomsFifthUnitQtyInfo);
			CheckQuantityQtyPresentWhenPositive(Parent.JI_CustomsFifthQuantity, Parent.JI_CustomsFifthUnitQtyInfo);
		}

		protected override void CheckJI_ConcessionOrder()
		{
			base.CheckJI_ConcessionOrder();

			var parent = Parent;
			if (parent.IsImport)
			{
				var primaryPreference = parent.JI_PrimaryPreference;
				if (primaryPreference.Length >= 2 && primaryPreference[1] == '2')
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.JI_ConcessionOrderInfo);
				}

				var concessionOrder = parent.JI_ConcessionOrder;
				if (!concessionOrder.IsEmpty
					&& (concessionOrder.Length != 6
						|| !concessionOrder.IsNumbersOnlyOrEmpty))
				{
					parent.JI_ConcessionOrderInfo.AddMessageError(QuotaNumberShouldBe6Characters);
				}

				if (parent.UniversalTariff != null)
				{
					ValidateJI_Tariff();
				}
			}
		}

		protected override void CheckJI_LinePrice()
		{
			base.CheckJI_LinePrice();

			var parent = Parent;
			CheckRuleCD0111();

			if (ValidationDecider is IImportInvoiceLineValidationDecider importValidationDecider && importValidationDecider.IsRuleC0710ActiveForJI_LinePrice)
			{
				CheckRuleC0710(parent.JI_LinePriceInfo, parent);
			}
		}

		void CheckRuleCD0111()
		{
			var parent = Parent;
			if ((ValidationDecider is IImportInvoiceLineValidationDecider importValidationDecider && importValidationDecider.IsRuleCD0111Active) && parent.JI_LinePrice.IsEmpty && IsPriceRequired(parent.EntryInstruction?.CEI_Style ?? ZString.Empty))
			{
				parent.JI_LinePriceInfo.AddMessageError(Res.GetString("CD81B8AA-C436-49D3-BB42-4F7C35BFC59D", "Price is required when Procedure is H1, H3 , H4, H5 or I1."));
			}
		}

		bool IsPriceRequired(ZString style)
		{
			return style == EUCommonConstants.ImportDeclarationTypeList.H1 || style == EUCommonConstants.ImportDeclarationTypeList.H3 || style == EUCommonConstants.ImportDeclarationTypeList.H4 || style == EUCommonConstants.ImportDeclarationTypeList.H5 || style == EUCommonConstants.ImportDeclarationTypeList.I1;
		}

		public static string QuotaNumberShouldBe6Characters
		{
			get { return Res.GetString("1DF4B6D2-CC95-444A-9701-A47E8175EE51", "The quota number should be 6 characters long (only digits are allowed)"); }
		}

		protected override void CheckJI_Procedure()
		{
			base.CheckJI_Procedure(); // Includes list & empty validation
			ValidateJI_Tariff();
			ValidateJI_CustomsSecondQuantity(); // Supplementary quantity
			ValidateFiscalReferences();
			ValidateAllowMultipleRequestedProcedure();

			var invoiceLine = Parent;
			var mainProcedureCode = invoiceLine.JI_Procedure;
			if (invoiceLine.IsAdditionalProcedureCodesApplicable && !mainProcedureCode.IsEmpty)
			{
				var mainProcedure = invoiceLine.CusProcedure;
				if (mainProcedure != null)
				{
					var currentPlusPreviousOfMainProcedure = mainProcedure.ZZ6_ProcedureCode + mainProcedure.ZZ6_PreviousProcedureCode;
					if (AdditionalProcedureCodesCheckFirst4Characters && invoiceLine.AdditionalProcedureCodes.Cast<AdditionalProcedureCode>().Any(x => !x.CY_Code.StartsWith(currentPlusPreviousOfMainProcedure, StringComparison.OrdinalIgnoreCase)))
					{
						invoiceLine.JI_ProcedureInfo.AddMessageError(Res.GetString("38502906-0081-4428-b983-5517932e9f01", "The first 4 characters of additional procedure codes should be all the same as the main procedure code's."));
					}
				}

				if (invoiceLine.AdditionalProcedureCodes.Cast<AdditionalProcedureCode>().Any(x => x.CY_Code == mainProcedureCode))
				{
					invoiceLine.JI_ProcedureInfo.AddMessageError(Res.GetString("8308B6E1-581E-410E-92B2-AC18EEA56B15", "Procedure code should not be duplicated in Additional Procedure codes."));
				}
			}
		}

		public void ValidateJI_SupplementaryCode1()
		{
			ValidateCalculatedProperty(Parent.JI_SupplementaryCode1Info);
		}

		protected virtual void CheckJI_SupplementaryCode1()
		{
			CheckJI_SupplementaryCode1RuleC0820();
		}

		void CheckJI_SupplementaryCode1RuleC0820()
		{
			var parent = Parent;

			if ((ValidationDecider is IImportInvoiceLineValidationDecider importValidationDecider && importValidationDecider.IsRuleC0820ActiveForJI_SupplementaryCode1)
				&& parent.JI_SupplementaryCode1.IsEmpty
				&& parent.Lookups.AdditionalCodesList.Count > 0
				&& parent.JI_Calc_Concession != Core.Constants.Customs.Universal.RefCusProcedure.Concession.F15)
			{
				AddMessageError_RuleC0820(parent.JI_SupplementaryCode1Info);
			}
		}

		internal static void AddMessageError_RuleC0820(ZPropertyInfo info)
		{
			info.AddMessageError(Res.GetString("6A29F085-9D9F-4F49-AE2B-031E32D9B593", "[C0820] This field is mandatory for any case besides Additional procedure F15."));
		}

		protected void ValidateAllowMultipleRequestedProcedure()
		{
			var parent = Parent;
			if (ValidationDecider is IImportInvoiceLineValidationDecider importValidationDecider && !importValidationDecider.AllowMultipleRequestedProcedure && Parent.EntryInstruction is CusEntryInstruction entryInstruction && entryInstruction.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.PK != parent.PK && x.JI_Procedure != parent.JI_Procedure))
			{
				parent.JI_ProcedureInfo.AddMessageError(Res.GetString("835C1BA2-D707-4573-9465-F7F5B9865A5B", "All invoice lines under the same entry must have the same Requested Procedure."));
			}
		}

		protected virtual bool AdditionalProcedureCodesCheckFirst4Characters => true;

		protected override string FriendlyNameForZz6Category => Res.GetString("57B46DC8-871F-4505-997B-BD509ED9E1B1", "series");

		protected override void CheckJI_CountryOfOrigin()
		{
			base.CheckJI_CountryOfOrigin();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_CountryOfOriginInfo);
			CheckJI_CountryOfOriginMandatoryValidation();
		}

		protected virtual void CheckJI_CountryOfOriginMandatoryValidation()
		{
			var parent = Parent;

			if (ValidationDecider is IImportInvoiceLineValidationDecider importValidationDecider && importValidationDecider.IsRuleC0710ActiveForJI_CountryOfOrigin)
			{
				CheckRuleC0710(parent.JI_CountryOfOriginInfo, parent);
			}

			CheckRuleC0919(parent.JI_CountryOfOriginInfo, parent.JI_PrimaryPreference);

			if (!(parent.Declaration?.IsUCC6AndIsImport ?? false))
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.JI_CountryOfOriginInfo);
			}

			CheckRuleCD5161(parent.JI_CountryOfOriginInfo, parent.JI_PrimaryPreference, parent.JI_PrimaryPreferenceInfo);
		}

		void CheckRuleCD5161(ZPropertyInfo countryOfOriginInfo, ZString primaryReference, ZPropertyInfo primaryReferenceInfo)
		{
			if ((ValidationDecider is IImportInvoiceLineValidationDecider importValidationDecider && importValidationDecider.IsRuleCD5161ActiveForJI_CountryOfOrigin) && countryOfOriginInfo.Value.IsEmpty && IsPreferencialOriginRequired(primaryReference))
			{
				countryOfOriginInfo.AddMessageError(Res.GetString("A6EB18B7-7B7A-4B78-957A-BECA5A2A376F", "[CD5161] Preferential Origin ({0}) is required when the first digit of {1} is ‘2’, ‘3’, ‘4’ or ‘5’.", countryOfOriginInfo.Description, primaryReferenceInfo.Description));
			}
		}

		bool IsPreferencialOriginRequired(ZString primaryReference)
		{
			return primaryReference.StartsWith("2") || primaryReference.StartsWith("3") || primaryReference.StartsWith("4") || primaryReference.StartsWith("5");
		}

		internal static void CheckRuleC0710(ZPropertyInfo info, JobComInvoiceLine invoiceLine)
		{
			if (info.Value.IsEmpty
				&& invoiceLine.EntryInstruction is CusEntryInstruction entryInstruction
				&& entryInstruction.CEI_SubStyle.In(EntrySubStyleListC0710))
			{
				info.AddMessageError(Res.GetString("DC675077-593F-42DC-B5A2-936426C77758", "[C0710] This field is mandatory for this declaration sub type."));
			}
		}

		void CheckRuleC0919(ZPropertyInfo info, ZString primaryPreference)
		{
			if ((ValidationDecider is IImportInvoiceLineValidationDecider importValidationDecider && importValidationDecider.IsRuleC0919Active)
				&& info.Value.IsEmpty
				&& (primaryPreference.StartsWith("1") || primaryPreference.StartsWith("4")))
			{
				info.AddMessageError(Res.GetString("1FDA83E1-52CB-46DB-B4A4-22022ED5C632", "[C0919] Country of origin is mandatory for the chosen Preference code."));
			}
		}

		public void ValidateAdditionalProcedureCodesAsString()
		{
			ValidateCalculatedProperty(Parent.AdditionalProcedureCodesAsStringInfo);
		}

		protected virtual void CheckAdditionalProcedureCodesAsString()
		{
			var parent = Parent;
			foreach (AdditionalProcedureCode additionalProcedureCode in parent.AdditionalProcedureCodes)
			{
				additionalProcedureCode.Validation.ValidateCY_Code();
				parent.AdditionalProcedureCodesAsStringInfo.AddAllNotificationsFrom(additionalProcedureCode.CY_CodeInfo);
			}
		}

		public virtual void ValidateSupervisingOfficeDocAddress()
		{
		}

		public static string SupplementaryQtyShouldNotBeEmptyWhenWeHaveUnitsWarning => Res.GetString("04991C92-216E-4DF5-AFD0-CBE4C3892C56", "A unit for the Supplementary Quantity has been defined, perhaps from the tariff chosen, yet no quantity has been given. Supply a non-zero quantity.");

		protected override void CheckJI_Description()
		{
			base.CheckJI_Description();
			if (IsJIDescriptionMandatory && !Parent.JI_Tariff.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_DescriptionInfo);
			}
		}

		public void ValidateNotAllowCreateNewEntryLine()
		{
			if (Parent.ShouldKeepNotAllowCreateNewEntryLineError)
			{
				var entryHeader = Parent.EntryInstruction?.EntryHeader as CusEntryHeader;
				if (entryHeader != null)
				{
					Parent.AddRowError(GetNotAllowCreateNewEntryLineErrorMessage(entryHeader));
				}
			}
		}

		public void AddNotAllowCreateNewEntryLineError(CusEntryHeader entryHeader)
		{
			if (entryHeader != null)
			{
				Parent.ShouldKeepNotAllowCreateNewEntryLineError = true;
				Parent.ClearRowNotificationsContaining(GetNotAllowCreateNewEntryLineErrorMessage(entryHeader));
				Parent.AddRowError(GetNotAllowCreateNewEntryLineErrorMessage(entryHeader));
			}
		}

		public ZString GetNotAllowCreateNewEntryLineErrorMessage(CusEntryHeader entryHeader) => GetNotAllowCreateNewEntryLineErrorMessageCore(entryHeader);

		protected virtual ZString GetNotAllowCreateNewEntryLineErrorMessageCore(CusEntryHeader entryHeader) => Res.GetString("0CD2FF76-F9AC-4935-A3FE-0AB4B36D73A0", "Entry Declared ({0}) : Cannot add new Entry lines to a Declared or Canceled Entry. Adding a new Entry Instruction then performing 'Generate Entries (Merge)' could solve the problem.", entryHeader.CH_BGMReference);

		protected override void CheckJI_BondedWhsQuantity()
		{
			base.CheckJI_BondedWhsQuantity();

			if (Parent.IsBondedWhsQuantityVisible && InventoryManagementSettingMatchesProcedure)
			{
				if (Parent.ComponentInventoryCollection.Count == 0)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_BondedWhsQuantityInfo);
				}
				MandatoryValidation.MessageErrorIfIsNegative(Parent.JI_BondedWhsQuantityInfo);
			}
		}

		protected virtual bool InventoryManagementSettingMatchesProcedure => true;

		protected override void CheckJI_BondedWhsUnitQty()
		{
			base.CheckJI_BondedWhsUnitQty();

			if (Parent.IsBondedWhsQuantityVisible && InventoryManagementSettingMatchesProcedure)
			{
				if (Parent.ComponentInventoryCollection.Count == 0)
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_BondedWhsUnitQtyInfo);
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.JI_BondedWhsUnitQtyInfo);
				}
			}
		}

		protected override void CheckJI_InvoiceQuantity()
		{
			base.CheckJI_InvoiceQuantity();

			var targetInfo = Parent.JI_InvoiceQuantityInfo;
			var invoiceQuantity = Parent.JI_InvoiceQuantity;
			if (Parent.ShouldCreateCusPackagePivotFromInvoiceQuantityForSinglePackageType().shouldCreate && invoiceQuantity > Int32.MaxValue)
			{
				targetInfo.AddWarning(InvoiceQuantityWarningWhenTooLarge);
			}

			CheckQuantityLengthValidation(invoiceQuantity, targetInfo);
		}

		public virtual void ValidateFiscalReferences()
		{
			var declaration = Declaration;
			if (IsFiscalReferenceSupportedOnCPC42And63Only(declaration))
			{
				var procedure = Parent.JI_Procedure;
				if (procedure.StartsWith(CustomsProcedureCodeList.ProcedureCode._42) || procedure.StartsWith(CustomsProcedureCodeList.ProcedureCode._63))
				{
					var invoiceLineLevelSupportsFiscalReference = IsFiscalReferenceSupportedOnInvLine(declaration);
					var invoiceLineLevelHasNoFiscalReference = invoiceLineLevelSupportsFiscalReference && !Parent.FiscalReferences.Any();
					var instructionLevelSupportsFiscalReference = IsFiscalReferenceSupportedOnInstruction(declaration);
					var instructionLevelHasNoFiscalReference = instructionLevelSupportsFiscalReference && (!(Parent.EntryInstruction is CusEntryInstruction instruction) || !instruction.FiscalReferences.Any());
					if (invoiceLineLevelHasNoFiscalReference && instructionLevelHasNoFiscalReference)
					{
						Parent.JI_ProcedureInfo.AddMessageError(Res.GetString("57E695E1-163B-4CD5-B937-05DA7CE73687", "When CPC starts with 42 or 63, at least one Fiscal Reference record must exist at the Instruction or Line level."));
					}
					else if (invoiceLineLevelHasNoFiscalReference && !instructionLevelSupportsFiscalReference)
					{
						Parent.JI_ProcedureInfo.AddMessageError(Res.GetString("FDA8D311-C0BD-476E-9778-482B892F6379", "When CPC starts with 42 or 63, at least one Fiscal Reference record must exist at the Line level."));
					}
					else if (instructionLevelHasNoFiscalReference && !invoiceLineLevelSupportsFiscalReference)
					{
						Parent.JI_ProcedureInfo.AddMessageError(Res.GetString("7C77BDEF-8FCC-49C9-8AF5-87E5004C3D07", "When CPC starts with 42 or 63, at least one Fiscal Reference record must exist at the Instruction level."));
					}
				}
				else
				{
					if (InvoiceLineLevelHasFiscalReference(declaration))
					{
						Parent.JI_ProcedureInfo.AddMessageError(Res.GetString("7F3F2990-C392-4147-936A-911863763DFF", "Fiscal Representation should only be entered when CPC starts with 42 or 63."));
					}
				}
			}
		}

		static bool IsFiscalReferenceSupportedOnInvLine(JobDeclaration declaration)
		{
			return declaration?.Configuration.InvoiceLineConfiguration.FiscalReferencesSupport(declaration) ?? false;
		}

		static bool IsFiscalReferenceSupportedOnInstruction(JobDeclaration declaration)
		{
			return declaration?.Configuration.InstructionConfiguration.FiscalReferencesSupport(declaration) ?? false;
		}

		bool InvoiceLineLevelHasFiscalReference(JobDeclaration declaration)
		{
			return IsFiscalReferenceSupportedOnInvLine(declaration) && Parent.FiscalReferences.Any();
		}

		static bool IsFiscalReferenceSupportedOnCPC42And63Only(JobDeclaration declaration)
		{
			return declaration?.Configuration.InstructionConfiguration.FiscalReferencesSupportOnCPC42And63Only(declaration) ?? false;
		}

		void CheckQuantityQtyPresentWhenPositive(ZDecimal quantity, ZPropertyInfo quantityQtyInfo)
		{
			if (quantity > 0)
			{
				MandatoryValidation.MessageErrorIfNotEntered(quantityQtyInfo);
			}
		}

		void CheckQuantityLengthValidation(ZDecimal value, ZPropertyInfo info)
		{
			const int maxLength = 16;
			var numberOfDigits = value.GetNumberOfSignificantDigits();

			if (numberOfDigits > maxLength)
			{
				info.AddMessageError(Res.GetString("B91EA916-9A7E-4A69-86EA-71DBD499A5D1", "The number {0} is too large, the maximum number of digits allowed for {1} is 16.", value, info.HumanReadableName));
			}
		}

		string TariffInfoMessageErrorNoPackageDetails => Res.GetString("34DBD545-61CC-4337-A3BF-8E5D0681049E",
					"This line has no packaging details. This may contribute to a potential overall lack of packaging details. Please tick at least one row on the invoice line's {0} tab. If there are no rows offered, please ensure that declaration-level packages are showing on the declaration's packing tab. For a shipment-linked declaration, these data should be entered on the shipment's packing tab, and they will be synchronized forward to the declaration. If package details have been supplied on the shipment packing tab, but have not synchronized to the declaration's packing tab, it usually means that bill information has not been supplied. ",
					GetPackagesTabName());

		protected override string VATWarningMessageWhenTaxTypeIsNotEmpty => Res.GetString("FC6C8B19-0162-4954-9BD0-6D6FA689F4DD1", "Procedure {0} indicates that VAT does not apply, and will not be calculated.", Parent.ProcedureIndicatesVATNotApply);

		string InvoiceQuantityWarningWhenTooLarge => Res.GetString("CFB4E5E1-3D68-4DF1-B16E-98E2AB57EA65", "No package pivot could be created automatically because this value is too large.");

		protected virtual bool IsJIDescriptionMandatory => true;

		void CheckRuleC0002()
		{
			var parent = Parent;
			var entryInstruction = parent.EntryInstruction;

			if ((ValidationDecider is IImportInvoiceLineValidationDecider importValidationDecider && importValidationDecider.IsRuleC0002Active)
				&& parent.JI_OA_ExporterAddress.IsEmpty
				&& (entryInstruction != null && entryInstruction.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => !x.JI_OA_ExporterAddress.IsEmpty)))
			{
				parent.JI_OA_ExporterAddressInfo.AddMessageError(Res.GetString("EBA302E4-B3C2-4FD5-BF28-8C4BE1AD8F04", "[C0002] In case data entered in Invoice line level, all related lines must have a value in this field."));
			}
		}

		static readonly ImmutableArray<ZString> EntrySubStyleListC0710 = new ZString[] {
			EntrySubStyleList.Codes.NormalDeclaration,
			EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA,
			EntrySubStyleList.Codes.SupplementaryRecapitulativeDeclarationOfSimplifiedDeclarationsCoveredByCAndF,
			EntrySubStyleList.Codes.SupplementaryRecapitulativeDeclarationUnderTheProcedureCoveredUnderArticle182OfTheCode,
			EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF,
			EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic,
		}.ToImmutableArray();

		static readonly ImmutableArray<ZString> BulkPackageTypesR0222 = new ZString[]
		{
			UniversalReferenceConstants.RefCusCodeBulkPackageUnitType.BulkGas,
			UniversalReferenceConstants.RefCusCodeBulkPackageUnitType.BulkLiquid,
			UniversalReferenceConstants.RefCusCodeBulkPackageUnitType.BulkNodules,
			UniversalReferenceConstants.RefCusCodeBulkPackageUnitType.BulkLiquidGas,
			UniversalReferenceConstants.RefCusCodeBulkPackageUnitType.BulkGrains,
			UniversalReferenceConstants.RefCusCodeBulkPackageUnitType.BulkScrap,
			UniversalReferenceConstants.RefCusCodeBulkPackageUnitType.BulkPowders,
		}.ToImmutableArray();
	}
}
