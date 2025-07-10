using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.MacroValueProviders;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.Visualisation
{
	class VisualiserComponentGridControlFactory : VisualiserComponentControlFactory<VisualiserComponentGrid, DataGridView>
	{
		const int GridRowSelectorPlusScrollBarWidth = 30;
		const int GridColumnMinimumWidth = 30;

		protected override DataGridView CreateCore(VisualiserComponentGrid component)
		{
			DataGridView grid = new DataGridView();
			grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			grid.Location = component.Location;
			grid.RowHeadersWidth = 10;
			grid.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
			grid.Size = ControlDpiScalingHelper.NewScaledSize(
				ControlDpiScalingHelper.UnscaleFromCurrentDpiX(component.Size.Width) + GridRowSelectorPlusScrollBarWidth,
				ControlDpiScalingHelper.UnscaleFromCurrentDpiY(component.Size.Height));
			grid.DataSource = component.DS;
			grid.DataMember = component.BindToName;

			grid.BackgroundColor = Color.AliceBlue;
			grid.DefaultCellStyle.BackColor = Color.AliceBlue;
			grid.DefaultCellStyle.SelectionBackColor = EnterpriseFormLookStrategy.SelectedControlColor;
			grid.DefaultCellStyle.SelectionForeColor = Color.Black;

			grid.ColumnAdded += (sender, e) =>
			{
				if (ModifiableField.MacroRegex.IsMatch(e.Column.HeaderText))
				{
					e.Column.HeaderText = ModifiableField.GetFieldIdentifier(e.Column.HeaderText);
				}
				e.Column.FillWeight = 1;
				VisualiserDataSet.VisualiserGridStyle style = component.DS.FindStyleFromTableAndField(component.BindToName, e.Column.Name);
				if (style != null && !(e.Column is DataGridViewCheckBoxColumn))
				{
					e.Column.DefaultCellStyle = CreateGridStyle(style.CellFormat.HTextAlign, style.UseFlexCelNumericFormatting);
					ControlDpiScalingHelper.SetWidth(e.Column, Math.Max(ControlDpiScalingHelper.ScaleToCurrentDpiX(GridColumnMinimumWidth), style.CellSize.Width), false);
				}

				e.Column.SortMode = DataGridViewColumnSortMode.NotSortable;
			};

			if (component.AutoHeight)
			{
				grid.RowsAdded += (sender, e) =>
				{
					var lastIndex = e.RowIndex + e.RowCount;
					for (var index = e.RowIndex; index < lastIndex; index++)
					{
						grid.Rows[index].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
						grid.Rows[index].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopLeft;
						ControlDpiScalingHelper.SetHeight(grid.Rows[index], grid.Rows[index].Height * 2, false);
					}
				};
			}

			return grid;
		}

		DataGridViewCellStyle CreateGridStyle(HorizontalTextAlignment alignment, bool useFlexCelNumericFormatting)
		{
			DataGridViewCellStyle result = new DataGridViewCellStyle();
			result.Alignment = ConvertExcelAlignmentToGridAlignment(alignment, useFlexCelNumericFormatting);
			return result;
		}

		DataGridViewContentAlignment ConvertExcelAlignmentToGridAlignment(HorizontalTextAlignment horizontalTextAlignment, bool useFlexCelNumericFormatting)
		{
			switch (horizontalTextAlignment)
			{
				case HorizontalTextAlignment.Right:
					return DataGridViewContentAlignment.MiddleRight;
				case HorizontalTextAlignment.Centre:
					return DataGridViewContentAlignment.MiddleCenter;
				case HorizontalTextAlignment.Left:
					return DataGridViewContentAlignment.MiddleLeft;
				case HorizontalTextAlignment.General:
					return useFlexCelNumericFormatting ? DataGridViewContentAlignment.MiddleRight : DataGridViewContentAlignment.MiddleLeft;
			}
			return DataGridViewContentAlignment.MiddleLeft;
		}
	}
}
