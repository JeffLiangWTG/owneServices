using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.GUI.Visualisation
{
	class VisualiserComponentTextBoxControlFactory : VisualiserComponentControlFactory<VisualiserComponentTextBox, ZTextBox>
	{
		protected override ZTextBox CreateCore(VisualiserComponentTextBox component)
		{
			ZTextBox result = new ZTextBox();
			result.Multiline = true;
			result.Location = component.Location;
			result.Size = component.Size;
			result.WordWrap = true;
			result.DataBindings.Add((NoResString)"Text", component.VisualiserDataSet.MainTable, component.BindToName, false, DataSourceUpdateMode.OnPropertyChanged); // We're not binding to BusinessObjects, so we can't use Z-Binding.
			result.BorderStyle = BorderStyle.None;
			result.Font = component.CellFormat.GetFont();
			result.ForeColor = component.CellFormat.TextColor;
			result.TextAlign = GetHorizontalAlignment(component.CellFormat.HTextAlign);
			result.CharacterCasing = CharacterCasing.Normal;
			if (component.CellFormat.FillPattern != FillPatternStyle.None && component.CellFormat.BackgroundColor.A != 0)
			{
				result.BackColor = component.CellFormat.BackgroundColor;
			}
			else
			{
				result.BackColor = Color.AliceBlue;
			}
			result.Leave += delegate(object sender, EventArgs e)
			{ component.VisualiserDataSet.MainTable.AcceptChanges(); };

			if (component.VisualiserDataSet.DataSetName == Report.NullVisualiserDataSetId)
			{
				result.ReadOnly = true;
			}

			return result;
		}

		public static HorizontalAlignment GetHorizontalAlignment(HorizontalTextAlignment input)
		{
			switch (input)
			{
				case HorizontalTextAlignment.General:
				case HorizontalTextAlignment.Left:
				case HorizontalTextAlignment.Justify:
					return HorizontalAlignment.Left;

				case HorizontalTextAlignment.Centre:
					return HorizontalAlignment.Center;

				case HorizontalTextAlignment.Right:
					return HorizontalAlignment.Right;
			}
			throw new InvalidOperationException("The above switch statement should be catering for all possible types of HorizontalTextAlignment. [" + input + "] has not been handled.");
		}

#if DEBUG
		static internal HorizontalAlignment GetHorizontalAlignmentForTesting(HorizontalTextAlignment input)
		{
			return GetHorizontalAlignment(input);
		}
#endif
	}
}
