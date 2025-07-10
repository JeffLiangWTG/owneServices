using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(NEXDOCAcknowledgeForm))]
	sealed class NEXDOCAcknowledgeFormTest : ZFormBasherTest
	{
		public void TestLabelTexts()
		{
			var notification = Factory.New<QuarantineNexDocNotification>();
			notification.QN_NotificationType = "TA";
			notification.QN_RexNumber = "REX10000010";
			notification.QN_ExporterReference = "EXP10000010";
			using (var form = new NEXDOCAcknowledgeForm(notification))
			{
				form.Show();
				var objectNameLable = form.Controls.Find("HeaderLable", true)[0] as ZLabel;
				var rexNumberTextBox = form.Controls.Find("RexNumberTextBox", true)[0] as ZTextBox;
				var exporterReferenceTextBox = form.Controls.Find("ExporterReferenceTextBox", true)[0] as ZTextBox;
				AssertEquals("ObjectNameLable", "Do you want to Accept or Reject this Transfer", objectNameLable.Text);
				AssertEquals("RexNumberLable", "REX10000010", rexNumberTextBox.Text);
				AssertEquals("ExporterReferenceLable", "EXP10000010", exporterReferenceTextBox.Text);
			}
		}

		protected override Form GetFormToBashCore() => new NEXDOCAcknowledgeForm(Factory.New<QuarantineNexDocNotification>());
	}
}
