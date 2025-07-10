using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class EnvProxyTest : TestCase
	{
		public void TestInstance()
		{
			IEnvironment env = EnvProxy.Instance;
			AssertNotNull("The environment instance should not be null.", env);
			Assert("The default environment instance should be the WinFormsEnvironment.", EnvProxy.Instance is IWinFormsEnvironment);
			AssertEquals("The WinFormsEnvironment instance should be cached.", env, EnvProxy.Instance);
		}

		public class IsInternalSystemTest : TestCase
		{
			public void TestIsInternalSystem()
			{
				Test(true, true);
				Test(false, false);
				Test(null, null);

				void Test(bool? value, bool? expected)
				{
					// Arrange
					using (ObjectFactory.Substitute(Mock.Of<IProductRegistration>(
								registration => registration.Key == Mock.Of<IProductRegistrationKey>(
									key => key.IsInternalSystem == value))))
					{
						// Act
						var result = EnvProxy.IsInternalSystem;

						// Assert
						AssertEquals(expected, result);
					}
				}
			}

			public void TestSetIsInternalSystemForTest()
			{
				Test(true);
				Test(false);
				Test(null);

				void Test(bool? value)
				{
					// Arrange
					EnvProxy.SetIsInternalSystemForTest(value);

					// Act
					var result = EnvProxy.IsInternalSystem;

					// Assert
					AssertEquals(value, result);
				}
			}
		}

		public class IsUATSystemTest : TestCase
		{
			public void TestIsUATSystem()
			{
				Test(true, true);
				Test(false, false);

				void Test(bool value, bool expected)
				{
					// Arrange
					using (ObjectFactory.Substitute(Mock.Of<IProductRegistration>(
								registration => registration.IsWiseTechGlobalInternalUATSystem() == value)))
					{
						// Act
						var result = EnvProxy.IsUATSystem;

						// Assert
						AssertEquals(expected, result);
					}
				}
			}

			public void TestSetIsUATSystemForTest()
			{
				Test(true, true);
				Test(false, false);
				Test(null, false);

				void Test(bool? value, bool expected)
				{
					// Arrange
					EnvProxy.SetIsUATSystemForTest(value);

					// Act
					var result = EnvProxy.IsUATSystem;

					// Assert
					AssertEquals(expected, result);
				}
			}
		}
	}
}
