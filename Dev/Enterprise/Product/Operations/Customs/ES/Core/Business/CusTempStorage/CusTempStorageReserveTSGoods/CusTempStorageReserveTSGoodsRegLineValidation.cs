using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.Business.CusTempStorage;

public class CusTempStorageReserveTSGoodsRegLineValidation : ZValidation
{
	public CusTempStorageReserveTSGoodsRegLineValidation(CusTempStorageReserveTSGoodsRegLine parent) : base(parent)
	{
		this.parent = parent;
		zValidationInternals = this;
	}

	public override Type AutoValidationType => typeof(CusTempStorageReserveTSGoodsRegLineValidation);

	public override void ValidateAll()
	{
		if (parent.IsPackageTypeBulk)
		{
			ValidateGrossWeightToUse();
		}
		else
		{
			ValidatePackagesToUse();
		}
	}

	public void ValidatePackagesToUse()
	{
		zValidationInternals.Validate(parent.PackagesToUseInfo, CheckPackagesToUse);
	}

	public void ValidateGrossWeightToUse()
	{
		zValidationInternals.Validate(parent.GrossWeightToUseInfo, CheckGrossWeightToUse);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by test via reflection")]
	void CheckPackagesToUse()
	{
		if (parent.PackagesToUse > parent.RemainingPackageQty)
		{
			parent.PackagesToUseInfo.AddError(ExceedRemainingPackageQuantities);
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by test via reflection")]
	void CheckGrossWeightToUse()
	{
		if (parent.GrossWeightToUse > parent.RemainingGrossWeight)
		{
			parent.GrossWeightToUseInfo.AddError(ExceedRemainingGrossWeight);
		}
	}

	readonly CusTempStorageReserveTSGoodsRegLine parent;
	[System.Diagnostics.DebuggerBrowsableAttribute(System.Diagnostics.DebuggerBrowsableState.Never)]
	readonly IValidationInternals zValidationInternals;

	public string ExceedRemainingPackageQuantities => Res.GetString("63DC4648-B719-45EA-834E-5A13EA3F7E50", "There is not enough package quantity of goods in the selected line. Please, distribute quantities to use according to availability.");
	public string ExceedRemainingGrossWeight => Res.GetString("A2D80F25-2C1F-440E-9C2F-C8254B18BC81", "There is not enough gross weight quantity of goods in the selected line. Please, distribute quantities to use according to availability.");
}
