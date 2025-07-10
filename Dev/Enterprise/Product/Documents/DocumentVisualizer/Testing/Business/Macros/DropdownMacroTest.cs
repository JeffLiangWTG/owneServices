using CargoWise.Macros;
using CargoWise.Macros.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DropdownMacroTest : TestCaseWithMacros
	{
		public void TestCustomDropDown()
		{
			var macro = "Description.SetDropDown([\"A\", \"B\", \"A\"])";

			var dummy = new Dummy();
			dummy.Description = "aaa";

			var data = dummy.MakeDynamic();

			var expr = macro
				.With<MetaDataLibrary>()
				.And<DataLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);

				var description = data.FindDynamicProperty("Description");

				var listMetaData = description.GetMetaData<CodeDescriptionPairList>(MetaDataType.ListDataSource);

				AssertNotNull("ListDataSource meta data has been set", listMetaData);
				AssertEquals(2, listMetaData.Count);
				AssertEquals("A", listMetaData[0].Code);
				AssertEquals("", listMetaData[0].Description);
				AssertEquals("B", listMetaData[1].Code);
				AssertEquals("", listMetaData[1].Description);
			}
		}

		#region Implementation

		class Dummy
		{
			public ZString Description { get; set; }
		}

		#endregion
	}
}