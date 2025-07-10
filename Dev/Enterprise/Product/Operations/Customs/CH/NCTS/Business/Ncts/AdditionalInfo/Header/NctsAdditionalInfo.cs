using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using static Enterprise.Core.Constants.Customs.Universal;
using RefCusCodeListTypesEU = Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsAdditionalInfo : EU.NCTS.Business.NctsAdditionalInfo
{
	public NctsAdditionalInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	new NctsHeader Header => (NctsHeader)base.Header;

	public INctsAdditionalInfoParent AdditionalInfoParent => base.Parent is INctsAdditionalInfoParent ? (INctsAdditionalInfoParent)base.Parent : null;

	public override ZGuid CSI_ParentID
	{
		get => base.CSI_ParentID;
		set
		{
			var oldValue = CSI_ParentID;
			base.CSI_ParentID = value;
			if (!IsCopying && oldValue != CSI_ParentID)
			{
				SetDefaultSubType();
			}
		}
	}

	public override ZString CSI_ParentTableCode
	{
		get => base.CSI_ParentTableCode;
		set
		{
			var oldValue = CSI_ParentTableCode;
			base.CSI_ParentTableCode = value;
			if (!IsCopying && oldValue != CSI_ParentTableCode)
			{
				SetDefaultSubType();
			}
		}
	}

	protected override ZString DefaultSubTypeValue => ZString.Empty;

	void SetDefaultSubType()
	{
		var parent = Parent;
		if (CSI_SubType.IsEmpty && parent != null)
		{
			var movementHeader = Header?.MovementHeader;
			if (movementHeader != null && movementHeader.IsNationalTransitSwitzerland)
			{
				if (parent is NctsHeader)
				{
					CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				}
				else if (parent is NctsCommonCargoDesc)
				{
					CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				}
			}
			else
			{
				CSI_SubType = base.DefaultSubTypeValue;
			}
		}
	}

	protected override CusSupportingInfoLookups GetNewPhase5Lookups() => new NctsAdditionalInfoLookups(this);

	protected override ZString CodeListTypeCore => GetCodeListTypeCore();

	ZString GetCodeListTypeCore()
	{
		var isNationalTransit = AdditionalInfoParent?.IsNationalTransitSwitzerland ?? false;

		if (Parent is NctsArrivalMovementHeader || Parent is NctsArrivalCargoDesc)
		{
			return RefCusCodeListTypesEU.Codes.Code_AI44N;
		}

		return CSI_SubType.ToString() switch
		{
			AdditionalInfoSubTypeList.Codes.AdditionalReference => isNationalTransit ? ZString.Empty : RefCusCodeListTypesEU.Codes.Code_AR44N,
			AdditionalInfoSubTypeList.Codes.AdditionalInformation => isNationalTransit ? RefCusCodeListTypes.Codes.ExportAddDocAdditionalInformation : RefCusCodeListTypesEU.Codes.Code_AI44N,
			AdditionalInfoSubTypeList.Codes.TransportDocument => (Parent is NctsHeader) ? RefCusCodeListTypesEU.Codes.Code_TD44N : ZString.Empty,
			_ => RefCusCodeListTypesEU.Codes.Code_AI44N
		};
	}
}
