using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	sealed class StorageDocsCollectionHelperTest : TestCaseWithFactory
	{
		protected override BusinessObjectFactory NewFactory()
		{
			return new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
		}

		public void TestGetFromUniqueKey()
		{
			StorageDocs eDoc1 = Factory.New<StorageDocs>();
			StorageDocs eDoc2 = Factory.New<StorageDocs>();

			StorageDocs[] eDocs = { eDoc1, eDoc2 };

			AssertEquals(eDoc1.PK, StorageDocsCollectionHelper.GetFromUniqueKey(eDoc1.PK.ToGuid(), eDocs).UniqueKey);
			AssertEquals(eDoc2.PK, StorageDocsCollectionHelper.GetFromUniqueKey(eDoc2.PK.ToGuid(), eDocs).UniqueKey);
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2007, 1, 1, 00, 00, 00)]
		public void TestGetMostRecentEDoc()
		{
			var parent = Factory.New<DummyBusinessObject>();
			var documentFactory = (IDocumentFactory)Factory;
			var bytes = File.ReadAllBytes(Path.Combine(BaseSourcePath, TestDocsHelper.TestDocsPath, @"Squares_100dpi.tif"));
			var eDoc1 = (StorageDocs)documentFactory.AddFileOrDocument(parent.PK, null, bytes, null, "x", "DEF", true);
			documentFactory.Save();

			TestDateAttribute.Date = new DateTime(2007, 1, 2, 00, 00, 00);
			var eDoc2 = (StorageDocs)documentFactory.AddFileOrDocument(parent.PK, null, bytes, null, "x", "DEF", true);
			documentFactory.Save();

			TestDateAttribute.Date = new DateTime(2007, 1, 4, 00, 00, 00);
			var eDoc3 = (StorageDocs)documentFactory.AddFileOrDocument(parent.PK, null, bytes, null, "y", "DEF", true);
			documentFactory.Save();

			TestDateAttribute.Date = new DateTime(2007, 1, 3, 00, 00, 00);
			var eDoc4 = (StorageDocs)documentFactory.AddFileOrDocument(parent.PK, null, bytes, null, "y", "DEF", true);
			documentFactory.Save();
			Factory.Save();

			StorageDocs[] eDocs =
			{
				eDoc1,
				eDoc2,
				eDoc3,
				eDoc4
			};

			AssertEquals("GetMostRecentEDoc(\"x\", eDocs)", eDoc2, StorageDocsCollectionHelper.GetMostRecentEDoc("x", eDocs));
			AssertEquals("GetMostRecentEDoc(\"y\", eDocs)", eDoc3, StorageDocsCollectionHelper.GetMostRecentEDoc("y", eDocs));

			eDoc2.SC_IsDeleted = true;
			AssertEquals("GetMostRecentEDoc(\"x\", eDocs)", eDoc1, StorageDocsCollectionHelper.GetMostRecentEDoc("x", eDocs));
			AssertEquals("GetMostRecentEDoc(\"y\", eDocs)", eDoc3, StorageDocsCollectionHelper.GetMostRecentEDoc("y", eDocs));

			TestDateAttribute.Date = new ZDateTime(2007, 1, 5, 00, 00, 00).ToDateTime();
			eDoc4.SC_Desc = "TEST";
			documentFactory.Save();
			Factory.Save();
			AssertEquals("GetMostRecentEDoc(\"x\", eDocs)", eDoc1, StorageDocsCollectionHelper.GetMostRecentEDoc("x", eDocs));
			AssertEquals("GetMostRecentEDoc(\"y\", eDocs)", eDoc4, StorageDocsCollectionHelper.GetMostRecentEDoc("y", eDocs));
		}

		public void TestContainsEDocType()
		{
			var eDoc1 = Factory.New<StorageDocs>();
			eDoc1.SC_DocType = "x";

			var eDoc2 = Factory.New<StorageDocs>();
			eDoc2.SC_DocType = "y";
		
			StorageDocs[] eDocs =
			{
				eDoc1,
				eDoc2
			};

			AssertEquals(true, StorageDocsCollectionHelper.ContainsEDocType("x", eDocs));

			eDoc1.SC_IsDeleted = true;
			AssertEquals(false, StorageDocsCollectionHelper.ContainsEDocType("x", eDocs));
		}
	}
}
