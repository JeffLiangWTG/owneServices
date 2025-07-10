using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ImportJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public ImportJobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public override void CheckSpecialCasesExists()
		{
			var cpc = Parent.JI_Procedure.SubstringSafe(0, 4);
			if (instruction != null && instruction.CEI_Style == ImportEntryTypeList.Codes.SingleDeclarationFreeCirculation
				&& (cpc == UniversalReferenceConstants.CustomsProcedureCodes._4054 || cpc == UniversalReferenceConstants.CustomsProcedureCodes._4254)
				&& !Parent.Taxes.Cast<JobComInvoiceLineTax>().Any(x => SpecialCaseGroupList.IsInGroups1to12And20(Parent.Factory, x.JLT_Type)))
			{
				Parent.AddRowMessageError(Res.GetString("B6674BB3-1C5A-4613-9F3E-F7774AC2C9F5", "The CPC of this Invoice Line requires Special Cases with Group '01'-'12' or Group '20'."));
			}
		}

		static readonly ImmutableArray<string> ConcessionsThatRequirePositiveCustomsThirdQuantity =
			ImmutableArray.Create(
				CustomsProcedureCodeList.Import.Concession._E01,
				CustomsProcedureCodeList.Import.Concession._E02,
				CustomsProcedureCodeList.Import.Concession._8E2,
				CustomsProcedureCodeList.Import.Concession._8E3,
				CustomsProcedureCodeList.Import.Concession._8E6,
				CustomsProcedureCodeList.Import.Concession._8E8,
				CustomsProcedureCodeList.Import.Concession._8E9
			);

		protected override void CheckJI_CustomsThirdQuantity()
		{
			base.CheckJI_CustomsThirdQuantity();
			var parent = Parent;
			if (parent.JI_CustomsThirdQuantity == 0 && ConcessionsThatRequirePositiveCustomsThirdQuantity.Contains(parent.Concession))
			{
				parent.JI_CustomsThirdQuantityInfo.AddMessageError(Res.GetString("86a3c073-ad21-4237-b1e7-bc2c48a976ae", "Third Qty (Customs Qty) should be greater than zero."));
			}
		}

		public override void CheckAirFreightCostsExists()
		{
			var parent = Parent;
			var declaration = parent.Declaration;
			if (declaration.ZG_IsHighValueOvrd
				&& declaration.JE_TransportMode == TransportTypeList.Codes.Air
				&& parent.InvoiceHeader.ZG_AgreedPlaceCode == UniversalReferenceConstants.AgreedPlaceCodes._1
				&& !airFreightCostsExist)
			{
				parent.AddRowMessageError(Res.GetString("1305181F-6371-47DC-B486-EAFE5B79485E", "You have not entered Air Freight Costs with charge code '010/014' – Or use 'Calculate Freight' Button on Invoice Header Level."));
			}
		}

		public override void CheckInwardProcessingProducts()
		{
			var parent = Parent;
			if (instruction != null &&
				instruction.CEI_SimplifiedGrantAuthorization == SimplifiedGrantAuthorizationList.Codes.J &&
				instruction.EnabledInwardProcessing &&
				!parent.InwardProcessingProducts.Any())
			{
				parent.AddRowMessageError(Res.GetString("919A3F7B-82EF-4719-88C3-B34951FFB8C7", "You should enter at least one Processed Product on the 'Inward Processing' tab."));
			}
		}

		protected override void CheckJI_CustomsValue()
		{
			base.CheckJI_CustomsValue();

			var parent = Parent;
			if (parent.JI_CustomsValue < 0)
			{
				parent.AddRowMessageError(Res.GetString("BB0748C0-A89F-44B9-BB7C-79107E652912", "Customs Value cannot be smaller than zero. Please check Line Price and/or deduction Charges."));
			}
		}

		protected override void CheckJI_BondedWhsQuantity()
		{
			var parent = Parent;
			if ((instruction?.IsInwardMovementApplicable ?? false) || parent.IsOutOfWarehouseWarehousing)
			{
				var info = parent.JI_BondedWhsQuantityInfo;
				var value = parent.JI_BondedWhsQuantity;
				MandatoryValidation.CheckNotNegative(info);

				if (!parent.IsOutOfWarehouseWarehousing || !parent.JI_BondedWhsUnitQty.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(info);
				}

				if (value.ToString().Replace(".", ZString.Empty).Length > 12)
				{
					info.AddMessageError(Res.GetString("C1719882-C082-4C26-9FA4-DD534552FCF5", "A maximum of 12 numbers (including decimals) can be entered."));
				}
				if (parent.Factory.IsIntegerRequiredUnitOfQuantity(parent.JI_BondedWhsUnitQty) && !value.IsInteger)
				{
					info.AddMessageError(Res.GetString("EB1A7617-8A27-46FE-A57B-EE4C309EF82C", "Unit of quantity 'NAR', 'NARB', 'NCL' or 'NPR' requires an Integer value."));
				}
			}
		}

		protected override void CheckJI_PreviousEntryLineNumber()
		{
			base.CheckJI_PreviousEntryLineNumber();
			var line = Parent;
			if (line.IsOutOfWarehouseWarehousing && !line.JI_PreviousEntryNumber.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(line.JI_PreviousEntryLineNumberInfo, line.JI_PreviousEntryLineNumberBondedWarehouseCaption.Caption);
			}
		}

		protected override void CheckJI_PreviousEntryNumber()
		{
			base.CheckJI_PreviousEntryNumber();
			var line = Parent;
			if (line.IsOutOfWarehouseWarehousing && !line.JI_BondedWhsQuantity.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(line.JI_PreviousEntryNumberInfo, line.JI_PreviousEntryNumberBondedWarehouseCaption.FullDescription);
			}
		}

		static readonly ImmutableArray<string> ConcessionsThatRequireValuationCode32 =
			ImmutableArray.Create(
				CustomsProcedureCodeList.Import.Concession._E01,
				CustomsProcedureCodeList.Import.Concession._E02
			);

		static readonly ImmutableArray<string> ConcessionsThatRequireValuationCodeOtherThen32 = ImmutableArray.Create(CustomsProcedureCodeList.Import.Concession._8E2);

		protected override void CheckJI_Procedure()
		{
			CheckJI_ProcedureForImportAndWarehouseAdjustment();

			var parent = Parent;
			var cpc = parent.JI_Procedure;
			if (!cpc.IsEmpty && cpc.SubstringSafe(0, 2) != (instruction?.CEI_Procedure ?? ZString.Empty))
			{
				parent.JI_ProcedureInfo.AddMessageError(Res.GetString("46013BCB-6E62-41C0-B408-951052E832D7", "Current Procedure must match Procedure on Entry Instruction."));
			}

			var valuationCode = parent.InvoiceHeader.JZ_ValuationCode;
			var currentConcessionCode = parent.Concession;
			if (valuationCode != UniversalReferenceConstants.ValuationCodes._32 && ConcessionsThatRequireValuationCode32.Contains(currentConcessionCode))
			{
				parent.JI_ProcedureInfo.AddMessageError(Res.GetString("829b921d-a811-4175-85e0-836d94274199", "Concession Codes 'E01' and 'E02' require [24] Transaction Nature to be '32'."));
			}
			else if (valuationCode == UniversalReferenceConstants.ValuationCodes._32 && ConcessionsThatRequireValuationCodeOtherThen32.Contains(currentConcessionCode))
			{
				parent.JI_ProcedureInfo.AddMessageError(Res.GetString("72753ab5-eb46-4639-ae65-3fd316eb4c08", "Concession Code '8E2' requires a [24] Transaction Nature other than '32'."));
			}
		}

		protected override void CheckJI_BondedWhsUnitQty()
		{
			if ((instruction?.IsInwardMovementApplicable ?? false) || Parent.IsOutOfWarehouseWarehousing)
			{
				var info = Parent.JI_BondedWhsUnitQtyInfo;
				ListValidation.MessageErrorIfInvalidCode(info);
				if (!Parent.JI_BondedWhsQuantity.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(info);
				}
			}
		}

		protected override void CheckJI_ValuationCode()
		{
		}

		protected override void CheckJI_CustomsSecondQuantityIsValidZDecimal()
		{
			TypeValidation.CheckValidDecimal(Parent.JI_CustomsSecondQuantityInfo, 12, 3);
		}

		protected override void CheckJI_LineNo()
		{
			base.CheckJI_LineNo();
			var parent = Parent;
			if (parent.ContentInformationTypes.Count > 3)
			{
				parent.JI_LineNoInfo.AddMessageError(Res.GetString("BE513068-62E6-448B-B40F-97D8C9CA220A", "The maximum number of the Content Information grid records is 3."));
			}
		}

		protected override void CheckJI_Tariff()
		{
			base.CheckJI_Tariff();

			var tariff = Parent.JI_Tariff;
			if (!tariff.IsEmpty)
			{
				var cusTariff = Parent.UniversalTariff;
				if (cusTariff != null && !cusTariff.IsTariffNationalCode)
				{
					Parent.JI_TariffInfo.AddMessageError(Res.GetString("8AE4EBD9-1923-4A59-9638-CB3A0FCD5D03", "The Tariff Code entered is not valid for the current context."));
				}
			}
		}

		protected override void CheckJI_Tariff_NoPackage_Import(EU.Business.Declaration.JobDeclaration dec)
		{
			if (instruction == null || instruction.CEI_Style != ImportDeclarationTypeList.Codes.AVABR)
			{
				base.CheckJI_Tariff_NoPackage_Import(dec);
			}
		}

		protected override void CheckJI_Weight()
		{
			base.CheckJI_Weight();

			var invoiceHeader = Parent.InvoiceHeader;
			if (invoiceHeader?.JZ_Weight <= ZDecimal.Zero && Parent.JI_Weight <= ZDecimal.Zero)
			{
				Parent.JI_WeightInfo.AddMessageError(Res.GetString("7A0257FA-3E95-4F79-B797-0976612C74CC", "You have not entered a valid Gross Weight."));
			}
		}

		protected override void CheckJI_ExtraInfoForClassification()
		{
			base.CheckJI_ExtraInfoForClassification();
			if (!Parent.ZG_IdentificationMeansType.IsEmpty && (instruction?.EnabledInwardProcessing ?? ZBool.False))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_ExtraInfoForClassificationInfo);
			}
		}

		protected override void CheckJI_RN_NKCountryOfExport()
		{
			base.CheckJI_RN_NKCountryOfExport();

			if (instruction != null && instruction.CEI_Style == ImportDeclarationTypeList.Codes.LUZ)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_RN_NKCountryOfExportInfo, Res.GetString("DA715A9A-F1DD-4C9C-9465-447CE5A88054", "Origin"));
				ListValidation.MessageErrorIfInvalidCode(Parent.JI_RN_NKCountryOfExportInfo, Parent.Lookups.CountryOfExports);
			}
		}

		protected override void CheckJI_CustomDate1()
		{
			base.CheckJI_CustomDate1();

			if (instruction != null && instruction.CEI_Style == ImportDeclarationTypeList.Codes.LUZ)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_CustomDate1Info);

				if (Parent.JI_CustomDate1 > ZDate.Today)
				{
					Parent.JI_CustomDate1Info.AddMessageError(Res.GetString("734BEF21-F7B1-4C81-93FE-A2D6F7ED9AB2", "Decisive Date cannot be in future."));
				}
			}
		}

		protected override void CheckJI_CustomsFourthQuantity()
		{
			base.CheckJI_CustomsFourthQuantity();
			if (Parent.Factory.IsIntegerRequiredUnitOfQuantity(Parent.JI_CustomsFourthUnitQty) && !Parent.JI_CustomsFourthQuantity.IsInteger)
			{
				Parent.JI_CustomsFourthQuantityInfo.AddMessageError(Res.GetString("C24D7F88-1A24-4B73-9CE7-C01D6C536CFE", "Only integer values are allowed for this Fourth Qty Unit"));
			}
		}

		protected override void CheckJI_CustomsFourthQuantityIsValidZDecimal()
		{
			TypeValidation.CheckValidDecimal(Parent.JI_CustomsFourthQuantityInfo, 12, 3);
		}

		protected override void CheckJI_CustomsFourthUnitQty()
		{
			base.CheckJI_CustomsFourthUnitQty();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_CustomsFourthUnitQtyInfo);
			if (Parent.JI_CustomsFourthQuantity > 0)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_CustomsFourthUnitQtyInfo);
			}
		}

		protected override void CheckJI_BondedWHSOrderLineNumber()
		{
			var parent = Parent;
			if (!parent.JI_BondedWHSOrderNumber.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_BondedWHSOrderLineNumberInfo);
			}
		}

		protected override CargoWise.ComponentModel.INotificationType NoApplicableRateNotificationSeverity => CargoWise.ComponentModel.NotificationType.Warning;

		protected override bool HasValidPackagePivots
		{
			get
			{
				var parent = Parent;
				bool result;
				if (!ZShort.TryParse(parent.JI_Calc_MergedLineNumber, out var mergedLineNumber))
				{
					result = parent.JI_LineNo > 1 || parent.PackagesPivot.Count > 0;
				}
				else
				{
					result = mergedLineNumber > 1 || parent.CusEntryLine.AtLeastOneInvoiceLineHasPackingDetails;
				}
				return result;
			}
		}

		bool airFreightCostsExist => Parent.Factory.GetValue(ref airFreightCostsExistCached, () => Parent.Charges.Cast<InvoiceLineCharge>().Any(x => x.J7_ChargeType == ImportChargeCodeList.Codes._010 || x.J7_ChargeType == ImportChargeCodeList.Codes._014)
			|| Parent.ApportionedCharges.Cast<InvoiceLineApportionCharge>().Any(x => x.J7_ChargeType == ImportChargeCodeList.Codes._010 || x.J7_ChargeType == ImportChargeCodeList.Codes._014));
		CachedProperty<bool> airFreightCostsExistCached;
	}
}
