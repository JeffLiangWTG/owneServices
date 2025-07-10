using System;
using System.Collections.Generic;
using Enterprise.Client.EDI.Escrow;
using Enterprise.Client.EDI.Escrow.Interfaces;
using Moq;
using NUnit.Framework;
using Octokit;

namespace ZClientEDI.Test.Escrow;

class GitHubInstallationTokenFactoryTest : TestCase
{
	protected override void SetUp()
	{
		base.SetUp();
		gitHubAppsMock = new Mock<IGitHubAppsClient>();
		gitHubAppsMock
			.Setup(client => client.CreateInstallationToken(It.IsAny<long>()))
			.ReturnsAsync(new AccessToken("valid_token", DateTimeOffset.UtcNow.AddHours(1)));
		gitHubAppsMock
			.Setup(client => client.GetAllInstallationsForCurrent())
			.ReturnsAsync(new List<Installation> { new Installation() });
		gitHubClientMock = new Mock<IGitHubClient>();
		gitHubClientMock.Setup(client => client.GitHubApps).Returns(gitHubAppsMock.Object);
		gitHubClientFactoryMock = new Mock<IGitHubClientFactory>();
		gitHubClientFactoryMock
			.Setup(factory => factory.Create(It.IsAny<IGitAuthConfigurationRegistry>()))
			.Returns(gitHubClientMock.Object);
	}

	public void TestWrongParamsCall()
	{
		var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new GitHubInstallationTokenFactory(null, Mock.Of<IGitHubClientFactory>()));
		AssertEquals("registry", result.ParamName);

		result = AssertExceptionThrown<ArgumentNullException>(() => _ = new GitHubInstallationTokenFactory(Mock.Of<IGitAuthConfigurationRegistry>(), null));
		AssertEquals("gitHubClientFactory", result.ParamName);
	}

	public void TestGetTokenFirstCallReturnsValidToken()
	{
		// Arrange
		var factory = new GitHubInstallationTokenFactory(Mock.Of<IGitAuthConfigurationRegistry>(), gitHubClientFactoryMock.Object);

		// Act
		var token = factory.GetToken();

		// Assert
		AssertNotNull(token);
		AssertEquals("valid_token", token.Token);
		gitHubAppsMock.Verify(x => x.CreateInstallationToken(It.IsAny<long>()), Times.Once);
	}

	public void TestGetTokenReuseTokenIfNotExpired()
	{
		// Arrange
		var factory = new GitHubInstallationTokenFactory(Mock.Of<IGitAuthConfigurationRegistry>(), gitHubClientFactoryMock.Object);
		var token1 = factory.GetToken();

		// Act
		var token2 = factory.GetToken();

		// Assert
		AssertEquals(token1.Token, token2.Token);
		gitHubAppsMock.Verify(x => x.CreateInstallationToken(It.IsAny<long>()), Times.Once);
	}

	public void TestGetTokenCreatesNewTokenIfCurrentTokenHasExpired()
	{
		// Arrange
		gitHubAppsMock.Setup(client => client.CreateInstallationToken(It.IsAny<long>()))
			.ReturnsAsync(new AccessToken("expired_token", DateTimeOffset.UtcNow.AddMinutes(-1)));

		var factory = new GitHubInstallationTokenFactory(Mock.Of<IGitAuthConfigurationRegistry>(), gitHubClientFactoryMock.Object);
		var token1 = factory.GetToken();

		gitHubAppsMock.Setup(client => client.CreateInstallationToken(It.IsAny<long>()))
			.ReturnsAsync(new AccessToken("new_token", DateTimeOffset.UtcNow.AddHours(1)));

		// Act
		var token2 = factory.GetToken();

		// Assert
		AssertNotEquals(token1.Token, token2.Token);
		gitHubAppsMock.Verify(x => x.CreateInstallationToken(It.IsAny<long>()), Times.Exactly(2));
	}

	public void TestGetTokenCreatesNewTokenIfCurrentTokenIsAboutToExpire()
	{
		// Arrange
		gitHubAppsMock.Setup(client => client.CreateInstallationToken(It.IsAny<long>()))
			.ReturnsAsync(new AccessToken("token_expire_soon", DateTimeOffset.UtcNow.AddMinutes(3)));

		var factory = new GitHubInstallationTokenFactory(Mock.Of<IGitAuthConfigurationRegistry>(), gitHubClientFactoryMock.Object);
		var token1 = factory.GetToken();

		gitHubAppsMock.Setup(client => client.CreateInstallationToken(It.IsAny<long>()))
			.ReturnsAsync(new AccessToken("new_token", DateTimeOffset.UtcNow.AddHours(1)));

		// Act
		var token2 = factory.GetToken();

		// Assert
		AssertNotEquals(token1.Token, token2.Token);
		gitHubAppsMock.Verify(x => x.CreateInstallationToken(It.IsAny<long>()), Times.Exactly(2));
	}

	Mock<IGitHubAppsClient> gitHubAppsMock;
	Mock<IGitHubClient> gitHubClientMock;
	Mock<IGitHubClientFactory> gitHubClientFactoryMock;
}

