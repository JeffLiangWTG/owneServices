using System;
using System.ServiceModel;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub2.Common;
using MessageSchemaType = CargoWise.eHub2.Common.MessageSchemaType;
using System.Text.RegularExpressions;

namespace CargoWise.eHub.Gateway
{
	public class eHub2MessageHandler : InboxMessageHandler
	{
		public override void Handle(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message)
		{
			MessageSchemaType messageSchemaType;
			var eHub2Message = new eHub2GatewayMessage
			{
				SenderID = senderID,
				ApplicationCode = message.ApplicationCode,
				ClientID = message.ClientID,
				EmailSubject = message.EmailSubject,
				FileName = message.FileName,
				MessageStream = message.MessageStream.DecodeAndDecompress(),
				MessageTrackingID = message.MessageTrackingID,
				SchemaName = message.SchemaName,
				SchemaType = Enum.TryParse(message.SchemaType.ToString(), out messageSchemaType) ? messageSchemaType : MessageSchemaType.Xml
			};

			using (ChannelFactory<IEHub2Reciever> channelFactory = NewChannelFactory())
			{ 
				try
				{
					channelFactory.Endpoint.Contract.SessionMode = SessionMode.Allowed;
					var channel = channelFactory.CreateChannel();
					channel.SendMessage(eHub2Message);
				}
				catch (FaultException ex)
				{
					if (IsInfrastructureException(ex))
						throw new SystemUnderMaintananceException(ex);

					throw;
				}
				catch (Exception ex)
				{
					throw new SystemUnderMaintananceException(ex);
				}
			}
		}

		bool IsInfrastructureException(FaultException faultException)
		{
			var HRESULT0xC0C0163CFromBizTalkAdapterWcfPattern = @"System.Runtime.InteropServices.COMException: Exception from HRESULT: 0xC0C0163C\s*at Microsoft.BizTalk.Adapter.Wcf.Runtime.BizTalkAsyncResult.End()";
			var sqlExceptionMessage = "The pipeline component threw database related exception.";
			var biztalkException = "Microsoft.BizTalk.Message.Interop.BTSException";
			var isInfrastructureException = false;

			if (Regex.IsMatch(faultException.ToString(), HRESULT0xC0C0163CFromBizTalkAdapterWcfPattern))
			{
				isInfrastructureException = true;
			}
			else if (Regex.IsMatch(faultException.ToString(), sqlExceptionMessage))
			{
				isInfrastructureException = true;
			}
			else if (Regex.IsMatch(faultException.ToString(), biztalkException) && !Regex.IsMatch(faultException.ToString(), "Invalid Message"))
			{
				isInfrastructureException = true;
			}

			return isInfrastructureException;
		}

		public virtual ChannelFactory<IEHub2Reciever> NewChannelFactory()
		{
			return new ChannelFactory<IEHub2Reciever>("eHub2GatewayService");
		}
	}
}
