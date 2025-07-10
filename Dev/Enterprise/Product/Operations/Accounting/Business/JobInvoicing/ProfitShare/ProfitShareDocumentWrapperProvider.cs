using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare
{
	public class ProfitShareDocumentWrapperProvider : IProfitShareDocumentWrapperProvider
	{
		public DocumentWrapper[] GetDocumentWrappers(IJobCostingPlugIn plugIn, BusinessObjectFactory factory)
		{
			List<DocumentWrapper> result = new List<DocumentWrapper>();
			ProfitShareDetailCollection createdProfitShares = new ProfitShareCalculator(factory, plugIn, plugIn.CostSupporter.ShipmentsList).CreateProfitShares();
			foreach (ProfitShareDetail detail in createdProfitShares)
			{
				DocumentWrapper wrapper = DocumentWrapperFactory.CreateWrapper(Constants.DataContext.ProfitShareDetail, detail);
				result.Add(wrapper);
			}

			return result.ToArray();
		}
	}
}
