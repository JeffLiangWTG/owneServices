using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Moq;

namespace Enterprise.ZArchitecture.Business.Test.Business.ClusterKey
{
	sealed class ClusterKeyPropagationStrategyTest : TestCaseWithDummy
	{
		public void TestIsParentEmpty_ReturnsTrue_WhenFkToParentPtyIsNull()
		{
			var mockClusterKeyWorker = new Mock<IClusterKeyWorker>();
			mockClusterKeyWorker.Setup(x => x.FkToParentPty).Returns((ZPropertyInfoGuid)null);

			var result = ClusterKeyPropagationStrategy<IClusterKeyEntity>.IsParentEmpty(mockClusterKeyWorker.Object);

			Assert(result);
		}

		public void TestIsParentEmpty_ReturnsTrue_WhenFkToParentPtyIsEmpty()
		{
			var fkToParentPty = (ZPropertyInfoGuid)Dummy.Z0_GuidInfo;
			fkToParentPty.Value = ZGuid.Empty;

			var mockClusterKeyWorker = new Mock<IClusterKeyWorker>();
			mockClusterKeyWorker.Setup(x => x.FkToParentPty).Returns(fkToParentPty);

			var result = ClusterKeyPropagationStrategy<IClusterKeyEntity>.IsParentEmpty(mockClusterKeyWorker.Object);

			Assert(result);
		}

		public void TestIsParentEmpty_ReturnsFalse_WhenFkToParentPtyIsNotEmpty()
		{
			var fkToParentPty = (ZPropertyInfoGuid)Dummy.Z0_GuidInfo;
			fkToParentPty.Value = ZGuid.NewZGuid();

			var mockClusterKeyWorker = new Mock<IClusterKeyWorker>();
			mockClusterKeyWorker.Setup(x => x.FkToParentPty).Returns(fkToParentPty);

			var result = ClusterKeyPropagationStrategy<IClusterKeyEntity>.IsParentEmpty(mockClusterKeyWorker.Object);

			Assert(!result);
		}

		public void TestIsParentDirty_ReturnsTrue_WhenClusterKeyPtyIsFlaggedForChange()
		{
			var clusterKeyPty = (ZPropertyInfoInt)Dummy.Z0_NumberInfo;
			clusterKeyPty.Value = ClusterKeyFlaggedForChangeValue;

			var mockClusterKeyWorker = new Mock<IClusterKeyWorker>();
			mockClusterKeyWorker.Setup(x => x.ClusterKeyPty).Returns(clusterKeyPty);

			var result = ClusterKeyPropagationStrategy<IClusterKeyEntity>.IsParentDirty(mockClusterKeyWorker.Object);

			Assert(result);
		}

		public void TestIsParentDirty_ReturnsFalse_WhenFkToParentPtyIsNull()
		{
			var clusterKeyPty = (ZPropertyInfoInt)Dummy.Z0_NumberInfo;
			clusterKeyPty.Value = NonClusterKeyFlaggedForChangeValue;

			var mockClusterKeyWorker = new Mock<IClusterKeyWorker>();
			mockClusterKeyWorker.Setup(x => x.FkToParentPty).Returns((ZPropertyInfoGuid)null);
			mockClusterKeyWorker.Setup(x => x.ClusterKeyPty).Returns(clusterKeyPty);

			var result = ClusterKeyPropagationStrategy<IClusterKeyEntity>.IsParentDirty(mockClusterKeyWorker.Object);

			Assert(!result);
		}

		public void TestIsParentDirty_ReturnsTrue_WhenFkToParentPtyHasChanges()
		{
			Factory.Save();

			var clusterKeyPty = (ZPropertyInfoInt)Dummy.Z0_NumberInfo;
			clusterKeyPty.Value = NonClusterKeyFlaggedForChangeValue;

			var fkToParentPty = (ZPropertyInfoGuid)Dummy.Z0_GuidInfo;
			fkToParentPty.Value = ZGuid.NewZGuid();

			var mockClusterKeyWorker = new Mock<IClusterKeyWorker>();
			mockClusterKeyWorker.Setup(x => x.FkToParentPty).Returns(fkToParentPty);
			mockClusterKeyWorker.Setup(x => x.ClusterKeyPty).Returns(clusterKeyPty);

			var result = ClusterKeyPropagationStrategy<IClusterKeyEntity>.IsParentDirty(mockClusterKeyWorker.Object);

			Assert(result);
		}

		public void TestIsParentDirty_ReturnsFalse_WhenFkToParentPtyHasNoChanges()
		{
			var clusterKeyPty = (ZPropertyInfoInt)Dummy.Z0_NumberInfo;
			clusterKeyPty.Value = NonClusterKeyFlaggedForChangeValue;

			var fkToParentPty = (ZPropertyInfoGuid)Dummy.Z0_GuidInfo;
			fkToParentPty.Value = ZGuid.NewZGuid();

			var mockClusterKeyWorker = new Mock<IClusterKeyWorker>();
			mockClusterKeyWorker.Setup(x => x.FkToParentPty).Returns(fkToParentPty);
			mockClusterKeyWorker.Setup(x => x.ClusterKeyPty).Returns(clusterKeyPty);

			var result = ClusterKeyPropagationStrategy<IClusterKeyEntity>.IsParentDirty(mockClusterKeyWorker.Object);

			Assert(!result);
		}

		const int ClusterKeyFlaggedForChangeValue = -1;
		const int NonClusterKeyFlaggedForChangeValue = 0;
	}
}
