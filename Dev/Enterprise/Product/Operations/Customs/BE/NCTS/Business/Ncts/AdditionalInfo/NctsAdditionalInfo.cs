using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business;

public class NctsAdditionalInfo : EU.NCTS.Business.NctsAdditionalInfo
{
	public NctsAdditionalInfo(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override CusSupportingInfoLookups GetNewPhase4Lookups() => new NctsAdditionalInfoPhase4Lookups(this);

	protected override CusSupportingInfoLookups GetNewPhase5Lookups() => new NctsAdditionalInfoPhase5Lookups(this);

	public override ZString CSI_SubType
	{
		get
		{
			return base.CSI_SubType;
		}
		set
		{
			base.CSI_SubType = value;
			RefreshDepartureMovementHeaderRepresentative();
		}
	}

	public override ZString CSI_Code
	{
		get
		{
			return base.CSI_Code;
		}
		set
		{
			base.CSI_Code = value;
			RefreshDepartureMovementHeaderRepresentative();
		}
	}

	public override void Delete()
	{
		base.Delete();
		RefreshDepartureMovementHeaderRepresentative();
	}

	void RefreshDepartureMovementHeaderRepresentative()
	{
		if (Header?.IsDepartureMovement ?? false)
		{
			Header.MovementHeader?.Representative.RefreshBinding();
		}
	}
}
