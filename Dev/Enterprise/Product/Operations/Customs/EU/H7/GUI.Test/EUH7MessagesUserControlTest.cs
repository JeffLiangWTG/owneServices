using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	sealed class EUH7MessagesUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestMessageTextTextBox()
		{
			using (var control = new EUH7MessagesUserControl())
			{
				var textBox = control.FindSingle<ZTextBox>("MessageTextTextBox");

				CombineAssertions(() =>
				{
					AssertEquals("Messages.EM_MessageTextIndentedXml", textBox.BindTo);
					AssertEquals(false, textBox.WordWrap);
					AssertEquals(ScrollBars.Both, textBox.ScrollBars);
				});
			}
		}
	}
}
