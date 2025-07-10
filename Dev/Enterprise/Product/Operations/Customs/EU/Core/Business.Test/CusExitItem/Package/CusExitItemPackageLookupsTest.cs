using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	class CusExitItemPackageLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestUnitTypeList()
		{
			var unpackPackType = Factory.SetupUnpackCusCode();
			var bulkPackType = Factory.SetupBulkCusCode();
			var lookups = GetLookupForTest();
			NUnit.Framework.Assert.That(lookups.UnitTypeList.CodesAsString, NUnit.Framework.Is.EqualTo($"{unpackPackType}, {bulkPackType}"));
		}

		[ExpectNoExceptions]
		public void TestUnpackedPackageUnitTypeList()
		{
			var unpackPackType = Factory.SetupUnpackCusCode();
			var lookups = GetLookupForTest();
			NUnit.Framework.Assert.That(lookups.UnpackedPackageUnitTypeList.CodesAsString, NUnit.Framework.Is.EqualTo(unpackPackType).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestBulkPackageUnitTypeList()
		{
			var bulkPackType = Factory.SetupBulkCusCode();
			var lookups = GetLookupForTest();
			NUnit.Framework.Assert.That(lookups.BulkPackageUnitTypeList.CodesAsString, NUnit.Framework.Is.EqualTo(bulkPackType).Using(CustomComparers.TypeComparison));
		}

		CusExitItemPackageLookups GetLookupForTest()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			exitHeader.CEH_ParentID = declaration.PK;
			exitHeader.CEH_ParentTableCode = declaration.TablePrefix;
			var exitDetail = exitHeader.CusExitDetails.AddNew();
			exitDetail.CED_Status = "XXX";
			var exitItem = exitDetail.CusExitItems.AddNew();
			exitItem.CXI_Status = ExitItemStatusList.Codes.UNK;
			var package = exitItem.Packages.AddNew();
			var lookups = new CusExitItemPackageLookups(package);
			return lookups;
		}
	}
}
