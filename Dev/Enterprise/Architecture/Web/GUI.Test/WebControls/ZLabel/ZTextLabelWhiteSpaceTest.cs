using System;
using System.Web.UI.WebControls;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZTextLabelWhiteSpaceTest : TestCase
	{
		#region TestConstruction

		public void TestConstruction()
		{
			AssertEquals(new Unit(10), new ZTextLabelWhiteSpace(10).Width);
		}

		#endregion

		#region TestSettingTextThrowsNotSupportedException

		[ExpectExceptionMessage(typeof(NotSupportedException), "If you want a text label, use ZTextLabel instead.")]
		public void TestSettingTextThrowsNotSupportedException()
		{
			new ZTextLabelWhiteSpace(0).Text = "blow up";
		}

		#endregion
	}
}
