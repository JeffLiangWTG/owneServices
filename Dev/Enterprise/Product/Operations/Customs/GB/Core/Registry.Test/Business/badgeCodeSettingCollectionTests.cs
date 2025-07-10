using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Registry.Testing
{
	[TestedType(typeof(BadgeCodeSettingCollection))]
	public class BadgeCodeSettingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<BadgeCodeSettingCollection>
	{
		public void TestFindByPortCodeIncludingDirection()
		{
			BadgeCodeSettingCollection collection = new BadgeCodeSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);

			CreateBadge(collection, "FEY", "GBFXT", "IMP");
			CreateBadge(collection, "JJB", "GBLON", "IMP");
			CreateBadge(collection, "ZPE", "", "EXP");

			AssertEquals("ZPE", collection.FindByPortCode("GBFXT", "EXP").BadgeCode);
			AssertEquals("FEY", collection.FindByPortCode("GBFXT", "IMP").BadgeCode);
			AssertEquals("ZPE", collection.FindByPortCode("GBLON", "EXP").BadgeCode);
			AssertEquals("JJB", collection.FindByPortCode("GBLON", "IMP").BadgeCode);
			AssertEquals("ZPE", collection.FindByPortCode("GBDTE", "EXP").BadgeCode);
			AssertEquals(null, collection.FindByPortCode("GBDTE", "IMP"));
			CreateBadge(collection, "ALL", "", "");
			AssertEquals("ALL", collection.FindByPortCode("GBDTE", "IMP").BadgeCode);
			CreateBadge(collection, "ZPE", "GBDTE", "IMP");
			AssertEquals("ZPE", collection.FindByPortCode("GBDTE", "IMP").BadgeCode);
		}

		void CreateBadge(BadgeCodeSettingCollection collection, string badgeCode, string portCode, string direction)
		{
			var badge = collection.AddNew();
			badge.BadgeCode = badgeCode;
			badge.RL_PortCode = portCode;
			badge.Direction = direction;
		}

		public void TestFindByBadgeCodeOnly()
		{
			BadgeCodeSettingCollection collection = new BadgeCodeSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			CreateBadge(collection, "FEY", "GBFXT", "IMP");
			CreateBadge(collection, "JJB", "GBLON", "IMP");
			CreateBadge(collection, "ZPE", "", "EXP");

			AssertEquals("FEY", collection.FindByBadgeCodeOnly("FEY").BadgeCode);
			AssertEquals("ZPE", collection.FindByBadgeCodeOnly("ZPE").BadgeCode);
			AssertEquals("JJB", collection.FindByBadgeCodeOnly("JJB").BadgeCode);
		}

		#region Implementation
		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override BadgeCodeSettingCollection GetCollectionToTest()
		{
			return new BadgeCodeSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new BadgeCodeSetting(Factory);
		}
		#endregion
	}
}
