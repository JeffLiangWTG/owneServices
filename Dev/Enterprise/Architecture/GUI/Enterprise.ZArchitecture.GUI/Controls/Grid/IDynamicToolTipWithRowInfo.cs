using Enterprise.Core.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IDynamicToolTipWithRowInfo : IDynamicToolTip
	{
		int RowNumber { get; set; }
	}
}
