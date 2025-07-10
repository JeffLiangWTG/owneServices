using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public class CusEntryInstructionValidation : AutoCNCusEntryInstructionValidation
	{
		public CusEntryInstructionValidation(CusEntryInstruction parent) : base(parent)
		{
		}

		internal IValidationModeProvider ValidationModeProvider => Parent.JobDeclaration;

		protected new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidateMaxCountOfEntryLines();
			ValidateSpecialBusinessIdentifiersAsString();
			ValidateBillOfLading();
			ValidateBillOfLadingDate();
			ValidateOperationMattersAsString();
			ValidateInvoiceHeadersAndLines();
		}

		public void ValidateMaxCountOfEntryLines()
		{
			var entryHeader = Parent.EntryHeader as CusEntryHeader;
			if (entryHeader != null && entryHeader.MergedLines.Count > entryHeader.MaximumEntryLinesAllowed)
			{
				Parent.AddRowNotification(GetMaximumEntryLinesAllowedExceededMessage(entryHeader.MaximumEntryLinesAllowed), ValidationModeProvider);
			}
		}

		string GetMaximumEntryLinesAllowedExceededMessage(int maximumEntryLinesAllowed)
		{
			return Res.GetString("2ae039f4-c273-44d6-9ef3-da6c63640fbd", "There are more than {0} Entry Lines linked to this Entry Instruction, please split it.", maximumEntryLinesAllowed);
		}

		protected override void CheckCEI_Style()
		{
			base.CheckCEI_Style();
			Parent.CEI_StyleInfo.AddNotificationIfNotEntered(ValidationModeProvider);
		}

		protected override void CheckCEI_SubStyle()
		{
			base.CheckCEI_SubStyle();

			var targetInfo = Parent.CEI_SubStyleInfo;
			targetInfo.AddNotificationIfInvalidCode(ValidationModeProvider);

			if (Parent.CEI_SubStyle == IntelligentDeclarationTypeList.Codes.Cancel)
			{
				var entryHeader = Parent.EntryHeader;
				if (entryHeader == null || entryHeader.CH_EntryStatus.IsEmpty)
				{
					targetInfo.AddWarning(Res.GetString("bdf400c9-33eb-4327-a258-ae7f1086023c", "This is the initial submission, please check the Intelligent Declaration Type."));
				}
			}
		}

		public void ValidateBillOfLading()
		{
			ValidateCalculatedProperty(Parent.BillOfLadingInfo);
		}

		protected void CheckBillOfLading()
		{
			Parent.JobDeclaration?.TransportDataHelper.ValidateBillOfLadingOnEntryInstructions(Parent.BillOfLadingInfo);
		}

		public void ValidateBillOfLadingDate()
		{
			ValidateCalculatedProperty(Parent.BillOfLadingDateInfo);
		}

		protected void CheckBillOfLadingDate()
		{
		}

		#region InvoiceHeaders & InvoiceLines

		void ValidateInvoiceHeadersAndLines()
		{
			var invoiceLines = (Parent.IsChild ? Parent.ParentInstruction.InvoiceLines : Parent.InvoiceLines).Cast<JobComInvoiceLine>().ToArray();
			ValidateIncoTerms(invoiceLines);
			ValidateChargesCurrencies(invoiceLines);
			ValidateCertificateOfOrigin(invoiceLines);
			ValidateItemNoOnCertOfOrigin(invoiceLines);
		}

		void ValidateIncoTerms(IEnumerable<JobComInvoiceLine> invoiceLines)
		{
			if (!invoiceLines.AllSame(line => line.InvoiceHeader?.JZ_IncoTerm))
			{
				Parent.AddRowNotification(Res.GetString(
					"a431df0f-15ca-b3ac-4505-fba8b6965ad1",
					"The Invoice Headers linked to this Entry Instruction have different Incoterms."
				), ValidationModeProvider);
			}
		}

		void ValidateChargesCurrencies(IEnumerable<JobComInvoiceLine> invoiceLines)
		{
			ValidateChargesHaveSameCurrency(invoiceLines, CustomsChargeTypeList.Codes.OverseasFreight);
			ValidateChargesHaveSameCurrency(invoiceLines, CustomsChargeTypeList.Codes.OverseasInsurance);
		}

		void ValidateChargesHaveSameCurrency(IEnumerable<JobComInvoiceLine> invoiceLines, ZString chargeType)
		{
			if (!invoiceLines.GetAllChargesFromInvoiceLines(chargeType).Where(x => !x.J7_RX_NKCurrency.IsEmpty).AllSame(x => x.J7_RX_NKCurrency))
			{
				Parent.AddRowNotification(Res.GetString("E36DEDE5-C311-4A12-841D-46FC331C4A1F", "The Invoice Lines linked to this Entry Instruction have multiply Currencies for {0} charges.", chargeType), ValidationModeProvider);
			}
		}

		void ValidateCertificateOfOrigin(IEnumerable<JobComInvoiceLine> invoiceLines)
		{
			if (!invoiceLines
				.Where(line => line.IsCertificateOfOriginApplicable && !line.CertificateOfOrigin.IsEmpty)
				.AllSame(line => new { line.CertificateOfOrigin, line.TradeAgreementCode, line.CertificateOfOriginType })
			)
			{
				Parent.AddRowNotification(Res.GetString(
					"4BB03185-57D1-4F0B-BBD7-438B11FF9A5F",
					@"The Invoice Lines linked to this Entry Instruction have multiply Certificate of Origin or Preferential Code."
				), ValidationModeProvider);
			}
		}

		void ValidateItemNoOnCertOfOrigin(IEnumerable<JobComInvoiceLine> invoiceLines)
		{
			if (!invoiceLines
				.Where(line => line.IsCertificateOfOriginApplicable && !line.ItemNoOnCertOfOrigin.IsEmpty && !line.CertificateOfOrigin.IsEmpty && !line.JI_CL.IsEmpty)
				.GroupBy(line => new { line.CertificateOfOrigin, line.ItemNoOnCertOfOrigin })
				.All(group => group.AllSame(line => line.JI_CL))
			)
			{
				Parent.AddRowNotification(Res.GetString(
					"C87FF24E-9E30-4B20-AACA-73CAD9176596",
					@"The Invoice Lines which have the same Certificate Of Origin Item No. should be merged to one Entry Line.
Please select appropriate Merge Options on Misc. tab or amend the Item No., then merge again by clicking
Brokerage > Generate Entries (Merge) to refresh this validation."
				), ValidationModeProvider);
			}
		}

		#endregion

		public void ValidateSpecialBusinessIdentifiersAsString()
		{
			ValidateCalculatedProperty(Parent.SpecialBusinessIdentifiersAsStringInfo);
		}

		protected void CheckSpecialBusinessIdentifiersAsString()
		{
			Parent.SpecialBusinessIdentifiers.ValidateSelectedOptionAsString();
		}

		public void ValidateOperationMattersAsString()
		{
			ValidateCalculatedProperty(Parent.OperationMattersAsStringInfo);
		}

		protected void CheckOperationMattersAsString()
		{
			Parent.OperationMatters.ValidateSelectedOptionAsString();
		}

		#region AddInfo properties

		protected override void CheckCEI_LevyType()
		{
			base.CheckCEI_LevyType();
			Parent.CEI_LevyTypeInfo.AddNotificationIfInvalidCode(ValidationModeProvider);

			var declaration = Parent.JobDeclaration;
			if (declaration != null)
			{
				if (declaration.WillGenerateBothEntries && Parent.WillGenerateRecordListing)
				{
					Parent.CEI_LevyTypeInfo.AddNotificationIfIsEntered(ValidationModeProvider);
				}
				else
				{
					var procedure = Parent.CEI_Style;
					if (!procedure.IsEmpty && !LevyTypeList.GetProcedureAllowEmptyLevyTypes(procedure, declaration.IsImport))
					{
						Parent.CEI_LevyTypeInfo.AddNotificationIfNotEntered(ValidationModeProvider);
					}

					var levyType = Parent.CEI_LevyType;
					if (!levyType.IsEmpty)
					{
						if (!procedure.IsEmpty)
						{
							var supportedLevyTypes = LevyTypeList.GetSupportedLevyTypesByProcedureCode(Parent.Factory, procedure, declaration.IsImport);
							if (supportedLevyTypes.Any() && !supportedLevyTypes.Contains(levyType.ToString()))
							{
								var message = string.Join(", ", supportedLevyTypes);
								if (LevyTypeList.GetProcedureAllowEmptyLevyTypes(procedure, declaration.IsImport))
								{
									message += Res.GetString("1357EE7E-D986-4919-87E0-1FEB68E7DD7F", " or blank");
								}

								Parent.CEI_LevyTypeInfo.AddWarning(Res.GetString("6D4450FF-F8A6-44D4-A9EB-C6C843735865", "The selected levy type might not suitable for the selected procedure code.\r\nThe suitable levy types: {0}.", message));
							}

							var procedureCodeOnlyAllowBlank = LevyTypeList.GetProcedureCodeOnlyAllowEmptyLevyType(declaration.IsImport);
							if (procedureCodeOnlyAllowBlank.Contains<string>(procedure))
							{
								Parent.CEI_LevyTypeInfo.AddNotification(Res.GetString("D10F2649-F637-4A02-8ACD-A43819F3CB0D", "Levy Type should be blank when Procedure Code is {0}.", procedure), ValidationModeProvider);
							}
						}

						CheckLevyTypeAgainstTransportCodes(levyType);
						CheckLevyTypeAgainstDistrictCode(levyType);
					}
				}
			}
		}

		void CheckLevyTypeAgainstTransportCodes(ZString levyType)
		{
			var transportCode = Parent.JobDeclaration?.JE_CNTransportMode ?? ZString.Empty;
			if (levyType == LevyTypeList.Codes._506 && transportCode == CNTransportModeList.Codes.Air)
			{
				Parent.CEI_LevyTypeInfo.AddNotification(Res.GetString("4B8F69C6-A06F-45D6-B2C1-40DB4C6A37CF", "The selected Levy Type is not suitable for Transport Mode Air"), ValidationModeProvider);
			}
		}

		void CheckLevyTypeAgainstDistrictCode(ZString levyType)
		{
			var declaration = Parent.JobDeclaration;
			if (declaration != null && (levyType == LevyTypeList.Codes._601 || levyType == LevyTypeList.Codes._602 || levyType == LevyTypeList.Codes._603 || levyType == LevyTypeList.Codes._799))
			{
				var targetInfo = Parent.CEI_LevyTypeInfo;
				var isImport = declaration.IsImport;
				var organization = isImport ? declaration.Importer : declaration.Supplier;
				var ccdCode = organization.GetChinaCustomsRegNo(OrgCusCode.CodeTypes.CustomsClientCode);
				if (!ccdCode.IsEmpty)
				{
					switch (levyType)
					{
						case LevyTypeList.Codes._601:
							if (!Regex.IsMatch(ccdCode, @"^.{5}3"))
							{
								targetInfo.AddNotification(GetCCDPatternNotMatchMessage(levyType, "3", isImport), ValidationModeProvider);
							}
							break;
						case LevyTypeList.Codes._602:
							if (!Regex.IsMatch(ccdCode, @"^.{5}2"))
							{
								targetInfo.AddNotification(GetCCDPatternNotMatchMessage(levyType, "2", isImport), ValidationModeProvider);
							}
							break;
						case LevyTypeList.Codes._603:
							if (!Regex.IsMatch(ccdCode, @"^.{5}4"))
							{
								targetInfo.AddNotification(GetCCDPatternNotMatchMessage(levyType, "4", isImport), ValidationModeProvider);
							}
							break;
						case LevyTypeList.Codes._799:
							if (!Regex.IsMatch(ccdCode, @"^.{5}[234]"))
							{
								targetInfo.AddNotification(GetCCDPatternNotMatchMessage(levyType, "2,3,4", isImport), ValidationModeProvider);
							}
							break;
					}
				}
			}
		}

		static string GetCCDPatternNotMatchMessage(ZString levyType, ZString shouldbe, bool isImport)
		{
			return isImport ? Res.GetString("63BF456C-4D34-4A97-9A8D-000B7F4E4A48", "When Levy Type is {0}, the sixth digit of the CCD number of Importer should be {1}.", levyType, shouldbe)
											: Res.GetString("20308FEB-C643-493C-85D9-BBBC3F64B326", "When Levy Type is {0}, the sixth digit of the CCD number of Supplier should be {1}.", levyType, shouldbe);
		}

		protected override void CheckCEI_ManualNo()
		{
			base.CheckCEI_ManualNo();
			var value = Parent.CEI_ManualNo;
			var targetInfo = Parent.CEI_ManualNoInfo;

			if (!value.IsEmpty && !Regex.IsMatch(value, ManualNumberPattern))
			{
				targetInfo.AddNotification(ManualNumberFormatErrorMessage, ValidationModeProvider);
			}

			var levyType = Parent.CEI_LevyType;
			if (!levyType.IsEmpty)
			{
				if (LevyTypeList.RequiresManualNo(levyType) && Parent.CEI_Style != CNRefCusProcedure.Codes._0815)
				{
					targetInfo.AddNotificationIfNotEntered(ValidationModeProvider);
				}

				var supportedTypes = LevyTypeList.GetSupportedManualTypesByLevyType(levyType);
				if (supportedTypes.Any() && !value.IsEmpty && !supportedTypes.Contains(value[0].ToString().ToUpper(CultureInfo.InvariantCulture)))
				{
					targetInfo.AddNotification(Res.GetString("F68E195C-F794-4380-A66C-1D5891DB561D", "When Levy Type is {0}, Manual Number should start with {1} .", levyType, string.Join(",", supportedTypes)), ValidationModeProvider);
				}
			}
		}

		protected override void CheckCEI_RelatedManualNo()
		{
			base.CheckCEI_RelatedManualNo();
			var value = Parent.CEI_RelatedManualNo;
			var targetInfo = Parent.CEI_RelatedManualNoInfo;
			if (!value.IsEmpty && !Regex.IsMatch(value, ManualNumberPattern))
			{
				targetInfo.AddNotification(ManualNumberFormatErrorMessage, ValidationModeProvider);
			}
		}

		protected override void CheckCEI_RelatedMRN()
		{
			base.CheckCEI_RelatedMRN();
			var value = Parent.CEI_RelatedMRN;
			var targetInfo = Parent.CEI_RelatedMRNInfo;
			if (!value.IsEmpty && !Regex.IsMatch(value, EntryNumberPattern))
			{
				targetInfo.AddNotification(EntryNumberFormatErrorMessage, ValidationModeProvider);
			}

			if (RequiresRelatedEntryNumber(Parent))
			{
				Parent.CEI_RelatedMRNInfo.AddNotificationIfNotEntered(ValidationModeProvider);
			}
		}

		bool RequiresRelatedEntryNumber(CusEntryInstruction parent)
		{
			return
				LevyTypeList.RequiresRelatedEntryNumber(parent.CEI_Style, parent.WillGenerateEnteringEntry)
				|| parent.JobDeclaration != null && parent.JobDeclaration.WillGenerateBothEntries && parent.WillGenerateExitingEntry;
		}

		protected override void CheckCEI_CIQRelatedReason()
		{
			base.CheckCEI_CIQRelatedReason();
			Parent.CEI_CIQRelatedReasonInfo.AddNotificationIfInvalidCode(ValidationModeProvider);
			if (!Parent.CEI_CIQRelatedNum.IsEmpty)
			{
				Parent.CEI_CIQRelatedReasonInfo.AddNotificationIfNotEntered(ValidationModeProvider);
			}
		}

		protected override void CheckCEI_Packages()
		{
			base.CheckCEI_Packages();
			Parent.CEI_PackagesInfo.AddNotificationIfIsNegative(ValidationModeProvider);
			Parent.CEI_PackagesInfo.AddNotificationIfIsZero(ValidationModeProvider);

			if (Parent.CEI_Packages > 1 && (Parent.CEI_PackageUQ == PackageType.Codes.Bulk || Parent.CEI_PackageUQ == PackageType.Codes.Naked))
			{
				Parent.CEI_PackagesInfo.AddNotification(PackTypeMessageError, ValidationModeProvider);
			}

			var relatedInstruction = Parent.ParentInstruction ?? Parent.ChildInstruction;
			if (relatedInstruction != null && Parent.CEI_Packages != relatedInstruction.CEI_Packages)
			{
				Parent.CEI_PackagesInfo.AddNotification(GetPropertyValueShouldEqualMessage(Parent.CEI_PackagesInfo), ValidationModeProvider);
			}
		}

		protected override void CheckCEI_PackageUQ()
		{
			base.CheckCEI_PackageUQ();
			Parent.CEI_PackageUQInfo.AddNotificationIfInvalidCodeOrEmpty(ValidationModeProvider);

			var relatedInstruction = Parent.ParentInstruction ?? Parent.ChildInstruction;
			if (relatedInstruction != null && Parent.CEI_PackageUQ != relatedInstruction.CEI_PackageUQ)
			{
				Parent.CEI_PackageUQInfo.AddNotification(GetPropertyValueShouldEqualMessage(Parent.CEI_PackageUQInfo), ValidationModeProvider);
			}

			ValidateCEI_Packages();
		}

		ZString GetPropertyValueShouldEqualMessage(ZPropertyInfo propertyInfo)
		{
			return Res.GetString("75B5E291-2A42-4C8E-B0C7-A97785F198C4", "{0} should equal to the value on its related Instruction.", propertyInfo.HumanReadableName);
		}

		#region Validation Pattern

		public static string ManualNumberPattern => (NoResString)@"^[A-Z]\d{4}[A-Z0-9]{3}\d{4}$";

		string EntryNumberPattern => (NoResString)"^\\d{18}$";

		#endregion

		#region Validation Message

		public static ZString ManualNumberFormatErrorMessage => Res.GetString("4bc90b6c-e189-474e-96df-3e0a2fd52a5d", "Customs Manual Number should have 12 characters and in the format: ANNNNXXXNNNN, where A is a letter, X is alphanumeric and N is a number.");

		static ZString EntryNumberFormatErrorMessage => Res.GetString("3e1220ce-1fab-4e48-b211-492edf2488df", "Customs Entry Number should be 18 digit (0-9) characters.");

		internal static ZString PackTypeMessageError => Res.GetString("b4abb08d-08fa-4246-9f79-b340e4eab0ab", "No of Packages cannot be greater than 1 for Pack Type 00 or 01.");

		internal static string GoodsRequireCIQMessage => Res.GetString("8D285F0C-84A1-4010-B5C4-99992F8FCB62", "This Entry Instruction requires inspection and quarantine as,");

		internal static string DomesticNotRequireCIQMessage => Res.GetString("F59E9E92-29F3-46C8-9E30-C55AE112C158", "Record Listing for domestic transportation do not require inspection and quarantine.");

		#endregion

		protected override void CheckCEI_CEI_Parent()
		{
			base.CheckCEI_CEI_Parent();

			var parentPK = Parent.CEI_CEI_Parent;
			var targetInfo = Parent.CEI_CEI_ParentInfo;
			var declaration = Parent.JobDeclaration;
			if (declaration != null && declaration.WillGenerateBothEntries)
			{
				if (!parentPK.IsEmpty)
				{
					if (Parent.ParentInstruction == null)
					{
						targetInfo.AddError(Res.GetString("1c846210-3e18-4661-8645-a9c4ece65604", "Please select a valid Parent Instruction."));
					}
					if (declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().Any(x => x != Parent && x.CEI_CEI_Parent == parentPK))
					{
						targetInfo.AddError(Res.GetString("5f746160-766f-4137-87a6-3b7fc9a66375", "There are two Entry Instructions have the same parent."));
					}
				}
				if (!Parent.IsParent && !Parent.IsChild)
				{
					targetInfo.AddNotification(Res.GetString("a56ee6f9-fa46-4bf6-92a3-8a99bb4abe5a", "This Entry Instruction is isolated."), ValidationModeProvider);
				}
			}
		}

		protected override void CheckCEI_DocumentSubmissionType()
		{
			base.CheckCEI_DocumentSubmissionType();

			Parent.CEI_DocumentSubmissionTypeInfo.AddNotificationIfInvalidCodeOrEmpty(ValidationModeProvider);

			if (Parent.JobDeclaration?.IsTwoStepDeclaration ?? false)
			{
				if (Parent.CEI_DocumentSubmissionType != EntryDocumentSubmissionTypes.Codes.PaperlessForCustoms)
				{
					Parent.CEI_DocumentSubmissionTypeInfo.AddNotification(Res.GetString("33F10AEE-4451-4430-8F5C-8ECE2D6B83B5", "Doc. Submission should be {0} – {1} for two-step declaration clearance mode.", EntryDocumentSubmissionTypes.Codes.PaperlessForCustoms, EntryDocumentSubmissionTypes.Descriptions.PaperlessForCustoms), ValidationModeProvider);
				}
			}
		}

		protected override void CheckCEI_CIQRequires()
		{
			base.CheckCEI_CIQRequires();

			var declaration = Parent.JobDeclaration;
			if (declaration != null)
			{
				if (!declaration.TransportDataHelper.IsCrossBorder && Parent.WillGenerateRecordListing)
				{
					if (Parent.CEI_CIQRequires)
					{
						Parent.CEI_CIQRequiresInfo.AddNotification(DomesticNotRequireCIQMessage, ValidationModeProvider);
					}
				}
				else
				{
					if (!Parent.CEI_CIQRequires && !Parent.CEI_CIQRequiresReadonly)
					{
						var validationResult = EntryRequiresCIQStrategy.ValidateEntry(Parent);
						if (validationResult.HasErrors)
						{
							var message = validationResult.GetFormattedMessage();
							if (!declaration.TransportDataHelper.IsCrossBorder && Parent.WillGenerateCustomsEntry && Parent.WillGenerateExitingEntry)
							{
								Parent.CEI_CIQRequiresInfo.AddWarning(message);
							}
							else
							{
								Parent.CEI_CIQRequiresInfo.AddNotification(message, ValidationModeProvider);
							}
						}
					}
				}
			}
		}

		#endregion
	}
}
