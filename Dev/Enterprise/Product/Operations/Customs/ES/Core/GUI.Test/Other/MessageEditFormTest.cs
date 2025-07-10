using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing
{
	[TestedType(typeof(MessageEditForm))]
	public class MessageEditFormTest : ZFormBasherTest
	{
		#region Overrides

		protected override Form GetFormToBashCore() => new MessageEditForm();

		#endregion

		public void TestEditMessage()
		{
			using (var editMessageForm = new MessageEditForm())
			{
				var testString = "This is test message";
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var editMessageReturnData = editMessageForm.EditMessage(testString);
				AssertEquals(testString, editMessageReturnData.Item1);
				AssertEquals(true, editMessageReturnData.Item2);
			}
		}
	}
}
