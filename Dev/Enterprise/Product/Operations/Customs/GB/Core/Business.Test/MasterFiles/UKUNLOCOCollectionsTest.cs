namespace Enterprise.Customs.GB.Business.MasterFiles.Testing
{
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	public abstract class BrexitCountryCollectionsTest<T> : ActiveBusinessObjectCollectionTestCase<T>
		where T : RefUNLOCOCollection
	{
		protected internal RefUNLOCO Barcelona { get; private set; }
		protected internal RefUNLOCO Chester { get; private set; }
		protected internal RefUNLOCO Belfast { get; private set; }
		protected internal RefUNLOCO Dublin { get; private set; }

		public override void TestAdd()
		{
			Assert("We do not allow adding", true);
		}
		public override void TestDelete()
		{
			Assert("We do not allow delete", true);
		}

		[StressTest]
		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			base.TestAddAndCancelOfElementAsThoughBinding();
		}

		[StressTest]
		public override void TestAddNew()
		{
			Assert("We do not allow add new", true);
		}

		[StressTest]
		public override void TestCancelNew_DoesNotReport()
		{
			base.TestCancelNew_DoesNotReport();
		}

		[StressTest]
		public override void TestRemoveFromRelationship()
		{
			base.TestRemoveFromRelationship();
		}

		[StressTest]
		public override void TestTypedget_Item()
		{
			base.TestTypedget_Item();
		}

		protected override void SetUp()
		{
			base.SetUp();

			Barcelona = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "ESBCN"));
			Chester = new RefUNLOCO.Loader(Factory).Load("GBCEG");
			if (Chester.CountryStates == null)
			{
				var ni = Factory.NewWithValidTestData<RefCountryStates>();
				ni.RW_RegionName = "ENGLAND";
				Chester.RL_RW = ni.PK;
			}

			Belfast = new RefUNLOCO.Loader(Factory).Load("GBBEL");
			if (Belfast.CountryStates == null || string.Compare(Belfast.CountryStates.RW_RegionName, "NORTHERN IRELAND", true) != 0)
			{
				var ni = Factory.NewWithValidTestData<RefCountryStates>();
				ni.RW_RegionName = "NORTHERN IRELAND";
				Belfast.RL_RW = ni.PK;
			}

			// make dublin GVMS
			Dublin = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "IEDUB"));
			var dublinGvms = Dublin.RefLocoMaps.OfType<RefLocoMap>().FirstOrDefault(m => m.RY_SystemUsage == "GVM");
			if (dublinGvms == null)
			{
				var map1 = Dublin.RefLocoMaps.AddNew();
				map1.RY_RN = Chester.Country.PK;
				map1.RY_SystemUsage = "GVM";
			}
			Factory.Save();
		}
	}

	[TestedType(typeof(UKUnlocoUKLocations))]
	public class UKUNLocoUKLocationsTest : BrexitCountryCollectionsTest<UKUnlocoUKLocations>
	{
		public void TestUKUnlocoUKLocations()
		{
			var list = GetCollectionToTest();
			AssertNull(list.FindByPK(Barcelona.PK));
			AssertNotNull(list.FindByPK(Chester.PK));
			AssertNotNull(list.FindByPK(Belfast.PK));
		}

		protected override UKUnlocoUKLocations GetCollectionToTest()
		{
			return new UKUnlocoUKLocations(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Chester;
		}
	}

	[TestedType(typeof(UKUnlocoGBLocations))]
	public class UKUNLocoGBLocationsTest : BrexitCountryCollectionsTest<UKUnlocoGBLocations>
	{
		public void TestUKUnlocoGBLocations()
		{
			var list = GetCollectionToTest();
			AssertNull(list.FindByPK(Barcelona.PK));
			AssertNotNull(list.FindByPK(Chester.PK));
			AssertNull(list.FindByPK(Belfast.PK));
		}

		protected override UKUnlocoGBLocations GetCollectionToTest()
		{
			return new UKUnlocoGBLocations(Factory);
		}
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Chester;
		}
	}

	[TestedType(typeof(UKUnlocoNILocations))]
	public class UKUNLocoNILocationsTest : BrexitCountryCollectionsTest<UKUnlocoNILocations>
	{
		public void TestUKUnlocoNILocations()
		{
			var list = GetCollectionToTest();
			AssertNull(list.FindByPK(Barcelona.PK));
			AssertNull(list.FindByPK(Chester.PK));
			AssertNotNull(list.FindByPK(Belfast.PK));
		}

		protected override UKUnlocoNILocations GetCollectionToTest()
		{
			return new UKUnlocoNILocations(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Belfast;
		}
	}

	[TestedType(typeof(UKUnlocoGVMSLocations))]
	public class UKUNLocoGVMSLocationsTest : BrexitCountryCollectionsTest<UKUnlocoGVMSLocations>
	{
		public void TestUKUnlocoGVMSLocations()
		{
			var list = GetCollectionToTest();
			AssertNull(list.FindByPK(Barcelona.PK));
			AssertNull(list.FindByPK(Chester.PK));
			AssertNull(list.FindByPK(Belfast.PK));
			AssertNotNull(list.FindByPK(Dublin.PK));
		}

		protected override UKUnlocoGVMSLocations GetCollectionToTest()
		{
			return new UKUnlocoGVMSLocations(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Dublin;
		}
	}
}
