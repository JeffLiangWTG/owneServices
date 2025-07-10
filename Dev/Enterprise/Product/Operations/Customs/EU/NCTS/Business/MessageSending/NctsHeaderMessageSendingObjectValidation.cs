using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsHeaderMessageSendingObjectValidation : AutoNctsHeaderMessageSendingObjectValidation
	{
		public NctsHeaderMessageSendingObjectValidation(AutoNctsHeaderMessageSendingObject parent) : base(parent)
		{
		}

		protected new NctsHeaderMessageSendingObject Parent => (NctsHeaderMessageSendingObject)base.Parent;

		protected ValidationRuleConfiguration ValidationRuleConfiguration => Parent.NctsHeader.Configuration.ValidationRuleConfiguration;

		protected INctsHeaderMessageSendingObjectValidationDecider ValidationDecider => Parent.ValidationDecider;

		protected override void CheckMessageType()
		{
			base.CheckMessageType();

			var messageTypeInfo = Parent.MessageTypeInfo;

			MandatoryValidation.CheckEntered(messageTypeInfo);
			ListValidation.ErrorIfInvalidCode(messageTypeInfo);
			CheckCountryCodeSameAsCurrentCompany(messageTypeInfo);
		}

		protected override void CheckReleaseRequest()
		{
			base.CheckReleaseRequest();

			var parent = Parent;
			if (parent.MessageType.EqualsIgnoringCase(parent.ReleaseRequestCode))
			{
				MandatoryValidation.CheckEntered(parent.ReleaseRequestInfo);
				ListValidation.ErrorIfInvalidCode(parent.ReleaseRequestInfo);
			}
		}

		protected override void CheckActualOfficeOfDestination()
		{
			base.CheckActualOfficeOfDestination();
			CheckRuleTR0020(Parent.ActualOfficeOfDestinationInfo);
			CheckRuleTR0021ForOrg(Parent.ActualConsignee.OrganisationPK, Parent.ActualOfficeOfDestinationInfo);
			CheckRuleC0315(Parent.ActualOfficeOfDestinationInfo);
		}

		protected override void CheckAdditionalText()
		{
			base.CheckAdditionalText();
			var parent = Parent;

			if (ValidationDecider.IsRuleC0220Active
				&& !parent.TC11DeliveryDate.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.AdditionalTextInfo, messagePrefix: $"{ValidationRuleCodeConstants.C0220.GetRuleCodeMessagePrefix()} ");
			}
		}

		void CheckRuleTR0020(ZPropertyInfo actualOfficeOfDestinationInfo)
		{
			if (ValidationDecider.IsRuleTR0020Active)
			{
				var message = Res.GetString("0449CB64-468E-4A00-8892-47A7281605B4", "Selected Customs Office has not the role DES. Select another one");
				ListValidation.IfInvalidCode(NotificationType.MessageError, actualOfficeOfDestinationInfo, Parent.Lookups.DestinationCustomsOfficeCodeList, message);
			}
		}

		void CheckRuleTR0021ForOrg(ZGuid orgPk, ZPropertyInfo orgPkInfo)
		{
			if (ValidationDecider.IsRuleTR0021Active
				&& orgPk.IsEmpty
				&& Parent.ActualOfficeOfDestination.IsEmpty
				&& !Parent.QueryInformation.IsEmpty)
			{
				orgPkInfo.AddMessageError(NctsHeaderValidationHelper.TR0021ValidationMessage);
			}
		}

		void CheckRuleC0315(ZPropertyInfo actualOfficeOfDestinationInfo)
		{
			if (ValidationDecider.IsRuleC0315Active)
			{
				MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(actualOfficeOfDestinationInfo, Parent.TC11DeliveryDateInfo, Res.GetString("77a9b314-ffda-4a34-b7ec-4e97f23c74e8", "[C0315] You have not entered Customs Office Of Destination Actual."));
			}
		}

		void CheckCountryCodeSameAsCurrentCompany(ZPropertyInfo info)
		{
			var header = Parent.NctsHeader;

			var officeCountryCode = GetOfficeCountryCode(header);

			if (ValidationRuleConfiguration.IsCountryCodeRequiredToBeSameAsCurrentCompany
				&& officeCountryCode != Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(header.CountryCode)
				&& header.CountryCode != Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(officeCountryCode))
			{
				info.AddError(NctsConstants.ValidationMessages.CountryCodeDepartureOfficeShouldBeSameOfNCTSCountryCode);
			}
		}

		ZString GetOfficeCountryCode(NctsHeader header)
		{
			var result = ZString.Empty;

			if (header.MovementHeader is NctsDepartureMovementHeader departureMovementHeader)
			{
				result = header.IsPhase5 ? departureMovementHeader.DepartureCustomsOfficeCodeCountry : header.DepartureCustomsOfficeCodeCountry;
			}
			else if	(header.ArrivalMovementHeader is NctsArrivalMovementHeader arrivalMovementHeader)
			{
				result = header.IsPhase5 ? arrivalMovementHeader.DestinationCustomsOfficeCodeCountryForArrival : header.DestinationCustomsOfficeCodeCountryForArrival;
			}

			return result;
		}
	}
}
