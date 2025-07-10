using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.Grid;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public partial class GridRowFinderForm : ZChildForm, IHotkeyProvider
	{
		public GridRowFinderForm(GridRowFinderBusinessObject rowFinder)
			: base(rowFinder)
		{
			this.RowFinder = rowFinder;
			RegisterHotkeys();
			//need to commit value in 'Text to Search for' before search starts
			rowFinder.OnBeforeSearch += new EventHandler((o, e) => { zTextBox1.PerformControlValidation(); });
		}

		string IHotkeyProvider.TypeNameForDisplay
		{
			get { return Res.GetString("Hotkeys|GridFindForm", "Grid Find Form"); }
		}

		void RegisterHotkeys()
		{
			Hotkeys.RegisterHotKey(Keys.Control | Keys.N, new Action(() => { DoSearch(true, true); }),
				Res.GetString("9a6ffee6-b4a4-4859-873e-c6189c80c751", "Find Next"));
			Hotkeys.RegisterHotKey(Keys.Control | Keys.P, new Action(() => { DoSearch(false, true); }),
				Res.GetString("284d3d84-bf31-4e18-9384-f5912eb59eb0", "Find Previous"));
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		#region Buttons

		void previousButton_Click(object sender, EventArgs e)
		{
			DoSearch(false, false);
		}

		void nextButton_Click(object sender, EventArgs e)
		{
			DoSearch(true, false);
		}

		void DoSearch(bool isNext, bool explicitlyDeselect)
		{
			RowFinder.DoSearch(isNext, explicitlyDeselect);
		}

		void CancelButtonX_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		void toggleColumnsButton_Click(object sender, EventArgs e)
		{
			RowFinder.ToggleSearchColumns();
		}
		#endregion
	}
}
