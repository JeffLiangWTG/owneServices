using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WebEDocsDownloadRegistryDataType))]
	sealed class WebEDocsDownloadRegistryDataTypeTest : RegistryDataTypeTestCase<WebEDocsDownloadRegistryDataType>
	{
		public void TestDeserialised()
		{
			var factory = new BusinessObjectFactory();
			var docTypeACV = factory.LoadTop1<IRefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "ACV"));
			var docTypeMSC = factory.LoadTop1<IRefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "MSC"));

			var collection = new RefDocTypeEntryCollection();
			collection.AddNew().RefDocTypePK = docTypeACV.PK;
			collection.AddNew().RefDocTypePK = docTypeMSC.PK;

			var modules = new WebEDocsDownloadEntryDictionary();
			modules.Add("AllModules", new WebEDocsDownloadEntry(true, collection));

			var dataType = new WebEDocsDownloadRegistryDataType();
			var deserialisedCollection = dataType.Deserialise(dataType.Serialise(modules));

			AssertNotNull("Must contains a module", deserialisedCollection["AllModules"]);

			AssertContainsExactElementsInAnyOrder(
				new[] { docTypeACV.PK, docTypeMSC.PK },
				deserialisedCollection["AllModules"].DocTypeCollection.Cast<RefDocTypeEntry>().Select(entry => entry.RefDocTypePK));
		}

		#region Implementation

		protected override WebEDocsDownloadRegistryDataType GetNewDataType()
		{
			return new WebEDocsDownloadRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();
			var docTypeACV = factory.LoadTop1<IRefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "ACV"));
			var docTypeBOE = factory.LoadTop1<IRefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "BOE"));
			var docTypeMSC = factory.LoadTop1<IRefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "MSC"));

			var collection1 = new RefDocTypeEntryCollection();
			collection1.AddNew().RefDocTypePK = docTypeACV.PK;
			var modules1 = new WebEDocsDownloadEntryDictionary();
			modules1.Add("AllModules", new WebEDocsDownloadEntry(true, collection1));

			var collection2 = new RefDocTypeEntryCollection();
			collection2.AddNew().RefDocTypePK = docTypeBOE.PK;
			collection2.AddNew().RefDocTypePK = docTypeMSC.PK;
			var modules2 = new WebEDocsDownloadEntryDictionary();
			modules2.Add("Bookings", new WebEDocsDownloadEntry(true, collection2));

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(modules1, GetNewDataType().Serialise(modules1)),
				new ValidSampleAndBinaryValueInDB(modules2, GetNewDataType().Serialise(modules2))
			};
		}

		#endregion
	}
}
