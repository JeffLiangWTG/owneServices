using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using static Enterprise.Integration.Customs.CH;

namespace Enterprise.Customs.CH.NCTS.Business;

public class CusGoodsLocation : EU.NCTS.Business.CusGoodsLocation, INctsCusGoodsLocation
{
	public CusGoodsLocation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new CusGoodsLocationAddress Address => (CusGoodsLocationAddress)base.Address;

	protected override Type AddressType => typeof(CusGoodsLocationAddress);

	[ReadOnlyMember(nameof(HeaderIsDepartureOrArrivalMovement))]
	public override ZString CGL_Qualifier { get => base.CGL_Qualifier; set => base.CGL_Qualifier = value; }

	[ReadOnlyMember(nameof(HeaderIsDepartureOrArrivalMovement))]
	public override ZString CGL_Type { get => base.CGL_Type; set => base.CGL_Type = value; }

	bool HeaderIsDepartureMovement => Header.IsDepartureMovement;

	bool HeaderIsArrivalMovement => Header.IsArrivalMovement;

	bool HeaderIsDepartureOrArrivalMovement => HeaderIsDepartureMovement || HeaderIsArrivalMovement;

	public bool ParentIsIncident => Parent is EnRouteIncident;

	protected override void SetDefaultsForNew()
	{
		base.SetDefaultsForNew();
		if (HeaderIsDepartureOrArrivalMovement)
		{
			using (Address.SuspendSettingHasChanges())
			using (Address.GetValidationSuspender())
			{
				CGL_Type = CusGoodsLocationTypeList.Codes.ApprovedPlace;
				CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
				Address.E2_GovRegNumType = CH.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar;
				SetDefaultIdentificationHolder();
			}
		}
	}

	void SetDefaultIdentificationHolder()
	{
		if (HeaderIsDepartureMovement)
		{
			var representativeOrganizationPK = Header?.MovementHeader?.Representative?.OrganisationPK ?? ZGuid.Empty;
			Address.IdentificationHolderPK = representativeOrganizationPK.IsValid ? representativeOrganizationPK : (Header?.Principal?.OrganisationPK ?? ZGuid.Empty);
		}
		else if (HeaderIsArrivalMovement)
		{
			Address.IdentificationHolderPK = Header?.DestinationTrader?.OrganisationPK ?? ZGuid.Empty;
		}
	}

	public bool IsForActivationSending { get; set; }
}
