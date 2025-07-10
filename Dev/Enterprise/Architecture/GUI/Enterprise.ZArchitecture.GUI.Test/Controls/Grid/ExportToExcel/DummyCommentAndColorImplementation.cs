using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Excel.Testing
{
	sealed class DummyCommentAndColorImplementation : IExcelExportCellComment, IExcelExportCellColor
	{
		#region IExcelExportCellComment Members

		public ZString GetComment(BusinessObject bizObj)
		{
			return CommentForTest;
		}

		#endregion

		#region IExcelExportCellColor Members

		public Color? GetCustomColor(BusinessObject bizObj)
		{
			return Color.Red;
		}

		#endregion

		public const string CommentForTest = "Test Comment";
	}
}
