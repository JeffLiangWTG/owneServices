using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Italy;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EventReference.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Universal
{
	public class AccEInvoicingBatchEventParentFinder : EventParentFinder
	{
		public AccEInvoicingBatchEventParentFinder(BusinessObjectFactory factory, AccEInvoicingBatchDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
		{
			BusinessObject[] result = null;
			if (xmlEvent.DataContext != null &&
				xmlEvent.DataContext.DataTargetCollection != null &&
				xmlEvent.DataContext.DataTargetCollection.Any(x => x.Type.HasValue && x.Type.Value == AccEInvoicingBatchSchema.Constants.TableName))  // Hard coded data context type value
			{
				var countryCode = EventParameters.GetEventParameter(EventReferenceParameters.Codes.MessageType, xmlEvent.EventParameters);
				switch (countryCode)
				{
					case Core.Constants.CountryCodes.Italy:
						result = FindLogParentsWithContextCollectionForItaly(xmlEvent);
						break;
					case Core.Constants.CountryCodes.Taiwan:
						result = FindLogParentsWithContextCollectionForTaiwan(xmlEvent);
						break;
					default:
						// do nothing
						break;
				}
			}
			return result;
		}

		#region Italy

		BusinessObject[] FindLogParentsWithContextCollectionForItaly(UniversalEvent xmlEvent)
		{
			BusinessObject[] result = null;
			var eventType = xmlEvent.EventType;
			var messageSubType = EventParameters.GetEventParameter(EventReferenceParameters.Codes.MessageSubType, xmlEvent.EventParameters);

			if (eventType.Value == AutoEvents.InterchangeRejectedCode ||
				(eventType.Value == AutoEvents.InterchangeAcknowledgedCode && messageSubType.HasValue && messageSubType.Value == EInvoicingEventMessageITProcessor.MessageSubTypeCode.RiceviFile))
			{
				LogMessageWithUniversalEventAsXML(Res.GetString("17E587A1-42B2-4B15-871E-46F923EE72EA", "Unable to find batch with Batch Number and Company Code."), LogType.Error, xmlEvent);
				return result;  // return null if the event is IRJ or IAK with subtype 2, no need to continue searching with the context collection.
			}

			if (IsUniversalEventValidForItaly(xmlEvent, out ZString batchNumber, out ZString companyCode, out ZString governmentAllocatedNumber, out ZString eHubAllocatedNumber))
			{
				result = GetBatchAndChildren(xmlEvent, batchNumber, companyCode);

				if (result != null)
				{
					CheckGovernmentAllocatedNumberAndEHubAllocatedNumberMatch(governmentAllocatedNumber, eHubAllocatedNumber, result[0] as AccEInvoicingBatch, companyCode, xmlEvent);
				}
			}

			return result;
		}

		bool IsUniversalEventValidForItaly(UniversalEvent xmlEvent, out ZString batchNumber, out ZString companyCode, out ZString governmentAllocatedNumber, out ZString eHubAllocatedNumber)
		{
			governmentAllocatedNumber = new ZString();
			eHubAllocatedNumber = new ZString();

			var errors = GetUniversalEventValidationError(xmlEvent, out batchNumber, out companyCode);

			var logBuilderForWarnings = new ZStringBuilder();
			var contextCollection = xmlEvent.ContextCollection;
			if (!contextCollection.IsNullOrEmpty())
			{
				governmentAllocatedNumber = ValidateContextTypeCode(contextCollection, logBuilderForWarnings, EInvoicingEventMessageITProcessor.ContextTypeCode.GovernmentAllocatedNumber);
				eHubAllocatedNumber = ValidateContextTypeCode(contextCollection, logBuilderForWarnings, EInvoicingEventMessageITProcessor.ContextTypeCode.EHubAllocatedNumber);
			}

			if (!errors.IsEmpty)
			{
				if (!logBuilderForWarnings.IsEmpty)
				{
					logger.LogBoth(LogType.Error, errors);
				}
				else
				{
					LogMessageWithUniversalEventAsXML(errors, LogType.Error, xmlEvent);
				}
			}

			if (!logBuilderForWarnings.IsEmpty)
			{
				LogMessageWithUniversalEventAsXML(logBuilderForWarnings.ToString(), LogType.Warning, xmlEvent);
			}

			return errors.IsEmpty;
		}

		void CheckGovernmentAllocatedNumberAndEHubAllocatedNumberMatch(ZString governmentAllocatedNumber, ZString eHubAllocatedNumber, AccEInvoicingBatch batch, ZString companyCode, UniversalEvent xmlEvent)
		{
			var logBuilder = new ZStringBuilder();
			if (batch.AIB_GovernmentAllocatedNumber != governmentAllocatedNumber)
			{
				logBuilder.AppendLine(Res.GetString("082D4400-FC11-4650-BFEF-726C7B0CBEEF", "For batch number {0} of company {1}, Government Allocated Number does not match between the Universal Event ({2}) and the batch ({3}).", batch.AIB_BatchNumber, companyCode, governmentAllocatedNumber, batch.AIB_GovernmentAllocatedNumber));
			}
			if (batch.AIB_EHubAllocatedNumber != eHubAllocatedNumber)
			{
				logBuilder.AppendLine(Res.GetString("70C7A721-F8DB-46C9-A567-82BEA1F4C00D", "For batch number {0} of company {1}, e-Hub Allocated Number does not match between the Universal Event ({2}) and the batch ({3}).", batch.AIB_BatchNumber, companyCode, eHubAllocatedNumber, batch.AIB_EHubAllocatedNumber));
			}

			if (!logBuilder.IsEmpty)
			{
				LogMessageWithUniversalEventAsXML(logBuilder.ToString(), LogType.Warning, xmlEvent);
			}
		}

		#endregion

		#region Taiwan

		BusinessObject[] FindLogParentsWithContextCollectionForTaiwan(UniversalEvent xmlEvent)
		{
			ZString batchNumber;
			ZString companyCode;
			if (IsUniversalEventValidForTaiwan(xmlEvent, out batchNumber, out companyCode))
			{
				return GetBatchAndChildren(xmlEvent, batchNumber, companyCode);
			}
			else
			{
				return null;
			}
		}

		bool IsUniversalEventValidForTaiwan(UniversalEvent xmlEvent, out ZString batchNumber, out ZString companyCode)
		{
			var errors = GetUniversalEventValidationError(xmlEvent, out batchNumber, out companyCode);
			if (errors.IsEmpty)
			{
				return true;
			}
			else
			{
				LogMessageWithUniversalEventAsXML(errors, LogType.Error, xmlEvent);
				return false;
			}
		}

		#endregion

		#region Get Log Parents

		BusinessObject[] GetBatchAndChildren(UniversalEvent xmlEvent, ZString batchNumber, ZString companyCode)
		{
			BusinessObject[] result = LoadBatchWithBatchNumberAndCompanyCode(batchNumber, companyCode);

			if (result != null)
			{
				var batchCount = result.Length;
				if (batchCount != 1)
				{
					LogMessageWithUniversalEventAsXML(Res.GetString("6670d781-fe2b-4f8c-a5b9-13067d3189a6", "Unable to find a batch with batch number {0} for company {1} as there are {2} matches.", batchNumber, companyCode, batchCount), LogType.Error, xmlEvent);
					result = null;
				}
				else
				{
					var batch = result[0] as AccEInvoicingBatch;

					result = result.Concat(GetChildrenBusinessObjectsForBatch(batch)).ToArray();
				}
			}

			return result;
		}

		AccEInvoicingBatch[] LoadBatchWithBatchNumberAndCompanyCode(ZString batchNumber, ZString companyCode)
		{
			var batchQuery = new ZDBOnlyQuery(typeof(AccEInvoicingBatch));
			batchQuery.AddToFilter(AccEInvoicingBatchSchema.AIB_BatchNumber, new ZInt(batchNumber));
			var companySubQuery = new ZDBOnlySubQuery(typeof(GlbCompany), AccEInvoicingBatchSchema.AIB_GC);
			companySubQuery.AddToFilter(GlbCompanySchema.GC_IsActive, ZBool.True);
			companySubQuery.AddToFilter(GlbCompanySchema.GC_Code, companyCode);
			batchQuery.AddSubQuery(companySubQuery, JoinCondition.And);
			return factory.Load<AccEInvoicingBatch>(batchQuery);
		}

		#endregion

		#region Validate Universal Event

		ZString GetUniversalEventValidationError(UniversalEvent xmlEvent, out ZString batchNumber, out ZString companyCode)
		{
			batchNumber = new ZString();
			companyCode = new ZString();

			var logBuilderForErrors = new ZStringBuilder();

			var key = xmlEvent.DataContext.DataTargetCollection.First().Key;
			if (!key.HasValue)
			{
				logBuilderForErrors.AppendLine(Res.GetString("64AAB7D4-DD65-4BDA-AB69-0804D35AEFAD", "<AccEInvoiceBatch> Key element is missing in Universal Event."));
			}
			else
			{
				batchNumber = key.Value;
				if (batchNumber.IsEmpty)
				{
					logBuilderForErrors.AppendLine(Res.GetString("AC826271-1447-4E1C-8649-31FBA66310AB", "<AccEInvoiceBatch> Key is empty in Universal Event."));
				}
			}

			var contextCollection = xmlEvent.ContextCollection;
			if (contextCollection.IsNullOrEmpty())
			{
				logBuilderForErrors.AppendLine(Res.GetString("88F4A95F-FDC1-4840-A599-D8AC4EE95663", "<ContextCollection> is missing in Universal Event."));
			}
			else
			{
				companyCode = ValidateContextTypeCode(contextCollection, logBuilderForErrors, EInvoicingEventMessageProcessor.ContextTypeCode.CompanyCode);
			}

			return logBuilderForErrors.ToString();
		}

		ZString ValidateContextTypeCode(List<Context> contextCollection, ZStringBuilder logBuilder, ZString contextTypeCode)
		{
			var context = contextCollection.FirstOrDefault(x => x.Type == contextTypeCode);
			var contextValue = ZString.Empty;

			if (context == null)
			{
				logBuilder.AppendLine(Res.GetString("6E97BA87-879B-452D-B255-4BA4F6C74CBC", "{0} context is missing in Universal Event.", contextTypeCode));
			}
			else
			{
				contextValue = context.Value ?? ZString.Empty;
				if (contextValue.IsEmpty)
				{
					logBuilder.AppendLine(Res.GetString("7F05E0A6-405B-4868-A17F-E3A79524CCBE", "{0} is empty in Universal Event.", contextTypeCode));
				}
			}

			return contextValue;
		}

		#endregion

		protected override BusinessObject[] GetChildrenIfSpecifiedInContext(BusinessObject[] logParents, UniversalEvent eventData)
		{
			if (logParents != null && logParents.Length == 1)
			{
				var result = new List<BusinessObject>();
				var batch = logParents[0] as AccEInvoicingBatch;
				if (batch != null)
				{
					result.Add(batch);
					result.AddRange(GetChildrenBusinessObjectsForBatch(batch));
				}
				return result.ToArray();
			}
			else
			{
				return base.GetChildrenIfSpecifiedInContext(logParents, eventData);
			}
		}

		string LogMessageWithUniversalEventAsXML(string message, LogType type, UniversalEvent universalEvent)
		{
			var messageWithEventXML = string.Join("\r\n", message, ReadUniversalEventAsString(universalEvent));
			logger.LogBoth(type, messageWithEventXML);
			return messageWithEventXML;
		}

		BusinessObject[] GetChildrenBusinessObjectsForBatch(AccEInvoicingBatch batch)
		{
			var result = new List<BusinessObject>();
			if (batch != null)
			{
				var transactionPKs = new List<ZGuid>();
				var complianceDocumentPKs = new List<ZGuid>();
				foreach (AccEInvoicingTransactionPivot pivot in batch.TransactionPivots)
				{
					switch (pivot.AIP_ParentTableCode)
					{
						case AccTransactionHeaderSchema.Constants.Prefix:
							transactionPKs.Add(pivot.AIP_ParentID);
							break;
						case AccComplianceDocumentHeaderSchema.Constants.Prefix:
							complianceDocumentPKs.Add(pivot.AIP_ParentID);
							break;
					}
				}

				if (transactionPKs.Count > 0)
				{
					result.AddRange(factory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.PK, transactionPKs)));
				}
				if (complianceDocumentPKs.Count > 0)
				{
					result.AddRange(factory.Load<AccComplianceDocumentHeader>(new ZQuery(AccComplianceDocumentHeaderSchema.PK, complianceDocumentPKs)));
				}
			}

			return result.ToArray();
		}

		string ReadUniversalEventAsString(UniversalEvent universalEvent)
		{
			var result = string.Empty;
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				var nameSpace = universalEvent.DataContext is UniversalDataBuss.DataObjects.Universal._2012_11.DataContext ? UniversalXmlInfo.Namespace_2012_11 : UniversalXmlInfo.Namespace_2011_11;
				ObjectFactory.Get<IXmlWriter>().WriteXML(universalEvent, stream, nameSpace);
				using (var reader = new StreamReader(stream))
				{
					result = reader.ReadToEnd();
				}
			}
			return result;
		}
	}
}
