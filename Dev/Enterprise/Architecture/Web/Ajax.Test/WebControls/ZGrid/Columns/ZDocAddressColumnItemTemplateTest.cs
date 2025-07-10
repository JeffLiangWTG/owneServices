using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZDocAddressColumnItemTemplateTest : ZItemTemplateTest
	{
		protected override Type ExpectedItemTemplateType
		{
			get { return typeof(ZDocAddressColumnItemTemplate); }
		}

		protected override Type ExpectedColumnType
		{
			get { return typeof(ZDocAddressColumn); }
		}

		protected override ZTemplateColumn GetNewColumn(string header, string bindTo)
		{
			var result = new ZDocAddressColumn();
			var grid = new ZGrid();
			grid.Columns.Add(result);
			return result;
		}
	}
}
