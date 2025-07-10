using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSCAHouseCollectionNonDependent))]
	sealed class CusSCAHouseCollectionNonDependentTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusSCAHouseCollectionNonDependent(Factory);
		}

		public void TestCreateRelationshipFilter()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.NewZealand))
			{
				var oceanbill1 = Factory.NewWithValidTestData<CusSCAOceanBill>();
				oceanbill1.CB_GB = GlbBranch.CurrentBranch.PK;
				var housebill1 = oceanbill1.HouseBills.AddNew();
				Factory.Save();
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Australia))
			{
				var oceanbill2 = Factory.NewWithValidTestData<CusSCAOceanBill>();
				oceanbill2.CB_GB = GlbBranch.CurrentBranch.PK;
				var housebill2 = oceanbill2.HouseBills.AddNew();

				var oceanbill3 = Factory.NewWithValidTestData<CusSCAOceanBill>();
				oceanbill3.CB_GB = GlbBranch.CurrentBranch.PK;
				var housebill3 = oceanbill3.HouseBills.AddNew();
				Factory.Save();

				oceanbill3.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACISea;
				Factory.Save();

				var collection = new CusSCAHouseCollectionNonDependent(Factory);
				collection.Load();
				AssertEquals(1, collection.Count);
				AssertEquals("only contains current company housebills and application code is CMR", housebill2.PK, collection[0].PK);
			}
		}
	}
}
