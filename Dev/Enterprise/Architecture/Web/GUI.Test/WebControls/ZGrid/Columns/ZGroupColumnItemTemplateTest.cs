using System;
using System.Web.UI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZGroupColumnItemTemplateTest : ZItemTemplateTest
	{
		#region Implementation

		protected override ZTemplateColumn GetNewColumn(string header, string bindTo)
		{
			return (ZTemplateColumn)Activator.CreateInstance(ExpectedColumnType, new object[] { header, Array.Empty<DataGridColumn>() });
		}

		protected override Type ExpectedColumnType
		{
			get { return typeof(ZGroupColumn); }
		}

		protected override Type ExpectedItemTemplateType
		{
			get { return typeof(ZGroupColumnItemTemplate); }
		}

		#endregion
	}
}
