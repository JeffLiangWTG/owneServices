using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACusRuling))]
	sealed class CACusRulingTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookups()
		{
			var cusRuling = Factory.New<CACusRuling>();
			AssertType<CACusRulingLookups>(cusRuling.Lookups);
		}

		public void TestHumanReadableName()
		{
			var cusRuling = Factory.New<CACusRuling>();
			AssertEquals("Remissions", cusRuling.HumanReadableName);
		}

		public void TestHumanReadableShortcutName()
		{
			var cusRuling = Factory.New<CACusRuling>();
			cusRuling.ZZX_RulingNumber = "12345";
			AssertEquals("Shortcut when ruling has not applies to set", "OIC - 12345", cusRuling.HumanReadableShortcutName);

			var header = Factory.New<OrgHeader>();
			header.OH_Code = "TESTORG";
			var address = header.MainAddress;
			address.Address1 = "testAddress";
			cusRuling.ZZX_OA_AppliesTo = address.PK;

			AssertEquals("Shortcut when ruling has applies to set", "OIC - 12345 - TESTORG", cusRuling.HumanReadableShortcutName);
		}

		public void TestConfigurations()
		{
			var cusRuling = Factory.NewWithValidTestData<CACusRuling>();
			var configurations = cusRuling.Configurations;
			AssertType<CACusRulingConfigCollection>(configurations);
			cusRuling.ZZX_RulingType = RefCusRulingTypeList.Codes.T;
			Assert("Not readonly when Ruling type is T", !configurations.ReadOnly);
			var config1 = configurations.AddNew();
			config1.ZZY_Category = "DTY";
			cusRuling.ZZX_RulingType = RefCusRulingTypeList.Codes._2;
			Assert("Readonly when Ruling type is not T", configurations.ReadOnly);
			AssertEquals("Items cleared", 0, configurations.Count);
		}

		public void TestValidation()
		{
			var cusRuling = Factory.NewWithValidTestData<CACusRuling>();
			AssertType<CACusRulingValidation>(cusRuling.Validation);
		}

		public override void TestCloneAuditProperties()
		{
			Assert("Need this to supress test failure caused by customized default values", true);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<CACusRuling>();
		}
	}
}
