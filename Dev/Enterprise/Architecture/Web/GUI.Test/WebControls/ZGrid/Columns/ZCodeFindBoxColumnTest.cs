using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZCodeFindBoxColumnTest : ZFindBoxColumnTest
	{
		protected override Type ExpectedColumnType
		{
			get { return typeof(ZCodeFindBoxColumn); }
		}
	}
}
