using System;
using CargoWise.Windows.UI;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZBindingTabPageTest : TestCase
	{
		public void TestDataMember_WithDisposedControl()
		{
			var testPage = new ZTabPage();
			testPage.Dispose();
			AssertExceptionThrown("message", typeof(ObjectDisposedException), () => KBindingSource.GetBindingSource(testPage));
		}
	}
}
