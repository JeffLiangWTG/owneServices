using Enterprise.Client.EDI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace ZClientEDI.Business.Test.Security
{
	public class MasterPasswordTest : TestCase
	{
		[TestDate(2009, 10, 15)]
		public void TestGenerateForCurrentUserToday()
		{
			var mockEnv = new Mock<IEnv>();
			var mockEnvironment = new Mock<IEnvironment>();
			var mockUser = new Mock<IUser>();
			mockEnv.Setup(env => env.Instance).Returns(mockEnvironment.Object);
			mockEnvironment.Setup(env => env.CurrentUser).Returns(mockUser.Object);
			using (EnvProxy.SetTemporaryEnvForTest(mockEnv.Object))
			{
				mockUser.Setup(user => user.Initials).Returns("ABC");
				var password = MasterPassword.GenerateForCurrentUserToday();
				AssertEquals("prefix is lowercase staff initials", "abc2ipz44", password);

				mockUser.Setup(user => user.Initials).Returns("AB");
				password = MasterPassword.GenerateForCurrentUserToday();
				AssertEquals("prefix is padded to fixed length", "ab0ezb5im", password);

				mockUser.Setup(user => user.Initials).Returns("ABCDE");
				password = MasterPassword.GenerateForCurrentUserToday();
				AssertEquals("prefix is truncated to fixed length", "abc2ipz44", password);
			}
		}
	}
}
