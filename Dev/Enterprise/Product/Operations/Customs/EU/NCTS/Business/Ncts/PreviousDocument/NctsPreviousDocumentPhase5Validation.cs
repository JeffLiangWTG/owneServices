using System;
using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsPreviousDocumentPhase5Validation : NctsPreviousDocumentValidation, IRuleG0321Checker
	{
		public NctsPreviousDocumentPhase5Validation(NctsPreviousDocument parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			Parent.ClearRowNotifications();
			CheckRowTR0030_1();
		}

		public bool CheckRuleG0321()
		{
			var parent = Parent;
			return IsPhase5RuleActive(parent, x => x.IsRuleG0321Active)
				&& parent.CSI_ReferenceNumber.IsEmpty
				&& !parent.CSI_Code.IsEmpty;
		}

		protected override void CheckCSI_ItemNumber()
		{
			base.CheckCSI_ItemNumber();

			var parent = Parent;
			CheckCSI_ItemNumberWithRefCusCodeAttribute(parent);
			CheckCSI_ItemNumberRuleG0058_1(parent);
			CheckCSI_ItemNumberRuleNR0066(parent);
		}

		protected virtual void CheckCSI_ItemNumberWithRefCusCodeAttribute(NctsPreviousDocument parent)
		{
			if (parent.RefCusCode.HasAttributeForMandatoryValidation(UniversalReferenceConstants.RefCusCodeListAttributeTypes.ItemNumber, RefCusCodeListLevelType.Item))
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_ItemNumberInfo, ItemNumberPropertyDescription);
			}
		}

		protected override void CheckCSI_ReferenceNumber2()
		{
			base.CheckCSI_ReferenceNumber2();
			CheckCSI_ReferenceNumber2MaxLength();
		}

		protected virtual void CheckCSI_ReferenceNumber2MaxLength()
		{
			var parent = Parent;
			UniversalValidationHelper.CheckMaxLengthIfPhase5TransitionPeriod(
				parent.IsInPhase5TransitionPeriod,
				parent.CSI_ReferenceNumber2Info,
				26,
				NctsConstants.ValidationRuleMessagePrefixes.E1117);
		}

		protected override void CheckCSI_UnitOfQuantity()
		{
			base.CheckCSI_UnitOfQuantity();

			var parent = Parent;
			if (parent.ValidationDecider is INctsPreviousDocumentDeparturePhase5ValidationDecider { IsRuleC0298Active: true }
				&& parent.CSI_UnitOfQuantity.IsEmpty
				&& parent.CSI_Quantity > 0)
			{
				parent.CSI_UnitOfQuantityInfo.AddMessageError(ValidationRuleConfiguration.Messages.C0298Message);
			}
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			var parent = Parent;
			var referenceNumber = parent.CSI_ReferenceNumber;
			var referenceNumberInfo = parent.CSI_ReferenceNumberInfo;
			var validationRuleConfiguration = ValidationRuleConfiguration;

			if (CheckRuleG0321())
			{
				referenceNumberInfo.AddWarning(validationRuleConfiguration.Messages.G0321Message);
			}

			if (parent.Parent != null
				&& !IsPhase5RuleActive(parent, x => x.IsRuleG0321Active)
				&& parent.RefCusCode.HasAttributeForMandatoryValidation(UniversalReferenceConstants.RefCusCodeListAttributeTypes.Reference, RefCusCodeListLevelType.Item))
			{
				MandatoryValidation.MessageErrorIfNotEntered(referenceNumberInfo);
			}

			if (IsPhase5DepartureRuleActive(parent, x => x.IsRuleNR0008Active)
				&& !referenceNumber.IsEmpty
				&& parent.CSI_Code == NctsConstants.NctsTypeOfPreviousDocument.Codes.N830)
			{
				var mrnFormatValidationError = NctsValidationHelper.CheckMRNFormat(referenceNumber, Parent.Factory, ZString.Empty);
				if (!mrnFormatValidationError.IsEmpty)
				{
					referenceNumberInfo.AddMessageError(Res.GetString("E7B80D40-5E87-4D9E-9B1B-C82E5C8AEBEC", "[NR0008]: {0}", mrnFormatValidationError));
				}
			}

			if (validationRuleConfiguration is not null)
			{
				new NctsPreviousDocumentPhase5RuleNR0046Validation(parent).ValidateReferenceNumber(validationRuleConfiguration.Messages, IsPhase5DepartureRuleActive(parent, x => x.IsRuleNR0046Active), referenceNumberInfo, referenceNumber);
			}
		}

		protected virtual ZString ItemNumberPropertyDescription => default;

		protected ValidationRuleConfiguration ValidationRuleConfiguration => Parent.GoodsItem?.Header?.Configuration.ValidationRuleConfiguration;

		void CheckCSI_ItemNumberRuleG0058_1(NctsPreviousDocument parent)
		{
			if (IsPhase5DepartureRuleActive(parent, x => x.IsRuleG0058_1Active)
				&& parent.CSI_ItemNumber <= ZInt.Zero
				&& parent.CSI_Code.In(previousDocumentTypesRequiresItemNumber)
				&& parent.GoodsItem is NctsDepartureCargoDesc departureGoodsItem)
			{
				parent.CSI_ItemNumberInfo.AddMessageError(Res.GetString("1C921A41-E1B6-4E2E-B915-78607644A76F", "{0} 'Goods Item Identifier' field must be filled with the Unique Body Reference (UBR).", ValidationRuleCodeConstants.G0058_1.GetRuleCodeMessagePrefix()));
			}
		}

		readonly ImmutableArray<ZString> previousDocumentTypesRequiresItemNumber = new ZString[]
		{
			NctsConstants.NctsTypeOfPreviousDocument.Codes.C651,
			NctsConstants.NctsTypeOfPreviousDocument.Codes.C658,
		}.ToImmutableArray();

		void CheckRowTR0030_1()
		{
			var parent = Parent;
			if (parent.IsNCTSPreviousDocument
				&& IsPhase5DepartureRuleActive(parent, x => x.IsRuleTR0030_1Active)
				&& parent.GoodsItem is NctsDepartureCargoDesc goodsItem
				&& goodsItem.Header is NctsHeader header
				&& header.IsInPhase5TransitionPeriod)
			{
				var bill = goodsItem.Bill;
				if (goodsItem.NCTSPreviousDocumentsCount + bill.NCTSPreviousDocumentsCount + header.NCTSPreviousDocumentsCount > 9)
				{
					parent.AddRowMessageError(ValidationRuleConfiguration.Messages.TR0030_1Message);
				}
			}
		}

		void CheckCSI_ItemNumberRuleNR0066(NctsPreviousDocument parent)
		{
			if (parent.GoodsItem is NctsDepartureCargoDesc departureGoodsItem
				&& parent.CSI_ItemNumber == ZInt.Zero
				&& parent.CSI_Code == NctsConstants.NctsTypeOfPreviousDocument.Codes.N830
				&& IsPhase5DepartureRuleActive(parent, x => x.IsRuleNR0066Active)
				&& parent.IsInPhase5TransitionPeriod)
			{
				parent.CSI_ItemNumberInfo.AddMessageError(ValidationRuleConfiguration.Messages.NR0066Message);
			}
		}

		bool IsPhase5DepartureRuleActive(NctsPreviousDocument info, Func<INctsPreviousDocumentDeparturePhase5ValidationDecider, bool> ruleCheck) =>
			info.ValidationDecider is INctsPreviousDocumentDeparturePhase5ValidationDecider phase5ValidationDecider && ruleCheck(phase5ValidationDecider);

		bool IsPhase5RuleActive(NctsPreviousDocument info, Func<INctsPreviousDocumentPhase5ValidationDecider, bool> ruleCheck) =>
			info.ValidationDecider is INctsPreviousDocumentPhase5ValidationDecider phase5ValidationDecider && ruleCheck(phase5ValidationDecider);
	}
}
