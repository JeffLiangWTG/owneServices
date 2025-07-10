using System;
using System.Drawing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	public class ButtonStripAction<T>
	{
		public string Name { get; set; }
		public string Text { get; set; }
		public MultilingualString ToolTip { get; set; }
		public Image Image { get; set; }
		public T Response { get; set; }
		public EventHandler FireAction { get; set; }
		public bool HideFormBeforeFireAction { get; set; }
	}
}
