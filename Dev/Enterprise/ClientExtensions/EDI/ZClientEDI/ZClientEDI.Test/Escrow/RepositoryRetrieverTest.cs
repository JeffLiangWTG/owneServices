using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.IO;
using Enterprise.Client.EDI.Escrow;
using Enterprise.Client.EDI.Escrow.Interfaces;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;
using Octokit;

namespace ZClientEDI.Test.Escrow
{
	class RepositoryRetrieverTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			repositoryConfigurationRegistryMock = new Mock<IRepositoryConfigurationRegistry>();
			gitAdapterMock = new Mock<IGitAdapter>();
			gitHubInstallationTokenFactoryMock = new Mock<IGitHubInstallationTokenFactory>();
			gitHubInstallationTokenFactoryMock
				.Setup(x => x.GetToken())
				.Returns(new AccessToken("dummyGitHubToken", DateTimeOffset.Now.AddHours(1)));
			gitAuthConfigurationRegistryMock = new Mock<IGitAuthConfigurationRegistry>();
			gitAuthConfigurationRegistryMock.SetupGet(x => x.DevOpsPatToken).Returns("dummyDevOpsToken");
			repositoryRetriever = new RepositoryRetriever(repositoryConfigurationRegistryMock.Object, gitAdapterMock.Object, gitHubInstallationTokenFactoryMock.Object, gitAuthConfigurationRegistryMock.Object);
			loggerMock = new Mock<ILogger>();
		}

		Mock<IGitAdapter> gitAdapterMock;
		Mock<ILogger> loggerMock;
		Mock<IRepositoryConfigurationRegistry> repositoryConfigurationRegistryMock;
		Mock<IGitHubInstallationTokenFactory> gitHubInstallationTokenFactoryMock;
		Mock<IGitAuthConfigurationRegistry> gitAuthConfigurationRegistryMock;
		RepositoryRetriever repositoryRetriever;

		class MiscellaneousTest : RepositoryRetrieverTest
		{
			public void TestWrongParamsCall()
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new RepositoryRetriever(null, gitAdapterMock.Object, gitHubInstallationTokenFactoryMock.Object, gitAuthConfigurationRegistryMock.Object));
				AssertEquals("repositoryConfigurationRegistry", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new RepositoryRetriever(repositoryConfigurationRegistryMock.Object, null, gitHubInstallationTokenFactoryMock.Object, gitAuthConfigurationRegistryMock.Object));
				AssertEquals("gitAdapter", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new RepositoryRetriever(repositoryConfigurationRegistryMock.Object, gitAdapterMock.Object, null, gitAuthConfigurationRegistryMock.Object));
				AssertEquals("gitHubInstallationTokenFactory", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new RepositoryRetriever(repositoryConfigurationRegistryMock.Object, gitAdapterMock.Object, gitHubInstallationTokenFactoryMock.Object, null));
				AssertEquals("gitAuthConfigurationRegistry", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => repositoryRetriever.DownloadRepositories(null, loggerMock.Object, Mock.Of<IWorkingDirectory>()));
				AssertEquals("workingDirectory", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => repositoryRetriever.DownloadRepositories(Mock.Of<IWorkingDirectory>(directory => directory.DirectoryName == string.Empty), null, Mock.Of<IWorkingDirectory>()));
				AssertEquals("logger", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => repositoryRetriever.DownloadRepositories(Mock.Of<IWorkingDirectory>(directory => directory.DirectoryName == string.Empty), loggerMock.Object, null));
				AssertEquals("gitDirectory", result.ParamName);
			}

			[ExpectNoExceptions]
			public void TestGitHubClientMethodsAreCalledIfRepoSourceIsGitHub()
			{
				// Arrange
				using var tempFolder = new TempDirectory(Temp.GetNewTempSubdirectory());
				using var gitFolder = new TempDirectory(Temp.GetNewTempSubdirectory());
				var workingDirectoryMock = Mock.Of<IWorkingDirectory>(directory => directory.DirectoryName == tempFolder.DirectoryName);
				var gitDirectoryMock = Mock.Of<IWorkingDirectory>(directory => directory.DirectoryName == gitFolder.DirectoryName);
				var mainRepositories = new[]
				{
					Mock.Of<IRepository>(repository => repository.Repository == "https://github.com/WiseTechGlobal/WTG.RtfConverter" && repository.Path == "/"),
				};
				repositoryConfigurationRegistryMock
					.SetupGet(registry => registry.MainRepositories)
					.Returns(mainRepositories);

				// Act
				repositoryRetriever.DownloadRepositories(workingDirectoryMock, loggerMock.Object, gitDirectoryMock);

				// Assert
				gitHubInstallationTokenFactoryMock.Verify(x => x.GetToken(), Times.Once);
				gitHubInstallationTokenFactoryMock.VerifyNoOtherCalls();
			}
		}

		class RecursiveLoadingTest : RepositoryRetrieverTest
		{
			public void TestCreatesRecursiveDestinationFolders()
			{
				Test("One main repo with one dependency",
					new[]
					{
						Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev"
															&& repository.Path == "/"),
					},
					new[]
					{
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev",
							new[]
							{
								(@"src\CargoWise\Dev", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF""", }),
							}),
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF",
							Array.Empty<(string, string[])>()),
					},
					new[]
					{
						@"\src",
						@"\src\CargoWise",
						@"\src\CargoWise\Dev",
						@"\src\CargoWise\Warehouse.RF",
					});

				Test("One main repo with one dependency with one dependency",
					new[]
					{
						Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev"
															&& repository.Path == "/"),
					},
					new[]
					{
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev",
							new[]
							{
								(@"src\CargoWise\Dev", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF""", }),
							}),
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF",
							new[]
							{
								(@"src\CargoWise\Warehouse.RF", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared""", }),
							}),
						("https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared",
							Array.Empty<(string, string[])>()),
					},
					new[]
					{
						@"\src",
						@"\src\CargoWise",
						@"\src\CargoWise\Dev",
						@"\src\CargoWise\Warehouse.RF",
						@"\src\RefDataRepo",
						@"\src\RefDataRepo\Shared",
					});

				Test("One main repo with one dependency with two build xmls on different levels, we stop on the first",
					new[]
					{
						Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev"
															&& repository.Path == "/"),
					},
					new[]
					{
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev",
							new[]
							{
								(@"src\CargoWise\Dev", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF""", }),
							}),
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF",
							new[]
							{
								(@"src\CargoWise\Warehouse.RF", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared""", }),
								(@"src\CargoWise\Warehouse.RF\Warehouse.RF.SpecialFileHandler", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/CargoWise/_git/Shared""", }),
							}),
						("https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared",
							Array.Empty<(string, string[])>()),
					},
					new[]
					{
						@"\src",
						@"\src\CargoWise",
						@"\src\CargoWise\Dev",
						@"\src\CargoWise\Warehouse.RF",
						@"\src\CargoWise\Warehouse.RF\Warehouse.RF.SpecialFileHandler",
						@"\src\RefDataRepo",
						@"\src\RefDataRepo\Shared",
					});

				Test("One main repo with two dependencies and one similar dependency each",
					new[]
					{
						Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev"
															&& repository.Path == "/"),
					},
					new[]
					{
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev",
							new[]
							{
								(@"src\CargoWise\Dev", new[]
								{
									@"Repository=""https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF""",
									@"Repository=""https://devops.wisetechglobal.com/wtg/IdentityAndSecurity/_git/IdentityAndSecurity"" Path=""/AWSCertificateIntegration""",
								}),
							}),

						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF",
							new[]
							{
								(@"src\CargoWise\Warehouse.RF", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared""", }),
							}),
						("https://devops.wisetechglobal.com/wtg/IdentityAndSecurity/_git/IdentityAndSecurity",
							new[]
							{
								(@"src\IdentityAndSecurity\IdentityAndSecurity\AWSCertificateIntegration", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared""", }),
							}),

						("https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared",
							Array.Empty<(string, string[])>()),
					},
					new[]
					{
						@"\src",
						@"\src\CargoWise",
						@"\src\CargoWise\Dev",
						@"\src\CargoWise\Warehouse.RF",
						@"\src\RefDataRepo",
						@"\src\RefDataRepo\Shared",
						@"\src\IdentityAndSecurity",
						@"\src\IdentityAndSecurity\IdentityAndSecurity",
						@"\src\IdentityAndSecurity\IdentityAndSecurity\AWSCertificateIntegration",
					});

				Test("One main repo with one dependency with not included buildXmls",
					new[]
					{
						Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev"
															&& repository.Path == "/"),
					},
					new[]
					{
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev",
							new[]
							{
								(@"src\CargoWise\Dev", new[]
								{
									@"Repository=""https://devops.wisetechglobal.com/wtg/IdentityAndSecurity/_git/IdentityAndSecurity"" Path=""/AWSCertificateIntegration""",
								}),
							}),
						("https://devops.wisetechglobal.com/wtg/IdentityAndSecurity/_git/IdentityAndSecurity",
							new[]
							{
								(@"src\IdentityAndSecurity\IdentityAndSecurity", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared""", }),
								(@"src\IdentityAndSecurity\IdentityAndSecurity\AzureCertificateIntegration", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared""", }),
								(@"src\IdentityAndSecurity\IdentityAndSecurity\AWSCertificateIntegration", Array.Empty<string>()),
							}),
					},
					new[]
					{
						@"\src",
						@"\src\CargoWise",
						@"\src\CargoWise\Dev",
						@"\src\IdentityAndSecurity",
						@"\src\IdentityAndSecurity\IdentityAndSecurity",
						@"\src\IdentityAndSecurity\IdentityAndSecurity\AzureCertificateIntegration",
						@"\src\IdentityAndSecurity\IdentityAndSecurity\AWSCertificateIntegration",
					});

				Test("One main repo in Github with one dependency in Devops",
					new[]
					{
						Mock.Of<IRepository>(repository => repository.Repository == "https://github.com/WiseTechGlobal/WTG.RtfConverter" && repository.Path == "/"),
					},
					new[]
					{
						("https://github.com/WiseTechGlobal/WTG.RtfConverter",
							new[]
							{
								(@"src\WiseTechGlobal\WTG.RtfConverter", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/CargoWise/_git/Shared"" Path=""/WTG.Playwright""", }),
							}),
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Shared",
							new[]
							{
								(@"src\CargoWise\Shared\WTG.Playwright", Array.Empty<string>()),
							}),
					},
					new[]
					{
						@"\src",
						@"\src\WiseTechGlobal",
						@"\src\WiseTechGlobal\WTG.RtfConverter",
						@"\src\CargoWise",
						@"\src\CargoWise\Shared",
						@"\src\CargoWise\Shared\WTG.Playwright",
					});

				Test("One main repo in Github with one dependency in Github",
					new[]
					{
						Mock.Of<IRepository>(repository => repository.Repository == "https://github.com/WiseTechGlobal/WTG.RtfConverter" && repository.Path == "/"),
					},
					new[]
					{
						("https://github.com/WiseTechGlobal/WTG.RtfConverter",
							new[]
							{
								(@"src\WiseTechGlobal\WTG.RtfConverter", new[] { @"Repository=""https://github.com/WiseTechGlobal/WTG.WindowMonitoring""", }),
							}),
						("https://github.com/WiseTechGlobal/WTG.WindowMonitoring",
							Array.Empty<(string, string[])>()),
					},
					new[]
					{
						@"\src",
						@"\src\WiseTechGlobal",
						@"\src\WiseTechGlobal\WTG.RtfConverter",
						@"\src\WiseTechGlobal\WTG.WindowMonitoring",
					});

				Test("dependency path without slash at the start",
					new[]
					{
						Mock.Of<IRepository>(repository => repository.Repository == "https://github.com/WiseTechGlobal/WTG.RtfConverter" && repository.Path == "/"),
					},
					new[]
					{
						("https://github.com/WiseTechGlobal/WTG.RtfConverter",
							new[]
							{
								(@"src\WiseTechGlobal\WTG.RtfConverter", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/CargoWise/_git/Shared"" Path=""WTG.Playwright""", }),
							}),
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Shared",
							new[]
							{
								(@"src\CargoWise\Shared\WTG.Playwright", Array.Empty<string>()),
							}),
					},
					new[]
					{
						@"\src",
						@"\src\WiseTechGlobal",
						@"\src\WiseTechGlobal\WTG.RtfConverter",
						@"\src\CargoWise",
						@"\src\CargoWise\Shared",
						@"\src\CargoWise\Shared\WTG.Playwright",
					});

				void Test(
					string title,
					IRepository[] mainRepositories,
					(string repository, (string path, string[] dependencies)[] buildXmls)[] recursiveRepos,
					string[] expected)
				{
					// Arrange
					using var tempFolder = new TempDirectory(Temp.GetNewTempSubdirectory());
					using var gitFolder = new TempDirectory(Temp.GetNewTempSubdirectory());
					var workingDirectoryMock = Mock.Of<IWorkingDirectory>(directory => directory.DirectoryName == tempFolder.DirectoryName);
					var gitDirectoryMock = Mock.Of<IWorkingDirectory>(directory => directory.DirectoryName == gitFolder.DirectoryName);
					repositoryConfigurationRegistryMock
						.SetupGet(registry => registry.MainRepositories)
						.Returns(mainRepositories);

					foreach (var recursiveRepo in recursiveRepos)
					{
						var cloneUrl = recursiveRepo.repository;

						gitAdapterMock
							.Setup(adapter => adapter.Clone(cloneUrl, It.IsAny<string>(), It.IsAny<ILogger>(), It.Is<string>(s => s.StartsWith("-c http.extraHeader=\"Authorization: Basic"))))
							.Callback(() =>
							{
								foreach (var (path, dependencies) in recursiveRepo.buildXmls)
								{
									CreateBuildXml(Path.Combine(tempFolder.DirectoryName, path), dependencies);
								}
							});
					}

					// Act
					repositoryRetriever.DownloadRepositories(workingDirectoryMock, loggerMock.Object, gitDirectoryMock);

					// Assert
					var result = Directory.GetDirectories(tempFolder.DirectoryName, "*", SearchOption.AllDirectories)
						.Select(s => s.Replace(tempFolder.DirectoryName, string.Empty));
					AssertContainsExactElementsInAnyOrder(title, StringComparer.OrdinalIgnoreCase, expected, result);
				}
			}

			public void TestClonesRecursiveRepositories()
			{
				Test("One main repo with one dependency",
					new[]
					{
						Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev"
															&& repository.Path == "/"),
					},
					new[]
					{
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev",
							new[]
							{
								(@"src\CargoWise\Dev", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF""", }),
							}),
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF",
							Array.Empty<(string, string[])>()),
					},
					new[]
					{
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev", @"src\CargoWise"),
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF", @"src\CargoWise"),
					});

				Test("One main repo with one dependency with one dependency",
					new[]
					{
						Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev"
															&& repository.Path == "/"),
					},
					new[]
					{
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev",
							new[]
							{
								(@"src\CargoWise\Dev", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF""", }),
							}),
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF",
							new[]
							{
								(@"src\CargoWise\Warehouse.RF", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared""", }),
							}),
						("https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared",
							Array.Empty<(string, string[])>()),
					},
					new[]
					{
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev", @"src\CargoWise"),
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF", @"src\CargoWise"),
						("https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared", @"src\RefDataRepo"),
					});

				Test("One main repo with one dependency with two build xmls on different levels, we stop on the first",
					new[]
					{
						Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev"
															&& repository.Path == "/"),
					},
					new[]
					{
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev",
							new[]
							{
								(@"src\CargoWise\Dev", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF""", }),
							}),
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF",
							new[]
							{
								(@"src\CargoWise\Warehouse.RF", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared""", }),
								(@"src\CargoWise\Warehouse.RF\Warehouse.RF.SpecialFileHandler", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/CargoWise/_git/Shared""", }),
							}),
						("https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared",
							Array.Empty<(string, string[])>()),
					},
					new[]
					{
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev", @"src\CargoWise"),
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF", @"src\CargoWise"),
						("https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared", @"src\RefDataRepo"),
					});

				Test("One main repo with two dependencies and one similar dependency each",
					new[]
					{
						Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev"
															&& repository.Path == "/"),
					},
					new[]
					{
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev",
							new[]
							{
								(@"src\CargoWise\Dev", new[]
								{
									@"Repository=""https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF""",
									@"Repository=""https://devops.wisetechglobal.com/wtg/IdentityAndSecurity/_git/IdentityAndSecurity"" Path=""/AWSCertificateIntegration""",
								}),
							}),

						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF",
							new[]
							{
								(@"src\CargoWise\Warehouse.RF", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared""", }),
							}),
						("https://devops.wisetechglobal.com/wtg/IdentityAndSecurity/_git/IdentityAndSecurity",
							new[]
							{
								(@"src\IdentityAndSecurity\IdentityAndSecurity\AWSCertificateIntegration", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared""", }),
							}),

						("https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared",
							Array.Empty<(string, string[])>()),
					},
					new[]
					{
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev", @"src\CargoWise"),
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF", @"src\CargoWise"),
						("https://devops.wisetechglobal.com/wtg/IdentityAndSecurity/_git/IdentityAndSecurity", @"src\IdentityAndSecurity"),
						("https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared", @"src\RefDataRepo"),
					});

				Test("One main repo with one dependency with not included buildXmls",
					new[]
					{
						Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev"
															&& repository.Path == "/"),
					},
					new[]
					{
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev",
							new[]
							{
								(@"src\CargoWise\Dev", new[]
								{
									@"Repository=""https://devops.wisetechglobal.com/wtg/IdentityAndSecurity/_git/IdentityAndSecurity"" Path=""/AWSCertificateIntegration""",
								}),
							}),
						("https://devops.wisetechglobal.com/wtg/IdentityAndSecurity/_git/IdentityAndSecurity",
							new[]
							{
								(@"src\IdentityAndSecurity\IdentityAndSecurity", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared""", }),
								(@"src\IdentityAndSecurity\IdentityAndSecurity\AzureCertificateIntegration", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared""", }),
								(@"src\IdentityAndSecurity\IdentityAndSecurity\AWSCertificateIntegration", Array.Empty<string>()),
							}),
					},
					new[]
					{
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev", @"src\CargoWise"),
						("https://devops.wisetechglobal.com/wtg/IdentityAndSecurity/_git/IdentityAndSecurity", @"src\IdentityAndSecurity"),
					});

				Test("One main repo in Github with one dependency in Github",
					new[]
					{
						Mock.Of<IRepository>(repository => repository.Repository == "https://github.com/WiseTechGlobal/WTG.RtfConverter" && repository.Path == "/"),
					},
					new[]
					{
						("https://github.com/WiseTechGlobal/WTG.RtfConverter",
							new[]
							{
								(@"src\WiseTechGlobal/WTG.RtfConverter", new[] { @"Repository=""https://github.com/WiseTechGlobal/WTG.WindowMonitoring""", }),
							}),
						("https://github.com/WiseTechGlobal/WTG.WindowMonitoring",
							Array.Empty<(string, string[])>()),
					},
					new[]
					{
						("https://github.com/WiseTechGlobal/WTG.RtfConverter", @"src\WiseTechGlobal"),
						("https://github.com/WiseTechGlobal/WTG.WindowMonitoring", @"src\WiseTechGlobal"),
					});

				Test("One main repo in Github with one dependency in Devops",
					new[]
					{
						Mock.Of<IRepository>(repository => repository.Repository == "https://github.com/WiseTechGlobal/WTG.RtfConverter" && repository.Path == "/"),
					},
					new[]
					{
						("https://github.com/WiseTechGlobal/WTG.RtfConverter",
							new[]
							{
								(@"src\WiseTechGlobal/WTG.RtfConverter", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared""", }),
							}),
						("https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared",
							Array.Empty<(string, string[])>()),
					},
					new[]
					{
						("https://github.com/WiseTechGlobal/WTG.RtfConverter", @"src\WiseTechGlobal"),
						("https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared", @"src\RefDataRepo"),
					});

				void Test(
					string title,
					IRepository[] mainRepositories,
					(string repository, (string path, string[] dependencies)[] buildXmls)[] recursiveRepos,
					(string repoitory, string path)[] expected)
				{
					// Arrange
					gitAdapterMock.Invocations.Clear();
					using var tempFolder = new TempDirectory(Temp.GetNewTempSubdirectory());
					using var gitFolder = new TempDirectory(Temp.GetNewTempSubdirectory());
					var workingDirectoryMock = Mock.Of<IWorkingDirectory>(directory => directory.DirectoryName == tempFolder.DirectoryName);
					var gitDirectoryMock = Mock.Of<IWorkingDirectory>(directory => directory.DirectoryName == gitFolder.DirectoryName);
					repositoryConfigurationRegistryMock
						.SetupGet(registry => registry.MainRepositories)
						.Returns(mainRepositories);

					foreach (var recursiveRepo in recursiveRepos)
					{
						var cloneUrl = recursiveRepo.repository;
						gitAdapterMock
							.Setup(adapter => adapter.Clone(cloneUrl, It.IsAny<string>(), It.IsAny<ILogger>(), It.Is<string>(s => s.StartsWith("-c http.extraHeader=\"Authorization: Basic"))))
							.Callback(() =>
							{
								foreach (var (path, dependencies) in recursiveRepo.buildXmls)
								{
									CreateBuildXml(Path.Combine(tempFolder.DirectoryName, path), dependencies);
								}
							});
					}

					// Act
					repositoryRetriever.DownloadRepositories(workingDirectoryMock, loggerMock.Object, gitDirectoryMock);

					// Assert
					AssertNoExceptionThrown(title,
						() =>
						{
							foreach (var (repository, path) in expected)
							{
								var folder = Path.Combine(tempFolder.DirectoryName, path);
								gitAdapterMock
									.Verify(adapter => adapter.Clone(repository, folder, loggerMock.Object, It.Is<string>(s => s.StartsWith("-c http.extraHeader=\"Authorization: Basic"))),
										Times.Once);
								gitAdapterMock.VerifySet(x => x.GitDirectory = It.IsAny<IWorkingDirectory>(), Times.Once());
							}

							gitAdapterMock.VerifyNoOtherCalls();
						});
				}
			}

			public void TestReturnsListOfRetrievedRepositories()
			{
				Test("One main repo with one dependency",
					new[]
					{
						Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev"
															&& repository.Path == "/"),
					},
					new[]
					{
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev",
							new[]
							{
								(@"src\CargoWise\Dev", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF""", }),
							}),
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF",
							Array.Empty<(string, string[])>()),
					},
					new[]
					{
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev", "/"),
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF", "/"),
					});

				Test("One main repo with one dependency with one dependency",
					new[]
					{
						Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev"
															&& repository.Path == "/"),
					},
					new[]
					{
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev",
							new[]
							{
								(@"src\CargoWise\Dev", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF""", }),
							}),
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF",
							new[]
							{
								(@"src\CargoWise\Warehouse.RF", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared""", }),
							}),
						("https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared",
							Array.Empty<(string, string[])>()),
					},
					new[]
					{
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev", "/"),
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF", "/"),
						("https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared", "/"),
					});

				Test("One main repo with one dependency with two build xmls on different levels, we stop on the first",
					new[]
					{
						Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev"
															&& repository.Path == "/"),
					},
					new[]
					{
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev",
							new[]
							{
								(@"src\CargoWise\Dev", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF""", }),
							}),
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF",
							new[]
							{
								(@"src\CargoWise\Warehouse.RF", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared""", }),
								(@"src\CargoWise\Warehouse.RF\Warehouse.RF.SpecialFileHandler", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/CargoWise/_git/Shared""", }),
							}),
						("https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared",
							Array.Empty<(string, string[])>()),
					},
					new[]
					{
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev", "/"),
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF", "/"),
						("https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared", "/"),
					});

				Test("One main repo with two dependencies and one similar dependency each",
					new[]
					{
						Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev"
															&& repository.Path == "/"),
					},
					new[]
					{
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev",
							new[]
							{
								(@"src\CargoWise\Dev", new[]
								{
									@"Repository=""https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF""",
									@"Repository=""https://devops.wisetechglobal.com/wtg/IdentityAndSecurity/_git/IdentityAndSecurity"" Path=""/AWSCertificateIntegration""",
								}),
							}),

						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF",
							new[]
							{
								(@"src\CargoWise\Warehouse.RF", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared""", }),
							}),
						("https://devops.wisetechglobal.com/wtg/IdentityAndSecurity/_git/IdentityAndSecurity",
							new[]
							{
								(@"src\IdentityAndSecurity\IdentityAndSecurity\AWSCertificateIntegration", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared""", }),
							}),

						("https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared",
							Array.Empty<(string, string[])>()),
					},
					new[]
					{
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev", "/"),
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF", "/"),
						("https://devops.wisetechglobal.com/wtg/IdentityAndSecurity/_git/IdentityAndSecurity", "/AWSCertificateIntegration"),
						("https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared", "/"),
					});

				Test("One main repo with one dependency with not included buildXmls",
					new[]
					{
						Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev"
															&& repository.Path == "/"),
					},
					new[]
					{
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev",
							new[]
							{
								(@"src\CargoWise\Dev", new[]
								{
									@"Repository=""https://devops.wisetechglobal.com/wtg/IdentityAndSecurity/_git/IdentityAndSecurity"" Path=""/AWSCertificateIntegration""",
								}),
							}),
						("https://devops.wisetechglobal.com/wtg/IdentityAndSecurity/_git/IdentityAndSecurity",
							new[]
							{
								(@"src\IdentityAndSecurity\IdentityAndSecurity", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared""", }),
								(@"src\IdentityAndSecurity\IdentityAndSecurity\AzureCertificateIntegration", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared""", }),
								(@"src\IdentityAndSecurity\IdentityAndSecurity\AWSCertificateIntegration", Array.Empty<string>()),
							}),
					},
					new[]
					{
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev", "/"),
						("https://devops.wisetechglobal.com/wtg/IdentityAndSecurity/_git/IdentityAndSecurity", "/AWSCertificateIntegration"),
					});

				Test("One main repo in Github with one dependency in Github",
					new[]
					{
						Mock.Of<IRepository>(repository => repository.Repository == "https://github.com/WiseTechGlobal/WTG.RtfConverter" && repository.Path == "/"),
					},
					new[]
					{
						("https://github.com/WiseTechGlobal/WTG.RtfConverter",
							new[]
							{
								(@"src\WiseTechGlobal\WTG.RtfConverter", new[] { @"Repository=""https://github.com/WiseTechGlobal/WTG.WindowMonitoring""", }),
							}),
						("https://github.com/WiseTechGlobal/WTG.WindowMonitoring",
							Array.Empty<(string, string[])>()),
					},
					new[]
					{
						("https://github.com/WiseTechGlobal/WTG.RtfConverter", "/"),
						("https://github.com/WiseTechGlobal/WTG.WindowMonitoring", "/"),
					});

				Test("One main repo in Github with one dependency in Devops",
					new[]
					{
						Mock.Of<IRepository>(repository => repository.Repository == "https://github.com/WiseTechGlobal/WTG.RtfConverter" && repository.Path == "/"),
					},
					new[]
					{
						("https://github.com/WiseTechGlobal/WTG.RtfConverter",
							new[]
							{
								(@"src\WiseTechGlobal\WTG.RtfConverter", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared""", }),
							}),
						("https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared",
							Array.Empty<(string, string[])>()),
					},
					new[]
					{
						("https://github.com/WiseTechGlobal/WTG.RtfConverter", "/"),
						("https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared", "/"),
					});

				void Test(
					string title,
					IRepository[] mainRepositories,
					(string repository, (string path, string[] dependencies)[] buildXmls)[] recursiveRepos,
					(string repoitory, string path)[] expected)
				{
					// Arrange
					gitAdapterMock.Invocations.Clear();
					using var tempFolder = new TempDirectory(Temp.GetNewTempSubdirectory());
					using var gitFolder = new TempDirectory(Temp.GetNewTempSubdirectory());
					var workingDirectoryMock = Mock.Of<IWorkingDirectory>(directory => directory.DirectoryName == tempFolder.DirectoryName);
					var gitDirectoryMock = Mock.Of<IWorkingDirectory>(directory => directory.DirectoryName == gitFolder.DirectoryName);
					repositoryConfigurationRegistryMock
						.SetupGet(registry => registry.MainRepositories)
						.Returns(mainRepositories);

					foreach (var recursiveRepo in recursiveRepos)
					{
						var cloneUrl = recursiveRepo.repository;
						gitAdapterMock
							.Setup(adapter => adapter.Clone(cloneUrl, It.IsAny<string>(), It.IsAny<ILogger>(), It.Is<string>(s => s.StartsWith("-c http.extraHeader=\"Authorization: Basic"))))
							.Callback(() =>
							{
								foreach (var (path, dependencies) in recursiveRepo.buildXmls)
								{
									CreateBuildXml(Path.Combine(tempFolder.DirectoryName, path), dependencies);
								}
							});
					}

					// Act
					var result = repositoryRetriever.DownloadRepositories(workingDirectoryMock, loggerMock.Object, gitDirectoryMock)
						.Select(repository => (repository.Repository, repository.Path));

					// Assert
					AssertContainsExactElementsInAnyOrder(title, expected, result);
				}
			}

			public void TestLoggingRecursiveRepositories()
			{
				Test("One main repo with one dependency",
					new[]
					{
						Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev"
															&& repository.Path == "/"),
					},
					new[]
					{
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev",
							new[]
							{
								(@"src\CargoWise\Dev", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF""", }),
							}),
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF",
							Array.Empty<(string, string[])>()),
					},
					new[]
					{
						(LogType.Information, @"> Cloning repository \[1\/1, https:\/\/devops\.wisetechglobal\.com\/wtg\/CargoWise\/_git\/Dev\]\.\.\."),
						(LogType.Information, @"-> Cloning repository \[1\/1, https:\/\/devops\.wisetechglobal\.com\/wtg\/CargoWise\/_git\/Warehouse.RF\]\.\.\."),
						(LogType.Information, @"-> Cloning repository \[1\/1, https:\/\/devops\.wisetechglobal\.com\/wtg\/CargoWise\/_git\/Warehouse.RF\] finished in \d+:\d{2}:\d{2}:\d{2}\.\d+\."),
						(LogType.Information, @"> Cloning repository \[1\/1, https:\/\/devops\.wisetechglobal\.com\/wtg\/CargoWise\/_git\/Dev\] finished in \d+:\d{2}:\d{2}:\d{2}\.\d+\."),
					});

				Test("One main repo with one dependency with one dependency",
					new[]
					{
						Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev"
															&& repository.Path == "/"),
					},
					new[]
					{
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev",
							new[]
							{
								(@"src\CargoWise\Dev", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF""", }),
							}),
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF",
							new[]
							{
								(@"src\CargoWise\Warehouse.RF", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared""", }),
							}),
						("https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared",
							Array.Empty<(string, string[])>()),
					},
					new[]
					{
						(LogType.Information, @"> Cloning repository \[1\/1, https:\/\/devops\.wisetechglobal\.com\/wtg\/CargoWise\/_git\/Dev\]\.\.\."),
						(LogType.Information, @"-> Cloning repository \[1\/1, https:\/\/devops\.wisetechglobal\.com\/wtg\/CargoWise\/_git\/Warehouse.RF\]\.\.\."),
						(LogType.Information, @"-> Cloning repository \[1\/1, https:\/\/devops\.wisetechglobal\.com\/wtg\/RefDataRepo\/_git\/Shared\]\.\.\."),
						(LogType.Information, @"-> Cloning repository \[1\/1, https:\/\/devops\.wisetechglobal\.com\/wtg\/RefDataRepo\/_git\/Shared\] finished in \d+:\d{2}:\d{2}:\d{2}\.\d+\."),
						(LogType.Information, @"-> Cloning repository \[1\/1, https:\/\/devops\.wisetechglobal\.com\/wtg\/CargoWise\/_git\/Warehouse.RF\] finished in \d+:\d{2}:\d{2}:\d{2}\.\d+\."),
						(LogType.Information, @"> Cloning repository \[1\/1, https:\/\/devops\.wisetechglobal\.com\/wtg\/CargoWise\/_git\/Dev\] finished in \d+:\d{2}:\d{2}:\d{2}\.\d+\."),
					});

				Test("One main repo with one dependency with two build xmls on different levels, we stop on the first",
					new[]
					{
						Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev"
															&& repository.Path == "/"),
					},
					new[]
					{
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev",
							new[]
							{
								(@"src\CargoWise\Dev", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF""", }),
							}),
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF",
							new[]
							{
								(@"src\CargoWise\Warehouse.RF", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared""", }),
								(@"src\CargoWise\Warehouse.RF\Warehouse.RF.SpecialFileHandler", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/CargoWise/_git/Shared""", }),
							}),
						("https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared",
							Array.Empty<(string, string[])>()),
					},
					new[]
					{
						(LogType.Information, @"> Cloning repository \[1\/1, https:\/\/devops\.wisetechglobal\.com\/wtg\/CargoWise\/_git\/Dev\]\.\.\."),
						(LogType.Information, @"-> Cloning repository \[1\/1, https:\/\/devops\.wisetechglobal\.com\/wtg\/CargoWise\/_git\/Warehouse.RF\]\.\.\."),
						(LogType.Information, @"-> Cloning repository \[1\/1, https:\/\/devops\.wisetechglobal\.com\/wtg\/RefDataRepo\/_git\/Shared\]\.\.\."),
						(LogType.Information, @"-> Cloning repository \[1\/1, https:\/\/devops\.wisetechglobal\.com\/wtg\/RefDataRepo\/_git\/Shared\] finished in \d+:\d{2}:\d{2}:\d{2}\.\d+\."),
						(LogType.Information, @"-> Cloning repository \[1\/1, https:\/\/devops\.wisetechglobal\.com\/wtg\/CargoWise\/_git\/Warehouse.RF\] finished in \d+:\d{2}:\d{2}:\d{2}\.\d+\."),
						(LogType.Information, @"> Cloning repository \[1\/1, https:\/\/devops\.wisetechglobal\.com\/wtg\/CargoWise\/_git\/Dev\] finished in \d+:\d{2}:\d{2}:\d{2}\.\d+\."),
					});

				Test("One main repo with two dependencies and one similar dependency each",
					new[]
					{
						Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev"
															&& repository.Path == "/"),
					},
					new[]
					{
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev",
							new[]
							{
								(@"src\CargoWise\Dev", new[]
								{
									@"Repository=""https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF""",
									@"Repository=""https://devops.wisetechglobal.com/wtg/IdentityAndSecurity/_git/IdentityAndSecurity"" Path=""/AWSCertificateIntegration""",
								}),
							}),

						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Warehouse.RF",
							new[]
							{
								(@"src\CargoWise\Warehouse.RF", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared""", }),
							}),
						("https://devops.wisetechglobal.com/wtg/IdentityAndSecurity/_git/IdentityAndSecurity",
							new[]
							{
								(@"src\IdentityAndSecurity\IdentityAndSecurity\AWSCertificateIntegration", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared""", }),
							}),

						("https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared",
							Array.Empty<(string, string[])>()),
					},
					new[]
					{
						(LogType.Information, @"> Cloning repository \[1\/1, https:\/\/devops\.wisetechglobal\.com\/wtg\/CargoWise\/_git\/Dev\]\.\.\."),
						(LogType.Information, @"-> Cloning repository \[1\/2, https:\/\/devops\.wisetechglobal\.com\/wtg\/CargoWise\/_git\/Warehouse.RF\]\.\.\."),
						(LogType.Information, @"-> Cloning repository \[1\/1, https:\/\/devops\.wisetechglobal\.com\/wtg\/RefDataRepo\/_git\/Shared\]\.\.\."),
						(LogType.Information, @"-> Cloning repository \[1\/1, https:\/\/devops\.wisetechglobal\.com\/wtg\/RefDataRepo\/_git\/Shared\] finished in \d+:\d{2}:\d{2}:\d{2}\.\d+\."),
						(LogType.Information, @"-> Cloning repository \[1\/2, https:\/\/devops\.wisetechglobal\.com\/wtg\/CargoWise\/_git\/Warehouse.RF\] finished in \d+:\d{2}:\d{2}:\d{2}\.\d+\."),
						(LogType.Information, @"-> Cloning repository \[2\/2, https:\/\/devops\.wisetechglobal\.com\/wtg\/IdentityAndSecurity\/_git\/IdentityAndSecurity\]\.\.\."),
						(LogType.Information, @"-> Skipped already cloned repository \[1\/1, https:\/\/devops\.wisetechglobal\.com\/wtg\/RefDataRepo\/_git\/Shared\]\."),
						(LogType.Information, @"-> Cloning repository \[2\/2, https:\/\/devops\.wisetechglobal\.com\/wtg\/IdentityAndSecurity\/_git\/IdentityAndSecurity\] finished in \d+:\d{2}:\d{2}:\d{2}\.\d+\."),
						(LogType.Information, @"> Cloning repository \[1\/1, https:\/\/devops\.wisetechglobal\.com\/wtg\/CargoWise\/_git\/Dev\] finished in \d+:\d{2}:\d{2}:\d{2}\.\d+\."),
					});

				Test("One main repo with one dependency with not included buildXmls",
					new[]
					{
						Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev"
															&& repository.Path == "/"),
					},
					new[]
					{
						("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev",
							new[]
							{
								(@"src\CargoWise\Dev", new[]
								{
									@"Repository=""https://devops.wisetechglobal.com/wtg/IdentityAndSecurity/_git/IdentityAndSecurity"" Path=""/AWSCertificateIntegration""",
								}),
							}),
						("https://devops.wisetechglobal.com/wtg/IdentityAndSecurity/_git/IdentityAndSecurity",
							new[]
							{
								(@"src\IdentityAndSecurity\IdentityAndSecurity", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared""", }),
								(@"src\IdentityAndSecurity\IdentityAndSecurity\AzureCertificateIntegration", new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/Shared""", }),
								(@"src\IdentityAndSecurity\IdentityAndSecurity\AWSCertificateIntegration", Array.Empty<string>()),
							}),
					},
					new[]
					{
						(LogType.Information, @"> Cloning repository \[1\/1, https:\/\/devops\.wisetechglobal\.com\/wtg\/CargoWise\/_git\/Dev\]\.\.\."),
						(LogType.Information, @"-> Cloning repository \[1\/1, https:\/\/devops\.wisetechglobal\.com\/wtg\/IdentityAndSecurity\/_git\/IdentityAndSecurity\]\.\.\."),
						(LogType.Information, @"-> Cloning repository \[1\/1, https:\/\/devops\.wisetechglobal\.com\/wtg\/IdentityAndSecurity\/_git\/IdentityAndSecurity\] finished in \d+:\d{2}:\d{2}:\d{2}\.\d+\."),
						(LogType.Information, @"> Cloning repository \[1\/1, https:\/\/devops\.wisetechglobal\.com\/wtg\/CargoWise\/_git\/Dev\] finished in \d+:\d{2}:\d{2}:\d{2}\.\d+\."),
					});

				Test("One main repo in Github with one dependency in Github",
					new[]
					{
						Mock.Of<IRepository>(repository => repository.Repository == "https://github.com/WiseTechGlobal/WTG.RtfConverter" && repository.Path == "/"),
					},
					new[]
					{
						("https://github.com/WiseTechGlobal/WTG.RtfConverter",
							new[]
							{
								(@"src\WiseTechGlobal/WTG.RtfConverter", new[] { @"Repository=""https://github.com/WiseTechGlobal/WTG.WindowMonitoring""", }),
							}),
						("https://github.com/WiseTechGlobal/WTG.WindowMonitoring",
							Array.Empty<(string, string[])>()),
					},
					new[]
					{
						(LogType.Information, @"> Cloning repository \[1\/1, https:\/\/github\.com\/WiseTechGlobal\/WTG\.RtfConverter\]\.\.\."),
						(LogType.Information, @"-> Cloning repository \[1\/1, https:\/\/github\.com\/WiseTechGlobal\/WTG\.WindowMonitoring\]\.\.\."),
						(LogType.Information, @"-> Cloning repository \[1\/1, https:\/\/github\.com\/WiseTechGlobal\/WTG\.WindowMonitoring\] finished in \d+:\d{2}:\d{2}:\d{2}\.\d+\."),
						(LogType.Information, @"> Cloning repository \[1\/1, https:\/\/github\.com\/WiseTechGlobal\/WTG\.RtfConverter\] finished in \d+:\d{2}:\d{2}:\d{2}\.\d+\."),
					});

				void Test(
					string title,
					IRepository[] mainRepositories,
					(string repository, (string path, string[] dependencies)[] buildXmls)[] recursiveRepos,
					(LogType logType, string pattern)[] expected)
				{
					// Arrange
					loggerMock.Invocations.Clear();
					using var tempFolder = new TempDirectory(Temp.GetNewTempSubdirectory());
					using var gitFolder = new TempDirectory(Temp.GetNewTempSubdirectory());
					var workingDirectoryMock = Mock.Of<IWorkingDirectory>(directory => directory.DirectoryName == tempFolder.DirectoryName);
					var gitDirectoryMock = Mock.Of<IWorkingDirectory>(directory => directory.DirectoryName == gitFolder.DirectoryName);
					repositoryConfigurationRegistryMock
						.SetupGet(registry => registry.MainRepositories)
						.Returns(mainRepositories);

					foreach (var recursiveRepo in recursiveRepos)
					{
						var cloneUrl = recursiveRepo.repository;
						gitAdapterMock
							.Setup(adapter => adapter.Clone(cloneUrl, It.IsAny<string>(), It.IsAny<ILogger>(), It.Is<string>(s => s.StartsWith("-c http.extraHeader=\"Authorization: Basic"))))
							.Callback(() =>
							{
								foreach (var (path, dependencies) in recursiveRepo.buildXmls)
								{
									CreateBuildXml(Path.Combine(tempFolder.DirectoryName, path), dependencies);
								}
							});
					}

					var logTypes = new List<LogType>();
					var logMessages = new List<string>();
					loggerMock
						.Setup(logger => logger.Log(Moq.Capture.In(logTypes), Moq.Capture.In(logMessages)));

					// Act
					repositoryRetriever.DownloadRepositories(workingDirectoryMock, loggerMock.Object, gitDirectoryMock);

					// Assert
					var result = logTypes.Zip(logMessages, (type, message) => (type, message)).ToArray();
					AssertContainsExactElementsInExactOrder(title, new LogRecordComparer(), expected, result);
					loggerMock.Verify(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()), Times.Exactly(expected.Length));
					loggerMock.VerifyNoOtherCalls();
				}
			}

			public void TestLoggingOnFailedRetrieve()
			{
				// Arrange
				using var tempFolder = new TempDirectory(Temp.GetNewTempSubdirectory());
				using var gitFolder = new TempDirectory(Temp.GetNewTempSubdirectory());
				var workingDirectoryMock = Mock.Of<IWorkingDirectory>(directory => directory.DirectoryName == tempFolder.DirectoryName);
				var gitDirectoryMock = Mock.Of<IWorkingDirectory>(directory => directory.DirectoryName == gitFolder.DirectoryName);
				repositoryConfigurationRegistryMock
					.SetupGet(registry => registry.MainRepositories)
					.Returns(new[]
					{
						Mock.Of<IRepository>(repository => repository.Repository == "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev"
															&& repository.Path == "/"),
					});

				gitAdapterMock
					.Setup(adapter => adapter.Clone("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev", It.IsAny<string>(), It.IsAny<ILogger>(), It.Is<string>(s => s.StartsWith("-c http.extraHeader=\"Authorization: Basic"))))
					.Callback(() => CreateBuildXml(Path.Combine(tempFolder.DirectoryName, @"src\CargoWise\Dev"), new[] { @"Repository=""https://devops.wisetechglobal.com/wtg/CargoWise/_git/Shared""" }));
				gitAdapterMock
					.Setup(adapter => adapter.Clone("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Shared", It.IsAny<string>(), It.IsAny<ILogger>(), It.Is<string>(s => s.StartsWith("-c http.extraHeader=\"Authorization: Basic"))))
					.Throws<IOException>();

				var logTypes = new List<LogType>();
				var logMessages = new List<string>();
				loggerMock
					.Setup(logger => logger.Log(Moq.Capture.In(logTypes), Moq.Capture.In(logMessages)));

				// Act
				AssertExceptionThrown<IOException>(() => repositoryRetriever.DownloadRepositories(workingDirectoryMock, loggerMock.Object, gitDirectoryMock));

				// Assert
				var result = logTypes.Zip(logMessages, (type, message) => (type, message)).ToArray();
				AssertContainsExactElementsInExactOrder(new LogRecordComparer(),
					new[]
					{
						(LogType.Information, @"> Cloning repository \[1\/1, https:\/\/devops\.wisetechglobal\.com\/wtg\/CargoWise\/_git\/Dev\]\.\.\."),
						(LogType.Information, @"-> Cloning repository \[1\/1, https:\/\/devops\.wisetechglobal\.com\/wtg\/CargoWise\/_git\/Shared\]\.\.\."),
					},
					result);
				loggerMock.Verify(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()), Times.Exactly(2));
				loggerMock.VerifyNoOtherCalls();
			}

			static void CreateBuildXml(string path, string[] dependencies)
			{
				var content = new List<string>();
				content.Add(@"<Build xmlns=""http://wisetechglobal.com/DevTools/Build.xsd"" MSBuild=""17"">");
				content.Add("  <Dependencies>");

				foreach (var dependency in dependencies)
				{
					content.Add($"    <Dependency {dependency}>");
					content.Add("    </Dependency>");
				}

				content.Add("  </Dependencies>");
				content.Add("</Build>");
				Directory.CreateDirectory(path);
				File.WriteAllLines(Path.Combine(path, "Build.xml"), content);
			}

			class LogRecordComparer : IEqualityComparer<(LogType logType, string)>
			{
				public bool Equals((LogType logType, string) x, (LogType logType, string) y)
				{
					return x.logType == y.logType
							&& Regex.IsMatch(y.Item2, x.Item2);
				}

				public int GetHashCode((LogType logType, string) obj)
				{
					throw new NotImplementedException();
				}
			}
		}
	}
}
