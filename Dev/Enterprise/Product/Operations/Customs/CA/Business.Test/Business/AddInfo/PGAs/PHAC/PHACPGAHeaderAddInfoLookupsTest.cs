using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class PHACPGAHeaderAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUNDGCodeList()
		{
			AssertEquals(typeof(UNDGSubstanceCollection), header.AddInfoLookups.UNDGCodeList.GetType());
		}

		public void TestPGAIndicatorList()
		{
			AssertEquals(typeof(YesNoList), header.AddInfoLookups.PGAIndicatorList.GetType());
		}

		public void TestProgramCodesList()
		{
			AssertEquals(typeof(PHACPGADepartmentCodes), header.AddInfoLookups.ProgramCodesList.GetType());
		}

		public void TestIntendedUseCodes()
		{
			AssertEquals(typeof(PHACEndUseCodes), header.AddInfoLookups.IntendedUseCodes.GetType());
		}

		public void TestCategories()
		{
			var ph01Categories = new[] { PHACCategories.Codes.PH01, PHACCategories.Codes.PH02, PHACCategories.Codes.PH05, PHACCategories.Codes.PH06 };
			header.CA_IntendedUseCode = PHACEndUseCodes.Codes.PH01;
			AssertContainsExactElementsInAnyOrder(header.AddInfoLookups.Categories.GetAllCodes(), ph01Categories);

			var ph02Categories = new[] { PHACCategories.Codes.PH01, PHACCategories.Codes.PH02, PHACCategories.Codes.PH03, PHACCategories.Codes.PH04, PHACCategories.Codes.PH05, PHACCategories.Codes.PH06 };
			header.CA_IntendedUseCode = PHACEndUseCodes.Codes.PH02;
			AssertContainsExactElementsInAnyOrder(header.AddInfoLookups.Categories.GetAllCodes(), ph02Categories);

			var ph03Categories = new[] { PHACCategories.Codes.PH01, PHACCategories.Codes.PH02, PHACCategories.Codes.PH03, PHACCategories.Codes.PH04, PHACCategories.Codes.PH05, PHACCategories.Codes.PH06 };
			header.CA_IntendedUseCode = PHACEndUseCodes.Codes.PH03;
			AssertContainsExactElementsInAnyOrder(header.AddInfoLookups.Categories.GetAllCodes(), ph03Categories);

			var ph04Categories = new[] { PHACCategories.Codes.PH01, PHACCategories.Codes.PH02, PHACCategories.Codes.PH05, PHACCategories.Codes.PH06 };
			header.CA_IntendedUseCode = PHACEndUseCodes.Codes.PH04;
			AssertContainsExactElementsInAnyOrder(header.AddInfoLookups.Categories.GetAllCodes(), ph04Categories);

			var ph05Categories = new[] { PHACCategories.Codes.PH01, PHACCategories.Codes.PH02, PHACCategories.Codes.PH03, PHACCategories.Codes.PH04, PHACCategories.Codes.PH05, PHACCategories.Codes.PH06 };
			header.CA_IntendedUseCode = PHACEndUseCodes.Codes.PH05;
			AssertContainsExactElementsInAnyOrder(header.AddInfoLookups.Categories.GetAllCodes(), ph05Categories);

			var ph06Categories = new[] { PHACCategories.Codes.PH01, PHACCategories.Codes.PH02, PHACCategories.Codes.PH05, PHACCategories.Codes.PH06 };
			header.CA_IntendedUseCode = PHACEndUseCodes.Codes.PH06;
			AssertContainsExactElementsInAnyOrder(header.AddInfoLookups.Categories.GetAllCodes(), ph06Categories);

			var emptyCategories = new[] { PHACCategories.Codes.PH01, PHACCategories.Codes.PH02, PHACCategories.Codes.PH03, PHACCategories.Codes.PH04, PHACCategories.Codes.PH05, PHACCategories.Codes.PH06 };
			header.CA_IntendedUseCode = string.Empty;
			AssertContainsExactElementsInAnyOrder(header.AddInfoLookups.Categories.GetAllCodes(), emptyCategories);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<PHACPGAHeader>();
		}
		PHACPGAHeader header;

		#endregion
	}
}
