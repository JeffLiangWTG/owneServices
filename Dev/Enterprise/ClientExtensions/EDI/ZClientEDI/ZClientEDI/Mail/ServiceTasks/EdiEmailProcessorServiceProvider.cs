using System;
using Enterprise.EConversation.ServiceTasks;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Mail.ServiceTasks
{
	abstract class EdiEmailProcessorServiceProvider : EmailProcessorServiceProvider
	{
		readonly IEmailReaderFactory emailReaderFactory;

		protected EdiEmailProcessorServiceProvider(IEmailReaderFactory emailReaderFactory)
		{
			this.emailReaderFactory = emailReaderFactory;
		}

		protected abstract MailboxSettings MailboxSettings { get; }

		protected override Guid RecipientGroupPk => EDIDataRegistry.Instance.InternalNotificationGroup.Value;

		protected override IGroupSourceLocator GroupLocator => GroupSourceLocator.GetFromRegistryItem(EDIDataRegistry.Instance.InternalNotificationGroup);

		protected override bool IsVerboseModeFromRegistry => EDIDataRegistry.Instance.EnableVerboseModeOnEmailProcessors.Value;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Translation not needed")]
		protected override bool HasValidMailboxSettings()
		{
			var result = true;

			if (string.IsNullOrWhiteSpace(MailboxSettings.Server))
			{
				Log(LogType.Debug, false, "Mail server is not configured");
				result = false;
			}

			if (string.IsNullOrWhiteSpace(MailboxSettings.UserName))
			{
				Log(LogType.Debug, false, "Mail account username is not configured");
				result = false;
			}

			return result;
		}

		protected override IEmailReader GetNewEmailReader()
		{
			return emailReaderFactory.Create(MailboxSettings, ServiceLogger);
		}
	}
}
