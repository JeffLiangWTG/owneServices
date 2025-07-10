using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.JobDeclarationForms
{
	public static class ControlExtensions
	{
		public static void RefreshControlCaptions(this Control parent)
		{
			foreach (var control in parent.FindAll<Control>())
			{
				control.GetExtension<ILabelCaptionRenderer>()?.Refresh();
			}
		}
	}

	public static class ZGridExtensions
	{
		public static void RefreshColumnCaptions(this ZGrid grid, Type dataType, IReadOnlyList<string> multipleKeys)
		{
			grid.SuspendDrawing();
			foreach (var column in grid.ColumnStyles.Cast<ZGridColumnInfo>())
			{
				if (!string.IsNullOrEmpty(column.ColumnName))
				{
					var caption = DataBoundResourceStrings.GetDataForProperty(dataType, column.ColumnName, multipleKeys)?.Caption;
					if (!string.IsNullOrEmpty(caption))
					{
						column.Caption = caption;
						var gridColumn = grid.Columns[column.ColumnName];
						if (gridColumn != null)
						{
							gridColumn.ColumnStyle.HeaderText = caption;
						}
					}
				}
			}
			grid.ResumeDrawing();
		}
	}
}
