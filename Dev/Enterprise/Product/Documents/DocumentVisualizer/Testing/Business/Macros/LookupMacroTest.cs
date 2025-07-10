using System;
using CargoWise.Macros;
using CargoWise.Macros.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class LookupMacroTest : TestCaseWithMacros
	{
		public void TestCustomLookup()
		{
			var macro1 = "Description.SetLookup({A = \"A Description\", B = \"B Description\"})";
			var macro2 = "Description.SetLookup([[\"A\", \"A Description\"], [\"B\", \"B Description\"], [\"A\", \"Second description\"]])";

			var expectList = new CodeDescriptionPairList
			{
				new CodeDescriptionPair("A", "A Description"),
				new CodeDescriptionPair("B", "B Description")
			};

			AssertLookup(macro1, expectList);
			AssertLookup(macro2, expectList);
		}

		public void TestCustomLookup_CodeDescription_MacroMap() => AssertCustomLookup_CodeDescription("Code.SetLookup({A = \"A Description\", B = \"B Description\"})");
		public void TestCustomLookup_CodeDescription_Array() => AssertCustomLookup_CodeDescription("Code.SetLookup([[\"A\", \"A Description\"], [\"B\", \"B Description\"], [\"A\", \"Second description\"]])");

		public void AssertCustomLookup_CodeDescription(string macro)
		{
			var dummy = new CodeDescriptionForTest
			{
				Code = "A",
				Description = "A Description"
			};

			var data = dummy.MakeDynamic();

			var expr = macro
				.With<MetaDataLibrary>()
				.And<DataLibrary>()
				.CreateExpression();

			using (var scope = new MacroScope(data))
			{
				expr.Evaluate(scope);

				var codeProperty = data.GetDynamicProperty(nameof(dummy.Code));
				codeProperty.SetValue("B");

				var descriptionProperty = data.GetDynamicProperty(nameof(dummy.Description));

				AssertEquals("Description has been updated", "B Description", Convert.ToString(descriptionProperty.Value));
			}
		}

		public void TestUnlocoLookup()
		{
			AssertLookup("Unlocos", typeof(RefUNLOCOCollection));
		}

		public void TestWeightUnitsLookup()
		{
			AssertLookup("WeightUnits", typeof(CodeDescriptionPairList));
		}

		public void TestVolumeUnitsLookup()
		{
			AssertLookup("VolumeUnits", typeof(CodeDescriptionPairList));
		}

		public void TestCurrencyLookup()
		{
			AssertLookup("Currency", typeof(RefCurrencyCollection));
		}

		void AssertLookup(string lookupName, Type expectedListType)
		{
			var macro = string.Format("Description.SetLookup(Lookups.{0})", lookupName);

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

				var listMetaData = description.GetMetaData<object>(MetaDataType.ListDataSource);

				AssertNotNull("ListDataSource meta data has been set", listMetaData);

				AssertEquals(string.Format("Expected to have a list meta data of type {0}", expectedListType.Name),
					expectedListType,
					listMetaData.GetType());
			}
		}

		void AssertLookup(string macro, CodeDescriptionPairList expectList)
		{
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
				AssertEquals(expectList.Count, listMetaData.Count);

				for (int i = 0; i < expectList.Count; i++)
				{
					AssertEquals(expectList[i].Code, listMetaData[i].Code);
					AssertEquals(expectList[i].Description, listMetaData[i].Description);
				}
			}
		}

		#region Implementation

		sealed class Dummy
		{
			public ZString Description { get; set; }
		}

		sealed class CodeDescriptionForTest : ICodeDescription
		{
			public ZString Code { get; set; }
			public ZString Description { get; set; }
			public object Codes { get; }
		}

		#endregion
	}
}