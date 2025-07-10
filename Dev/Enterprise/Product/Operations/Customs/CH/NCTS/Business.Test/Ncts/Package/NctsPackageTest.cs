using System.Runtime.CompilerServices;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsPackage))]
sealed class NctsPackageTest : CusInvPackTest<NctsCommonCargoDesc>
{
	public void TestLookups() => AssertType<NctsPackageLookups>(PackagePivot.Lookups);

	public void TestValidation() => AssertType<NctsPackageValidation>(PackagePivot.Validation);

	public void TestUnloadedColumsReadOnly_Empty() => AssertUnloadedColumsReadOnly(string.Empty, false);

	public void TestUnloadedColumsReadOnly_NEW() => AssertUnloadedColumsReadOnly(NctsUnloadedStateList.Codes.NEW, false);

	public void TestUnloadedColumsReadOnly_DEC() => AssertUnloadedColumsReadOnly(NctsUnloadedStateList.Codes.DEC, true);

	public void TestUnloadedColumsReadOnly_MIS() => AssertUnloadedColumsReadOnly(NctsUnloadedStateList.Codes.MIS, true);

	public void TestUnloadedColumsReadOnly_DIF_CopyData() => CombineAssertions(() =>
	{
		PackagePivot.B5_MarksAndNumbers = "m+n";
		PackagePivot.B5_SequenceNumber = 2;
		PackagePivot.B5_UnitCount = 125;
		PackagePivot.B5_UnitType = "PT";

		PackagePivot.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;

		AssertEquals("Number of packages disabled", true, PackagePivot.B5_UnitCountInfo.ReadOnly);
		AssertEquals("Type disabled", true, PackagePivot.B5_UnitTypeInfo.ReadOnly);
		AssertEquals("Marks & Numbers disabled", true, PackagePivot.B5_MarksAndNumbersInfo.ReadOnly);

		var packDifference = PackagePivot.PackDifference;

		AssertEquals("Number of packages enabled for child", false, packDifference.B5_UnitCountInfo.ReadOnly);
		AssertEquals("Type enabled for child", false, packDifference.B5_UnitTypeInfo.ReadOnly);
		AssertEquals("Marks & Numbers enabled for child", false, packDifference.B5_MarksAndNumbersInfo.ReadOnly);

		AssertEquals("Copied B5_MarksAndNumbers to child", PackagePivot.B5_MarksAndNumbers, packDifference.B5_MarksAndNumbers);
		AssertEquals("Copied B5_SequenceNumber to child", PackagePivot.B5_SequenceNumber, packDifference.B5_SequenceNumber);
		AssertEquals("Copied B5_UnitCount to child", PackagePivot.B5_UnitCount, packDifference.B5_UnitCount);
		AssertEquals("Copied B5_UnitType to child", PackagePivot.B5_UnitType, packDifference.B5_UnitType);
	});

	public void TestIsDIFWithDifferences() => CombineAssertions(() =>
	{
		PackagePivot.B5_UnitType = "PT";
		PackagePivot.B5_UnitCount = 12;
		PackagePivot.B5_MarksAndNumbers = "M+N";

		AssertResult(false);
		AssertResult(true, unitType: "CT");
		AssertResult(true, unitCount: 23);
		AssertResult(true, marksAndNumbers: "XXX");
		AssertResult(false, unitType: ZString.Empty);
		AssertResult(false, unitCount: ZLong.Zero);
		AssertResult(false, marksAndNumbers: ZString.Empty);

		AssertResult(false, marksAndNumbers: "XXX", typeOfDifference: NctsUnloadedStateList.Codes.NEW);
		AssertResult(false, marksAndNumbers: "XXX", typeOfDifference: NctsUnloadedStateList.Codes.DEC);
		AssertResult(false, marksAndNumbers: "XXX", typeOfDifference: NctsUnloadedStateList.Codes.MIS);

		void AssertResult(bool expectedResult, string typeOfDifference = NctsUnloadedStateList.Codes.DIF, ZString? unitType = null, ZLong? unitCount = null, ZString? marksAndNumbers = null, [CallerLineNumber] int line = 0)
		{
			PackagePivot.B5_TypeOfDifference = typeOfDifference;
			var packDifference = PackagePivot.PackDifference;
			if (packDifference != null)
			{
				packDifference.B5_UnitType = unitType ?? PackagePivot.B5_UnitType;
				packDifference.B5_UnitCount = unitCount ?? PackagePivot.B5_UnitCount;
				packDifference.B5_MarksAndNumbers = marksAndNumbers ?? PackagePivot.B5_MarksAndNumbers;
			}
			var assertionMessage = $"[{line}] {typeOfDifference}: UnitType={PackagePivot.B5_UnitType}/{packDifference?.B5_UnitType} UnitCount={PackagePivot.B5_UnitCount}/{packDifference?.B5_UnitCount} MarksAndNumbers={PackagePivot.B5_MarksAndNumbers}/{packDifference?.B5_MarksAndNumbers}";
			AssertEquals(assertionMessage, expectedResult, PackagePivot.IsDIFWithDifferences);
		}
	});

	void AssertUnloadedColumsReadOnly(string typeOfDifference, bool expectedReadOnly) => CombineAssertions(() =>
	{
		PackagePivot.B5_TypeOfDifference = typeOfDifference;

		AssertEquals("Number of packages", expectedReadOnly, PackagePivot.B5_UnitCountInfo.ReadOnly);
		AssertEquals("Type", expectedReadOnly, PackagePivot.B5_UnitCountInfo.ReadOnly);
		AssertEquals("Marks & Numbers", expectedReadOnly, PackagePivot.B5_UnitCountInfo.ReadOnly);
	});

	protected override NctsCommonCargoDesc GetNewParent()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		return nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
	}

	NctsPackage PackagePivot => (NctsPackage)base.packagePivot;
}
