using System.Windows.Forms;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.BatchPosting.Testing
{
	[TestedType(typeof(BatchPostingForm))]
	public class BatchPostingFormTest : ZFormBasherTest
	{
		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			if (control.Name == "ProgressTextBox")
			{
				return true;
			}
			return base.ShouldIgnoreMissingBindingMember(control);
		}

		protected override Form GetFormToBashCore()
		{
			BaseBatchInvoicingPostManagerGUIWrapper guiWrapper = GetGuiWrapperForTest();
			BatchPostingBusinessObject batchPostingBizObj = GetBatchPostingBizObjForTest(guiWrapper);
			BatchPostingForm formToTest = new BatchPostingForm(batchPostingBizObj);
			return formToTest;
		}

		BatchPostingBusinessObject GetBatchPostingBizObjForTest(BaseBatchInvoicingPostManagerGUIWrapper guiWrapper)
		{
			return new BatchPostingBusinessObject(guiWrapper);
		}

		JobBatchInvoicingPostManagerGUIWrapper GetGuiWrapperForTest()
		{
			return new JobBatchInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, System.Array.Empty<Job>(), "All", "");
		}
	}
}
