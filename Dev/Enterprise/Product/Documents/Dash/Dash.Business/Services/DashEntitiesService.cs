using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Dash.Business.BusinessObjects;
using Enterprise.DocumentScanning.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using SharedConstants = WTG.Shared.Dash.Common.Constants;

namespace Enterprise.Dash.Business.Services
{
	public class DashEntitiesService : IDashEntitiesService
	{
		readonly IShipamaxService shipamaxService;
		readonly IDashErrorReporter dashErrorReporter;

		public DashEntitiesService(IShipamaxService shipamaxService, IDashErrorReporter dashErrorReporter)
		{
			this.shipamaxService = shipamaxService;
			this.dashErrorReporter = dashErrorReporter;
		}

		public DashAPInvoice[] LoadAPInvoices(IEnumerable<ZGuid> dashDocumentPKs, BusinessObjectFactory factory = null)
		{
			const int dashDocumentBatchSize = 100;
			var currentFactory = factory ?? new BusinessObjectFactory();

			var dashDocuments = new List<DashDocument>();
			var batchedDashDocumentsPKs = dashDocumentPKs.Batch(dashDocumentBatchSize);

			foreach (var dashDocumentPkBatch in batchedDashDocumentsPKs)
			{
				var documentQuery = new ZQuery(DashDocumentSchema.PK, dashDocumentPkBatch);
				var documentsBatch = currentFactory.Load<DashDocument>(documentQuery);
				dashDocuments.AddRange(documentsBatch);

				var dashDocumentsByPk = dashDocuments.ToDictionary(x => x.PK);

				var apInvoiceQuery = new ZQuery(DashAPInvoiceSchema.DPI_DDD_DashDocID, dashDocumentPkBatch);
				var apInvoicesBatch = currentFactory.Load<DashAPInvoice>(apInvoiceQuery);

				foreach (var apInvoice in apInvoicesBatch)
				{
					if (dashDocumentsByPk.TryGetValue(apInvoice.DPI_DDD_DashDocID, out var dashDocument))
					{
						dashDocument.SetDashAPInvoice(apInvoice);
						apInvoice.SetDashDocument(dashDocument);
					}
				}
			}

			var apInvoices = dashDocuments.Select(x => x.DashAPInvoice).ToArray();

			foreach (var apInvoice in apInvoices)
			{
				LoadAPInvoiceChargeLineRefs(currentFactory, apInvoice);
			}

			return apInvoices;
		}

		static void LoadAPInvoiceChargeLineRefs(BusinessObjectFactory currentFactory, DashAPInvoice apInvoice)
		{
			var apInvoiceRefs = apInvoice.DashAPInvoiceRefs.ToDictionary(x => x.PK, x => x);

			// There are may be many charge lines, so we would like to avoid too many SQL requests. Using batching...
			foreach (var item in apInvoice.DashAPInvoiceClusters)
			{
				var apInvoiceCluster = (DashAPInvoiceCluster)item;
				var apInvoiceChargeLinesDictionary = apInvoiceCluster.DashAPInvoiceChargeLines.ToDictionary(x => x.PK, x => (DashAPInvoiceChargeLine)x);
				var apInvoiceChargeLinesPksBatches = apInvoiceChargeLinesDictionary.Keys.Batch(100);

				foreach (var pkBatch in apInvoiceChargeLinesPksBatches)
				{
					var apInvoiceChargeLineRefsGroups = currentFactory
						.Load<DashAPInvoiceChargeLineRef>(new ZQuery(DashAPInvoiceChargeLineRefSchema.DLR_DPL_ChargeLineID, pkBatch))
						.GroupBy(x => x.DLR_DPL_ChargeLineID);

					foreach (var group in apInvoiceChargeLineRefsGroups)
					{
						if (apInvoiceChargeLinesDictionary.TryGetValue(group.Key, out var apInvoiceChargeLine))
						{
							var apInvoiceChargeLineRefCollection = new DashAPInvoiceChargeLineRefCollection(apInvoiceChargeLine);
							using (apInvoiceChargeLineRefCollection.SuspendListChanged())
							{
								apInvoiceChargeLineRefCollection.AddRange(group);
							}

							foreach (var refItem in apInvoiceChargeLineRefCollection)
							{
								var apInvoiceChargeLineRef = (DashAPInvoiceChargeLineRef)refItem;
								if (apInvoiceRefs.TryGetValue(apInvoiceChargeLineRef.DLR_DPR_RefID, out var dashAPInvoiceRef))
								{
									apInvoiceChargeLineRef.SetDashAPInvoiceRef((DashAPInvoiceRef)dashAPInvoiceRef);
								}
							}

							apInvoiceChargeLine.SetDashAPInvoiceChargeLineRefs(apInvoiceChargeLineRefCollection);
						}
					}
				}
			}
		}

		public bool UpdateStatusToComplete(DashDocument dashDocument, bool callFactorySaveAfterUpdatingStatus = false)
		{
			dashDocument.DDD_ParseStatus = SharedConstants.ParseStatus.Code.Complete;

			var shipamaxParseResult = new ShipamaxParseResult()
			{
				ParseStatus = ShipamaxParseStatus.Complete
			};

			try
			{
				shipamaxService.SaveParseResult(dashDocument.DDD_DocID.ToGuid(), dashDocument.DDD_DocToken, shipamaxParseResult);
			}
			catch (ShipamaxServiceException ex)
			{
				using (dashErrorReporter.GatherAdditionalInformation(dashDocument))
				{
					ErrorReporter.ReportOnce($"{GetType()}_{nameof(UpdateStatusToComplete)}_RunningError", ex.Message, ex);
				}
				return false;
			}

			if (callFactorySaveAfterUpdatingStatus)
			{
				dashDocument.Factory.Save();
			}

			return true;
		}
	}
}
