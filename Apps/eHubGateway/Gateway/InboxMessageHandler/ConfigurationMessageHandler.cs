using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using eServices.Configuration.Framework;
using eServices.Configuration.Schemas;
using eServices.eHubDataModel.Common;
using eServices.eHubDataModel.eHubTransactions;
using Common.Logging;

namespace CargoWise.eHub.Gateway
{
    class ConfigurationMessageHandler : MessageHandler
    {
        readonly Func<eHubTransactionsContext> contextFactory;
        readonly Func<ConfigurationMessage, IConfigurationHandler> configurationHandlerFactory;
        readonly ILog logger;
        private const string Base64SuccessMessageContent = "H4sIAAAAAAAEAOy9B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/It7kTZu+butiefH/BAAA//+kn3eSCwAAAA==";
        private const string ConfigurationMessageType = "http://www.wisetechglobal.com/Schemas/Configuration#Configuration";

        public ConfigurationMessage ConfigurationMessage { get; set; }

        internal ConfigurationMessageHandler(eHubGatewayMessage message)
            : this(
                () => new eHubTransactionsContext(),
                ConfigurationHandlerFactory.GetConfigurationHandler,
                LogManager.GetLogger(typeof(ConfigurationMessageHandler).Name))
        {
            ConfigurationMessage = ConfigurationMessage.DeserializeFromStream(message.MessageStream.DecodeAndDecompress());
        }

        internal ConfigurationMessageHandler(
            Func<eHubTransactionsContext> contextFactory,
            Func<ConfigurationMessage, IConfigurationHandler> configurationHandlerFactory,
            ILog logger)
        {
            this.contextFactory = contextFactory;
            this.configurationHandlerFactory = configurationHandlerFactory;
            this.logger = logger;
        }

        public override void Handle(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message)
        {
            var handler = configurationHandlerFactory(ConfigurationMessage);
            handler.SenderId = senderID;

            DatabaseAccessHelpers.AccessDatabaseWithRetries(() =>
            {
                using (var context = contextFactory())
                {
                    (handler as IDbContextInjectable)?.SetDbContext(context);

                    var inboxMsg = InboxMsg(senderID, message.ClientID, message.MessageTrackingID, message, message.MessageStream.ReadToEnd(), 3, context);

                    IConfigurationValidation validation = handler as IConfigurationValidation;
                    var failConfiguration = validation?.ValidateAndGenerateResponseIfFailed(ConfigurationMessage);

                    context.eHubInboxMessages.Add(inboxMsg);

                    if (failConfiguration == null)
                    {
                        try
                        {
                            handler.AddOrUpdate(ConfigurationMessage);
                        }
                        catch (Exception ex)
                        {
                            if (validation != null)
                            {
                                var failConfigurationMessage = validation.GenerateErrorConfigurationResponse(ConfigurationMessage, ex.Message);
                                using (var messageStream = ConfigurationMessage.SerializeToStream(failConfigurationMessage))
                                {
                                    inboxMsg.EI_Status = 255;
                                    context.eHubErrors.Add(CreateeHubError(inboxMsg.EI_PK, ex.Message));
                                    var failOutboxContent = new MemoryStream(Encoding.UTF8.GetBytes(ex.ToString())).CompressAndEncode().ReadToEnd();
                                    var failOutboxMsg = OutboxMsg(message.MessageTrackingID.ToString(), inboxMsg.EI_CC_Recipient.Value, inboxMsg.EI_CC_Sender, inboxMsg, "MessageStatusFailed", failOutboxContent, 0, context);
                                    context.eHubOutboxMessages.Add(failOutboxMsg);

                                    var inboxOutboxContent = messageStream.CompressAndEncode().ReadToEnd();
                                    var inboxMsgToSender = InboxMsg(message.ClientID, senderID, InternalNewGuid(), message, inboxOutboxContent, 2, context);
                                    var outboxMsgToSender = OutboxMsg(inboxMsgToSender.EI_MessageTrackingID, inboxMsgToSender.EI_CC_Sender, inboxMsgToSender.EI_CC_Recipient.Value, inboxMsgToSender, ConfigurationMessageType, inboxOutboxContent, 0, context);
                                    context.eHubInboxMessages.Add(inboxMsgToSender);
                                    context.eHubOutboxMessages.Add(outboxMsgToSender);

                                    context.SaveChanges();
                                }
                            }

                            throw;
                        }

                        var outboxMsg = OutboxMsg(inboxMsg.EI_MessageTrackingID, inboxMsg.EI_CC_Recipient.Value, inboxMsg.EI_CC_Sender, inboxMsg, "MessageStatusSuccess", Base64SuccessMessageContent, 0, context);
                        context.eHubOutboxMessages.Add(outboxMsg);
                        context.SaveChanges();
                    }
                    else
                    {
                        var failConfigurationMessage = failConfiguration.Item1;
                        var failConfigurationErrors = failConfiguration.Item2;

                        using (var messageStream = ConfigurationMessage.SerializeToStream(failConfigurationMessage))
                        {
                            inboxMsg.EI_Status = 255;
                            context.eHubErrors.Add(CreateeHubError(inboxMsg.EI_PK, failConfigurationErrors != null ? failConfigurationErrors.ToString().TrimEnd() : "Error during validating ConfigurationMessage."));
                            var failOutboxContent = new MemoryStream(Encoding.UTF8.GetBytes(failConfigurationErrors != null ? failConfigurationErrors.ToString().TrimEnd() : "Error during validating ConfigurationMessage.")).CompressAndEncode().ReadToEnd();
                            var failOutboxMsg = OutboxMsg(message.MessageTrackingID.ToString(), inboxMsg.EI_CC_Recipient.Value, inboxMsg.EI_CC_Sender, inboxMsg, "MessageStatusFailed", failOutboxContent, 0, context);
                            context.eHubOutboxMessages.Add(failOutboxMsg);

                            var inboxOutboxContent = messageStream.CompressAndEncode().ReadToEnd();
                            var inboxMsgToSender = InboxMsg(message.ClientID, senderID, InternalNewGuid(), message, inboxOutboxContent, 2, context);
                            var outboxMsgToSender = OutboxMsg(inboxMsgToSender.EI_MessageTrackingID, inboxMsgToSender.EI_CC_Sender, inboxMsgToSender.EI_CC_Recipient.Value, inboxMsgToSender, ConfigurationMessageType, inboxOutboxContent, 0, context);
                            context.eHubInboxMessages.Add(inboxMsgToSender);
                            context.eHubOutboxMessages.Add(outboxMsgToSender);

                            context.SaveChanges();
                        }
                    }
                }
            }, logger);
        }

