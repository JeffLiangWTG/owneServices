using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.BE.NCTS.Business;

public class CusGoodsLocation : EU.NCTS.Business.CusGoodsLocation
	, Integration.Customs.BE.INctsCusGoodsLocation
{
	public CusGoodsLocation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new NctsArrivalMovementHeader ArrivalMovementHeader => (NctsArrivalMovementHeader)base.ArrivalMovementHeader;

	public new CusGoodsLocationAddress Address => (CusGoodsLocationAddress)base.Address;

	protected override Type AddressType => typeof(CusGoodsLocationAddress);

	public new CusGoodsLocationLookups Lookups => (CusGoodsLocationLookups)base.Lookups;

	protected override Customs.Business.CusGoodsLocationLookups GetNewLookups() => new CusGoodsLocationLookups(this);

	public new CusGoodsLocationValidation Validation => (CusGoodsLocationValidation)base.Validation;

	protected override Customs.Business.CusGoodsLocationValidation GetNewValidation() => new CusGoodsLocationValidation(this);

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		if (IsParentIncidentPhase5Arrival)
		{
			CGL_Type = ZString.Empty;
		}
	}

	[MaxLength(17)]
	public override ZString Unlocode { get => base.Unlocode; set => base.Unlocode = value; }

	public override ZString CGL_Qualifier
	{
		get => base.CGL_Qualifier;
		set
		{
			var oldValue = CGL_Qualifier;
			base.CGL_Qualifier = value;
			if (!IsCopying && oldValue != CGL_Qualifier)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString CGL_ParentTableCode
	{
		get => base.CGL_ParentTableCode;
		set
		{
			var oldValue = CGL_ParentTableCode;
			base.CGL_ParentTableCode = value;
			if (!IsCopying && oldValue != CGL_ParentTableCode)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZGuid CGL_ParentID
	{
		get => base.CGL_ParentID;
		set
		{
			var oldValue = CGL_ParentID;
			base.CGL_ParentID = value;
			if (!IsCopying && oldValue != CGL_ParentID)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString CGL_LocationUse
	{
		get => base.CGL_LocationUse;
		set
		{
			var oldValue = CGL_LocationUse;
			base.CGL_LocationUse = value;
			if (!IsCopying && oldValue != CGL_LocationUse)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString CGL_AdditionalIdentifier
	{
		get => base.CGL_AdditionalIdentifier;
		set
		{
			var oldValue = CGL_AdditionalIdentifier;
			base.CGL_AdditionalIdentifier = value;
			if (!IsCopying && oldValue != CGL_AdditionalIdentifier)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString CGL_Type
	{
		get => base.CGL_Type;
		set
		{
			var oldValue = CGL_Type;
			base.CGL_Type = value;
			if (!IsCopying && oldValue != CGL_Type)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString CGL_CustomsOffice
	{
		get => base.CGL_CustomsOffice;
		set
		{
			var oldValue = CGL_CustomsOffice;
			base.CGL_CustomsOffice = value;
			if (!IsCopying && oldValue != CGL_CustomsOffice)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}
}
