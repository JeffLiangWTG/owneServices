using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZDateTimeStatusColumnTest : ZDateTimeColumnTest
	{
		protected override Type ExpectedColumnType
		{
			get { return typeof(ZDateTimeStatusColumn); }
		}
	}
}
