using System;
using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.MailManager.ExternalMailInterface.IMAP;
using MailKit;

namespace Enterprise.MailManager.ExternalMailInterface
{
	internal class MailServiceHelper
	{
		internal MailServiceHelper(MailService mailService, MailServerConnectionConfiguration connectionConfiguration, UserPasswordAuthConfiguration userPasswordAuthConfiguration)
			: this(mailService, connectionConfiguration)
		{
			this.userPasswordAuthConfiguration = userPasswordAuthConfiguration;
		}

		internal MailServiceHelper(MailService mailService, MailServerConnectionConfiguration connectionConfiguration, IOAuth2Configuration oAuth2Configuration)
			: this(mailService, connectionConfiguration)
		{
			this.oAuth2Configuration = oAuth2Configuration;
			useOAuth2 = true;
		}

		MailServiceHelper(MailService mailService, MailServerConnectionConfiguration connectionConfiguration)
		{
			this.mailService = mailService;
			this.connectionConfiguration = connectionConfiguration;
		}

		readonly MailService mailService;
		readonly MailServerConnectionConfiguration connectionConfiguration;
		readonly UserPasswordAuthConfiguration userPasswordAuthConfiguration;
		readonly IOAuth2Configuration oAuth2Configuration;
		readonly bool useOAuth2;

		internal void Open()
		{
			try
			{
				mailService.Timeout = 30000;
				TryHardConnectAndAuthenticate();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				Close();
				ExceptionHandlingHelper.ThrowFailedToAuthenticateException(e);
			}

			AssociateDisConnectedEvent();
		}

		internal void Close()
		{
			DisassociateDisConnectedEvent();
			mailService.Disconnect(true);
		}

		void AssociateDisConnectedEvent()
		{
			mailService.Disconnected += MailService_Disconnected;
		}

		void DisassociateDisConnectedEvent()
		{
			mailService.Disconnected -= MailService_Disconnected;
		}

		internal void MailService_Disconnected(object sender, DisconnectedEventArgs e)
		{
			if (!e.IsRequested
#if DEBUG
			|| AlwaysReconnectWhenDisconnected
#endif
			)
			{
				TryHardConnectAndAuthenticate();
			}
		}

		internal void TryHardConnectAndAuthenticate()
		{
			var retriesMax = 3;
			for (var retries = 0; retries < retriesMax; ++retries)
			{
				try
				{
					try
					{
						if (!mailService.IsConnected)
						{
							mailService.Connect(connectionConfiguration.Server, connectionConfiguration.Port, SecureSocketOptionsLookup.FromSecureConnectionTypes(connectionConfiguration.SecureConnectionType));
						}
					}
					catch (Exception e) when (e.ShouldReconnect())
					{
						//IOException might be caused by wrong connection type, let's let MailKit decide which connection type is good.
						mailService.Connect(connectionConfiguration.Server, connectionConfiguration.Port);
					}

					TryAuthenticate();

					if (mailService is MailKitImap imap && !imap.Inbox.IsOpen)
					{
						imap.Inbox.Open(FolderAccess.ReadWrite);
					}

					return;
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					if (retries == retriesMax - 1)
					{
						throw;
					}
				}
			}
		}

		internal void TryAuthenticate()
		{
			if (!mailService.IsAuthenticated)
			{
				if (useOAuth2)
				{
					var oAuth = oAuth2Configuration.GetSaslMechanism();
					mailService.Authenticate(oAuth);
				}
				else
				{
					mailService.Authenticate(userPasswordAuthConfiguration.UserName, userPasswordAuthConfiguration.Password);
				}
			}
		}

#if DEBUG
		internal bool AlwaysReconnectWhenDisconnected { get; set; }
#endif

		internal void DeleteMessageByNumbers(List<long> messageNumbers, Action<IList<int>> batchDeleteAction, Action<int> deleteAction, Func<Exception, bool> isMessageNotExistFunc)
		{
			var convertedMessageNumbers = messageNumbers.ConvertAll(i => (int)i - 1); // Index of MailKit is 0 based, while the old Limilabs dll is 1 based, we need -1 here.
			try
			{
				batchDeleteAction(convertedMessageNumbers);
			}
			catch (Exception ex) when (isMessageNotExistFunc(ex))
			{
				// try manually deleting them one by one, since we don't know which one was deleted
				foreach (var msgNum in convertedMessageNumbers)
				{
					try
					{
						deleteAction(msgNum);
					}
					catch (Exception ex2) when (isMessageNotExistFunc(ex2))
					{
						// message already been deleted, do nothing
					}
				}
			}
		}
	}
}
