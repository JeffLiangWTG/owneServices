using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Event = Enterprise.ZArchitecture.Business.Event;
using EventParameters = Enterprise.UniversalDataBuss.DataObjects.Universal.EventParameters;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.UniversalDataBuss.Management.EventProcessing
{
	class UniversalEventMessageProcessor : ITopLevelDataObjectProcessor
	{
		internal UniversalEventMessageProcessor() { }

		public MessageStatus ProcessDataObject(IEDIMessage message, ITopLevelDataObject dataObject, IUniversalObjectFactory factory, IXmlSessionTracker logger)
		{
			var eventDataObject = (UniversalEvent)dataObject;
			return ImportEvent(new EventImporterLogParentFinderVisitor(eventDataObject, (XmlSessionTracker)logger), message, eventDataObject, (UniversalObjectFactory)factory, logger);
		}

		public MessageKeyProviderResult GetKeys(IEDIMessage message, ITopLevelDataObject dataObject, IUniversalObjectFactory factory, IXmlSessionTracker logger)
		{
			var eventDataObject = (UniversalEvent)dataObject;
			var logFinderKeys = new UniversalEventGrengineKeyProviderLogParentFinderVisitor();
			ImportEvent(logFinderKeys, message, eventDataObject, (UniversalObjectFactory)factory, logger);
			var keys = logFinderKeys.KeysInfo
				.Concat(GetKeysFromUniversalEvent(eventDataObject))
				.Where(s => !string.IsNullOrEmpty(s.KeyValue))
				.GroupBy(k => k.KeyValue)
				.Select(g => (g.Key, string.Join("+", g.Select(k => k.KeySource))))
				.ToArray();
			return new MessageKeyProviderResult(keys);
		}

		public void ValidateDataObject(ITopLevelDataObject dataObject, IXmlSessionTracker logger)
		{
			((UniversalEvent)dataObject).ValidateContextValues(logger);
		}

		MessageStatus ImportEvent(LogParentFinder.Visitor logParentFinderVisitor, IEDIMessage message, UniversalEvent eventDataObject, UniversalObjectFactory factory, IXmlSessionTracker logger)
		{
			var xmlSessionTracker = (XmlSessionTracker)logger;
			var eventType = eventDataObject.EventType.HasValue ? Events.All[eventDataObject.EventType] : null;
			if (eventType == null)
			{
				logger.LogBoth(LogType.Error, string.Format(CultureInfo.InvariantCulture, "[{0}] is not a valid {1} Event Code. Cannot import XML Event unless it has a valid code.", eventDataObject.EventType, Core.Constants.ProductName));
				return MessageStatus.Rejected;
			}

			if (HasDuplicatedParameters(eventDataObject))
			{
				logger.LogBoth(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Event contains duplicated parameters. Cannot import XML Event unless all parameters are unique."));
				return MessageStatus.Rejected;
			}

			var mbol = ((IXmlEventValueObject)eventDataObject).Context.MBOLNumber.GetValueOrDefault();
			if (!mbol.IsEmpty && mbol.Length < 5)
			{
				((IXmlEventValueObject)eventDataObject).Context.ClearInvalidMBOL();
				logger.Log(LogType.Warning, string.Format(CultureInfo.InvariantCulture, "Disregarded MBOL '{0}' as it is less than 5 characters.", mbol));
			}

			var foundAtLeastOneLogParent = LogParentFinder.FindInterestedLogParents(factory, eventDataObject, logParentFinderVisitor, xmlSessionTracker, message, eventType);
			if (!foundAtLeastOneLogParent)
			{
				xmlSessionTracker.LogWasNotUsedByModule(LogType.Warning, string.Format(CultureInfo.InvariantCulture, "No Module found a Business Entity to link this Universal Event to."));
			}

			return foundAtLeastOneLogParent ? MessageStatus.Processed : MessageStatus.Discarded;
		}

		delegate StmALogAdder GetNewLogAdderDelegate(Event eventType, IEventDataContextManager dataContextManager);

		class EventImporterLogParentFinderVisitor : LogParentFinder.Visitor
		{
			public EventImporterLogParentFinderVisitor(UniversalEvent eventDataObject, XmlSessionTracker logger)
			{
				fieldUpdater = new AdditionalDataFieldsUpdater(eventDataObject, logger);
				logParentsAlreadyAdded = new HashSet<ZGuid>();
				logParentsAlreadyUpdateMatchingDataFields = new HashSet<ZGuid>();
			}

			readonly AdditionalDataFieldsUpdater fieldUpdater;
			readonly HashSet<ZGuid> logParentsAlreadyAdded;
			readonly HashSet<ZGuid> logParentsAlreadyUpdateMatchingDataFields;

			internal override bool FindLogs(IEDIMessage message, IEventDataContextManager contextManager, UniversalEvent eventDataObject, UniversalObjectFactory factory, XmlSessionTracker logger, Event eventType)
			{
				var eventParents = contextManager.GetLogParentsForEvent(eventDataObject, factory.BOFactory, logger);
				var validEventParentsFound = false;
				if (eventParents != null)
				{
					if (eventParents.Length > 100)
					{
						logger.LogBoth(LogType.Error, Res.GetString("e0c2dfef-08c0-47e2-acdc-dd1c0e0ee5e7", "Matching failed, more than 100 ({0}) matches were found in the [{1}] Data Context for this event using references from the Context Collection. This occurs when references specified are generic rather than unique.", eventParents.Length, contextManager.DataContextType.ToString()));
						eventParents = null;
					}
					else
					{
						var linkedJobs = new List<IEntityID>();
						foreach (var eventParent in eventParents)
						{
							var logParent = eventParent as IStmALogParent;
							if (logParent == null)
							{
								if (eventParent != null)
								{
									ErrorReporter.ReportOnce(FormattableString.Invariant($"The context manager {contextManager.GetType().FullName} returned {eventParent.GetType().FullName}, which can not have StmALogs."));
								}
								else
								{
									ErrorReporter.ReportOnce(FormattableString.Invariant($"The context manager {contextManager.GetType().FullName} returned an enumerable containing null. Please don't return enumerables containing null."));
								}
							}
							else
							{
								validEventParentsFound = true;

								if (contextManager.CanUpdateLogParentFromEvent(eventParent, eventDataObject, out var failureReason))
								{
									OnFindLogParent_UpdateParent(logger, logParent, contextManager, (evt, dataContextManager) => new MessageLinkedLogAdder(message, evt, eventDataObject, dataContextManager, factory, logger), eventDataObject, eventType);
								}
								else
								{
									logger.Log(LogType.Error, Res.GetString("dc65002b-5276-45ce-b978-0504a9e478e3", "Cannot update {0} because:{1}{2}", eventParent.GetType().Name, System.Environment.NewLine, failureReason));
								}

								var contextManagerFromEDIMessage = contextManager as IDataContextManagerFromEDIMessage;
								if (message != null && contextManagerFromEDIMessage != null)
								{
									contextManagerFromEDIMessage.OnLogParentFoundFromEDIMessage(logger, eventDataObject, message, eventParent);
								}

								var eventParentDataContextManager = eventParent.GetUniversalDataContextManager();
								if (eventParentDataContextManager != null)
								{
									linkedJobs.Add(eventParentDataContextManager);
								}
							}
						}
						logger.IndividualSetDataContextKeyIfPossible(linkedJobs.ToArray());
					}
				}

				return validEventParentsFound;
			}

			void OnFindLogParent_UpdateParent(XmlSessionTracker logger, IStmALogParent logParent, IEventDataContextManager dataContextManager, GetNewLogAdderDelegate getNewStmALogAdder, UniversalEvent eventDataObject, Event eventType)
			{
				var logsProvider = logParent as IStmALogParentProvider;
				var logParentToAddLog = logsProvider == null ? logParent : logsProvider.LogParent;
				if (logParentToAddLog == null)
				{
					var logParentName = logsProvider == null ? logParent.GetType().FullName : logsProvider.GetType().FullName;
					ErrorReporter.ReportOnce(logParentName + ".LogParent Is Null", logParentName + ".LogParent should not null.");
				}
				else
				{
					var logsParentPK = logParentToAddLog.LogsParentPK;
					if (!logParentsAlreadyAdded.Contains(logsParentPK))
					{
						var attachedDocuments = eventDataObject.AttachedDocumentCollection;
						if (attachedDocuments != null && attachedDocuments.Count > 0)
						{
							if (logParent is IDocManagerSupportBase eDocsParent)
							{
								var eDocLogAdder = getNewStmALogAdder(AutoEvents.DocumentImported, dataContextManager);
								var eventReferenceOrig = eventDataObject.EventReference;
								var attachedDocumentDataObjectReader = ObjectFactory.New<IAttachedDocumentDataObjectReader>();
								foreach (var attachedDocument in eventDataObject.AttachedDocumentCollection)
								{
									if (attachedDocumentDataObjectReader.TryAddAttachedDocument(attachedDocument, logger, eDocsParent, out IeDoc eDoc))
									{
										eventDataObject.EventReference = eDoc.CreateReference();
										eDocLogAdder.AddNewLogToParent(logParentToAddLog);
										logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Adding eDoc with a document type of {0} and name of {1}.", eDoc.DocType, eDoc.FileName));
									}
								}
								eventDataObject.EventReference = eventReferenceOrig;
							}
							else
							{
								logger.Log(LogType.Warning, string.Format(CultureInfo.InvariantCulture, "Could not link Attached Documents to {0}. Does not have eDocs.", logParentToAddLog.HumanReadableName));
							}
						}

						if (logParentToAddLog is ISupportUniversalEventImporting supportUniversalEventImporting)
						{
							supportUniversalEventImporting.IsSupportUniversalEventImporting = true;
						}

						var stmALogAdder = getNewStmALogAdder(eventType, dataContextManager);
						if (eventType.Code != AutoEvents.DocumentImportedCode)
						{
							stmALogAdder.AddNewLogToParent(logParentToAddLog);
						}

						var manager = (IEventDataContextManager)((BusinessObject)logParentToAddLog).GetUniversalDataContextManager();
						if (manager != null)
						{
							manager.OnUniversalEventAdded(logger, stmALogAdder.EventDataObject);

							if (eventDataObject.ContextCollection == null || eventDataObject.ContextCollection.Count == 0)
							{
								eventDataObject.ContextCollection = GetContexts(manager.EventContextValues);
							}
						}

						// Useful for events generated by children of UniversalDataContextManagers.
						// For example, ForwardingConsolDataContextManager uses `Transform` to transform the event to a Transport,
						// which does not have a universal data context manager
						if (logParentToAddLog is IUniversalEventAddedHandler universalEventAddedHandler)
						{
							universalEventAddedHandler.UniversalEventAdded(logger, stmALogAdder.EventDataObject);
						}

						if (((ISimpleLogger)logger).Logs.All(log => log.Type != LogType.Error))
						{
							logger.LogBoth(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Linked Event to {0}.", logParentToAddLog.HumanReadableName));
						}
						logParentsAlreadyAdded.Add(logsParentPK);
					}
				}
				if (!logParentsAlreadyUpdateMatchingDataFields.Contains(logParent.LogsParentPK))
				{
					fieldUpdater.UpdateMatchingDataFields(logParent);
					logParentsAlreadyUpdateMatchingDataFields.Add(logParent.LogsParentPK);
				}
			}
		}

		static List<Context> GetContexts(IEnumerable<KeyValuePair<TypeWithDescription, IZType>> contextValues)
		{
			return contextValues != null
				? contextValues.Select(o => new Context { Type = new ContextType { Type = o.Key.Type, Description = o.Key.Description }, Value = SimpleTypeFormatter.FormatForMultiTypedStringElementTargetInDataObject(o.Value) }).ToList()
				: new List<Context>();
		}

		static bool HasDuplicatedParameters(UniversalEvent eventDataObject)
		{
			var result = false;

			try
			{
				var parameters = StmALog.GetParametersFromReference(eventDataObject.EventReference, throwOnDuplicates: StmALog.ParseReferenceError.Exception);

				if (eventDataObject.EventParameters != null)
				{
					foreach (var property in typeof(EventParameters).GetProperties())
					{
						var parameterIsSpecifiedExplicitly = property.GetValue(eventDataObject.EventParameters, null) != null;

						if (parameterIsSpecifiedExplicitly && parameters.ContainsKey(EventParameters.GetCodeByName(property.Name)))
						{
							result = true;
							break;
						}
					}
				}
			}
			catch (ArgumentException)
			{
				result = true;
			}

			return result;
		}

		class UniversalEventGrengineKeyProviderLogParentFinderVisitor : LogParentFinder.Visitor
		{
			public HashSet<(string KeyValue, string KeySource)> KeysInfo { get; } = new HashSet<(string KeyValue, string KeySource)>();

			internal override bool FindLogs(IEDIMessage message, IEventDataContextManager contextManager, UniversalEvent eventDataObject, UniversalObjectFactory factory, XmlSessionTracker logger, Event eventType)
			{
				var result = false;
				var logKeys = contextManager.GetLogKeysForEvent(eventDataObject, factory.BOFactory, logger);
				if (logKeys.IsMatch)
				{
					foreach (var keyInfo in logKeys.KeysInfo)
					{
						result |= KeysInfo.Add(keyInfo);
					}
				}

				return result;
			}
		}

		IEnumerable<(string KeyValue, string KeySource)> GetKeysFromUniversalEvent(UniversalEvent evnt)
		{
			var ctx = ((IXmlEventValueObject)evnt).Context;
			var commonSourcePrefix = "Context/";

			return Extensions.AsStringsIgnoringNull(
					(ctx.AdjustmentReference, commonSourcePrefix + nameof(ctx.AdjustmentReference)),
					(ctx.AgentsReference, commonSourcePrefix + nameof(ctx.AgentsReference)),
					(ctx.CarriersBookingReference, commonSourcePrefix + nameof(ctx.CarriersBookingReference)),
					(ctx.CFSReference, commonSourcePrefix + nameof(ctx.CFSReference)),
					(ctx.ClientReference, commonSourcePrefix + nameof(ctx.ClientReference)),
					(ctx.CommercialInvoiceNumber, commonSourcePrefix + nameof(ctx.CommercialInvoiceNumber)),
					(ctx.DeclarationReference, commonSourcePrefix + nameof(ctx.DeclarationReference)),
					(Extensions.JoinIgnoringNull(ctx.EntryNumberType, ctx.EntryNumberCountryOfIssue, ctx.EntryNumber),
						commonSourcePrefix + $"{nameof(ctx.EntryNumberType)}+{nameof(ctx.EntryNumberCountryOfIssue)}+{nameof(ctx.EntryNumber)}"),
					(ctx.FlightNumber.HasValue ? new ZString(ctx.FlightNumber + ctx.FlightDate.ToString()) : ZString.Empty,
						commonSourcePrefix + $"{nameof(ctx.FlightNumber)}+{nameof(ctx.FlightDate)}"),
					(ctx.GoodsDeclarationNumber, commonSourcePrefix + nameof(ctx.GoodsDeclarationNumber)),
					(ctx.HAWBNumber, commonSourcePrefix + nameof(ctx.HAWBNumber)),
					(ctx.HBOLNumber, commonSourcePrefix + nameof(ctx.HBOLNumber)),
					(ctx.MasterHouseBill.CleanMasterBill(), commonSourcePrefix + nameof(ctx.MasterHouseBill)),
					(ctx.MAWBNumber, commonSourcePrefix + nameof(ctx.MAWBNumber)),
					(ctx.MBOLNumber, commonSourcePrefix + nameof(ctx.MBOLNumber)),
					(ctx.OrderNumber, commonSourcePrefix + nameof(ctx.OrderNumber)),
					(ctx.OrderTrackingNumber, commonSourcePrefix + nameof(ctx.OrderTrackingNumber)),
					(ctx.QuoteNumber, commonSourcePrefix + nameof(ctx.QuoteNumber)),
					(ctx.ReceiveReference, commonSourcePrefix + nameof(ctx.ReceiveReference)),
					(ctx.ShippersReference, commonSourcePrefix + nameof(ctx.ShippersReference)),
					(ctx.TransportBookingInstructionID, commonSourcePrefix + nameof(ctx.TransportBookingInstructionID)),
					(ctx.TransportBookingJobID, commonSourcePrefix + nameof(ctx.TransportBookingJobID)),
					(ctx.TransportBookingPackageID, commonSourcePrefix + nameof(ctx.TransportBookingPackageID)),
					(ctx.TransportReference, commonSourcePrefix + nameof(ctx.TransportReference)),
					(Extensions.JoinIgnoringNull(ctx.VesselName, ctx.VoyageNumber),
						commonSourcePrefix + $"{nameof(ctx.VesselName)}+{nameof(ctx.VoyageNumber)}"),
					(ctx.WaybillNumber, commonSourcePrefix + nameof(ctx.WaybillNumber)))
				.ConcatIgnoreNull(ctx.ContainerNumbers?.Select(s => (s.ToString(), commonSourcePrefix + nameof(ctx.ContainerNumbers))))
				.ConcatIgnoreNull(ctx.ULDIdentifications?.Select(s => (s.ToString(), commonSourcePrefix + nameof(ctx.ULDIdentifications))))
				.ConcatIgnoreNull(evnt?.DataContext?.DataSourceCollection?.Where(s => s != null)
					.Select(s => (Extensions.JoinIgnoringNull(s.Type, s.Key), $"{nameof(evnt.DataContext)}/{nameof(evnt.DataContext.DataSourceCollection)}/nameof{nameof(s.Type)}+{nameof(s.Key)}")));
		}
	}
}
