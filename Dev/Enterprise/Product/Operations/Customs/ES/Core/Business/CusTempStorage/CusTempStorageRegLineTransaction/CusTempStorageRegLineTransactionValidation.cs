using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.CusTempStorage;

public class CusTempStorageRegLineTransactionValidation : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionValidation
{
	public CusTempStorageRegLineTransactionValidation(CusTempStorageRegLineTransaction parent)
		: base(parent)
	{
	}

	public new CusTempStorageRegLineTransaction Parent => (CusTempStorageRegLineTransaction)base.Parent;

	protected override void CheckSRT_GrossWeight()
	{
		base.CheckSRT_GrossWeight();

		var parent = Parent;

		if (!parent.IsInDatabase)
		{
			MandatoryValidation.CheckEntered(parent.SRT_GrossWeightInfo);

			var regLine = parent.RegLine;
			var remainingGrossWeight = regLine.GrossWeightRemainingCalculated;
			if (remainingGrossWeight < 0)
			{
				parent.SRT_GrossWeightInfo.AddError(Res.GetString("F18E9C69-1AF7-49F5-9FC2-59EA64A6FE0D", "Remaining Gross Weight should not be negative."));
			}

			var grossWeight = parent.SRT_GrossWeight;
			if (grossWeight > 0)
			{
				parent.SRT_GrossWeightInfo.AddWarning(GetValuePositiveValidationMessage(parent.SRT_GrossWeightInfo.HumanReadableName));
			}

			if (!regLine.IsPackageTypeBulk && remainingGrossWeight == 0 && regLine.PackagesRemainingCalculated != 0)
			{
				parent.SRT_GrossWeightInfo.AddError(Res.GetString("76F62B9A-F871-4288-8858-58B4BB5423B4", "If Remaining Gross Weight is 0 then Remaining Packages must be also 0 and vice-versa."));
			}
		}
	}

	protected override void CheckSRT_PackageQty()
	{
		base.CheckSRT_PackageQty();

		var parent = Parent;

		if (!parent.IsInDatabase)
		{
			var regLine = parent.RegLine;

			if (!regLine.IsPackageTypeBulk)
			{
				MandatoryValidation.CheckEntered(parent.SRT_PackageQtyInfo);
			}

			var remainingPacks = regLine.PackagesRemainingCalculated;
			if (remainingPacks < 0)
			{
				parent.SRT_PackageQtyInfo.AddError(Res.GetString("70C47750-02E8-4CB4-89DB-4B90F8783932", "Remaining Packages should not be negative."));
			}

			var packageQty = parent.SRT_PackageQty;
			if (packageQty > 0)
			{
				parent.SRT_PackageQtyInfo.AddWarning(GetValuePositiveValidationMessage(parent.SRT_PackageQtyInfo.HumanReadableName));
			}

			if (!regLine.IsPackageTypeBulk && remainingPacks == 0 && regLine.GrossWeightRemainingCalculated != 0)
			{
				parent.SRT_PackageQtyInfo.AddError(Res.GetString("00B8C4E1-EDA8-4B5F-A572-8A7F6FB09FEB", "If Remaining Packages is 0 then Remaining Gross Weight must be also 0 and vice-versa."));
			}
		}
	}

	protected override void CheckSRT_BondAmount()
	{
		base.CheckSRT_BondAmount();

		var parent = Parent;

		if (!parent.IsInDatabase)
		{
			var grossWeight = parent.SRT_GrossWeight;
			if (grossWeight > 0)
			{
				parent.SRT_BondAmountInfo.AddWarning(Res.GetString("1FC7E040-B7F2-47B0-AB14-1CFC69362543", "When adding goods to stock, the liability amount cannot be calculated so no guarantee transaction will be created. If needed, it should be manually added by the user."));
			}
		}
	}

	protected override void CheckSRT_ReferenceType()
	{
		base.CheckSRT_ReferenceType();

		var parent = Parent;
		if (!parent.IsInDatabase)
		{
			ListValidation.ErrorIfInvalidCodeOrEmpty(parent.SRT_ReferenceTypeInfo);
		}
	}

	protected override void CheckSRT_Reference()
	{
		base.CheckSRT_Reference();

		var parent = Parent;
		if (!parent.IsInDatabase)
		{
			MandatoryValidation.CheckEntered(parent.SRT_ReferenceInfo);
		}
	}

	protected override void CheckSRT_InternalReferenceType()
	{
		base.CheckSRT_InternalReferenceType();

		var parent = Parent;
		if (!parent.IsInDatabase)
		{
			ListValidation.ErrorIfInvalidCodeOrEmpty(parent.SRT_InternalReferenceTypeInfo);
		}
	}

	protected override void CheckSRT_InternalReferenceNumber()
	{
		base.CheckSRT_InternalReferenceNumber();

		var parent = Parent;
		if (!parent.IsInDatabase)
		{
			MandatoryValidation.CheckEntered(parent.SRT_InternalReferenceNumberInfo);
		}
	}

	protected override void CheckSRT_TransactionDate()
	{
		base.CheckSRT_TransactionDate();

		var parent = Parent;
		if (!parent.IsInDatabase)
		{
			MandatoryValidation.CheckEntered(parent.SRT_TransactionDateInfo);
		}
	}

	protected override void CheckSRT_PhysicalInOutDate()
	{
		base.CheckSRT_PhysicalInOutDate();

		var parent = Parent;
		if (!parent.IsInDatabase)
		{
			MandatoryValidation.CheckEntered(parent.SRT_PhysicalInOutDateInfo);
		}
	}

	ZString GetValuePositiveValidationMessage(ZString fieldName) => Res.GetString("CF5B2E13-446E-4CB2-8A79-E5F1D70A097B", "Please note value entered in {0} will be summed to the stock. If you need to make a manual exit, value entered should be negative.", fieldName);
}
