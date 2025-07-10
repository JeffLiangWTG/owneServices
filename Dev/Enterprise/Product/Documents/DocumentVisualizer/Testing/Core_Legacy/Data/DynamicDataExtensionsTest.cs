using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DynamicDataExtensionsTest : DynamicDataTestCase
	{
		#region MakeDynamic

		public void TestMakeDynamic_BusinessObject()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			var dynamicData = dummy.MakeDynamic();

			AssertNotNull("Dynamic data has been created", dynamicData);
			AssertEquals("Dynamic data has been created", typeof(DynamicData), dynamicData.GetType());
		}

		public void TestMakeDynamic_BusinessObjectCollection()
		{
			var factory = new BusinessObjectFactory();
			var dummy = new DummyBusinessObjectCollection(factory);
			var dynamicData = dummy.MakeDynamic();

			AssertNotNull("Dynamic data has been created", dynamicData);
			AssertEquals("Dynamic data has been created", typeof(DynamicDataCollection), dynamicData.GetType());
		}

		#endregion

		#region TestIsValueType

		public void TestIsValueType()
		{
			var data = 123.MakeDynamic();
			AssertEquals("int is Value Type", true, data.IsValueType());

			data = ((byte)12).MakeDynamic();
			AssertEquals("byte is Value Type", true, data.IsValueType());

			data = 123f.MakeDynamic();
			AssertEquals("float is Value Type", true, data.IsValueType());

			data = ((ZString)"xxx").MakeDynamic();
			AssertEquals("ZString is Value Type", true, data.IsValueType());

			data = ((ZInt)12).MakeDynamic();
			AssertEquals("ZInt is Value Type", true, data.IsValueType());

			data = "xxx".MakeDynamic();
			AssertEquals("string is Value Type", true, data.IsValueType());

			data = (new Consol()).MakeDynamic();
			AssertEquals("object is not Value Type", false, data.IsValueType());
		}

		#endregion

		#region ToCodeDescriptionPairList

		public void TestTestToCodeDescriptionPairList_Null()
		{
			object[] untypedList = null;
			var result = untypedList.ToCodeDescriptionPairList();
			AssertNull("ToCodeDescriptionPairList on null returns null", result);
		}

		public void TestTestToCodeDescriptionPairList_UntypedList()
		{
			object[] untypedList = new object[]
				{
					new object[]
					{
						"1",
						"1 desc"
					},
					new object[]
					{
						"2",
						"2 desc"
					}
				};

			var result = untypedList.ToCodeDescriptionPairList();

			AssertNotNull("untyped list was converted to typed list", result);

			var listAsStrings = result
				.OfType<ICodeDescription>()
				.Select(cd => $"{cd.Code}|{cd.Description}");

			AssertContainsExactElementsInAnyOrder("",
				new[]
				{
					"1|1 desc",
					"2|2 desc"
				}, listAsStrings);
		}

		#endregion
	}
}