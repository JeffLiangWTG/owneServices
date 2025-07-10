using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;
using Amazon.Runtime;

namespace Enterprise.DataTools.DbBackupAndRestore.Business;

public interface IAWSHelper
{
	string GetReadOnlyAccessKeysWithAssumeRole(string bucketName);
}

public class AWSHelper : IAWSHelper
{
	readonly IAWSIAMClientProvider clientProvider;
	readonly IWindowsRegistryHelper windowsRegistryHelper;

	const string BucketNamePattern = @"^([a-z]+)-[a-z]+-([0-9]+)-([a-z0-9]+)-(\d+)-[a-z]+$";
	const string CredentialPattern = @"^(.*);(.*);(.*)$";

	public AWSHelper()
	{
		clientProvider = new AWSIAMClientProvider();
		windowsRegistryHelper = new WindowsRegistryHelper();
	}

	internal AWSHelper(IAWSIAMClientProvider clientProvider, IWindowsRegistryHelper windowsRegistryHelper)
	{
		this.clientProvider = clientProvider;
		this.windowsRegistryHelper = windowsRegistryHelper;
	}

	public string GetReadOnlyAccessKeysWithAssumeRole(string bucketName)
	{
		var match = Regex.Match(bucketName, BucketNamePattern);

		if (!match.Success)
		{
			throw new FormatException("Unknown bucket name format. Unable to determine username.");
		}

		var username = $"{match.Groups[1].Value.Trim()}.{match.Groups[3].Value.Trim()}.{match.Groups[4].Value.Trim()}.ro";
		var accountId = match.Groups[2].Value.Trim();
		const string roleName = "local-scuser-role";
		const string sessionName = "CreateReadOnlyKeys";
		var roleArn = $"arn:aws:iam::{accountId}:role/{roleName}";

		var parsedCredentials = ParseCredentialString(windowsRegistryHelper.GetAWSAccessKeyFromLocalHostWindowsRegistry());
		var client = clientProvider.GetClientWithAssumeRole(new BasicAWSCredentials(parsedCredentials.AccessKeyId, parsedCredentials.SecretKey), parsedCredentials.ExternalId, roleArn, sessionName);

		var accessKeys = ListAccessKeys(client, username);

		foreach (var accessKey in accessKeys)
		{
			DeleteAccessKey(client, username, accessKey.AccessKeyId);
		}

		var newAccessKey = CreateAccessKey(client, username);

		return $"KeyId={newAccessKey.AccessKeyId};Secret={newAccessKey.SecretAccessKey}";
	}

	internal ParsedCredentials ParseCredentialString(string accessKeyAndSecret)
	{
		var match = Regex.Match(accessKeyAndSecret, CredentialPattern);

		if (!match.Success)
		{
			throw new FormatException("Invalid credential string format.");
		}

		return new ParsedCredentials()
		{
			AccessKeyId = match.Groups[1].Value.Trim(),
			SecretKey = match.Groups[2].Value.Trim(),
			ExternalId = match.Groups[3].Value.Trim()
		};
	}

	static List<AccessKeyMetadata> ListAccessKeys(AmazonIdentityManagementServiceClient client, string username)
	{
		var listAccessKeysResponse = client.ListAccessKeys(new ListAccessKeysRequest
		{
			UserName = username
		});

		return listAccessKeysResponse.AccessKeyMetadata;
	}

	static void DeleteAccessKey(AmazonIdentityManagementServiceClient client, string username, string accessKeyId)
	{
		client.DeleteAccessKey(new DeleteAccessKeyRequest
		{
			UserName = username,
			AccessKeyId = accessKeyId
		});
	}

	static AccessKey CreateAccessKey(AmazonIdentityManagementServiceClient client, string username)
	{
		var createAccessKeyResponse = client.CreateAccessKey(new CreateAccessKeyRequest
		{
			UserName = username
		});

		return createAccessKeyResponse.AccessKey;
	}

	internal class ParsedCredentials
	{
		public string AccessKeyId { get; set; }
		public string SecretKey { get; set; }
		public string ExternalId { get; set; }
	}
}
