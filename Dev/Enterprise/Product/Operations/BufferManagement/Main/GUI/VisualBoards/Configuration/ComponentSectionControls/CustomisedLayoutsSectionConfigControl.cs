using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI
{
	public partial class CustomisedLayoutsSectionConfigControl : ZUserControl
	{
		public CustomisedLayoutsSectionConfigControl()
		{
			InitializeComponent();
		}

		void RelevantLayoutsGrid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			GridEntityFormOpener.OpenForm(RelevantLayoutsGrid, e, () =>
			{
				var selectedRow = RelevantLayoutsGrid.SelectedElements.FirstOrDefault() as ApplicableCustomisedLayout;

				if (selectedRow != null)
				{
					var factory = new BusinessObjectFactory { NameForDebugging = GetType().Name };
					return factory.Load<BMControlCustomisation>(selectedRow.LayoutPK);
				}

				return null;
			}, ControllerIDs.BMControlCustomisation);
		}
	}
}
