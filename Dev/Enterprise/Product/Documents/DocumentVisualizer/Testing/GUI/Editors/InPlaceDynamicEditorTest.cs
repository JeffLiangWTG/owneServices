using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	sealed class InPlaceDynamicEditorTest : TestCaseWithFactory
	{
		#region TestCreateControlForVisualizerCodeFindBox

		public void TestCreateControlForVisualizerCodeFindBox()
		{
			var property = new Mock<IMacroBusinessObjectProperty>();

			using (var form = new ZForm())
			using (var control = new VisualizerCodeFindBox())
			{
				control.Font = new Font("Arial", 8f);

				property.SetupGet(p => p.PropertyName).Returns("Name");
				property.SetupGet(p => p.PropertyType).Returns(typeof(ZString));

				var cell = new DummyCell();
				var element = new DynamicContent(cell, new PointF(10f, 10f), new SizeF(100f, 100f));
				var content = new DynamicContentLayoutElement(element);
				var presenter = new DummyEditorPresenter();

				var editor = new InPlaceDynamicEditorView(content, presenter, control, property.Object, 1.5f);

				form.Controls.Add((Control)editor.Control);

				form.Show();

				var controlContainer = form.Controls.OfType<ZUserControl>().ElementAt(0);

				AssertContainsExactElementsInAnyOrder("control container contains editor control",
					new[] { control },
					controlContainer.Controls);

				AssertEquals("control.Dock", DockStyle.Fill, control.Dock);
				AssertEquals("control.Font has been scaled -> 8 * 1.5", 12f, control.Font.Size);
				AssertEquals("control.Width should be 96", 96, control.Width);
				AssertEquals("control.CodeBox.Width should be 65", 65, control.CodeBox.Width);
			}
		}

		#endregion

		#region TestCreateControl

		public void TestCreateControl()
		{
			var property = new Mock<IMacroBusinessObjectProperty>();

			using (var form = new ZForm())
			using (var control = new ZTextBox())
			{
				control.Font = new Font("Arial", 8f);

				property.SetupGet(p => p.PropertyName).Returns("Name");
				property.SetupGet(p => p.PropertyType).Returns(typeof(ZString));

				var cell = new DummyCell();
				var element = new DynamicContent(cell, new PointF(10f, 10f), new SizeF(100f, 100f));
				var content = new DynamicContentLayoutElement(element);
				var presenter = new DummyEditorPresenter();

				var editor = new InPlaceDynamicEditorView(content, presenter, control, property.Object, 1.5f);

				form.Controls.Add((Control)editor.Control);

				form.Show();

				var controlContainer = form.Controls.OfType<ZUserControl>().ElementAt(0);

				AssertContainsExactElementsInAnyOrder("control container contains editor control",
					new[] { control },
					controlContainer.Controls);

				AssertEquals("control.Dock", DockStyle.Fill, control.Dock);
				AssertEquals("control.Font has been scaled -> 8 * 1.5", 12f, control.Font.Size);
			}
		}

		#endregion

		#region TestCommitEditOnEnter

		[ExpectNoExceptions]
		public void TestCommitEditOnEnter()
		{
			var property = new Mock<IMacroBusinessObjectProperty>();
			var presenter = new Mock<IEditorPresenter>();

			using (var form = new ZForm())
			using (var control = new ZTextBox())
			{
				property.SetupGet(p => p.PropertyName).Returns("Name");
				property.SetupGet(p => p.PropertyType).Returns(typeof(ZString));

				presenter.Setup(p => p.CommitChanges());

				var cell = new DummyCell();
				var element = new DynamicContent(cell, new PointF(10f, 10f), new SizeF(100f, 100f));
				var content = new DynamicContentLayoutElement(element);

				var editor = new InPlaceDynamicEditorView(content, presenter.Object, control, property.Object, 1.5f);

				form.Controls.Add((Control)editor.Control);

				form.Show();

				control.Parent.SendCmdKey(Keys.Enter);
				Application.DoEvents();

				presenter.Verify(p => p.CommitChanges(), Times.Once);
			}
		}

		#endregion

		#region TestCancelEditOnEscape

		[ExpectNoExceptions]
		public void TestCancelEditOnEscape()
		{
			var property = new Mock<IMacroBusinessObjectProperty>();
			var presenter = new Mock<IEditorPresenter>();

			using (var form = new ZForm())
			using (var control = new ZTextBox())
			{
				property.SetupGet(p => p.PropertyName).Returns("Name");
				property.SetupGet(p => p.PropertyType).Returns(typeof(ZString));

				presenter.Setup(p => p.CancelChanges());

				var cell = new DummyCell();
				var element = new DynamicContent(cell, new PointF(10f, 10f), new SizeF(100f, 100f));
				var content = new DynamicContentLayoutElement(element);

				var editor = new InPlaceDynamicEditorView(content, presenter.Object, control, property.Object, 1.5f);

				form.Controls.Add((Control)editor.Control);

				form.Show();

				control.Parent.SendCmdKey(Keys.Escape);
				Application.DoEvents();

				presenter.Verify(p => p.CancelChanges(), Times.Once);
			}
		}

		#endregion

		#region TestSelectNextControl

		[ExpectNoExceptions]
		public void TestSelectNextControl()
		{
			var property = new Mock<IMacroBusinessObjectProperty>();
			var presenter = new Mock<IEditorPresenter>();

			using (var form = new ZForm())
			using (var control = new ZTextBox())
			{
				property.SetupGet(p => p.PropertyName).Returns("Name");
				property.SetupGet(p => p.PropertyType).Returns(typeof(ZString));

				presenter.Setup(p => p.MoveToNextEditor());

				var cell = new DummyCell();
				var element = new DynamicContent(cell, new PointF(10f, 10f), new SizeF(100f, 100f));
				var content = new DynamicContentLayoutElement(element);

				var editor = new InPlaceDynamicEditorView(content, presenter.Object, control, property.Object, 1.5f);

				form.Controls.Add((Control)editor.Control);

				form.Show();

				control.Parent.SendCmdKey(Keys.Tab);
				Application.DoEvents();

				presenter.Verify(p => p.MoveToNextEditor(), Times.Once);
			}
		}

		#endregion

		#region TestSelectPreviousControl

		[ExpectNoExceptions]
		public void TestSelectPreviousControl()
		{
			var property = new Mock<IMacroBusinessObjectProperty>();
			var presenter = new Mock<IEditorPresenter>();

			using (var form = new ZForm())
			using (var control = new ZTextBox())
			{
				property.SetupGet(p => p.PropertyName).Returns("Name");
				property.SetupGet(p => p.PropertyType).Returns(typeof(ZString));
				presenter.Setup(p => p.MoveToPreviousEditor());

				var cell = new DummyCell();
				var element = new DynamicContent(cell, new PointF(10f, 10f), new SizeF(100f, 100f));
				var content = new DynamicContentLayoutElement(element);

				var editor = new InPlaceDynamicEditorView(content, presenter.Object, control, property.Object, 1.5f);

				form.Controls.Add((Control)editor.Control);

				form.Show();

				control.Parent.SendCmdKey(Keys.Shift | Keys.Tab);
				Application.DoEvents();

				presenter.Verify(p => p.MoveToPreviousEditor(), Times.Once);
			}
		}

		#endregion
	}
}
