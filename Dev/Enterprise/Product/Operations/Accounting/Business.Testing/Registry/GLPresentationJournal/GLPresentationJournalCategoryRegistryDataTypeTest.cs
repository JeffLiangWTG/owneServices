using System;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(GLPresentationJournalCategoryRegistryDataType))]
	class GLPresentationJournalCategoryRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<GLPresentationJournalCategoryRegistryDataType>
	{
		[ExpectExceptionMessage(typeof(RegistryValidationException), "Please enter something in the list.")]
		public void TestCannotSaveEmptyList()
		{
			var collection = new GLPresentationJournalCategoryCollection();
			DataType.Validate(null, collection, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		public void TestDeserialise()
		{
			var collection = new GLPresentationJournalCategoryCollection(1);
			var element = collection.AddNew();
			element.Code = "1";
			element.Description = (NoResString)"2";
			element.Bool = true;
			element = collection.AddNew();
			element.Code = "x";
			element.Description = (NoResString)"Desc x";
			element.Bool = true;
			element.Bool2 = true;
			element.Bool3 = false;
			element.Bool4 = false;

			var dataType = new GLPresentationJournalCategoryRegistryDataType(1);
			var deserialisedValue = dataType.Deserialise(dataType.Serialise(collection));
			AssertEquals("Deserialise().Count", 2, deserialisedValue.Count);
			AssertEquals("Deserialise()[0].Code", "1", deserialisedValue[0].Code);
			AssertEquals("Deserialise()[0].Description", "2", deserialisedValue[0].Description);
			AssertEquals("Deserialise()[0].SystemDefined", false, deserialisedValue[0].SystemDefined);
			AssertEquals("Deserialise()[0].CodeMaxLength", 1, deserialisedValue[0].CodeMaxLength);
			AssertEquals("Deserialise()[0].Bool", true, deserialisedValue[0].Bool);
			AssertEquals("Deserialise()[0].Bool2", false, deserialisedValue[0].Bool2);
			AssertEquals("Deserialise()[0].Bool3", false, deserialisedValue[0].Bool3);
			AssertEquals("Deserialise()[0].Bool4", false, deserialisedValue[0].Bool4);
			AssertEquals("Deserialise()[1].Code", "x", deserialisedValue[1].Code);
			AssertEquals("Deserialise()[1].Description", "Desc x", deserialisedValue[1].Description);
			AssertEquals("Deserialise()[1].SystemDefined", false, deserialisedValue[1].SystemDefined);
			AssertEquals("Deserialise()[1].CodeMaxLength", 1, deserialisedValue[1].CodeMaxLength);
			AssertEquals("Deserialise()[1].Bool", true, deserialisedValue[1].Bool);
			AssertEquals("Deserialise()[1].Bool2", true, deserialisedValue[1].Bool2);
			AssertEquals("Deserialise()[1].Bool3", false, deserialisedValue[1].Bool3);
			AssertEquals("Deserialise()[1].Bool4", false, deserialisedValue[1].Bool4);
		}

		protected override GLPresentationJournalCategoryRegistryDataType GetNewDataType()
		{
			return new GLPresentationJournalCategoryRegistryDataType(0);
		}

		protected override bool HasEditor
		{
			get { return true; }
		}

		protected override string ExpectedEditorName
		{
			get
			{
				return "GLPresentationJournalCategoryRegistryItemEditor";
			}
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new GLPresentationJournalCategoryCollection();
			var element1 = collection.AddNew();
			var element2 = collection.AddNew();

			element1.CodeMaxLength = 3;
			element1.Code = "a";
			element1.Description = (NoResString)"Desc a";
			element1.Bool = true;
			element1.Bool2 = true;
			element1.Bool3 = false;
			element1.Bool4 = false;
			element1.SystemDefined = false;

			element2.CodeMaxLength = 3;
			element2.Code = "x";
			element2.Description = (NoResString)"Desc x";
			element2.Bool2 = false;
			element2.Bool3 = false;
			element2.Bool4 = true;
			element2.SystemDefined = false;

			return new[] { new ValidSampleAndBinaryValueInDB(collection, new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,0,45,0,
				49,0,54,0,34,0,63,0,62,0,60,0,71,0,76,0,80,0,114,0,101,0,115,0,101,0,110,0,116,0,97,0,116,0,105,0,111,0,110,0,74,0,111,0,117,0,114,0,110,0,97,0,108,0,67,0,97,0,116,0,101,0,103,0,111,0,114,0,
				121,0,67,0,111,0,108,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,62,0,60,0,71,0,76,0,80,0,114,0,101,0,115,0,101,0,110,0,116,0,97,0,116,0,105,0,111,0,110,0,74,0,111,0,117,0,114,0,110,0,97,0,108,
				0,67,0,97,0,116,0,101,0,103,0,111,0,114,0,121,0,62,0,60,0,67,0,111,0,100,0,101,0,77,0,97,0,120,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,51,0,60,0,47,0,67,0,111,0,100,0,101,0,77,0,97,0,120,0,
				76,0,101,0,110,0,103,0,116,0,104,0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,97,0,60,0,47,0,67,0,111,0,100,0,101,0,62,0,60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,68,
				0,101,0,115,0,99,0,32,0,97,0,60,0,47,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,60,0,66,0,111,0,111,0,108,0,62,0,89,0,60,0,47,0,66,0,111,0,111,0,108,0,62,0,60,0,66,
				0,111,0,111,0,108,0,50,0,62,0,89,0,60,0,47,0,66,0,111,0,111,0,108,0,50,0,62,0,60,0,66,0,111,0,111,0,108,0,51,0,62,0,78,0,60,0,47,0,66,0,111,0,111,0,108,0,51,0,62,0,60,0,66,0,111,0,111,0,108,0,
				52,0,62,0,78,0,60,0,47,0,66,0,111,0,111,0,108,0,52,0,62,0,60,0,47,0,71,0,76,0,80,0,114,0,101,0,115,0,101,0,110,0,116,0,97,0,116,0,105,0,111,0,110,0,74,0,111,0,117,0,114,0,110,0,97,0,108,0,67,0,
				97,0,116,0,101,0,103,0,111,0,114,0,121,0,62,0,60,0,71,0,76,0,80,0,114,0,101,0,115,0,101,0,110,0,116,0,97,0,116,0,105,0,111,0,110,0,74,0,111,0,117,0,114,0,110,0,97,0,108,0,67,0,97,0,116,0,101,0,
				103,0,111,0,114,0,121,0,62,0,60,0,67,0,111,0,100,0,101,0,77,0,97,0,120,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,51,0,60,0,47,0,67,0,111,0,100,0,101,0,77,0,97,0,120,0,76,0,101,0,110,0,103,0,116,
				0,104,0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,120,0,60,0,47,0,67,0,111,0,100,0,101,0,62,0,60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,68,0,101,0,115,0,99,0,32,0,120,
				0,60,0,47,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,60,0,66,0,111,0,111,0,108,0,62,0,89,0,60,0,47,0,66,0,111,0,111,0,108,0,62,0,60,0,66,0,111,0,111,0,108,0,50,0,62,0,
				78,0,60,0,47,0,66,0,111,0,111,0,108,0,50,0,62,0,60,0,66,0,111,0,111,0,108,0,51,0,62,0,78,0,60,0,47,0,66,0,111,0,111,0,108,0,51,0,62,0,60,0,66,0,111,0,111,0,108,0,52,0,62,0,89,0,60,0,47,0,66,0,111,
				0,111,0,108,0,52,0,62,0,60,0,47,0,71,0,76,0,80,0,114,0,101,0,115,0,101,0,110,0,116,0,97,0,116,0,105,0,111,0,110,0,74,0,111,0,117,0,114,0,110,0,97,0,108,0,67,0,97,0,116,0,101,0,103,0,111,0,114,0,
				121,0,62,0,60,0,47,0,71,0,76,0,80,0,114,0,101,0,115,0,101,0,110,0,116,0,97,0,116,0,105,0,111,0,110,0,74,0,111,0,117,0,114,0,110,0,97,0,108,0,67,0,97,0,116,0,101,0,103,0,111,0,114,0,121,0,67,0,111,
				0,108,0,108,0,101,0,99,0,116,0,105,0,111,0,110,0,62,0 })
			};
		}
	}
}
