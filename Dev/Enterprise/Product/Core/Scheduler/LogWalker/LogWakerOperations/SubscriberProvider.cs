using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.LogWalker
{
	public class SubscriberProvider
	{
		public LogSubscriber[] GetAllSubscribersWithValidationAndAppendingToNotificationLog(ILogger notifier)
		{
			var result = AllLogSubscribers.ToArray();

			ValidateAndLogSubscribers(notifier, result);

			return result;
		}

		protected internal virtual IEnumerable<LogSubscriber> AllLogSubscribers
		{
			get
			{
				foreach (var subscriber in new SystemLogSubscribers())
				{
					yield return subscriber;
				}

				var clientHook = ClientHookLoader.Instance.ClientHook;
				if (clientHook != null && clientHook.LogSubscribers != null)
				{
					foreach (LogSubscriber clientSubscriber in clientHook.LogSubscribers)
					{
#if DEBUG
						if (!clientSubscriber.IsClientSpecificSubscriber)
						{
							throw new InvalidLogSubscriberException(string.Format(CultureInfo.InvariantCulture, "Client subscriber [{0}] must have IsClientSpecificSubscriber = true.", clientSubscriber.Name));
						}
#endif
						yield return clientSubscriber;
					}
				}
			}
		}

		[System.Diagnostics.Conditional("DEBUG")]
		void ValidateAndLogSubscribers(ILogger logger, LogSubscriber[] subscribers)
		{
			var names = new HashSet<string>();
			var listNotificationBuilder = new ZStringBuilder();

			foreach (var subscriber in subscribers)
			{
				if (names.Contains(subscriber.Name))
				{
					throw new InvalidLogSubscriberException(string.Format(CultureInfo.InvariantCulture, "There is already a subscriber called [{0}].", subscriber.Name));
				}
				else
				{
					names.Add(subscriber.Name);
					listNotificationBuilder.Append(subscriber.FriendlyName);
				}
			}

			logger.Log(LogType.Debug, "Registered Subscribers:\r\n\r\n" + listNotificationBuilder.ToStringWithNewLineBetweenAppends());
		}
	}

	[Serializable]
	public class InvalidLogSubscriberException : Exception
	{
		public InvalidLogSubscriberException()
		{
		}

		public InvalidLogSubscriberException(string message) : base(message)
		{
		}

		public InvalidLogSubscriberException(string message, Exception innerException) : base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected InvalidLogSubscriberException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
