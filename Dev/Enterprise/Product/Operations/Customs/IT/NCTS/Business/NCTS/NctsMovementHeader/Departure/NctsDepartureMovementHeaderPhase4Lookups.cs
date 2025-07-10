using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsDepartureMovementHeaderPhase4Lookups : EU.NCTS.Business.NctsDepartureMovementHeaderPhase4Lookups, INctsDepartureMovementHeaderLookups
{
	public NctsDepartureMovementHeaderPhase4Lookups(NctsDepartureMovementHeader parent) : base(parent)
	{
	}

	public new NctsDepartureMovementHeader Parent => (NctsDepartureMovementHeader)base.Parent;

	public CodeDescriptionPairList PaymentPartyList => Factory.GetCachedValue<NctsPaymentPartyList>();

	public CodeDescriptionPairList DefermentApprovalNumberList => GetDefermentApprovalNumberList();

	public CodeDescriptionPairList CustomsChannelCodeList => Factory.GetCachedValue<CustomsChannelCodeList>();

	public CodeDescriptionPairList NctsParticipantTypeList => Factory.GetCachedValue<NctsParticipantTypeList>();

	public override CodeDescriptionPairList LocationOfGoodsCodeList => (goodsLocationProvider ?? (goodsLocationProvider = new NctsHeaderGoodsLocationListProvider(Parent.Header, Factory))).Locations;
	NctsHeaderGoodsLocationListProvider goodsLocationProvider;

	#region Implementation

	CodeDescriptionPairList GetDefermentApprovalNumberList()
	{
		var (organization, organizationTypeKeyIdentifier) = GetSourceOrganization();
		if (organization == null)
		{
			return new CodeDescriptionPairList();
		}
		return Factory.GetCachedValue(FormattableString.Invariant($"{organization.PK}_{organizationTypeKeyIdentifier}"), () => organization.GetDefermentApprovalNumberList());
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant identifier for consignor, consignee, declarant")]
	(OrgHeader Organization, ZString OrganizationTypeKeyIdentifier) GetSourceOrganization()
	{
		var header = Parent.Header;
		return header != null ? GetOrganization() : GetEmptyResult();

		(OrgHeader Organization, ZString OrganizationTypeKeyIdentifier) GetOrganization()
		{
			const string consignorKeyIdentifier = "Consignor";
			const string declarantKeyIdentifier = "Declarant";
			const string consigneeKeyIdentifier = "Consignee";

			if (Parent.PaymentParty == NctsPaymentPartyList.Codes.DeclarantsAccount)
			{
				return header.RepresentationType == RepresentationTypeList.Codes._1Self
					? (header.Consignor?.Address?.Header, consignorKeyIdentifier)
					: (header.DeclarantAddress?.Header, declarantKeyIdentifier);
			}
			else if (Parent.PaymentParty == NctsPaymentPartyList.Codes.ConsigneesAccount)
			{
				return (header.Consignee?.Address?.Header, consigneeKeyIdentifier);
			}
			return GetEmptyResult();
		}

		(OrgHeader Organization, ZString OrganizationTypeKeyIdentifier) GetEmptyResult() => (null, ZString.Empty);
	}

	#endregion
}
