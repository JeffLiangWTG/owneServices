using System;
using CargoWise.ApplicationManager.Common;
using CargoWise.Common;
using Moq;

namespace Enterprise.Client.Common.Testing
{
	static class AppManagerTestHelper
	{
		public static IDisposable MockAppManager(IAppManager appManager)
		{
			var mockAppManagerFactory = new Mock<IAppManagerClientFactory>();
			mockAppManagerFactory.Setup(f => f.GetNewAppManager()).Returns(appManager);

			var defaultAppManagerClientFactory = AppManagerClientFactory.Instance;
			AppManagerClientFactory.Instance = mockAppManagerFactory.Object;

			return new DisposableAction(() => AppManagerClientFactory.Instance = defaultAppManagerClientFactory);
		}

		public static void VerifyNeverInvoked(this Mock<IAppManager> mockAppManager)
		{
			mockAppManager.Verify(c => c.Invoke(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<MutexRequest>()), Times.Never);
		}

		public static IDisposable MockIsAdmin(bool isAdmin)
		{
			var mockChecker = new Mock<IAdminChecker>();
			mockChecker.Setup(c => c.IsAdmin()).Returns(isAdmin);

			var defaultChecker = AdminChecker.Instance;
			AdminChecker.Instance = mockChecker.Object;
			return new DisposableAction(() => AdminChecker.Instance = defaultChecker);
		}
	}
}
