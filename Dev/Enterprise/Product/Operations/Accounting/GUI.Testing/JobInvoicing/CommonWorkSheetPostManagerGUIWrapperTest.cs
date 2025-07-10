using System.Linq;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Accounting.GUI.JobInvoicing.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.GUI.Testing.JobInvoicing.BatchPosting.CommonWorkSheet
{
	public class CommonWorkSheetPostManagerGUIWrapperTest : ConsolInvoicingPostManagerGUIWrapperTest
	{
		protected override PostManagerGUIWrapper GUIWrapper
		{
			get
			{
				if (GUIWrapper_inner == null)
				{
					GUIWrapper_inner = new CommonWorkSheetPostManagerGUIWrapper(JobInvoicingPostingOption.Agent, Factory, Jobs.Cast<Job>(), Consol, FormForTest, Apportionments);
					GUIWrapper_inner.DoTestPostTransactions = true;
				}
				return GUIWrapper_inner;
			}
		}

		protected override void SetupPosting()
		{
			Jobs.Add(Job1);
			GUIWrapper_inner = new CommonWorkSheetPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, Jobs.Cast<Job>(), Consol, FormForTest, Apportionments);
		}
	}
}
