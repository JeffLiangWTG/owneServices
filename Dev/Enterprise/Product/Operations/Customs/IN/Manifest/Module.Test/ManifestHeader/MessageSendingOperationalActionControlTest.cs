using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Module.Testing;

[TestedType(typeof(MessageSendingOperationalActionControl))]
sealed class MessageSendingOperationalActionControlTest : TestCaseWithFactory
{
	public void TestControls()
	{
		var applicator = new MessageSendingOperationalActionMethodApplicator();
		using (var form = new ZForm(applicator))
		using (var control = new MessageSendingOperationalActionControl())
		{
			form.Controls.Add(control);
			form.Show();
			var allowSendWithMessageErrorCheckBox = control.FindSingle<ZCheckBox>("AllowSendWithMessageErrorCheckBox");
			AssertEquals("AllowSendWithMessageErrorCheckBox", nameof(applicator.AllowSendWithMessageError), allowSendWithMessageErrorCheckBox.BindTo);
		}
	}
}
