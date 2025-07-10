using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	sealed class PopupDynamicEditorTest : TestCaseWithFactory
	{
		#region TestCreateControl

		public void TestCreateControl()
		{
			var property1 = new Mock<IMacroBusinessObjectProperty>();
			var property2 = new Mock<IMacroBusinessObjectProperty>();

			using (var form = new ZForm())
			using (var control1 = new ZTextBox())
			using (var control2 = new ZCalcEdit())
			{
				control1.Font = new Font("Arial", 8f);
				control2.Font = new Font("Arial", 8f);

				property1.SetupGet(p => p.PropertyName).Returns("Name");
				property1.SetupGet(p => p.PropertyType).Returns(typeof(ZString));

				property2.SetupGet(p => p.PropertyName).Returns("Age");
				property2.SetupGet(p => p.PropertyType).Returns(typeof(ZInt));

				var cell = new DummyCell();
				var element = new DynamicContent(cell, new PointF(10f, 10f), new SizeF(100f, 100f));
				var content = new DynamicContentLayoutElement(element);
				var presenter = new DummyEditorPresenter();

				var editor = new PopupDynamicEditorView(content, presenter, new[] { control1, control2 }, new [] { property1.Object, property2.Object }, 1.5f);

				form.Controls.Add((Control)editor.Control);

				form.Show();

				var controlContainer = form.Controls.Find("contentPanel", true).First();

				AssertContainsExactElementsInAnyOrder("control container contains editor control",
					new[] { control1, control2 },
					controlContainer.Controls.OfType<ZTextBox>());

				AssertEquals("control.Font has been scaled -> 8 * 1.5", 12f, control1.Font.Size);
				AssertEquals("control.Font has been scaled -> 8 * 1.5", 12f, control2.Font.Size);
			}
		}

		#endregion
	}
}