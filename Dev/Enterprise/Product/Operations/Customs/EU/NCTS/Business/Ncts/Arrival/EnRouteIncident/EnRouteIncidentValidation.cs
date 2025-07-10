using System.Collections.Immutable;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class EnRouteIncidentValidation : CusInBondEventValidation
	{
		public EnRouteIncidentValidation(EnRouteIncident parent) : base(parent)
		{
		}

		protected new EnRouteIncident Parent => (EnRouteIncident)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateGoodsLocationDescription();
		}

		protected void ValidateGoodsLocationDescription()
		{
			ValidateCalculatedProperty(Parent.GoodsLocationDescriptionInfo);
		}

		protected void CheckGoodsLocationDescription()
		{
			if (ValidationEnabled)
			{
				CheckGoodsLocationDescriptionCore();
			}
		}

		protected virtual void CheckGoodsLocationDescriptionCore()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.GoodsLocationDescriptionInfo);
		}

		protected sealed override void CheckBN_Information()
		{
			if (ValidationEnabled)
			{
				CheckBN_InformationCore();
			}
		}

		protected virtual void CheckBN_InformationCore()
		{
			base.CheckBN_Information();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BN_InformationInfo);
		}

		protected sealed override void CheckBN_EventCountryCode()
		{
			if (ValidationEnabled)
			{
				CheckBN_EventCountryCodeCore();
			}
		}

		protected virtual void CheckBN_EventCountryCodeCore()
		{
			base.CheckBN_EventCountryCode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BN_EventCountryCodeInfo);
		}

		protected override void CheckBN_EventPlace()
		{
			base.CheckBN_EventPlace();
			if (!Parent.IsPhase5)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BN_EventPlaceInfo);
			}
		}

		protected sealed override void CheckBN_EndorsementCountryCode()
		{
			if (ValidationEnabled)
			{
				CheckBN_EndorsementCountryCodeCore();
			}
		}

		protected virtual void CheckBN_EndorsementCountryCodeCore()
		{
			base.CheckBN_EndorsementCountryCode();

			var parent = Parent;
			ListValidation.MessageErrorIfInvalidCode(Parent.BN_EndorsementCountryCodeInfo);

			var header = parent.Header;
			if (header.IsPhase5 && (ValidationDecider?.IsRuleTR0015Active ?? false) &&
				(!parent.BN_EndorsementDate.IsEmpty || !parent.BN_EndorsementAuthority.IsEmpty || !parent.BN_EndorsementPlace.IsEmpty))
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.BN_EndorsementCountryCodeInfo, messagePrefix: "[TR0015] ");
			}
		}

		protected sealed override void CheckBN_EndorsementPlace()
		{
			if (ValidationEnabled)
			{
				CheckBN_EndorsementPlaceCore();
			}
		}

		protected virtual void CheckBN_EndorsementPlaceCore()
		{
			base.CheckBN_EndorsementPlace();

			var parent = Parent;
			var header = parent.Header;
			if (header.IsPhase5 && (ValidationDecider?.IsRuleTR0014Active ?? false) &&
				(!parent.BN_EndorsementDate.IsEmpty || !parent.BN_EndorsementAuthority.IsEmpty || !parent.BN_EndorsementCountryCode.IsEmpty))
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.BN_EndorsementPlaceInfo, messagePrefix: "[TR0014] ");
			}
		}

		protected sealed override void CheckBN_TransportAtDepartureType()
		{
			if (ValidationEnabled)
			{
				CheckBN_TransportAtDepartureTypeCore();
			}
		}

		protected virtual void CheckBN_TransportAtDepartureTypeCore()
		{
			base.CheckBN_TransportAtDepartureType();

			var parent = Parent;
			var propertyInfo = parent.BN_TransportAtDepartureTypeInfo;
			var propertyValue = propertyInfo.Value;
			var incidentCode = Parent.BN_IncidentCode;

			if ((ValidationDecider?.IsRuleC0240_1Active ?? false) && propertyValue.IsEmpty && IncidentCodesRequiringTransportMeans.Contains(incidentCode))
			{
				propertyInfo.AddMessageError(Res.GetString("03C6063B-01AE-41D0-BBF4-73B7402EBFAD", "[C0240-1] You have not entered a Type of ID."));
			}
		}

		protected sealed override void CheckBN_TransportAtDepartureID()
		{
			if (ValidationEnabled)
			{
				CheckBN_TransportAtDepartureIDCore();
			}
		}
		protected virtual void CheckBN_TransportAtDepartureIDCore()
		{
			base.CheckBN_TransportAtDepartureID();

			var parent = Parent;
			var propertyInfo = parent.BN_TransportAtDepartureIDInfo;
			var propertyValue = propertyInfo.Value;
			if ((ValidationDecider?.IsRuleC0240_1Active ?? false) && propertyValue.IsEmpty && !parent.BN_TransportAtDepartureType.IsEmpty)
			{
				propertyInfo.AddMessageError(Res.GetString("606A7A9B-E34C-4575-AF08-A47996A613B8", "[C0240-1] You have not entered an Identification Number."));
			}

			UniversalValidationHelper.CheckMaxLengthIfPhase5TransitionPeriod(parent.Header?.IsInPhase5TransitionPeriod ?? false, propertyInfo, 27);
		}

		protected sealed override void CheckBN_RN_NKTransportAtDepartureIDNationality()
		{
			if (ValidationEnabled)
			{
				CheckBN_RN_NKTransportAtDepartureIDNationalityCore();
			}
		}

		protected virtual void CheckBN_RN_NKTransportAtDepartureIDNationalityCore()
		{
			base.CheckBN_RN_NKTransportAtDepartureIDNationality();

			var parent = Parent;
			var propertyInfo = parent.BN_RN_NKTransportAtDepartureIDNationalityInfo;
			var propertyValue = propertyInfo.Value;
			if ((ValidationDecider?.IsRuleC0240_1Active ?? false) && propertyValue.IsEmpty && !parent.BN_TransportAtDepartureType.IsEmpty)
			{
				propertyInfo.AddMessageError(Res.GetString("833C5913-EBD1-4FBA-80E3-BAA1F5E2CC17", "[C0240-1] You have not entered a Nationality."));
			}
		}

		protected sealed override void CheckBN_IncidentCode()
		{
			if (ValidationEnabled)
			{
				CheckBN_IncidentCodeCore();
			}
		}

		protected virtual void CheckBN_IncidentCodeCore()
		{
			base.CheckBN_IncidentCode();

			var parent = Parent;
			var targetInfo = parent.BN_IncidentCodeInfo;
			var incidentCode = parent.BN_IncidentCode;
			var header = parent.Header;
			if (header.IsPhase5 && (ValidationDecider?.IsRuleTR0010Active ?? false))
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo, messagePrefix: "[TR0010] ");
			}

			ListValidation.ErrorIfInvalidCode(
				ResString.GetMultilingualString("DD6220B2-11B5-4971-8DB4-97E0968E6415", "The Incident Code you have selected is not in the list: {0}.", Parent.Lookups.IncidentCodeList.CodesAsString),
				targetInfo);

			if (ValidationRuleConfiguration.IsRuleC0240_3Active && parent.IncidentContainers.Count == 0 && IncidentCodesRequiringContainer.Contains(incidentCode))
			{
				targetInfo.AddMessageError(Res.GetString("7B5FD706-EB78-4742-8108-10835FB58D7C", "[C0240-3] You have not captured any Containers/Equipment."));
			}
		}

		protected sealed override void CheckBN_EndorsementDate()
		{
			if (ValidationEnabled)
			{
				CheckBN_EndorsementDateCore();
			}
		}

		protected virtual void CheckBN_EndorsementDateCore()
		{
			base.CheckBN_EndorsementDate();
			var parent = Parent;
			var header = parent.Header;
			if (header.IsPhase5 && (ValidationDecider?.IsRuleTR0012Active ?? false) &&
				(!parent.BN_EndorsementAuthority.IsEmpty || !parent.BN_EndorsementPlace.IsEmpty || !parent.BN_EndorsementCountryCode.IsEmpty))
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.BN_EndorsementDateInfo, Res.GetString("1E856A60-0133-4877-B16B-B7EB52885D1E", "Date of Endorsement"), "[TR0012] ");
			}
		}

		protected sealed override void CheckBN_EndorsementAuthority()
		{
			if (ValidationEnabled)
			{
				CheckBN_EndorsementAuthorityCore();
			}
		}

		protected virtual void CheckBN_EndorsementAuthorityCore()
		{
			base.CheckBN_EndorsementAuthority();
			var parent = Parent;
			var header = parent.Header;
			if (header.IsPhase5 && (ValidationDecider?.IsRuleTR0013Active ?? false)
				&& (!parent.BN_EndorsementDate.IsEmpty || !parent.BN_EndorsementPlace.IsEmpty || !parent.BN_EndorsementCountryCode.IsEmpty))
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.BN_EndorsementAuthorityInfo, Res.GetString("8DA61647-A02D-46CD-BB92-99F0F3AFAD15", "Authority of Endorsement"), "[TR0013] ");
			}
		}

		protected virtual bool ValidationEnabled => !(Parent.Header is NctsHeader header) || !header.IsArrivalDetailsReadOnly;

		static readonly ImmutableHashSet<string> IncidentCodesRequiringTransportMeans = ImmutableHashSet.Create(
			CusInBondEventIncidentCodeList.Codes._3,
			CusInBondEventIncidentCodeList.Codes._6
		);

		static readonly ImmutableHashSet<string> IncidentCodesRequiringContainer = ImmutableHashSet.Create(
			CusInBondEventIncidentCodeList.Codes._2,
			CusInBondEventIncidentCodeList.Codes._4
		);

		ValidationRuleConfiguration ValidationRuleConfiguration => Parent.Header.Configuration.ValidationRuleConfiguration;

		IEnRouteIncidentValidationDecider ValidationDecider => Parent.EnRouteIncidentValidationDecider;
	}
}