        private static eHubOutboxMessage OutboxMsg(string trackingID, Guid sender, Guid recipient, eHubInboxMessage inboxMsg, string messageType, string rawContent, byte status, eHubTransactionsContext context)
        {
            var outboxMsg = new eHubOutboxMessage
            {
                OI_PK = InternalNewGuid(),
                OI_CC_Sender = sender,
                OI_CC_Recipient = recipient,
                OI_MessageTrackingID = trackingID.ToString(),
                OI_Status = status,
                OI_InsertUTC = inboxMsg.EI_InsertUTC.Value,
                OI_DT_Target = context.eHubMessageTypes.First(m => m.DT_Code == messageType)
                    .DT_PK,
                OI_Content = rawContent,
                eHubInboxMessage = (messageType == "MessageStatusSuccess" || messageType == "MessageStatusFailed") ? null : inboxMsg
            };
            return outboxMsg;
        }

        private static eHubInboxMessage InboxMsg(string senderID, string recipientID, Guid trackingID, eHubGatewayMessage gatewayMessage, string content, byte status, eHubTransactionsContext context)
        {
            return new eHubInboxMessage
            {
                EI_PK = InternalNewGuid(),
                EI_MessageTrackingID = trackingID.ToString(),
                EI_EnvelopeTrackingID = string.Empty,
                EI_CC_Sender = context.eHubClients.First(c => c.CC_ID == senderID).CC_PK,
                EI_CC_Recipient = context.eHubClients.First(c => c.CC_ID == recipientID).CC_PK,
                EI_MessageType = gatewayMessage.SchemaName,
                EI_IsFlatFile = false,
                EI_ApplicationCode = gatewayMessage.ApplicationCode,
                EI_EmailSubjectOverride = string.Empty,
                EI_FileNameOverride = string.Empty,
                EI_InsertUTC = InternalUtcNow(),
				EI_LastUpdateUTC = InternalUtcNow(),
				EI_Status = status,
                EI_Content = content
            };
        }

        private static eHubError CreateeHubError(Guid inboxPK, string errorMessage)
        {
            return new eHubError
            {
                EE_PK = InternalNewGuid(),
                EE_EI_Inbox = inboxPK,
                EE_ErrorDetail = errorMessage,
                EE_Description = errorMessage,
                EE_ErrorType = "EXP",
                EE_Source = "HUB",
                EE_DateTimeUTC = InternalUtcNow(),
                EE_Alerted = false
            };
        }

        internal static Func<Guid> InternalNewGuid = Guid.NewGuid;
        internal static Func<DateTime> InternalUtcNow = () => DateTime.UtcNow;
    }
}
