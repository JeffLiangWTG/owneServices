using System.Collections.Generic;
using System.Drawing;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public class ZSearchForm : ZDropForm
	{
		public ZSearchForm(IDropFormParent parentDropEdit) : base(parentDropEdit)
		{
		}

		#if !WINZOR

		protected override IEnumerable<Rectangle> GetSpecialAreasCore()
		{
			var parent = (ZGuidSearchEdit)ParentDropEdit;
			var codeBox = parent.CodeBox;
			var location = codeBox.PointToScreen(codeBox.Location);
			yield return RectangleToClient(ControlDpiScalingHelper.NewScaledRectangle(location, codeBox.Size, false));
		}

		#endif
	}
}
