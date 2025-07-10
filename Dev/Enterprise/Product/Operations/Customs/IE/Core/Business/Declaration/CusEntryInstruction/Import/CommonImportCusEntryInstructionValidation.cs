using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public abstract class CommonImportCusEntryInstructionValidation : CusEntryInstructionValidation
	{
		public CommonImportCusEntryInstructionValidation(CusEntryInstruction parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateFirstPlaceOfUseOrProcessingDescription();
			ValidateBR1030();
			ValidateBR2037();
			ValidateBR8F0012();
			ValidateDetailsOfPlannedActivities();
			ValidateBR8076();
			ValidateBR5153();
			ValidateBR0339();
		}

		public void ValidateFirstPlaceOfUseOrProcessingDescription()
		{
			ValidateCalculatedProperty(Parent.FirstPlaceOfUseOrProcessingDescriptionInfo);
		}

		protected override void CheckIdentificationofGoodsDetails()
		{
			base.CheckIdentificationofGoodsDetails();
			CheckBR8F0013();
		}

		void CheckBR8F0013()
		{
			var parent = Parent;
			if (parent.IdentificationofGoodsDetails.IsEmpty && parent.IsPlacesOfUsageAndDetailsOfPlannedActivitiesRequired)
			{
				parent.IdentificationofGoodsDetailsInfo.AddMessageError(Res.GetString("5FC80F77-FC26-4642-B456-B134BA13FA39", "[BR8F00013] Identification of Goods > Detail is required."));
			}
		}

		protected void CheckFirstPlaceOfUseOrProcessingDescription()
		{
			ValidateRuleBR8F0007();
			ValidateRuleBR8F0008();
		}

		void ValidateRuleBR8F0007()
		{
			var parent = Parent;
			if (parent.FirstPlaceOfUseOrProcessingDescription.IsEmpty && parent.IsPlacesOfUsageAndDetailsOfPlannedActivitiesRequired)
			{
				parent.FirstPlaceOfUseOrProcessingDescriptionInfo.AddMessageError(Res.GetString("33C02B5D-1753-4AEC-A065-EED8772D6374", "[BR8F0007] First Place of Use or Processing is required."));
			}
		}

		void ValidateRuleBR8F0008()
		{
			var parent = Parent;
			if (parent.PlaceOfUseOrProcessingCollection.IsNullOrEmpty() && parent.IsPlacesOfUsageAndDetailsOfPlannedActivitiesRequired)
			{
				parent.FirstPlaceOfUseOrProcessingDescriptionInfo.AddMessageError(Res.GetString("8F851546-21C9-4337-A536-FD490E1F4FB9", "[BR8F0008] Please enter at least one row in the below Places of Use or Processing grid."));
			}
		}

		void ValidateBR8F0012()
		{
			var parent = Parent;
			if (parent.IsH4 && parent.IsInwardProcessingProcedure51
				&& parent.ZG_ProcessedProductsCommodityCode.IsEmpty
				&& parent.HasAuthorisationForSpecialProcedure)
			{
				parent.AddRowMessageError(Res.GetString("D4C416CB-EF7C-44A1-84C1-945B0B568F17", "[BR8F0012] Please enter at least one Processed Product under the Entry Instructions > Special Procedures > Processed Products grid."));
			}
		}

		public void ValidateDetailsOfPlannedActivities()
		{
			ValidateCalculatedProperty(Parent.DetailsOfPlannedActivitiesInfo);
		}

		protected void CheckDetailsOfPlannedActivities()
		{
			ValidateBR8F0014();
		}

		void ValidateBR8F0014()
		{
			var parent = Parent;
			if (parent.DetailsOfPlannedActivities.IsEmpty && parent.IsPlacesOfUsageAndDetailsOfPlannedActivitiesRequired)
			{
				parent.DetailsOfPlannedActivitiesInfo.AddMessageError(Res.GetString("1E0707C4-C7A3-4DAB-92F0-A65D2687B706", "[BR8F0014] Details of Planned Activities is required."));
			}
		}

		void ValidateBR1030()
		{
			var parent = Parent;
			if (parent.CEI_Style == ImportDeclarationTypeList.Codes.H3 && parent.IsProcedureCode53)
			{
				if (!parent.CusAuthorizationUsages.Cast<CusAuthorizationUsage>().Any(a => a.AGC_Code.EqualsIgnoringCase(UniversalReferenceConstants.AuthorizationUsage.Codes.TEA)))
				{
					parent.AddRowMessageError(Res.GetString("D11AE68D-4D8B-476A-BD72-27DFE5D182AA", "[BR1030] Authorization code 'TEA' is required when declaration type is H3 and procedure code '53'."));
				}
			}
		}

		void ValidateBR2037()
		{
			var parent = Parent;
			if (parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => !x.AdditionalProcedureCodesIncludingConcession.Contains(UniversalReferenceConstants.ProcedureCodes.Concession.C08) && !HasSupportingDocumentForBR2037(x)))
			{
				parent.AddRowMessageError(RuleBR2037ErrorMessage);
			}
		}

		protected abstract string RuleBR2037ErrorMessage { get; }

		protected abstract bool IsValidForBR2037(ZString supportingDocumentCode);

		bool HasSupportingDocumentForBR2037(JobComInvoiceLine invoiceLine)
		{
			return HasInstructionSupportingDocumentForBR2037 || HasInvoiceLineSupportingDocumentForBR2037(invoiceLine) || HasInvoiceSupportingDocumentForBR2037(invoiceLine);
		}

		bool HasInstructionSupportingDocumentForBR2037 => Parent.Factory.GetValue(ref hasInstructionSupportingDocumentForBR2037Cached, () => ContainsSupportingDocumentForBR2037(Parent.SupportingDocuments));
		CachedProperty<bool> hasInstructionSupportingDocumentForBR2037Cached;

		bool HasInvoiceLineSupportingDocumentForBR2037(JobComInvoiceLine invoiceLine) => ContainsSupportingDocumentForBR2037(invoiceLine.SupportingDocuments);

		bool HasInvoiceSupportingDocumentForBR2037(JobComInvoiceLine invoiceLine)
		{
			var invoicePK = invoiceLine.JI_JZ;
			if (!InvoiceToHasSupportingDocumentForBR2037Map.TryGetValue(invoicePK, out var result) && invoiceLine.InvoiceHeader is JobComInvoiceHeader invoiceHeader)
			{
				result = ContainsSupportingDocumentForBR2037(invoiceHeader.SupportingDocuments);
				InvoiceToHasSupportingDocumentForBR2037Map.Add(invoicePK, result);
			}
			return result;
		}

		Dictionary<ZGuid, bool> InvoiceToHasSupportingDocumentForBR2037Map => Parent.Factory.GetValue(ref invoiceToHasSupportingDocumentForBR2037MapCached, () => new Dictionary<ZGuid, bool>());
		CachedProperty<Dictionary<ZGuid, bool>> invoiceToHasSupportingDocumentForBR2037MapCached;

		bool ContainsSupportingDocumentForBR2037(SupportingDocumentCollection supportingDocuments)
		{
			return supportingDocuments.Cast<SupportingDocument>().Any(doc => IsValidForBR2037(doc.CSI_Code));
		}

		protected override void CheckCEI_OA_Warehouse2()
		{
			base.CheckCEI_OA_Warehouse2();
			CheckToWarehouseRules();
		}

		void CheckToWarehouseRules()
		{
			ValidateCalculatedProperty(Parent.ToWarehouseTypeInfo);
			ValidateCalculatedProperty(Parent.ToWarehouseCodeInfo);
		}

		protected void CheckToWarehouseType()
		{
			CheckWarehousePropertyIsNotEmptyForDeclarationTypeH2(Parent.ToWarehouseTypeInfo);
		}

		protected void CheckToWarehouseCode()
		{
			CheckWarehousePropertyIsNotEmptyForDeclarationTypeH2(Parent.ToWarehouseCodeInfo);
			CheckWarehousePropertyForRuleBR2075(Parent.ToWarehouseCodeInfo);
		}

		void CheckWarehousePropertyIsNotEmptyForDeclarationTypeH2(ZPropertyInfo info)
		{
			var parent = Parent;
			if (info.Value.IsEmpty &&
				parent.CEI_Style == ImportDeclarationTypeList.Codes.H2 &&
				parent.CEI_SubStyle.In(new ZString[] { EntrySubStyleList.Codes.NormalDeclaration, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA }))
			{
				info.AddMessageError(Res.GetString("F1065084-6A70-4488-82F9-149CEE593997", "[C0626] Warehouse Type & ID are required for Declaration Type 'H2' if Sub Style is either 'A' or 'D'."));
			}
		}

		void CheckWarehousePropertyForRuleBR2075(ZPropertyInfo info)
		{
			var parent = Parent;

			if (info.Value.IsEmpty && parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(
				x => x.JI_Calc_RequestedProcedure == UniversalReferenceConstants.PreviousProcedureCodes.Codes._07
				&& x.JI_Calc_PreviousProcedure == UniversalReferenceConstants.PreviousProcedureCodes.Codes._00
			))
			{
				info.AddMessageError(Res.GetString("8D3E4F12-A217-4D3C-BDD0-92AB5297DC8C", "[BR2075] Warehouse ID is required for To Warehouse when Requested Procedure is '07' and at least one Previous Procedure is '00'. Please enter a To Warehouse with an EORI number."));
			}
		}

		protected override void CheckCEI_SubStyle()
		{
			base.CheckCEI_SubStyle();
			var parent = Parent;
			var subStyle = parent.CEI_SubStyle;
			var info = parent.CEI_SubStyleInfo;
			CheckCEI_SubStyle_BR8077(subStyle, info);

			CheckCEI_SubStyle_BR8063_NoAmend(subStyle, info);

			switch (parent.CEI_Style)
			{
				case ImportDeclarationTypeList.Codes.H1:
				case ImportDeclarationTypeList.Codes.H2:
				case ImportDeclarationTypeList.Codes.H3:
				case ImportDeclarationTypeList.Codes.H4:
				case ImportDeclarationTypeList.Codes.H6:
					CheckCEI_SubStyle_BR1010(subStyle, info);
					CheckCEI_SubStyle_BR1021(subStyle, info);
					CheckCEI_SubStyle_BR2027_NotI1(subStyle, info);
					break;
				case ImportDeclarationTypeList.Codes.I1:
					CheckCEI_SubStyle_BR1010(subStyle, info);
					CheckCEI_SubStyle_BR1021(subStyle, info);
					CheckCEI_SubStyle_BR2027_I1(subStyle, info);
					CheckCEI_SubStyle_BR6142(subStyle, info);
					break;
			}
		}

		void CheckCEI_SubStyle_BR8063_NoAmend(ZString subStyle, ZPropertyInfo info)
		{
			if (Parent.IsAmendmentValidationMode && !Parent.OriginalAdditionalDeclarationType.IsEmpty && Parent.OriginalAdditionalDeclarationType != subStyle)
			{
				info.AddMessageError(CommonResStrings.ShouldNotAmendThisValue);
			}
		}

		void CheckCEI_SubStyle_BR1010(ZString subStyle, ZPropertyInfo info)
		{
			if (!subStyle.IsEmpty && subStyle.In(new ZString[] { EntrySubStyleList.Codes.IncompleteDeclaration, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB, EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE }))
			{
				info.AddMessageError(Res.GetString("A1AC7BE3-245C-4745-8AD2-E5150D84CB78", "[BR1010] Additional Declaration Type cannot be B, E, or X."));
			}
		}

		void CheckCEI_SubStyle_BR1021(ZString subStyle, ZPropertyInfo info)
		{
			if (subStyle.IsEmpty)
			{
				info.AddMessageError(Res.GetString("4BC46BE8-7150-4120-91C2-CDFC9C8C316A", "[BR1021] Additional Declaration Type is required when Declaration Type is H1, H2, H3, H4, H6, or I1."));
			}
		}

		void CheckCEI_SubStyle_BR2027_I1(ZString subStyle, ZPropertyInfo info)
		{
			if (!subStyle.IsEmpty && !subStyle.In(new ZString[] { EntrySubStyleList.Codes.SimplifiedDeclaration, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC, EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic }))
			{
				info.AddMessageError(Res.GetString("BB95BE51-2F1F-4F43-9DEF-18D31907FDA5", "[BR2027] Additional Declaration Type can only contain one of the following values: C, F, Z."));
			}
		}

		void CheckCEI_SubStyle_BR2027_NotI1(ZString subStyle, ZPropertyInfo info)
		{
			if (!subStyle.IsEmpty && !subStyle.In(new ZString[] { EntrySubStyleList.Codes.NormalDeclaration, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA,
				EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF, EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic }))
			{
				info.AddMessageError(Res.GetString("572DA51D-FC7E-4494-860E-454BDCF24DC3", "[BR2027] Additional Declaration Type can only contain one of the following values: A, D, Y, Z."));
			}
		}

		void CheckCEI_SubStyle_BR6142(ZString subStyle, ZPropertyInfo info)
		{
			var parent = Parent;

			switch (subStyle)
			{
				case EntrySubStyleList.Codes.SupplementaryRecapitulativeDeclarationOfSimplifiedDeclarationsCoveredByCAndF:
				case EntrySubStyleList.Codes.SupplementaryRecapitulativeDeclarationUnderTheProcedureCoveredUnderArticle182OfTheCode:
				case EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE:
				case EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF:
				case EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic:
					if (!subStyle.IsEmpty && parent.IsTotalPriceLessThanOrEqualTo22EUR)
					{
						info.AddMessageError(Res.GetString("76855F8E-7F92-49EB-AD9A-7EF2AB4C8787", "[BR6142] Supplementary additional declaration types are not allowed when the previous dataset is 'I1', and the total invoice amount is less than or equal to €22"));
					}
					break;

				default:
					break;
			}
		}

		void CheckCEI_SubStyle_BR8077(ZString subStyle, ZPropertyInfo info)
		{
			var parent = Parent;
			var additionalProcedureCodesList = parent.PreviousDocuments.Where(x => x.CSI_Code == Constants.PreviousDocumentTypeCodes.MRN).ToList();
			if (subStyle == EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF && (additionalProcedureCodesList.Count != 1 || additionalProcedureCodesList[0].CSI_ReferenceNumber.Length != 18))
			{
				info.AddMessageError(Res.GetString("1CE25A36-3DA7-4CD9-AA2F-FBAC4FB58945", "[BR8077] Please enter a MRN number under Entry Instructions > Previous Documents grid with type 'MRN' when declaring a supplementary declaration."));
			}
		}

		protected override void CheckCEI_Style()
		{
			base.CheckCEI_Style();
			ValidateCEI_Style_BR1118();
			ValidateCEI_Style_BR2011();
			ValidateCEI_Style_BR3005();
			ValidateCEI_Style_BR3241();
		}

		void ValidateCEI_Style_BR3005()
		{
			var parent = Parent;
			var message = Res.GetString("DD93A80B-E801-4BE3-8BCC-EFBB9AD844E1", "[BR3005] Please enter an Additional Information code where Kind is 'INF' and Full Type is '00200' to the Entry Instructions > Additional Documents grid.");
			if (ShouldTriggerRuleBR3005(parent))
			{
				parent.CEI_StyleInfo.AddMessageError(message);
			}
		}

		protected abstract bool ShouldTriggerRuleBR3005(CusEntryInstruction parent);

		void ValidateCEI_Style_BR1118()
		{
			var parent = Parent;
			if (!parent.CEI_Style.EqualsIgnoringCase(ImportDeclarationTypeList.Codes.H5) && JobComInvoiceLineHelper.HasProcedureCodeConcessionF15(parent.InvoiceLines.Cast<JobComInvoiceLine>()))
			{
				parent.CEI_StyleInfo.AddMessageError(Res.GetString("E0527945-4B34-4081-AF00-71A5364C644B", "[BR1118] Declaration must be H5 when at least one invoice line has Additional Procedure Code F15"));
			}
		}

		void ValidateCEI_Style_BR2011()
		{
			var parent = Parent;
			if (parent.JobDeclaration != null && (!IsValidStyle() || !IsValidSubStyle())
				&& (!HasPreviousDocument && !HasInvoiceHeaderWithPreviousDocument))
			{
				AddBR2011MessageError();
			}

			void AddBR2011MessageError() => parent.CEI_StyleInfo.AddMessageError(Res.GetString("A6FDB414-7B51-464E-A5CF-63FAEE36A053", "[BR2011] Please enter at least one previous document under Entry Instructions > Previous Documents."));

			bool IsValidStyle() => parent.CEI_Style.In(new ZString[] {
				ImportDeclarationTypeList.Codes.H1, ImportDeclarationTypeList.Codes.H2,
				ImportDeclarationTypeList.Codes.H3, ImportDeclarationTypeList.Codes.H4,
				ImportDeclarationTypeList.Codes.H6, ImportDeclarationTypeList.Codes.H7,
				ImportDeclarationTypeList.Codes.I1
			});

			bool IsValidSubStyle() => parent.CEI_SubStyle.In(new ZString[] {
				EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA,
				EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC
			});
		}

		void ValidateCEI_Style_BR3241()
		{
			var parent = Parent;
			if (parent.CEI_Style.EqualsIgnoringCase(ImportDeclarationTypeList.Codes.H1))
			{
				var info = parent.CEI_StyleInfo;
				parent.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x =>
				{
					if (x.InvoiceHeader.IsSellerOrBuyerDifferentFromDeclaration() && !x.HasAddtionalInfoINF00200())
					{
						info.AddMessageError(Res.GetString("8E405ACC-6C48-4366-8234-8692DF6F9F11", "[BR3241] Please enter an Additional Document where Kind is 'INF' and Full Type is '00200' under Invoice Line: {{{0}}} > Additional Documents tab.", x.JI_LineNo));
					}
				});
			}
		}

		bool HasPreviousDocument => Parent.Factory.GetValue(ref hasPreviousDocumentCached, () => Parent.PreviousDocuments.Any());
		CachedProperty<bool> hasPreviousDocumentCached;

		bool HasInvoiceHeaderWithPreviousDocument => Parent.Factory.GetValue(ref hasInvoiceHeaderWithPreviousDocumentCached, () => Parent.Invoices.Cast<JobComInvoiceHeader>().Any(header => header.PreviousDocuments.Any()));
		CachedProperty<bool> hasInvoiceHeaderWithPreviousDocumentCached;

		void ValidateBR5153()
		{
			var parent = Parent;
			if (parent.IsInwardProcessingProcedure51 && !parent.HasAuthorisationForSpecialProcedureAdditionalInfoOnShipmentLevel && !parent.HasAuthorisationInwardProcessingProcedureOnShipmentLevel)
			{
				parent.AddRowMessageError(Res.GetString("62FB98C9-508F-43B9-8F71-2C5881E191A0", "[BR5153] If Requested Procedure is '51', then please either provide either a 'C601' Supporting Document under the Entry Instructions > Supporting Documents tab, or a '00100' Additional Information under the Entry Instructions > Additional Documents tab. Do not provide both."));
			}
		}

		void ValidateBR8076()
		{
			var parent = Parent;
			if (parent.IsInwardProcessingProcedure51 && !IsValidForBR8076(parent.ZG_ProcessingProcedureCode))
			{
				parent.AddRowMessageError(Res.GetString("778F6D3F-CA87-4747-ABC3-1A44E63A1A98", "[BR8076] If [11 09 001 000] Requested Procedure is '51', then [Annex A 6/2] Conditions and Terms > Economic Conditions > Processing Procedure Code can't be 6, 7, 8, 9 or 22."));
			}

			bool IsValidForBR8076(ZString processingProcedureCode)
			{
				switch (processingProcedureCode)
				{
					case ProcessingProcedureCodeList.Codes._6:
					case ProcessingProcedureCodeList.Codes._7:
					case ProcessingProcedureCodeList.Codes._8:
					case ProcessingProcedureCodeList.Codes._9:
					case ProcessingProcedureCodeList.Codes._22:
						return false;
					default:
						return true;
				}
			}
		}

		protected override void CheckCEI_OH_Owner()
		{
			base.CheckCEI_OH_Owner();
			CheckCEI_OH_Owner_BR8F0001();
		}

		void CheckCEI_OH_Owner_BR8F0001()
		{
			var entryInstruction = Parent;
			if (entryInstruction.CEI_Style.EqualsIgnoringCase(ImportDeclarationTypeList.Codes.H3) &&
				entryInstruction.IsProcedureCode53 &&
				entryInstruction.CEI_OH_Owner.IsEmpty && !entryInstruction.OwnerOfGoodsCollection.Any() &&
				entryInstruction.AdditionalInfos.Cast<AdditionalInfo>().Any(info =>
					info.IsAnAdditionalInformation
					&& info.CSI_Code.EqualsIgnoringCase(Constants.AdditionalReferenceCodes.AuthorisationForSpecialProcedureOtherThanTransit
			)))
			{
				entryInstruction.CEI_OH_OwnerInfo.AddMessageError(Res.GetString("7229DDEC-F371-4C09-8189-E82AA01B3146", "[BR8F0001] Primary Owner of Goods is required."));
			}
		}

		void ValidateBR0339()
		{
			var entryInstruction = Parent;
			if (entryInstruction.IsI1 && entryInstruction.CusAuthorizationUsages.Count is 0)
			{
				entryInstruction.AddRowMessageError(Res.GetString("51B04B61-F7BB-422E-9E77-93F9C35DF522", "[BR0339] At least one [3/39] Authorizations have to be filled in."));
			}
		}
	}
}
