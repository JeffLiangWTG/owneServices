using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType))]
	public class CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType>
	{
		[ExpectExceptionMessage(typeof(RegistryValidationException), "Please enter something in the list.")]
		public void TestCannotSaveEmptyList()
		{
			var collection = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection();
			DataType.Validate(null, collection, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		[ExpectExceptionMessage(typeof(RegistryValidationException), "Please select a default code.")]
		public void TestCannotSaveWithoutDefaultCode()
		{
			var collection = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection();
			var element = collection.AddNew();
			element.Code = "x";
			DataType.Validate(null, collection, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		public void TestDeserialise()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("1", "2");

			var collection = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection(1);
			var element = collection.AddNew();
			element.Code = "x";
			element.Bool2 = true;

			var dataType = new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType(list, true);
			var deserialisedValue = dataType.Deserialise(dataType.Serialise(collection));
			AssertEquals("Deserialise().Count", 2, deserialisedValue.Count);
			AssertEquals("Deserialise()[0].Code", "1", deserialisedValue[0].Code);
			AssertEquals("Deserialise()[0].SystemDefined", true, deserialisedValue[0].SystemDefined);
			AssertEquals("Deserialise()[0].CodeMaxLength", 1, deserialisedValue[0].CodeMaxLength);
			AssertEquals("Deserialise()[0].Bool2", false, deserialisedValue[0].Bool2);
			AssertEquals("Deserialise()[1].Code", "x", deserialisedValue[1].Code);
			AssertEquals("Deserialise()[1].SystemDefined", false, deserialisedValue[1].SystemDefined);
			AssertEquals("Deserialise()[1].CodeMaxLength", 1, deserialisedValue[1].CodeMaxLength);
			AssertEquals("Deserialise()[1].Bool2", true, deserialisedValue[1].Bool2);
		}

		public void TestDeserialiseKeepsAllowNew()
		{
			var pairList = new CodeDescriptionPairList
			{
				new CodeDescriptionPair("ABC", "ABC Description"),
				new CodeDescriptionPair("XYZ", "XYZ Description"),
			};

			CombineAssertions(() =>
			{
				Test(true);
				Test(false);
			});

			void Test(bool allowNew)
			{
				// Arrange
				var collection = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection(pairList.MaxCodeLength, pairList, false, allowNew);
				var dataType = new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType(pairList, allowNew);

				// Act
				var result = dataType.Deserialise(dataType.Serialise(collection)).AllowNew;

				// Assert
				AssertEquals(allowNew, result);
			}
		}

		public void TestAllowNew()
		{
			var pairList = new CodeDescriptionPairList
			{
				new CodeDescriptionPair("ABC", "ABC Description"),
				new CodeDescriptionPair("XYZ", "XYZ Description"),
			};

			CombineAssertions(() =>
			{
				Test(true);
				Test(false);
			});

			void Test(bool allowNew)
			{
				// Arrange
				var collection = new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType(pairList, allowNew);

				// Act
				var result = collection.AllowNew;

				// Assert
				AssertEquals(allowNew, result);
			}
		}

		public void TestDefaultValuesForSecondBool()
		{
			var pairList = new CodeDescriptionPairList();
			pairList.AddPair("CODE1", "Code 1");
			pairList.AddPair("CODE2", "Code 2");
			pairList.AddPair("CODE3", "Code 3");

			CombineAssertions(() =>
			{
				Test(Array.Empty<string>(), Array.Empty<string>());
				Test(new[] { "CODE1" }, new[] { "CODE1" });
				Test(new[] { "CODE3" }, new[] { "CODE3" });
				Test(new[] { "CODE1", "CODE3" }, new[] { "CODE1", "CODE3" });
				Test(new[] { "CODE3", "CODE2" }, new[] { "CODE3", "CODE2" });
				Test(new[] { "CODE1", "CODE2", "CODE3" }, new[] { "CODE1", "CODE2", "CODE3" });
				Test(new[] { "CODE4" }, Array.Empty<string>());
				Test(new[] { "CODE1", "CODE2", "CODE4" }, new[] { "CODE1", "CODE2" });
			});

			void Test(string[] codes, IEnumerable<string> expected)
			{
				// Arrange
				var registryDataType = new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType(pairList, true, codes);

				// Act
				var result = registryDataType.ExtraBoolCheckedCodes;

				// Assert
				AssertContainsExactElementsInAnyOrder(expected, result);
			}
		}

		protected override CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType GetNewDataType()
		{
			return new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType(null, true);
		}

		protected override bool HasEditor
		{
			get { return true; }
		}

		protected override string ExpectedEditorName
		{
			get
			{
				return "CodeDescriptionBoolWithExtraBoolRegistryItemEditor";
			}
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection();
			var element1 = collection.AddNew();
			var element2 = collection.AddNew();

			element1.CodeMaxLength = 3;
			element1.Code = "a";
			element1.Bool = true;
			element1.SystemDefined = false;

			element2.CodeMaxLength = 3;
			element2.Code = "x";
			element2.Bool2 = true;
			element2.SystemDefined = false;

			return new[] { new ValidSampleAndBinaryValueInDB(collection, new byte[]
{
255,254,60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,
0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,83,0,121,0,115,0,116,0,101,0,109,0,68,0,101,0,102,0,105,0,110,0,97,0,98,0,108,0,101,0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,
0,112,0,116,0,105,0,111,0,110,0,66,0,111,0,111,0,108,0,87,0,105,0,116,0,104,0,69,0,120,0,116,0,114,0,97,0,66,0,111,0,111,0,108,0,67,0,111,0,108,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,32,
0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,67,0,111,0,100,0,101,0,61,0,34,0,97,0,34,0,62,0,60,0,83,0,121,0,115,0,116,0,101,0,109,0,68,0,101,0,102,0,105,0,110,0,97,0,98,0,108,0,101,0,67,
0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,66,0,111,0,111,0,108,0,87,0,105,0,116,0,104,0,69,0,120,0,116,0,114,0,97,0,66,0,111,0,111,0,108,0,62,0,60,
0,67,0,111,0,100,0,101,0,77,0,97,0,120,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,51,0,60,0,47,0,67,0,111,0,100,0,101,0,77,0,97,0,120,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,60,0,67,
0,111,0,100,0,101,0,62,0,97,0,60,0,47,0,67,0,111,0,100,0,101,0,62,0,60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,32,0,47,0,62,0,60,0,66,0,111,0,111,0,108,0,62,
0,89,0,60,0,47,0,66,0,111,0,111,0,108,0,62,0,60,0,66,0,111,0,111,0,108,0,50,0,62,0,78,0,60,0,47,0,66,0,111,0,111,0,108,0,50,0,62,0,60,0,83,0,121,0,115,0,116,0,101,0,109,0,68,0,101,
0,102,0,105,0,110,0,101,0,100,0,62,0,70,0,97,0,108,0,115,0,101,0,60,0,47,0,83,0,121,0,115,0,116,0,101,0,109,0,68,0,101,0,102,0,105,0,110,0,101,0,100,0,62,0,60,0,47,0,83,0,121,0,115,0,116,
0,101,0,109,0,68,0,101,0,102,0,105,0,110,0,97,0,98,0,108,0,101,0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,66,0,111,0,111,0,108,0,87,0,105,0,116,
0,104,0,69,0,120,0,116,0,114,0,97,0,66,0,111,0,111,0,108,0,62,0,60,0,83,0,121,0,115,0,116,0,101,0,109,0,68,0,101,0,102,0,105,0,110,0,97,0,98,0,108,0,101,0,67,0,111,0,100,0,101,0,68,0,101,
0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,66,0,111,0,111,0,108,0,87,0,105,0,116,0,104,0,69,0,120,0,116,0,114,0,97,0,66,0,111,0,111,0,108,0,62,0,60,0,67,0,111,0,100,0,101,0,77,
0,97,0,120,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,51,0,60,0,47,0,67,0,111,0,100,0,101,0,77,0,97,0,120,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,120,
0,60,0,47,0,67,0,111,0,100,0,101,0,62,0,60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,32,0,47,0,62,0,60,0,66,0,111,0,111,0,108,0,62,0,78,0,60,0,47,0,66,0,111,
0,111,0,108,0,62,0,60,0,66,0,111,0,111,0,108,0,50,0,62,0,89,0,60,0,47,0,66,0,111,0,111,0,108,0,50,0,62,0,60,0,83,0,121,0,115,0,116,0,101,0,109,0,68,0,101,0,102,0,105,0,110,0,101,0,100,
0,62,0,70,0,97,0,108,0,115,0,101,0,60,0,47,0,83,0,121,0,115,0,116,0,101,0,109,0,68,0,101,0,102,0,105,0,110,0,101,0,100,0,62,0,60,0,47,0,83,0,121,0,115,0,116,0,101,0,109,0,68,0,101,0,102,
0,105,0,110,0,97,0,98,0,108,0,101,0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,66,0,111,0,111,0,108,0,87,0,105,0,116,0,104,0,69,0,120,0,116,0,114,
0,97,0,66,0,111,0,111,0,108,0,62,0,60,0,47,0,83,0,121,0,115,0,116,0,101,0,109,0,68,0,101,0,102,0,105,0,110,0,97,0,98,0,108,0,101,0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,
0,112,0,116,0,105,0,111,0,110,0,66,0,111,0,111,0,108,0,87,0,105,0,116,0,104,0,69,0,120,0,116,0,114,0,97,0,66,0,111,0,111,0,108,0,67,0,111,0,108,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,62,
0
}) };
		}
	}
}
