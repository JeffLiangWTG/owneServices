using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsPackage : EU.NCTS.Business.NctsPackage
{
	public NctsPackage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override CusInvPackValidation GetNewPhase5Validation() => new NctsPackageValidation(this);

	protected override CusInvPackLookups GetNewLookups() => new NctsPackageLookups(this);

	protected override EU.NCTS.Business.NctsPackage CreateAndCopyPackDifference()
	{
		var packDifference = base.CreateAndCopyPackDifference();
		packDifference.B5_MarksAndNumbers = B5_MarksAndNumbers;
		packDifference.B5_SequenceNumber = B5_SequenceNumber;
		packDifference.B5_UnitCount = B5_UnitCount;
		packDifference.B5_UnitType = B5_UnitType;
		return packDifference;
	}

	protected override bool B5_MarksAndNumbersReadOnly => UnloadedColumsReadOnly;

	protected override bool B5_UnitCountReadOnly => UnloadedColumsReadOnly;

	protected override bool B5_UnitTypeReadOnly => UnloadedColumsReadOnly;

	bool UnloadedColumsReadOnly => !IsUnloadedStateEmptyOrNew || IsUnloadingRemarksReadOnly;

	bool IsUnloadedStateEmptyOrNew => B5_TypeOfDifference.IsEmpty || B5_TypeOfDifference == NctsUnloadedStateList.Codes.NEW;

	public bool IsDIFWithDifferences => Factory.GetCached(ref isDIFWithDifferences, () =>
	{
		return B5_TypeOfDifference == NctsUnloadedStateList.Codes.DIF
			&& PackDifference is NctsPackage packDifference
			&& (InventoryDataProviderHelper.IsDifferent(B5_UnitType, packDifference.B5_UnitType)
			|| InventoryDataProviderHelper.IsDifferent(B5_UnitCount, packDifference.B5_UnitCount)
			|| InventoryDataProviderHelper.IsDifferent(B5_MarksAndNumbers, packDifference.B5_MarksAndNumbers));
	});
	CachedProperty<bool> isDIFWithDifferences;

	public new NctsPackage PackDifference => (NctsPackage)base.PackDifference;
}
