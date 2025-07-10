using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.CusTempStorage;

public class CusTempStorageRegLineTransactionFormEditableValidation : AutoCusTempStorageRegLineTransactionFormEditableValidation
{
	public CusTempStorageRegLineTransactionFormEditableValidation(AutoCusTempStorageRegLineTransactionFormEditable transaction) : base(transaction)
	{
	}

	public new CusTempStorageRegLineTransactionFormEditable Parent => (CusTempStorageRegLineTransactionFormEditable)base.Parent;

	protected override void CheckSRT_GrossWeight()
	{
		base.CheckSRT_GrossWeight();

		var parent = Parent;

		MandatoryValidation.CheckEntered(parent.SRT_GrossWeightInfo);

		var regLine = parent.RegLine;
		var remainingGrossWeight = regLine.GrossWeightRemainingCalculated + Parent.SRT_GrossWeight;
		if (remainingGrossWeight < 0)
		{
			parent.SRT_GrossWeightInfo.AddError(Res.GetString("17ADA5CE-4252-48C7-9304-3F9C488F5642", "Remaining Gross Weight should not be negative."));
		}

		var grossWeight = parent.SRT_GrossWeight;
		if (grossWeight > 0)
		{
			parent.SRT_GrossWeightInfo.AddWarning(GetValuePositiveValidationMessage(parent.SRT_GrossWeightInfo.HumanReadableName));
		}
		var remainingPacks = regLine.PackagesRemainingCalculated + Parent.SRT_PackageQty;
		if (!regLine.IsPackageTypeBulk && remainingGrossWeight == 0 && remainingPacks != 0)
		{
			parent.SRT_GrossWeightInfo.AddError(Res.GetString("F07AE07D-ECC1-40B0-BA99-1DA5BD177A6D", "If Remaining Gross Weight is 0 then Remaining Packages must be also 0 and vice-versa."));
		}
	}

	protected override void CheckSRT_PackageQty()
	{
		base.CheckSRT_PackageQty();

		var parent = Parent;

		var regLine = parent.RegLine;

		if (!regLine.IsPackageTypeBulk)
		{
			MandatoryValidation.CheckEntered(parent.SRT_PackageQtyInfo);
		}

		var remainingPacks = regLine.PackagesRemainingCalculated + Parent.SRT_PackageQty;
		if (remainingPacks < 0)
		{
			parent.SRT_PackageQtyInfo.AddError(Res.GetString("68A91E88-C13A-4DFD-864C-343B51F2A46F", "Remaining Packages should not be negative."));
		}

		var packageQty = parent.SRT_PackageQty;
		if (packageQty > 0)
		{
			parent.SRT_PackageQtyInfo.AddWarning(GetValuePositiveValidationMessage(parent.SRT_PackageQtyInfo.HumanReadableName));
		}
		var remainingGrossWeight = regLine.GrossWeightRemainingCalculated + Parent.SRT_GrossWeight;
		if (!regLine.IsPackageTypeBulk && remainingPacks == 0 && remainingGrossWeight != 0)
		{
			parent.SRT_PackageQtyInfo.AddError(Res.GetString("CDEFF5C9-CB1B-4A6A-B695-4DAE4A8331DA", "If Remaining Packages is 0 then Remaining Gross Weight must be also 0 and vice-versa."));
		}
	}

	protected override void CheckSRT_ReferenceType()
	{
		base.CheckSRT_ReferenceType();
		ListValidation.ErrorIfInvalidCodeOrEmpty(Parent.SRT_ReferenceTypeInfo);
	}

	protected override void CheckSRT_Reference()
	{
		base.CheckSRT_Reference();
		MandatoryValidation.CheckEntered(Parent.SRT_ReferenceInfo);
	}

	protected override void CheckSRT_InternalReferenceType()
	{
		base.CheckSRT_InternalReferenceType();
		ListValidation.ErrorIfInvalidCodeOrEmpty(Parent.SRT_InternalReferenceTypeInfo);
	}

	protected override void CheckSRT_InternalReferenceNumber()
	{
		base.CheckSRT_InternalReferenceNumber();
		MandatoryValidation.CheckEntered(Parent.SRT_InternalReferenceNumberInfo);
	}

	protected override void CheckTransactionDate()
	{
		base.CheckTransactionDate();
		MandatoryValidation.CheckEntered(Parent.TransactionDateInfo);
	}

	protected override void CheckPhysicalInOutDate()
	{
		base.CheckPhysicalInOutDate();
		MandatoryValidation.CheckEntered(Parent.PhysicalInOutDateInfo);
	}

	ZString GetValuePositiveValidationMessage(ZString fieldName) => Res.GetString("0EE5F5A3-2ACD-4996-AC63-7B6EC4BFD761", "Please note value entered in {0} will be summed to the stock. If you need to make a manual exit, value entered should be negative.", fieldName);
}
