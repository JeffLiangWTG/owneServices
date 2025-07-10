using System;
using System.Web.UI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZGroupColumnTest : ZTemplateColumnTest
	{
		protected override Type ExpectedColumnType
		{
			get { return typeof(ZGroupColumn); }
		}

		protected override string GetExpectedBindTo()
		{
			return string.Empty;
		}

		protected override ZTemplateColumn GetNewColumn(string header, string bindTo)
		{
			DataGridColumn[] columns = new DataGridColumn[] { null };
			ZTemplateColumn result = (ZTemplateColumn)Activator.CreateInstance(ExpectedColumnType, new object[] { header, columns });
			return result;
		}
	}
}
