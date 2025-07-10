using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Core.Forms
{
	[ToolboxItem(false)]
	public class ZPostOrCancelButton : ZButton, IPostOrCancel
	{
		public ZPostOrCancelButton()
		{
			TabStop = true;
		}
	}
}
