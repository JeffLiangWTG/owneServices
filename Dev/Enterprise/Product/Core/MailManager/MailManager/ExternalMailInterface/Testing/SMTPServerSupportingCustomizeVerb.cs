#if DEBUG
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Rnwood.SmtpServer;
using Rnwood.SmtpServer.Extensions;
using Rnwood.SmtpServer.Extensions.Auth;
using Rnwood.SmtpServer.Verbs;

namespace Enterprise.MailManager.ExternalMailInterface
{
	public class SMTPServerSupportingCustomizeVerb : Server
	{
		public SMTPServerSupportingCustomizeVerb(CustomizeVerbServerBehaviour behaviour) : base(behaviour)
		{
		}

		public SMTPServerSupportingCustomizeVerb(Dictionary<string, IVerb> extraProcessorVerbs = null)
			: this(new CustomizeVerbServerBehaviour(extraProcessorVerbs))
		{
		}

		new protected CustomizeVerbServerBehaviour Behaviour
		{
			get
			{
				return (CustomizeVerbServerBehaviour)base.Behaviour;
			}
		}

		public event EventHandler<MessageEventArgs> MessageReceived
		{
			add { Behaviour.MessageReceived += value; }
			remove { Behaviour.MessageReceived -= value; }
		}

		public event EventHandler<SessionEventArgs> SessionCompleted
		{
			add { Behaviour.SessionCompleted += value; }
			remove { Behaviour.SessionCompleted -= value; }
		}

		public event EventHandler<SessionEventArgs> SessionStarted
		{
			add { Behaviour.SessionStarted += value; }
			remove { Behaviour.SessionStarted -= value; }
		}

		public event EventHandler<MessageEventArgs> MessageCompleted
		{
			add { Behaviour.MessageCompleted += value; }
			remove { Behaviour.MessageCompleted -= value; }
		}
	}

	public class CustomizeVerbServerBehaviour : IServerBehaviour
	{
		public CustomizeVerbServerBehaviour(Dictionary<string, IVerb> extraProcessorVerbs)
		{
			PortNumber = 25;
			this.extraProcessorVerbs = extraProcessorVerbs;
		}

		public virtual void OnMessageReceived(IConnection connection, Message message)
		{
			if (MessageReceived != null)
			{
				MessageReceived(this, new MessageEventArgs(message));
			}
		}

		public virtual string DomainName
		{
			get { return System.Environment.MachineName; }
		}

		public virtual IPAddress IpAddress
		{
			get { return IPAddress.Any; }
		}

		public virtual int PortNumber { get; private set; }

		public bool IsSSLEnabled(IConnection connection) => false;

		public bool IsSessionLoggingEnabled(IConnection connection) => true;

		public virtual long? GetMaximumMessageSize(IConnection connection) => null;

		public void OnMessageRecipientAdding(IConnection connection, Message message, string recipient) { }

		public virtual IEnumerable<IExtension> GetExtensions(IConnection connection) => new List<IExtension>();

		public virtual void OnSessionCompleted(IConnection connection, ISession session)
		{
			if (SessionCompleted != null)
			{
				SessionCompleted(this, new SessionEventArgs(session));
			}
		}

		public void OnSessionStarted(IConnection connection, ISession session)
		{
			if (extraProcessorVerbs != null)
			{
				foreach (var verb in extraProcessorVerbs)
				{
					connection.VerbMap.SetVerbProcessor(verb.Key, verb.Value);
				}
			}

			if (SessionStarted != null)
			{
				SessionStarted(this, new SessionEventArgs(session));
			}
		}

		public virtual int GetReceiveTimeout(IConnection connection) => (int)new TimeSpan(0, 5, 0).TotalMilliseconds;

		public virtual AuthenticationResult ValidateAuthenticationCredentials(IConnection connection, IAuthenticationRequest request) => AuthenticationResult.Success;

		public virtual void OnMessageStart(IConnection connection, string from) { }

		public virtual bool IsAuthMechanismEnabled(IConnection connection, IAuthMechanism authMechanism) => true;

		public void OnCommandReceived(IConnection connection, SmtpCommand command) { }

		public IMessage CreateMessage(IConnection connection) => new Message(connection.Session);

		public virtual void OnMessageCompleted(IConnection connection)
		{
			if (MessageCompleted != null)
			{
				MessageCompleted(this, new MessageEventArgs(connection.CurrentMessage));
			}
		}

		internal readonly Dictionary<string, IVerb> extraProcessorVerbs;

		public X509Certificate GetSSLCertificate(IConnection connection) => null;

		public event EventHandler<MessageEventArgs> MessageCompleted;
		public event EventHandler<MessageEventArgs> MessageReceived;
		public event EventHandler<SessionEventArgs> SessionCompleted;
		public event EventHandler<SessionEventArgs> SessionStarted;
	}
}
#endif
