using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Accounting.APAutomation.APReconciliation
{
	public interface IReconciliationLineConverter
	{
		public void ImportReconciliationLinesToInvoice(InvoicingBase invoice, IEnumerable<IReconciliationLineIdentifier> reconciliationLines);
	}

	[CodeAlive("Will be implemented in a future workitem")]
	public class ReconciliationLineConverter : IReconciliationLineConverter
	{
		void IReconciliationLineConverter.ImportReconciliationLinesToInvoice(InvoicingBase invoice, IEnumerable<IReconciliationLineIdentifier> reconciliationLines)
		{
			var importer = ObjectFactory.Get<IInvoicingBaseLineImporter>();

			importer.ImportLinesFromChargeCollection(invoice
				, RetriveCharges(invoice.Factory, reconciliationLines));

			importer.ImportLinesFromConsolCostCollection(invoice
				, RetriveConsolCosts(invoice.Factory, reconciliationLines));
		}

		IEnumerable<Charge> RetriveCharges(BusinessObjectFactory factory, IEnumerable<IReconciliationLineIdentifier> reconciliationLines)
		{
			var chargePKs = reconciliationLines
				.Where(x => x.LineType == APReconciliationLineTypes.JobCharge)
				.Select(x => x.LineIdentifier)
				.ToArray();

			return factory.Load<Charge>(new ZQuery(JobChargeSchema.PK, chargePKs));
		}

		IEnumerable<JobConsolCost> RetriveConsolCosts(BusinessObjectFactory factory, IEnumerable<IReconciliationLineIdentifier> reconciliationLines)
		{
			var consolCostPKs = reconciliationLines
				.Where(x => x.LineType == APReconciliationLineTypes.ConsolCost)
				.Select(x => x.LineIdentifier)
				.ToArray();

			return factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.PK, consolCostPKs));
		}
	}
}
