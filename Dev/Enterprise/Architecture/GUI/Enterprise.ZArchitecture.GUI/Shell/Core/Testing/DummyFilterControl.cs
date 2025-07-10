#if DEBUG

using System.Windows.Forms;
using CargoWise.EntityFramework;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.ZArchitecture.Modules.Testing
{
	public interface IDummyFilterControl
	{
		ToolStripSplitButton FindButton { get; }
		void ResetFilter();
		IBusinessObjectCollection GridList { get; }
		void SelectRow(int rowIndex);
		void UnselectRow(int rowIndex);
	}
}

#endif
