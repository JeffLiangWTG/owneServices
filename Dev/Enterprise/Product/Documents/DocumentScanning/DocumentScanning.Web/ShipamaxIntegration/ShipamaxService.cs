using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Dash.Integration.Services;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentScanning.Web
{
	public class ShipamaxService : IShipamaxService
	{
		public void SaveParseResult(Guid docPK, string docToken, ShipamaxParseResult parseResult)
		{
			try
			{
				CheckShipamaxIntegrationServiceRequirement();

				var shipamaxMessage = GetEDocsShipamaxMessage(docPK, docToken);

				// Suppress switch context check so that we can use the branch which created the EDIMessage to update the parse status,
				// otherwise switching context from CW1WebUser to CW1ServiceUser would throw error.
				using (Env.Instance.SuppressSwitchContextCheck())
				using (DisposableEnvironment.ForBranch(shipamaxMessage.EM_GB.ToGuid()))
				{
					var oldStatus = shipamaxMessage.EM_Status;

					var statusInParseResult = parseResult.ParseStatus;
					var newStatus = GetMessageStatusCode(statusInParseResult);
					shipamaxMessage.EM_Status = newStatus;
					if (!newStatus.Equals(oldStatus) && (statusInParseResult == ShipamaxParseStatus.Complete || statusInParseResult == ShipamaxParseStatus.NeedReview))
					{
						LogParseResultUpdateEvent(shipamaxMessage.LinkedEDoc?.ParentMain, docPK, newStatus, oldStatus);
					}

					var currentParseResultXml = shipamaxMessage.MessageData.ParseResultXml;
					var currentParseDataJson = shipamaxMessage.MessageData.ParseDataJson;
					parseResult.XmlParseResult = parseResult.XmlParseResult?.TrimWithUnicodeWhitespace();
					parseResult.JsonParseResult = parseResult.JsonParseResult?.TrimWithUnicodeWhitespace();

					if (parseResult.XmlParseResult is not { Length: 0 })
					{
						currentParseResultXml = parseResult.XmlParseResult;
					}

					if (parseResult.JsonParseResult is not { Length: 0 })
					{
						currentParseDataJson = parseResult.JsonParseResult;
					}

					shipamaxMessage.MessageData = new EDocsShipamaxMessageData(currentParseResultXml, currentParseDataJson);
					shipamaxMessage.Factory.Save();
				}
			}
			catch (Exception ex) when (ex is not ShipamaxServiceException && !ex.IsCriticalException())
			{
				throw new ShipamaxServiceException(ShipamaxServiceErrorType.RunningError, ex.Message, ex);
			}
		}

		protected virtual EDocsShipamaxMessage GetEDocsShipamaxMessage(Guid docPK, string docToken)
		{
			if (!ValidateDocToken(docPK, docToken, out var shipamaxMessage))
			{
				throw new ShipamaxServiceException(ShipamaxServiceErrorType.ValidationError, "The eDoc authorization token is invalid.");
			}

			if (!shipamaxMessage.EM_IsActive)
			{
				throw new ShipamaxServiceException(ShipamaxServiceErrorType.StatusError, "The operation cannot be performed on an inactive record.");
			}

			if (shipamaxMessage.EM_Status != EDIMessageStatusList.Codes.Sent
				&& shipamaxMessage.EM_Status != EDIMessageStatusList.Codes.PreProcessedOK
				&& shipamaxMessage.EM_Status != EDIMessageStatusList.Codes.ProcessedOK)
			{
				throw new ShipamaxServiceException(ShipamaxServiceErrorType.StatusError, $"The operation cannot be performed on a record in status '{shipamaxMessage.EM_Status}'.");
			}

			return shipamaxMessage;
		}

		protected virtual bool ValidateDocToken(Guid docPK, string docToken, out EDocsShipamaxMessage shipamaxMessage)
		{
			shipamaxMessage = null;
			try
			{
				var (docChanges, message) = ValidateShipamaxDocToken(docPK, docToken);
				shipamaxMessage = message;
				return !docChanges.Any();
			}
			catch (ShipamaxServiceException)
			{
				return false;
			}
		}

		void CheckShipamaxIntegrationServiceRequirement()
		{
			if (!EDocsParsingHelper.IsDocumentParsingEnabled())
			{
				throw new ShipamaxServiceException(ShipamaxServiceErrorType.ConfigurationError, "Document Ingestion service is not enabled.");
			}
		}

		void LogParseResultUpdateEvent(StorageMain parentMain, Guid docPK, string newStatus, string oldStatus)
		{
			if (parentMain == null || parentMain.OwnerAssemblyData == null)
			{
				return;
			}

			var bizObj = MasterFactory.Load(parentMain.OwnerAssemblyData.BusinessObjectType, parentMain.SM_ParentFK);
			if (bizObj != null && bizObj is IStmALogProvider logProvider)
			{
				var correctionUrl = ObjectFactory.Get<IDashUtils>().GetCorrectionToolUrl(docPK.ToString()) ?? string.Empty;
				var eventMessage = $"{docPK}|NEW={newStatus}|OLD={oldStatus}";
				if (newStatus == EDIMessageStatusList.Codes.PreProcessedOK || newStatus == EDIMessageStatusList.Codes.ProcessedOK)
				{
					eventMessage = $"{eventMessage}|LINK={correctionUrl}";
				}
				_ = logProvider.Logs.AddNew(AutoEvents.DocumentParseStatusUpdate, eventMessage);
				MasterFactory.Save();
			}
		}

		static string GetMessageStatusCode(ShipamaxParseStatus status)
		{
			string result;

			switch (status)
			{
				case ShipamaxParseStatus.Unparsed:
					result = EDIMessageStatusList.Codes.Queued;
					break;
				case ShipamaxParseStatus.Processing:
					result = EDIMessageStatusList.Codes.Sent;
					break;
				case ShipamaxParseStatus.Failed:
					result = EDIMessageStatusList.Codes.Error;
					break;
				case ShipamaxParseStatus.Complete:
					result = EDIMessageStatusList.Codes.ProcessedOK;
					break;
				case ShipamaxParseStatus.NeedReview:
					result = EDIMessageStatusList.Codes.PreProcessedOK;
					break;
				case ShipamaxParseStatus.Discarded:
					result = EDIMessageStatusList.Codes.Discarded;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(status), status, null);
			}

			return result;
		}

		(IEnumerable<ShipamaxEDocsChange>, EDocsShipamaxMessage) ValidateShipamaxDocToken(Guid docPK, string docToken)
		{
			var tokenManager = new ShipamaxIntegrationTokenManager();
			if (!tokenManager.TryDecryptEDocsAuthToken(docToken, out var docTokenDetails))
			{
				throw new ShipamaxServiceException(ShipamaxServiceErrorType.ValidationError, "Doc token is not recognized.");
			}

			var shipamaxMessage = MasterFactory.Load<EDocsShipamaxMessage>(docTokenDetails.EDIMessagePK);
			if (shipamaxMessage == null)
			{
				throw new ShipamaxServiceException(ShipamaxServiceErrorType.ValidationError, "Cannot find Shipamax message for the doc token.");
			}
			else if (shipamaxMessage.EM_LinkUniqueID != docPK)
			{
				throw new ShipamaxServiceException(ShipamaxServiceErrorType.ValidationError, "Shipamax message in doc token is not related to the eDoc.");
			}

			var docChanges = new List<ShipamaxEDocsChange>();
			var loadedEDoc = shipamaxMessage.LinkedEDoc;
			if (loadedEDoc == null)
			{
				docChanges.Add(new ShipamaxEDocsChange { Field = ShipamaxEDocsChangeType.DocumentPK, OldValue = docPK, NewValue = Guid.Empty });
				return (docChanges, shipamaxMessage);
			}

			if (loadedEDoc.SC_DocType != docTokenDetails.DocType)
			{
				docChanges.Add(new ShipamaxEDocsChange { Field = ShipamaxEDocsChangeType.DocType, OldValue = docTokenDetails.DocType, NewValue = loadedEDoc.SC_DocType });
			}

			if (loadedEDoc.SC_DataType != docTokenDetails.DataType)
			{
				docChanges.Add(new ShipamaxEDocsChange { Field = ShipamaxEDocsChangeType.DocFormat, OldValue = docTokenDetails.DataType, NewValue = loadedEDoc.SC_DataType });
			}

			if (loadedEDoc.SC_Date != docTokenDetails.EDocLastEditTime)
			{
				docChanges.Add(new ShipamaxEDocsChange { Field = ShipamaxEDocsChangeType.DocEditTime, OldValue = docTokenDetails.EDocLastEditTime, NewValue = loadedEDoc.SC_Date });
			}

			if (loadedEDoc.SC_IsDeleted)
			{
				docChanges.Add(new ShipamaxEDocsChange { Field = ShipamaxEDocsChangeType.Deleted, OldValue = false, NewValue = true });
			}

			if (string.Compare(loadedEDoc.ActiveShipamaxMessage?.EM_Status, EDIMessageStatusList.Codes.Cancelled, StringComparison.OrdinalIgnoreCase) == 0)
			{
				docChanges.Add(new ShipamaxEDocsChange { Field = ShipamaxEDocsChangeType.ParsingEnabled, OldValue = true, NewValue = false });
			}

			return (docChanges, shipamaxMessage);
		}

		public IEnumerable<ShipamaxEDocsChange> CheckEDocsChanges(Guid docPK, string docToken)
		{
			var (docChanges, _) = ValidateShipamaxDocToken(docPK, docToken);
			return docChanges;
		}

		#region Master Factory

		DocumentFactory MasterFactory => masterFactory ??= new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());

		DocumentFactory masterFactory;

		#endregion
	}
}
