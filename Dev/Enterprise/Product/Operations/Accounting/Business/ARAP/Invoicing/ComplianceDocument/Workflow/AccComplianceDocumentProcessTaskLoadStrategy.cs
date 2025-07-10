using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public class AccComplianceDocumentProcessTaskLoadStrategy : IProcessTaskLoadStrategy
	{
		public void AddAdditionalParentFilters(WorkflowDescriptor workflowDescriptor, ZDBOnlySubQuery subQuery)
		{
			switch (workflowDescriptor.Code)
			{
				case WorkflowDescriptors.ARComplianceDocumentCode:
					subQuery.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_Ledger, LedgerTypes.AccountsReceivable);
					break;
				case WorkflowDescriptors.APComplianceDocumentCode:
					subQuery.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_Ledger, LedgerTypes.AccountsPayable);
					break;
			}
		}

		public Type GetTypeForLoad(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			return typeof(AccComplianceDocumentProcessTask);
		}
	}
}
