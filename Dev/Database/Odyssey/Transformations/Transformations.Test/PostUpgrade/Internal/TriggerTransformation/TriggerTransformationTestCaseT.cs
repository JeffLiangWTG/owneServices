using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.Common;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Testing
{
	[TestsSubclassesOf(typeof(CreateTriggerTransformation))]
	public abstract class TriggerTransformationTestCase<T> : DataTransformationTestCase where T : CreateTriggerTransformation, new()
	{
		/// <summary>
		/// Infamous new modifier used, because having the base test virtual
		/// was encouraging people to override this test defeating its purpose.
		/// </summary>
		public new void TestRunAndAssertResultsTwice()
		{
			// The trigger transformations will be only run once. Running second time will cause exception by design.
			// MSH: Trigger transforms are excluded from ScriptSynchroniser, therefore raising exception in particular transform
			// would indicate broken logic in ScriptSynchroniser earlier it’s better then silently hide this problem and have potentially broken data.
			// [DS] Any transformation may run more than once if re-mapped or after a downgrade.
			//      This is why transformation tests must run twice.
			PrepareTestData();

			TransformationToTest.Run();

			AssertTransformationResults();
		}

		public void TestRunTwice_FromNegativeOneMinorVersion()
		{
			PrepareTestData();

			TransformationToTest.Run();

			AssertTransformationResults();

			var upgradeManager = new UpgradeManagerForTestWithMockSchemaVersionBeforeUpgrade(new VersionLabel(9001, -1));
			AssertNoExceptionThrown(GetNewTestTransformationInstance(upgradeManager).Run);

			AssertTransformationResults();
		}

		protected sealed override DataTransformation GetNewTestTransformationInstance() => GetNewTestTransformationInstance(new DummyUpgradeManager());
		protected CreateTriggerTransformation GetNewTestTransformationInstance(IUpgradeManager upgradeManager)
		{
			var result = new T();
			result.Initialise(null, upgradeManager);
			return result;
		}
	}
}
