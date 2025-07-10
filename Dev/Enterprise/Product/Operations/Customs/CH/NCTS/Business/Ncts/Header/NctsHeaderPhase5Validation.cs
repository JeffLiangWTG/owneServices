using CargoWise.EntityFramework;
using CoreConstants = Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsHeaderPhase5Validation : EU.NCTS.Business.NctsHeaderPhase5Validation
{
	public NctsHeaderPhase5Validation(NctsHeader parent) : base(parent)
	{
	}

	new NctsHeader Parent => (NctsHeader)base.Parent;

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateMovementReferenceNumber();
	}

	protected override void CheckBH_CommunicationLanguage()
	{
		base.CheckBH_CommunicationLanguage();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.BH_CommunicationLanguageInfo);
		if (Parent.IsArrivalMovement)
		{
			PassarValidation.CheckNP70180(Parent.BH_CommunicationLanguageInfo, Parent);
		}
	}

	public void ValidateMovementReferenceNumber()
	{
		ValidateCalculatedProperty(Parent.MovementReferenceNumberInfo);
	}

	protected void CheckMovementReferenceNumber()
	{
		if (Parent.IsDepartureMovement && Parent.MovementReferenceEntryNumber.CE_EntryStatus == CoreConstants.EntryStatusCodes.New)
		{
			Parent.MovementReferenceNumberInfo.AddWarning(Res.GetString("F83F3085-8034-457B-844C-54AC043EFB66", "The MRN version has changed because FOCBS has modified the transit movement. If you want to browse new transit movement data, please send NC016 message."));
		}
	}

	protected override void CheckArrivalMrnFromUserCore()
	{
	}

	protected override void CheckDestinationCustomsOfficeCodeForArrivalCore()
	{
	}

	protected override bool EitherDispatchCountryOnGoodsItemOrOnHeaderMustBeFilled => false;
}
