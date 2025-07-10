using System.Windows.Forms;

namespace Enterprise.ZArchitecture.Testing
{
	sealed class TestGridControl : Control, IGridControl
	{
		public int SelectionStart { get; set; }

		public int SelectionLength { get; set; }

		public int ButtonWidth { get { return 0; } }

		public int MaxLength { get; set; }

		public void ActivateEditControl() { }

		public bool ShouldHandleKey(Keys keyData) { return true; }

		bool IGridControl.ShownForReadOnly { get { return false; } }
	}
}
