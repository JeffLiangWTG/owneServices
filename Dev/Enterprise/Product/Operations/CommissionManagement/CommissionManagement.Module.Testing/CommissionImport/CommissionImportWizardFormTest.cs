using System.Windows.Forms;
using Enterprise.CommissionManagement.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Module.Testing
{
	[TestedType(typeof(CommissionImportWizardForm))]
	public class CommissionImportWizardFormTest : ZFormBasherTest
	{
		#region Overrides

		protected override Form GetFormToBashCore()
		{
			var commissionLineCollection = new AccCommissionLineCollection(Factory);
			var flattenedCollection = new CommissionFlattenedCollection(Factory);
			var flattenedCollectionInfo = new CommissionImportCollectionInfo(flattenedCollection, false);
			var flattenedProcessor = new CommissionFlattenedDataTransferProcessor(commissionLineCollection, flattenedCollectionInfo);
			var saveProcessor = new CommissionSaveProcessor(Factory);
			return new CommissionImportWizardForm(flattenedCollectionInfo, "XXX", flattenedProcessor, saveProcessor);
		}

		#endregion
	}
}
