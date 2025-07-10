using System;
using System.Collections;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Shared;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using GuaranteeCodes = Enterprise.Customs.EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsDepartureCargoDescPhase5Validation(NctsDepartureCargoDesc parent)
		: NctsDepartureCargoDescValidation(parent)
	{
		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidatePackages_TR0001Rule();
			CheckConditionR0020();
			CheckConditionR0020_1();
			CheckSupportingAndAdditionalDocumentsCountRuleE1407();
			CheckRuleDutyTR0096();
		}

		protected override void CheckBY_HarmonisedTariff()
		{
			base.CheckBY_HarmonisedTariff();
			CheckBY_HarmonisedTariffLength();
			if (Parent.IsInPhase5TransitionPeriod)
			{
				CheckBY_HarmonisedTariff_NR0025();
			}
			else
			{
				CheckBY_HarmonisedTariff_C0153_1Rule();
			}

			CheckBY_HarmonisedTariff_NR0024();
			CheckBY_HarmonisedTariff_C0821_1Rule();
			CheckBY_HarmonisedTariffDataGroup();
		}

		protected virtual void CheckBY_HarmonisedTariffLength()
		{
			var parent = Parent;

			var harmonisedTariff = parent.BY_HarmonisedTariff;
			if (!harmonisedTariff.IsEmpty && !harmonisedTariff.Length.In(harmonisedTariffAllowedLength))
			{
				var info = parent.BY_HarmonisedTariffInfo;
				info.AddMessageError(Res.GetString("D7E14D7D-BD02-4C21-861C-DA745326E0E6", "{0} must be 6/8/10 digits", info.HumanReadableName));
			}
		}
		int[] harmonisedTariffAllowedLength => new[] { 6, 8, 10 };

		protected virtual void CheckBY_HarmonisedTariffDataGroup()
		{
			var parent = Parent;

			if (!parent.BY_HarmonisedTariff.IsEmpty && parent.UniversalTariff == null)
			{
				parent.BY_HarmonisedTariffInfo.AddMessageError(ListValidation.InvalidCodeMessageError.ToString());
			}
		}

		protected override void CheckCountryOfDestinationRule()
		{
			if (ValidationDecider is INctsDepartureCargoDescPhase5ValidationDecider validationDecider && (IsRuleC0343_1Applicable || validationDecider.IsRuleC0343_2Active))
			{
				var parent = Parent;
				var considerBillLevel = validationDecider.IsRuleC0343_2Active || !parent.IsInPhase5TransitionPeriod;
				if (parent.BY_RN_NKCountryOfDestination.IsEmpty && (!considerBillLevel || (parent.Bill?.B0_RN_NKCountryOfDestination.IsEmpty ?? false)) && (parent.MoveHeader?.BM_RL_NKDestinationPort.IsEmpty ?? false))
				{
					var message = validationDecider.IsRuleC0343_2Active ? ValidationRuleConfiguration.Messages.C0343_2Message
					: parent.IsInPhase5TransitionPeriod ? ValidationRuleConfiguration.Messages.C0343_1aMessage : ValidationRuleConfiguration.Messages.C0343_1bMessage;
					parent.BY_RN_NKCountryOfDestinationInfo.AddMessageError(message);
				}
			}
		}

		protected virtual bool IsRuleC0343_1Applicable => ValidationDecider?.IsRuleC0343_1Active ?? false;

		protected override void CheckCountryOfDispatchRule()
		{
			if ((ValidationDecider?.IsRuleC0909Active ?? false) || IsRuleC0909_1Applicable)
			{
				var parent = Parent;

				if (parent.MoveHeader is NctsDepartureMovementHeader moveHeader)
				{
					var info = parent.BY_RN_NKCountryOfDispatchInfo;
					var goodsItemDispatchCountry = parent.BY_RN_NKCountryOfDispatch;
					var declarationDispatchCountry = moveHeader.BM_RN_NKCountryOfDispatch;
					var houseConsignmentDispatchCountry = parent.Bill?.B0_RN_NKCountryOfExport ?? ZString.Empty;
					var numberOfTimesDispatchCountrySpecified = (goodsItemDispatchCountry.IsEmpty ? 0 : 1) + (declarationDispatchCountry.IsEmpty ? 0 : 1);
					var isInTransitionPeriod = Parent.IsInPhase5TransitionPeriod;
					if (!isInTransitionPeriod)
					{
						numberOfTimesDispatchCountrySpecified += houseConsignmentDispatchCountry.IsEmpty ? 0 : 1;
					}

					if (ValidationDecider?.IsRuleC0909Active ?? false)
					{
						if (numberOfTimesDispatchCountrySpecified == 0 && houseConsignmentDispatchCountry.IsEmpty)
						{
							parent.BY_RN_NKCountryOfDispatchInfo.AddMessageError(NctsHeaderValidationHelper.C0909ValidationMessage);
						}
					}

					if (IsRuleC0909_1Applicable
						&& numberOfTimesDispatchCountrySpecified == 0)
					{
						info.AddMessageError(isInTransitionPeriod ? ValidationRuleConfiguration.Messages.C0909_1bMessage : ValidationRuleConfiguration.Messages.C0909_1aMessage);
					}
				}
			}
		}

		protected virtual bool IsRuleC0909_1Applicable => ValidationDecider?.IsRuleC0909_1Active ?? false;

		protected override void CheckBY_CusC4Number()
		{
			base.CheckBY_CusC4Number();

			ListValidation.MessageErrorIfInvalidCode(Parent.BY_CusC4NumberInfo);
		}

		protected override void CheckBY_RN_NKCountryOfDestination()
		{
			base.CheckBY_RN_NKCountryOfDestination();

			CheckBY_RN_NKCountryOfDestination_C0343Rule();
			Parent.CheckConditionR0507(Parent.BY_RN_NKCountryOfDestinationInfo, x => x.BY_RN_NKCountryOfDestination);
		}

		protected override void CheckBY_CommercialReferenceNumber()
		{
			base.CheckBY_CommercialReferenceNumber();
			CheckBY_CommercialReferenceNumber_R0507_1Rule();
			var parent = Parent;
			var bill = parent.Bill;

			if (bill is null)
			{
				return;
			}

			var header = parent.Header;
			var ruleMessages = header.Configuration.ValidationRuleConfiguration.Messages;
			var uniqueConsignmentReferenceIsEmpty = header.MovementHeader.BM_UniqueConsignmentReference.IsEmpty;
			var referenceNumberIsEmpty = parent.BY_CommercialReferenceNumber.IsEmpty;
			var info = parent.BY_CommercialReferenceNumberInfo;

			if (bill.ValidationDecider is INctsBillDeparturePhase5ValidationDecider billValidationDecider)
			{
				if (billValidationDecider.IsRuleB1895_1Active)
				{
					if (!referenceNumberIsEmpty && !uniqueConsignmentReferenceIsEmpty)
					{
						info.AddMessageError(ruleMessages.B1895_1Message);
					}
				}
				else if (IsRuleC0502Applicable
						&& IsBY_CommercialReferenceNumberMandatory()
						&& referenceNumberIsEmpty)
				{
					info.AddMessageError(ruleMessages.C0502Message);
				}
			}

			if (!suspenderForBY_CommercialReferenceNumberUniqueValidation.IsSuspended)
			{
				parent.CheckConditionR0507(info, x => x.BY_CommercialReferenceNumber);
			}
		}

		protected virtual bool IsRuleC0502Applicable => Parent.Bill?.ValidationDecider is INctsBillDeparturePhase5ValidationDecider billValidationDecider && billValidationDecider.IsRuleC0502Active;

		void CheckBY_CommercialReferenceNumber_R0507_1Rule()
		{
			if ((ValidationDecider?.IsRuleR0507_1Active ?? false)
				&& !Parent.BY_CommercialReferenceNumber.IsEmpty
				&& Parent.Bill is NctsBill bill
				&& bill.GoodsItems.Count > 1
				&& !bill.GoodsItems.Any(x => !x.BY_CommercialReferenceNumber.EqualsIgnoringCase(Parent.BY_CommercialReferenceNumber)))
			{
				Parent.BY_CommercialReferenceNumberInfo.AddMessageError(ValidationRuleConfiguration.Messages.R0507_1CommercialReferenceNumberMessage);
			}
		}

		protected override void CheckBY_NetWeight()
		{
			base.CheckBY_NetWeight();

			var parent = Parent;
			CheckConditionC0837();
			CheckConditionRP11(parent, parent.BY_NetWeightInfo);
			CheckConditionE1109(parent.BY_NetWeightInfo);
			CheckConditionB1805_1Rule(parent, parent.BY_NetWeightInfo);
			CheckConditionC0837_1();
		}

		protected override void AddR0223Notification(ZPropertyInfo netWeightInfo)
		{
			if (!Parent.IsInPhase5TransitionPeriod)
			{
				netWeightInfo.AddMessageError(ValidationRuleConfiguration.Messages.R0223Message);
				return;
			}
			base.AddR0223Notification(netWeightInfo);
		}

		protected override void CheckBY_TransportChargesMethodOfPayment()
		{
			base.CheckBY_TransportChargesMethodOfPayment();
			ValidateTransportChargesMethodOfPaymentRuleB1875_1();
			CheckBY_TransportChargesMethodOfPayment_B2400_1();
		}

		void CheckConditionC0837()
		{
			var parent = Parent;
			if (parent.BY_NetWeight.IsEmpty
				&& BillHasAnyPreviousDocumentWithKindN830()
				&& IsRuleC0837Applicable)
			{
				parent.BY_NetWeightInfo.AddMessageError(Res.GetString("94F4E383-33B3-4FB4-A918-732F08363C09", "[C0837] You have not entered a Net Weight"));
			}
		}

		protected virtual bool IsRuleC0837Applicable => ValidationDecider?.IsRuleC0837Active ?? false;

		void CheckConditionC0837_1()
		{
			var parent = Parent;

			if (parent.Header is NctsHeader header
				&& !parent.BY_NetWeight.IsEmpty
				&& (ValidationDecider?.IsRuleC0837_1Active ?? false)
				&& !parent.IsInPhase5TransitionPeriod
				&& header.MovementHeader is NctsDepartureMovementHeader movementHeader
				&& movementHeader.BM_ReducedDatasetIndicator
				&& !BillHasAnyPreviousDocumentWithKindN830())
			{
				parent.BY_NetWeightInfo.AddMessageError(header.Configuration.ValidationRuleConfiguration.Messages.C0837_1Message);
			}
		}

		void CheckConditionB1805_1Rule(NctsDepartureCargoDesc goodsItem, ZPropertyInfo attributeInfo)
		{
			var header = goodsItem.Header;
			if ((ValidationDecider?.IsRuleB1805_1Active ?? false)
				&& header.MovementHeader.BM_ReducedDatasetIndicator
				&& !goodsItem.BY_NetWeight.IsEmpty)
			{
				attributeInfo.AddMessageError(Res.GetString("6E207746-7F7B-4BE6-8132-5AB97651AB88", "[B1805-1] During the transition period, which is now, Net weight must be empty"));
			}
		}

		protected override void CheckConditionC901()
		{
			if (ValidationDecider?.IsRuleC901Active ?? false)
			{
				base.CheckConditionC901();
			}
		}

		protected override void CheckBY_GrossWeight()
		{
			base.CheckBY_GrossWeight();
			var parent = Parent;
			CheckConditionRP11(parent, parent.BY_GrossWeightInfo);
			CheckRuleR0221_1(parent);
			CheckRuleR0221_3(parent);
			CheckGrossWeightRuleB2101(parent);
			CheckConditionE1109(Parent.BY_GrossWeightInfo);
			CheckRuleNR0020(parent);
		}

		void CheckRuleNR0020(NctsDepartureCargoDesc goodsItem)
		{
			if ((ValidationDecider?.IsRuleNR0020Active ?? false)
				&& goodsItem.BY_GrossWeight == 0
				&& goodsItem.Packages.Cast<NctsPackage>().Any(p => p.B5_UnitCount > 0 || p.IsBulk))
			{
				goodsItem.BY_GrossWeightInfo.AddMessageError(ValidationRuleConfiguration.Messages.NR0020Message);
			}
		}

		void CheckConditionE1109(ZPropertyInfo propertyInfo)
		{
			var parent = Parent;
			if (ValidationDecider?.IsRuleE1109Active ?? false)
			{
				UniversalValidationHelper.CheckMaxValueAndMaxDecimalLengthForWeightIfPhase5TransitionPeriod(
					parent.IsInPhase5TransitionPeriod,
					parent.Header.Configuration.ValidationRuleConfiguration.Messages.E1109Message(propertyInfo.Description), propertyInfo, 11, 3);
			}
		}

		protected override void CheckBY_RN_NKCountryOfDispatch()
		{
			base.CheckBY_RN_NKCountryOfDispatch();
			Parent.CheckConditionR0507(Parent.BY_RN_NKCountryOfDispatchInfo, x => x.BY_RN_NKCountryOfDispatch);
			CheckBY_RN_NKCountryOfDispatch_R0507_1Rule();
		}

		void CheckBY_RN_NKCountryOfDispatch_R0507_1Rule()
		{
			if ((ValidationDecider?.IsRuleR0507_1Active ?? false)
				&& !Parent.BY_RN_NKCountryOfDispatch.IsEmpty
				&& Parent.Bill is NctsBill bill
				&& bill.GoodsItems.Count > 1
				&& !bill.GoodsItems.Any(x => !x.BY_RN_NKCountryOfDispatch.EqualsIgnoringCase(Parent.BY_RN_NKCountryOfDispatch)))
			{
				Parent.BY_RN_NKCountryOfDispatchInfo.AddMessageError(ValidationRuleConfiguration.Messages.R0507_1CountryOfDispatchMessage);
			}
		}

		protected override void CheckBY_Type()
		{
			base.CheckBY_Type();
			var parent = Parent;
			parent.CheckConditionR0507(parent.BY_TypeInfo, x => x.BY_Type);
			CheckConditionR0601_1(parent, parent.BY_TypeInfo);
			CheckConditionR0909(parent, parent.BY_TypeInfo);
			CheckConditionC0045(parent, parent.BY_TypeInfo);
			CheckBY_TypeRuleB1922(parent);
		}

		void CheckConditionR0601_1(NctsDepartureCargoDesc goodsItem, ZPropertyInfo info)
		{
			if (!goodsItem.BY_Type.IsEmpty
				&& goodsItem.Header is NctsHeader nctsHeader
				&& ValidationDecider.IsRuleR0601_1Active
				&& !goodsItem.IsInPhase5TransitionPeriod)
			{
				var refCusCodeListCL234 = nctsHeader.GetCL234List();

				if (goodsItem.AdditionalInfos.Cast<AdditionalInfo>().Any(d => d.CSI_SubType.EqualsIgnoringCase(AdditionalInfoSubTypeList.Codes.AdditionalReference) && refCusCodeListCL234.ContainsCode(d.CSI_Code))
					&& (!goodsItem.BY_Type.EqualsIgnoringCase(NctsPhase5DeclarationTypeList.Codes.T1) || !goodsItem.PreviousDocuments.Cast<AutoCusSupportingInfo>().Any(d => d.CSI_Code.EqualsIgnoringCase(UniversalReferenceConstants.PreviousDocumentTypes.CommercialInvoice))))
				{
					info.AddMessageError(Res.GetString("5887AB4C-9B17-49EE-99AF-6B93463098BE", "Please enter 'N380' in Previous Document and 'T1' as Declaration type at Consignment item when Additional Reference Type at Consignment item has Excise Codes."));
				}

				if (!goodsItem.BY_Type.EqualsIgnoringCase(NctsPhase5DeclarationTypeList.Codes.T2) && !goodsItem.BY_Type.EqualsIgnoringCase(NctsPhase5DeclarationTypeList.Codes.T2F)
					&& goodsItem.SupportingDocuments.Cast<AutoCusSupportingInfo>().Any(d => refCusCodeListCL234.ContainsCode(d.CSI_Code)))
				{
					info.AddMessageError(Res.GetString("7ACABE42-9858-41DA-BFC4-A9549F61C316", "Declaration type should be T2 or T2F at Consignment level or Consignment Item level when Supporting Document Type at Consignment item level has Excise Codes."));
				}
			}
		}

		void CheckConditionR0909(NctsDepartureCargoDesc goodsItem, ZPropertyInfo info)
		{
			var declarationType = goodsItem.BY_Type;
			if (goodsItem.Header is NctsHeader header
				&& (ValidationDecider?.IsRuleR0909Active ?? false)
				&& !declarationType.IsEmpty
				&& declarationType != NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure
				&& declarationType != NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedureBetweenDifferentFiscalTerritories)
			{
				header.CheckConditionR0909_ToSanMarinoFromACountryOtherThanItaly(info);
			}
		}

		void CheckConditionC0045(NctsDepartureCargoDesc goodsItem, ZPropertyInfo info)
		{
			if (goodsItem.MoveHeader is NctsDepartureMovementHeader nctsDepartureMovementHeader
				&& (ValidationDecider?.IsRuleC0045Active ?? false))
			{
				if (nctsDepartureMovementHeader.IsMixedConsignment)
				{
					if (goodsItem.BY_Type.IsEmpty)
					{
						info.AddMessageError(goodsItem.Header.Configuration.ValidationRuleConfiguration.Messages.C0045aMessage);
					}
				}
				else if (!goodsItem.BY_Type.IsEmpty)
				{
					info.AddMessageError(goodsItem.Header.Configuration.ValidationRuleConfiguration.Messages.C0045bMessage);
				}
			}
		}

		protected override void CheckBY_GrossWeightUnit()
		{
			base.CheckBY_GrossWeightUnit();
			CheckGrossWeightUnitRuleB2101(Parent);
		}

		protected override void CheckBY_Description()
		{
			base.CheckBY_Description();
			ValidateDescriptionRuleE1107();
			ValidateDescriptionRuleE1107_1(Parent);
			ValidateDescriptionRuleRuleNR0021(Parent);
		}

		protected override void CheckBY_BondedWhsQuantity()
		{
			base.CheckBY_BondedWhsQuantity();

			if (!Parent.BY_BondedWhsUnitQty.IsEmpty)
			{
				if (ValidationDecider?.IsRuleNR0040Active ?? false)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.BY_BondedWhsQuantityInfo, messagePrefix: "[NR0040] ");
				}

				if ((ValidationDecider?.IsRuleNR0041Active ?? false)
					&& Parent.Factory.IsIntegerRequiredUnitOfQuantity(Parent.BY_BondedWhsUnitQty)
					&& !Parent.BY_BondedWhsQuantity.IsInteger)
				{
					Parent.BY_BondedWhsQuantityInfo.AddMessageError(Parent.Header.Configuration.ValidationRuleConfiguration.Messages.NR0041Message);
				}
			}
		}

		protected override void CheckBY_CustomsThirdQuantity()
		{
			base.CheckBY_CustomsThirdQuantity();

			var goodsItem = Parent;
			NctsValidationHelper.CheckRuleTR0084(goodsItem.Header, goodsItem.BY_CustomsThirdQuantityInfo, goodsItem.BY_CustomsThirdUnitQty, goodsItem.BY_CustomsThirdQuantity, NotificationType.MessageError);
		}

		protected override void CheckBY_CustomsFourthQuantity()
		{
			base.CheckBY_CustomsFourthQuantity();

			var goodsItem = Parent;
			NctsValidationHelper.CheckRuleTR0084(goodsItem.Header, goodsItem.BY_CustomsFourthQuantityInfo, goodsItem.BY_CustomsFourthUnitQty, goodsItem.BY_CustomsFourthQuantity, NotificationType.MessageError);
		}

		protected override void CheckBY_BondedWhsUnitQty()
		{
			base.CheckBY_BondedWhsUnitQty();

			if ((ValidationDecider?.IsRuleNR0042Active ?? false) && !Parent.BY_BondedWhsQuantity.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BY_BondedWhsUnitQtyInfo, messagePrefix: ValidationRuleCodeConstants.NR0042.GetRuleCodeMessagePrefix(true));
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.BY_BondedWhsUnitQtyInfo, Parent.Lookups.BondedWhsUnitQtyList);
		}

		protected override void CheckBY_WarehouseEntryLineNo()
		{
			base.CheckBY_WarehouseEntryLineNo();

			if ((ValidationDecider?.IsRuleNR0043Active ?? false) && !Parent.BY_WarehouseEntryNumber.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BY_WarehouseEntryLineNoInfo, messagePrefix: Parent.Header.Configuration.ValidationRuleConfiguration.Messages.NR0043RuleCode.GetRuleCodeMessagePrefix(true));
			}
		}

		protected override void CheckBY_BondedWHSOrderLineNumber()
		{
			base.CheckBY_BondedWHSOrderLineNumber();

			if ((ValidationDecider?.IsRuleNR0044Active ?? false) && !Parent.BY_BondedWHSOrderNumber.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BY_BondedWHSOrderLineNumberInfo, messagePrefix: ValidationRuleCodeConstants.NR0044.GetRuleCodeMessagePrefix(true));
			}
		}

		protected override void CheckBY_OP_PartIsValidZGuid()
		{
			if ((ValidationDecider?.IsRuleNR0045Active ?? false))
			{
				ListValidation.ErrorIfInvalidPK(Parent.BY_OP_PartInfo, Parent.Lookups.Parts,
					ResString.GetMultilingualString("9631BB24-CB2F-474B-94AE-B11F1DF52BEA", "{0} Please enter a valid Product which is required for Inventory Management integration.", Parent.Header.Configuration.ValidationRuleConfiguration.Messages.NR0045RuleCode.GetRuleCodeMessagePrefix()));
			}
		}

		protected override void CheckUNDGsAsString()
		{
			base.CheckUNDGsAsString();

			var dangerousGoods = Parent.UNDGs;
			if (dangerousGoods.Count > 0)
			{
				Parent.UNDGsAsStringInfo.AddNotificationBasedOnChildValidationStatus(
					Res.GetString("36E41D89-9098-4ADA-ACA6-B1643A6D4660", "There are errors within the 'Dangerous Goods', please click on 'More' to view the error information"),
					() => dangerousGoods.ForEach(undg => undg.Validation.ValidateAll()),
					dangerousGoods);
			}
		}

		protected override void CheckBY_RN_NKCountryOfOrigin()
		{
			base.CheckBY_RN_NKCountryOfOrigin();

			NctsValidationHelper.CheckBY_RN_NKCountryOfOrigin_RuleNR0058(Parent, ValidationDecider, ValidationRuleConfiguration);
		}

		protected override void CheckBY_MonetaryValue()
		{
			base.CheckBY_MonetaryValue();

			NctsValidationHelper.CheckBY_MonetaryValue_RuleNR0059(Parent, ValidationDecider, ValidationRuleConfiguration);
		}

		protected override void CheckBY_Supplements()
		{
			base.CheckBY_Supplements();

			NctsValidationHelper.CheckBY_Supplements_HasChildValidationNotification(Parent);
			NctsValidationHelper.CheckBY_Supplements_RuleNR0060(Parent, ValidationDecider, ValidationRuleConfiguration);
		}

		protected override void CheckBY_LinePrice()
		{
			base.CheckBY_LinePrice();

			var parent = Parent;
			var bill = parent.Bill;
			if ((ValidationDecider?.IsRuleTR0076Active ?? false) && parent.BY_LinePrice.IsEmpty &&
				bill?.Header != null && !bill.Header.Bills.SelectMany(b => b.GoodsItems).All(gi => gi.BY_LinePrice.IsEmpty))
			{
				parent.BY_LinePriceInfo.AddMessageError(ValidationRuleConfiguration.Messages.TR0076Message);
			}
			if ((ValidationDecider?.IsRuleTR0100Active ?? false) && parent.BY_LinePrice < 0)
			{
				parent.BY_LinePriceInfo.AddError(ValidationRuleConfiguration.Messages.TR0100Message);
			}
		}

		protected override void CheckBY_RX_NKLinePriceCurrency()
		{
			base.CheckBY_RX_NKLinePriceCurrency();

			var parent = Parent;
			if ((ValidationDecider?.IsRuleTR0068Active ?? false) && !parent.BY_LinePrice.IsEmpty && parent.BY_RX_NKLinePriceCurrency.IsEmpty)
			{
				parent.BY_RX_NKLinePriceCurrencyInfo.AddError(ValidationRuleConfiguration.Messages.TR0068Message);
			}

			ListValidation.ErrorIfInvalidCode(Parent.BY_RX_NKLinePriceCurrencyInfo);
		}

		void ValidateDescriptionRuleRuleNR0021(NctsDepartureCargoDesc goodsItem)
		{
			if (ValidationDecider?.IsRuleNR0021Active ?? false)
			{
				MandatoryValidation.MessageErrorIfNotEntered(goodsItem.BY_DescriptionInfo, messagePrefix: $"[{ValidationRuleCodeConstants.NR0021}] ");
			}
		}

		void ValidateDescriptionRuleE1107()
		{
			if (ValidationDecider?.IsRuleE1107Active ?? false)
			{
				var parent = Parent;
				UniversalValidationHelper.CheckMaxLengthIfPhase5TransitionPeriod(
					parent.IsInPhase5TransitionPeriod,
					parent.BY_DescriptionInfo,
					NctsConstants.CustomsFieldMaxLength.TransitionPeriod.DescriptionInGoodsItem,
					parent.Header.Configuration.ValidationRuleConfiguration.Messages.E1107RuleCode.GetRuleCodeMessagePrefix(addSpaceAtEnd: true));
			}
		}

		void ValidateDescriptionRuleE1107_1(NctsDepartureCargoDesc goodsItem)
		{
			var descriptionMaxLengthInTransitionPeriod = NctsConstants.CustomsFieldMaxLength.TransitionPeriod.DescriptionInGoodsItem;
			if ((ValidationDecider?.IsRuleE1107_1Active ?? false)
				&& goodsItem.IsInPhase5TransitionPeriod
				&& goodsItem.BY_Description.Length > descriptionMaxLengthInTransitionPeriod)
			{
				goodsItem.BY_DescriptionInfo.AddWarning(Res.GetString("186566E6-15AE-496E-BFBD-779CEDF601F5", "[E1107] In transition period, which is now, the maximum number of characters allowed is {0}; excess characters will be truncated in the Message", descriptionMaxLengthInTransitionPeriod));
			}
		}

		void CheckGrossWeightUnitRuleB2101(NctsDepartureCargoDesc goodsItem)
		{
			if ((ValidationDecider?.IsRuleB2101Active ?? false)
				&& !goodsItem.IsInPhase5TransitionPeriod
				&& goodsItem.BY_GrossWeightUnit.IsEmpty)
			{
				goodsItem.BY_GrossWeightUnitInfo.AddMessageError(ValidationRuleConfiguration.Messages.B2101GrossWeightUnitMessage);
			}
		}

		void CheckGrossWeightRuleB2101(NctsDepartureCargoDesc goodsItem)
		{
			if ((ValidationDecider?.IsRuleB2101Active ?? false)
				&& !goodsItem.IsInPhase5TransitionPeriod
				&& goodsItem.BY_GrossWeight.IsEmpty)
			{
				goodsItem.BY_GrossWeightInfo.AddMessageError(ValidationRuleConfiguration.Messages.B2101GrossWeightMessage);
			}
		}

		void ValidateTransportChargesMethodOfPaymentRuleB1875_1()
		{
			var parent = Parent;
			var header = parent.Header;
			if ((ValidationDecider?.IsRuleB1875_1Active ?? false)
				&& !parent.BY_TransportChargesMethodOfPayment.IsEmpty
				&& parent.IsInPhase5TransitionPeriod
				&& header.MovementHeader is NctsDepartureMovementHeader movementHeader
				&& (movementHeader.BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.NON || !movementHeader.BM_MethodOfPayment.IsEmpty))
			{
				var errorMessage = Res.GetString("92BF5DC9-3FFF-4ED1-AE86-9B07F12178D7", "{0} Transport Charges MoP must be empty.", ValidationRuleCodeConstants.B1875_1.GetRuleCodeMessagePrefix());
				parent.BY_TransportChargesMethodOfPaymentInfo.AddMessageError(errorMessage);
			}
		}

		protected virtual bool IsBY_CommercialReferenceNumberMandatory()
		{
			var parent = Parent;
			var bill = parent.Bill;

			return bill != null
					&& bill.B0_ReferenceID.IsEmpty
					&& (parent.MoveHeader?.BM_UniqueConsignmentReference.IsEmpty ?? true)
					&& !HasRelevantDocuments(bill.AdditionalDocuments)
					&& !HasRelevantDocuments(Header.AdditionalDocuments);

			bool HasRelevantDocuments(IEnumerable collection)
			{
				return collection.Cast<CusSupportingInfo>().Any(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument);
			}
		}

		internal IDisposable SuspendBY_CommercialReferenceNumberUniqueValidation()
		{
			return suspenderForBY_CommercialReferenceNumberUniqueValidation.Suspend();
		}

		readonly ActionSuspender suspenderForBY_CommercialReferenceNumberUniqueValidation = new ActionSuspender();

		void CheckBY_RN_NKCountryOfDestination_C0343Rule()
		{
			var parent = Parent;
			if (ValidationDecider.IsRuleC0343Active
				&& parent.BY_RN_NKCountryOfDestination.IsEmpty
				&& Header.MovementHeader.BM_RL_NKDestinationPort.IsEmpty)
			{
				Parent.BY_RN_NKCountryOfDestinationInfo.AddMessageError(Res.GetString("44393512-BAFF-4493-AB68-C4F2C12C3028", "[C0343] You have not entered Country of Destination. It is required either on Declaration or House Consignment Item."));
			}
		}

		void ValidatePackages_TR0001Rule()
		{
			var parent = Parent;
			if (Header.IsRuleActive(x => x.IsRuleTR0001Active) && parent.Packages.Count == 0)
			{
				parent.AddRowMessageError(Res.GetString("FC335075-D942-46EB-801D-E80A2C31F98D", "[TR0001] At least one Package Record is required."));
			}
		}

		void CheckRuleR0221_1(NctsDepartureCargoDesc goodsItem)
		{
			if ((ValidationDecider?.IsRuleR0221_1Active ?? false)
				&& goodsItem.BY_GrossWeight == 0
				&& goodsItem.Packages.Cast<NctsPackage>().Any(p => p.B5_UnitCount > 0))
			{
				var error = Res.GetString("4559c9e6-075d-4ab7-815e-870449cb135c", "[R0221-1] Package Count greater than zero requires also a Gross Weight greater than zero.");
				goodsItem.BY_GrossWeightInfo.AddMessageError(error);
			}
		}

		void CheckRuleR0221_3(NctsDepartureCargoDesc goodsItem)
		{
			if ((ValidationDecider?.IsRuleR0221_3Active ?? false)
				&& goodsItem.BY_GrossWeight > 0
				&& !goodsItem.Packages.Cast<NctsPackage>().Any(p => p.IsBulk)
				&& goodsItem.Packages.Cast<NctsPackage>().Sum(x => x.B5_UnitCount) == 0)
			{
				var error = Res.GetString("7CA6FFEB-A8B9-4809-A097-F819C835FD1D", "[R0221-3] Gross mass must be 0 if Pack Type is not bulk and Pack Qty is 0.");
				goodsItem.BY_GrossWeightInfo.AddMessageError(error);
			}
		}

		void CheckConditionR0020()
		{
			var parent = Parent;
			if (!parent.MoveHeader.IsDeparturePhase5RuleActive(x => x.IsRuleR0020Active))
			{
				return;
			}

			var isDecTypeT2OrT2F = NctsValidationHelper.IsEntryTypeT2OrT2F(parent.BY_Type);
			if ((isDecTypeT2OrT2F || NctsValidationHelper.IsEntryTypeT2OrT2F(parent.MoveHeader.BM_InBondEntryType))
				&& CheckOfficeOfDepartureAndPreviousDocuments())
			{
				var errorMessage = isDecTypeT2OrT2F
					? Res.GetString("EDE34462-9841-40D6-BC7A-498DEE4BEAF2", "[R0020] Previous Document of Type in CL178 is required either at Consignment level or for this Goods Item when Country of Customs Office of Departure is from the CL112 (Country Codes CTC) list and Declaration Type for this Goods Item is T2/T2F.")
					: NctsConstants.ValidationMessages.PreviousDocumentOfTypeCL178IsRequiredForCustomsOfficeOfDepartureInCL112AndDeclarationTypeT2OrT2F;

				parent.AddRowMessageError(errorMessage);
			}

			bool CheckOfficeOfDepartureAndPreviousDocuments() => Header.MovementHeader.CustomsOfficesForDeparture.HasCL112OfficeOfDepartureCountry(parent.Factory)
				&& HasNoCL178PreviousDocument(parent);
		}

		void CheckConditionR0020_1()
		{
			var parent = Parent;
			if (parent.MoveHeader.IsDeparturePhase5RuleActive(x => x.IsRuleR0020_1Active) &&
				NctsValidationHelper.IsEntryTypeT2OrT2F(parent.BY_Type) &&
				HasNoCL178PreviousDocument(parent))
			{
				parent.AddRowMessageError(Header.Configuration.ValidationRuleConfiguration.Messages.R0020_1aMessage);
			}
		}

		static bool HasNoCL178PreviousDocument(NctsDepartureCargoDesc parent) => !parent.Header.PreviousDocuments.HasCL178PreviousDocument(parent.Factory)
			&& !parent.PreviousDocuments.HasCL178PreviousDocument(parent.Factory);

		void CheckSupportingAndAdditionalDocumentsCountRuleE1407()
		{
			var parent = Parent;
			if ((ValidationDecider?.IsRuleE1407Active ?? false)
				&& parent.IsInPhase5TransitionPeriod
				&& ActualNumberOfDocumentsExceedsTotalAllowedCount(parent))
			{
				var errorMessage = Res.GetString("24733516-525E-40C5-B58E-D4369D829175", "[E1407] In transition period, which is now, the maximum cumulative number of Supporting Documents, Transport Document and Additional Reference must not exceed 99");
				parent.AddRowMessageError(errorMessage);
			}
		}

		void CheckBY_HarmonisedTariff_NR0024()
		{
			if ((ValidationDecider?.IsRuleNR0024Active ?? false) && Parent.BY_HarmonisedTariff.IsEmpty)
			{
				Parent.BY_HarmonisedTariffInfo.AddMessageError(Res.GetString("67296725-C6A4-4DCF-9540-BD5B0272D6B9", "[NR0024] This field must be filled"));
			}
		}

		void CheckBY_HarmonisedTariff_C0153_1Rule()
		{
			var parent = Parent;
			var header = Header;

			if ((ValidationDecider?.IsRuleC0153_1Active ?? false)
				&& header != null
				&& parent.BY_HarmonisedTariff.IsEmpty
				&& header.MovementHeader is NctsDepartureMovementHeader departureMovementHeader
				&& (!departureMovementHeader.IsTIRDeclaration || BillHasAnyPreviousDocumentWithKindN830()))
			{
				MandatoryValidation.MessageErrorIfNotEntered(
					parent.BY_HarmonisedTariffInfo,
					messagePrefix: ValidationRuleConfiguration.Messages.C0153_1RuleCode.GetRuleCodeMessagePrefix(true));
			}
		}

		void CheckBY_HarmonisedTariff_C0821_1Rule()
		{
			var parent = Parent;
			var header = Header;
			const int tariffCodeMaxLengthRuleC0821 = 6;

			if ((ValidationDecider?.IsRuleC0821_1Active ?? false)
				&& header.MovementHeader.CustomsOfficesForDeparture.HasCL112OfficeOfDepartureCountry(parent.Factory)
				&& parent.BY_FormattedHarmonisedTariff.KeepNumericCharacters().Length > tariffCodeMaxLengthRuleC0821)
			{
				parent.BY_FormattedHarmonisedTariffInfo.AddWarning(ValidationRuleConfiguration.Messages.C0821_1Message);
			}
		}

		bool ActualNumberOfDocumentsExceedsTotalAllowedCount(NctsDepartureCargoDesc parent)
		{
			var overallMaxNumberOfDocuments = 99;

			var enteredDocuments = parent.SupportingDocuments.Cast<CusSupportingInfo>()
				.Union(parent.AdditionalInfos.Where(x => x.IsAnAdditionalReference))
				.Union(parent.Bill?.AdditionalDocuments.Where(x => x.IsATransportDocument) ?? Enumerable.Empty<CusSupportingInfo>());
			return enteredDocuments.Skip(overallMaxNumberOfDocuments).Any();
		}

		void CheckBY_TransportChargesMethodOfPayment_B2400_1()
		{
			var parent = Parent;
			if ((ValidationDecider?.IsRuleB2400_1Active ?? false)
				&& !parent.IsInPhase5TransitionPeriod
				&& !parent.BY_TransportChargesMethodOfPayment.IsEmpty)
			{
				parent.BY_TransportChargesMethodOfPaymentInfo.AddMessageError(Res.GetString("BFE35AAD-9F48-4FA3-9E99-658002E55608", "[B2400-1] Transport MoP field must be empty"));
			}
		}

		void CheckBY_HarmonisedTariff_NR0025()
		{
			if (Parent.ValidationDecider is INctsDepartureCargoDescPhase5ValidationDecider validationDecider
				&& validationDecider.IsRuleNR0025Active
				&& Parent.BY_HarmonisedTariff.IsEmpty
				&& Parent.MoveHeader.Guarantees.Cast<CusBondDetail>().Any(x => x.PW_BondType == GuaranteeCodes.IndividualGuaranteeByGuarantor))
			{
				Parent.BY_HarmonisedTariffInfo.AddMessageError(Res.GetString("CC3A543E-52AB-46A8-99E3-E7E28D915526", "[NR0025] You have not entered a Commodity Code."));
			}
		}

		bool BillHasAnyPreviousDocumentWithKindN830()
		{
			var parent = Parent;
			var bill = parent.Bill;
			var previousDocumentsWithDocumentKindN830 = bill?.PreviousDocuments
				.Cast<CommonPreviousDocument>()
				.Any(x => x.CSI_Code == NctsConstants.NctsTypeOfPreviousDocument.Codes.N830) ?? false;

			return previousDocumentsWithDocumentKindN830;
		}

		void CheckBY_TypeRuleB1922(NctsDepartureCargoDesc goodsItem)
		{
			var declarationType = goodsItem.BY_Type;
			if (!declarationType.IsEmpty
				&& goodsItem.Header is NctsHeader nctsHeader
				&& (ValidationDecider?.IsRuleB1922Active ?? false)
				&& goodsItem.IsInPhase5TransitionPeriod)
			{
				var refCusCodeListCL234 = nctsHeader.GetCL234List();

				if (HasCL234AdditionalInfoWithSubTypeREF())
				{
					var hasPreviousDocumentN830 = HasPreviousDocumentWithTypeN830();
					if (hasPreviousDocumentN830
						&& declarationType != NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure)
					{
						goodsItem.BY_TypeInfo.AddMessageError(Res.GetString("854B1E2E-6BEC-41C4-A74D-C860E21AABE3", "[B1922] Declaration Type must be T1."));
						return;
					}

					if (!hasPreviousDocumentN830
						&& declarationType != NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure
						&& declarationType != NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedureBetweenDifferentFiscalTerritories)
					{
						goodsItem.BY_TypeInfo.AddMessageError(Res.GetString("06D64511-25E3-4B68-B3A2-6107F8DC86ED", "[B1922] Declaration Type must be T2 or T2F."));
					}
				}

				bool HasCL234AdditionalInfoWithSubTypeREF() => goodsItem.AdditionalInfos.Any(d => d.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference && refCusCodeListCL234.ContainsCode(d.CSI_Code));

				bool HasPreviousDocumentWithTypeN830() => goodsItem.PreviousDocuments.Any(d => d.CSI_Code == NctsConstants.NctsTypeOfPreviousDocument.Codes.N830);
			}
		}

		void CheckConditionRP11(NctsDepartureCargoDesc goodsItem, ZPropertyInfo info)
		{
			if (info.Value.IsEmpty && ExistsPreviousDocumentOfTypeN830() && (ValidationDecider?.IsRuleRP11Active ?? false))
			{
				MandatoryValidation.MessageErrorIfNotEntered(info, messagePrefix: ValidationRuleConfiguration.Messages.RP11RuleCode.GetRuleCodeMessagePrefix(true));
			}

			bool ExistsPreviousDocumentOfTypeN830()
			{
				return goodsItem.PreviousDocuments.Cast<NctsPreviousDocument>().Any(p => p.CSI_Code == NctsConstants.NctsTypeOfPreviousDocument.Codes.N830)
					|| (goodsItem.Bill?.PreviousDocuments.Cast<CommonPreviousDocument>().Any(p => p.CSI_Code == NctsConstants.NctsTypeOfPreviousDocument.Codes.N830) ?? false)
					|| (goodsItem.Bill?.Header.PreviousDocuments.Cast<CommonPreviousDocument>().Any(p => p.CSI_Code == NctsConstants.NctsTypeOfPreviousDocument.Codes.N830) ?? false);
			}
		}

		void CheckRuleDutyTR0096()
		{
			if (ValidationDecider?.IsRuleTR0096Active ?? false)
			{
				var hasDutyRecord = Parent.Fees.Any(f => f.BFE_ChargeType == NctsCommonCargoDesc.ChargeType.Duty);

				if (!hasDutyRecord && Header.GetEffectiveGuarantees().Any(guarantee => !guarantee.PW_Override))
				{
					Parent.AddRowMessageError(Header.Configuration.ValidationRuleConfiguration.Messages.TR0096Message);
				}
			}
		}

		NctsHeader Header => Parent.Header;

		ValidationRuleConfiguration ValidationRuleConfiguration => Header.Configuration.ValidationRuleConfiguration;

		INctsDepartureCargoDescPhase5ValidationDecider ValidationDecider => Parent.ValidationDecider as INctsDepartureCargoDescPhase5ValidationDecider;
	}
}
