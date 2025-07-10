using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WebEDocsDownloadRegistryItem))]
	sealed class WebEDocsDownloadRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<WebEDocsDownloadEntryDictionary>
	{
		protected override StronglyTypedRegistryItem<WebEDocsDownloadEntryDictionary, WebEDocsDownloadEntryDictionary> GetNewRegistryItem()
		{
			var docTypeACV = Factory.LoadTop1<IRefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "ACV"));
			var docTypeMSC = Factory.LoadTop1<IRefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "MSC"));

			var collection = new RefDocTypeEntryCollection();
			collection.AddNew().RefDocTypePK = docTypeACV.PK;
			collection.AddNew().RefDocTypePK = docTypeMSC.PK;
			return new WebEDocsDownloadRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.Company, RegistryOptions.Default);
		}
	}
}
