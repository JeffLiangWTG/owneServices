using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public class SupportingDocumentValidation : CusSupportingInfoValidation
	{
		public SupportingDocumentValidation(SupportingDocument parent)
			: base(parent)
		{
		}

		protected new SupportingDocument Parent => (SupportingDocument)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();

			CheckUnitAndCurrencyWithOtherDocuments();
		}

		protected override void CheckCSI_AdditionalDescription()
		{
			base.CheckCSI_AdditionalDescription();
			CheckIssuingAuthorityNameForEucdm();
		}

		void CheckIssuingAuthorityNameForEucdm()
		{
			var parent = Parent;
			if (!parent.CSI_AdditionalDescription.IsEmpty)
			{
				var bo = parent.Parent;
				if (bo is ISupportingDocumentsProviderWithValidationDecider provider && (provider.ValidationDecider?.ShouldCheckIssuingAuthorityNameForEucdm ?? false))
				{
					if (bo is CusEntryInstruction instruction)
					{
						if (!IsH1OrI1(instruction.CEI_Style))
						{
							AddWarning();
						}
					}
					else if (bo is JobComInvoiceHeader invoice)
					{
						if (invoice.CusEntryInstructions.Cast<CusEntryInstruction>().Any(x => !IsH1OrI1(x.CEI_Style)))
						{
							AddWarning();
						}
					}
					else if (bo is JobComInvoiceLine invoiceLine)
					{
						if (invoiceLine.EntryInstruction is CusEntryInstruction entryInstruction && !IsH1OrI1(entryInstruction.CEI_Style))
						{
							AddWarning();
						}
					}
				}
			}

			void AddWarning() => parent.CSI_AdditionalDescriptionInfo.AddWarning(Res.GetString("BE06F2D6-4E25-4CCB-A55F-CC191A54DB41", "Issuing Authority Name is only declared when 'H1', or 'I1'."));

			bool IsH1OrI1(ZString code)
			{
				switch (code.ToUpper())
				{
					case EUCommonConstants.ImportDeclarationTypeList.H1:
					case EUCommonConstants.ImportDeclarationTypeList.I1:
						return true;
					default:
						return false;
				}
			}
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();

			CheckCodeCore();
			CheckCSI_CodeRuleC0612();
		}

		protected override void CheckCSI_Description()
		{
			if (!Parent.CSI_Code.IsEmpty && Parent.CSI_Description.IsEmpty && Parent.Lookups.StatementTextList.Count > 0)
			{
				Parent.CSI_DescriptionInfo.AddWarning(EmptyStatementTextWarning);
			}
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			if (Parent.ShowCodeFindBoxForReferenceNumber)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CSI_ReferenceNumberInfo);
			}

			if (ShouldCheckReferenceNumberMandatoryBasedOnCusConditions)
			{
				CheckReferenceNumberMandatoryBasedOnCusConditions();
			}

			CheckRuleBR20311IfApplicable();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		void CheckRuleBR20311IfApplicable()
		{
			const string patternYYYYMMDD = @"^[12]\d{3}((0[1-9])|(1[012]))((0[1-9])|([12]\d)|(3[01]))$";
			var parent = Parent;
			if (parent.Parent is ISupportingDocumentsProviderWithValidationDecider provider && (provider.ValidationDecider?.SupportBR20311Rule ?? false))
			{
				if (parent.CSI_Code.In(new ZString[]
					{
						UniversalReferenceConstants.SupportingDocumentTypes.U164,
						UniversalReferenceConstants.SupportingDocumentTypes.U165,
						UniversalReferenceConstants.SupportingDocumentTypes.U166,
						UniversalReferenceConstants.SupportingDocumentTypes.U167
					}) &&
					!Regex.Match(parent.CSI_ReferenceNumber, patternYYYYMMDD).Success)
				{
					parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString(
						"9A687F2F-BC40-49E9-B434-8B7BEE47845D",
						"The format of Supporting Document Reference Number must be YYYYMMDD when Type is U164, U165, U166 or U167."));
				}
			}
		}

		protected virtual bool ShouldCheckReferenceNumberMandatoryBasedOnCusConditions => true;

		void CheckReferenceNumberMandatoryBasedOnCusConditions()
		{
			if (Parent.Parent is JobComInvoiceLine invoiceLine)
			{
				var universalTariff = invoiceLine.UniversalTariff;
				if (universalTariff != null)
				{
					var supportingDocumentCode = Parent.CSI_Code;
					var supportingDocumentReferenceNumber = Parent.CSI_ReferenceNumber;

					if (!supportingDocumentCode.IsEmpty && supportingDocumentReferenceNumber.IsEmpty)
					{
						var applicableConditions = ConditionChecker.GetApplicableConditions(Parent.Factory, universalTariff, invoiceLine.ConditionSelectionCriterias);
						var supConditionValueList = applicableConditions.SelectMany(x => x.ConditionValues).Where(x => x.ValueType == Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument).Select(x => x.ZX3_Value);
						if (supConditionValueList.Contains(supportingDocumentCode))
						{
							MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
						}
					}
					invoiceLine.Validation.ValidateJI_Tariff();
				}
			}
		}

		protected override void CheckCSI_RX_NKCurrency()
		{
			base.CheckCSI_RX_NKCurrency();
			CheckCSI_RX_NKCurrencyListValidationCore();
		}

		protected virtual void CheckCSI_RX_NKCurrencyListValidationCore()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_RX_NKCurrencyInfo);
		}

		protected virtual void CheckCodeCore()
		{
			if (Parent.CSI_Code.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_CodeInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo);
			}

			CheckDuplicatedDocuments(Parent.CSI_CodeInfo);
		}

		protected virtual void CheckDuplicatedDocuments(ZPropertyInfo info)
		{
			var invLine = Parent.Parent as JobComInvoiceLine;
			if (invLine != null)
			{
				if (HasDuplicateSupportingDocuments(invLine.SupportingDocuments.OfType<SupportingDocument>().ToList()))
				{
					info.AddMessageError(Res.GetString("917A60F6-D442-41C3-B782-1702041CE2EC", "A row with that document type and reference already exists on this invoice line."));
				}
			}
		}

		protected bool HasDuplicateSupportingDocuments(List<SupportingDocument> docs)
		{
			var keyToDeterimeUniqueness = Parent.KeyToDeterimeUniqueness;
			var pk = Parent.PK;
			return (from SupportingDocument sd in docs
					where sd.PK != pk && sd.KeyToDeterimeUniqueness == keyToDeterimeUniqueness
					select sd).Any();
		}

		protected virtual void CheckUnitAndCurrencyWithOtherDocuments()
		{
			Parent.RemoveRowMessageError(InconsistentUnitOrCurrency);

			if (Parent.SupportsPermitIntegration && Parent.Parent is ISupportingDocumentsProvider supportingDocsProvider)
			{
				var supportDocsCollection = supportingDocsProvider.SupportingDocuments;
				var supportDocsSharingSamePermit = supportDocsCollection.Cast<SupportingDocument>().Where(x => x.CSI_ReferenceNumber == Parent.CSI_ReferenceNumber);
				var differentUnits = supportDocsSharingSamePermit.Select(x => x.CSI_UnitOfQuantity).Distinct();
				var differentCurrencies = supportDocsSharingSamePermit.Select(x => x.CSI_RX_NKCurrency).Distinct();
				if (differentUnits.Count() > 1 || differentCurrencies.Count() > 1)
				{
					Parent.AddRowMessageError(InconsistentUnitOrCurrency);
				}
			}
		}
		void CheckForAggregateQuantityForLineOnlyDocumentSuppliedAndInvHeader(ZPropertyInfo zPropertyInfo)
		{
			if (Parent.IsLineOnly
					&& (Parent.ImportExportParent is JobComInvoiceHeader invHeader && invHeader.InvoiceLines.Count > 1)
				)  // only need to worry about aggregatable quantities when multiple invoicelines exist
			{
				if (!zPropertyInfo.Value.IsEmpty)
				{
					zPropertyInfo.AddWarning(WarningForAggregateableFields);
				}
			}
		}

		protected override void CheckCSI_Quantity()
		{
			base.CheckCSI_Quantity();

			var parent = Parent;
			CheckForAggregateQuantityForLineOnlyDocumentSuppliedAndInvHeader(parent.CSI_QuantityInfo);

			var quantityDigitsCount = decimal.Truncate(parent.CSI_Quantity).ToString().Length + parent.CSI_Quantity.DecimalPlaces;
			if (quantityDigitsCount > 16)
			{
				parent.CSI_QuantityInfo.AddMessageError(
					Res.GetString(
						"2BD28070-841D-4B71-A055-90B82DFFB20B",
						"Supporting documents > Quantity, the maximum allowed number of digits is 16."));
			}
		}

		protected override void CheckCSI_Quantity2()
		{
			base.CheckCSI_Quantity2();
			CheckForAggregateQuantityForLineOnlyDocumentSuppliedAndInvHeader(Parent.CSI_Quantity2Info);
		}

		protected override void CheckCSI_Quantity3()
		{
			base.CheckCSI_Quantity3();
			CheckForAggregateQuantityForLineOnlyDocumentSuppliedAndInvHeader(Parent.CSI_Quantity3Info);
		}

		protected override void CheckCSI_Value()
		{
			base.CheckCSI_Value();
			CheckForAggregateQuantityForLineOnlyDocumentSuppliedAndInvHeader(Parent.CSI_ValueInfo);
		}

		void CheckCSI_CodeRuleC0612()
		{
			var parent = Parent;

			if (!parent.ParentIsJobComInvoiceLine
				&& parent.Parent is ISupportingDocumentsProviderWithValidationDecider supportingDocumentProvider
				&& (supportingDocumentProvider.ValidationDecider?.SupportC0612Rule ?? false))
			{
				CusSupportingInfoValidationHelper.ValidateRuleC0612(parent.CSI_Code, parent.CSI_CodeInfo);
			}
		}

		public static string WarningForAggregateableFields => Res.GetString("C61F9E53-841F-4BF0-9FC6-799563ED5B52", "This supporting document applies to the entry line. This amount is not apportioned among the invoice lines, you are recommended to instead supply the document and relevant amount against each invoice line");
		public static string InconsistentUnitOrCurrency => Res.GetString("8A242555-62D9-400C-B644-0E4C91A29EFE", "Documents sharing the same permit must show same unit of quantity and currency.");
		public static string EmptyStatementTextWarning => Res.GetString("7924685D-EA45-4C96-A89B-CF4A62DADF0D", "This document code allows a statement text (reason / description), and {0} knows about at least one allowable option, please select one", Core.Constants.ProductName);
	}
}
