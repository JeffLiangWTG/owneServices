using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using Moq;

namespace Enterprise.ZArchitecture.Business
{
	sealed class AdvOrmFeatureHelperTest : TestCaseWithFactory
	{
		public void TestRunWith_IsEnabled_WhenMockedToBeTrue_IsTrue()
		{
			SetupFeatureControlRule(enabled: false);
			Assert("should be disabled when not mocked", !AdvOrmFeatureHelper.IsEnabled);

			AdvOrmFeatureHelper.RunTestWith(isEnabled: true, action: () =>
			{
				Assert("mocked to be enabled", AdvOrmFeatureHelper.IsEnabled);
			});
			Assert("should be reset after mocked run", !AdvOrmFeatureHelper.IsEnabled);
		}

		public void TestRunWith_IsEnabled_WhenMockedToBeFalse_IsFalse()
		{
			SetupFeatureControlRule(enabled: true);
			Assert("should be enabled when not mocked", AdvOrmFeatureHelper.IsEnabled);

			AdvOrmFeatureHelper.RunTestWith(isEnabled: false, action: () =>
			{
				Assert("mockedn to be disabled", !AdvOrmFeatureHelper.IsEnabled);
			});
			Assert("should be reset after mocked run", AdvOrmFeatureHelper.IsEnabled);
		}

		public void TestGetMockedDisposable_IsEnabled_WhenMockedToBeTrue_IsTrue()
		{
			SetupFeatureControlRule(enabled: false);
			Assert("should be disabled when not mocked", !AdvOrmFeatureHelper.IsEnabled);

			using (AdvOrmFeatureHelper.GetMockedDisposable(isEnabled: true))
			{
				Assert("mocked to be enabled", AdvOrmFeatureHelper.IsEnabled);
			}
			Assert("should be reset after mocked run", !AdvOrmFeatureHelper.IsEnabled);
		}

		public void TestGetMockedDisposable_IsEnabled_WhenMockedToBeFalse_IsFalse()
		{
			SetupFeatureControlRule(enabled: true);
			Assert("should be enabled when not mocked", AdvOrmFeatureHelper.IsEnabled);

			using (AdvOrmFeatureHelper.GetMockedDisposable(isEnabled: false))
			{
				Assert("mocked to be disabled", !AdvOrmFeatureHelper.IsEnabled);
			}
			Assert("should be reset after mocked run", AdvOrmFeatureHelper.IsEnabled);
		}

		public void TestIsEnabled_WhenFeatureControlNotActive_IsFalse()
		{
			SetupFeatureControlRule(enabled: false);

			Assert(!AdvOrmFeatureHelper.IsEnabled);
		}

		public void TestIsEnabled_WhenFeatureControlActive_IsTrue()
		{
			SetupFeatureControlRule(enabled: true);

			Assert(AdvOrmFeatureHelper.IsEnabled);
		}

		void SetupFeatureControlRule(bool enabled)
		{
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureControlMock
				.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.AdvancedOrderManagerFeatureControl, CancellationToken.None))
				.Returns(Task.FromResult(enabled ? new Mock<IFeatureData>().Object : null));
			ObjectFactory.Substitute(featureControlMock.Object);
		}
	}
}
