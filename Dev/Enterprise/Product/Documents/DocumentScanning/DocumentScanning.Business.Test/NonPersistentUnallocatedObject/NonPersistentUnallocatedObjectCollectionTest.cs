using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(NonPersistentUnallocatedObjectCollection))]
	public class NonPersistentUnallocatedObjectCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestFilterWillIgnoreRowsNotInThisCountry()
		{
			StorageDocsUnallocated documentInAU = MasterFactory.New<StorageDocsUnallocated>();
			documentInAU.SM_Type = "DEC";
			documentInAU.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;

			StorageDocsUnallocated documentInSG = MasterFactory.New<StorageDocsUnallocated>();
			documentInSG.SM_Type = "CLS";
			documentInSG.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;

			StorageDocsUnallocated documentInBoth = MasterFactory.New<StorageDocsUnallocated>();
			documentInBoth.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			documentInBoth.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;

			MasterFactory.Save();

			GlbCompany.CurrentCompany.SetCountry("AU");
			NonPersistentUnallocatedObjectCollection collection = new NonPersistentUnallocatedObjectCollection(MasterFactory);
			collection.Load();
			AssertEquals("Collection should load absolutely nothing", 0, collection.Count);
		}

		#region Implementation

		readonly DocumentFactory MasterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());

		protected override BusinessObjectFactory NewFactory()
		{
			return MasterFactory;
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new NonPersistentUnallocatedObjectCollection(MasterFactory);
		}

		#endregion
	}
}
