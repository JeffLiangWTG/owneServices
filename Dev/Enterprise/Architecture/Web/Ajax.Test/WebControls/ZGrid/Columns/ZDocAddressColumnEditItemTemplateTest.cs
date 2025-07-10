using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZDocAddressColumnEditItemTemplateTest : ZDocAddressColumnItemTemplateTest
	{
		protected override Type ExpectedItemTemplateType
		{
			get { return typeof(ZDocAddressColumnEditItemTemplate); }
		}

		protected override Type ExpectedColumnType
		{
			get { return typeof(ZDocAddressColumn); }
		}
	}
}
