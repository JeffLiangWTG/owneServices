using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(AccComplianceDocumentProcessTask))]
	public class AccComplianceDocumentProcessTaskTest : ProcessTaskTest
	{
		public void TestProcessTask()
		{
			var arHeader = Factory.NewWithValidTestData<ARComplianceDocumentHeader>();
			arHeader.ADH_Ledger = LedgerTypes.AccountsReceivable;
			var arProcessTask = ((AccComplianceDocumentProcessTaskCollection)arHeader.WorkflowItems).AddNew();

			AssertEquals("ParentControllerID", ControllerIDs.ARComplianceDocument, arProcessTask.ParentControllerID);

			var apHeader = Factory.NewWithValidTestData<APComplianceDocumentHeader>();
			apHeader.ADH_Ledger = LedgerTypes.AccountsPayable;
			var apProcessTask = ((AccComplianceDocumentProcessTaskCollection)apHeader.WorkflowItems).AddNew();

			AssertEquals("ParentControllerID", ControllerIDs.APComplianceDocument, apProcessTask.ParentControllerID);
		}

		public void TestNoHeaderReturnsNullControllerID()
		{
			// Standalone task with no parent/header
			var task = Factory.NewWithValidTestData<AccComplianceDocumentProcessTask>();
			AssertEquals("Pre-condition", false, task.Parent is AccComplianceDocumentHeader);
			AssertNull(task.ParentControllerID);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<ARComplianceDocumentHeader>();
			header.ADH_Ledger = LedgerTypes.AccountsReceivable;
			return ((AccComplianceDocumentProcessTaskCollection)header.WorkflowItems).AddNew();
		}

		#endregion
	}
}
