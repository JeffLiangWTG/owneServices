using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.GUI.DocumentMenu;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.GUI
{
	public class ReportCustomisationMenusMaker : CustomisationMenusMaker<MenuItem>
	{
		readonly string businessContext;

		public ReportCustomisationMenusMaker(Form parentForm, ISecurityCheckpoint customisationSecurityCheckpoint, string businessContext)
			: base(parentForm, customisationSecurityCheckpoint, new ZDocumentsMenuItemMenuHelper())
		{
			this.businessContext = businessContext;
		}

		protected override MultilingualString CustomizeMenuText
		{
			get { return ResString.GetMultilingualString("9c67422c-6cfd-4e2e-872d-3068a0dbb035", "Customize Reports"); }
		}

		protected override IMenuCustomisationForm GetCustomisationForm()
		{
			ReportMenuCustomisation customisation = new ReportMenuCustomisation(new BusinessObjectFactory(), businessContext);
			var form = new ReportCustomisationForm(customisation);
			form.FormClosed += ReportCustomisationForm_FormClosed;
			return form;
		}

		void ReportCustomisationForm_FormClosed(object sender, FormClosedEventArgs e)
		{
#if DEBUG
			UpdateCheckedOut();
#endif
		}

#if DEBUG

		protected override string CustomisationMenusDescription => "Reports";

#endif
	}
}
