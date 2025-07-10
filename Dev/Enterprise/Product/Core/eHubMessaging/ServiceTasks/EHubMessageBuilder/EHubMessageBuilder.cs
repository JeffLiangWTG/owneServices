using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;

namespace Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder
{
	public abstract class EHubMessageBuilder
	{
		protected readonly INotifications notifier;
		protected readonly EDIInterchange interchange;
		readonly string serviceName;

		public EHubMessageBuilder(EDIInterchange interchange, INotifications notifier)
		{
			this.notifier = Argument.NotNull(notifier, "notifier");
			this.interchange = Argument.NotNull(interchange, "interchange");
			serviceName = interchange.TransportModeDescription;
		}

		public IeHubMessage Build()
		{
			if (!PreCheck())
			{
				return null;
			}

			return BuildCore();
		}

		protected virtual bool RequiresMessage()
		{
			return true;
		}

		protected virtual bool FailIfNoSchemaNameFound()
		{
			return true;
		}

		protected abstract IeHubMessage BuildCore();

		protected virtual string GetSchemaName()
		{
			return XmlMessageHelper.GetMessageSchemaName(interchange);
		}

		protected bool InterchangeHasNoMessages
		{
			get { return interchange.ContainedMessages.Count == 0; }
		}

		protected void LogInterchangeError(string message)
		{
			interchange.AddEHubError(message);
			notifier.AddEHubError(message);
		}

		#region Implementation

		bool PreCheck()
		{
			return ValidateNumberOfMessages() && TryGetSchemaName();
		}

		bool ValidateNumberOfMessages()
		{
			if (RequiresMessage() && InterchangeHasNoMessages)
			{
				LogInterchangeError(Res.GetString("faf2391c-248b-4214-aaf9-dd539d305d8b", "Cannot create {0} message: {1} should contain at least one message, but it contains no messages", serviceName, GetInterchangeInfo(interchange)));
				return false;
			}
			return true;
		}

		string GetInterchangeInfo(EDIInterchange interchange)
		{
			return Res.GetString("a3261fe5-5fe5-4901-9399-e53b89e003f8", "Interchange(Number: {0}, Tracking ID: {1}, Application Code: {2})", interchange.EI_InterchangeNum, interchange.EI_SessionGUID, interchange.EI_ApplicationCode);
		}

		bool TryGetSchemaName()
		{
			schemaName = GetSchemaName();

			if (string.IsNullOrEmpty(schemaName) && FailIfNoSchemaNameFound())
			{
				LogInterchangeError(Res.GetString("dc46d879-8a72-4d67-a21d-419b41e725f7", "Cannot create {0} message: schema name cannot be found.", serviceName));
				return false;
			}

			return true;
		}

		#endregion

		protected string schemaName;
	}
}
