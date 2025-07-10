using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsHeaderDepartureMessageSendingObjectValidation : NctsHeaderCommonMessageSendingObjectValidation
{
	public NctsHeaderDepartureMessageSendingObjectValidation(NctsHeaderCommonMessageSendingObject parent) : base(parent)
	{
	}

	new NctsHeaderDepartureMessageSendingObject Parent => (NctsHeaderDepartureMessageSendingObject)base.Parent;

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateGoodsLocationDescription();
	}

	protected override void CheckReasonCode()
	{
		base.CheckReasonCode();

		ListValidation.MessageErrorIfInvalidCode(Parent.ReasonCodeInfo);

		if (Parent.IsNT141)
		{
			MandatoryValidation.CheckEntered(Parent.ReasonCodeInfo);
		}

		PassarValidation.CheckNZ50016(Parent.ReasonCodeInfo, Parent);
	}

	protected override void CheckReasonText()
	{
		base.CheckReasonText();
		PassarValidation.CheckNS30035(Parent.ReasonTextInfo, Parent);
		PassarValidation.CheckNS30093(Parent.ReasonTextInfo, Parent);
		PassarValidation.CheckNS30094(Parent.ReasonTextInfo, Parent);
	}

	protected override void CheckActualDestinationCustomsOffice()
	{
		base.CheckActualDestinationCustomsOffice();

		var targetInfo = Parent.ActualDestinationCustomsOfficeInfo;
		PassarValidation.CheckNS30035(targetInfo, Parent);

		if (Parent.IsNT141)
		{
			ListValidation.ErrorIfInvalidCode(ResString.GetMultilingualString("E2F7BE38-6F6D-424F-8BD0-2C5B875DCF76", "Invalid Destination Customs Office"), targetInfo);
		}
	}

	protected override void CheckDoubleEntryMRN()
	{
		base.CheckDoubleEntryMRN();

		var targetInfo = Parent.DoubleEntryMRNInfo;
		PassarValidation.CheckNS30122(targetInfo, Parent);

		if (Parent.IsNT141)
		{
			CHNCTSValidationHelper.CheckMRN(Parent.Factory, targetInfo, CargoWise.EntityFramework.NotificationType.Error);
		}
	}

	protected override void CheckCommunicationLanguage()
	{
		base.CheckCommunicationLanguage();

		if (Parent.IsNC123)
		{
			ListValidation.ErrorIfInvalidCodeOrEmpty(Parent.CommunicationLanguageInfo);
		}
	}

	protected override void CheckInlandTransportModeAtDeparture()
	{
		base.CheckInlandTransportModeAtDeparture();

		if (Parent.IsNC123)
		{
			ListValidation.ErrorIfInvalidCodeOrEmpty(Parent.InlandTransportModeAtDepartureInfo);
		}
	}

	protected override void CheckTransportAtDeparture()
	{
		base.CheckTransportAtDeparture();

		var parent = Parent;

		if (parent.IsNC123 && parent.TransportAtDeparture.IsEmpty)
		{
			switch (parent.InlandTransportModeAtDeparture)
			{
				case ModeOfTransportList.Codes._3_RoadTransport:
				case ModeOfTransportList.Codes._9_OwnPropulsion:
					CheckEntered();
					break;
				case ModeOfTransportList.Codes._2_RailTransport:
					CheckEntered(Res.GetString("4D4141C7-41FC-4FC4-9E6A-5CFD0ADD597B", "Train Number"));
					break;
				case ModeOfTransportList.Codes._8_InlandWaterwayTransport:
					CheckEntered(Res.GetString("AEB489CE-A1B5-45B3-BEA2-57B61E8EFE14", "Vessel"));
					break;
				case ModeOfTransportList.Codes._4_AirTransport:
					if (parent.AircraftIDAtDeparture.IsEmpty)
					{
						CheckEntered(Res.GetString("DFB35F9D-CB02-40DB-A249-EDA0C899E2C8", "Registration Number"));
					}
					break;
			}
		}

		void CheckEntered(string propertyDescription = "")
		{
			MandatoryValidation.CheckEntered(parent.TransportAtDepartureInfo, propertyDescription, errorNotificationPrefix: PassarValidationMessages.NS30106.GetRuleCodeMessagePrefix(true));
		}
	}

	protected override void CheckAircraftIDAtDeparture()
	{
		base.CheckAircraftIDAtDeparture();
		var parent = Parent;

		if (parent.IsNC123
			&& parent.InlandTransportModeAtDeparture == ModeOfTransportList.Codes._4_AirTransport
			&& parent.TransportAtDeparture.IsEmpty)
		{
			MandatoryValidation.CheckEntered(parent.AircraftIDAtDepartureInfo, Res.GetString("B79B20CF-FDAE-4C93-A926-AC46854B5290", "Aircraft Identification"), errorNotificationPrefix: PassarValidationMessages.NS30106.GetRuleCodeMessagePrefix(true));
		}
	}

	protected override void CheckTransportCountryAtDeparture()
	{
		base.CheckTransportCountryAtDeparture();

		var parent = Parent;

		if (parent.IsNC123 && parent.InlandTransportModeAtDeparture != ModeOfTransportList.Codes._7_FixedTransportInstallations)
		{
			var propertyInfo = parent.TransportCountryAtDepartureInfo;
			AddErrorIfEmpty(propertyInfo, PassarValidationMessages.NS30106);
			ListValidation.IfInvalidCode(NotificationType.Error, propertyInfo, parent.MovementHeader.Lookups.TransportNationalityList, InvalidCodeMessage(propertyInfo, PassarValidationMessages.NS30000));
		}
	}
	protected override void CheckTransportTypeAtDeparture()
	{
		base.CheckTransportTypeAtDeparture();
		var parent = Parent;

		if (parent.IsNC123 && parent.InlandTransportModeAtDeparture == ModeOfTransportList.Codes._9_OwnPropulsion)
		{
			var propertyInfo = parent.TransportTypeAtDepartureInfo;
			AddErrorIfEmpty(propertyInfo, PassarValidationMessages.NS30106);
			ListValidation.IfInvalidCode(NotificationType.Error, propertyInfo, parent.MovementHeader.Lookups.TransportAtDepartureTypeOfIdList, InvalidCodeMessage(propertyInfo, PassarValidationMessages.NS30000));
		}
	}

	void ValidateGoodsLocationDescription() => ValidateCalculatedProperty(Parent.GoodsLocationDescriptionInfo);

	protected void CheckGoodsLocationDescription()
	{
		if (Parent.IsNC123)
		{
			CusGoodsLocationValidationHelper.ValidateInnerGoodsLocation(Parent);
		}
	}

	static void AddErrorIfEmpty(ZPropertyInfo propertyInfo, string messagePrefix)
	{
		if (propertyInfo.Value.IsEmpty)
		{
			propertyInfo.AddError(messagePrefix.GetRuleCodeMessagePrefix(true) + MandatoryValidation.MustBeEnteredMessage(propertyInfo.HumanReadableName));
		}
	}
	static string InvalidCodeMessage(ZPropertyInfo propertyInfo, string messagePrefix)
		=> Res.GetString("32FC5E4C-5BCD-45F8-B9EB-0DC16CD85CAB", "{0} Enter a valid {1}.", messagePrefix.GetRuleCodeMessagePrefix(), propertyInfo.HumanReadableName);
}
