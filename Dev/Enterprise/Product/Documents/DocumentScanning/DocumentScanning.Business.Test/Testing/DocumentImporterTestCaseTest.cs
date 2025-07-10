using System;
using System.IO;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DocumentEngine.Imaging;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.Business.Test
{
	abstract class DocumentImporterTestCase : NonPersistentBusinessObjectTestCase
	{
		BusinessObject declarationB00001000;
		BusinessObject declarationB00001001;
		DocumentFactory masterFactory;
		BusinessObject shipmentS00001000;
		BusinessObject shipmentWithHouseBill;

		protected DocumentImporterTestCase()
		{
		}

		protected BusinessObject DeclarationB00001000
		{
			get { return declarationB00001000; }
		}

		protected BusinessObject DeclarationB00001001
		{
			get { return declarationB00001001; }
		}

		protected DocumentFactory MasterFactory
		{
			get
			{
				if (masterFactory == null)
				{
					masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				}
				return masterFactory;
			}
		}

		protected BusinessObject ShipmentS00001000
		{
			get { return shipmentS00001000; }
		}

		protected BusinessObject ShipmentWithHouseBill
		{
			get { return shipmentWithHouseBill; }
		}

		BusinessObject CreateIfDoesNotExist(Type type, SchemaColumn idColumn, ZString id)
		{
			type = type.IsInterface ? ObjectFactory.GetType(type) : type;

			BusinessObject result;
			var query = new ZQuery(idColumn, id);
			result = MasterFactory.LoadTop1(type, query);

			if (result == null)
			{
				result = MasterFactory.New(type);
				result[idColumn] = id;
			}

			return result;
		}

		protected abstract ScanningFinishedEventArgs GetImportResult(string outputOption, string sourceFileName);

		protected void SetupDeclarationObjects()
		{
			declarationB00001000 = CreateIfDoesNotExist(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration), JobDeclarationSchema.JE_DeclarationReference, "B00001000");
			declarationB00001001 = CreateIfDoesNotExist(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration), JobDeclarationSchema.JE_DeclarationReference, "B00001001");
			MasterFactory.Save();
		}

		protected void SetupShipmentObjects()
		{
			shipmentS00001000 = CreateIfDoesNotExist(typeof(Enterprise.Integration.Freight.ICommonShipment), JobShipmentSchema.JS_UniqueConsignRef, "S00001000");
			shipmentWithHouseBill = CreateIfDoesNotExist(typeof(Enterprise.Integration.Freight.ICommonShipment), JobShipmentSchema.JS_UniqueConsignRef, "S00001001");
			ShipmentWithHouseBill[JobShipmentSchema.JS_HouseBill] = "12345678901234567890";
			ShipmentWithHouseBill[JobShipmentSchema.JS_RL_NKOrigin] = "AUSYD";
			ShipmentWithHouseBill[JobShipmentSchema.JS_RL_NKDestination] = "USLAX";
			MasterFactory.Save();
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportWithBarcodeAsIncludedDocument()
		{
			SetupDeclarationObjects();
			var importResult = GetImportResult(Constants.Automatic, TestUtils.TestRepositoryPath + "MultipageTestDocument2.tif");

			try
			{
				AssertEquals("Number of Documents", 4, importResult.FileDetailsCount);

				var singleResult = (BaseBarcode)importResult.GetFileDetail(0);
				using (var pageReader = new ImageFileReaderWithLock(singleResult.FilePath))
				{
					AssertEquals("Number of Pages ", 2, pageReader.PageSelector.TotalPages);
					AssertEquals("DocManagerCode", Core.Constants.DocManagerCodes.JobDeclaration, singleResult.DocManagerCode);
					AssertEquals("RefCode", "B00001000", singleResult.RefCode);
					AssertEquals("DocType", "INS", singleResult.DocType);
					AssertEquals("RefPK", DeclarationB00001000.PK, singleResult.RefPK);
				}

				singleResult = (BaseBarcode)importResult.GetFileDetail(1);
				using (var pageReader = new ImageFileReaderWithLock(singleResult.FilePath))
				{
					AssertEquals("Number of Pages ", 2, pageReader.PageSelector.TotalPages);
					AssertEquals("DocManagerCode", Core.Constants.DocManagerCodes.JobDeclaration, singleResult.DocManagerCode);
					AssertEquals("RefCode", "B00001000", singleResult.RefCode);
					AssertEquals("DocType", "AGI", singleResult.DocType);
					AssertEquals("RefPK", DeclarationB00001000.PK, singleResult.RefPK);
				}

				singleResult = (BaseBarcode)importResult.GetFileDetail(2);
				using (var pageReader = new ImageFileReaderWithLock(singleResult.FilePath))
				{
					AssertEquals("Number of Pages ", 1, pageReader.PageSelector.TotalPages);
					AssertEquals("DocManagerCode", Core.Constants.DocManagerCodes.JobDeclaration, singleResult.DocManagerCode);
					AssertEquals("RefCode", "B00001001", singleResult.RefCode);
					AssertEquals("DocType", "AGI", singleResult.DocType);
					AssertEquals("RefPK", DeclarationB00001001.PK, singleResult.RefPK);
				}

				singleResult = (BaseBarcode)importResult.GetFileDetail(3);
				using (var pageReader = new ImageFileReaderWithLock(singleResult.FilePath))
				{
					AssertEquals("Number of Pages ", 1, pageReader.PageSelector.TotalPages);
					AssertEquals("DocManagerCode", Core.Constants.DocManagerCodes.JobDeclaration, singleResult.DocManagerCode);
					AssertEquals("RefCode", "B00001001", singleResult.RefCode);
					AssertEquals("DocType", "INS", singleResult.DocType);
					AssertEquals("RefPK", DeclarationB00001001.PK, singleResult.RefPK);
				}
			}
			finally
			{
				for (int i = 0; i < importResult.FileDetailsCount; i++)
				{
					var singleResult = importResult.GetFileDetail(i);
					if (File.Exists(singleResult.FilePath))
					{
						string dummy = "";
						Enterprise.ZArchitecture.Core.TempFile.TryDelete(singleResult.FilePath, out dummy, false);
					}
				}
			}
		}
	}
}
