#if DEBUG
namespace Enterprise.MailManager.ExternalMailInterface
{
	using System;
	using System.Collections.Generic;
	using System.Security.Cryptography.X509Certificates;
	using Rnwood.SmtpServer;
	using Rnwood.SmtpServer.Extensions;
	using Rnwood.SmtpServer.Extensions.Auth;

	class ServerSupportingAuthLogin : Server
	{
		public ServerSupportingAuthLogin(int portNumber, string username, string password)
			: this(portNumber, username, password, false)
		{ }

		public ServerSupportingAuthLogin(int portNumber, string username, string password, bool requiresSecureConnection)
			: base(new Behavior(portNumber, username, password, requiresSecureConnection, null))
		{ }

		public ServerSupportingAuthLogin(int portNumber, string username, string password, bool requiresSecureConnection, X509Certificate sslCertificate)
			: base(new Behavior(portNumber, username, password, requiresSecureConnection, sslCertificate))
		{ }

		class Behavior : DefaultServerBehaviour
		{
			public Behavior(int portNumber, string username, string password, bool requiresSecureConnection, X509Certificate sslCertificate)
				: base(portNumber, sslCertificate)
			{
				this.username = username;
				this.password = password;
				this.requiresSecureConnection = requiresSecureConnection;
			}

			readonly string username;
			readonly string password;
			readonly bool requiresSecureConnection;

			public override IEnumerable<IExtension> GetExtensions(IConnection connection)
			{
				List<IExtension> list = new List<IExtension>(base.GetExtensions(connection));
				list.Add(new StartTlsExtension());
				if (requiresSecureConnection)
				{
					list.Add(new ExtensionRequiringSecureConnection(new AuthExtension()));
				}
				else
				{
					list.Add(new AuthExtension());
				}
				return list;
			}

			public override bool IsAuthMechanismEnabled(IConnection connection, IAuthMechanism authMechanism)
			{
				return (!requiresSecureConnection || connection.Session.SecureConnection) && authMechanism.Identifier == "LOGIN";
			}

			public override AuthenticationResult ValidateAuthenticationCredentials(IConnection connection, IAuthenticationRequest request)
			{
				if (requiresSecureConnection && !connection.Session.SecureConnection)
				{
					throw new SmtpServerException(new SmtpResponse(StandardSmtpResponseCode.BadSequenceOfCommands, "Must issue a STARTTLS command first."));
				}
				var loginRequest = request as UsernameAndPasswordAuthenticationRequest;
				return loginRequest != null && loginRequest.Username == username && loginRequest.Password == password ? AuthenticationResult.Success : AuthenticationResult.Failure;
			}

			public override X509Certificate GetSSLCertificate(IConnection connection)
			{
				var cert = base.GetSSLCertificate(connection) ?? ServerCertificate.GetSSLCertificate();
				return cert;
			}

			class ExtensionRequiringSecureConnection : IExtension
			{
				public ExtensionRequiringSecureConnection(IExtension extension)
				{
					this.extension = extension;
				}

				readonly IExtension extension;

				public IExtensionProcessor CreateExtensionProcessor(IConnection connection)
				{
					return new ExtensionProcessorRequiringSecureConnection(connection, extension.CreateExtensionProcessor(connection));
				}

				class ExtensionProcessorRequiringSecureConnection : IExtensionProcessor
				{
					public ExtensionProcessorRequiringSecureConnection(IConnection connection, IExtensionProcessor extensionProcessor)
					{
						this.connection = connection;
						this.extensionProcessor = extensionProcessor;
					}

					readonly IConnection connection;
					readonly IExtensionProcessor extensionProcessor;

					public string[] EHLOKeywords
					{
						get
						{
							if (connection.Session.SecureConnection)
							{
								return extensionProcessor.EHLOKeywords;
							}
							else
							{
								return Array.Empty<string>();
							}
						}
					}
				}
			}
		}

		new protected DefaultServerBehaviour Behaviour
		{
			get
			{
				return (DefaultServerBehaviour)base.Behaviour;
			}
		}

		public event EventHandler<SessionEventArgs> SessionCompleted
		{
			add { Behaviour.SessionCompleted += value; }
			remove { Behaviour.SessionCompleted -= value; }
		}

		public event EventHandler<MessageEventArgs> MessageCompleted
		{
			add { Behaviour.MessageCompleted += value; }
			remove { Behaviour.MessageCompleted -= value; }
		}
	}
}
#endif
