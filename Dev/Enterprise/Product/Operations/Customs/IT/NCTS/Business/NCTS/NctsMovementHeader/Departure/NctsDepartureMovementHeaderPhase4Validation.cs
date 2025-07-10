using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using UniversalReferenceConstants = Enterprise.Customs.IT.Business.UniversalReferenceConstants;
using ValidationCaptions = Enterprise.Customs.IT.Business.ValidationCaptions;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsDepartureMovementHeaderPhase4Validation : EU.NCTS.Business.NctsDepartureMovementHeaderPhase4Validation
{
	public NctsDepartureMovementHeaderPhase4Validation(NctsDepartureMovementHeader parent) : base(parent)
	{
	}

	protected new NctsDepartureMovementHeader Parent => (NctsDepartureMovementHeader)base.Parent;

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidatePaymentParty();
		ValidateDefermentAccountNumber();
	}

	#region GoodsLocation

	protected override void CheckBM_LocationOfGoodsCode()
	{
		base.CheckBM_LocationOfGoodsCode();

		if (Parent.Header != null && !Parent.Header.Authorization.IsEmpty)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BM_LocationOfGoodsCodeInfo);
		}
	}

	#endregion

	#region BM_AdditionalText

	protected override void CheckBM_AdditionalText()
	{
		base.CheckBM_AdditionalText();

		ValidateRuleR876();
	}

	protected virtual void ValidateRuleR876()
	{
		new CommercialReferenceAdditionalTextValidator(Parent).ValidateAdditionalInfo();
	}

	#endregion

	#region PaymentParty

	public void ValidatePaymentParty()
	{
		ValidateCalculatedProperty(Parent.PaymentPartyInfo);
	}

	protected void CheckPaymentParty()
	{
		ListValidation.MessageErrorIfInvalidCode(Parent.PaymentPartyInfo);
	}

	#endregion

	#region DefermentAccountNumber

	public void ValidateDefermentAccountNumber()
	{
		ValidateCalculatedProperty(Parent.DefermentAccountNumberInfo);
	}

	protected void CheckDefermentAccountNumber()
	{
		var defermentAccountNumberInfo = Parent.DefermentAccountNumberInfo;
		ListValidation.MessageErrorIfInvalidCode(defermentAccountNumberInfo);
		ValidationHelper.ValidateDefermentAccountNumber((ZPropertyInfoString)defermentAccountNumberInfo);
		CheckConditionC558();
	}

	void CheckConditionC558()
	{
		var existsMethodOfPaymentEFG = Parent.GoodsItems
			.Cast<NctsDepartureCargoDesc>()
			.SelectMany(goodsItem => goodsItem.Fees)
			.Cast<NctsCargoDescFee>()
			.Any(fee => IsDeferredPayment(fee.BFE_MethodOfPayment));

		var defermentAccountNumberInfo = Parent.DefermentAccountNumberInfo;
		var isDefermentAccountNumberEmpty = Parent.DefermentAccountNumber.IsEmpty;

		if (isDefermentAccountNumberEmpty && existsMethodOfPaymentEFG)
		{
			defermentAccountNumberInfo.AddMessageError(ValidationCaptions.NctsDepartureMovementHeader.ApprovalDeferNoMustBeFilled);
		}
		else if (!isDefermentAccountNumberEmpty && !existsMethodOfPaymentEFG)
		{
			defermentAccountNumberInfo.AddMessageError(ValidationCaptions.NctsDepartureMovementHeader.ApprovalDeferNoMustBeEmpty);
		}
	}

	bool IsDeferredPayment(ZString methodOfPayment)
	{
		switch (methodOfPayment)
		{
			case UniversalReferenceConstants.DutyMethodOfPayment.DeferredPaymentE:
			case UniversalReferenceConstants.DutyMethodOfPayment.DeferredPaymentCustomsProcedureF:
			case UniversalReferenceConstants.DutyMethodOfPayment.DeferredPaymentVatProcedureG:
				return true;
			default:
				return false;
		}
	}

	#endregion

	#region BM_RL_NKDestinationPort

	protected override void CheckBM_RL_NKDestinationPort()
	{
		base.CheckBM_RL_NKDestinationPort();
		new DestinationPortValidator(Parent).CheckDestinationPort();
	}

	#endregion

	#region BM_ControlChannel

	protected override void CheckBM_ControlChannel()
	{
		base.CheckBM_ControlChannel();
		ListValidation.ErrorIfInvalidCode(Parent.BM_ControlChannelInfo);
	}

	#endregion

	#region BM_PlaceOfLoading

	protected override void CheckBM_PlaceOfLoading()
	{
		base.CheckBM_PlaceOfLoading();
		CheckConditionC191(Parent.Header, Parent.BM_PlaceOfLoadingInfo);
	}

	#endregion

	#region BM_PlaceOfUnloading

	protected override bool ShouldListValidatePlaceOfUnloading => false;

	#endregion

	#region BM_ExportTransportMode

	protected override void CheckBM_ExportTransportMode()
	{
		base.CheckBM_ExportTransportMode();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_ExportTransportModeInfo);
	}

	#endregion

	#region BM_ExportDate

	protected override void CheckBM_ExportDate()
	{
		base.CheckBM_ExportDate();
		var exportDate = Parent.BM_ExportDate.Date;
		if (exportDate.IsValid && Parent.BM_CustomsStatus.IsEmpty)
		{
			var today = ZDate.Today;
			if (exportDate < today)
			{
				Parent.BM_ExportDateInfo.AddMessageError(ValidationCaptions.NctsDepartureMovementHeader.DateLimitCannotBeInThePast);
			}
			else if (exportDate.IsInRangeExcludingBounds(today, today.AddDays(NctsDepartureMovementHeader.ExportDateDaysGap)))
			{
				Parent.BM_ExportDateInfo.AddWarning(ValidationCaptions.NctsDepartureMovementHeader.DateLimitIsLessThan8DaysFromNow);
			}
		}
	}

	protected override ZBool IsExportDateMandatory => true;

	#endregion

	#region DestinationPortValidator

	class DestinationPortValidator
	{
		public DestinationPortValidator(NctsDepartureMovementHeader header)
		{
			movementHeader = header;
		}

		readonly NctsDepartureMovementHeader movementHeader;

		public void CheckDestinationPort()
		{
			var messageError = CheckDestinationPort(movementHeader.GoodsItems.Cast<NctsDepartureCargoDesc>(), movementHeader.BM_RL_NKDestinationPort);
			AddMessageErrorIfNotEmpty(messageError);
		}

		ZString CheckDestinationPort(IEnumerable<NctsDepartureCargoDesc> goodItems, ZString destintaionPort)
		{
			if (goodItems.Any(x => x.MoveHeader.GoodsItems.AtLeastOneOfGoodItemsCountryOfDestinationIsFilledButNotAllOfThem()))
			{
				return ZString.Empty;
			}

			if (IsCountryOfDestinationEmptyBothHeaderAndGoodItems())
			{
				return ValidationCaptions.NctsDepartureMovementHeader.NotEnteredDestinationCountry;
			}

			if (!IsCountryOfDestinationEqualBetweenHeaderAndGoodItems())
			{
				if (!IsCountryOfDestinationHeaderOrGoodItemsEmpty())
				{
					return ValidationCaptions.NctsDepartureMovementHeader.DestinationCountryDeclarationDifferentGoodItems;
				}
			}

			return ZString.Empty;

			bool IsCountryOfDestinationEmptyBothHeaderAndGoodItems() => destintaionPort.IsEmpty && goodItems.All(x => x.BY_RN_NKCountryOfDestination.IsEmpty);
			bool IsCountryOfDestinationHeaderOrGoodItemsEmpty() => destintaionPort.IsEmpty || goodItems.All(x => x.BY_RN_NKCountryOfDestination.IsEmpty);
			bool IsCountryOfDestinationEqualBetweenHeaderAndGoodItems() => !destintaionPort.IsEmpty && goodItems.Any(x => x.BY_RN_NKCountryOfDestination == destintaionPort);
		}

		void AddMessageErrorIfNotEmpty(ZString messageError)
		{
			if (!messageError.IsEmpty)
			{
				movementHeader.BM_RL_NKDestinationPortInfo.AddMessageError(messageError);
			}
		}
	}

	#endregion

	#region ParticipantType

	public void ValidateParticipantType()
	{
		ValidateCalculatedProperty(Parent.ParticipantTypeInfo);
	}

	protected void CheckParticipantType()
	{
		ListValidation.ErrorIfInvalidCode(Parent.ParticipantTypeInfo);
		MandatoryValidation.CheckEntered(Parent.ParticipantTypeInfo);
	}

	#endregion

	#region ConditionC010

	protected override bool ShouldCheckConditionC010 => !Parent.IsTIRDeclaration;

	#endregion

	#region CheckTirCarnetExpiryDateMandatory

	protected override void CheckTirCarnetExpiryDateMandatory() { }

	#endregion

	protected override void CheckMandatoryGuaranteeIfNeeded(EU.NCTS.Business.NctsHeader nctsHeader, ZPropertyInfo info)
	{
		if (!Parent.IsTIRDeclaration)
		{
			base.CheckMandatoryGuaranteeIfNeeded(nctsHeader, info);
		}
	}
}
