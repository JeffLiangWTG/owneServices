using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class MessageSendingDeclarationValidation : AutoMessageSendingDeclarationValidation
{
	public MessageSendingDeclarationValidation(AutoMessageSendingDeclaration parent) : base(parent)
	{
	}

	new MessageSendingDeclaration Parent => (MessageSendingDeclaration)base.Parent;

	PlausiValidation PlausiValidation => plausiValidation ??= new ExportPlausiValidation();
	PlausiValidation plausiValidation;

	protected override void CheckJE_DeclarationLanguage()
	{
		if (Parent.SendingObjectParent.IsExportAndAnyNC123)
		{
			base.CheckJE_DeclarationLanguage();
			MessageSendingDeclarationValidationHelper.CheckJE_DeclarationLanguage(Parent.JE_DeclarationLanguageInfo);
			NotificationHelper.TurnMessageErrorsIntoErrors(Parent.JE_DeclarationLanguageInfo);
		}
	}

	protected override void CheckJE_LocationOfGoods()
	{
		if (Parent.SendingObjectParent.IsExportAndAnyNC123)
		{
			base.CheckJE_LocationOfGoods();
			MessageSendingDeclarationValidationHelper.CheckJE_LocationOfGoods(Parent.JE_LocationOfGoodsInfo);
			NotificationHelper.TurnMessageErrorsIntoErrors(Parent.JE_LocationOfGoodsInfo);
		}
	}

	protected override void CheckJE_TransportMode()
	{
		if (Parent.SendingObjectParent.IsExportAndAnyNC123)
		{
			base.CheckJE_TransportMode();
			MessageSendingDeclarationValidationHelper.CheckJE_TransportMode(PlausiValidation, Parent.JE_TransportModeInfo, Parent);
			NotificationHelper.TurnMessageErrorsIntoErrors(Parent.JE_TransportModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JE_TransportModeInfo);
		}
	}

	protected override void CheckJE_TransportMeans()
	{
		if (Parent.SendingObjectParent.IsExportAndAnyNC123)
		{
			base.CheckJE_TransportMeans();
			MessageSendingDeclarationValidationHelper.CheckJE_TransportMode(PlausiValidation, Parent.JE_TransportModeInfo, Parent);
			MandatoryValidation.AddErrorIfNotEnteredAndOtherPropertyHasValue(Parent.JE_TransportMeansInfo, Parent.JE_TransportModeInfo, (ZString)TransportTypeList.Codes.OwnPropulsion);
			NotificationHelper.TurnMessageErrorsIntoErrors(Parent.JE_TransportMeansInfo);
		}
	}

	protected override void CheckJE_VesselName()
	{
		if (Parent.SendingObjectParent.IsExportAndAnyNC123)
		{
			base.CheckJE_VesselName();

			if (!Parent.JE_TransportMeans.IsEmpty)
			{
				MandatoryValidation.AddErrorIfNotEnteredAndOtherPropertyHasValue(Parent.JE_VesselNameInfo, Parent.JE_TransportModeInfo, (ZString)TransportTypeList.Codes.OwnPropulsion);
			}

			if (!Parent.JE_TransportMode.IsEmpty && !Parent.IsAir && !Parent.JE_RN_NKTransportNationality.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_VesselNameInfo);
			}
			NotificationHelper.TurnMessageErrorsIntoErrors(Parent.JE_VesselNameInfo);
		}
	}

	protected override void CheckJE_VoyageFlightNo()
	{
		if (Parent.SendingObjectParent.IsExportAndAnyNC123)
		{
			base.CheckJE_VoyageFlightNo();

			if (!Parent.JE_TransportMode.IsEmpty && Parent.IsAir && !Parent.JE_RN_NKTransportNationality.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_VoyageFlightNoInfo);
			}
			NotificationHelper.TurnMessageErrorsIntoErrors(Parent.JE_VoyageFlightNoInfo);
		}
	}

	protected override void CheckJE_RN_NKTransportNationality()
	{
		if (Parent.SendingObjectParent.IsExportAndAnyNC123)
		{
			base.CheckJE_RN_NKTransportNationality();
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_RN_NKTransportNationalityInfo);

			if (!Parent.JE_TransportMode.IsEmpty && ((!Parent.JE_VesselName.IsEmpty && !Parent.IsAir) || (!Parent.JE_VoyageFlightNo.IsEmpty && Parent.IsAir)))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RN_NKTransportNationalityInfo);
			}
			NotificationHelper.TurnMessageErrorsIntoErrors(Parent.JE_RN_NKTransportNationalityInfo);
		}
	}
}
