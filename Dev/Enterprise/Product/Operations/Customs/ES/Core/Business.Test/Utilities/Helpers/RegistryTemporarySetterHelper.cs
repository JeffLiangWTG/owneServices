using System;
using Enterprise.Customs.ES.Registry;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ES.Business.Testing
{
	public static class RegistryTemporarySetterHelper
	{
		public static IDisposable SetEST2LMessageVersion(string version) => ESCustomsDataRegistry.Instance.EST2LMessageVersion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, version);

		public static IDisposable SetESExportMessageVersion(string version) => ESCustomsDataRegistry.Instance.ESExportMessageVersion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, version);

		public static IDisposable SetESImportMessageVersion(string version) => ESCustomsDataRegistry.Instance.ESImportMessageVersion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, version);

		public static IDisposable SetCustomsClearanceEmailRecipient(string emailRecipient) => ESCustomsDataRegistry.Instance.CustomsClearanceEmailRecipient.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, emailRecipient);

		public static IDisposable SetCustomsClearanceEmailFrom(string emailFrom) => ESCustomsDataRegistry.Instance.CustomsClearanceEmailFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, emailFrom);

		public static IDisposable SetMailboxEmailAddress(string email) => RawDataRegistry.Instance.MailboxEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, email);

		public static IDisposable SetEnableESInboxMessagesThroughDirectxTInterface(bool enable) => ESCustomsDataRegistry.Instance.EnableESInboxMessagesThroughDirectxTInterface.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enable);

		public static IDisposable SetEnableESMessagingThroughDirectxTInterface(bool enable) => ESCustomsDataRegistry.Instance.EnableESMessagingThroughDirectxTInterface.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enable);

		public static IDisposable SetInboxXTExpirationPeriodDays(int days) => ESCustomsDataRegistry.Instance.InboxXTExpirationPeriodDays.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, days);

		public static IDisposable SetAllowEditEDIMessageBody(bool allow) => ESCustomsDataRegistry.Instance.AllowEditEDIMessageBody.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, allow);
	}
}
