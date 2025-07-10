using Amazon.IdentityManagement;
using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;

namespace Enterprise.DataTools.DbBackupAndRestore.Business;

interface IAWSIAMClientProvider
{
	AmazonIdentityManagementServiceClient GetClientWithAssumeRole(AWSCredentials credentials, string externalId, string roleArn, string roleSessionName);
}

class AWSIAMClientProvider : IAWSIAMClientProvider
{
	public AmazonIdentityManagementServiceClient GetClientWithAssumeRole(AWSCredentials credentials, string externalId, string roleArn, string roleSessionName)
	{
		var stsClient = new AmazonSecurityTokenServiceClient(credentials);

		var assumeRoleResponse = stsClient.AssumeRole(new AssumeRoleRequest
		{
			ExternalId = externalId,
			RoleArn = roleArn,
			RoleSessionName = roleSessionName
		});

		return new AmazonIdentityManagementServiceClient(assumeRoleResponse.Credentials);
	}
}
