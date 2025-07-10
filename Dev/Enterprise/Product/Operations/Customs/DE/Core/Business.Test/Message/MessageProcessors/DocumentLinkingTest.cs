using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class DocumentLinkingTest : TestCaseWithFactory
	{
		public void TestSubscribe()
		{
			var regHeader = Factory.New<CusTempStorageRegHeader>();
			var nctsHeader = (BusinessObject)Factory.New<Integration.Customs.DE.ICusInBondHeader>();
			documentLinking.Subscribe(regHeader);
			documentLinking.Subscribe(regHeader);
			documentLinking.Subscribe(nctsHeader);
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { regHeader, nctsHeader }, documentLinking);
		}

		public void TestDoLink()
		{
			var regHeader = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var nctsHeader = (BusinessObject)Factory.New<Integration.Customs.DE.ICusInBondHeader>();

			var attachments = new List<AttachedDocument>
			{
				new AttachedDocument
				{
					FileName = "file1.pdf",
					Type = new DocumentType { Code = "AAA", Description = "AAA Desc" },
					ImageData = (SubStreamableStream)new MemoryStream(Convert.FromBase64String("XXX="))
				}
			};

			using (Factory.AddDisposableService())
			{
				documentLinking.Subscribe(regHeader);
				documentLinking.Subscribe(nctsHeader);
				documentLinking.DoLink(attachments);
				Factory.Save();
			}

			regHeader = new BusinessObjectFactory().Load<CusTempStorageRegHeader>(regHeader.PK);
			nctsHeader = (BusinessObject)new BusinessObjectFactory().Load<Integration.Customs.DE.ICusInBondHeader>(nctsHeader.PK);

			CombineAssertions(() =>
			{
				TestAttachments(regHeader);
				TestAttachments((IDocManagerSupport)nctsHeader);
			});
		}

		void TestAttachments(IDocManagerSupport docManagerSupport)
		{
			var testCase = docManagerSupport.GetType().FullName;
			AssertEquals($"{testCase}|AllEDocs.Count", 1, docManagerSupport.DocManagerInfo.AllEDocs.Count);
			var eDoc = docManagerSupport.DocManagerInfo.AllEDocs[0];
			AssertEquals($"{testCase}|FileName", "file1.pdf", eDoc.FileName);
			AssertEquals($"{testCase}|DocType", "AAA", eDoc.DocType);
			AssertEquals($"{testCase}|Description", "AAA Desc", eDoc.Description);
			AssertEquals($"{testCase}|ImageData", Convert.FromBase64String("XXX="), eDoc.GetImageDataReader().ConvertToByteArrayAndCloseStream());
		}

		protected override void SetUp()
		{
			base.SetUp();
			documentLinking = new DocumentLinking(new LoggingInformation());
		}
		DocumentLinking documentLinking;
	}
}
