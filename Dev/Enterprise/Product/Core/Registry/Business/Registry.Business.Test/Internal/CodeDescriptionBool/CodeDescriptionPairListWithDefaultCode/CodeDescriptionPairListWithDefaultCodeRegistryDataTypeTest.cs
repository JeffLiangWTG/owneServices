using System;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionPairListWithDefaultCodeRegistryDataType))]
	sealed class CodeDescriptionPairListWithDefaultCodeRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CodeDescriptionPairListWithDefaultCodeRegistryDataType>
	{
		[ExpectExceptionMessage(typeof(RegistryValidationException), "Please enter something in the list.")]
		public void TestCannotSaveEmptyList()
		{
			SystemDefinableCodeDescriptionBoolCollection collection = new SystemDefinableCodeDescriptionBoolCollection();
			DataType.Validate(null, collection, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		[ExpectExceptionMessage(typeof(RegistryValidationException), "Please select a default code.")]
		public void TestCannotSaveWithoutDefaultCode()
		{
			SystemDefinableCodeDescriptionBoolCollection collection = new SystemDefinableCodeDescriptionBoolCollection();
			SystemDefinableCodeDescriptionBool element = collection.AddNew();
			element.Code = "x";
			element.Description = (NoResString)"y";
			DataType.Validate(null, collection, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		public void TestDeserialise()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("1", "2");

			SystemDefinableCodeDescriptionBoolCollection collection = new SystemDefinableCodeDescriptionBoolCollection(1);
			SystemDefinableCodeDescriptionBool element = collection.AddNew();
			element.Code = "x";
			element.Description = (NoResString)"y";

			CodeDescriptionPairListWithDefaultCodeRegistryDataType dataType = new CodeDescriptionPairListWithDefaultCodeRegistryDataType(list, 1);
			SystemDefinableCodeDescriptionBoolCollection deserialisedValue = dataType.Deserialise(dataType.Serialise(collection));
			AssertEquals("Deserialise().Count", 2, deserialisedValue.Count);
			AssertEquals("Deserialise()[0].Code", "1", deserialisedValue[0].Code);
			AssertEquals("Deserialise()[0].SystemDefined", true, deserialisedValue[0].SystemDefined);
			AssertEquals("Deserialise()[0].CodeMaxLength", 1, deserialisedValue[0].CodeMaxLength);
			AssertEquals("Deserialise()[1].Code", "x", deserialisedValue[1].Code);
			AssertEquals("Deserialise()[1].SystemDefined", false, deserialisedValue[1].SystemDefined);
			AssertEquals("Deserialise()[1].CodeMaxLength", 1, deserialisedValue[1].CodeMaxLength);
		}

		protected override CodeDescriptionPairListWithDefaultCodeRegistryDataType GetNewDataType()
		{
			return new CodeDescriptionPairListWithDefaultCodeRegistryDataType(null, 0);
		}

		protected override bool HasEditor
		{
			get { return false; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			SystemDefinableCodeDescriptionBoolCollection collection = new SystemDefinableCodeDescriptionBoolCollection();
			SystemDefinableCodeDescriptionBool element1 = collection.AddNew();
			SystemDefinableCodeDescriptionBool element2 = collection.AddNew();

			element1.Code = "a";
			element1.Description = (NoResString)"b";
			element2.Code = "x";
			element2.Description = (NoResString)"y";
			element1.Bool = true;

			byte[] bytes =
			{
				255,254,60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,
				34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,0,45,0,49,0,54,0,34,0,63,
				0,62,0,60,0,83,0,121,0,115,0,116,0,101,0,109,0,68,0,101,0,102,0,105,0,110,0,97,0,98,0,108,0,101,0,67,0,111,
				0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,66,0,111,0,111,0,108,0,67,0,
				111,0,108,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,32,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,67,0,111,
				0,100,0,101,0,61,0,34,0,97,0,34,0,62,0,60,0,83,0,121,0,115,0,116,0,101,0,109,0,68,0,101,0,102,0,105,0,110,0,
				97,0,98,0,108,0,101,0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,
				0,66,0,111,0,111,0,108,0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,97,0,60,0,47,0,67,0,111,0,100,0,101,0,62,0,
				60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,98,0,60,0,47,0,68,0,101,0,115,0,
				99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,60,0,47,0,83,0,121,0,115,0,116,0,101,0,109,0,68,0,101,0,
				102,0,105,0,110,0,97,0,98,0,108,0,101,0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,
				0,105,0,111,0,110,0,66,0,111,0,111,0,108,0,62,0,60,0,83,0,121,0,115,0,116,0,101,0,109,0,68,0,101,0,102,0,
				105,0,110,0,97,0,98,0,108,0,101,0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,
				0,111,0,110,0,66,0,111,0,111,0,108,0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,120,0,60,0,47,0,67,0,111,0,100,0,
				101,0,62,0,60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,121,0,60,0,47,0,68,0,
				101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,60,0,47,0,83,0,121,0,115,0,116,0,101,0,109,
				0,68,0,101,0,102,0,105,0,110,0,97,0,98,0,108,0,101,0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,
				0,112,0,116,0,105,0,111,0,110,0,66,0,111,0,111,0,108,0,62,0,60,0,47,0,83,0,121,0,115,0,116,0,101,0,109,0,68,
				0,101,0,102,0,105,0,110,0,97,0,98,0,108,0,101,0,67,0,111,0,100,0,101,0,68,0,101,0,115,0,99,0,114,0,105,0,
				112,0,116,0,105,0,111,0,110,0,66,0,111,0,111,0,108,0,67,0,111,0,108,0,108,0,101,0,99,0,116,0,105,0,111,0,
				110,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB(collection, bytes) };
		}
	}
}
