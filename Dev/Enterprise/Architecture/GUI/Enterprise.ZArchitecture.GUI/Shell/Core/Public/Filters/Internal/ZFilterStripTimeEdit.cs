using System.ComponentModel;
using CargoWise.Windows.UI.Testing;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	[SuppressFormDesignerAnalysis]
	[ToolboxItem(false)]
	public class ZFilterStripTimeEdit : ZTimeTimeEdit
	{
		public static int DefaultWidth
		{
			get
			{
				if (defaultWidth == -1)
				{
					using (var timeEdit = new ZFilterStripTimeEdit())
					{
						defaultWidth = timeEdit.Width;
					}
				}
				return defaultWidth;
			}
		}

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static int defaultWidth = -1;
	}
}
