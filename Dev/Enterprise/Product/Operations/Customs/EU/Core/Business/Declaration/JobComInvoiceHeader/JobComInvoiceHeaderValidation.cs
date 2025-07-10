using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.MasterFiles.Business.Customs.EU;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class JobComInvoiceHeaderValidation : InvoiceHeaderValidation
	{
		public JobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

		protected override void CheckJZ_Weight()
		{
			base.CheckJZ_Weight();
			var headerWeight = Parent.JZ_Weight;
			if (!headerWeight.IsEmpty)
			{
				var sumWeightsSameTypeAsHeader = TotalLineGrossWeightInHeaderUnitOfMesure();
				if (headerWeight != sumWeightsSameTypeAsHeader)
				{
					Parent.JZ_WeightInfo.AddWarning(Res.GetString("A80BE535-E348-403B-A9FE-A8E53FDC7150", "The sum of gross weight {0} {1} in Invoice Lines does not match with the total gross weight of the invoice.", sumWeightsSameTypeAsHeader, Parent.JZ_WeightUQ));
				}
			}
		}

		protected override void CheckJZ_NetWeight()
		{
			base.CheckJZ_NetWeight();
			var netWeight = Parent.JZ_NetWeight;
			if (netWeight != 0)
			{
				var sumNetWeightsSameTypeAsHeader = TotalLineNetWeightInHeaderUnitOfMesure();
				if (netWeight != sumNetWeightsSameTypeAsHeader)
				{
					Parent.JZ_NetWeightInfo.AddWarning(Res.GetString("090A9A5C-39AA-4696-AF0C-175F9E8AB280", "The sum of {0} {1} {2} in Invoice Lines does not match with the total net weight of the invoice.", Parent.JZ_NetWeightInfo.HumanReadableName, sumNetWeightsSameTypeAsHeader, Parent.JZ_NetWeightUQ));
				}
			}
		}

		protected override void AddWarningOrMessageErrorToJZ_InvoiceCurrExRateInfoWhenExchangeRateStale(ZPropertyInfo exchangeRateInfo)
		{
			// Base adds a warning.  We in EU would prefer a message error.
			exchangeRateInfo.AddMessageError(ExchangeRateOutOfDateWarningMessage);
		}

		// ******************
		//	This should be removed, and we should revert to base, when proper value-build up gets in
		protected override void CheckJZ_Calc_CIFAmount()
		{
			if (Parent.JZ_Calc_CIFAmount < 0)
			{
				Parent.JZ_Calc_CIFAmountInfo.AddMessageError(Res.GetString("3702A63A-1524-4228-A883-10D6CB53ECD7", "The CIF amount should be greater than zero before submitting this declaration.\r\nThis usually happens when you have entered more invoice charge amounts than invoice total amount."));
			}
		}

		protected override void CheckJZ_InvoiceNumber()
		{
			base.CheckJZ_InvoiceNumber();
			var targetPropertyInfo = Parent.JZ_InvoiceNumberInfo;
			var declaration = Parent.JobDeclaration;
			if (declaration != null
							&& ShouldCheckMissingPreviousDocuments
							&& Parent.PreviousDocuments.Count == 0 && declaration.Configuration.InvoiceHeaderConfiguration.PreviousDocumentsSupport(declaration)
							&& (declaration.PreviousDocuments.Count == 0 || !declaration.Configuration.MiscPreviousDocumentsSupport(declaration))
							&& (Parent.InvoiceLines.Count == 0 || !declaration.Configuration.InvoiceLineConfiguration.PreviousDocumentsSupport(declaration) || Parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(invoiceLine => invoiceLine.PreviousDocuments.Count == 0))
							&& (!PreviousDocumentsCheckExceptedDeclarationTypes.Contains(declaration.CustomsEntryInstructions.FirstOrDefault()?.CEI_Style ?? ZString.Empty)))
			{
				targetPropertyInfo.AddWarning(EntryMayBeRejectedBecauseNoPreviousDocuments);
			}

			if (Parent.AddingSupportingDocumentAutomaticallyEnabled && Parent.NeedAtLeastOneInvoiceSupportingDocument && Parent.JZ_InvoiceNumber.IsEmpty)
			{
				targetPropertyInfo.AddWarning(Res.GetString("2702BAF6-4988-4C23-8A04-90254C2CB8FC", "A Supporting Document of type N380 can’t be created automatically because invoice has no {0}", targetPropertyInfo.HumanReadableName));
			}

			if (declaration?.Invoices.Any(x => x.JZ_InvoiceNumber == Parent.JZ_InvoiceNumber && x.PK != Parent.PK) == true)
			{
				Parent.JZ_InvoiceNumberInfo.AddMessageError(Res.GetString("4E6D9F4-B173-41EE-82EF-D0A9B0A872D8", "Invoice numbers within a declaration must be unique."));
			}
		}

		protected override bool WarnIfInvoiceNumberAlreadyExistsInThisDeclaration => false;

		protected virtual IEnumerable<ZString> PreviousDocumentsCheckExceptedDeclarationTypes
		{
			get { return Array.Empty<ZString>(); }
		}

		protected override void CheckJZ_ValuationCode()
		{
			base.CheckJZ_ValuationCode();

			ListValidation.MessageErrorIfInvalidCode(Parent.JZ_ValuationCodeInfo, Parent.Lookups.ValuationCodeList);

			var isRuleC0627Active = ValidationDecider?.IsRuleC0627Active ?? false;
			var isRuleR0012AndC0002Active = (ValidationDecider?.IsRuleR0012Active ?? false) && (ValidationDecider?.IsRuleC0002Active ?? false);

			CheckRuleC0627();
			CheckRuleR0012AndC0002();

			if (IsJZ_ValuationCodeMandatory && !isRuleC0627Active && !isRuleR0012AndC0002Active)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_ValuationCodeInfo);
			}
		}

		protected virtual bool IsJZ_ValuationCodeUsed => true;

		void CheckRuleC0627()
		{
			if (IsJZ_ValuationCodeUsed && (ValidationDecider?.IsRuleC0627Active ?? false))
			{
				var parent = Parent;
				if (parent.CusEntryInstructions.Cast<CusEntryInstruction>().All(x => x.IsSimplifiedOrPreliminaryUnderCodeC))
				{
					if (!parent.JZ_ValuationCode.IsEmpty)
					{
						parent.JZ_ValuationCodeInfo.AddMessageError(Res.GetString("E23F15A3-CFF1-44A2-8D6E-EA8F8CCE6E1D", "[C0627] This field must be empty in case of Declaration Sub Type C or F."));
					}
				}
				else
				{
					if (parent.JZ_ValuationCode.IsEmpty && parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.ZG_TransNature.IsEmpty))
					{
						parent.JZ_ValuationCodeInfo.AddMessageError(Res.GetString("162BD7ED-4D48-4BE4-A5D0-5685BF8B9770", "[C0627] This field is mandatory for this declaration Sub Type."));
					}
				}
			}
		}

		protected override void CheckJZ_IncoTerm()
		{
			base.CheckJZ_IncoTerm();
			var parent = Parent;

			var declaration = parent.JobDeclaration;
			if (declaration != null)
			{
				var incoTerm = parent.JZ_IncoTerm;

				if (declaration.Configuration.IsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoice)
				{
					if (incoTerm != declaration.JE_ShipmentIncoTerm)
					{
						parent.JZ_IncoTermInfo.AddMessageError(Res.GetString("0088EED0-67DB-4D75-A1A4-0613951A56B8", "Incoterm values do not match between Declaration and Invoice Header."));
					}
				}

				if (ShouldValidateWaterIncoTermAndTransportMode)
				{
					IncoTermValidationHelper.ValidateWaterIncoTermAndTransportMode(parent.JZ_IncoTermInfo, declaration.JE_TransportMode);
				}
			}
		}

		protected override bool IncoTermRequired
		{
			get
			{
				return IsIncotermRequiredAsPerRuleC0729(Parent.JobDeclaration) || IsIncotermRequiredAsPerRuleC0738(Parent.JobDeclaration);
			}
		}

		bool IsIncotermRequiredAsPerRuleC0729(JobDeclaration jobDeclaration)
		{
			var result = base.IncoTermRequired;
			var cusEntryInstuctions = jobDeclaration?.CustomsEntryInstructions;
			if (ValidationDecider?.IsRuleC0729Active ?? false)
			{
				result = cusEntryInstuctions.IsNullOrEmpty() || cusEntryInstuctions.Any(x => !subStylesForRuleC0729.Contains(x.CEI_SubStyle) || !proceduresForRuleC0729.Contains(x.CEI_Procedure));
			}
			return result;
		}

		readonly List<string> subStylesForRuleC0729 = new List<string>() { EntrySubStyleList.Codes.NormalDeclaration, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA, EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF };
		readonly List<string> proceduresForRuleC0729 = new List<string>() { Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration, Core.Constants.Customs.Universal.RefCusProcedure.Codes._53 };

		bool IsIncotermRequiredAsPerRuleC0738(JobDeclaration jobDeclaration)
		{
			var result = base.IncoTermRequired;
			if (ValidationDecider?.IsRuleC0738Active ?? false)
			{
				result = jobDeclaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.JI_ValuationCode.Equals(ValuationMethodList.Codes._1));
			}
			return result;
		}

		public virtual bool ShouldValidateWaterIncoTermAndTransportMode => true;

		protected override void CheckJZ_IncoTermPlace()
		{
			var incoTermPlaceInfo = Parent.JZ_IncoTermPlaceInfo;
			if (!incoTermPlaceInfo.ReadOnly)
			{
				base.CheckJZ_IncoTermPlace();

				if (RequireJZ_IncoTermPlaceMandatory)
				{
					MandatoryValidation.MessageErrorIfNotEntered(incoTermPlaceInfo);
				}
			}
		}

		protected virtual ZBool RequireJZ_IncoTermPlaceMandatory => (Parent.AgreedPlaceCodeSupportAndVisible && Parent.ZG_AgreedPlaceCode.Length == 2)
			|| (Parent.AgreedPlaceCodeSupport && Parent.JZ_IncoTerm == Core.Constants.IncoTerms.Other);

		protected override void CheckJZ_InvoiceDate()
		{
			base.CheckJZ_InvoiceDate();
			if (Parent.AddingSupportingDocumentAutomaticallyEnabled && Parent.NeedAtLeastOneInvoiceSupportingDocument && Parent.JZ_InvoiceDate.IsEmpty)
			{
				Parent.JZ_InvoiceDateInfo.AddWarning(Res.GetString("C8658A50-76BE-4BC7-A0FC-9ECDA1014155", "A Supporting Document of type N380 can’t be created automatically because invoice has no {0}", Parent.JZ_InvoiceDateInfo.HumanReadableName));
			}
		}

		protected override void CheckJZ_OH_Supplier()
		{
			base.CheckJZ_OH_Supplier();
			if (Parent.AddingSupportingDocumentAutomaticallyEnabled && Parent.NeedAtLeastOneInvoiceSupportingDocument && Parent.Supplier_Effective == null)
			{
				Parent.JZ_OH_SupplierInfo.AddWarning(Res.GetString("32CED8D4-9E58-4222-A8AA-E81BD4B8F9EF", "A Supporting Document of type N380 can’t be created automatically because invoice has no Supplier and Declaration has no Supplier"));
			}
		}

		protected override void CheckJZ_OA_SupplierAddress()
		{
			base.CheckJZ_OA_SupplierAddress();

			CheckRuleC0002();
			CheckSupplierCannotBeSavedBecauseNoAddressIsSelected();
		}

		protected virtual void CheckSupplierCannotBeSavedBecauseNoAddressIsSelected()
		{
			var parent = Parent;
			if (!parent.SupplierOrgPK.IsEmpty && parent.JZ_OA_SupplierAddress.IsEmpty)
			{
				parent.JZ_OA_SupplierAddressInfo.AddWarning(Res.GetString("EU.ComInvoiceHeaderValidation|JZ_OA_Supplier_Empty", "Supplier organization will not be saved because no address is selected."));
			}
		}

		protected override void CheckJZ_OA_BuyerAddress()
		{
			base.CheckJZ_OA_BuyerAddress();

			var parent = Parent;
			if (!parent.BuyerOrgPK.IsEmpty && parent.JZ_OA_BuyerAddress.IsEmpty)
			{
				parent.JZ_OA_BuyerAddressInfo.AddWarning(BuyerAddressEmptyWarningMessage);
			}
		}

		protected virtual string BuyerAddressEmptyWarningMessage => Res.GetString("EU.JobComInvoiceHeaderValidation|JZ_OA_BuyerAddress_Empty", "Buyer organization will not be saved because no address is selected.");

		protected override void CheckJZ_OA_SellerAddress()
		{
			base.CheckJZ_OA_SellerAddress();

			var parent = Parent;
			if (!parent.SellerOrgPK.IsEmpty && parent.JZ_OA_SellerAddress.IsEmpty)
			{
				parent.JZ_OA_SellerAddressInfo.AddWarning(Res.GetString("EU.JobComInvoiceHeaderValidation|JZ_OA_SellerAddress_Empty", "Seller organization will not be saved because no address is selected."));
			}
		}

		protected override void CheckJZ_OA_ExporterAddress()
		{
			base.CheckJZ_OA_ExporterAddress();

			var parent = Parent;
			if (!parent.ExporterOrgPK.IsEmpty && parent.JZ_OA_ExporterAddress.IsEmpty)
			{
				parent.JZ_OA_ExporterAddressInfo.AddWarning(Res.GetString("EU.JobComInvoiceHeaderValidation|JZ_OA_ExporterAddress_Empty", "Exporter organization will not be saved because no address is selected."));
			}
		}

		protected virtual ZString EntryMayBeRejectedBecauseNoPreviousDocuments => Res.GetString("D68E7A61-B65D-4718-9E29-A8B30CE20F6F", "This invoice has no previous documents and not all of its invoice lines have one. Without a previous document the entry may be rejected.");

		protected virtual bool IsJZ_ValuationCodeMandatory => true;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateNeedAtLeastOneInvoiceSupportingDocument();
		}

		protected override void CheckBuyerOrgPK()
		{
			base.CheckBuyerOrgPK();
			CheckRuleC0728(Parent.BuyerOrgPKInfo);
		}

		protected override void CheckSellerOrgPK()
		{
			base.CheckSellerOrgPK();
			CheckRuleC0728(Parent.SellerOrgPKInfo);
		}

		void CheckRuleC0728(ZPropertyInfo info)
		{
			if ((ValidationDecider?.IsRuleC0728Active ?? false) && info.Value.IsValid && Parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(line => line.ShouldNotHaveTraders()))
			{
				info.AddMessageError(Res.GetString("F01A2BDC-575B-4726-B9F9-7BEC0D66190A", "[C0728] – This field must be empty for this Requested Procedure / Additional Declaration / Declaration Sub Type."));
			}
		}

		protected override void CheckJZ_RelatedIndicator()
		{
			base.CheckJZ_RelatedIndicator();
			CheckRuleC0624_InvoiceHeader(Parent, Parent.RelatedIndicatorInfo);
		}

		internal void CheckRuleC0624_InvoiceHeader(JobComInvoiceHeader invoiceHeader, ZPropertyInfo relatedIndicatorInfo)
		{
			if (invoiceHeader is JobComInvoiceHeader invoice
				&& (ValidationDecider?.IsRuleC0624Active ?? false)
				&& (ZBool)relatedIndicatorInfo.Value
				&& invoiceHeader.InvoiceLines.Cast<JobComInvoiceLine>().All(line => EntryInstruction_FallIntoRuleC0624(line.EntryInstruction)))
			{
				relatedIndicatorInfo.AddMessageError(errorStringC0624);
			}
		}

		bool EntryInstruction_FallIntoRuleC0624(CusEntryInstruction entryInstruction)
		{
			return entryInstruction != null
					&& (entryInstruction.IsSimplifiedOrPreliminaryUnderCodeC || entryInstruction.CEI_Procedure == Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration || entryInstruction.CEI_Procedure == Core.Constants.Customs.Universal.RefCusProcedure.Codes._53);
		}
