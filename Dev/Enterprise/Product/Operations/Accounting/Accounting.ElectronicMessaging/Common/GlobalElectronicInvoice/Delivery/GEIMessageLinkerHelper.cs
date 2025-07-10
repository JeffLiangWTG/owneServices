using System.Collections.Generic;
using System.Linq;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	internal class GEIMessageLinkerHelper
	{
		internal static IEnumerable<IStmALogParent> GetLogParentsToLinkWithGEIMessage(IStmALogParent logParentFromDeliveryContext)
		{
			var eventParentsToLink = new List<IStmALogParent> { };

			if (logParentFromDeliveryContext.LogsParentTableName == AccEInvoicingBatchSchema.Constants.TableName)
			{
				if (logParentFromDeliveryContext is AccEInvoicingBatch eInvoicingBatch)
				{
					var invoices = eInvoicingBatch.GetInvoicesWithStatus();
					if (invoices.Any())
					{
						eventParentsToLink.AddRange(invoices.Cast<IStmALogParent>());
					}

					var complianceDocuments = eInvoicingBatch.GetComplianceDocumentsWithStatus();
					if (complianceDocuments.Any())
					{
						eventParentsToLink.AddRange(complianceDocuments.Cast<IStmALogParent>());
					}
				}
			}

			return eventParentsToLink;
		}
	}
}