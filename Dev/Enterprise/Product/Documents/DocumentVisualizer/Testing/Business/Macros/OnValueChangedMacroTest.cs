using CargoWise.Macros;
using CargoWise.Macros.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class OnValueChangedMacroTest : TestCaseWithMacros
	{
		public void TestOnValueChangedMacro()
		{
			var dummy = new Dummy();
			dummy.Code = "AAA";
			dummy.Description = "AAA DESC";
			var data = dummy.MakeDynamic();

			const string macro = "Code.OnValueChanged({Description.SetValue(\"111\")})";

			var expr = macro
				.With<DocumentLibrary>()
				.And<DataLibrary>().CreateExpression();

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);
			}

			var propertyCode = data.GetDynamicProperty("Code");
			var propertyDescription = data.GetDynamicProperty("Description");
			AssertEquals("AAA", propertyCode.ToString());
			AssertEquals("AAA DESC", propertyDescription.ToString());

			propertyCode.SetValue("XXX");

			AssertEquals("XXX", propertyCode.ToString());
			AssertEquals("111", propertyDescription.ToString());
		}

		#region Implementation

		class Dummy
		{
			public ZString Code { get; set; }
			public ZString Description { get; set; }
		}

		#endregion
	}
}