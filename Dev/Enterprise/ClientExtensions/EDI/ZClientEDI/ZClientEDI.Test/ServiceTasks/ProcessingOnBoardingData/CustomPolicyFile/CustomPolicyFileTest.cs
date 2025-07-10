using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Enterprise.Client.EDI.ServiceTasks.ProcessingOnBoardingData;
using Moq;
using NUnit.Framework;
using Octokit;
using ContentType = Octokit.ContentType;
using Team = Octokit.Team;
using User = Octokit.User;

namespace Enterprise.Client.EDI.Test
{
	public class CustomPolicyFileTest : TestCase
	{
		const string IssuerList =
			"https://login.microsoftonline.com/8b493985-e1b4-4b95-ade6-98acafdbdb01,\n              https://login.microsoftonline.com/4dbcef22-d396-4ee1-b2e3-fa62fa6191df,\r\n              https://login.microsoftonline.com/e7520e4d-d5a0-488d-9e9f-949faae7dce8";

		const string policyFileXML = $@"<TrustFrameworkPolicy xmlns=""http://schemas.microsoft.com/online/cpim/schemas/2013/06"" PolicyId=""B2C_1A_TrustFrameworkExtensions"">
  <BasePolicy>
    <TenantId>CargoWiseB2CTest01.onmicrosoft.com</TenantId>
    <PolicyId>B2C_1A_TrustFrameworkTemplates</PolicyId>
  </BasePolicy>
  <BuildingBlocks>
    <ClaimsTransformations>
      <ClaimsTransformation Id=""DomainLookup"" TransformationMethod=""LookupValue"">
        <InputParameters>
          <InputParameter Id=""Azure"" DataType=""string"" Value=""true"" />
          <InputParameter Id=""WC_EDI"" DataType=""string"" Value=""true"" />
          <InputParameter Id=""WC_QDT"" DataType=""string"" Value=""true"" />
        </InputParameters>
      </ClaimsTransformation>
      <ClaimsTransformation Id=""ClientIdToCompanyCode"" TransformationMethod=""LookupValue"">
        <InputParameters>
          <InputParameter Id=""8b493985-e1b4-4b95-ade6-98acafdbdb01"" DataType=""string"" Value=""WTG"" />
          <InputParameter Id=""4dbcef22-d396-4ee1-b2e3-fa62fa6191df"" DataType=""string"" Value=""****"" />
          <InputParameter Id=""e7520e4d-d5a0-488d-9e9f-949faae7dce8"" DataType=""string"" Value=""UP3"" />
        </InputParameters>
      </ClaimsTransformation>
    </ClaimsTransformations>
  </BuildingBlocks>
  <ClaimsProviders>
    <ClaimsProvider>
      <Domain>Azure</Domain>
      <DisplayName>Azure</DisplayName>
      <TechnicalProfiles>
        <TechnicalProfile Id=""Azure-OpenIdConnect"">
          <Metadata>
            <Item Key=""ValidTokenIssuerPrefixes"">
              {IssuerList}
            </Item>
            <Item Key=""ClaimValueOnWhichToEnable"">Azure</Item>
          </Metadata>
          <IncludeTechnicalProfile ReferenceId=""Azure-OpenIdConnect-Template"" />
        </TechnicalProfile>
      </TechnicalProfiles>
    </ClaimsProvider>
    <ClaimsProvider>
      <DisplayName>Okta</DisplayName>
      <TechnicalProfiles>
        <TechnicalProfile Id=""EDI-OpenIdConnect"">
          <DisplayName>EDI</DisplayName>
          <Metadata>
            <Item Key=""METADATA"">https://okta.com/oauth2/default/.well-known/openid-configuration</Item>
            <Item Key=""client_id"">0oa9u2c1b3c4fRFH</Item>
            <Item Key=""ValidTokenIssuerPrefixes"">https://okta.com/oauth2/default</Item>
            <Item Key=""ClaimValueOnWhichToEnable"">WC_EDI</Item>
          </Metadata>
          <OutputClaims>
            <OutputClaim ClaimTypeReferenceId=""company_code"" DefaultValue=""EDI"" />
          </OutputClaims>
          <IncludeTechnicalProfile ReferenceId=""Okta-OpenIdConnect-Template"" />
        </TechnicalProfile>
      </TechnicalProfiles>
    </ClaimsProvider>
    <ClaimsProvider>
      <DisplayName>OneLogin</DisplayName>
      <TechnicalProfiles>
        <TechnicalProfile Id=""QDT-OpenIdConnect"">
          <DisplayName>QDT</DisplayName>
          <Metadata>
            <Item Key=""METADATA"">https://noatum.onelogin.com/oidc/2/.well-known/openid-configuration</Item>
            <Item Key=""client_id"">1fdbc130-0a2d-013d-9373-1afdadf0d84b38120</Item>
            <Item Key=""ValidTokenIssuerPrefixes"">https://noatum.onelogin.com/oidc/2</Item>
            <Item Key=""ClaimValueOnWhichToEnable"">WC_QDT</Item>
          </Metadata>
          <OutputClaims>
            <OutputClaim ClaimTypeReferenceId=""company_code"" DefaultValue=""QDT"" />
          </OutputClaims>
          <IncludeTechnicalProfile ReferenceId=""OneLogin-OpenIdConnect-Template"" />
        </TechnicalProfile>
      </TechnicalProfiles>
    </ClaimsProvider>
  </ClaimsProviders>
  <SubJourneys>
    <SubJourney Id=""IdPSignInAndSignUp"" Type=""Call"">
      <OrchestrationSteps>
        <OrchestrationStep Order=""1"" Type=""CombinedSignInAndSignUp"" ContentDefinitionReferenceId=""api.signuporsignin"">
          <ClaimsProviderSelections>
            <ClaimsProviderSelection TargetClaimsExchangeId=""AzureAccountExchange"" />
            <ClaimsProviderSelection TargetClaimsExchangeId=""EDIAccountExchange"" />
            <ClaimsProviderSelection TargetClaimsExchangeId=""QDTAccountExchange"" />
          </ClaimsProviderSelections>
        </OrchestrationStep>
        <!-- Check if the user has selected to sign in using one of the social providers -->
        <OrchestrationStep Order=""2"" Type=""ClaimsExchange"">
          <Preconditions>
            <Precondition Type=""ClaimsExist"" ExecuteActionsIf=""true"">
              <Value>objectId</Value>
              <Action>SkipThisOrchestrationStep</Action>
            </Precondition>
          </Preconditions>
          <ClaimsExchanges>
            <ClaimsExchange Id=""AzureAccountExchange"" TechnicalProfileReferenceId=""Azure-OpenIdConnect"" />
            <ClaimsExchange Id=""EDIAccountExchange"" TechnicalProfileReferenceId=""EDI-OpenIdConnect"" />
            <ClaimsExchange Id=""QDTAccountExchange"" TechnicalProfileReferenceId=""QDT-OpenIdConnect"" />
          </ClaimsExchanges>
        </OrchestrationStep>
      </OrchestrationSteps>
    </SubJourney>
  </SubJourneys>
</TrustFrameworkPolicy>";

		public void TestUpdatePR_CreatesPullRequest_MergeSucceed()
		{
			UpdatePR_CreatesPullRequestMergeFailed();
		}

		public void TestUpdatePR_CreatesPullRequest_MergeFailed()
		{
			UpdatePR_CreatesPullRequestMergeFailed(true);
		}

		void UpdatePR_CreatesPullRequestMergeFailed(bool mergeFailed = false)
		{
			var customPolicyFile = new Mock<CustomPolicyFileBase> { CallBase = true };
			var mockGitHubClient = new Mock<IGitHubClient>();

			customPolicyFile.Setup(x => x.CreateInstallationClient(It.IsAny<GitHubAppInfo>()))
				.Returns(mockGitHubClient.Object);

			// Mocking GitHub API calls for getting the latest commit
			mockGitHubClient.Setup(x =>
					x.Repository.Commit.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()).Result)
				.Returns(new GitHubCommit(
					nodeId: "12345",
					url: "https://github.com/example/commit/12345",
					label: "Initial commit",
					@ref: "main",
					sha: "abcdef123456",
					user: new User(),
					repository: new Repository(),
					author: new Author(),
					commentsUrl: "https://github.com/example/commit/12345/comments",
					commit: new Commit(),
					committer: new Author(),
					htmlUrl: "https://github.com/example/commit/abcdef123456",
					stats: new GitHubCommitStats(3, 2, 1),
					parents: new List<GitReference> { new GitReference() },
					files: new List<GitHubCommitFile> { new GitHubCommitFile() }));

			var reference = new Reference(@ref: "refs/heads/main", nodeId: "12345",
				url: "https://github.com/example/repo.git", @object: new TagObject());

			// Mocking GitHub API calls for creating a new branch
			mockGitHubClient.Setup(x =>
					x.Git.Reference.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NewReference>()).Result)
				.Returns(reference);

			// Mocking GitHub API calls for getting existing file content
			mockGitHubClient.Setup(x =>
					x.Repository.Content.GetAllContentsByRef(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
						It.IsAny<string>()).Result)
				.Returns(new[]
				{
					new RepositoryContent(
						name: "example.txt",
						path: "path/to/example.txt",
						sha: "abcdef123456",
						size: 1024,
						type: ContentType.File,
						downloadUrl: "https://github.com/example/repo/raw/main/path/to/example.txt",
						url: "https://github.com/example/repo/blob/main/path/to/example.txt",
						gitUrl: "git://github.com/example/repo.git",
						htmlUrl: "https://github.com/example/repo/blob/main/path/to/example.txt",
						encoding: "utf-8",
						encodedContent: Convert.ToBase64String(Encoding.UTF8.GetBytes(policyFileXML)),
						target: "_blank",
						submoduleGitUrl: "git://github.com/example/submodule.git")
				});

