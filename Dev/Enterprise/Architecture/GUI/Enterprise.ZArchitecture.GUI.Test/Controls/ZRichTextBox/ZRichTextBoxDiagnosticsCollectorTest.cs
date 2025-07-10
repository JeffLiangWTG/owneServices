using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Interop.DataObjects;

namespace Enterprise.ZArchitecture.GUI.RichEdit.Testing
{
	sealed class ZRichTextBoxDiagnosticsCollectorTest : TestCaseWithFactory
	{
		public void TestNotifyObjectInserted_WithBitmap()
		{
			using (var data = ZDataObject.FromData(SystemIcons.Hand.ToBitmap()))
			{
				Collector.NotifyObjectInserted(data);
				Assert("Expected diagnostic from NotifyObjectInserted", Collector.AsString.Contains(data.GetType().Name + " inserted with size -1"));
			}
		}

		public void TestNotified_AfterInsertObject()
		{
			using (var form = new ZForm())
			using (var box = new TestRichTextBox())
			{
				form.Controls.Add(box);
				box.CreateControl();
				var data = ZDataObject.FromData("hello");
				box.InsertObject(data);

				Assert("Expected diagnostic from really inserting an objet",
					ZRichTextBoxDiagnosticsCollector.Instance.AsString.EndsWith(
					data.GetType().Name + " inserted with size -1 with supported formats (System.String,UnicodeText,Text)\r\n"));
			}
		}

		class TestRichTextBox : ZRichTextBox
		{
			public new RichTextBox RichEdit
			{
				get { return base.RichEdit; }
			}
		}

		readonly ZRichTextBoxDiagnosticsCollector Collector = new ZRichTextBoxDiagnosticsCollector();
	}
}
