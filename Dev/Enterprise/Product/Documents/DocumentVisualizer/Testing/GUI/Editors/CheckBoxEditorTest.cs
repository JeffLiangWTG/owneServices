using System.Drawing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.GUI;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	sealed class CheckBoxEditorTest : TestCaseWithFactory
	{
		public void TestEdit()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Bool = true;

			var data = dummy.Z0_Bool.MakeDynamic();

			var cell = new DummyCell();
			var element = new DynamicContent(cell, new PointF(0f, 0f), new SizeF(10f, 10f));
			var content = new DynamicContentLayoutElement(element);

			using (var scope = new MacroScope(data))
			{
				var property = new MacroBusinessObjectProperty("data", data);

				AssertEquals(typeof(ZBool), property.PropertyType);
				AssertEquals(true, property.Value);

				var editor = new CheckBoxEditorView(content, property);

				editor.EndEdit();
				AssertEquals(false, property.Value);

				editor.EndEdit();
				AssertEquals(true, property.Value);
			}
		}
	}
}