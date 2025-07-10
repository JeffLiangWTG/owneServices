using System.ComponentModel;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	[SuppressFormDesignerAnalysis]
	[ToolboxItem(false)]
	public class ZFilterStripDateEdit : ZDateEdit
	{
		protected override int GetDateFormatWidth()
		{
			return ControlDpiScalingHelper.ScaleToCurrentDpiX(96);
		}

		public static int DefaultWidth
		{
			get
			{
				if (defaultWidth == -1)
				{
					using (var dateEdit = new ZFilterStripDateEdit())
					{
						defaultWidth = dateEdit.Width;
					}
				}
				return defaultWidth;
			}
		}

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static int defaultWidth = -1;
	}
}
