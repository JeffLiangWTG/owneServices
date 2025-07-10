using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobConsolInvoiceReverser
	{
		public JobConsolInvoiceReverser(InvoicingBase aRInvoiceToReverse)
		{
			this.ARInvoiceToReverse = aRInvoiceToReverse;
			this.Factory = aRInvoiceToReverse.Factory;
		}

		readonly InvoicingBase ARInvoiceToReverse;
		readonly BusinessObjectFactory Factory;

		public bool IsJobConsolInvoice()
		{
			return RelatedJobConsolCosts.Count > 0 &&
							(ARInvoiceToReverse is ARInvoice || ARInvoiceToReverse is ARCreditNote);
		}

		public void ReverseRelatedAPInvAndApportionment()
		{
			List<ZGuid> distinctAPInvoicePKs = new List<ZGuid>();
			foreach (JobConsolCost cost in RelatedJobConsolCosts)
			{
				cost.E6_AH_ARInvoice = ZGuid.Empty;
				if (!distinctAPInvoicePKs.Contains(cost.E6_AH_APInvoice))
				{
					distinctAPInvoicePKs.Add(cost.E6_AH_APInvoice);
				}
			}

			ZQuery relatedAPInvoiceQuery = new ZQuery(AccTransactionHeaderSchema.PK, distinctAPInvoicePKs);
			List<InvoicingBase> aPInvoicesToReverse = new List<InvoicingBase>(ARInvoiceToReverse.Factory.Load<InvoicingBase>(relatedAPInvoiceQuery));
			foreach (InvoicingBase relatedAPInvoice in aPInvoicesToReverse)
			{
				foreach (InvoicingLineBase line in relatedAPInvoice.Lines)
				{
					if (line.InvoicingJob != null)
					{
						line.InvoicingJob.ClearProfitShareReferences();
					}
				}

				APInvoiceReversing reversing = new APInvoiceReversing(relatedAPInvoice);
				reversing.Reverse();
				if (reversing.ReverseTransaction != null)
				{
					((InvoicingBase)reversing.ReverseTransaction).AH_TransactionNum = InvoiceLiteralNumberGenerator.GetNextLiteralInvoiceNumber_WithSuffix(reversing.ReverseTransaction.Factory, (InvoicingBase)reversing.ReverseTransaction, relatedAPInvoice.AH_TransactionNum, "-C");
				}
			}
		}

		List<JobConsolCost> fRelatedJobConsolCosts;
		List<JobConsolCost> RelatedJobConsolCosts
		{
			get
			{
				if (fRelatedJobConsolCosts == null)
				{
					ZQuery relatedCostsQuery = new ZQuery(JobConsolCostSchema.E6_AH_ARInvoice, ARInvoiceToReverse.PK);
					fRelatedJobConsolCosts = new List<JobConsolCost>(Factory.Load<JobConsolCost>(relatedCostsQuery));
				}
				return fRelatedJobConsolCosts;
			}
		}
	}
}