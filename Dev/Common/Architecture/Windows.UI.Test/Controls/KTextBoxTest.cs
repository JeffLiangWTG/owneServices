using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Windows.UI.Testing
{
	class KTextBoxTest : ControlTestCase<KTextBox>
	{
		public void TestDecimalPlaces()
		{
			Form.Controls.Add(TextBox);
			Form.Show();

			TextBox.DecimalPlaces = 2;
			TextBox.Text = "1.1234";
			AssertEquals("1.12", TextBox.Text);
			TextBox.Text = "1.1234.1234";
			AssertEquals("1.12", TextBox.Text);

			TextBox.DecimalPlaces = 0;
			TextBox.Text = "1.12";
			AssertEquals("1", TextBox.Text);
			TextBox.Text = "1.1.1";
			AssertEquals("1", TextBox.Text);
		}

#if !WINZOR

		public void TestDragAndDrop()
		{
			Form.Controls.Add(TextBox);
			Form.Show();

			Point point = TextBox.PointToScreen(new Point(5, 5));
			DragEventArgs drgevent = new DragEventArgs(new DataObject(DataFormats.Text, "Drag Text"), 0, point.X, point.Y, DragDropEffects.All, DragDropEffects.None);
			TextBox.OnDragEnter(drgevent);
			AssertEquals(DragDropEffects.Copy, drgevent.Effect);
			TextBox.OnDragOver(drgevent);
			TextBox.OnDragDrop(drgevent);
			AssertEquals("Drag Text", TextBox.Text);

			point = TextBox.PointToScreen(TextBox.GetPositionFromCharIndex(4));
			drgevent = new DragEventArgs(new DataObject(DataFormats.Text, "ing"), 0, point.X, point.Y, DragDropEffects.All, DragDropEffects.None);
			TextBox.OnDragEnter(drgevent);
			AssertEquals(DragDropEffects.Copy, drgevent.Effect);
			TextBox.OnDragOver(drgevent);
			TextBox.OnDragDrop(drgevent);
			AssertEquals("Draging Text", TextBox.Text);

			point = TextBox.PointToScreen(TextBox.GetPositionFromCharIndex(11));
			point.Offset(10, 0);
			drgevent = new DragEventArgs(new DataObject(DataFormats.Text, "."), 0, point.X, point.Y, DragDropEffects.All, DragDropEffects.None);
			TextBox.OnDragEnter(drgevent);
			AssertEquals(DragDropEffects.Copy, drgevent.Effect);
			TextBox.OnDragOver(drgevent);
			TextBox.OnDragDrop(drgevent);
			AssertEquals("Draging Text.", TextBox.Text);
		}

#endif

		#region IDisposeStackProvider

		[RequiresSTA]
		public void TestReportObjectDisposedException()
		{
			using (Db.DisposableActionForDbConnection())
			using (var form = new KForm { Name = "KFormTest" })
			using (var panel = new KPanel { Name = "KPanelTest" })
			using (var textBox = new KTextBox { TrackDisposedAccess = true })
			{
				form.Controls.Add(panel);
				textBox.Dispose();
				panel.Controls.Add(textBox);
				AssertExceptionThrown(typeof(ObjectDisposedException), form.Show);

				AssertEquals(typeof(ObjectDisposedException), ErrorReporter.LastExceptionReported.GetType());
				AssertStartsWith("Exception Message Should Start With", "Control [name:'' type:'CargoWise.Windows.UI.KTextBox'] is already disposed.", ErrorReporter.LastMessageReported);
				AssertNotContains("Control Path: Empty\r\nControl Dispose stack trace:\r\nEmpty", ErrorReporter.LastExceptionReported.Message);
				ErrorReporter.Clear();
			}
		}

		#endregion

		#region Test Classes

		[CodeAlive("WI00575476 Baseline")]
		protected class TestDataSource : ComponentModel.Testing.KComponent
		{
			public decimal DecimalValue { get; set; }
		}

		#endregion

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
		}

		KForm Form
		{ get { return form ?? (form = new KForm()); } }
		KForm form;

		KTextBoxForTest TextBox
		{ get { return textBox ?? (textBox = new KTextBoxForTest()); } }
		KTextBoxForTest textBox;
		#endregion

		class KTextBoxForTest : KTextBox
		{
			new internal void OnDragEnter(DragEventArgs drgevent) => base.OnDragEnter(drgevent);
			new internal void OnDragOver(DragEventArgs drgevent) => base.OnDragOver(drgevent);
			new internal void OnDragDrop(DragEventArgs drgevent) => base.OnDragDrop(drgevent);
		}
	}
}
