using System;
using System.Windows.Forms;

using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture
{
	public interface IGridControl
	{
		int SelectionStart { get; set; }
		int SelectionLength { get; set; }
		string Text { get; set; }
		event EventHandler TextChanged;
		int ButtonWidth { get; }
		int MaxLength { get; set; }
		event KeyEventHandler KeyDown;
		void ActivateEditControl();
		bool ShouldHandleKey(Keys keyData);
		bool ShownForReadOnly { get; }
	}

	public interface IZColumnStyleInfo
	{
	}

	internal interface IZColumnStyleInfoWithModuleID : IZColumnStyleInfo
	{
		ModuleIdentifier ModuleID { get; set; }
	}

	internal interface IZColumn
	{
	}
}