using System.Data;
using System.IO;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[TestedType(typeof(InvoiceRequestHandler))]
	class InvoiceRequestHandlerTest : DataRequestHandlerTestCase<InvoiceRequestHelper>
	{
		public void TestGetBinaryData()
		{
			var bytes = RequestHandler.GetBinaryData();
			using (var zipStream = new MemoryStream(bytes))
			{
				AssertEquals(3, new ZipExtractor().GetZipFileInfos(zipStream).Length);
				AssertZipStream(bytes, "TAX INVOICE - AUS00335939 - JUSINTHKG (30-Jun-23).pdf", new byte[] { 1, 1, 1, 1 });
				AssertZipStream(bytes, "JU0PRD June 2023 Billing Summary.pdf", new byte[] { 2, 2, 2, 2 });
				AssertZipStream(bytes, "JU0PRD June 2023 Billing Summary.xlsx", new byte[] { 3, 3, 3, 3 });
			}
		}

		void AssertZipStream(byte[] zipBytes, string fileName, byte[] contentExpected)
		{
			using (var zipStream = new MemoryStream(zipBytes))
			using (var outputStream = new MemoryStream())
			{
				new ZipExtractor().ExtractZipStream(zipStream, outputStream, fileName);
				AssertSequencesEqual(contentExpected, outputStream.ToArray());
			}
		}

		public override void TestGetBinaryDataWithLock()
		{
			AssertNotNull("Nothing to lock in this class");
		}

		public void TestContentType()
		{
			AssertEquals(DataContentTypes.Zip, RequestHandler.ContentType);
		}

		public void TestFileName()
		{
			AssertEquals("TAX INVOICE - AUS00335939 - JUSINTHKG (30-Jun-23).zip", RequestHandler.FileName);
		}

		protected override DataRequestHandler<InvoiceRequestHelper> GetNewRequestHandler()
		{
			DummyInvoiceRequestHandler result = new DummyInvoiceRequestHandler();
			result.QueryString.Add(DataRequestHelper.DataKey, header.PK.ToString());
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<DummyAccTransactionHeader>();
			Factory.Save();
		}

		DummyAccTransactionHeader header;
		class DummyInvoiceRequestHandler : InvoiceRequestHandler
		{
			protected override BusinessObject[] GetNewBusinessObjects()
			{
				DummyAccTransactionHeader transaction = Factory.Load<DummyAccTransactionHeader>(PKs[0]);
				transaction.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 1, 1, 1 }, "TAX INVOICE - AUS00335939 - JUSINTHKG (30-Jun-23).pdf", "INV");
				transaction.DocManagerInfo.AddFileOrDocument(new byte[] { 2, 2, 2, 2 }, "JU0PRD June 2023 Billing Summary.pdf", StlBill.ParentSummaryDocType);
				transaction.DocManagerInfo.AddFileOrDocument(new byte[] { 3, 3, 3, 3 }, "JU0PRD June 2023 Billing Summary.xlsx", StlBill.ParentSummaryDocType);
				return new BusinessObject[] { transaction };
			}
		}

		class DummyAccTransactionHeader : AccTransactionHeader
		{
			public DummyAccTransactionHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override DocManagerInfo DocManagerInfo
			{
				get
				{
					return docManagerInfo ?? (docManagerInfo = base.DocManagerInfo);
				}
			}

			DocManagerInfo docManagerInfo;
		}
	}
}
