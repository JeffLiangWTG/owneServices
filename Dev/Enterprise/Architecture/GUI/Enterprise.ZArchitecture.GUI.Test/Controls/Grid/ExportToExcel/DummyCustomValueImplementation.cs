using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Excel.Testing
{
	sealed class DummyCustomValueImplementation : IExcelExportCustomValue, IExcelExportCellComment, IExcelExportCellColor
	{
		#region IExcelExportCustomValue Members

		public IZType GetCustomValue(BusinessObject bizObj)
		{
			return (ZString)ValueForTest;
		}

		public ZString GetValueFormat(IZType value)
		{
			return "";
		}

		public ZString GetDescription()
		{
			return DescriptionForTest;
		}

		#endregion

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

		public const string ValueForTest = "Test Value";

		public const string DescriptionForTest = "Test Description";
	}
}
