using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;
using RefCusProcedure = Enterprise.Core.Constants.Customs.Universal.RefCusProcedure.Codes;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusEntryInstructionValidation : Customs.Business.CusEntryInstructionValidation
	{
		public CusEntryInstructionValidation(CusEntryInstruction parent) : base(parent)
		{
		}

		protected new CusEntryInstruction Parent
		{
			get { return (CusEntryInstruction)base.Parent; }
		}

		public IEntryInstructionValidationDecider ValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, GetValidationDecider);
		CachedValue<IEntryInstructionValidationDecider> validationDeciderCached;
		protected virtual IEntryInstructionValidationDecider GetValidationDecider() => Parent.JobDeclaration is JobDeclaration declaration ? declaration.Configuration.InstructionConfiguration.GetValidationDecider(Parent) : null;

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidateNotAllowDeleteEntryLines();
			ValidateInvoiceLinesCountryOfSupply();
			ValidateFiscalReferences();
			ValidateGuaranteeTypesCount();
			ValidateAllInvoicesUseSame();
			ValidateGoodsLocationDescription();
			ValidateIdentificationofGoodsDetails();
			ValidateMaximumEntryLineNumber();
		}

		protected override void CheckCEI_SubStyle()
		{
			base.CheckCEI_SubStyle();
			CheckCEI_SubStyleListValidation();
			CheckRuleC0614();
			CheckRuleR0028E();
		}

		protected virtual void CheckCEI_SubStyleListValidation()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CEI_SubStyleInfo);
		}

		void CheckRuleC0614()
		{
			if (ValidationDecider is IRuleC0614ForCEI_SubStyleDecider decider && decider.IsActive)
			{
				var entryInstruction = Parent;
				switch (entryInstruction.CEI_SubStyle.ToUpperInvariant())
				{
					case EntrySubStyleList.Codes.NormalDeclaration:
					case EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA:
					case EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF:
						if (!entryInstruction.HasHeaderLevelPreviousDocuments())
						{
							entryInstruction.CEI_SubStyleInfo.AddMessageError(Res.GetString("76DF0F04-BB37-4E07-A13A-55D337F1C683",
								"[C0614] Previous Documents on Entry Instruction or Invoice Header required when Sub Style is 'A' or 'D' or 'Y'."));
						}
						break;
				}
			}
		}

		void CheckRuleR0028E()
		{
			if (ValidationDecider is IRuleR0028EForCEI_SubStyleDecider decider && decider.IsActive)
			{
				var entryInstruction = Parent;
				var subStyle = entryInstruction.CEI_SubStyle.ToUpperInvariant();
				var needVerifyAuthorizationUsages = entryInstruction.CusAuthorizationUsages.Where(n =>
				{
					var customsCode = n.CustomsCode;
					return customsCode == EUCommonConstants.CusAuthorizationUsageType.C512
							|| customsCode == EUCommonConstants.CusAuthorizationUsageType.C513
							|| customsCode == EUCommonConstants.CusAuthorizationUsageType.C514;
				}).Distinct().ToArray();
				foreach (var cusAuthorizationUsage in needVerifyAuthorizationUsages)
				{
					var customsCode = cusAuthorizationUsage.CustomsCode;
					switch (customsCode)
					{
						case EUCommonConstants.CusAuthorizationUsageType.C512
						when !AllowSubStyleForC512(subStyle):
							entryInstruction.CEI_SubStyleInfo.AddMessageError(Res.GetString("7ae2762d-6613-4516-a4cb-49dc3159b5d2",
								"[R0028E] Only declaration Sub Style C or F or Y is allowed for an authorization code C512."));
							break;
						case EUCommonConstants.CusAuthorizationUsageType.C513
						when !AllowSubStyleForC513(subStyle):
							entryInstruction.CEI_SubStyleInfo.AddMessageError(Res.GetString("89bde423-af95-4350-a287-b196424bb10e",
								"[R0028E] Only declaration Sub Style A or B or C or D or E or F or X or Y or Z is allowed for an authorization code C513."));
							break;
						case EUCommonConstants.CusAuthorizationUsageType.C514
						when !AllowSubStyleForC514(subStyle):
							entryInstruction.CEI_SubStyleInfo.AddMessageError(Res.GetString("104fa65d-9959-4664-aec9-1d0f76858be7",
								"[R0028E] Only declaration Sub Style A or C or D or F or Y or Z is allowed for an authorization code C514."));
							break;
					}
				}
			}

			bool AllowSubStyleForC512(ZString subStyle) =>
				subStyle == EntrySubStyleList.Codes.SimplifiedDeclaration ||
				subStyle == EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC ||
				subStyle == EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF;

			bool AllowSubStyleForC513(ZString subStyle) =>
				subStyle == EntrySubStyleList.Codes.NormalDeclaration ||
				subStyle == EntrySubStyleList.Codes.IncompleteDeclaration ||
				subStyle == EntrySubStyleList.Codes.SimplifiedDeclaration ||
				subStyle == EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA ||
				subStyle == EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB ||
				subStyle == EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC ||
				subStyle == EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE ||
				subStyle == EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF ||
				subStyle == EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic;

			bool AllowSubStyleForC514(ZString subStyle) =>
				subStyle == EntrySubStyleList.Codes.NormalDeclaration ||
				subStyle == EntrySubStyleList.Codes.SimplifiedDeclaration ||
				subStyle == EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA ||
				subStyle == EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC ||
				subStyle == EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF ||
				subStyle == EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic;
		}

		protected override void ValidateWarehouseAddressIsInValidCountry(ZPropertyInfo wareHouseInfo, OrgAddress address, BaseJobDeclaration declaration, Customs.Business.CusEntryHeader entryHeader)
		{
			var factory = Parent.Factory;
			var isValidPortCode = address.RelatedPortCode is RefUNLOCO;
			if (!(isValidPortCode) || !Customs.Business.Extensions.BusinessObjectExtensions.IsMemberOfEU(factory, address.RelatedPortCode.RL_RN_NKCountryCode))
			{
				if (!(isValidPortCode) || (declaration.Country.Code != Core.Constants.CountryCodes.UnitedKingdom && !address.RelatedPortCode.IsInNorthernIreland))
				{
					wareHouseInfo.AddMessageError(CusEntryHeader.BondedWarehouseAddressShouldBeInsideDeclarationRegionEU(declaration.Importer?.OH_Code ?? ZString.Empty, entryHeader.EntryHeaderDescriptiveMenuItemText, declaration.Country?.RN_DescMultilingual ?? ZString.Empty));
				}
			}
		}

		public void ValidateNotAllowDeleteEntryLines()
		{
			if (Parent.ShouldKeepNotAllowDeleteEntryLinesErrors)
			{
				var entryHeader = Parent.EntryHeader as CusEntryHeader;
				if (entryHeader != null)
				{
					Parent.AddRowError(NotAllowDeleteEntryLineErrorMessage(entryHeader));
				}
			}
		}

		public void AddNotAllowDeleteEntryLinesError()
		{
			var entryHeader = (CusEntryHeader)Parent.EntryHeader;
			if (entryHeader != null)
			{
				Parent.ClearRowNotificationsContaining(NotAllowDeleteEntryLineErrorMessage(entryHeader));
				if (entryHeader.LockNumberOfEntryLines && entryHeader.MergedLines.Count < entryHeader.CH_HighestLineNumber)
				{
					Parent.ShouldKeepNotAllowDeleteEntryLinesErrors = true;
					Parent.AddRowError(NotAllowDeleteEntryLineErrorMessage(entryHeader));
				}
			}
		}

		protected virtual ZString NotAllowDeleteEntryLineErrorMessage(CusEntryHeader entryHeader) => Res.GetString("49C64C53-054B-4253-8887-4CA0399D6B60", "Entry Declared ({0}) : Cannot delete Entry lines from a Declared or Canceled Entry. Please, cancel and reopen the declaration. Data will revert to the last save.", entryHeader.CH_BGMReference);

		public void ValidateInvoiceLinesCountryOfSupply()
		{
			var declaration = Parent.JobDeclaration;
			if (declaration != null && declaration.IsImport && declaration.Configuration.InvoiceLineConfiguration.CountryOfSupplyMustBeTheSameForAllLinesOnInstruction(declaration) && !Parent.IsCountryOfSupplySameForAllInvoiceLines)
			{
				Parent.AddRowMessageError(InvoiceCountryOfSupplyMustBeTheSameErrorMessage);
			}
			else
			{
				Parent.RemoveRowMessageError(InvoiceCountryOfSupplyMustBeTheSameErrorMessage);
			}
		}

		ZString InvoiceCountryOfSupplyMustBeTheSameErrorMessage => Res.GetString("CBCFC7AE-C0B7-4A9D-BA21-DC8C54462509", "All invoice lines on an Entry instruction must have the same Country of Supply");

		public void ValidateFiscalReferences()
		{
			var declaration = Parent.JobDeclaration;
			if (IsFiscalReferenceSupportedOnInstruction(declaration) && IsFiscalReferenceSupportedOnCPC42And63Only(declaration))
			{
				if (!Parent.FiscalReferences.Any())
				{
					var invoiceLinesStartingWith42Or62 = GetInvoiceLinesStartingWith42Or62().ToArray();
					if (invoiceLinesStartingWith42Or62.Any())
					{
						if (IsFiscalReferenceSupportedOnInvoiceLine(declaration))
						{
							if (invoiceLinesStartingWith42Or62.Any(x => !x.FiscalReferences.Any()))
							{
								Parent.AddRowMessageError(Res.GetString("2A3A4450-930E-4E2D-91CF-4C25B4D25E49", "If there is an invoice line with CPC starts with 42 or 63, at least one Fiscal Reference record must exist at the Instruction or Line level."));
							}
						}
						else
						{
							Parent.AddRowMessageError(Res.GetString("532A4C25-B0CC-48F8-800B-24124947C2E3", "If there is an invoice line with CPC starts with 42 or 63, at least one Fiscal Reference record must exist at the Instruction."));
						}
					}
				}
				else if (!CanFiscalRepresentationEnteredWhenNoInvoiceLinesStartwith42Or63 && !GetInvoiceLinesStartingWith42Or62().Any())
				{
					Parent.AddRowMessageError(Res.GetString("C35D8ED2-B689-4959-BBE4-82D855CD6B1A", "Fiscal Representation should only be entered when there is at least one invoice line with CPC starts with 42 or 63."));
				}
			}
		}

		IEnumerable<JobComInvoiceLine> GetInvoiceLinesStartingWith42Or62()
		{
			return Parent.InvoiceLines.Cast<JobComInvoiceLine>().Where(x =>
			{
				var procedure = x.JI_Procedure;
				return procedure.StartsWith(CustomsProcedureCodeList.ProcedureCode._42) || procedure.StartsWith(CustomsProcedureCodeList.ProcedureCode._63);
			});
		}

		void ValidateGuaranteeTypesCount() => ValidateGuaranteeTypesCountCore();

		protected virtual void ValidateGuaranteeTypesCountCore()
		{
			if (Parent.Guarantees.Cast<GuaranteeForEntryInstruction>().Select(g => g.PW_BondType).Distinct().Skip(MaxGuaranteeTypesCount).Any())
			{
				Parent.AddRowMessageError(Res.GetString("0CAF5129-2F66-432E-BA7B-3057FB3B0B1A", "You have entered more than 9 different Types."));
			}
		}

		const int MaxGuaranteeTypesCount = 9;

		static bool IsFiscalReferenceSupportedOnInvoiceLine(JobDeclaration declaration)
		{
			return declaration?.Configuration.InvoiceLineConfiguration.FiscalReferencesSupport(declaration) ?? false;
		}

		static bool IsFiscalReferenceSupportedOnInstruction(JobDeclaration declaration)
		{
			return declaration?.Configuration.InstructionConfiguration.FiscalReferencesSupport(declaration) ?? false;
		}

		static bool IsFiscalReferenceSupportedOnCPC42And63Only(JobDeclaration declaration)
		{
			return declaration?.Configuration.InstructionConfiguration.FiscalReferencesSupportOnCPC42And63Only(declaration) ?? false;
		}

		protected virtual ZBool AllRelatedInvoicesMustHaveSameTransactionNature => ZBool.True;

		protected virtual ZString AllRelatedInvoicesMustHaveSameTransactionNatureMessage => Res.GetString("ED5A53B9-D697-4F88-B857-B9E1214B3E04", "All Invoices on an Entry Instruction must have the same '[24] Tran. Nature'.");

		protected virtual ZBool AllRelatedInvoicesMustHaveSameIncoterm => ZBool.True;

		protected virtual ZString AllRelatedInvoicesMustHaveSameIncotermMessage => Res.GetString("3CB258F9-4FE3-42CA-8386-85DDE672818D", "All Invoices on an Entry Instruction must have the same '[20] Incoterm'.");

		protected virtual ZBool AllRelatedInvoicesMustHaveSameCurrency => ZBool.False;

		protected virtual ZString AllRelatedInvoicesMustHaveSameCurrencyMessage => Res.GetString("32607338-7958-4F2A-8A23-0E38244E56E9", "All Invoices on an Entry Instruction must have the same '[22] Currency'.");

		protected virtual ZBool AllRelatedInvoicesMustHaveSameAgreedPlace => ZBool.False;

		protected virtual ZString AllRelatedInvoicesMustHaveSameAgreedPlaceMessage => Res.GetString("F022BC04-DFCF-4013-8740-BB8334460CC9", "All Invoices on an Entry Instruction must have the same 'Agreed Place'.");

		protected virtual ZBool CanFiscalRepresentationEnteredWhenNoInvoiceLinesStartwith42Or63 => ZBool.False;

		void ValidateAllInvoicesUseSame()
		{
			CheckInvoicesCurrency();
			CheckInvoicesIncoTermPlace();
			CheckInvoicesIncoTerm();
			CheckInvoicesValuationCode();
		}

		void CheckInvoicesCurrency()
		{
			if (AllRelatedInvoicesMustHaveSameCurrency
				&& !Parent.Invoices.AllSame(x => x.JZ_RX_NKInvoice_Currency))
			{
				Parent.AddRowMessageError(AllRelatedInvoicesMustHaveSameCurrencyMessage);
			}
		}

		void CheckInvoicesIncoTermPlace()
		{
			if (AllRelatedInvoicesMustHaveSameAgreedPlace
				&& !Parent.Invoices.AllSame(x => x.JZ_IncoTermPlace))
			{
				Parent.AddRowMessageError(AllRelatedInvoicesMustHaveSameAgreedPlaceMessage);
			}
		}

		void CheckInvoicesIncoTerm()
		{
			if (AllRelatedInvoicesMustHaveSameIncoterm
				&& !Parent.Invoices.AllSame(x => x.JZ_IncoTerm))
			{
				Parent.AddRowMessageError(AllRelatedInvoicesMustHaveSameIncotermMessage);
			}
		}

		void CheckInvoicesValuationCode()
		{
			if (AllRelatedInvoicesMustHaveSameTransactionNature
				&& !Parent.Invoices.AllSame(x => x.JZ_ValuationCode))
			{
				Parent.AddRowMessageError(AllRelatedInvoicesMustHaveSameTransactionNatureMessage);
			}
		}

		public void ValidateIdentificationofGoodsDetails()
		{
			ValidateCalculatedProperty(Parent.IdentificationofGoodsDetailsInfo);
		}

		protected virtual void CheckIdentificationofGoodsDetails()
		{
		}

		public void ValidateGoodsLocationDescription()
		{
			ValidateCalculatedProperty(Parent.GoodsLocationDescriptionInfo);
		}

		protected virtual void CheckGoodsLocationDescription()
		{
			if (ValidationDecider is IEntryInstructionValidationDecider validationDecider)
			{
				CheckRuleC0619(validationDecider);
				CheckRuleC0628(validationDecider);
			}

			var parent = Parent;
			if (parent.HasLoadedGoodsLocation && parent.IsInnerGoodsLocationActive)
			{
				CusGoodsLocationValidationHelper.ValidateInnerGoodsLocation(parent);
			}
		}

		public void ValidateMaximumEntryLineNumber()
		{
			var parent = Parent;

			if (ValidationDecider is IEntryInstructionValidationDecider entryInstructionValidationDecider
				&& entryInstructionValidationDecider.IsMaximumEntryLinesAllowedRuleActive
				&& parent.EntryHeader is CusEntryHeader header
				&& header.MergedLinesCount > MaximumEntryLinesAllowed)
			{
				var messageError = Res.GetString(
					"F5D4FEF4-C071-4C18-8F0C-C1FB97B64A0B",
					"The number of entry lines in this entry instruction is greater than the {0} allowed in the message\r\nYou can create an additional Entry instruction to move invoices and/or invoice lines to it.",
					MaximumEntryLinesAllowed);

				parent.AddRowMessageError(messageError);
			}
		}
		void CheckRuleC0619(IEntryInstructionValidationDecider validationDecider)
		{
			if (validationDecider.IsRuleC0619ActiveForGoodsLocationDescription)
			{
				var entryInstruction = Parent;
				if (!entryInstruction.GoodsLocationDescription.IsEmpty && entryInstruction.CEI_Procedure == Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration)
				{
					entryInstruction.GoodsLocationDescriptionInfo.AddMessageError(Res.GetString("66091AB3-A04B-46BA-A3AC-8BA9020B4234", "[C0619] Location Of Goods must be empty if CPC=71."));
				}
			}
		}

		void CheckRuleC0628(IEntryInstructionValidationDecider validationDecider)
		{
			if (validationDecider.IsRuleC0628ActiveForGoodsLocationDescription)
			{
				var entryInstruction = Parent;
				if (entryInstruction.GoodsLocationDescription.IsEmpty && entryInstruction.CEI_SubStyle != EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA && entryInstruction.CEI_SubStyle != EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC)
				{
					entryInstruction.GoodsLocationDescriptionInfo.AddMessageError(Res.GetString("0E8E8DC6-4FAE-4FDE-9243-8AD2CC41341A", "[C0628] Location Of Goods is mandatory for this declaration sub style."));
				}
			}
		}

		protected override void CheckCEI_Procedure()
		{
			var info = Parent.CEI_ProcedureInfo;
			if (Parent.JobDeclaration is JobDeclaration declaration && declaration.IsRequestedProcedureEnable)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(info);
			}
			ValidateGoodsLocationDescription();
			CheckRuleG0128();
		}

		protected override void CheckCEI_OA_Warehouse2()
		{
			base.CheckCEI_OA_Warehouse2();
			var entryInstruction = Parent;
			if (entryInstruction.CEI_OA_Warehouse2.IsEmpty && ValidationDecider is IEntryInstructionValidationDecider validationDecider)
			{
				CheckRuleC0626(validationDecider, entryInstruction);
				CheckRuleC0829(validationDecider, entryInstruction);
			}
		}

		void CheckRuleC0626(IEntryInstructionValidationDecider validationDecider, CusEntryInstruction entryInstruction)
		{
			if (validationDecider.IsRuleC0626ActiveForCEI_OA_Warehouse2 && entryInstruction.CEI_Procedure == Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration && HasSubStyleForRuleC0626(entryInstruction.CEI_SubStyle))
			{
				entryInstruction.CEI_OA_Warehouse2Info.AddMessageError(Res.GetString("A6979469-377F-4AE8-BD1C-163D32A04D0C", "[C0626] To Warehouse is mandatory in case Sub Style equals A or D and CPC = 71."));
			}
		}

		bool HasSubStyleForRuleC0626(ZString subStyle)
		{
			switch (subStyle.ToUpperInvariant())
			{
				case EntrySubStyleList.Codes.NormalDeclaration:
				case EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA:
					return true;
				default:
					return false;
			}
		}

		void CheckRuleC0829(IEntryInstructionValidationDecider validationDecider, CusEntryInstruction entryInstruction)
		{
			if (validationDecider.IsRuleC0829ActiveForCEI_OA_Warehouse2 && HasCPCForRuleC0829(entryInstruction.CEI_Procedure))
			{
				entryInstruction.CEI_OA_Warehouse2Info.AddMessageError(Res.GetString("2197EF66-48A1-4DFE-BDC1-62862DC60F7A", "[C0829] To Warehouse is mandatory in case Requested Procedure =  07 / 45 / 68 / 95 / 96."));
			}
		}

		bool HasCPCForRuleC0829(ZString procedure)
		{
			switch (procedure)
			{
				case Core.Constants.Customs.Universal.RefCusProcedure.Codes._07:
				case Core.Constants.Customs.Universal.RefCusProcedure.Codes._45:
				case Core.Constants.Customs.Universal.RefCusProcedure.Codes._68:
				case Core.Constants.Customs.Universal.RefCusProcedure.Codes._95:
				case Core.Constants.Customs.Universal.RefCusProcedure.Codes._96:
					return true;
				default:
					return false;
			}
		}

		protected override void CheckCEI_OA_Warehouse()
		{
			base.CheckCEI_OA_Warehouse();
			CheckRuleC0853();
		}

		void CheckRuleC0853()
		{
			if (ValidationDecider?.IsRuleC0853ActiveForCEI_OA_Warehouse ?? false)
			{
				var entryInstruction = Parent;
				var procedure = entryInstruction.CEI_Procedure;
				if (HasCPCForRuleC0853(procedure) && entryInstruction.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.JI_Calc_PreviousProcedure == Core.Constants.Customs.Universal.RefCusProcedure.Codes.StorageDeclaration))
				{
					if (entryInstruction.CEI_OA_Warehouse.IsEmpty)
					{
						entryInstruction.CEI_OA_WarehouseInfo.AddMessageError(Res.GetString("A6C5CB66-DF37-4106-9C92-2D65F50AB597", "[C0853] From Warehouse is mandatory in case Requested Procedure = {0} and Previous Procedure = 71.", procedure));
					}
				}
				else
				{
					if (!entryInstruction.CEI_OA_Warehouse.IsEmpty)
					{
						entryInstruction.CEI_OA_WarehouseInfo.AddMessageError(Res.GetString("CB22B7A9-AD8B-4724-8717-4BD17E01B452", "[C0853] From Warehouse must be empty for that Requested Procedure and Previous Procedure combination."));
					}
				}
			}
		}

		void CheckRuleG0128()
		{
			var entryInstruction = Parent;
			var procedure = entryInstruction.CEI_Procedure;
			if ((ValidationDecider as IRuleG0128ForCEI_ProcedureDecider)?.IsActive != true || entryInstruction.JobDeclaration is not JobDeclaration declaration || procedure.IsEmpty)
			{
				return;
			}

			switch (declaration.JE_EntryStyle)
			{
				case RefCusCodeListEntryStyle.Export:
					if (procedure != RefCusProcedure._10 &&
						procedure != RefCusProcedure._11 &&
						procedure != RefCusProcedure._21 &&
						procedure != RefCusProcedure._22 &&
						procedure != RefCusProcedure._23 &&
						procedure != RefCusProcedure._31)
					{
						entryInstruction.CEI_ProcedureInfo.AddMessageError(Res.GetString("1e4b7557-b95c-445c-a31d-7245325df87f",
							"[G0128] Only requested procedure ‘10’ or ‘11’ or ‘21’ or ‘22’ or ‘23’ or ‘31’ is allowed for Declaration type ‘EX’."));
					}
					break;

				case RefCusCodeListEntryStyle.ImportOfGoodsFromSpecialTerritoryOfTheCommunity:
					if (procedure != RefCusProcedure._10 &&
						procedure != RefCusProcedure._76 &&
						procedure != RefCusProcedure._77)
					{
						entryInstruction.CEI_ProcedureInfo.AddMessageError(Res.GetString("e1b920c8-2544-471c-bc41-8244e4c11319",
							"[G0128] Only requested procedure '10' or '76' or '77' is allowed for Declaration type 'CO'."));
					}
					break;
			}
		}

		bool HasCPCForRuleC0853(ZString procedure)
		{
			switch (procedure)
			{
				case RefCusProcedure._01:
				case RefCusProcedure._07:
				case RefCusProcedure._40:
				case RefCusProcedure._42:
				case RefCusProcedure._43:
				case RefCusProcedure._44:
				case RefCusProcedure._45:
				case RefCusProcedure._46:
				case RefCusProcedure._48:
				case RefCusProcedure._51:
				case RefCusProcedure._53:
				case RefCusProcedure._61:
				case RefCusProcedure._63:
				case RefCusProcedure._68:
					return true;
				default:
					return false;
			}
		}

		protected override void CheckCEI_Style()
		{
			base.CheckCEI_Style();
			CheckLegalForIND();
		}

		void CheckLegalForIND()
		{
			var parent = Parent;
			if (parent.JobDeclaration is JobDeclaration declaration && declaration.ShouldCheckLegalByDeclarantType)
			{
				if (declaration.JE_DeclarantType == RepresentationTypeList.Codes._3Indirect)
				{
					if (parent.HasIntoInwardProcessingProcedure
						 || parent.HasIntoOutwardProcessingProcedure
						 || parent.HasIntoTemporaryImportProcedure)
					{
						parent.CEI_StyleInfo.AddMessageError(Res.GetString("069C6F1B-B3DA-451F-B54B-26AFF67887CB", "It is legally forbidden to use Indirect representation for this procedure, you should consider not using Special Procedure or changing Representation Type on Misc Tab."));
					}
				}
			}
		}

		protected virtual int MaximumEntryLinesAllowed => DefaultMaximumEntryLinesAllowed;

		const int DefaultMaximumEntryLinesAllowed = 999;
	}
}
