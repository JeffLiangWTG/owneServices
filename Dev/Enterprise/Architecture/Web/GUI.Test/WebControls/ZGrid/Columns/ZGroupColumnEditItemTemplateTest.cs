using System;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZGroupColumnEditItemTemplateTest : ZGroupColumnItemTemplateTest
	{
		#region Implementation

		protected override Type ExpectedItemTemplateType
		{
			get { return typeof(ZGroupColumnEditItemTemplate); }
		}

		#endregion
	}
}
