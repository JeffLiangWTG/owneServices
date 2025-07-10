using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Aga.Controls.Tree
{
	public partial class TreeNodeAdv
	{
		public Brush RowBackgroundBrush { get; set; }

		public List<DragDropEffects?> DropPositionEffects { get; } = new();

		public bool IsMasterActivitySalesRelation { get; set; }
	}
}
