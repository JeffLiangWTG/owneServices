using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsHeader.Loader))]
class NctsHeaderLoaderTest : LoaderTestCase
{
	protected override BusinessObject.Loader GetNewLoaderToTest() => new NctsHeader.Loader(Factory);

	public void TestFindByMovementReferenceNumber() => CombineAssertions(() =>
	{
		var nctsHeaders = Setup(NctsMovementType.Codes.Departure, CusEntryNumberTypes.Standard.MovementReferenceNumber);
		var loader = new NctsHeader.Loader(Factory);

		AssertEquals(nctsHeaders.expectedNctsHeader.PK, Find("100", "1")?.PK);
		AssertEquals(nctsHeaders.expectedNctsHeader.PK, FindByMrnOnly("100.1")?.PK);
		AssertNull("Other MovementType", Find("100", "1", movementType: NctsMovementType.Codes.Arrival));
		AssertNull("Other MRNNumber", Find("109", "1"));
		AssertNull("Other MRNVersion", Find("100", "9"));
		AssertNull("Other EntryNumType", Find("200", "1"));
		AssertNull("Other EntryNumCountry", Find("300", "1"));
		AssertNull("Not NCTS5", Find("400", "1"));
		AssertEquals("Other branch", nctsHeaders.nctsHeaderOtherBranch.PK, Find("500", "1")?.PK);
		AssertNull("Other company", Find("600", "1"));
		AssertNull("Empty MRN", FindByMrnOnly(string.Empty));
		AssertNull("Empty MRNNumber", Find(string.Empty, "1"));
		AssertNull("Empty MRNVersion", Find("100", string.Empty));

		NctsHeader FindByMrnOnly(string mrnNumber, string movementType = NctsMovementType.Codes.Departure)
		{
			return loader.FindByMovementReferenceNumber(movementType, mrnNumber);
		}

		NctsHeader Find(string mrnNumber, string mrnVersion, string movementType = NctsMovementType.Codes.Departure)
		{
			return loader.FindByMovementReferenceNumber(movementType, mrnNumber, mrnVersion);
		}
	});

	public void TestFindByMovementReferenceNumberAnyVersion()
	{
		var nctsHeaders = Setup(NctsMovementType.Codes.Departure, CusEntryNumberTypes.Standard.MovementReferenceNumber);
		var loader = new NctsHeader.Loader(Factory);

		AssertEquals(nctsHeaders.expectedNctsHeader.PK, Find("100")?.PK);
		AssertNull("Other MovementType", Find("100", movementType: NctsMovementType.Codes.Arrival));
		AssertNull("Other MRNNumber", Find("109"));
		AssertNull("Other EntryNumType", Find("200"));
		AssertNull("Other EntryNumCountry", Find("300"));
		AssertNull("Not NCTS5", Find("400"));
		AssertEquals("Other branch", nctsHeaders.nctsHeaderOtherBranch.PK, Find("500")?.PK);
		AssertNull("Other company", Find("600"));
		AssertEquals("700", Find("700")?.MovementReferenceNumber);
		AssertNull("Empty MRN", Find(string.Empty));

		NctsHeader Find(string mrnNumber, string movementType = NctsMovementType.Codes.Departure)
		{
			return loader.FindByMovementReferenceNumberAnyVersion(movementType, mrnNumber);
		}
	}

	public void TestFindByArrivalReferenceNumber() => CombineAssertions(() =>
	{
		var nctsHeaders = Setup(NctsMovementType.Codes.Arrival, CusEntryNumberTypes.EU.ArrivalReferenceNumber);
		var loader = new NctsHeader.Loader(Factory);

		AssertEquals(nctsHeaders.expectedNctsHeader.PK, Find("100.1")?.PK);
		AssertNull("Other ARN", Find("109.1"));
		AssertNull("Other EntryNumType", Find("200.1"));
		AssertNull("Other EntryNumCountry", Find("300.1"));
		AssertNull("Not NCTS5", Find("400.1"));
		AssertEquals("Other branch", nctsHeaders.nctsHeaderOtherBranch.PK, Find("500.1")?.PK);
		AssertNull("Other company", Find("600.1"));
		AssertNull("Empty MRN", Find(string.Empty));

		NctsHeader Find(string entryNum)
		{
			return loader.FindByArrivalReferenceNumber(entryNum);
		}
	});

