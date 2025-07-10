using System;
using Amazon.S3;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class AWSPersisterConfigTest : TransactionedTestCase
	{
		public void TestValueFromRegistry()
		{
			var config = new AWSPersisterConfig();

			//Default registry values
			AssertExceptionThrown<ExternalStorageAccessException>("BucketName", () => _ = config.BucketName);
			AssertExceptionThrown<ExternalStorageAccessException>("ServiceURL", () => _ = config.ServiceURL);
			AssertExceptionThrown<ExternalStorageAccessException>("AccessKeyId", () => _ = config.AccessKeyId);
			AssertExceptionThrown<ExternalStorageAccessException>("SecretAccessKey", () => _ = config.SecretAccessKey);
			AssertEquals("ConnectionTimeOut", 30, config.ConnectionTimeOut);
			AssertEquals("UseChunkEncoding", true, config.UseChunkEncoding);
			AssertEquals("UsePathStyleAddressing", true, config.UsePathStyleAddressing);
			AssertEquals("WriteVerificationMode", true, config.WriteVerificationMode);
			AssertEquals("S3StorageClass", null, config.AWSS3StorageClass);

			//Set value in registry
			SystemDataRegistry.Instance.DocManagerStorageBucketName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "testbucket101");
			SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://s3.test.url.com");
			SystemDataRegistry.Instance.EDocsStorageAccess.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "KeyId=KKKJJJMMM111000111;Secret=7h15115453cR3+");
			SystemDataRegistry.Instance.EDocsStorageConnectionTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 56);
			SystemDataRegistry.Instance.UseChunkEncoding.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			SystemDataRegistry.Instance.UsePathStyleAddressing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			SystemDataRegistry.Instance.DocManagerParanoidModeEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.AWSS3StorageClass.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "GLACIER_IR");

			AssertEquals("BucketName", "testbucket101", config.BucketName);
			AssertEquals("ServiceURL", "https://s3.test.url.com", config.ServiceURL);
			AssertEquals("AccessKeyId", "KKKJJJMMM111000111", config.AccessKeyId);
			AssertEquals("SecretAccessKey", "7h15115453cR3+", config.SecretAccessKey);
			AssertEquals("ConnectionTimeOut", 56, config.ConnectionTimeOut);
			AssertEquals("UseChunkEncoding", false, config.UseChunkEncoding);
			AssertEquals("UsePathStyleAddressing", false, config.UsePathStyleAddressing);
			AssertEquals("WriteVerificationMode", true, config.WriteVerificationMode);
			AssertEquals("S3StorageClass", S3StorageClass.GlacierInstantRetrieval, config.AWSS3StorageClass);
		}

		public void TestValueOverride()
		{
			SystemDataRegistry.Instance.DocManagerStorageBucketName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "testbucket101");
			SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://s3.test.url.com");
			SystemDataRegistry.Instance.EDocsStorageAccess.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "KeyId=KKKJJJMMM111000111;Secret=7h15115453cR3+");
			SystemDataRegistry.Instance.EDocsStorageConnectionTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 56);
			SystemDataRegistry.Instance.UseChunkEncoding.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.UsePathStyleAddressing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.DocManagerParanoidModeEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.AWSS3StorageClass.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "GLACIER_IR");

			//override registry value
			var config = new AWSPersisterConfig("overrideBucket", "overrideURL", "KeyId=overrideKeyId;Secret=overrideSecret", "DEEP_ARCHIVE");

			AssertEquals("BucketName", "overrideBucket", config.BucketName);
			AssertEquals("ServiceURL", "overrideURL", config.ServiceURL);
			AssertEquals("AccessKeyId", "overrideKeyId", config.AccessKeyId);
			AssertEquals("SecretAccessKey", "overrideSecret", config.SecretAccessKey);
			AssertEquals("ConnectionTimeOut", 56, config.ConnectionTimeOut);
			AssertEquals("UseChunkEncoding", true, config.UseChunkEncoding);
			AssertEquals("UsePathStyleAddressing", true, config.UsePathStyleAddressing);
			AssertEquals("WriteVerificationMode", true, config.WriteVerificationMode);
			AssertEquals("BucketName", S3StorageClass.DeepArchive, config.AWSS3StorageClass);
		}

		public void TestExceptions()
		{
			var config = new AWSPersisterConfig();
			SystemDataRegistry.Instance.DocManagerStorageBucketName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			AssertExceptionThrown<ExternalStorageAccessException>("BucketName empty", () => _ = config.BucketName);

			SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			AssertExceptionThrown<ExternalStorageAccessException>("Service URL empty", () => { _ = config.ServiceURL; });

			SystemDataRegistry.Instance.EDocsStorageAccess.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			AssertExceptionThrown<ExternalStorageAccessException>("Credential not set - KeyId", () => { _ = config.AccessKeyId; });
			AssertExceptionThrown<ExternalStorageAccessException>("Credential not set - Secret", () => { _ = config.SecretAccessKey; });

			SystemDataRegistry.Instance.EDocsStorageAccess.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "keyid=bla");
			AssertExceptionThrown<ExternalStorageAccessException>("Credential not set - KeyId", () => { _ = config.AccessKeyId; });
			AssertExceptionThrown<ExternalStorageAccessException>("Credential not set - Secret", () => { _ = config.SecretAccessKey; });

			SystemDataRegistry.Instance.EDocsStorageAccess.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "keyid=bla;secret=meh");
			AssertEquals("KeyId", "bla", config.AccessKeyId);
			AssertEquals("Secret", "meh", config.SecretAccessKey);
		}
	}
}
