using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZGuidDropDownListColumnTest : ZDropDownListColumnTest
	{
		#region setup

		protected override Type ExpectedColumnType
		{
			get { return typeof(ZGuidDropDownListColumn); }
		}

		#endregion
	}
}
