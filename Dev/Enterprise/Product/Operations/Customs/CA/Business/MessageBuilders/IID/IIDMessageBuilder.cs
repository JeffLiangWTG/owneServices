using System;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageBuilders;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageBuilders;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	public class IIDMessageBuilder : IMessageBuilder
	{
		#region Constructor

		public IIDMessageBuilder(ZString actionPurpose, IIDMessageWrapper dataWrapper)
		{
			this.actionPurpose = actionPurpose;
			this.dataWrapper = Argument.NotNull(dataWrapper, "dataWrapper");
		}

		#endregion

		protected readonly ZString actionPurpose;
		protected readonly IIDMessageWrapper dataWrapper;

		#region Implementation of IMessageBuilder

		public IMessageBuilderResult PopulateMessages()
		{
			var messageBuilderResult = new MessageBuilderResult();
			var builderResult = new BuilderResult(null, Array.Empty<string>(), null);
			builderResult.Message = PopulateMessagesCore();
			messageBuilderResult.AddBuilderResult(builderResult);
			return messageBuilderResult;
		}

		#endregion

		Enterprise.Messaging.Business.EDIMessage PopulateMessagesCore()
		{
			Enterprise.Messaging.Business.EDIMessage message = null;
			message = dataWrapper.AddNew();
			message.EM_MessageSubType = actionPurpose;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;

			try
			{
				var cancellationResult = PopulateMessageOfCancellation(message);
				if (!cancellationResult)
				{
					PopulateMessageFromUniversalShipment(message);
				}
			}
			catch(Exception ex)
			{
				if (message.EM_MessageText.IsEmpty)
				{
					message.Delete();
					var messageDesc = new IIDMessageSubTypeList().GetDescriptionFromCode(actionPurpose);
					throw new InvalidMessageContentException($"Failed to create IID {messageDesc} message. Please try to save the changes and send the IID {messageDesc} message again.", ex);
				}
				throw;
			}

			return message;
		}

		bool PopulateMessageOfCancellation(Enterprise.Messaging.Business.EDIMessage message)
		{
			var cancellationResult = false;
			if (actionPurpose == IIDMessageSubTypeList.Codes.Cancellation && dataWrapper.LatestSentAcceptedMessage is EDIMessage existedSentMessage)
			{
				using (var stringReader = new StringReader(existedSentMessage.EM_MessageText))
				{
					UniversalShipment readShipment = null;
					try
					{
						readShipment = stringReader.Parse<UniversalShipment>();
						SetEventReference(readShipment.DataContext, IIDMessageSubTypeList.Codes.Cancellation);
					}
					catch (Exception e)
					{
						var row = ((IBusinessObjectInternals)existedSentMessage).Row;
						var errorMessage = $@"Message: {e.Message}
EDIMessage Information:
Type: {existedSentMessage.GetType().FullName}
Message Type: {row[EDIMessageSchema.Constants.EM_MessageType]}
MessageType Sub Type: {row[EDIMessageSchema.Constants.EM_MessageSubType]}
Application Code: {row[EDIMessageSchema.Constants.EM_ApplicationCode]}
Message Number: {row[EDIMessageSchema.Constants.EM_MessageNum]}
Status: {row[EDIMessageSchema.Constants.EM_Status]}
Direction: {row[EDIMessageSchema.Constants.EM_ReceiveTransmit]}
Message Text: {row[EDIMessageSchema.Constants.EM_MessageText]}
Message NText: {row[EDIMessageSchema.Constants.EM_MessageNText]}
Message Data: {row[EDIMessageSchema.Constants.EM_MessageData]}
Branch: {existedSentMessage?.Branch.GB_Code}
Company: {existedSentMessage?.Company.GC_Code}
DataContext is Null: {readShipment?.DataContext == null}
";
						throw new DeveloperNotificationException(errorMessage, e);
					}

					if (readShipment != null)
					{
						SetMessageDataFromShipment(readShipment, message);
					}
					cancellationResult = true;
				}
			}
			return cancellationResult;
		}

		void PopulateMessageFromUniversalShipment(Enterprise.Messaging.Business.EDIMessage message)
		{
			UniversalShipment universalShipment = null;
			var topLevelBO = ((ICAEDIFACTMessageAttachee)dataWrapper).TopLevelBusinessObject;
			var manager = topLevelBO?.GetUniversalDataContextManager() as IShipmentDataContextManager;
			var writer = manager?.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, topLevelBO)) { FilteredDataContextType = DataContextType.CustomsDeclaration });
			if (writer != null)
			{
				if (writer is Integration.Customs.CA.IIIDMessagingDeclarationDataObjectWriter iidWriter)
				{
					iidWriter.IsExportingForIIDMessaging = true;
				}
				using (((IExternalFetchHintSupporter)topLevelBO.Factory).SetupCreator())
				{
					universalShipment = writer.GetDataObject(topLevelBO) as UniversalShipment;
				}
			}

			try
			{
				SetEventReference(universalShipment.DataContext, actionPurpose);
				SetMessageDataFromShipment(universalShipment, message);
			}
			catch (Exception e)
			{
				var declaration = topLevelBO as BaseJobDeclaration;
				var errorMessage = $@"Message: {e.Message}
TopLevelBusinessObject is null: {topLevelBO == null}
UniversalDataContextAttribute: {topLevelBO?.GetAttribute<UniversalDataContextAttribute>().DataContextType}
TopLevelBusinessObject is Declaration: {declaration == null}
Declaration Application Code: {declaration?.JE_ApplicationCode}
Declaration Country/Region Code: {declaration?.CountryCode}
IShipmentDataContextManager is null: {manager == null}
ITopLevelDataObjectWriter is null: {writer == null}
";
				throw new DeveloperNotificationException(errorMessage, e);
			}
		}

		void SetMessageDataFromShipment(UniversalShipment univShipment, Enterprise.Messaging.Business.EDIMessage message)
		{
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				new XmlWriter().WriteXML(univShipment, stream, false);
				message.SetEM_MessageTextOrDataSource(stream.Copy());
			}
		}

		void SetEventReference(IDataContextDataObject dataContext, ZString actionType)
		{
			var workflowFormat = "MST={0}|MSB={1}|RFN=4.02";

			dataContext.SetWorkflowInfo(new WorkflowInfo()
			{
				EventReference = ZString.Format(workflowFormat, MessageTypeList.Codes.IntegratedImportDeclaration, actionType),
			});
		}
	}
}
