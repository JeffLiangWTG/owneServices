using System;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.ZArchitecture.Business.ClusterKey.Testing
{
	[TestsSubclassesOf(typeof(IClusterKeyMaster), RequireTestOnlyInFirstSubLevel = true, IncludeAbstractClasses = true)]
	public abstract class ClusterKeyMasterMandatoryTest : ClusterKeyEntityTest
	{
		public void TestSetClusterKeyOnNewMasterObject()
		{
			AssertEquals("[PRE-CONDITION] ClusterKey", 0, ClusterKeyEntityToTest.ClusterKeyPty.Value);
			Factory.Save();
			AssertEquals("[After Save] ClusterKey", 1, ClusterKeyEntityToTest.ClusterKeyPty.Value);
		}

		public void TestSetClusterKeyOnExistingMasterObject()
		{
			AssertEquals("[PRE-CONDITION] ClusterKey", 0, ClusterKeyEntityToTest.ClusterKeyPty.Value);
			Factory.Save();

			void setClusterKeyAction() => ClusterKeyEntityToTest.ClusterKeyPty.Value++;

			if (ClusterKeyEntityToTest is IClusterKeyWorker)
			{
				NUnit.Framework.Assert.That(setClusterKeyAction, CustomConstraints.InnermostExceptionThrown(typeof(InvalidOperationException), ClusterKeyEntityExtension.InvalidAttemptToModifyMidLevelMasterWhenParentFkHasNotChanged, true), "Worker-or-Master cluster keys can only be modified when their FK to the ClusterKeyParent has changes.");
			}
			else
			{
				NUnit.Framework.Assert.That(setClusterKeyAction, CustomConstraints.InnermostExceptionThrown(typeof(InvalidOperationException), ClusterKeyEntityExtension.InvalidAttemptToModifyExistingTopLevelMaster), "Top level IClusterKeyMaster cluster keys can only be modified on new business objects.");
			}
		}

		public void TestConstraintDoesNotAllowClusterKeyToBeLessOrEqualToZeroInDatabase()
		{
			AssertEquals("[PRE-CONDITION] ClusterKey", 0, ClusterKeyEntityToTest.ClusterKeyPty.Value);
			Factory.Save();
			AssertEquals("ClusterKey - after save", 1, ClusterKeyEntityToTest.ClusterKeyPty.Value);

			var bizObj = ClusterKeyEntityToTestAsBizObj;
			AssertUpdateClusterKeyException(bizObj, 0);
			AssertUpdateClusterKeyException(bizObj, -1);
		}
	}
}
