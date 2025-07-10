using Enterprise.Core.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IGridColumnReference
	{
		ZGridColumnInfo CreateGridColumnInfo();

		string ColumnName { get; }
	}
}
