using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Startup.Tools.Testing
{
	[TestedType(typeof(DownloadForm))]
	sealed class DownloadFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new DownloadFormForTest(new Uri("https://google.com/somefile.txt"));
		}
	}
}
