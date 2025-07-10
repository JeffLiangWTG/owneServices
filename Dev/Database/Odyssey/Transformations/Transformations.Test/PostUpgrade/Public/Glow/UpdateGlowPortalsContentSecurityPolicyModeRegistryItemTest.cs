using System.Text;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Glow;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.PostUpgrade.Public.Glow
{
	[TestedType(typeof(UpdateGlowPortalsContentSecurityPolicyModeRegistryItem))]
	abstract class UpdateGlowPortalsContentSecurityPolicyModeRegistryItemTest_Base : RegistryDataTransformationTestCase
	{
		protected const string PolicyMode = "ReportOnly";

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateGlowPortalsContentSecurityPolicyModeRegistryItem();
		}

		protected override void AssertTransformationResults()
		{
			AssertNull("The old registry item is deleted.", Helper.GetStmDataValue(UpdateGlowPortalsContentSecurityPolicyModeRegistryItem.PreviousRegistryItemName));
			AssertEquals("The new registry item value is transformed.", PolicyMode, Encoding.Unicode.GetString(Helper.GetStmDataValue(UpdateGlowPortalsContentSecurityPolicyModeRegistryItem.NewRegistryItemName)));
		}
	}

	[TestedType(typeof(UpdateGlowPortalsContentSecurityPolicyModeRegistryItem))]
	class UpdateGlowPortalsContentSecurityPolicyModeRegistryItemTest_TransferValue : UpdateGlowPortalsContentSecurityPolicyModeRegistryItemTest_Base
	{
		protected override void PrepareTestData()
		{
			Helper.InsertStmDataRow(UpdateGlowPortalsContentSecurityPolicyModeRegistryItem.PreviousRegistryItemName, "STR", Encoding.Unicode.GetBytes(PolicyMode));
		}
	}

	[TestedType(typeof(UpdateGlowPortalsContentSecurityPolicyModeRegistryItem))]
	class UpdateGlowPortalsContentSecurityPolicyModeRegistryItemTest_DeleteOldValue : UpdateGlowPortalsContentSecurityPolicyModeRegistryItemTest_Base
	{
		protected override void PrepareTestData()
		{
			Helper.InsertStmDataRow(UpdateGlowPortalsContentSecurityPolicyModeRegistryItem.PreviousRegistryItemName, "STR", Encoding.Unicode.GetBytes("Foo"));
			Helper.InsertStmDataRow(UpdateGlowPortalsContentSecurityPolicyModeRegistryItem.NewRegistryItemName, "STR", Encoding.Unicode.GetBytes(PolicyMode));
		}
	}
}
