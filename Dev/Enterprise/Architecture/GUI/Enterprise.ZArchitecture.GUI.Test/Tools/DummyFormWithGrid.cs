using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class DummyFormWithGrid : ZForm
	{
		internal DummyFormWithGrid(DummyBusinessObject bizo)
			: base(bizo)
		{
			Grid = new ZGrid
			{
				Dock = DockStyle.Fill,
			};

			var columnStyle = new ZTextBoxColumnStyleInfo
			{
				ColumnName = "Z0_Description",
				IsVisible = true,
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
			};

			Grid.ColumnStyles.Add(columnStyle);

			BindingSource.SetBindingMember(Grid, "Collection");
			Controls.Add(Grid);

			Grid.MouseDoubleClick += Grid_MouseDoubleClick;

			RequireSavedFormBeforeOpeningGridEntityForm = true;
		}

		internal ZGrid Grid { get; private set; }

		internal bool RequireSavedFormBeforeOpeningGridEntityForm { get; set; }

		void Grid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (RequireSavedFormBeforeOpeningGridEntityForm)
			{
				GridEntityFormOpener.OpenFormForSavedParent((ZForm)FindForm(), Grid, e, DummyControllerIDs.Dummy);
			}
			else
			{
				GridEntityFormOpener.OpenForm(Grid, e, () => (BusinessObject)Grid.ListManager.GetCurrent(), DummyControllerIDs.Dummy);
			}
		}
	}
}
