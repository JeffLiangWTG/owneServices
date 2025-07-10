using System.ComponentModel.Design.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace CargoWise.Windows.UI
{
	[DesignerSerializer(typeof(Design.ControlMostDerivedTypeCodeDomSerializer), typeof(CodeDomSerializer))]
	public class KCheckedListBox : CheckedListBox
	{
		public KCheckedListBox() : base() { }

		#region Implementation

		int lastSelectedIndex = -1;

		[SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a button")]
		protected override void OnMouseMove(MouseEventArgs e)
		{
			var index = IndexFromPoint(e.X, e.Y);
			if (index != -1 && index != lastSelectedIndex)
			{
				var itemText = Items[index].ToString();
				ToolTipService.SetToolTip(this, itemText);
				lastSelectedIndex = index;
			}
			base.OnMouseMove(e);
		}

		#endregion
	}
}
