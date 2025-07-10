using System.Windows.Forms;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngine.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	internal class CASSBillingDocumentCustomisationDevTool : DocumentCustomisationDevTool
	{
		public override string Name
		{
			get { return Res.GetString("9E279BDC-193E-4AB8-8CE3-6A4C3612003B", "Customize CASS Discrepancies Report"); }
		}

		protected override void ShowCore(Form form)
		{
			ZForm zForm;
			CASSBilling bizo;

			if ((zForm = form as ZForm) == null)
			{
				Globals.Message.Show((NoResString)"Not a ZForm"); // Developer Diagnostic Tool
			}
			else if ((bizo = zForm.BusinessEntity as CASSBilling) == null)
			{
				Globals.Message.Show((NoResString)"Could not find the CASSBilling"); // Developer Diagnostic Tool
			}
			else
			{
				ShowCustomisationForm(bizo);
			}
		}
	}
}
