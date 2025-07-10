using System;
using System.Collections.Generic;
using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;
using Amazon.Runtime;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Business;

sealed class AWSHelperTest : TestCase
{
	public void TestGetReadOnlyAccessKeysWithAssumeRole()
	{
		// Arrange
		var mockClient = new Mock<AmazonIdentityManagementServiceClient>();
		var mockClientProvider = new Mock<IAWSIAMClientProvider>();
		var mockRegistryHelper = new Mock<IWindowsRegistryHelper>();
		var bucketName = "xyz-apac-012345678901-abcdef-001-pri";
		var username = "xyz.abcdef.001.ro";

		mockClientProvider.Setup(provider => provider.GetClientWithAssumeRole(It.IsAny<AWSCredentials>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
			.Returns(mockClient.Object);
		mockRegistryHelper.Setup(helper => helper.GetAWSAccessKeyFromLocalHostWindowsRegistry()).Returns("accessKey;secretKey;externalId");

		mockClient.Setup(client => client.ListAccessKeys(It.IsAny<ListAccessKeysRequest>()))
			.Returns(new ListAccessKeysResponse
			{
				AccessKeyMetadata = new List<AccessKeyMetadata>()
			});

		mockClient.Setup(client => client.CreateAccessKey(It.IsAny<CreateAccessKeyRequest>()))
			.Returns(new CreateAccessKeyResponse
			{
				AccessKey = new AccessKey()
			});

		var helper = new AWSHelper(mockClientProvider.Object, mockRegistryHelper.Object);

		// Act
		var result = helper.GetReadOnlyAccessKeysWithAssumeRole(bucketName);

		// Assert
		AssertNotNull(result);
		mockClient.Verify(client => client.ListAccessKeys(It.Is<ListAccessKeysRequest>(r => r.UserName == username)), Times.Once);
		mockClient.Verify(client => client.DeleteAccessKey(It.IsAny<DeleteAccessKeyRequest>()), Times.Never);
		mockClient.Verify(client => client.CreateAccessKey(It.Is<CreateAccessKeyRequest>(r => r.UserName == username)), Times.Once);
	}

	public void TestGetReadOnlyAccessKeysWithAssumeRole_DeletesExistingAccessKeys()
	{
		// Arrange
		var mockClient = new Mock<AmazonIdentityManagementServiceClient>();
		var mockClientProvider = new Mock<IAWSIAMClientProvider>();
		var mockRegistryHelper = new Mock<IWindowsRegistryHelper>();
		var bucketName = "xyz-apac-012345678901-abc123-001-pri";
		var username = "xyz.abc123.001.ro";

		mockClientProvider.Setup(provider => provider.GetClientWithAssumeRole(It.IsAny<AWSCredentials>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
			.Returns(mockClient.Object);
		mockRegistryHelper.Setup(helper => helper.GetAWSAccessKeyFromLocalHostWindowsRegistry()).Returns("accessKey;secretKey;externalId");

		mockClient.Setup(client => client.ListAccessKeys(It.Is<ListAccessKeysRequest>(r => r.UserName == username)))
			.Returns(new ListAccessKeysResponse
			{
				AccessKeyMetadata = new List<AccessKeyMetadata>
				{
					new () { AccessKeyId = "oldAccessKey1" },
					new () { AccessKeyId = "oldAccessKey2" },
				}
			});

		mockClient.Setup(client => client.CreateAccessKey(It.IsAny<CreateAccessKeyRequest>()))
			.Returns(new CreateAccessKeyResponse
			{
				AccessKey = new AccessKey()
			});

		var helper = new AWSHelper(mockClientProvider.Object, mockRegistryHelper.Object);

		// Act
		var result = helper.GetReadOnlyAccessKeysWithAssumeRole(bucketName);

		// Assert
		AssertNotNull(result);
		mockClient.Verify(client => client.ListAccessKeys(It.Is<ListAccessKeysRequest>(r => r.UserName == username)), Times.Once);
		mockClient.Verify(client => client.DeleteAccessKey(It.IsAny<DeleteAccessKeyRequest>()), Times.Exactly(2));
		mockClient.Verify(client => client.CreateAccessKey(It.Is<CreateAccessKeyRequest>(r => r.UserName == username)), Times.Once);
	}

	public void TestGetReadOnlyAccessKeysWithAssumeRole_InvalidBucketNameFormat_ThrowsException()
	{
		// Arrange
		var mockClient = new Mock<AmazonIdentityManagementServiceClient>();
		var mockClientProvider = new Mock<IAWSIAMClientProvider>();
		var mockRegistryHelper = new Mock<IWindowsRegistryHelper>();

		var helper = new AWSHelper(mockClientProvider.Object, mockRegistryHelper.Object);

		// Act & Assert
		var ex = AssertExceptionThrown<FormatException>(() => helper.GetReadOnlyAccessKeysWithAssumeRole("some-wrong-1234-format"));
		AssertEquals("Unknown bucket name format. Unable to determine username.", ex.Message);
	}

	public void TestParseCredentialString_ExtractsKeysCorrectly()
	{
		// Arrange
		var helper = new AWSHelper();

		// Act
		var result = helper.ParseCredentialString("accessKey;secretKey;external");

		//Assert
		AssertEquals("accessKey", result.AccessKeyId);
		AssertEquals("secretKey", result.SecretKey);
		AssertEquals("external", result.ExternalId);
	}

	public void TestParseCredentialString_InvalidString_ThrowsException()
	{
		// Arrange
		var helper = new AWSHelper();
		// Act & Assert
		var ex = AssertExceptionThrown<FormatException>(() => helper.ParseCredentialString("some.Wrong / Formatting"));
		AssertEquals("Invalid credential string format.", ex.Message);
	}
}
