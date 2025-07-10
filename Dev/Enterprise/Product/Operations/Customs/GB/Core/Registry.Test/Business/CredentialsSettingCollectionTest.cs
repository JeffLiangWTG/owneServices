namespace Enterprise.Customs.GB.Registry.Testing
{
	using CargoWise.EntityFramework;
	using Enterprise.Customs.GB.Registry;
	using Enterprise.Environment;
	using Enterprise.Registry.Business.Testing;
	using Enterprise.ZArchitecture.Environment;
	using NUnit.Framework;

	[TestedType(typeof(CredentialsSettingCollection))]
	public class CredentialsSettingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<CredentialsSettingCollection>
	{
		public void TestFindByCredentials()
		{
			CredentialsSettingCollection collection = new CredentialsSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			AssertEquals(null, collection.FindByBadgeCode("FEY"));
			collection.AddNew().BadgeCode = "XXX";
			AssertEquals(null, collection.FindByBadgeCode("FEY"));
			CredentialsSetting setting = collection.AddNew();
			setting.BadgeCode = "FEY";
			setting.Username = "Hello";
			AssertEquals(setting, collection.FindByBadgeCode("FEY"));
			collection.AddNew().BadgeCode = "YYY";
			AssertEquals(setting, collection.FindByBadgeCode("FEY"));
			AssertEquals("Hello", collection.FindByBadgeCode("FEY").Username);
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

		protected override CredentialsSettingCollection GetCollectionToTest()
		{
			return new CredentialsSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CredentialsSetting();
		}
		#endregion
	}
}
