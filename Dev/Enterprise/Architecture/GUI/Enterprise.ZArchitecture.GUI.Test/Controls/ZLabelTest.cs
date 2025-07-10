using System.Data;
using System.Reflection;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ZLabelTest : ZControlBaseTestCase<ZLabel>
	{
		protected override string[] BindablePropertyNames
		{
			get { return new[] { "Text", "IsVisibleForBinding" }; }
		}

		public void TestBindableZLabel()
		{
			var table = new DataTable();
			table.Columns.Add("TextColumn");

			DataBoundControl.Get(Control).SetDataBinding(table, "TextColumn");
			AssertEquals("Bindings added after SetDataBinding", 1, Control.DataBindings.Count);

			DataBoundControl.Get(Control).SetDataBinding(null, "");
			AssertEquals("Bindings cleared after unbinding", 0, Control.DataBindings.Count);
		}

		public void TestShouldSerializeCaptionResourceStringForDesigner()
		{
			var methodName = $"ShouldSerialize{nameof(ZLabel.CaptionResourceString)}";
			AssertNotNull($"{nameof(ZLabel)} should have the method '{methodName}' defined for visual studio designer.", typeof(ZLabel).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic));
		}
	}
}
