using System;
using System.Collections;
using CargoWise.Application;
using Enterprise.EConversation.ServiceTasks;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService("NCV",
	"Neo eConversations Email Processor",
	"MAI",
	typeof(NeoConversationEmailProcessorServiceProvider),
	MinimumPeriod = EmailProcessorServiceProvider.MinimumPeriod,
	DefaultScheduleRunEvery = "30seconds",
	CanRunInAnyBranch = true,
	ActiveByDefault = false)]

namespace Enterprise.EConversation.ServiceTasks
{
	public class NeoConversationEmailProcessorServiceProvider : EmailProcessorServiceProvider
	{
		readonly IEmailReaderFactory emailReaderFactory;
		readonly IOAuth2MailboxSettings mailboxSettings;

		public NeoConversationEmailProcessorServiceProvider()
			: this(new EmailReaderFactory(), GlowRegistry.Instance.NeoConversationsMailBox)
		{
		}

		public NeoConversationEmailProcessorServiceProvider(IEmailReaderFactory factory, IOAuth2MailboxSettings settings)
		{
			emailReaderFactory = factory;
			mailboxSettings = settings;
		}

		protected override Guid RecipientGroupPk => WebDataRegistry.Instance.WebAdminsEmailNotificationGroup.Value;

		protected override IGroupSourceLocator GroupLocator => GroupSourceLocator.GetFromRegistryItem(WebDataRegistry.Instance.WebAdminsEmailNotificationGroup);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Translation not needed")]
		protected override bool HasValidMailboxSettings()
		{
			var result = true;

			if (!GlowRegistry.Instance.NeoEnableConversations.Value)
			{
				Log(LogType.Information, false, $"The registry item '{GlowRegistry.Instance.NeoEnableConversations.Name}' is not enabled");
				result = false;
			}

			if (!mailboxSettings.IsValid(out var errorMessages))
			{
				foreach (var error in errorMessages)
				{
					Log(LogType.Information, false, error);
				}
				result = false;
			}
			return result;
		}

		protected override IEmailReader GetNewEmailReader()
		{
			return emailReaderFactory.Create(mailboxSettings, ServiceLogger);
		}

		protected override bool IsVerboseModeFromRegistry => GlowRegistry.Instance.NeoEnableVerboseModeOnEmailProcessors.Value;

		protected override bool ProcessEmailCore(Email email)
		{
			var emailProcessors = ObjectFactory.Get<ArrayList>("NeoEmailProcessors");

			foreach (IBusinessObjectEmailProcessor emailProcessor in emailProcessors)
			{
				if (emailProcessor.CreateAndProcessMailItem(email, this))
				{
					return true;
				}
			}

			return true;
		}
	}
}
