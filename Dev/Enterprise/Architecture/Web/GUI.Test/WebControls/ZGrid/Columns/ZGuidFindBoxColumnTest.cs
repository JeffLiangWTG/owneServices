using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZGuidFindBoxColumnTest : ZFindBoxColumnTest
	{
		protected override Type ExpectedColumnType
		{
			get { return typeof(ZGuidFindBoxColumn); }
		}
	}
}
