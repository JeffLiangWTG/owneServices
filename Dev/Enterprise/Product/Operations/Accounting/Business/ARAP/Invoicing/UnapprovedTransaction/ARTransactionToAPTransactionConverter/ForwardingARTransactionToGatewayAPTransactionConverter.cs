using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.ZArchitecture;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	internal class ForwardingARTransactionToGatewayAPTransactionConverter : ARTransactionToAPTransactionConverterBase
	{
		public ForwardingARTransactionToGatewayAPTransactionConverter(NotificationBuffer notificationBuffer) : base(notificationBuffer)
		{
		}

		void SetXmlLineInvoicingJobToGatewayConsol(TxnHeader xmlInvoiceHeader, IJobCostingPlugIn consol)
		{
			foreach (TxnLine xmlInvoiceLine in xmlInvoiceHeader.TxnLines)
			{
				xmlInvoiceLine.OriginalShipmentJobNumber = xmlInvoiceLine.ConsolOrJobNo;
				xmlInvoiceLine.ConsolOrJobNo = consol.JK_UniqueConsignRef;
				xmlInvoiceLine.ConsolOrJobType = TxnLineConsolOrJobType.GCN;
				xmlInvoiceLine.Sequence = ZString.Empty;
			}
		}

		void SetXmlLineRelatedJob(TxnHeader xmlInvoiceHeader, InvoicingBase transaction)
		{
			var arLines = transaction.Lines.Cast<InvoicingLineBase>();
			var xmlLines = xmlInvoiceHeader.TxnLines.Cast<TxnLine>();
			var matchingInvoiceLinesAndXmlLines = from arLine in arLines
							 join xmlLine in xmlLines
							 on arLine.PK.ToString() equals xmlLine.TxnLineGUID.ToString()
							 select (arLine, xmlLine);
			foreach (var (arLine, xmlLine) in matchingInvoiceLinesAndXmlLines)
			{
				if (arLine.Job != null)
				{
					xmlLine.RelatedJobID = arLine.Job.JH_ParentID.ToString();
					xmlLine.RelatedJobNumber = arLine.Job.JH_JobNum;
				}
			}
		}

		#region Overrides

		protected override string DataAdapterType => "ForwardingToGatewayIntercompanyTransactionFinancialInvoiceDataAdapter";

		protected override void AdjustXmlInvoiceLineValuesBeforeImport(IJobCostingPlugIn consol, TxnHeader xmlInvoiceHeader, InvoicingBase transaction)
		{
			SetXmlLineInvoicingJobToGatewayConsol(xmlInvoiceHeader, consol);
			SetXmlLineRelatedJob(xmlInvoiceHeader, transaction);
		}

		protected override void AddJobsFromARTransaction(HashSet<Job> jobHeaders, InvoicingBase transaction, BusinessObjectFactory factoryForConvertedObject)
		{
		}

		#endregion
	}
}
