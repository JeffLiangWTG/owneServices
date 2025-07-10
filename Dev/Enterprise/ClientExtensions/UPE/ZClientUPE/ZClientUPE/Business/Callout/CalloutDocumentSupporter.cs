using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Client.UPE.Business
{
	public class CalloutDocumentSupporter : UPECusHAWBDocumentSupporter
	{
		public CalloutDocumentSupporter(UPECusHAWB cusHAWB)
			: base(cusHAWB)
		{
		}

		public Callout Callout
		{
			get { return (Callout)Factory.Load(typeof(Callout), CusHAWB.PK); }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result;
			if (dataContext == Core.Constants.DataContext.CusHAWB)
			{
				result = new DocumentWrapper[] { DocCallout.New(Callout, Factory) };
			}
			else
			{
				result = base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
			}
			return result;
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			return new OrgHeaderContact(Callout.BillTo, null);
		}

		protected override void OnDocumentPrintRequested(DocumentCancelEventArgs e)
		{
			base.OnDocumentPrintRequested(e);
			if (e.MenuItem.SU_MenuName == UPEDocumentMenuItemLoader.UPSTaxInvoiceQueueForBatchPrintMenuName)
			{
				UPEPrintBatch currentPrintBatch = new UPEPrintBatch.Loader(Factory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.TaxInvoice);
				currentPrintBatch.QueueForBatchPrintAndSave(Callout, new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice().PK);
				e.Cancel = true;
			}
			else if (e.MenuItem.SU_MenuName == UPEDocumentMenuItemLoader.UPSTaxInvoiceAndCommercialInvoiceQueueForBatchPrintMenuName)
			{
				UPEPrintBatch currentPrintBatch = new UPEPrintBatch.Loader(Factory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.TaxInvoice);
				currentPrintBatch.QueueForBatchPrintAndSave(Callout, new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoiceAndCommercialInvoice().PK);
				e.Cancel = true;
			}
		}
	}
}
