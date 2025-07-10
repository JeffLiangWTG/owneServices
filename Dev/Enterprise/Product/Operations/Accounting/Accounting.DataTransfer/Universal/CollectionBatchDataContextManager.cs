using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Riba;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.DataTransfer.Universal
{
	public class CollectionBatchDataContextManager : EventDataContextManager<AccCollectionBatch>, ITransactionBatchDataContextManager, IDataContextManagerFromEDIMessage
	{
		const string ReceiptAmountCode = "ReceiptAmount";
		const string DepositedDateCode = "DepositedDate";

		public override DataContextType DataContextType
		{
			get { return DataContextType.CollectionBatch; }
		}

		public override ZString DataContextKey
		{
			get { return ParentBO.UniqueBatchID; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var result = ZQuery.NoResultQuery;

			GlbCompany company = null;
			var companyCode = matchingValues.CompanyCode;
			if (!companyCode.IsEmpty)
			{
				company = factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, companyCode);
			}

			if (company != null)
			{
				var batchNumber = matchingValues.Key;
				if (!batchNumber.IsEmpty)
				{
					result = new ZQuery(AccCollectionBatchSchema.ACB_GC, company.PK);
					result.AddToFilter(AccCollectionBatchSchema.ACB_BatchNumber, batchNumber);
				}
			}
			else
			{
				logger.Log(LogType.Error, "Invalid company code."); // Log might not be translatable
			}

			return result;
		}

		public void OnLogParentFoundFromEDIMessage(IXmlSessionTracker logger, IXmlEventValueObject eventDataObject, IEDIMessage message, BusinessObject businessObject)
		{
			if (businessObject is AccCollectionOrder collectionOrder)
			{
				var universalEvent = eventDataObject as UniversalEvent;

				var dateContext = universalEvent.ContextCollection.FirstOrDefault(x => x.Type == DepositedDateCode);
				if (dateContext != null && dateContext.Value.HasValue)
				{
					DateTime depositedDate;
					if (DateTime.TryParse(dateContext.Value.Value, out depositedDate))
					{
						collectionOrder.ACO_DepositedDate = new ZDate(depositedDate.Date);
						logger.Log(LogType.Information, FormattableString.Invariant($"Order {collectionOrder.ACO_OrderNumber} Deposited Date is updated")); // Log might not be translatable
					}
					else
					{
						logger.Log(LogType.Error, $"Context: Value is invalid date for Type={DepositedDateCode} in ContextCollection"); // Log might not be translatable
					}
				}
				else
				{
					logger.Log(LogType.Error, $"Context: Type={DepositedDateCode} is missing in ContextCollection"); // Log might not be translatable
				}
			}
		}

		public bool ManagesTransactionBatches
		{
			get { return true; }
		}

		public ITopLevelDataObjectWriter GetTransactionBatchDataObjectWriter(IDataWritingManager writeManager)
		{
			return new CollectionBatchWriter(writeManager);
		}

		public override string DefaultOutputDirectory
		{
			get { return null; }
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			return Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new CollectionBatchEventParentFinder(factory, this, logger);
		}

		class CollectionBatchEventParentFinder : EventParentFinder
		{
			internal CollectionBatchEventParentFinder(BusinessObjectFactory factory, CollectionBatchDataContextManager manager, IXmlImportLogger logger)
				: base(factory, manager, logger)
			{
			}

			bool IsDDIEvent(UniversalEvent xmlEvent) => xmlEvent.EventType.HasValue && xmlEvent.EventType.Value == AutoEvents.DocumentImportedCode;
			bool IsDIMEvent(UniversalEvent xmlEvent) => xmlEvent.EventType.HasValue && xmlEvent.EventType.Value == AutoEvents.DataImportCode;

			protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
			{
				AccCollectionBatch[] result = null;

				if (IsDDIEvent(xmlEvent))
				{
					if (logger.TopLevelDataContext != null && logger.TopLevelDataContext.DataTargetCollection != null && logger.TopLevelDataContext.DataTargetCollection.Any())
					{
						var uniqueID = logger.TopLevelDataContext.DataTargetCollection.FirstOrDefault(x => x.Type.HasValue && x.Type.Value == nameof(DataContextType.CollectionBatch));
						if (uniqueID != null && uniqueID.Key.HasValue)
						{
							var company = factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, uniqueID.Key.Value.Left(3));
							var filter = new ZQuery(AccCollectionBatchSchema.ACB_GC, company.PK);
							filter.AddToFilter(AccCollectionBatchSchema.ACB_BatchNumber, uniqueID.Key.Value.Substring(3));
							result = factory.Load<AccCollectionBatch>(filter);
						}
					}
				}

				return result;
			}

			protected override BusinessObject[] GetChildrenIfSpecifiedInContext(BusinessObject[] logParents, UniversalEvent eventData)
			{
				if (IsDIMEvent(eventData))
				{
					var result = new List<BusinessObject>();

					foreach (BusinessObject parent in logParents)
					{
						if (parent is AccCollectionBatch batch)
						{
							var query = new ZQuery();
							query.AddToFilter(AccCollectionOrderSchema.ACO_ACB, batch.PK);

							var collectionOrder = eventData.ContextCollection.FirstOrDefault(x => x.Type == AccCollectionOrderSchema.Constants.TableName);
							if (collectionOrder != null && collectionOrder.Value.HasValue)
							{
								query.AddToFilter(AccCollectionOrderSchema.ACO_OrderNumber, collectionOrder.Value.Value);
							}

							var amountContext = eventData.ContextCollection.FirstOrDefault(x => x.Type == ReceiptAmountCode);
							if (amountContext != null && amountContext.Value.HasValue)
							{
								decimal receiptAmount;
								if (decimal.TryParse(amountContext.Value.Value, out receiptAmount))
								{
									query.AddToFilter(AccCollectionOrderSchema.ACO_Amount, decimal.Parse(amountContext.Value.Value, CultureInfo.InvariantCulture));
								}
								else
								{
									logger.Log(LogType.Error, $"Context: Value is invalid amount for Type={ReceiptAmountCode} in ContextCollection"); // Log might not be translatable
									return result.ToArray();
								}
							}

							result.Add(batch);

							var orders = factory.Load<AccCollectionOrder>(query);
							foreach (var order in orders)
							{
								if (order.ACO_DepositedDate.IsValid && order.IsMatchedWithReceipt)
								{
									continue;
								}

								result.Add(order);
							}
						}
					}

					return result.ToArray();
				}
				else
				{
					return logParents;
				}
			}
		}

		public bool UseIncomingTransactionBatchData(IEDIMessage message, ITopLevelDataObject dataObject, IXmlSessionTracker logger, IUniversalObjectFactory factory)
		{
			return false;
		}

		public IKeysResult GetKeysForBlockingParallelImport(IEDIMessage message, ITopLevelDataObject dataObject, IXmlSessionTracker logger, IUniversalObjectFactory factory)
		{
			return KeysResult.NoMatch();
		}
	}
}
