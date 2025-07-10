using System;
using System.Text;
using Enterprise.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(S3StorageRegistryDataType))]
	sealed class S3StorageRegistryDataTypeTest : StringRegistryDataTypeTest
	{
		protected override bool IsValidatedOnSetEvenIfEqualDefaultValue => true;

		public void TestEDocsStorageAccess()
		{
			CombinedAssertion(SystemDataRegistry.Instance.EDocsStorageAccess, "KeyId=A1B2C3;Secret=aAbBcCdDeE", "KeyId=D1E2F3;Secret=aAbBcCdDeE");
		}

		public void TestEDocsStorageServiceUrl()
		{
			CombinedAssertion(SystemDataRegistry.Instance.EDocsStorageServiceUrl, "wtg.old.zone", "http://wtg.new.zone");
		}

		public void TestEDocsStorageServiceUrl_InvalidUrls()
		{
			var registryItem = SystemDataRegistry.Instance.EDocsStorageServiceUrl;
			AssertExceptionThrown<RegistryValidationException>("Missing Http or Https", "The URL is invalid, please input a valid URL that must be HTTPS or HTTP.", () => DataType.ValidateBeforeRegistryFormSave(registryItem, "wtg.new.zone", Guid.Empty, Guid.Empty, Guid.Empty));
			AssertExceptionThrown<RegistryValidationException>("Invalid prefix", "The URL is invalid, please input a valid URL that must be HTTPS or HTTP.", () => DataType.ValidateBeforeRegistryFormSave(registryItem, "httttp://wtg.new.zone", Guid.Empty, Guid.Empty, Guid.Empty));
			AssertNoExceptionThrown("Http", () => DataType.ValidateBeforeRegistryFormSave(registryItem, "http://wtg.new.zone", Guid.Empty, Guid.Empty, Guid.Empty));
			AssertNoExceptionThrown("Https", () => DataType.ValidateBeforeRegistryFormSave(registryItem, "https://wtg.new.zone", Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestDocManagerStorageBucketName()
		{
			CombinedAssertion(SystemDataRegistry.Instance.DocManagerStorageBucketName, "bucket name 1", "bucket name 2");
		}

		void CombinedAssertion(StringRegistryItem registryItem, string oldValue, string newValue)
		{
			defaultValueForDataType = registryItem.DefaultValue;
			AssertNotShowNotificationIfS3IsNotEnabled(registryItem, oldValue, newValue);
			AssertNotShowNotificationIfChangingDefaultValue(registryItem, newValue);
			AssertShowNotificationAndNotThrowIfAcceptChanges(registryItem, oldValue, newValue);
			AssertThrowExceptionIfCancelChanges(registryItem, oldValue, newValue);
		}

		void AssertNotificationMessages(StringRegistryItem registryItem)
		{
			AssertEquals("Caption", $"Change {registryItem.Caption}", UnitTestUserNotification.Instance.LastMessage.Caption);
			AssertEquals("Message", $"Changing {registryItem.Caption} might cause all eDocs stored in S3 to be inaccessible.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Confirmation string", SystemDataRegistry.Instance.UpdateS3ConfigContinueString, UnitTestUserNotification.Instance.LastConfirmationStringShown);
			AssertEquals("WasQuestion", true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
		}

		void AssertNotShowNotificationIfS3IsNotEnabled(StringRegistryItem registryItem, string oldValue, string newValue)
		{
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.EDocsStorageProviders.Code.DB))
			using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, oldValue))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNoExceptionThrown(() => DataType.ValidateBeforeRegistryFormSave(registryItem, newValue, Guid.Empty, Guid.Empty, Guid.Empty));
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		void AssertNotShowNotificationIfChangingDefaultValue(StringRegistryItem registryItem, string newValue)
		{
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.EDocsStorageProviders.Code.S3))
			using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryItem.DefaultValue))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNoExceptionThrown(() => DataType.ValidateBeforeRegistryFormSave(registryItem, newValue, Guid.Empty, Guid.Empty, Guid.Empty));
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		void AssertShowNotificationAndNotThrowIfAcceptChanges(StringRegistryItem registryItem, string oldValue, string newValue)
		{
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.EDocsStorageProviders.Code.S3))
			using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, oldValue))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
				AssertNoExceptionThrown("Double check accepted.", () => DataType.ValidateBeforeRegistryFormSave(registryItem, newValue, Guid.Empty, Guid.Empty, Guid.Empty));
				AssertNotificationMessages(registryItem);
			}
		}

		void AssertThrowExceptionIfCancelChanges(StringRegistryItem registryItem, string oldValue, string newValue)
		{
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.EDocsStorageProviders.Code.S3))
			using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, oldValue))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);
				var exception = AssertExceptionThrown<RegistryValidationException>(() => DataType.ValidateBeforeRegistryFormSave(registryItem, newValue, Guid.Empty, Guid.Empty, Guid.Empty));
				AssertEquals("Should throw exception if double check is not accepted.", $"Changing {registryItem.Caption} canceled.", exception.Message);
				AssertNotificationMessages(registryItem);
			}
		}

		string defaultValueForDataType = string.Empty;

		protected override StringRegistryDataType GetNewDataType()
		{
			return new S3StorageRegistryDataType(defaultValueForDataType);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new[]
			{
				new ValidSampleAndBinaryValueInDB("KeyId=A1B2C3;Secret=aAbBcCdDeE", Encoding.Unicode.GetBytes("KeyId=A1B2C3;Secret=aAbBcCdDeE")),
				new ValidSampleAndBinaryValueInDB("wtg.zone", Encoding.Unicode.GetBytes("wtg.zone")),
				new ValidSampleAndBinaryValueInDB("wisetechedocs-unit-tests", Encoding.Unicode.GetBytes("wisetechedocs-unit-tests"))
			};
		}

		protected override object[] GetInvalidSamples()
		{
			return Array.Empty<object>();
		}
	}
}
