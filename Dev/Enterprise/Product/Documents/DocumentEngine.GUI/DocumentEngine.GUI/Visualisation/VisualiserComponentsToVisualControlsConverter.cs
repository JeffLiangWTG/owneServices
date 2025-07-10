using System;
using System.Collections.Generic;
using System.Windows.Forms;

using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.GUI.Visualisation
{
	/// <summary>
	/// Creates form controls on a given control using the collection of VisualComponents given.
	/// </summary>
	internal class VisualiserComponentsToVisualControlsConverter : IVisualiserDrawer
	{
		public VisualiserComponentsToVisualControlsConverter(Control parentControl)
		{
			this.parentControl = parentControl;
		}

		readonly Control parentControl;
		int RightMostX = -1;
		int BottomY = -1;
		List<DataGridView> Grids;

		public void Draw(IEnumerable<VisualiserComponent> components)
		{
			VisualiserComponentControlFactory controlFactory = new VisualiserComponentControlFactory();
			Grids = new List<DataGridView>();

			foreach (VisualiserComponent component in components)
			{
				Control controlToAdd = controlFactory.Create(component);
				if (controlToAdd != null)
				{
					if (controlToAdd.Right > RightMostX)
					{
						RightMostX = controlToAdd.Right;
					}

					if (controlToAdd.Bottom > BottomY)
					{
						BottomY = controlToAdd.Bottom;
					}

					parentControl.Controls.Add(controlToAdd);

					DataGridView grid = controlToAdd as DataGridView;
					if (grid != null)
					{
						grid.DataError += Grid_DataError;
						Grids.Add(grid);
					}
				}
			}
			foreach (Control cnt in parentControl.Controls)
			{
				if (cnt is PictureBox)
				{
					cnt.SendToBack();
				}
			}

			parentControl.VisibleChanged += new EventHandler(ParentControl_VisibleChanged);
		}

		void Grid_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			Globals.Message.Show(e.Exception.Message);
			e.Cancel = false;
		}

		void ParentControl_VisibleChanged(object sender, EventArgs e)
		{
			if (((Control)sender).Visible)
			{
				foreach (DataGridView grid in Grids)
				{
					grid.Show();
					List<string> columnHeadings = new List<string>();
					foreach (DataGridViewColumn column in grid.Columns)
					{
						columnHeadings.Add(column.HeaderText);
					}
					int commonTextInAllHeadersLength = GetCommonText(columnHeadings).Length;
					if (commonTextInAllHeadersLength != 0)
					{
						foreach (DataGridViewColumn column in grid.Columns)
						{
							column.HeaderText = column.HeaderText.Remove(0, commonTextInAllHeadersLength);
						}
					}

					ControlDpiScalingHelper.SetWidth(grid, RightMostX - grid.Left, false);
				}

				((Control)sender).VisibleChanged -= new EventHandler(ParentControl_VisibleChanged);
			}
		}

		internal string GetCommonText(List<string> columnHeadings)
		{
			string result = "";
			if (columnHeadings.Count > 1)
			{
				bool emptyStringFound = false;
				foreach (string columnHeading in columnHeadings)
				{
					if (columnHeading.Length == 0)
					{
						emptyStringFound = true;
						break;
					}
				}
				if (!emptyStringFound)
				{
					int matchLength = 1;
					while (MatchFoundInAll(columnHeadings, columnHeadings[0].Substring(0, matchLength)))
					{
						matchLength++;
					}
					result = columnHeadings[0].Substring(0, matchLength - 1);
				}
			}
			return result;
		}

		bool MatchFoundInAll(List<string> columnHeadings, string match)
		{
			bool result = true;
			foreach (string columnHeading in columnHeadings)
			{
				if (!columnHeading.StartsWith(match))
				{
					result = false;
					break;
				}
			}

			return result;
		}
	}
}
