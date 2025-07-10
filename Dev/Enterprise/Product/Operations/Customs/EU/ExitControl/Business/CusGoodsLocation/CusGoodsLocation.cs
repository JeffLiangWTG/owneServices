using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.ExitControl.Business;

public class CusGoodsLocation : EU.Business.CusGoodsLocation
	, Integration.Customs.EUExitControl.ICusGoodsLocation
{
	public CusGoodsLocation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public static new readonly CusGoodsLocationTypeDecider TypeDecider = new CusGoodsLocationTypeDecider();

	public CusExitReport CusExitReport => cusExitReport ??= Parent as CusExitReport;
	CusExitReport cusExitReport;

	public new CusGoodsLocationAddress Address => (CusGoodsLocationAddress)base.Address;

	protected override Type AddressType => typeof(CusGoodsLocationAddress);

	public ZBool IsUCC6 => CusExitReport?.IsUCC6 ?? false;

	public override ZString CGL_Qualifier
	{
		get => base.CGL_Qualifier;
		set
		{
			base.CGL_Qualifier = value;
			if (!IsCopying && IsUCC6)
			{
				CusExitReport?.MarkAsNeedingValidation();
				Address.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString CGL_Type
	{
		get => base.CGL_Type;
		set
		{
			base.CGL_Type = value;
			if (!IsCopying && IsUCC6)
			{
				CusExitReport?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString CGL_ParentTableCode
	{
		get => base.CGL_ParentTableCode;
		set
		{
			base.CGL_ParentTableCode = value;
			if (!IsCopying && IsUCC6)
			{
				CusExitReport?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZGuid CGL_ParentID
	{
		get => base.CGL_ParentID;
		set
		{
			base.CGL_ParentID = value;
			if (!IsCopying && IsUCC6)
			{
				CusExitReport?.MarkAsNeedingValidation();
				Address.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString CGL_LocationUse
	{
		get => base.CGL_LocationUse;
		set
		{
			base.CGL_LocationUse = value;
			if (!IsCopying && IsUCC6)
			{
				CusExitReport?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString CGL_AdditionalIdentifier
	{
		get => base.CGL_AdditionalIdentifier;
		set
		{
			base.CGL_AdditionalIdentifier = value;
			if (!IsCopying && IsUCC6)
			{
				CusExitReport?.MarkAsNeedingValidation();
			}
		}
	}

	protected override TypeLoaderCollection GetParentLoaders()
	{
		var parentLoaders = base.GetParentLoaders();
		parentLoaders.Add(typeof(CusExitReport));
		return parentLoaders;
	}
}
