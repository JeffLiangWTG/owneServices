using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Escrow;
using Enterprise.Client.EDI.Escrow.Interfaces;
using Moq;

namespace ZClientEDI.Test.Escrow
{
	class DataRegistryProviderTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			dataRegistryProvider = new DataRegistryProvider();
		}

		DataRegistryProvider dataRegistryProvider;

		public class ProGetAssetDirectoryRegistryTest : DataRegistryProviderTest
		{
			IProGetAssetDirectoryRegistry RegistryProvider => dataRegistryProvider;

			public void TestAssetPathUrl()
			{
				CombineAssertions(() =>
				{
					Test("http://proget.wtg.zone/assets/Escrow/");
					Test("http://proget.wtg.zone/assets/EscrowSourceExport/");
					Test("https://proget.wtg.zone/assets/EscrowSourceExport/");
				});

				void Test(string value)
				{
					// Arrange
					using (EDIDataRegistry.Instance.ProGetAssetDirectoryPathUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value))
					{
						// Act
						var result = RegistryProvider.AssetPathUrl;

						// Assert
						AssertEquals(value, result);
					}
				}
			}

			public void TestApiKey()
			{
				CombineAssertions(() =>
				{
					Test("aaa");
					Test("bbb");
				});

				void Test(string value)
				{
					// Arrange
					using (EDIDataRegistry.Instance.ProGetAssetDirectoryApiKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value))
					{
						// Act
						var result = RegistryProvider.ApiKey;

						// Assert
						AssertEquals(value, result);
					}
				}
			}

			public void TestUserName()
			{
				CombineAssertions(() =>
				{
					Test("aaa");
					Test("bbb");
				});

				void Test(string value)
				{
					// Arrange
					using (EDIDataRegistry.Instance.ProGetAssetDirectoryUserName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value))
					{
						// Act
						var result = RegistryProvider.UserName;

						// Assert
						AssertEquals(value, result);
					}
				}
			}

			public void TestPassword()
			{
				CombineAssertions(() =>
				{
					Test("aaa");
					Test("bbb");
				});

				void Test(string value)
				{
					// Arrange
					using (EDIDataRegistry.Instance.ProGetAssetDirectoryPassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value))
					{
						// Act
						var result = RegistryProvider.Password;

						// Assert
						AssertEquals(value, result);
					}
				}
			}
		}

		public class RepositoryConfigurationRegistryTest : DataRegistryProviderTest
		{
			IRepositoryConfigurationRegistry RegistryProvider => dataRegistryProvider;

			public void TestMainRepository()
			{
				CombineAssertions(() =>
				{
					Test(Array.Empty<string>(), Array.Empty<IRepository>());
					Test(new[]
						{
							"https://devops.wisetechglobal.com/wtg/WiseCloud/_git/CCDS",
						},
						new[]
						{
							Mock.Of<IRepository>(repository =>
								repository.Repository == "https://devops.wisetechglobal.com/wtg/WiseCloud/_git/CCDS"
								&& repository.Path == "/"),
						});
					Test(new[]
						{
							"https://github.com/WiseTechGlobal/Glow;/DotNet",
						},
						new[]
						{
							Mock.Of<IRepository>(repository =>
								repository.Repository == "https://github.com/WiseTechGlobal/Glow"
								&& repository.Path == "/DotNet"),
						});
					Test(new[]
						{
							"https://github.com/WiseTechGlobal/Glow;/DotNet",
							"https://devops.wisetechglobal.com/wtg/WiseCloud/_git/CCDS",
						},
						new[]
						{
							Mock.Of<IRepository>(repository =>
								repository.Repository == "https://devops.wisetechglobal.com/wtg/WiseCloud/_git/CCDS"
								&& repository.Path == "/"),
							Mock.Of<IRepository>(repository =>
								repository.Repository == "https://github.com/WiseTechGlobal/Glow"
								&& repository.Path == "/DotNet"),
						});
				});

				void Test(string[] value, IRepository[] expected)
				{
					// Arrange
					using (EDIDataRegistry.Instance.MainRepositories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value))
					{
						// Act
						var result = RegistryProvider.MainRepositories;

						// Assert
						AssertContainsExactElementsInAnyOrder(new RepositoryComparer(), repository => $"{repository.Repository};{repository.Path}", expected, result);
					}
				}
			}

			class RepositoryComparer : IEqualityComparer<IRepository>
			{
				public bool Equals(IRepository x, IRepository y)
				{
					return x.Repository == y.Repository
							&& x.Path == y.Path;
				}

				public int GetHashCode(IRepository obj)
				{
					throw new NotImplementedException();
				}
			}
		}

		public class IncidentConfigurationRegistryTest : DataRegistryProviderTest
		{
			IIncidentConfigurationRegistry RegistryProvider => dataRegistryProvider;

			public void TestIncidentProduct()
			{
				CombineAssertions(() =>
				{
					Test("IST||CR9");
					Test("||");
					Test("AAA|BBB|CR9");
				});

				void Test(string value)
				{
					// Arrange
					using (EDIDataRegistry.Instance.EscrowIncidentConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value))
					{
						// Act
						var result = RegistryProvider.IncidentProduct;

						// Assert
						AssertEquals(value.Split('|')[0], result);
					}
				}
			}

			public void TestIncidentModule()
			{
				CombineAssertions(() =>
				{
					Test("IST||CR9");
					Test("||");
					Test("AAA|BBB|CR9");
				});

				void Test(string value)
				{
					// Arrange
					using (EDIDataRegistry.Instance.EscrowIncidentConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value))
					{
						// Act
						var result = RegistryProvider.IncidentModule;

						// Assert
						AssertEquals(value.Split('|')[1], result);
					}
				}
			}

			public void TestIncidentPriority()
			{
				CombineAssertions(() =>
				{
					Test("IST||CR9");
					Test("||");
					Test("AAA|BBB|CR9");
				});

				void Test(string value)
				{
					// Arrange
					using (EDIDataRegistry.Instance.EscrowIncidentConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value))
					{
						// Act
						var result = RegistryProvider.IncidentPriority;

						// Assert
						AssertEquals(value.Split('|')[2], result);
					}
				}
			}
		}

		public class GitAuthConfigurationRegistryTest : DataRegistryProviderTest
		{
			IGitAuthConfigurationRegistry RegistryProvider => dataRegistryProvider;

			public void TestGitHubAppId()
			{
				CombineAssertions(() =>
				{
					Test("dummy_id_1");
					Test("dummyId2");
				});

				void Test(string value)
				{
					// Arrange
					using (EDIDataRegistry.Instance.EscrowGitHubAppId.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value))
					{
						// Act
						var result = RegistryProvider.GitHubAppId;

						// Assert
						AssertEquals(value, result);
					}
				}
			}

			public void TestGitHubAppPrivateKey()
			{
				CombineAssertions(() =>
				{
					Test("privatekeytest123");
					Test("privatekeytestabc");
				});
				void Test(string value)
				{
					// Arrange
					using (EDIDataRegistry.Instance.EscrowGitHubAppPrivateKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, System.Text.Encoding.UTF8.GetBytes(value)))
					{
						// Act
						var result = RegistryProvider.GitHubAppPrivateKey;

						// Assert
						AssertEquals(value, result);
					}
				}
			}

			public void TestDevOpsPatToken()
			{
				CombineAssertions(() =>
				{
					Test("token123");
					Test("tokenabc");
				});
				void Test(string value)
				{
					// Arrange
					using (EDIDataRegistry.Instance.EscrowDevOpsToken.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value))
					{
						// Act
						var result = RegistryProvider.DevOpsPatToken;

						// Assert
						AssertEquals(value, result);
					}
				}
			}
		}
	}
}
