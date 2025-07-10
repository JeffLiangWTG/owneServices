using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.VisualBoards.Business;

namespace Enterprise.VisualBoards.GUI
{
	public static class VisualBoardSectionsRenderer
	{
		[SuppressMessage("Microsoft.Design", "CA1031: Do not catch general exception types", Justification = "valid for handling disposables in factory methods")]
		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Not assigning pixels")]
		public static KTableLayoutPanel Render(IEnumerable<BoardSectionViewModelPair> boardSections)
		{
			var sections = boardSections.ToArray();
			var panel = new KTableLayoutPanel();
			try
			{
				using (new DisposableAction(panel.SuspendLayout, panel.ResumeLayout))
				{
					panel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
					panel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
					panel.ColumnStyles.Clear();
					panel.RowStyles.Clear();

					panel.ColumnCount = sections.Length == 1 ? 1 : sections.Length > 0 ? sections.Max(s => s.Section.Column + s.Section.ColSpan) : 0;
					panel.RowCount = sections.Length == 1 ? 1 : sections.Length > 0 ? sections.Max(s => s.Section.Row + s.Section.RowSpan) : 0;

					if (sections.Length == 1)
					{
						panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
						panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
					}
					else
					{
						if (sections.Any(s => s.Section.RowHeightPercent != 100 || s.Section.ColWidthPercent != 100))
						{
							foreach (var pair in sections)
							{
								var section = pair.Section;
								if (section.Column < panel.ColumnStyles.Count)
								{
									var columnStyle = panel.ColumnStyles[section.Column];
									if (section.ColWidthPercent < columnStyle.Width)
									{
										columnStyle.Width = section.ColWidthPercent;
									}
								}
								else
								{
									var width = section.ColWidthPercent == 0 ? 100 : (int)section.ColWidthPercent;
									panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, width));
								}
								if (section.Row < panel.RowStyles.Count)
								{
									var rowStyle = panel.RowStyles[section.Row];
									if (section.RowHeightPercent < rowStyle.Height)
									{
										rowStyle.Height = section.RowHeightPercent;
									}
								}
								else
								{
									var height = section.RowHeightPercent == 0 ? 100 : (int)section.RowHeightPercent;
									panel.RowStyles.Add(new RowStyle(SizeType.Percent, height));
								}
							}
						}
						else
						{
							for (var i = 0; i < panel.RowCount; i++)
							{
								panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
							}
							for (var i = 0; i < panel.ColumnCount; i++)
							{
								panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
							}
						}
					}

					foreach (var pair in sections)
					{
						var section = pair.Section;
						var descriptor = SectionDescriptorProvider.Get(section.MS_SectionType);
						var control = (Control)descriptor?.GetSectionControl(section, pair.ViewModel);
						if (control != null)
						{
							control.Dock = DockStyle.Fill;

							if (sections.Length == 1)
							{
								panel.Controls.Add(control, 0, 0);
							}
							else
							{
								panel.Controls.Add(control, section.Column, section.Row);
								panel.SetRowSpan(control, section.RowSpan);
								panel.SetColumnSpan(control, section.ColSpan);
							}
						}
					}
				}
			}
			catch
			{
				try
				{
					if (!panel.IsDisposed)
					{
						panel.Dispose();
					}
				}
				catch { }
				throw;
			}

			return panel;
		}
	}
}
