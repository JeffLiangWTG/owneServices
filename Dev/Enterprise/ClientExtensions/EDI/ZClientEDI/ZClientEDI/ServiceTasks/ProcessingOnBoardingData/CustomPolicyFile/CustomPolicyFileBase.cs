using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.Common;
using CargoWise.Types;
using Octokit;
using WTG.IdentitySecurity;
using AuthenticationType = Octokit.AuthenticationType;
using ProductHeaderValue = Octokit.ProductHeaderValue;

namespace Enterprise.Client.EDI.ServiceTasks.ProcessingOnBoardingData
{
	public abstract class CustomPolicyFileBase
	{
		public string CreatePRAndMerge(string companyCode, string systemUniqueIdentifier, string validTokenIssuerPrefix, GitHubAppInfo gitHubAppInfo, bool isRollback = false)
		{
			try
			{
				return CreateAndMerge(companyCode, systemUniqueIdentifier, validTokenIssuerPrefix, gitHubAppInfo, isRollback);
			}
			catch (Exception ex) when (!ex.IsCriticalException() && ex is not InvalidOperationException)
			{
				var message = string.Join(System.Environment.NewLine, ex.FlattenInnerExceptions().Select(e => e.Message));
				throw new InvalidOperationException(message);
			}
		}

		string CreateAndMerge(string companyCode, string systemUniqueIdentifier, string validTokenIssuerPrefix, GitHubAppInfo gitHubAppInfo, bool isRollback)
		{
			var branchName = isRollback ? $"Rollback/OnBoarding/{companyCode}/{ZDateTime.UtcNow:yyyyMMdd-HHmmssfff}" : $"OnBoarding/{companyCode}/{ZDateTime.UtcNow:yyyyMMdd-HHmmssfff}";

			var installationClient = CreateInstallationClient(gitHubAppInfo);

			var latestCommit = installationClient.Repository.Commit.Get(gitHubAppInfo.RepositoryOwner, gitHubAppInfo.RepositoryName, "refs/heads/master").Result;
			var shaOfLatestCommit = latestCommit.Sha;

			var branchReference = installationClient.Git.Reference.Create(gitHubAppInfo.RepositoryOwner, gitHubAppInfo.RepositoryName, new NewReference($"refs/heads/{branchName}", shaOfLatestCommit)).Result;

			var existingFile = installationClient.Repository.Content.GetAllContentsByRef(gitHubAppInfo.RepositoryOwner, gitHubAppInfo.RepositoryName, gitHubAppInfo.FilePath, branchName).Result;

			var updateContent = UpdatePolicyFile(existingFile[0].Content, companyCode, systemUniqueIdentifier, validTokenIssuerPrefix, isRollback);

			var updateRequest = new UpdateFileRequest($"Modified CustomPolicy", updateContent, existingFile[0].Sha, branchName);
			var contentChangeSet = installationClient.Repository.Content.UpdateFile(gitHubAppInfo.RepositoryOwner, gitHubAppInfo.RepositoryName, gitHubAppInfo.FilePath, updateRequest).Result;
			installationClient.Git.Reference.Update(gitHubAppInfo.RepositoryOwner, gitHubAppInfo.RepositoryName, branchReference.Ref, new ReferenceUpdate(contentChangeSet.Commit.Sha, force: true)).Wait();

			var title = $"{companyCode} - Custom Policy File Change";
			var newPr = new NewPullRequest(title, branchName, "master");

			var pullRequest = installationClient.Repository.PullRequest.Create(gitHubAppInfo.RepositoryOwner, gitHubAppInfo.RepositoryName, newPr).Result;

			var pullRequestMerge = installationClient.Repository.PullRequest.Merge(gitHubAppInfo.RepositoryOwner, gitHubAppInfo.RepositoryName, pullRequest.Number, new MergePullRequest { CommitTitle = title }).Result;

			if (!pullRequestMerge.Merged)
			{
				throw new InvalidOperationException($"Failed to merge pull request: {pullRequest.HtmlUrl}");
			}

			return pullRequest.HtmlUrl;
		}

		public virtual IGitHubClient CreateInstallationClient(GitHubAppInfo gitHubAppInfo)
		{
			var token = GetAppToken(gitHubAppInfo.PrivateKey, gitHubAppInfo.AppId);

			var appClient = new GitHubClient(new ProductHeaderValue(gitHubAppInfo.AppName))
			{
				Credentials = new Credentials(token, AuthenticationType.Bearer)
			};

			var installation = appClient.GitHubApps.GetRepositoryInstallationForCurrent(gitHubAppInfo.RepositoryOwner, gitHubAppInfo.RepositoryName).Result;
			var appInstallationToken = appClient.GitHubApps.CreateInstallationToken(installation.Id).Result;

			var installationClient = new GitHubClient(new ProductHeaderValue(gitHubAppInfo.AppName))
			{
				Credentials = new Credentials(appInstallationToken.Token)
			};
			return installationClient;
		}

		string UpdatePolicyFile(string fileContent, string companyCode, string systemUniqueIdentifier, string validTokenIssuerPrefix, bool isRollback)
		{
			var xmlDocument = new XmlDocument();
			var xmlReaderSettings = new XmlReaderSettings();
			xmlReaderSettings.DtdProcessing = DtdProcessing.Prohibit;
			using (var stringReader = new StringReader(fileContent))
			{
				using (var xmlReader = XmlReader.Create(stringReader, xmlReaderSettings))
				{
					xmlDocument.Load(xmlReader);
				}
			}

			UpdatePolicyFileCore(xmlDocument, companyCode, systemUniqueIdentifier, validTokenIssuerPrefix, isRollback);

			var ms = new MemoryStream();
			xmlDocument.Save(ms);
			return System.Text.Encoding.UTF8.GetString(ms.ToArray());
		}

		public abstract void UpdatePolicyFileCore(XmlDocument xmlDocument, string companyCode, string systemUniqueIdentifier, string validTokenIssuerPrefix, bool isRollback);

		string GetAppToken(string privateKeyContent, string appId)
		{
			var rsa = RSAKeyProvider.ImportPrivateKey(privateKeyContent);
			var time = ZDateTime.UtcNow.ToDateTime();
			var payload = new JwtPayload(null, null, null, time, time.AddMinutes(10), time)
			{
				{ "iss", appId },
			};
			var jwtSecurityToken = JwtSecurity.GenerateSignedJwt(rsa, payload);
			var handler = new JwtSecurityTokenHandler();
			return handler.WriteToken(jwtSecurityToken);
		}

		public static XmlNamespaceManager BuildXmlNamespaceManager(XmlDocument xmlDocument)
		{
			if (xmlDocument == null)
			{
				throw new ArgumentNullException(nameof(xmlDocument));
			}

			var xmlns = xmlDocument["TrustFrameworkPolicy"].GetAttribute("xmlns");
			var xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
			xmlNamespaceManager.AddNamespace("az", xmlns);
			return xmlNamespaceManager;
		}
	}

	public class GitHubAppInfo
	{
		public string AppId { get; set; }
		public string AppName { get; set; }
		public string RepositoryOwner { get; set; }
		public string RepositoryName { get; set; }
		public string FilePath { get; set; }
		public string PrivateKey { get; set; }
	}
}
