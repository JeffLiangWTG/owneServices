using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Business.Test.Business.ClusterKey.Test;
using WTG.NUnit;

namespace Enterprise.ZArchitecture.Business.Test.Business.ClusterKey
{
	sealed class ClusterKeyEntityExtensionTest : TestCaseWithDummy
	{
		public void TestCheckCanSetMasterClusterKey_ThrowsException_WhenClusterKeyMasterIsNull()
		{
			IClusterKeyMaster clusterKeyMaster = null;

			AssertExceptionThrown<ArgumentNullException>(() => clusterKeyMaster.CheckCanSetMasterClusterKey());
		}

		public void TestCheckCanSetMasterClusterKey_ThrowsInvalidOperationException_WhenFkToParentPtyIsNull()
		{
			var dummyWorkerOrMaster = Factory.New<DummyClusterKeyChildBizoWithNullParentPty>();
			var expectedPk = dummyWorkerOrMaster.PK;

			Factory.Save();

			Assert("ClusterKey should be set", dummyWorkerOrMaster.ClusterKeyPty.Value > 0);
			var exception = AssertExceptionThrown<InvalidOperationException>(() => dummyWorkerOrMaster.CheckCanSetMasterClusterKey());

			var expectedMessage =
				ClusterKeyEntityExtension.InvalidAttemptToModifyMidLevelMasterWhenParentFkHasNotChanged;
			expectedMessage += $" PK: {expectedPk}";
			expectedMessage += " ParentPK: ";
			expectedMessage += " ClusterKey: 1";

			AssertEquals(expectedMessage, exception.Message);

			using (SetCountryTemporarily(Enterprise.Core.Constants.CountryCodes.Brazil))
			{
				NUnit.Framework.Assert.That(() => dummyWorkerOrMaster.CheckCanSetMasterClusterKey(), CustomConstraints.InnermostExceptionThrown(typeof(InvalidOperationException), ClusterKeyEntityExtension.InvalidAttemptToModifyMidLevelMasterWhenParentFkHasNotChanged, true), "Worker-or-Master cluster keys can only be modified when their FK to the ClusterKeyParent has changes.");
			}
		}

		public void TestCheckCanSetMasterClusterKey_ThrowsInvalidOperationException_WhenNotWorker()
		{
			var dummyWorkerOrMaster = Factory.New<DummyClusterKeyParentBizo>();

			Factory.Save();

			Assert("ClusterKey should be set", dummyWorkerOrMaster.ClusterKeyPty.Value > 0);
			AssertExceptionThrown<InvalidOperationException>(() => dummyWorkerOrMaster.CheckCanSetMasterClusterKey());

			using (SetCountryTemporarily(Enterprise.Core.Constants.CountryCodes.Brazil))
			{
				NUnit.Framework.Assert.That(() => dummyWorkerOrMaster.CheckCanSetMasterClusterKey(), CustomConstraints.InnermostExceptionThrown(typeof(InvalidOperationException), ClusterKeyEntityExtension.InvalidAttemptToModifyExistingTopLevelMaster, true), "Worker-or-Master cluster keys can only be modified when their FK to the ClusterKeyParent has changes.");
			}
		}

		IDisposable SetCountryTemporarily(string countryCode)
		{
			var currentCountryCode = StaticCurrentFetcher.Instance.CurrentCompany.Country.RN_Code;

			StaticCurrentFetcher.Instance.CurrentCompany.SetCountry(countryCode);

			return new DisposableAction(() =>
				StaticCurrentFetcher.Instance.CurrentCompany.SetCountry(currentCountryCode));
		}
	}
}
