using System.Windows.Forms;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(FinalizeAVABRForm))]
	public class FinalizeAVABRFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.CustomsEntryHeaders.AddNew();

			var parent = new FinalizeAVABRMessageSendingActionParent(dec);

			return new FinalizeAVABRForm(parent);
		}

		public void TestFinalizeButtonCaption()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var sendButton = form.FindSingle<Button>("SendButton");
				AssertEquals("Finalize", sendButton.Text);
			}
		}
	}
}
