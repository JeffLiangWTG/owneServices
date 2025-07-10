using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Dash.Business.Services
{
	public interface IDashEntitiesService
	{
		DashAPInvoice[] LoadAPInvoices(IEnumerable<ZGuid> dashDocumentPKs, BusinessObjectFactory factory = null);

		bool UpdateStatusToComplete(DashDocument dashDocument, bool callFactorySaveAfterUpdatingStatus = false);
	}
}
