using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsPackage : EU.NCTS.Business.NctsPackage,
	Integration.Customs.IT.INctsPackage,
	IRN22CheckablePackage
{
	public NctsPackage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new NctsDepartureCargoDesc Parent => (NctsDepartureCargoDesc)base.Parent;

	protected override CusInvPackValidation GetNewPhase5Validation() => new EU.NCTS.Business.NctsPackagePhase5Validation(this);

	protected override CusInvPackValidation GetNewPhase4Validation() => new NctsPackagePhase4Validation(this);

	protected override ZString HumanReadableShortcutNameCore => GetHumanReadableName();

	protected override ZString HumanReadableNameCore => GetHumanReadableName();

	ZString GetHumanReadableName() => Res.GetString("5A222BF3-7B24-4BCD-B6CE-1FF591A33B4E", "Package");

	public override ZString B5_MarksAndNumbers
	{
		get => base.B5_MarksAndNumbers;
		set
		{
			var oldValue = B5_MarksAndNumbers;
			base.B5_MarksAndNumbers = value;
			if (!IsCopying && !IsValidationSuspended && oldValue != B5_MarksAndNumbers)
			{
				Validation.ValidateB5_UnitCount();
			}
		}
	}

	#region IRN22CheckablePackage

	IEnumerable<IRN22CheckablePackage> IRN22CheckablePackage.GetRelatedEntryPreviousPackages()
	{
		var currentGoodsItem = Parent;
		return currentGoodsItem.MoveHeader
			.GoodsItems
			.Cast<NctsDepartureCargoDesc>()
			.Where(x => x.BY_LineNo < currentGoodsItem.BY_LineNo)
			.OrderByDescending(x => x.BY_LineNo)
			.Select(x => x.Package)
			.ToArray();
	}

	ZString ICheckablePackage.UnitType => B5_UnitType;

	ZString ICheckablePackage.MarksAndNumbers => B5_MarksAndNumbers;

	ZLong ICheckablePackage.UnitCount => B5_UnitCount;

	ZPropertyInfo ICheckablePackage.UnitCountInfo => B5_UnitCountInfo;

	#endregion
}
