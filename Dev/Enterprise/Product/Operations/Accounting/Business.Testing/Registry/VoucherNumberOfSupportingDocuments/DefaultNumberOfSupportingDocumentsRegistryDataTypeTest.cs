using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(DefaultNumberOfSupportingDocumentsRegistryDataType))]
	class DefaultNumberOfSupportingDocumentsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DefaultNumberOfSupportingDocumentsRegistryDataType>
	{
		public void TestSerializeAndDeSerialize()
		{
			var dataType = new DefaultNumberOfSupportingDocumentsRegistryDataType();
			var collection = new DefaultNumberOfSupportingDocumentsCollection();

			var item = collection.AddNew();
			item.Description = (NoResString)"General Ledger";
			byte[] value = dataType.Serialise(collection);

			var deserializedValue = dataType.Deserialise(value);
			AssertEquals(collection.Count, deserializedValue.Count);

			for (int i = 0; i < collection.Count; i++)
			{
				AssertEquals(collection[i].Description, deserializedValue[i].Description);
			}
		}

		public void TestDescription()
		{
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				var registryItem = AccountingConfigurationRegistry.Instance.VoucherNumberOfSupportingDocumentDefaults;
				var collection = new DefaultNumberOfSupportingDocumentsCollection();
				var item = collection.AddNew();
				item.Description = (NoResString)"AP Payment";
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

				var key = ((ResourceString)registryItem.Value[0].Description).ResourceKey;
				mockChs.Put(key, new ResourceStringData(key, "应付账款付款"));
				AssertEquals("应付账款付款", registryItem.Value[0].Description.ToString(Core.SharedConstants.Languages.ChineseSimplified));
			}
		}

		protected override DefaultNumberOfSupportingDocumentsRegistryDataType GetNewDataType()
		{
			return new DefaultNumberOfSupportingDocumentsRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "DefaultNumberOfSupportingDocumentsRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			DefaultNumberOfSupportingDocumentsCollection items = new DefaultNumberOfSupportingDocumentsCollection();
			DefaultNumberOfSupportingDocuments item = items.AddNew();
			item.Code = "GLJNL";
			item.DefaultDescription = (NoResString)"Number of Documents for GL";
			item.Description = (NoResString)"Number of Documents for GL";
			item.NumberOfDefault = 1;

			byte[] byteArrayValue = new byte[]
			{
				255,254,60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,
				0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,78,0,117,0,109,0,98,0,101,0,114,0,79,0,102,0,83,0,117,0,112,
				0,112,0,111,0,114,0,116,0,105,0,110,0,103,0,68,0,111,0,99,0,117,0,109,0,101,0,110,0,116,0,115,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,
				0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,
				0,110,0,99,0,101,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,
				0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,62,0,60,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,78,0,117,0,109,0,98,0,101,0,114,0,79,0,102,
				0,83,0,117,0,112,0,112,0,111,0,114,0,116,0,105,0,110,0,103,0,68,0,111,0,99,0,117,0,109,0,101,0,110,0,116,0,115,0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,71,0,76,0,74,0,78,0,76,0,60,0,47,
				0,67,0,111,0,100,0,101,0,62,0,60,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,78,0,117,0,109,0,98,0,101,0,114,0,32,0,111,
				0,102,0,32,0,68,0,111,0,99,0,117,0,109,0,101,0,110,0,116,0,115,0,32,0,102,0,111,0,114,0,32,0,71,0,76,0,60,0,47,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,68,0,101,0,115,0,99,0,114,0,105,
				0,112,0,116,0,105,0,111,0,110,0,62,0,60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,78,0,117,0,109,0,98,0,101,0,114,0,32,0,111,0,102,0,32,0,68,0,111,0,99,0,117,
				0,109,0,101,0,110,0,116,0,115,0,32,0,102,0,111,0,114,0,32,0,71,0,76,0,60,0,47,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,60,0,78,0,117,0,109,0,98,0,101,0,114,
				0,79,0,102,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,62,0,49,0,60,0,47,0,78,0,117,0,109,0,98,0,101,0,114,0,79,0,102,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,62,0,60,0,47,0,68,0,101,
				0,102,0,97,0,117,0,108,0,116,0,78,0,117,0,109,0,98,0,101,0,114,0,79,0,102,0,83,0,117,0,112,0,112,0,111,0,114,0,116,0,105,0,110,0,103,0,68,0,111,0,99,0,117,0,109,0,101,0,110,0,116,0,115,0,62,
				0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,78,0,117,0,109,0,98,0,101,0,114,0,79,0,102,0,83,0,117,0,112,0,112,0,111,0,114,0,116,0,105,0,110,
				0,103,0,68,0,111,0,99,0,117,0,109,0,101,0,110,0,116,0,115,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(items, byteArrayValue)
			};
		}
	}
}
