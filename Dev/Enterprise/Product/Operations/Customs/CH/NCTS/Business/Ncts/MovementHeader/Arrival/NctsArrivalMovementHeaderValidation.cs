using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsArrivalMovementHeaderValidation : EU.NCTS.Business.NctsArrivalMovementHeaderValidation
{
	public NctsArrivalMovementHeaderValidation(NctsArrivalMovementHeader parent) : base(parent)
	{
	}

	protected new NctsArrivalMovementHeader Parent => (NctsArrivalMovementHeader)base.Parent;

	public override void ValidateAll()
	{
		base.ValidateAll();
		PassarValidation.CheckNZ50023(Parent);
	}

	protected override void CheckBM_TransportAtArrivalType()
	{
		base.CheckBM_TransportAtArrivalType();

		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BM_TransportAtArrivalTypeInfo);
	}

	protected override void CheckBM_TransportAtArrivalID()
	{
		base.CheckBM_TransportAtArrivalType();

		MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_TransportAtArrivalIDInfo);
	}

	protected override void CheckBM_RN_NKTransportAtArrivalIDNationality()
	{
		base.CheckBM_RN_NKTransportAtArrivalIDNationality();

		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BM_RN_NKTransportAtArrivalIDNationalityInfo);
	}

	protected override void CheckBM_PaperlessInbondNum()
	{
		base.CheckBM_PaperlessInbondNum();

		if (Parent.ShouldGenerateLocalReferenceNumberOnFactorySaving)
		{
			if (Parent.ShouldGenerateLrnNumberByAuthorizedLocationCode)
			{
				if (CHCustomsDataRegistry.Instance.ArrivalCustomerReferenceFormat.Value.GetFormatByAuthorizationLocationCode(Parent.AuthorisationLocationCode) == null)
				{
					Parent.BM_PaperlessInbondNumInfo.AddMessageError(Res.GetString("461FF302-88E1-43EF-B458-33D7B5EF12AA", "No entry is found in the Registry for Customer Reference customization. Please check ‘NCTS Arrival Customer Reference Customization’ Registry Item."));
				}
			}
			else if (Parent.Header.DestinationTrader.Organisation.GetCHCustomsRegNo(OrgCusCode.SwissCodeTypes.BID).IsEmpty)
			{
				Parent.BM_PaperlessInbondNumInfo.AddMessageError(Res.GetString("4DDB33E3-8EE1-40A9-B800-5A5CDF8460EB", "Destination Trader must have a Business Partner ID (BID) in order to generate the Customer Reference."));
			}
		}
	}

	protected override void CheckDestinationCustomsOfficeCodeForArrivalCore()
	{
	}

	protected override void CheckBM_UnloadingDate()
	{
		base.CheckBM_UnloadingDate();
		PassarValidation.CheckNP70025(Parent.BM_UnloadingDateInfo, Parent);
	}
}
