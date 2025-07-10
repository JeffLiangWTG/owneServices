using System;
using System.Threading;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.Common.Testing
{
	sealed class TransformationTest : TestCase
	{
		class OnlineTransformHelper : DataTransformation
		{
			public override string UserDescription => "OnlineTransform";

			protected override void OnlinePostUpgradeTransform(CancellationToken token)
			{
				token.ThrowIfCancellationRequested();
			}
		}

		public void TestCancellationTokenOnlinePostUpgrade()
		{
			var transform = new OnlineTransformHelper();
			var cancellationTokenSource = new CancellationTokenSource();

			transform.Run(TransformationSection.OnlinePostUpgrade, cancellationTokenSource.Token);

			cancellationTokenSource.Cancel();
			AssertExceptionThrown(typeof(OperationCanceledException), () => transform.Run(TransformationSection.OnlinePostUpgrade, cancellationTokenSource.Token));
		}

		public void TestIsRequiredReturnsTrueForNull()
		{
			var mock = new Mock<DataTransformation>();
			mock.CallBase = true;
			Assert(mock.Object.IsRequired);
		}

		class DataTransformListeningForToken : DataTransformation
		{
			public override string UserDescription => "Listening...";
			protected override void OnlinePostUpgradeTransform(CancellationToken token)
			{
				token.ThrowIfCancellationRequested();
			}
		}

		public void TestCancellationTokenIsPassedDown()
		{
			var cts = new CancellationTokenSource();
			new DataTransformListeningForToken().Run(TransformationSection.OnlinePostUpgrade, cts.Token);
			cts.Cancel();
			AssertExceptionThrown<OperationCanceledException>(() => new DataTransformListeningForToken().Run(TransformationSection.OnlinePostUpgrade, cts.Token));
		}

		public void TestIsRequiredForOlderNewerAndSameVersions()
		{
			var mockUpgradeManager = new Mock<IUpgradeManager>();
			mockUpgradeManager.Setup(x => x.TransformationVersionBeforeUpgrade).Returns(new VersionLabel(7500, 0));

			var mock = new Mock<DataTransformation>();
			mock.CallBase = true;
			var transform = mock.Object;
			transform.Initialise(new VersionLabel(7499, 0), mockUpgradeManager.Object);
			Assert(!transform.IsRequired);

			transform.Initialise(new VersionLabel(7500, 0), mockUpgradeManager.Object);
			Assert(!transform.IsRequired);

			transform.Initialise(new VersionLabel(7500, 1), mockUpgradeManager.Object);
			Assert(transform.IsRequired);

			transform.Initialise(new VersionLabel(7501, 0), mockUpgradeManager.Object);
			Assert(transform.IsRequired);
		}
	}
}