			// Mocking GitHub API calls for updating file content
			mockGitHubClient.Setup(x => x.Repository.Content.UpdateFile(It.IsAny<string>(), It.IsAny<string>(),
					It.IsAny<string>(), It.IsAny<UpdateFileRequest>()).Result)
				.Returns(new RepositoryContentChangeSet(
					new RepositoryContentInfo(),
					new Commit(nodeId: "12345",
						url: "https://github.com/example/repo/commit/12345",
						label: "Initial commit",
						@ref: "main",
						sha: "abcdef123456",
						user: new User(),
						repository: new Repository(),
						message: "This is the commit message",
						author: new Committer(),
						committer: new Committer(),
						tree: new GitReference(),
						parents: new List<GitReference> { new GitReference() },
						commentCount: 3,
						verification: new Verification())));

			// Mocking GitHub API calls for updating branch reference
			mockGitHubClient.Setup(x =>
					x.Git.Reference.Update(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
						It.IsAny<ReferenceUpdate>()).Result)
				.Returns(reference);

			// Mocking GitHub API calls for creating a pull request
			mockGitHubClient.Setup(x =>
					x.Repository.PullRequest.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NewPullRequest>())
						.Result)
				.Returns(new PullRequest(
					id: 123,
					nodeId: "abcdefghijkl",
					url: "https://github.com/example/repo/pull/123",
					htmlUrl: "https://github.com/example/repo/pull/123",
					diffUrl: "https://github.com/example/repo/pull/123.diff",
					patchUrl: "https://github.com/example/repo/pull/123.patch",
					issueUrl: "https://github.com/example/repo/issues/123",
					statusesUrl: "https://api.github.com/repos/example/repo/statuses/123",
					number: 123,
					state: ItemState.Open,
					title: "Fix a bug",
					body: "This pull request fixes a bug in the code.",
					createdAt: DateTimeOffset.Now.AddDays(-7),
					updatedAt: DateTimeOffset.Now,
					closedAt: null,
					mergedAt: null,
					head: new GitReference(),
					@base: new GitReference(),
					user: new User(),
					assignee: new User(),
					assignees: new List<User> { new User() },
					draft: false,
					mergeable: true,
					mergeableState: MergeableState.Clean,
					mergedBy: null,
					mergeCommitSha: null,
					comments: 5,
					commits: 3,
					additions: 10,
					deletions: 2,
					changedFiles: 3,
					milestone: new Milestone(),
					locked: false,
					maintainerCanModify: true,
					requestedReviewers: new List<User> { new User() },
					requestedTeams: new List<Team> { new Team() },
					labels: new List<Label> { new Label() },
					activeLockReason: null));

			mockGitHubClient.Setup(x =>
					x.Repository.PullRequest.Merge(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
						It.IsAny<MergePullRequest>()).Result)
				.Returns(new PullRequestMerge("abcdef123456", !mergeFailed, "Merged pull request"));

			if (mergeFailed)
			{
				var exception = AssertExceptionThrown<InvalidOperationException>(() => customPolicyFile.Object.CreatePRAndMerge("companyCode", "systemIdentifier", "prefix", new GitHubAppInfo(), isRollback: false));

				AssertEquals("Failed to merge pull request: https://github.com/example/repo/pull/123", exception.Message);
			}
			else
			{
				var prUrl = customPolicyFile.Object.CreatePRAndMerge("companyCode", "systemIdentifier", "prefix", new GitHubAppInfo(), isRollback: false);
				AssertNotNull(prUrl);
			}

			mockGitHubClient.Verify(x => x.Repository.PullRequest.Merge(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<MergePullRequest>()), Times.Once());
		}

		public void TestAzureCustomPolicyFileRollback()
		{
			var customPolicyFile = new AzureCustomPolicyFile();
			var xmlDocument = LoadXmlDocument(policyFileXML);
			customPolicyFile.UpdatePolicyFileCore(xmlDocument, "WTG", "8b493985-e1b4-4b95-ade6-98acafdbdb01", null, true);

			var expectedContent = @"<TrustFrameworkPolicy xmlns=""http://schemas.microsoft.com/online/cpim/schemas/2013/06"" PolicyId=""B2C_1A_TrustFrameworkExtensions"">
  <BasePolicy>
    <TenantId>CargoWiseB2CTest01.onmicrosoft.com</TenantId>
    <PolicyId>B2C_1A_TrustFrameworkTemplates</PolicyId>
  </BasePolicy>
  <BuildingBlocks>
    <ClaimsTransformations>
      <ClaimsTransformation Id=""DomainLookup"" TransformationMethod=""LookupValue"">
        <InputParameters>
          <InputParameter Id=""Azure"" DataType=""string"" Value=""true"" />
          <InputParameter Id=""WC_EDI"" DataType=""string"" Value=""true"" />
          <InputParameter Id=""WC_QDT"" DataType=""string"" Value=""true"" />
        </InputParameters>
      </ClaimsTransformation>
      <ClaimsTransformation Id=""ClientIdToCompanyCode"" TransformationMethod=""LookupValue"">
        <InputParameters>
          <InputParameter Id=""4dbcef22-d396-4ee1-b2e3-fa62fa6191df"" DataType=""string"" Value=""****"" />
          <InputParameter Id=""e7520e4d-d5a0-488d-9e9f-949faae7dce8"" DataType=""string"" Value=""UP3"" />
        </InputParameters>
      </ClaimsTransformation>
    </ClaimsTransformations>
  </BuildingBlocks>
  <ClaimsProviders>
    <ClaimsProvider>
      <Domain>Azure</Domain>
      <DisplayName>Azure</DisplayName>
      <TechnicalProfiles>
        <TechnicalProfile Id=""Azure-OpenIdConnect"">
          <Metadata>
            <Item Key=""ValidTokenIssuerPrefixes"">
              https://login.microsoftonline.com/4dbcef22-d396-4ee1-b2e3-fa62fa6191df,
              https://login.microsoftonline.com/e7520e4d-d5a0-488d-9e9f-949faae7dce8
            </Item>
            <Item Key=""ClaimValueOnWhichToEnable"">Azure</Item>
          </Metadata>
          <IncludeTechnicalProfile ReferenceId=""Azure-OpenIdConnect-Template"" />
        </TechnicalProfile>
      </TechnicalProfiles>
    </ClaimsProvider>
    <ClaimsProvider>
      <DisplayName>Okta</DisplayName>
      <TechnicalProfiles>
        <TechnicalProfile Id=""EDI-OpenIdConnect"">
          <DisplayName>EDI</DisplayName>
          <Metadata>
            <Item Key=""METADATA"">https://okta.com/oauth2/default/.well-known/openid-configuration</Item>
            <Item Key=""client_id"">0oa9u2c1b3c4fRFH</Item>
            <Item Key=""ValidTokenIssuerPrefixes"">https://okta.com/oauth2/default</Item>
            <Item Key=""ClaimValueOnWhichToEnable"">WC_EDI</Item>
          </Metadata>
          <OutputClaims>
            <OutputClaim ClaimTypeReferenceId=""company_code"" DefaultValue=""EDI"" />
          </OutputClaims>
          <IncludeTechnicalProfile ReferenceId=""Okta-OpenIdConnect-Template"" />
        </TechnicalProfile>
      </TechnicalProfiles>
    </ClaimsProvider>
    <ClaimsProvider>
      <DisplayName>OneLogin</DisplayName>
      <TechnicalProfiles>
        <TechnicalProfile Id=""QDT-OpenIdConnect"">
          <DisplayName>QDT</DisplayName>
          <Metadata>
            <Item Key=""METADATA"">https://noatum.onelogin.com/oidc/2/.well-known/openid-configuration</Item>
            <Item Key=""client_id"">1fdbc130-0a2d-013d-9373-1afdadf0d84b38120</Item>
            <Item Key=""ValidTokenIssuerPrefixes"">https://noatum.onelogin.com/oidc/2</Item>
            <Item Key=""ClaimValueOnWhichToEnable"">WC_QDT</Item>
          </Metadata>
          <OutputClaims>
            <OutputClaim ClaimTypeReferenceId=""company_code"" DefaultValue=""QDT"" />
          </OutputClaims>
          <IncludeTechnicalProfile ReferenceId=""OneLogin-OpenIdConnect-Template"" />
        </TechnicalProfile>
      </TechnicalProfiles>
    </ClaimsProvider>
  </ClaimsProviders>
  <SubJourneys>
    <SubJourney Id=""IdPSignInAndSignUp"" Type=""Call"">
      <OrchestrationSteps>
        <OrchestrationStep Order=""1"" Type=""CombinedSignInAndSignUp"" ContentDefinitionReferenceId=""api.signuporsignin"">
          <ClaimsProviderSelections>
            <ClaimsProviderSelection TargetClaimsExchangeId=""AzureAccountExchange"" />
            <ClaimsProviderSelection TargetClaimsExchangeId=""EDIAccountExchange"" />
            <ClaimsProviderSelection TargetClaimsExchangeId=""QDTAccountExchange"" />
          </ClaimsProviderSelections>
        </OrchestrationStep>
        <!-- Check if the user has selected to sign in using one of the social providers -->
        <OrchestrationStep Order=""2"" Type=""ClaimsExchange"">
          <Preconditions>
            <Precondition Type=""ClaimsExist"" ExecuteActionsIf=""true"">
              <Value>objectId</Value>
              <Action>SkipThisOrchestrationStep</Action>
            </Precondition>
          </Preconditions>
          <ClaimsExchanges>
            <ClaimsExchange Id=""AzureAccountExchange"" TechnicalProfileReferenceId=""Azure-OpenIdConnect"" />
            <ClaimsExchange Id=""EDIAccountExchange"" TechnicalProfileReferenceId=""EDI-OpenIdConnect"" />
            <ClaimsExchange Id=""QDTAccountExchange"" TechnicalProfileReferenceId=""QDT-OpenIdConnect"" />
          </ClaimsExchanges>
        </OrchestrationStep>
      </OrchestrationSteps>
    </SubJourney>
  </SubJourneys>
</TrustFrameworkPolicy>";

			var policyFileContent = GetXmlContent(xmlDocument);
			AssertEquals(expectedContent, policyFileContent);
		}

		public void TestAzureCustomPolicyFileDoesNotChangeAnyCharacterWithSameCompanyCode()
		{
			var customPolicyFile = new AzureCustomPolicyFile();
			var xmlDocument = LoadXmlDocument(policyFileXML);
			customPolicyFile.UpdatePolicyFileCore(xmlDocument, "WTG", "8b493985-e1b4-4b95-ade6-98acafdbdb01", null, false);

			var policyFileContent = GetXmlContent(xmlDocument);
			var updatedText = Regex.Replace(policyFileXML, "(?<!\r)\n", "\r\n");
			AssertEquals(policyFileContent, updatedText);
		}

		public void TestAzureCustomPolicyFileChangesForValidTokenIssuerPrefixes()
		{
			var customPolicyFile = new AzureCustomPolicyFile();
			var xmlDocument = LoadXmlDocument(policyFileXML);

			var systemUniqueIdentifier = Guid.NewGuid().ToString();
			customPolicyFile.UpdatePolicyFileCore(xmlDocument, "TST", systemUniqueIdentifier, null, false);

			var expectedContent = $@"<TrustFrameworkPolicy xmlns=""http://schemas.microsoft.com/online/cpim/schemas/2013/06"" PolicyId=""B2C_1A_TrustFrameworkExtensions"">
  <BasePolicy>
    <TenantId>CargoWiseB2CTest01.onmicrosoft.com</TenantId>
    <PolicyId>B2C_1A_TrustFrameworkTemplates</PolicyId>
  </BasePolicy>
  <BuildingBlocks>
    <ClaimsTransformations>
      <ClaimsTransformation Id=""DomainLookup"" TransformationMethod=""LookupValue"">
        <InputParameters>
          <InputParameter Id=""Azure"" DataType=""string"" Value=""true"" />
          <InputParameter Id=""WC_EDI"" DataType=""string"" Value=""true"" />
          <InputParameter Id=""WC_QDT"" DataType=""string"" Value=""true"" />
        </InputParameters>
      </ClaimsTransformation>
      <ClaimsTransformation Id=""ClientIdToCompanyCode"" TransformationMethod=""LookupValue"">
        <InputParameters>
          <InputParameter Id=""8b493985-e1b4-4b95-ade6-98acafdbdb01"" DataType=""string"" Value=""WTG"" />
          <InputParameter Id=""4dbcef22-d396-4ee1-b2e3-fa62fa6191df"" DataType=""string"" Value=""****"" />
          <InputParameter Id=""e7520e4d-d5a0-488d-9e9f-949faae7dce8"" DataType=""string"" Value=""UP3"" />
          <InputParameter Id=""{systemUniqueIdentifier}"" DataType=""string"" Value=""TST"" />
        </InputParameters>
      </ClaimsTransformation>
    </ClaimsTransformations>
  </BuildingBlocks>
  <ClaimsProviders>
    <ClaimsProvider>
      <Domain>Azure</Domain>
      <DisplayName>Azure</DisplayName>
      <TechnicalProfiles>
        <TechnicalProfile Id=""Azure-OpenIdConnect"">
          <Metadata>
            <Item Key=""ValidTokenIssuerPrefixes"">
              https://login.microsoftonline.com/8b493985-e1b4-4b95-ade6-98acafdbdb01,
              https://login.microsoftonline.com/4dbcef22-d396-4ee1-b2e3-fa62fa6191df,
              https://login.microsoftonline.com/e7520e4d-d5a0-488d-9e9f-949faae7dce8,
              https://login.microsoftonline.com/{systemUniqueIdentifier}
            </Item>
            <Item Key=""ClaimValueOnWhichToEnable"">Azure</Item>
          </Metadata>
          <IncludeTechnicalProfile ReferenceId=""Azure-OpenIdConnect-Template"" />
        </TechnicalProfile>
      </TechnicalProfiles>
    </ClaimsProvider>
    <ClaimsProvider>
      <DisplayName>Okta</DisplayName>
      <TechnicalProfiles>
        <TechnicalProfile Id=""EDI-OpenIdConnect"">
          <DisplayName>EDI</DisplayName>
          <Metadata>
            <Item Key=""METADATA"">https://okta.com/oauth2/default/.well-known/openid-configuration</Item>
            <Item Key=""client_id"">0oa9u2c1b3c4fRFH</Item>
            <Item Key=""ValidTokenIssuerPrefixes"">https://okta.com/oauth2/default</Item>
            <Item Key=""ClaimValueOnWhichToEnable"">WC_EDI</Item>
          </Metadata>
          <OutputClaims>
            <OutputClaim ClaimTypeReferenceId=""company_code"" DefaultValue=""EDI"" />
          </OutputClaims>
          <IncludeTechnicalProfile ReferenceId=""Okta-OpenIdConnect-Template"" />
        </TechnicalProfile>
      </TechnicalProfiles>
    </ClaimsProvider>
    <ClaimsProvider>
      <DisplayName>OneLogin</DisplayName>
      <TechnicalProfiles>
        <TechnicalProfile Id=""QDT-OpenIdConnect"">
          <DisplayName>QDT</DisplayName>
          <Metadata>
            <Item Key=""METADATA"">https://noatum.onelogin.com/oidc/2/.well-known/openid-configuration</Item>
            <Item Key=""client_id"">1fdbc130-0a2d-013d-9373-1afdadf0d84b38120</Item>
            <Item Key=""ValidTokenIssuerPrefixes"">https://noatum.onelogin.com/oidc/2</Item>
            <Item Key=""ClaimValueOnWhichToEnable"">WC_QDT</Item>
          </Metadata>
          <OutputClaims>
            <OutputClaim ClaimTypeReferenceId=""company_code"" DefaultValue=""QDT"" />
          </OutputClaims>
          <IncludeTechnicalProfile ReferenceId=""OneLogin-OpenIdConnect-Template"" />
        </TechnicalProfile>
      </TechnicalProfiles>
    </ClaimsProvider>
  </ClaimsProviders>
  <SubJourneys>
    <SubJourney Id=""IdPSignInAndSignUp"" Type=""Call"">
      <OrchestrationSteps>
        <OrchestrationStep Order=""1"" Type=""CombinedSignInAndSignUp"" ContentDefinitionReferenceId=""api.signuporsignin"">
          <ClaimsProviderSelections>
            <ClaimsProviderSelection TargetClaimsExchangeId=""AzureAccountExchange"" />
            <ClaimsProviderSelection TargetClaimsExchangeId=""EDIAccountExchange"" />
            <ClaimsProviderSelection TargetClaimsExchangeId=""QDTAccountExchange"" />
          </ClaimsProviderSelections>
        </OrchestrationStep>
        <!-- Check if the user has selected to sign in using one of the social providers -->
        <OrchestrationStep Order=""2"" Type=""ClaimsExchange"">
          <Preconditions>
            <Precondition Type=""ClaimsExist"" ExecuteActionsIf=""true"">
              <Value>objectId</Value>
              <Action>SkipThisOrchestrationStep</Action>
            </Precondition>
          </Preconditions>
          <ClaimsExchanges>
            <ClaimsExchange Id=""AzureAccountExchange"" TechnicalProfileReferenceId=""Azure-OpenIdConnect"" />
            <ClaimsExchange Id=""EDIAccountExchange"" TechnicalProfileReferenceId=""EDI-OpenIdConnect"" />
            <ClaimsExchange Id=""QDTAccountExchange"" TechnicalProfileReferenceId=""QDT-OpenIdConnect"" />
          </ClaimsExchanges>
        </OrchestrationStep>
      </OrchestrationSteps>
    </SubJourney>
  </SubJourneys>
</TrustFrameworkPolicy>";

			var policyFileContent = GetXmlContent(xmlDocument);
			AssertEquals(expectedContent, policyFileContent);

			customPolicyFile.UpdatePolicyFileCore(xmlDocument, "EDI", "8b493985-e1b4-4b95-ade6-98acafdbdb01", null, false);
			expectedContent = $@"<TrustFrameworkPolicy xmlns=""http://schemas.microsoft.com/online/cpim/schemas/2013/06"" PolicyId=""B2C_1A_TrustFrameworkExtensions"">
  <BasePolicy>
    <TenantId>CargoWiseB2CTest01.onmicrosoft.com</TenantId>
    <PolicyId>B2C_1A_TrustFrameworkTemplates</PolicyId>
  </BasePolicy>
  <BuildingBlocks>
    <ClaimsTransformations>
      <ClaimsTransformation Id=""DomainLookup"" TransformationMethod=""LookupValue"">
        <InputParameters>
          <InputParameter Id=""Azure"" DataType=""string"" Value=""true"" />
          <InputParameter Id=""WC_EDI"" DataType=""string"" Value=""true"" />
          <InputParameter Id=""WC_QDT"" DataType=""string"" Value=""true"" />
        </InputParameters>
      </ClaimsTransformation>
      <ClaimsTransformation Id=""ClientIdToCompanyCode"" TransformationMethod=""LookupValue"">
        <InputParameters>
          <InputParameter Id=""8b493985-e1b4-4b95-ade6-98acafdbdb01"" DataType=""string"" Value=""EDI"" />
          <InputParameter Id=""4dbcef22-d396-4ee1-b2e3-fa62fa6191df"" DataType=""string"" Value=""****"" />
          <InputParameter Id=""e7520e4d-d5a0-488d-9e9f-949faae7dce8"" DataType=""string"" Value=""UP3"" />
          <InputParameter Id=""{systemUniqueIdentifier}"" DataType=""string"" Value=""TST"" />
        </InputParameters>
      </ClaimsTransformation>
    </ClaimsTransformations>
  </BuildingBlocks>
  <ClaimsProviders>
    <ClaimsProvider>
      <Domain>Azure</Domain>
      <DisplayName>Azure</DisplayName>
      <TechnicalProfiles>
        <TechnicalProfile Id=""Azure-OpenIdConnect"">
          <Metadata>
            <Item Key=""ValidTokenIssuerPrefixes"">
              https://login.microsoftonline.com/8b493985-e1b4-4b95-ade6-98acafdbdb01,
              https://login.microsoftonline.com/4dbcef22-d396-4ee1-b2e3-fa62fa6191df,
              https://login.microsoftonline.com/e7520e4d-d5a0-488d-9e9f-949faae7dce8,
              https://login.microsoftonline.com/{systemUniqueIdentifier}
            </Item>
            <Item Key=""ClaimValueOnWhichToEnable"">Azure</Item>
          </Metadata>
          <IncludeTechnicalProfile ReferenceId=""Azure-OpenIdConnect-Template"" />
        </TechnicalProfile>
      </TechnicalProfiles>
    </ClaimsProvider>
    <ClaimsProvider>
      <DisplayName>Okta</DisplayName>
      <TechnicalProfiles>
        <TechnicalProfile Id=""EDI-OpenIdConnect"">
          <DisplayName>EDI</DisplayName>
          <Metadata>
            <Item Key=""METADATA"">https://okta.com/oauth2/default/.well-known/openid-configuration</Item>
            <Item Key=""client_id"">0oa9u2c1b3c4fRFH</Item>
            <Item Key=""ValidTokenIssuerPrefixes"">https://okta.com/oauth2/default</Item>
            <Item Key=""ClaimValueOnWhichToEnable"">WC_EDI</Item>
          </Metadata>
          <OutputClaims>
            <OutputClaim ClaimTypeReferenceId=""company_code"" DefaultValue=""EDI"" />
          </OutputClaims>
          <IncludeTechnicalProfile ReferenceId=""Okta-OpenIdConnect-Template"" />
        </TechnicalProfile>
      </TechnicalProfiles>
    </ClaimsProvider>
    <ClaimsProvider>
      <DisplayName>OneLogin</DisplayName>
      <TechnicalProfiles>
        <TechnicalProfile Id=""QDT-OpenIdConnect"">
          <DisplayName>QDT</DisplayName>
          <Metadata>
            <Item Key=""METADATA"">https://noatum.onelogin.com/oidc/2/.well-known/openid-configuration</Item>
            <Item Key=""client_id"">1fdbc130-0a2d-013d-9373-1afdadf0d84b38120</Item>
            <Item Key=""ValidTokenIssuerPrefixes"">https://noatum.onelogin.com/oidc/2</Item>
            <Item Key=""ClaimValueOnWhichToEnable"">WC_QDT</Item>
          </Metadata>
          <OutputClaims>
            <OutputClaim ClaimTypeReferenceId=""company_code"" DefaultValue=""QDT"" />
          </OutputClaims>
          <IncludeTechnicalProfile ReferenceId=""OneLogin-OpenIdConnect-Template"" />
        </TechnicalProfile>
      </TechnicalProfiles>
    </ClaimsProvider>
  </ClaimsProviders>
  <SubJourneys>
    <SubJourney Id=""IdPSignInAndSignUp"" Type=""Call"">
      <OrchestrationSteps>
        <OrchestrationStep Order=""1"" Type=""CombinedSignInAndSignUp"" ContentDefinitionReferenceId=""api.signuporsignin"">
          <ClaimsProviderSelections>
            <ClaimsProviderSelection TargetClaimsExchangeId=""AzureAccountExchange"" />
            <ClaimsProviderSelection TargetClaimsExchangeId=""EDIAccountExchange"" />
            <ClaimsProviderSelection TargetClaimsExchangeId=""QDTAccountExchange"" />
          </ClaimsProviderSelections>
        </OrchestrationStep>
        <!-- Check if the user has selected to sign in using one of the social providers -->
        <OrchestrationStep Order=""2"" Type=""ClaimsExchange"">
          <Preconditions>
            <Precondition Type=""ClaimsExist"" ExecuteActionsIf=""true"">
              <Value>objectId</Value>
              <Action>SkipThisOrchestrationStep</Action>
            </Precondition>
          </Preconditions>
          <ClaimsExchanges>
            <ClaimsExchange Id=""AzureAccountExchange"" TechnicalProfileReferenceId=""Azure-OpenIdConnect"" />
            <ClaimsExchange Id=""EDIAccountExchange"" TechnicalProfileReferenceId=""EDI-OpenIdConnect"" />
            <ClaimsExchange Id=""QDTAccountExchange"" TechnicalProfileReferenceId=""QDT-OpenIdConnect"" />
          </ClaimsExchanges>
        </OrchestrationStep>
      </OrchestrationSteps>
    </SubJourney>
  </SubJourneys>
</TrustFrameworkPolicy>";
			policyFileContent = GetXmlContent(xmlDocument);
			AssertEquals("The WTG company code elements sequence should be changed", policyFileContent, expectedContent);
		}

		public void TestOktaCustomPolicyFileRollback()
		{
			var customPolicyFile = new OktaCustomPolicyFile();

			var xmlDocument = LoadXmlDocument(policyFileXML);

			var companyCode = "EDI";

			customPolicyFile.UpdatePolicyFileCore(xmlDocument, companyCode, "", "", true);

			var expectedContent = @"<TrustFrameworkPolicy xmlns=""http://schemas.microsoft.com/online/cpim/schemas/2013/06"" PolicyId=""B2C_1A_TrustFrameworkExtensions"">
  <BasePolicy>
    <TenantId>CargoWiseB2CTest01.onmicrosoft.com</TenantId>
    <PolicyId>B2C_1A_TrustFrameworkTemplates</PolicyId>
  </BasePolicy>
  <BuildingBlocks>
    <ClaimsTransformations>
      <ClaimsTransformation Id=""DomainLookup"" TransformationMethod=""LookupValue"">
        <InputParameters>
          <InputParameter Id=""Azure"" DataType=""string"" Value=""true"" />
          <InputParameter Id=""WC_QDT"" DataType=""string"" Value=""true"" />
        </InputParameters>
      </ClaimsTransformation>
      <ClaimsTransformation Id=""ClientIdToCompanyCode"" TransformationMethod=""LookupValue"">
        <InputParameters>
          <InputParameter Id=""8b493985-e1b4-4b95-ade6-98acafdbdb01"" DataType=""string"" Value=""WTG"" />
          <InputParameter Id=""4dbcef22-d396-4ee1-b2e3-fa62fa6191df"" DataType=""string"" Value=""****"" />
          <InputParameter Id=""e7520e4d-d5a0-488d-9e9f-949faae7dce8"" DataType=""string"" Value=""UP3"" />
        </InputParameters>
      </ClaimsTransformation>
    </ClaimsTransformations>
  </BuildingBlocks>
  <ClaimsProviders>
    <ClaimsProvider>
      <Domain>Azure</Domain>
      <DisplayName>Azure</DisplayName>
      <TechnicalProfiles>
        <TechnicalProfile Id=""Azure-OpenIdConnect"">
          <Metadata>
            <Item Key=""ValidTokenIssuerPrefixes"">
              https://login.microsoftonline.com/8b493985-e1b4-4b95-ade6-98acafdbdb01,
              https://login.microsoftonline.com/4dbcef22-d396-4ee1-b2e3-fa62fa6191df,
              https://login.microsoftonline.com/e7520e4d-d5a0-488d-9e9f-949faae7dce8
            </Item>
            <Item Key=""ClaimValueOnWhichToEnable"">Azure</Item>
          </Metadata>
          <IncludeTechnicalProfile ReferenceId=""Azure-OpenIdConnect-Template"" />
        </TechnicalProfile>
      </TechnicalProfiles>
    </ClaimsProvider>
    <ClaimsProvider>
      <DisplayName>Okta</DisplayName>
      <TechnicalProfiles>
      </TechnicalProfiles>
    </ClaimsProvider>
    <ClaimsProvider>
      <DisplayName>OneLogin</DisplayName>
      <TechnicalProfiles>
        <TechnicalProfile Id=""QDT-OpenIdConnect"">
          <DisplayName>QDT</DisplayName>
          <Metadata>
            <Item Key=""METADATA"">https://noatum.onelogin.com/oidc/2/.well-known/openid-configuration</Item>
            <Item Key=""client_id"">1fdbc130-0a2d-013d-9373-1afdadf0d84b38120</Item>
            <Item Key=""ValidTokenIssuerPrefixes"">https://noatum.onelogin.com/oidc/2</Item>
            <Item Key=""ClaimValueOnWhichToEnable"">WC_QDT</Item>
          </Metadata>
          <OutputClaims>
            <OutputClaim ClaimTypeReferenceId=""company_code"" DefaultValue=""QDT"" />
          </OutputClaims>
          <IncludeTechnicalProfile ReferenceId=""OneLogin-OpenIdConnect-Template"" />
        </TechnicalProfile>
      </TechnicalProfiles>
    </ClaimsProvider>
  </ClaimsProviders>
  <SubJourneys>
    <SubJourney Id=""IdPSignInAndSignUp"" Type=""Call"">
      <OrchestrationSteps>
        <OrchestrationStep Order=""1"" Type=""CombinedSignInAndSignUp"" ContentDefinitionReferenceId=""api.signuporsignin"">
          <ClaimsProviderSelections>
            <ClaimsProviderSelection TargetClaimsExchangeId=""AzureAccountExchange"" />
            <ClaimsProviderSelection TargetClaimsExchangeId=""QDTAccountExchange"" />
          </ClaimsProviderSelections>
        </OrchestrationStep>
        <!-- Check if the user has selected to sign in using one of the social providers -->
        <OrchestrationStep Order=""2"" Type=""ClaimsExchange"">
          <Preconditions>
            <Precondition Type=""ClaimsExist"" ExecuteActionsIf=""true"">
              <Value>objectId</Value>
              <Action>SkipThisOrchestrationStep</Action>
            </Precondition>
          </Preconditions>
          <ClaimsExchanges>
            <ClaimsExchange Id=""AzureAccountExchange"" TechnicalProfileReferenceId=""Azure-OpenIdConnect"" />
            <ClaimsExchange Id=""QDTAccountExchange"" TechnicalProfileReferenceId=""QDT-OpenIdConnect"" />
          </ClaimsExchanges>
        </OrchestrationStep>
      </OrchestrationSteps>
    </SubJourney>
  </SubJourneys>
</TrustFrameworkPolicy>";

			var policyFileContent = GetXmlContent(xmlDocument);
			AssertEquals(expectedContent, policyFileContent);
		}

		public void TestOktaCustomPolicyFile()
		{
			var customPolicyFile = new OktaCustomPolicyFile();

			var xmlDocument = LoadXmlDocument(policyFileXML);

			var companyCode = "WTG";
			var systemUniqueIdentifier = "0oa9u2c1b3c4fRFH55";
			var validTokenIssuerPrefix = "https://wtg.okta.com/oauth2/default";

			customPolicyFile.UpdatePolicyFileCore(xmlDocument, companyCode, systemUniqueIdentifier, validTokenIssuerPrefix, false);

			var expectedContent = @"<TrustFrameworkPolicy xmlns=""http://schemas.microsoft.com/online/cpim/schemas/2013/06"" PolicyId=""B2C_1A_TrustFrameworkExtensions"">
  <BasePolicy>
    <TenantId>CargoWiseB2CTest01.onmicrosoft.com</TenantId>
    <PolicyId>B2C_1A_TrustFrameworkTemplates</PolicyId>
  </BasePolicy>
  <BuildingBlocks>
    <ClaimsTransformations>
      <ClaimsTransformation Id=""DomainLookup"" TransformationMethod=""LookupValue"">
        <InputParameters>
          <InputParameter Id=""Azure"" DataType=""string"" Value=""true"" />
          <InputParameter Id=""WC_EDI"" DataType=""string"" Value=""true"" />
          <InputParameter Id=""WC_QDT"" DataType=""string"" Value=""true"" />
          <InputParameter Id=""WC_WTG"" DataType=""string"" Value=""true"" />
        </InputParameters>
      </ClaimsTransformation>
      <ClaimsTransformation Id=""ClientIdToCompanyCode"" TransformationMethod=""LookupValue"">
        <InputParameters>
          <InputParameter Id=""8b493985-e1b4-4b95-ade6-98acafdbdb01"" DataType=""string"" Value=""WTG"" />
          <InputParameter Id=""4dbcef22-d396-4ee1-b2e3-fa62fa6191df"" DataType=""string"" Value=""****"" />
          <InputParameter Id=""e7520e4d-d5a0-488d-9e9f-949faae7dce8"" DataType=""string"" Value=""UP3"" />
        </InputParameters>
      </ClaimsTransformation>
    </ClaimsTransformations>
  </BuildingBlocks>
  <ClaimsProviders>
    <ClaimsProvider>
      <Domain>Azure</Domain>
      <DisplayName>Azure</DisplayName>
      <TechnicalProfiles>
        <TechnicalProfile Id=""Azure-OpenIdConnect"">
          <Metadata>
            <Item Key=""ValidTokenIssuerPrefixes"">
              https://login.microsoftonline.com/8b493985-e1b4-4b95-ade6-98acafdbdb01,
              https://login.microsoftonline.com/4dbcef22-d396-4ee1-b2e3-fa62fa6191df,
              https://login.microsoftonline.com/e7520e4d-d5a0-488d-9e9f-949faae7dce8
            </Item>
            <Item Key=""ClaimValueOnWhichToEnable"">Azure</Item>
          </Metadata>
          <IncludeTechnicalProfile ReferenceId=""Azure-OpenIdConnect-Template"" />
        </TechnicalProfile>
      </TechnicalProfiles>
    </ClaimsProvider>
    <ClaimsProvider>
      <DisplayName>Okta</DisplayName>
      <TechnicalProfiles>
        <TechnicalProfile Id=""EDI-OpenIdConnect"">
          <DisplayName>EDI</DisplayName>
          <Metadata>
            <Item Key=""METADATA"">https://okta.com/oauth2/default/.well-known/openid-configuration</Item>
            <Item Key=""client_id"">0oa9u2c1b3c4fRFH</Item>
            <Item Key=""ValidTokenIssuerPrefixes"">https://okta.com/oauth2/default</Item>
            <Item Key=""ClaimValueOnWhichToEnable"">WC_EDI</Item>
          </Metadata>
          <OutputClaims>
            <OutputClaim ClaimTypeReferenceId=""company_code"" DefaultValue=""EDI"" />
          </OutputClaims>
          <IncludeTechnicalProfile ReferenceId=""Okta-OpenIdConnect-Template"" />
        </TechnicalProfile>
        <TechnicalProfile Id=""WTG-OpenIdConnect"">
          <DisplayName>WTG</DisplayName>
          <Metadata>
            <Item Key=""METADATA"">https://wtg.okta.com/oauth2/default/.well-known/openid-configuration</Item>
            <Item Key=""client_id"">0oa9u2c1b3c4fRFH55</Item>
            <Item Key=""ValidTokenIssuerPrefixes"">https://wtg.okta.com/oauth2/default</Item>
            <Item Key=""ClaimValueOnWhichToEnable"">WC_WTG</Item>
          </Metadata>
          <OutputClaims>
            <OutputClaim ClaimTypeReferenceId=""company_code"" DefaultValue=""WTG"" />
          </OutputClaims>
          <IncludeTechnicalProfile ReferenceId=""Okta-OpenIdConnect-Template"" />
        </TechnicalProfile>
      </TechnicalProfiles>
    </ClaimsProvider>
    <ClaimsProvider>
      <DisplayName>OneLogin</DisplayName>
      <TechnicalProfiles>
        <TechnicalProfile Id=""QDT-OpenIdConnect"">
          <DisplayName>QDT</DisplayName>
          <Metadata>
            <Item Key=""METADATA"">https://noatum.onelogin.com/oidc/2/.well-known/openid-configuration</Item>
            <Item Key=""client_id"">1fdbc130-0a2d-013d-9373-1afdadf0d84b38120</Item>
            <Item Key=""ValidTokenIssuerPrefixes"">https://noatum.onelogin.com/oidc/2</Item>
            <Item Key=""ClaimValueOnWhichToEnable"">WC_QDT</Item>
          </Metadata>
          <OutputClaims>
            <OutputClaim ClaimTypeReferenceId=""company_code"" DefaultValue=""QDT"" />
          </OutputClaims>
          <IncludeTechnicalProfile ReferenceId=""OneLogin-OpenIdConnect-Template"" />
        </TechnicalProfile>
      </TechnicalProfiles>
    </ClaimsProvider>
  </ClaimsProviders>
  <SubJourneys>
    <SubJourney Id=""IdPSignInAndSignUp"" Type=""Call"">
      <OrchestrationSteps>
        <OrchestrationStep Order=""1"" Type=""CombinedSignInAndSignUp"" ContentDefinitionReferenceId=""api.signuporsignin"">
          <ClaimsProviderSelections>
            <ClaimsProviderSelection TargetClaimsExchangeId=""AzureAccountExchange"" />
            <ClaimsProviderSelection TargetClaimsExchangeId=""EDIAccountExchange"" />
            <ClaimsProviderSelection TargetClaimsExchangeId=""QDTAccountExchange"" />
            <ClaimsProviderSelection TargetClaimsExchangeId=""WTGAccountExchange"" />
          </ClaimsProviderSelections>
        </OrchestrationStep>
        <!-- Check if the user has selected to sign in using one of the social providers -->
        <OrchestrationStep Order=""2"" Type=""ClaimsExchange"">
          <Preconditions>
            <Precondition Type=""ClaimsExist"" ExecuteActionsIf=""true"">
              <Value>objectId</Value>
              <Action>SkipThisOrchestrationStep</Action>
            </Precondition>
          </Preconditions>
          <ClaimsExchanges>
            <ClaimsExchange Id=""AzureAccountExchange"" TechnicalProfileReferenceId=""Azure-OpenIdConnect"" />
            <ClaimsExchange Id=""EDIAccountExchange"" TechnicalProfileReferenceId=""EDI-OpenIdConnect"" />
            <ClaimsExchange Id=""QDTAccountExchange"" TechnicalProfileReferenceId=""QDT-OpenIdConnect"" />
            <ClaimsExchange Id=""WTGAccountExchange"" TechnicalProfileReferenceId=""WTG-OpenIdConnect"" />
          </ClaimsExchanges>
        </OrchestrationStep>
      </OrchestrationSteps>
    </SubJourney>
  </SubJourneys>
</TrustFrameworkPolicy>";

			var policyFileContent = GetXmlContent(xmlDocument);
			AssertEquals(expectedContent, policyFileContent);
		}

		public void TestOneLoginCustomPolicyFileRollback()
		{
			var customPolicyFile = new OneLoginCustomPolicyFile();

			var xmlDocument = LoadXmlDocument(policyFileXML);

			var companyCode = "QDT";

			customPolicyFile.UpdatePolicyFileCore(xmlDocument, companyCode, "", "", true);

			var expectedContent = @"<TrustFrameworkPolicy xmlns=""http://schemas.microsoft.com/online/cpim/schemas/2013/06"" PolicyId=""B2C_1A_TrustFrameworkExtensions"">
  <BasePolicy>
    <TenantId>CargoWiseB2CTest01.onmicrosoft.com</TenantId>
    <PolicyId>B2C_1A_TrustFrameworkTemplates</PolicyId>
  </BasePolicy>
  <BuildingBlocks>
    <ClaimsTransformations>
      <ClaimsTransformation Id=""DomainLookup"" TransformationMethod=""LookupValue"">
        <InputParameters>
          <InputParameter Id=""Azure"" DataType=""string"" Value=""true"" />
          <InputParameter Id=""WC_EDI"" DataType=""string"" Value=""true"" />
        </InputParameters>
      </ClaimsTransformation>
      <ClaimsTransformation Id=""ClientIdToCompanyCode"" TransformationMethod=""LookupValue"">
        <InputParameters>
          <InputParameter Id=""8b493985-e1b4-4b95-ade6-98acafdbdb01"" DataType=""string"" Value=""WTG"" />
          <InputParameter Id=""4dbcef22-d396-4ee1-b2e3-fa62fa6191df"" DataType=""string"" Value=""****"" />
          <InputParameter Id=""e7520e4d-d5a0-488d-9e9f-949faae7dce8"" DataType=""string"" Value=""UP3"" />
        </InputParameters>
      </ClaimsTransformation>
    </ClaimsTransformations>
  </BuildingBlocks>
  <ClaimsProviders>
    <ClaimsProvider>
      <Domain>Azure</Domain>
      <DisplayName>Azure</DisplayName>
      <TechnicalProfiles>
        <TechnicalProfile Id=""Azure-OpenIdConnect"">
          <Metadata>
            <Item Key=""ValidTokenIssuerPrefixes"">
              https://login.microsoftonline.com/8b493985-e1b4-4b95-ade6-98acafdbdb01,
              https://login.microsoftonline.com/4dbcef22-d396-4ee1-b2e3-fa62fa6191df,
              https://login.microsoftonline.com/e7520e4d-d5a0-488d-9e9f-949faae7dce8
            </Item>
            <Item Key=""ClaimValueOnWhichToEnable"">Azure</Item>
          </Metadata>
          <IncludeTechnicalProfile ReferenceId=""Azure-OpenIdConnect-Template"" />
        </TechnicalProfile>
      </TechnicalProfiles>
    </ClaimsProvider>
    <ClaimsProvider>
      <DisplayName>Okta</DisplayName>
      <TechnicalProfiles>
        <TechnicalProfile Id=""EDI-OpenIdConnect"">
          <DisplayName>EDI</DisplayName>
          <Metadata>
            <Item Key=""METADATA"">https://okta.com/oauth2/default/.well-known/openid-configuration</Item>
            <Item Key=""client_id"">0oa9u2c1b3c4fRFH</Item>
            <Item Key=""ValidTokenIssuerPrefixes"">https://okta.com/oauth2/default</Item>
            <Item Key=""ClaimValueOnWhichToEnable"">WC_EDI</Item>
          </Metadata>
          <OutputClaims>
            <OutputClaim ClaimTypeReferenceId=""company_code"" DefaultValue=""EDI"" />
          </OutputClaims>
          <IncludeTechnicalProfile ReferenceId=""Okta-OpenIdConnect-Template"" />
        </TechnicalProfile>
      </TechnicalProfiles>
    </ClaimsProvider>
    <ClaimsProvider>
      <DisplayName>OneLogin</DisplayName>
      <TechnicalProfiles>
      </TechnicalProfiles>
    </ClaimsProvider>
  </ClaimsProviders>
  <SubJourneys>
    <SubJourney Id=""IdPSignInAndSignUp"" Type=""Call"">
      <OrchestrationSteps>
        <OrchestrationStep Order=""1"" Type=""CombinedSignInAndSignUp"" ContentDefinitionReferenceId=""api.signuporsignin"">
          <ClaimsProviderSelections>
            <ClaimsProviderSelection TargetClaimsExchangeId=""AzureAccountExchange"" />
            <ClaimsProviderSelection TargetClaimsExchangeId=""EDIAccountExchange"" />
          </ClaimsProviderSelections>
        </OrchestrationStep>
        <!-- Check if the user has selected to sign in using one of the social providers -->
        <OrchestrationStep Order=""2"" Type=""ClaimsExchange"">
          <Preconditions>
            <Precondition Type=""ClaimsExist"" ExecuteActionsIf=""true"">
              <Value>objectId</Value>
              <Action>SkipThisOrchestrationStep</Action>
            </Precondition>
          </Preconditions>
          <ClaimsExchanges>
            <ClaimsExchange Id=""AzureAccountExchange"" TechnicalProfileReferenceId=""Azure-OpenIdConnect"" />
            <ClaimsExchange Id=""EDIAccountExchange"" TechnicalProfileReferenceId=""EDI-OpenIdConnect"" />
          </ClaimsExchanges>
        </OrchestrationStep>
      </OrchestrationSteps>
    </SubJourney>
  </SubJourneys>
</TrustFrameworkPolicy>";

			var policyFileContent = GetXmlContent(xmlDocument);
			AssertEquals(expectedContent, policyFileContent);
		}

		public void TestOneLoginCustomPolicyFile()
		{
			var customPolicyFile = new OneLoginCustomPolicyFile();

			var xmlDocument = LoadXmlDocument(policyFileXML);

			var companyCode = "QDN";
			var systemUniqueIdentifier = "0oa9u2c1b3c4fRFH55";
			var validTokenIssuerPrefix = "https://noatum.onelogin.com/oidc/2";

			customPolicyFile.UpdatePolicyFileCore(xmlDocument, companyCode, systemUniqueIdentifier, validTokenIssuerPrefix, false);

			var expectedContent = @"<TrustFrameworkPolicy xmlns=""http://schemas.microsoft.com/online/cpim/schemas/2013/06"" PolicyId=""B2C_1A_TrustFrameworkExtensions"">
  <BasePolicy>
    <TenantId>CargoWiseB2CTest01.onmicrosoft.com</TenantId>
    <PolicyId>B2C_1A_TrustFrameworkTemplates</PolicyId>
  </BasePolicy>
  <BuildingBlocks>
    <ClaimsTransformations>
      <ClaimsTransformation Id=""DomainLookup"" TransformationMethod=""LookupValue"">
        <InputParameters>
          <InputParameter Id=""Azure"" DataType=""string"" Value=""true"" />
          <InputParameter Id=""WC_EDI"" DataType=""string"" Value=""true"" />
          <InputParameter Id=""WC_QDT"" DataType=""string"" Value=""true"" />
          <InputParameter Id=""WC_QDN"" DataType=""string"" Value=""true"" />
        </InputParameters>
      </ClaimsTransformation>
      <ClaimsTransformation Id=""ClientIdToCompanyCode"" TransformationMethod=""LookupValue"">
        <InputParameters>
          <InputParameter Id=""8b493985-e1b4-4b95-ade6-98acafdbdb01"" DataType=""string"" Value=""WTG"" />
          <InputParameter Id=""4dbcef22-d396-4ee1-b2e3-fa62fa6191df"" DataType=""string"" Value=""****"" />
          <InputParameter Id=""e7520e4d-d5a0-488d-9e9f-949faae7dce8"" DataType=""string"" Value=""UP3"" />
        </InputParameters>
      </ClaimsTransformation>
    </ClaimsTransformations>
  </BuildingBlocks>
  <ClaimsProviders>
    <ClaimsProvider>
      <Domain>Azure</Domain>
      <DisplayName>Azure</DisplayName>
      <TechnicalProfiles>
        <TechnicalProfile Id=""Azure-OpenIdConnect"">
          <Metadata>
            <Item Key=""ValidTokenIssuerPrefixes"">
              https://login.microsoftonline.com/8b493985-e1b4-4b95-ade6-98acafdbdb01,
              https://login.microsoftonline.com/4dbcef22-d396-4ee1-b2e3-fa62fa6191df,
              https://login.microsoftonline.com/e7520e4d-d5a0-488d-9e9f-949faae7dce8
            </Item>
            <Item Key=""ClaimValueOnWhichToEnable"">Azure</Item>
          </Metadata>
          <IncludeTechnicalProfile ReferenceId=""Azure-OpenIdConnect-Template"" />
        </TechnicalProfile>
      </TechnicalProfiles>
    </ClaimsProvider>
    <ClaimsProvider>
      <DisplayName>Okta</DisplayName>
      <TechnicalProfiles>
        <TechnicalProfile Id=""EDI-OpenIdConnect"">
          <DisplayName>EDI</DisplayName>
          <Metadata>
            <Item Key=""METADATA"">https://okta.com/oauth2/default/.well-known/openid-configuration</Item>
            <Item Key=""client_id"">0oa9u2c1b3c4fRFH</Item>
            <Item Key=""ValidTokenIssuerPrefixes"">https://okta.com/oauth2/default</Item>
            <Item Key=""ClaimValueOnWhichToEnable"">WC_EDI</Item>
          </Metadata>
          <OutputClaims>
            <OutputClaim ClaimTypeReferenceId=""company_code"" DefaultValue=""EDI"" />
          </OutputClaims>
          <IncludeTechnicalProfile ReferenceId=""Okta-OpenIdConnect-Template"" />
        </TechnicalProfile>
      </TechnicalProfiles>
    </ClaimsProvider>
    <ClaimsProvider>
      <DisplayName>OneLogin</DisplayName>
      <TechnicalProfiles>
        <TechnicalProfile Id=""QDT-OpenIdConnect"">
          <DisplayName>QDT</DisplayName>
          <Metadata>
            <Item Key=""METADATA"">https://noatum.onelogin.com/oidc/2/.well-known/openid-configuration</Item>
            <Item Key=""client_id"">1fdbc130-0a2d-013d-9373-1afdadf0d84b38120</Item>
            <Item Key=""ValidTokenIssuerPrefixes"">https://noatum.onelogin.com/oidc/2</Item>
            <Item Key=""ClaimValueOnWhichToEnable"">WC_QDT</Item>
          </Metadata>
          <OutputClaims>
            <OutputClaim ClaimTypeReferenceId=""company_code"" DefaultValue=""QDT"" />
          </OutputClaims>
          <IncludeTechnicalProfile ReferenceId=""OneLogin-OpenIdConnect-Template"" />
        </TechnicalProfile>
        <TechnicalProfile Id=""QDN-OpenIdConnect"">
          <DisplayName>QDN</DisplayName>
          <Metadata>
            <Item Key=""METADATA"">https://noatum.onelogin.com/oidc/2/.well-known/openid-configuration</Item>
            <Item Key=""client_id"">0oa9u2c1b3c4fRFH55</Item>
            <Item Key=""ValidTokenIssuerPrefixes"">https://noatum.onelogin.com/oidc/2</Item>
            <Item Key=""ClaimValueOnWhichToEnable"">WC_QDN</Item>
          </Metadata>
          <OutputClaims>
            <OutputClaim ClaimTypeReferenceId=""company_code"" DefaultValue=""QDN"" />
          </OutputClaims>
          <IncludeTechnicalProfile ReferenceId=""OneLogin-OpenIdConnect-Template"" />
        </TechnicalProfile>
      </TechnicalProfiles>
    </ClaimsProvider>
  </ClaimsProviders>
  <SubJourneys>
    <SubJourney Id=""IdPSignInAndSignUp"" Type=""Call"">
      <OrchestrationSteps>
        <OrchestrationStep Order=""1"" Type=""CombinedSignInAndSignUp"" ContentDefinitionReferenceId=""api.signuporsignin"">
          <ClaimsProviderSelections>
            <ClaimsProviderSelection TargetClaimsExchangeId=""AzureAccountExchange"" />
            <ClaimsProviderSelection TargetClaimsExchangeId=""EDIAccountExchange"" />
            <ClaimsProviderSelection TargetClaimsExchangeId=""QDTAccountExchange"" />
            <ClaimsProviderSelection TargetClaimsExchangeId=""QDNAccountExchange"" />
          </ClaimsProviderSelections>
        </OrchestrationStep>
        <!-- Check if the user has selected to sign in using one of the social providers -->
        <OrchestrationStep Order=""2"" Type=""ClaimsExchange"">
          <Preconditions>
            <Precondition Type=""ClaimsExist"" ExecuteActionsIf=""true"">
              <Value>objectId</Value>
              <Action>SkipThisOrchestrationStep</Action>
            </Precondition>
          </Preconditions>
          <ClaimsExchanges>
            <ClaimsExchange Id=""AzureAccountExchange"" TechnicalProfileReferenceId=""Azure-OpenIdConnect"" />
            <ClaimsExchange Id=""EDIAccountExchange"" TechnicalProfileReferenceId=""EDI-OpenIdConnect"" />
            <ClaimsExchange Id=""QDTAccountExchange"" TechnicalProfileReferenceId=""QDT-OpenIdConnect"" />
            <ClaimsExchange Id=""QDNAccountExchange"" TechnicalProfileReferenceId=""QDN-OpenIdConnect"" />
          </ClaimsExchanges>
        </OrchestrationStep>
      </OrchestrationSteps>
    </SubJourney>
  </SubJourneys>
</TrustFrameworkPolicy>";

			var policyFileContent = GetXmlContent(xmlDocument);
			AssertEquals(expectedContent, policyFileContent);
		}

		public void TestGenericCustomPolicyFile_WhenDisplayNameIsEmpty()
		{
			var customPolicyFile = new GenericCustomPolicyFile();

			var xmlDocument = LoadXmlDocument(policyFileXML);

			var companyCode = "QDN";
			var systemUniqueIdentifier = "0oa9u2c1b3c4fRFH55";
			var validTokenIssuerPrefix = "https://noatum.onelogin.com/oidc/2";

			AssertExceptionThrown<InvalidOperationException>("DisplayName Can not be empty", () => customPolicyFile.UpdatePolicyFileCore(xmlDocument, companyCode, systemUniqueIdentifier, validTokenIssuerPrefix, false));
		}

		public void TestGitHubAppWithoutInnerException()
		{
			var customPolicyFile = new Mock<CustomPolicyFileBase> { CallBase = true };
			var mockGitHubClient = new Mock<IGitHubClient>();

			customPolicyFile.Setup(x => x.CreateInstallationClient(It.IsAny<GitHubAppInfo>()))
				.Returns(mockGitHubClient.Object);

			// Mocking GitHub API calls for getting the latest commit
			mockGitHubClient.Setup(x =>
					x.Repository.Commit.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()).Result)
				.Returns(new GitHubCommit(
					nodeId: "12345",
					url: "https://github.com/example/commit/12345",
					label: "Initial commit",
					@ref: "main",
					sha: "abcdef123456",
					user: new User(),
					repository: new Repository(),
					author: new Author(),
					commentsUrl: "https://github.com/example/commit/12345/comments",
					commit: new Commit(),
					committer: new Author(),
					htmlUrl: "https://github.com/example/commit/abcdef123456",
					stats: new GitHubCommitStats(3, 2, 1),
					parents: new List<GitReference> { new GitReference() },
					files: new List<GitHubCommitFile> { new GitHubCommitFile() }));

			var reference = new Reference(@ref: "refs/heads/main", nodeId: "12345",
				url: "https://github.com/example/repo.git", @object: new TagObject());

			// Mocking GitHub API calls for creating a new branch
			mockGitHubClient.Setup(x =>
					x.Git.Reference.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NewReference>()).Result)
				.Returns(reference);

			// Mocking GitHub API calls for getting existing file content
			mockGitHubClient.Setup(x =>
					x.Repository.Content.GetAllContentsByRef(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
						It.IsAny<string>()).Result)
				.Returns(Array.Empty<RepositoryContent>());

			var message = AssertExceptionThrown<InvalidOperationException>(() => customPolicyFile.Object.CreatePRAndMerge("companyCode", "systemIdentifier", "prefix",
				new GitHubAppInfo(), isRollback: false)).Message;
			AssertContains("Index was out of range. Must be non-negative and less than the size of the collection.", message);
		}

		public void TestGitHubAppWithInnerException()
		{
			var customPolicyFile = new Mock<CustomPolicyFileBase> { CallBase = true };
			var mockGitHubClient = new Mock<IGitHubClient>();

			customPolicyFile.Setup(x => x.CreateInstallationClient(It.IsAny<GitHubAppInfo>()))
				.Returns(mockGitHubClient.Object);

			// Mocking GitHub API calls for getting the latest commit
			mockGitHubClient.Setup(x =>
					x.Repository.Commit.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()).Result)
				.Returns(new GitHubCommit(
					nodeId: "12345",
					url: "https://github.com/example/commit/12345",
					label: "Initial commit",
					@ref: "main",
					sha: "abcdef123456",
					user: new User(),
					repository: new Repository(),
					author: new Author(),
					commentsUrl: "https://github.com/example/commit/12345/comments",
					commit: new Commit(),
					committer: new Author(),
					htmlUrl: "https://github.com/example/commit/abcdef123456",
					stats: new GitHubCommitStats(3, 2, 1),
					parents: new List<GitReference> { new GitReference() },
					files: new List<GitHubCommitFile> { new GitHubCommitFile() }));

			var reference = new Reference(@ref: "refs/heads/main", nodeId: "12345",
				url: "https://github.com/example/repo.git", @object: new TagObject());

			// Mocking GitHub API calls for creating a new branch
			mockGitHubClient.Setup(x =>
					x.Git.Reference.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NewReference>()).Result)
				.Returns(reference);

			// Mocking GitHub API calls for getting existing file content
			mockGitHubClient.Setup(x =>
					x.Repository.Content.GetAllContentsByRef(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
						It.IsAny<string>()).Result)
				.Throws(new InvalidOperationException("Can not find Github info."));

			var message = AssertExceptionThrown<InvalidOperationException>(() => customPolicyFile.Object.CreatePRAndMerge("companyCode", "systemIdentifier", "prefix",
				new GitHubAppInfo(), isRollback: false)).Message;