#pragma warning disable IDE0044 // conflicting warnings ( conflict between IDE0044 and Res.GetString ) suppressing the latest one, i.e., IDE0044
		string errorStringC0624 = Res.GetString("4309E3E2-4E2D-4DD9-963A-C0D7F43EE48F", "[C0624] Value indicator should not be entered for the provided Declaration Type and Requested Procedure.");
#pragma warning restore IDE0044 // Restoring IDE0044
		void CheckRuleR0012AndC0002()
		{
			if (IsJZ_ValuationCodeUsed && (ValidationDecider?.IsRuleR0012Active ?? false) && (ValidationDecider?.IsRuleC0002Active ?? false))
			{
				if (Parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(l => !l.ZG_TransNature.IsEmpty) &&
					Parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(l => l.ZG_TransNature.IsEmpty))
				{
					if (Parent.JZ_ValuationCode.IsEmpty)
					{
						Parent.JZ_ValuationCodeInfo.AddMessageError(Res.GetString("baad0bc3-2d3c-4cd0-a0fc-1360d6b388e5", "[R0012 & C0002] If Nature Of transaction is not entered in all invoice lines, it must be entered in Invoice Header also."));
					}
					else
					{
						Parent.JZ_ValuationCodeInfo.AddWarning(Res.GetString("52a30a7b-7395-4ce0-9a40-63af00f3e08d", "Invoice lines without value in Nature of Transaction will be mapped from the Invoice header value."));
					}
				}
			}
		}

		#region NeedAtLeastOneInvoiceSupportingDocument

		protected virtual bool ShouldValidateNeedAtLeastOneInvoiceSupportingDocument => true;

		protected virtual bool ShouldValidateNeedAtLeastOneInvoiceSupportingDocumentInAddedInvoiceLine => false;

		protected virtual CargoWise.ComponentModel.INotificationType AtLeastOneSupportingDocumentMessageNotificationType => NotificationType.MessageError;

		void ValidateNeedAtLeastOneInvoiceSupportingDocument()
		{
			var parent = Parent;
			var message = GetAtLeastOneSupportingDocumentMessage();

			parent.ClearRowNotificationsContaining(message);

			if (parent.AddingSupportingDocumentAutomaticallyEnabled
				&& ShouldValidateNeedAtLeastOneInvoiceSupportingDocument
				&& parent.NeedAtLeastOneInvoiceSupportingDocument
				&& CheckNeedAtLeastOneInvoiceSupportingDocumentInAddedInvoiceLines())
			{
				parent.AddRowNotification(new CargoWise.ComponentModel.Notification(AtLeastOneSupportingDocumentMessageNotificationType, message));
			}
		}

		bool CheckNeedAtLeastOneInvoiceSupportingDocumentInAddedInvoiceLines()
		{
			return !ShouldValidateNeedAtLeastOneInvoiceSupportingDocumentInAddedInvoiceLine || !ContainsAtLeastOneInvoiceSupportingDocumentInAddedInvoiceLine;
		}

		ZBool ContainsAtLeastOneInvoiceSupportingDocumentInAddedInvoiceLine => !(Parent.InvoiceLines.Count == 0) && Parent.InvoiceLines.Cast<JobComInvoiceLine>().All(x => x.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.IsCodeAnInvoiceType));

		ZString GetAtLeastOneSupportingDocumentMessage()
		{
			return Res.GetString("946BFAC3-D4F6-45F9-B44D-243CBFB9D859", "At least one Supporting Document of type: {0} must be present at the Invoice Header level or in all of its Invoice Lines.", GetSupportingDocumentTypesString());

			string GetSupportingDocumentTypesString()
			{
				var supportingDocumentArray = Parent.SupportingDocuments.Helper.GetInvoiceSupportingDocumentTypes().ToArray();

				if (supportingDocumentArray.Length == 1)
				{
					return supportingDocumentArray[0];
				}

				if (supportingDocumentArray.Length > 1)
				{
					return new ZStringBuilder(string.Join(", ", supportingDocumentArray.Take(supportingDocumentArray.Length - 1)))
					   .Append((NoResString)" or ")
					   .Append(supportingDocumentArray[supportingDocumentArray.Length - 1])
					   .ToString();
				}

				return string.Empty;
			}
		}

		#endregion

		protected virtual ZBool ShouldCheckMissingPreviousDocuments => !Parent.InvoiceLines.Any() || Parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.ShouldCheckMissingPreviousDocuments);

		public IInvoiceHeaderValidationDecider ValidationDecider => Parent.Factory.GetValue(ref validationDeciderCached, GetValidationDecider);
		CachedProperty<IInvoiceHeaderValidationDecider> validationDeciderCached;

		IInvoiceHeaderValidationDecider GetValidationDecider() => Parent.JobDeclaration?.Configuration.InvoiceHeaderConfiguration.GetValidationDecider(Parent);

		ZDecimal TotalLineGrossWeightInHeaderUnitOfMesure()
		{
			var weight = ZDecimal.Zero;
			var weightUQ = Parent.JZ_WeightUQ;
			weight = ((ZDecimal)Parent.InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => Core.Constants.Weight.ConvertSafe(x.JI_Weight, x.JI_WeightUQ, weightUQ))).Round(3).Normalize();
			return weight;
		}

		ZDecimal TotalLineNetWeightInHeaderUnitOfMesure()
		{
			var netWeight = ZDecimal.Zero;
			var netWeightUQ = Parent.JZ_NetWeightUQ;
			netWeight = ((ZDecimal)Parent.InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => Core.Constants.Weight.ConvertSafe(x.JI_CustomsQuantity, x.CustomsQuantityConverter.GetEffectiveWeightUnit(x.JI_CustomsUnitQty), netWeightUQ))).Round(3).Normalize();
			return netWeight;
		}

		void CheckRuleC0002()
		{
			var parent = Parent;

			if ((ValidationDecider?.IsRuleC0002Active ?? false)
				&& !parent.JZ_OA_SupplierAddress.IsEmpty && parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => !x.JI_OA_ExporterAddress.IsEmpty))
			{
				parent.JZ_OA_SupplierAddressInfo.AddMessageError(Res.GetString("0FAAB521-F643-44DB-A491-7A115E3A79D0", "[C0002] Value can’t be entered in both Invoice header and Invoice lines."));
			}
		}
	}
}
