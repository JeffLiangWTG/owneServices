using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.FR.Business.MessagesWrappers;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.FR.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class CusEntryInstructionValidation : EU.Business.Declaration.CusEntryInstructionValidation
	{
		public CusEntryInstructionValidation(CusEntryInstruction parent) : base(parent)
		{
		}

		readonly List<string> declarationTypesNeedValidation = new List<string>()
		{
			DeltaGExportDeclarationTypeList.Codes.ExportationForOutwardProcessing,
			DeltaGExportDeclarationTypeList.Codes.TemporaryExportOtherThanUnderCode21,
			DeltaGExportDeclarationTypeList.Codes.TemporaryExportWithEI,
			DeltaGExportDeclarationTypeList.Codes.PlacingGoodsUnderBW,
			DeltaGExportDeclarationTypeList.Codes.ManufacturingOfGoodsUnderSupervision,
			DeltaGImportDeclarationTypeList.Codes.ImportationForInwardProcessing,
			DeltaGImportDeclarationTypeList.Codes.EntryForEndUse,
			DeltaGImportDeclarationTypeList.Codes.EntryForEndUseWithEI,
			DeltaGImportDeclarationTypeList.Codes.TemporaryImportation,
			DeltaGImportDeclarationTypeList.Codes.EntryOfGoodsForAFreeZone,
			DeltaGImportDeclarationTypeList.Codes.EntryOfGoodsForAFreeZoneWithEI
		};

		protected new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

		public static string AssessmentDateInThePastMessage => Res.GetString("D02CF695-49F8-4CB2-890E-1C713D7303F0", "[NAT_004BIS] Assessment Date must be in future.");
		public static string AssessmentDateInTheFutureMessage => Res.GetString("AC5D7D31-7B47-4446-8A85-94B24D0E6CA0", "[NAT_004BIS] Assessment Date must be not later than 30 days from declaration date.");

		protected override void CheckCEI_DateForDutyIsValidZDateTimeRange()
		{
			base.CheckCEI_DateForDutyIsValidZDateTimeRange();
			CheckRuleNAT_004Bis();
		}

		void CheckRuleNAT_004Bis()
		{
			var parent = Parent;
			if (ValidationDecider is IEntryInstructionValidationDecider validationDecider
				&& validationDecider.IsRuleNAT_004BisActive
				&& (parent.CEI_SubStyle == EntrySubstyleCodePairList.Codes.D || parent.CEI_SubStyle == EntrySubstyleCodePairList.Codes.F))
			{
				if (parent.CEI_DateForDuty.IsValid && !parent.CEI_DateForDuty.IsInTheFuture())
				{
					parent.CEI_DateForDutyInfo.AddMessageError(AssessmentDateInThePastMessage);
				}

				if (parent.CEI_DateForDuty.IsValid && parent.CEI_DateForDuty.AddDays(-30).IsInTheFuture())
				{
					parent.CEI_DateForDutyInfo.AddMessageError(AssessmentDateInTheFutureMessage);
				}
			}
		}

		protected override void CheckGoodsLocationDescription()
		{
			base.CheckGoodsLocationDescription();
			CheckRuleC0810_N01();
		}

		void CheckRuleC0810_N01()
		{
			var instruction = Parent;
			if (instruction?.Validation.ValidationDecider is IEntryInstructionValidationDecider validationDecider
				&& validationDecider.IsRuleC0810_N01Active
				&& !instruction.GoodsLocationDescription.IsEmpty
				&& instruction.InvoiceLines.Cast<JobComInvoiceLine>().All(x => x.IsTradingWithSpecialFiscalTerritoriesProcedure))
			{
				instruction.GoodsLocationDescriptionInfo.AddMessageError(Res.GetString("274B89F9-84D4-4A62-80CC-6AE592134201", "[C0810_N01] Location Of Goods must be empty when all invoice lines Customs procedure end with F15 (Trading With Special Fiscal Territories)."));
			}
		}

		protected override void CheckCEI_Procedure()
		{
			base.CheckCEI_Procedure();
			CheckRuleC0834_N02(Parent.CEI_ProcedureInfo);
		}

		void CheckRuleC0834_N02(ZPropertyInfo propertyInfo)
		{
			var parent = Parent;
			if (parent.Validation.ValidationDecider is IEntryInstructionValidationDecider validationDecider
				&& validationDecider.IsRuleC0834_N02Active
				&& parent.CEI_Procedure == RefCusProcedure.Procedure._53
				&& !parent.FiscalReferences.Cast<CusFiscalReference>().Any(x => IsSuitableFor53Procedure(x.CFR_Code)))
			{
				propertyInfo.AddMessageError(Res.GetString("F40A90BB-53E5-4F6C-BA11-40B3D4B17C95", "[C0834_N02] For Temporary Admission under CPC 53 a fiscal reference of any type but FR5 must be served."));
			}
		}

		bool IsSuitableFor53Procedure(string fiscalReferenceCode) => fiscalReferenceCode == FiscalReferenceCodeList.Codes.FR1_Importer
																|| fiscalReferenceCode == FiscalReferenceCodeList.Codes.FR2_Customer
																|| fiscalReferenceCode == FiscalReferenceCodeList.Codes.FR3_TaxRepresentative
																|| fiscalReferenceCode == FiscalReferenceCodeList.Codes.FR4_HolderOfDeferredPaymentAuth;

		protected override void CheckCEI_Style()
		{
			base.CheckCEI_Style();
			CheckRuleR0933_N03();
			var parent = Parent;
			var declarationType = parent.CEI_Style;
			MandatoryValidation.MessageErrorIfNotEntered(parent.CEI_StyleInfo);

			if (!declarationType.IsEmpty && declarationTypesNeedValidation.Contains(declarationType))
			{
				if (parent.SpecificRegimeAuthorisationUsage is null)
				{
					var listOfValidAuthorisationTypes = string.Join(",", FRCusAuthorizationUsageUpdater.GetValidAuthorizationTypes(declarationType).ToArray());
					parent.CEI_StyleInfo.AddMessageError(Res.GetString("F3E11F92-A122-4840-A57A-8BD286368145","One of {0} authorization is mandatory for Declaration Type {1}, please configure it by clicking menu item Brokerage > Data > Auto Populate Authorizations, or selecting one in the Authorization grid manually.", listOfValidAuthorisationTypes, declarationType));
				}
			}
		}

		void CheckRuleR0933_N03()
		{
			var parent = Parent;
			if (parent?.Validation.ValidationDecider is IEntryInstructionValidationDecider validationDecider
				&& validationDecider.IsRuleR0933_N03Active
				&& parent.CEI_Style == DeltaIEImportDeclarationTypeList.Codes.I1
				&& parent.CusAuthorizationUsages.Cast<CusAuthorizationUsage>().All(x => x.AGC_Code != Customs.Business.CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration))
			{
				parent.CEI_StyleInfo.AddMessageError(Res.GetString("B1A5C30C-DFA3-4E1F-96E4-AA586AFF19CD", "R0933_N03 A Simplified authorization must be selected (SDE)."));
			}
		}
		protected override void CheckCEI_DateForDuty()
		{
			base.CheckCEI_DateForDuty();

			if (Parent.CEI_DateForDuty.IsEmpty)
			{
				Parent.CEI_DateForDutyInfo.AddMessageError(EmptyAssessmentDateMessage);
			}
		}

		protected override void CheckCEI_SubStyle()
		{
			base.CheckCEI_SubStyle();
			if (Parent.Validation.ValidationDecider is IEntryInstructionValidationDecider { IsRuleNAT_130BisActive: true }
				&& (Parent.JobDeclaration as JobDeclaration).IsDeclarationStandard
				&& Parent.CEI_SubStyle != EntrySubstyleCodePairList.Codes.A)
			{
				Parent.CEI_SubStyleInfo.AddMessageError(Res.GetString("39A673D5-3988-4DA4-806B-CD0A7703FE83", "[NAT_130_Bis] Entry Instruction Sub Style must be A when the declaration has E0001 Additional Information."));
			}
		}

		protected override void CheckCEI_OA_Warehouse()
		{
			base.CheckCEI_OA_Warehouse();
			var instruction = Parent;
			if (!instruction.HasIntoRegimeProcedure && instruction.HasOutOfRegimeProcedure && instruction.Warehouse is OrgAddress warehouse)
			{
				CheckWarehouseHasAuthorisation(warehouse, instruction.CEI_OA_WarehouseInfo);
			}
		}

		protected override void CheckCEI_OA_Warehouse2()
		{
			base.CheckCEI_OA_Warehouse2();
			var instruction = Parent;
			if (instruction.HasIntoRegimeProcedure && instruction.Warehouse2 is OrgAddress warehouse2)
			{
				CheckWarehouseHasAuthorisation(warehouse2, instruction.CEI_OA_Warehouse2Info);
			}
		}

		void CheckWarehouseHasAuthorisation(OrgAddress warehouseAddress, ZPropertyInfo info)
		{
			var country = Parent.CountryCode;
			if (!warehouseAddress.GetCusAuthorisationHeadersNumberWithApplyingAddressForCustomsWarehouse(country).Any())
			{
				info.AddMessageError(ErrorCollectorHelper.WarehouseAuthorisationNotFoundMessage);

				var randomAuthorisations = warehouseAddress.GetCusAuthorisationHeadersWithApplyingAddress(country).ToList();
				if (randomAuthorisations.Any())
				{
					info.AddMessageError(ZString.Format(ErrorCollectorHelper.AlternativePurposeAuthorisationFound, randomAuthorisations.First().CPH_Type));
				}
			}
		}

		public static string EmptyAssessmentDateMessage => Res.GetString("3bb1dace-ad13-43cd-beca-aa39ea6a7042", "Please enter an assessment date.");
	}
}
