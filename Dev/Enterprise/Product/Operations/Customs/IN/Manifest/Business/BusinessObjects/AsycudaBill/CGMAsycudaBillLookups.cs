using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.IN.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IN.Manifest.Business;

public sealed class CGMAsycudaBillLookups : AsycudaBillLookups
{
	public CGMAsycudaBillLookups(CGMAsycudaBill parent) : base(parent)
	{
	}

	public CodeDescriptionPairList SpecialCargoCodeList => Parent.IsSea ? Factory.GetCachedValue<ItemTypeList>() : Factory.GetCachedValue<ShipmentTypeList>();

	public CodeDescriptionPairList InlandTransportModeList => Factory.GetCachedValue<ModeOfTransportList>();

	public CodeDescriptionPairList NatureOfCargoList => Factory.GetCachedValue<NatureOfCargoList>();

	public CodeDescriptionPairList DestinationCodeList => Factory.GetCachedValue<DestinationCodeList>();

	public ICollection CustomsFinalDestinationPortList => Parent.FinalDestinationIsCustomsHouse ? UniversalReferenceDataHelper.GetCustomsOfficeCollection(Factory, isAir: false) : new CodeDescriptionPairList();

	public override CodeDescriptionPairList CustomsStatusList
	{
		get
		{
			var parent = Parent;
			var header = parent.Header;
			var billMessageStatus = parent.ABL_MessageStatus;
			var headerMessageStatus = header?.MessageStatus ?? ZString.Empty;
			var registrationStatus = header?.RegistrationStatus ?? ZString.Empty;

			return Factory.GetCachedValue($"IN.CGMAsycudaBillLookups|ActionList|{billMessageStatus}|{headerMessageStatus}|{registrationStatus}", () =>
			{
				var actionList = new CodeDescriptionPairList();

				if (billMessageStatus.IsEmpty)
				{
					if (headerMessageStatus.IsEmpty && registrationStatus.IsEmpty)
					{
						actionList.AddPair(BillActionList.Codes.Fresh, BillActionList.Descriptions.Fresh);
					}
					else if (headerMessageStatus == IN.Business.MessageStatusList.Codes.MessageAccepted
						&& registrationStatus == RegistrationStatusList.Codes.ManifestRegistered)
					{
						actionList.AddPair(BillActionList.Codes.Supplementary, BillActionList.Descriptions.Supplementary);
					}
					else
					{
						actionList = Factory.GetCachedValue<BillActionList>();
					}
				}
				else if (billMessageStatus == BillMessageStatusList.Codes.Accepted)
				{
					actionList.AddPair(BillActionList.Codes.Amendment, BillActionList.Descriptions.Amendment);
					actionList.AddPair(BillActionList.Codes.Delete, BillActionList.Descriptions.Delete);
				}
				else
				{
					actionList = Factory.GetCachedValue<BillActionList>();
				}

				return actionList;
			});
		}
	}

	protected override CodeDescriptionPairList CargoStatusListCore => Factory.GetCachedValue<CargoMovementList>();

	public override OrgHeaderCollection LocalTransportCarriers
	{
		get
		{
			var collection = base.LocalTransportCarriers;
			var organisationSecondaryType = Parent.ABL_InlandTransportMode.ToString() switch
			{
				ModeOfTransportList.Codes.Road => OrganisationSecondaryTypes.LocalTransport,
				ModeOfTransportList.Codes.Ship => OrganisationSecondaryTypes.ShippingLine,
				ModeOfTransportList.Codes.Train => OrganisationSecondaryTypes.Rail,
				_ => ZString.Empty,
			};

			if (!organisationSecondaryType.IsEmpty)
			{
				var filterBusinessObjectDefaults = collection.FilterBusinessObjectDefaults;
				filterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", organisationSecondaryType, false));
				filterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property4", ZBool.True, false));
				filterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "AndJoinCondition", ZBool.True, false));
			}

			return collection;
		}
	}

	public override CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<BillMessageStatusList>();

	new CGMAsycudaBill Parent => (CGMAsycudaBill)base.Parent;
}