#if WINZOR
			AssertEquals("One or more errors occurred. (Can not find Github info.)\r\nCan not find Github info.", message);
#else
			AssertEquals("One or more errors occurred.\r\nCan not find Github info.", message);
#endif
			mockGitHubClient.Setup(x =>
					x.Repository.Content.GetAllContentsByRef(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
						It.IsAny<string>()).Result)
				.Throws(new InvalidOperationException("Can not find Github info.", new InvalidOperationException("Test exception message.")));

			message = AssertExceptionThrown<InvalidOperationException>(() => customPolicyFile.Object.CreatePRAndMerge("companyCode", "systemIdentifier", "prefix",
				new GitHubAppInfo(), isRollback: false)).Message;
#if WINZOR
			AssertEquals("One or more errors occurred. (Can not find Github info.)\r\nCan not find Github info.\r\nTest exception message.", message);
#else
			AssertEquals("One or more errors occurred.\r\nCan not find Github info.\r\nTest exception message.", message);
#endif
		}

		[TestDate(2025, 01, 13)]
		[TestUtcOffset(0, 0, 0)]
		public void TestCreatePR_IsRollback()
		{
			var customPolicyFile = new Mock<CustomPolicyFileBase> { CallBase = true };
			var mockGitHubClient = new Mock<IGitHubClient>();

			customPolicyFile.Setup(x => x.CreateInstallationClient(It.IsAny<GitHubAppInfo>()))
				.Returns(mockGitHubClient.Object);

			// Mocking GitHub API calls for getting the latest commit
			mockGitHubClient.Setup(x =>
					x.Repository.Commit.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()).Result)
				.Returns(new GitHubCommit(
					nodeId: "12345",
					url: "https://github.com/example/commit/12345",
					label: "Initial commit",
					@ref: "main",
					sha: "abcdef123456",
					user: new User(),
					repository: new Repository(),
					author: new Author(),
					commentsUrl: "https://github.com/example/commit/12345/comments",
					commit: new Commit(),
					committer: new Author(),
					htmlUrl: "https://github.com/example/commit/abcdef123456",
					stats: new GitHubCommitStats(3, 2, 1),
					parents: new List<GitReference> { new GitReference() },
					files: new List<GitHubCommitFile> { new GitHubCommitFile() }));

			var reference = new Reference(@ref: "refs/heads/main", nodeId: "12345",
				url: "https://github.com/example/repo.git", @object: new TagObject());

			// Mocking GitHub API calls for creating a new branch
			mockGitHubClient.Setup(x =>
					x.Git.Reference.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NewReference>()).Result)
				.Returns(reference);

			// Mocking GitHub API calls for getting existing file content
			mockGitHubClient.Setup(x =>
					x.Repository.Content.GetAllContentsByRef(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
						It.IsAny<string>()).Result)
				.Returns(new[]
				{
					new RepositoryContent(
						name: "example.txt",
						path: "path/to/example.txt",
						sha: "abcdef123456",
						size: 1024,
						type: ContentType.File,
						downloadUrl: "https://github.com/example/repo/raw/main/path/to/example.txt",
						url: "https://github.com/example/repo/blob/main/path/to/example.txt",
						gitUrl: "git://github.com/example/repo.git",
						htmlUrl: "https://github.com/example/repo/blob/main/path/to/example.txt",
						encoding: "utf-8",
						encodedContent: Convert.ToBase64String(Encoding.UTF8.GetBytes(policyFileXML)),
						target: "_blank",
						submoduleGitUrl: "git://github.com/example/submodule.git")
				});

			// Mocking GitHub API calls for updating file content
			mockGitHubClient.Setup(x => x.Repository.Content.UpdateFile(It.IsAny<string>(), It.IsAny<string>(),
					It.IsAny<string>(), It.IsAny<UpdateFileRequest>()).Result)
				.Returns(new RepositoryContentChangeSet(
					new RepositoryContentInfo(),
					new Commit(nodeId: "12345",
						url: "https://github.com/example/repo/commit/12345",
						label: "Initial commit",
						@ref: "main",
						sha: "abcdef123456",
						user: new User(),
						repository: new Repository(),
						message: "This is the commit message",
						author: new Committer(),
						committer: new Committer(),
						tree: new GitReference(),
						parents: new List<GitReference> { new GitReference() },
						commentCount: 3,
						verification: new Verification())));

			// Mocking GitHub API calls for updating branch reference
			mockGitHubClient.Setup(x =>
					x.Git.Reference.Update(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
						It.IsAny<ReferenceUpdate>()).Result)
				.Returns(reference);

			// Mocking GitHub API calls for creating a pull request
			mockGitHubClient.Setup(x =>
					x.Repository.PullRequest.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NewPullRequest>())
						.Result)
				.Returns(new PullRequest(
					id: 123,
					nodeId: "abcdefghijkl",
					url: "https://github.com/example/repo/pull/123",
					htmlUrl: "https://github.com/example/repo/pull/123",
					diffUrl: "https://github.com/example/repo/pull/123.diff",
					patchUrl: "https://github.com/example/repo/pull/123.patch",
					issueUrl: "https://github.com/example/repo/issues/123",
					statusesUrl: "https://api.github.com/repos/example/repo/statuses/123",
					number: 123,
					state: ItemState.Open,
					title: "Fix a bug",
					body: "This pull request fixes a bug in the code.",
					createdAt: DateTimeOffset.Now.AddDays(-7),
					updatedAt: DateTimeOffset.Now,
					closedAt: null,
					mergedAt: null,
					head: new GitReference(),
					@base: new GitReference(),
					user: new User(),
					assignee: new User(),
					assignees: new List<User> { new User() },
					draft: false,
					mergeable: true,
					mergeableState: MergeableState.Clean,
					mergedBy: null,
					mergeCommitSha: null,
					comments: 5,
					commits: 3,
					additions: 10,
					deletions: 2,
					changedFiles: 3,
					milestone: new Milestone(),
					locked: false,
					maintainerCanModify: true,
					requestedReviewers: new List<User> { new User() },
					requestedTeams: new List<Team> { new Team() },
					labels: new List<Label> { new Label() },
					activeLockReason: null));

			mockGitHubClient.Setup(x =>
			x.Repository.PullRequest.Merge(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<MergePullRequest>()).Result).Returns(new PullRequestMerge("abcdef123456", true, "Merged pull request"));

			var prUrl = customPolicyFile.Object.CreatePRAndMerge("companyCode", "systemIdentifier", "prefix",
				new GitHubAppInfo(), isRollback: false);
			AssertNotNull(prUrl);
			mockGitHubClient.Verify(x => x.Repository.Content.GetAllContentsByRef(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
				"OnBoarding/companyCode/20250113-000000000"), Times.Exactly(1));
			mockGitHubClient.Verify(x => x.Repository.PullRequest.Merge(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<MergePullRequest>()), Times.Exactly(1));

			prUrl = customPolicyFile.Object.CreatePRAndMerge("companyCode", "systemIdentifier", "prefix",
				new GitHubAppInfo(), isRollback: true);
			AssertNotNull(prUrl);
			mockGitHubClient.Verify(x => x.Repository.Content.GetAllContentsByRef(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
				"Rollback/OnBoarding/companyCode/20250113-000000000"), Times.Exactly(1));
			mockGitHubClient.Verify(x => x.Repository.PullRequest.Merge(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<MergePullRequest>()), Times.Exactly(2));
		}

		static XmlDocument LoadXmlDocument(string xml)
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xml);
			return xmlDoc;
		}

		string GetXmlContent(XmlDocument xmlDoc)
		{
			using (var memoryStream = new MemoryStream())
			{
				xmlDoc.Save(memoryStream);

				memoryStream.Position = 0;
				using (var reader = new StreamReader(memoryStream, Encoding.UTF8))
				{
					return reader.ReadToEnd();
				}
			}
		}
	}
}
