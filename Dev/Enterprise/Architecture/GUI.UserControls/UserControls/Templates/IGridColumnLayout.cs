using System.Collections.Generic;
using Enterprise.Core.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IGridColumnLayout
	{
		IReadOnlyCollection<ZGridColumnInfo> Columns { get; }

		bool HasColumn(string columnName);
	}
}
