using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.GenericConsol;
using Enterprise.Accounting.Export.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using GenericJob = Enterprise.Accounting.Business.GenericJob.GenericJob;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using UniversalTransaction = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.DataTransfer.Universal
{
	public class TransactionDataObjectWriter : TopLevelDataObjectWriter<InvoicingBase, UniversalTransaction>
	{
		public TransactionDataObjectWriter(IDataWritingManager manager) : base(manager) { }

		protected override IDataContextManager GetDataContextManager(BusinessObject sourceBO)
		{
			return sourceBO.GetUniversalDataContextManager();
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalTransaction;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.AccountingInvoice;
		}

		protected override void PopulateDataObject(InvoicingBase sourceBO, UniversalTransaction dataObject)
		{
			var validLedgers = new[] { LedgerTypes.AccountsReceivable, LedgerTypes.AccountsPayable };
			if (validLedgers.Contains(sourceBO.AH_Ledger.ToString()))
			{
				var accountingStrategy = IsOrgProxyOnlySelected()
										? AllFieldsAllowedAccountingTransactionWriterStrategy.Instance
										: DefaultAccountingTransactionWriterStrategy.Instance;
				var writerStrategy = new AccountingTransactionDataObjectWriterStrategy(
					parentStrategy: writeManager.WriterStrategy,
					accountingStrategy: accountingStrategy,
					context: "UniversalTransaction"
				);
				dataObject.SetWriterStrategy(writerStrategy);

				var dataAcess = new BatchExportDataAccess(((IDbConnectionInternals)CargoWise.Data.Db.Connection).ADOConnection, ((IDbConnectionInternals)CargoWise.Data.Db.Connection).ADOTransaction);
				var transactionExporter = new TransactionExporter(dataAcess);
				transactionExporter.PopulateUniversalTransaction(writerStrategy, sourceBO.Company.GC_Code.ToString(), sourceBO.PK.ToGuid(), dataObject);

				//Add new method and call it here. It will take all published eDocs using properties like in EventDataObjectWriter.FindEDocs and use new AttachedDocumentDataObjectWriter to add documents to dataObject.
				var helper = new CreateUniversalShipmentHelper(writeManager);
				helper.PopulatePublishedEDocs(sourceBO, dataObject);
				helper.PopulateLineJobs(sourceBO, dataObject);
				helper.PopulateLineConsols(sourceBO, dataObject);
			}
		}
	}

	public class CreateUniversalShipmentHelper
	{
		public CreateUniversalShipmentHelper(IDataWritingManager manager)
		{
			writeManager = manager;
		}

		public void PopulatePublishedEDocs(InvoicingBase sourceBO, UniversalTransaction dataObject)
		{
			var docManagerSupport = sourceBO as IDocManagerSupport;
			if (docManagerSupport != null && dataObject.IsAllowSet(nameof(UniversalTransaction.AttachedDocumentCollection)))
			{
				var publishedEDocs = docManagerSupport.DocManagerInfo.AllEDocs.Cast<IeDoc>().Where(x => x.IsPublished);
				var attachedDocuments = new AttachedDocumentDataObjectWriter().GenerateAttachedDocuments(false, publishedEDocs.ToArray());
				if (attachedDocuments.Any())
				{
					dataObject.SetAttachedDocumentCollection(() => new List<AttachedDocument>(attachedDocuments));
				}
			}
		}

		public void PopulateLineJobs(InvoicingBase sourceBO, UniversalTransaction dataObject)
		{
			var linesjobPKs = sourceBO.Lines.Cast<InvoicingLineBase>().Where(x => x.AL_JH != ZGuid.Empty).Select(x => x.AL_JH).Distinct().ToList();
			if (linesjobPKs.Count > 0 && dataObject.IsAllowSet(nameof(UniversalTransaction.ShipmentCollection)))
			{
				var sqlText = @"
			SELECT JH_ParentID, JH_ParentTableCode
			FROM dbo.JobHeader
			WHERE JH_PK IN (SELECT Value FROM @linesjobPKs)";

				var parameters = new ZSqlParameter[] {
					ZSqlParameter.New("@linesjobPKs", linesjobPKs, JobHeaderSchema.PK, true)
				};

				var lineJobs = new DynamicBusinessObjectCollection(sourceBO.Factory);
				lineJobs.Load(sqlText, parameters);

				foreach (DynamicBusinessObject lineJob in lineJobs)
				{
					var factory = new BusinessObjectFactory();
					using (((IExternalFetchHintSupporter)factory).SetupCreator())
					{
						var genericJob = factory.LoadGenericJob<GenericJob>((ZGuid)lineJob[JobHeaderSchema.Constants.JH_ParentID], (ZString)lineJob[JobHeaderSchema.Constants.JH_ParentTableCode]);
						if (genericJob != null)
						{
							var consumer = genericJob.Consumer as BusinessObject;
							AddUniversalShipment(dataObject, consumer);
						}
					}
				}
			}
		}

		public void PopulateLineConsols(InvoicingBase sourceBO, UniversalTransaction dataObject)
		{
			if (!dataObject.IsAllowSet(nameof(UniversalTransaction.ShipmentCollection)))
			{
				return;
			}

			var tuples = sourceBO.Lines.Cast<InvoicingLineBase>().Where(x => x.GetConsolID() != null).Select(y => y.GetConsolID()).Distinct().ToList();

			if (sourceBO.IsConsolInvoice)
			{
				var consol = !tuples.IsNullOrEmpty() ? GenericConsol.GetIJobCostingPlugInByPK(sourceBO.Factory, tuples.First().Item1, tuples.First().Item2) : null;

				if (consol == null || consol.JK_UniqueConsignRef != sourceBO.ConsolNumberFromConsolidatedInvoiceRef)
				{
					consol = GenericConsol.GetIJobCostingPlugInByPrimaryCode(sourceBO.Factory, sourceBO.ConsolNumberFromConsolidatedInvoiceRef);

					if (consol != null && tuples.All(tuple => tuple.Item1 != consol.PK))
					{
						var parentTableCode = ((BusinessObject)consol).TablePrefix;
						tuples.Add(new Tuple<ZGuid, ZString>(consol.PK, parentTableCode));
					}
				}
			}

			foreach (var tuple in tuples)
			{
				var factory = new BusinessObjectFactory();
				using (((IExternalFetchHintSupporter)factory).SetupCreator())
				{
					var consumer = GenericConsol.GetIJobCostingPlugInByPK(factory, tuple.Item1, tuple.Item2);
					if (consumer != null)
					{
						AddUniversalShipment(dataObject, consumer as BusinessObject);
					}
				}
			}
		}

		#region Implementation

		void AddUniversalShipment(UniversalTransaction dataObject, BusinessObject consumer)
		{
			UniversalShipment shipment = GetUniversalShipmentIfAvailable(consumer);

			if (shipment != null)
			{
				if (dataObject.ShipmentCollection != null || dataObject.SetShipmentCollection(() => new List<UniversalShipment>()))
				{
					dataObject.ShipmentCollection.Add(shipment);
				}
			}
		}

		UniversalShipment GetUniversalShipmentIfAvailable(BusinessObject jobHeaderParentBO)
		{
			var dataContextManager = jobHeaderParentBO.GetUniversalDataContextManager() as IShipmentDataContextManager;
			if (dataContextManager != null && dataContextManager.ManagesShipments)
			{
				using (writeManager.UseNewListForDuplicatePKCheck())
				{
					var shipmentDataObjectWriter = dataContextManager.GetShipmentDataObjectWriter(writeManager);
					var universalShipment = shipmentDataObjectWriter.GetDataObject(jobHeaderParentBO);
					return (UniversalShipment)universalShipment;
				}
			}

			return null;
		}

		readonly IDataWritingManager writeManager;

		#endregion
	}
}

