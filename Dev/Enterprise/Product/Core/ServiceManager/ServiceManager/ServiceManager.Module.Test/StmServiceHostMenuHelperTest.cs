using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ServiceManager.Business;

namespace Enterprise.ServiceManager.Module.Testing
{
	class StmServiceHostMenuHelperTest : TestCaseWithFactory
	{
		public void TestToInstallationArguments_WithUsername_IncludesUsername()
		{
			// Assert
			var installationInfo = new ServiceInstallInfo(Factory);
			installationInfo.Username = "Username";

			// Act
			var args = StmServiceHostMenuHelper.ToInstallationArguments(installationInfo);

			// Assert
			Assert(args.Contains(@"-username:Username"));
		}

		public void TestToInstallationArguments_WithUsernameAndPassword_IncludesPassword()
		{
			// Assert
			var installationInfo = new ServiceInstallInfo(Factory);
			installationInfo.Username = "Username";
			installationInfo.Password = "Password";

			// Act
			var args = StmServiceHostMenuHelper.ToInstallationArguments(installationInfo);

			// Assert
			Assert(args.Contains(@"-password:Password"));
		}

		public void TestToInstallationArguments_UsernameWithSpace_QuotesUsername()
		{
			// Assert
			var installationInfo = new ServiceInstallInfo(Factory);
			installationInfo.Username = @"User Name";

			// Act
			var args = StmServiceHostMenuHelper.ToInstallationArguments(installationInfo);

			// Assert
			Assert(args.Contains(@"""-username:User Name"""));
		}

		public void TestToInstallationArguments_PasswordWithSpace_QuotesPassword()
		{
			// Assert
			var installationInfo = new ServiceInstallInfo(Factory);
			installationInfo.Username = "Username";
			installationInfo.Password = @"Pass Word";

			// Act
			var args = StmServiceHostMenuHelper.ToInstallationArguments(installationInfo);

			// Assert
			Assert(args.Contains(@"""-password:Pass Word"""));
		}

		public void TestToInstallationArguments_UsernameWithQuotes_EscapesUsername()
		{
			// Assert
			var installationInfo = new ServiceInstallInfo(Factory);
			installationInfo.Username = @"User""Name";

			// Act
			var args = StmServiceHostMenuHelper.ToInstallationArguments(installationInfo);

			// Assert
			Assert(args.Contains(@"-username:User\""Name"));
		}

		public void TestToInstallationArguments_PasswordWithQuotes_EscapesPassword()
		{
			// Assert
			var installationInfo = new ServiceInstallInfo(Factory);
			installationInfo.Username = "Username";
			installationInfo.Password = @"Pass""Word";

			// Act
			var args = StmServiceHostMenuHelper.ToInstallationArguments(installationInfo);

			// Assert
			Assert(args.Contains(@"-password:Pass\""Word"));
		}
	}
}