		public void TestFindByMovementReferenceNumbers() => CombineAssertions(() =>
		{
			_ = Setup(NctsMovementType.Codes.Arrival, CusEntryNumberTypes.Standard.MovementReferenceNumber);
			var loader = new NctsHeader.Loader(Factory);
			var nctsHeader1 = CreateNctsHeader(Common.EU.NctsMoveHeaderType.Codes.Arrival, "110", CusEntryNumberTypes.Standard.MovementReferenceNumber);
			var nctsHeader2 = CreateNctsHeader(Common.EU.NctsMoveHeaderType.Codes.Arrival, "120", CusEntryNumberTypes.Standard.MovementReferenceNumber);
			Factory.Save();

		AssertFind(new[] { nctsHeader1, nctsHeader2 }, "110", "120");
		AssertFind(new[] { nctsHeader1 }, "110", "220");
		AssertFind(Array.Empty<NctsHeader>(), "210", "220");
		AssertFind(new[] { nctsHeader1 }, "110", ZString.Empty);

			void AssertFind(NctsHeader[] expectedResult, params ZString[] entryNums)
			{
				var assertionMessage = string.Join(" ", entryNums.Select(x => x.ToString()));
				var actualResult = loader.FindByMovementReferenceNumbers(Common.EU.NctsMoveHeaderType.Codes.Arrival, entryNums);
				AssertContainsExactElementsInAnyOrder(assertionMessage, expectedResult.Select(x => x.PK), actualResult.Select(x => x.PK));
			}
		});

	(NctsHeader expectedNctsHeader, NctsHeader nctsHeaderOtherBranch) Setup(string expectedMovementType, string expectedEntryNumType)
	{
		var otherBranch = GlbCompany.GetCurrentCompany(Factory).Branches.AddNew();
		otherBranch.GB_Code = "B2";
		otherBranch.GB_IsActive = ZBool.True;
		var otherCompany = Factory.New<GlbCompany>();
		otherCompany.GC_Code = "C9";
		var otherCompanyBranch = otherCompany.Branches.AddNew();
		otherCompanyBranch.GB_Code = "B9";
		otherCompanyBranch.GB_IsActive = ZBool.True;
		Factory.Save();

		var nctsHeader1 = CreateNctsHeader(expectedMovementType, "100.1", expectedEntryNumType);
		_ = CreateNctsHeader(expectedMovementType, string.Empty, expectedEntryNumType);
		_ = CreateNctsHeader(expectedMovementType, null, expectedEntryNumType);
		_ = CreateNctsHeader(expectedMovementType, "200.1", CusEntryNumberTypes.Standard.ImportControlNumber);
		_ = CreateNctsHeader(expectedMovementType, "300.1", expectedEntryNumType, entryNumCountry: Core.Constants.CountryCodes.Ecuador);

		var nctsHeader4 = CreateNctsHeader(expectedMovementType, "400.1", expectedEntryNumType);
		nctsHeader4.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

		NctsHeader nctsHeader5;
		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, otherBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
		{
			nctsHeader5 = CreateNctsHeader(expectedMovementType, "500.1", expectedEntryNumType);
		}

		using (DisposableEnvironment.ForBranch(otherCompanyBranch.PK.ToGuid()))
		{
			var nctsHeader6 = CreateNctsHeader(expectedMovementType, "600.1", expectedEntryNumType);
		}
		_ = CreateNctsHeader(expectedMovementType, "700", expectedEntryNumType);

		Factory.Save();

		return (nctsHeader1, nctsHeader5);
	}

	NctsHeader CreateNctsHeader(string movementType, string entryNum, string entryNumType, string entryNumCountry = null)
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(movementType);
		if (entryNum != null)
		{
			CusEntryNumber.LoadOrCreate(nctsHeader, entryNumType, entryNumCountry ?? Core.Constants.CountryCodes.Switzerland).CE_EntryNum = entryNum;
		}
		return nctsHeader;
	}
}
