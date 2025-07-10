using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class DepartureCusTransportMeansValidation : Customs.Business.CusTransportMeansValidation
	{
		public DepartureCusTransportMeansValidation(DepartureCusTransportMeans parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();

			ValidateRuleR0789_2();
		}

		protected override void CheckTPM_TypeOfIdentification()
		{
			base.CheckTPM_TypeOfIdentification();
			var parent = Parent;
			var targetInfo = parent.TPM_TypeOfIdentificationInfo;

			if (parent.MovementHeaderParent != null)
			{
				ListValidation.ErrorIfInvalidCode(targetInfo);

				if (IsRuleB2101Applicable)
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo, messagePrefix: ValidationRuleConfiguration.Messages.B2101RuleCode.GetRuleCodeMessagePrefix(true));
				}
			}
		}

		protected override void CheckTPM_IdentificationNumber()
		{
			base.CheckTPM_IdentificationNumber();

			var parent = Parent;
			var value = parent.TPM_IdentificationNumber;
			var targetInfo = parent.TPM_IdentificationNumberInfo;
			var bill = parent.Parent as NctsBill;
			if (value.IsEmpty)
			{
				if (!IsRuleB2101Applicable && !parent.TPM_TypeOfIdentification.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}
			}
			else
			{
				if (RequireTransportUpperCaseID)
				{
					UniversalValidationHelper.CheckNoLowerCaseLetters(targetInfo);
				}
			}

			if (bill != null)
			{
				if (parent.TPM_SequenceNumber >= 2 && bill.InlandTransportModeAtDeparture == ModeOfTransportList.Codes._2_RailTransport)
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}
			}

			if (IsRuleB2101Applicable)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo, messagePrefix: ValidationRuleConfiguration.Messages.B2101RuleCode.GetRuleCodeMessagePrefix(true));
			}
		}

		protected virtual bool RequireTransportUpperCaseID => TransportMeansValidationHelper.TransportTypesRequiringUpperCaseIDs.Contains(Parent.TPM_TypeOfIdentification);

		protected override void CheckTPM_RN_NKTransportNationality()
		{
			base.CheckTPM_RN_NKTransportNationality();

			var parent = Parent;
			var nctsHeader = parent.Header;
			var targetInfo = parent.TPM_RN_NKTransportNationalityInfo;

			ListValidation.MessageErrorIfInvalidCode(targetInfo);

			if (!IsRuleB2101Applicable
				&& (!parent.TPM_TypeOfIdentification.IsEmpty
				|| (parent.IsTransportBorder && !parent.TPM_IdentificationNumber.IsEmpty)))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(targetInfo);
			}

			if (IsRuleB2101Applicable)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo, messagePrefix: ValidationRuleConfiguration.Messages.B2101RuleCode.GetRuleCodeMessagePrefix(true));
			}

			if (parent.Parent is NctsBill bill
				&& parent.ValidationDecider is IDepartureCusTransportMeansPhase5ValidationDecider { IsRuleTR0078Active: true }
				&& !parent.IsInPhase5TransitionPeriod
				&& parent.TPM_SequenceNumber >= 2 && bill.InlandTransportModeAtDeparture == ModeOfTransportList.Codes._2_RailTransport)
			{
				MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(
					parent.WagonNationalityInfo,
					parent.WagonNumberInfo,
					nctsHeader.Configuration.ValidationRuleConfiguration.Messages.TR0078Message);
			}
		}

		protected override void CheckTPM_CustomsOffice()
		{
			base.CheckTPM_CustomsOffice();

			var parent = Parent;
			var customsOfficeInfo = parent.TPM_CustomsOfficeInfo;

			if (!IsRuleB2101Applicable
				&& parent.IsTransportBorder
				&& !parent.TPM_TypeOfIdentification.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(customsOfficeInfo);
			}

			if (IsRuleB2101Applicable)
			{
				MandatoryValidation.MessageErrorIfNotEntered(customsOfficeInfo, messagePrefix: ValidationRuleConfiguration.Messages.B2101RuleCode.GetRuleCodeMessagePrefix(true));
			}

			NctsDepartureMovementHeaderPhase5RuleG0789Validation.CheckCustomsOfficeAtBorderRuleG0789(customsOfficeInfo, parent.TPM_CustomsOffice, parent, ValidationRuleConfiguration.Messages.G0789_1Message);
		}

		protected override void CheckTPM_ParentTableCode()
		{
			base.CheckTPM_ParentTableCode();

			var parent = Parent;
			var movementHeaderParent = parent.MovementHeaderParent;
			if (movementHeaderParent != null)
			{
				if (movementHeaderParent.BM_ActiveBorderIdentificationType.IsEmpty)
				{
					parent.AddRowError(EnterFirstTransportInFieldsOfTransportBorderMessage);
				}
				else
				{
					parent.RemoveRowError(EnterFirstTransportInFieldsOfTransportBorderMessage);
				}
			}
		}

		protected override void CheckTPM_ReferenceNumber()
		{
			base.CheckTPM_ReferenceNumber();

			var parent = Parent;
			var movementHeaderParent = parent.MovementHeaderParent;
			if (movementHeaderParent != null)
			{
				TransportMeansValidationHelper.CheckReferenceNumber(movementHeaderParent, parent.TPM_TypeOfIdentification, parent.TPM_ReferenceNumberInfo);
			}
		}

		void ValidateRuleR0789_2()
		{
			var parent = Parent;

			if (parent.MovementHeaderParent is NctsDepartureMovementHeader departureMovementHeader
				&& !departureMovementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().Any(r => r.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit))
			{
				parent.AddRowMessageError(Res.GetString("A0FA09BD-EEAF-400A-94B9-770D7AE8A5AE", "[R0789-2] If a Customs Office of Transit is not present then no additional Active Border Transport Means are allowed"));
			}
		}

		new DepartureCusTransportMeans Parent => (DepartureCusTransportMeans)base.Parent;

		ValidationRuleConfiguration ValidationRuleConfiguration => Parent.Header?.Configuration.ValidationRuleConfiguration;

		bool IsRuleB2101Applicable => Parent.ValidationDecider is IDepartureCusTransportMeansPhase5ValidationDecider { IsRuleB2101Active: true } && !Parent.IsInPhase5TransitionPeriod;

		string EnterFirstTransportInFieldsOfTransportBorderMessage => Res.GetString("FECC1C68-E94E-4A06-86A9-4F627880FEF5", "Enter the first Transport in the fields of Transport Border");
	}
}
