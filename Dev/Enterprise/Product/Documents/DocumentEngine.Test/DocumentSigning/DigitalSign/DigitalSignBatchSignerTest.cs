using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Enterprise.DocumentEngine.DigitalSignature.DigitalSign;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentEngineCore.Registry.Testing;
using FlexCel.XlsAdapter;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using static Enterprise.DocumentEngineCore.Registry.DocumentSigningRegistryConstants;

namespace Enterprise.DocumentEngine.DigitalSignature.Testing
{
	sealed class DigitalSignBatchSignerTest : TransactionedTestCase
	{
		public void TestApplyingSignature_WithinSize()
		{
			DocumentsDataRegistry.Instance.DigitalSignature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DigitalSignatureTestHelper.GetDigitalSignatureRegistry());

			var excelFile = new XlsFile(1, true);
			excelFile.SetCellValue(1, 1, @"excelFile1 - batch");

			var pdfs = new List<byte[]>();
			using (var stream = new MemoryStream())
			using (var pdfExport = new FlexCelPdfExportSafe(excelFile))
			{
				var sig = PdfSignatureFactory.NewPdfSignature(PdfSigningOptionCodes.Placeholder);
				pdfExport.Sign(sig);
				pdfExport.BeginExport(stream);
				pdfExport.ExportSheet();
				pdfExport.EndExport();
				pdfs.Add(stream.ToArray());
			}

			var client = new DigitalSignClientForTest();
			client.SignedBytesSize = 256;

			var signer = new DigitalSignBatchSigner(client);

			var pdfContents = new Dictionary<string, byte[]>();
			pdfContents.Add("1", pdfs.ToArray()[0]);

			var result = signer.Sign(pdfContents).ToArray();
			AssertEquals("", result[0].ErrorMessage);
		}

		public void TestApplyingSignature_InvalidSize()
		{
			DocumentsDataRegistry.Instance.DigitalSignature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DigitalSignatureTestHelper.GetDigitalSignatureRegistry());

			var excelFile = new XlsFile(1, true);
			excelFile.SetCellValue(1, 1, @"excelFile1 - batch");

			var pdfs = new List<byte[]>();
			using (var stream = new MemoryStream())
			using (var pdfExport = new FlexCelPdfExportSafe(excelFile))
			{
				var sig = PdfSignatureFactory.NewPdfSignature(PdfSigningOptionCodes.Placeholder);
				pdfExport.Sign(sig);
				pdfExport.BeginExport(stream);
				pdfExport.ExportSheet();
				pdfExport.EndExport();
				pdfs.Add(stream.ToArray());
			}

			var client = new DigitalSignClientForTest();
			client.SignedBytesSize = 1024 * 50;
			var signer = new DigitalSignBatchSigner(client);

			var pdfContents = new Dictionary<string, byte[]>();
			pdfContents.Add("1", pdfs.ToArray()[0]);

			var result = signer.Sign(pdfContents).ToArray();
			AssertEquals("DGS-PLACEHOLDER_ERROR - The signature size is invalid and cannot be copied in the PKCS7.", result[0].ErrorMessage);
		}

		public void TestNoErrorIfPlaceholder()
		{
			DocumentsDataRegistry.Instance.DigitalSignature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DigitalSignatureTestHelper.GetDigitalSignatureRegistry());

			var excelFile1 = new XlsFile(1, true);
			excelFile1.SetCellValue(1, 1, @"excelFile1 - batch");
			var excelFile2 = new XlsFile(1, true);
			excelFile2.SetCellValue(1, 1, @"excelFile2 - batch");

			var pdfs = new List<byte[]>();
			foreach (var file in new[] { excelFile1, excelFile2 })
			{
				using (var stream = new MemoryStream())
				using (var pdfExport = new FlexCelPdfExportSafe(file))
				{
					var sig = PdfSignatureFactory.NewPdfSignature(PdfSigningOptionCodes.Placeholder);
					pdfExport.Sign(sig);
					pdfExport.BeginExport(stream);
					pdfExport.ExportSheet();
					pdfExport.EndExport();
					pdfs.Add(stream.ToArray());
				}
			}

			var signer = new DigitalSignBatchSigner(new DigitalSignClientForTest());
			var pdfContents = new Dictionary<string, byte[]>();
			pdfContents.Add("1", pdfs.ToArray()[0]);

			var result = signer.Sign(pdfContents).ToArray();

			AssertEquals("DGS-ErrorCode - ErrorMessage", result[0].ErrorMessage);
			AssertEquals("1", result[0].TransactionId);
		}

		public void TestThrowsIncorrectInput()
		{
			DocumentsDataRegistry.Instance.DigitalSignature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DigitalSignatureTestHelper.GetDigitalSignatureRegistry());

			var signer = new DigitalSignBatchSigner(new DigitalSignClientForTest());

			AssertExceptionThrown(typeof(ArgumentOutOfRangeException), delegate
			{ signer.Sign(null); });
			AssertExceptionThrown(typeof(ArgumentOutOfRangeException), delegate
			{ signer.Sign(new Dictionary<string, byte[]>()); });
			AssertExceptionThrown(typeof(ArgumentOutOfRangeException), delegate
			{ signer.Sign(null); });

			var dict = new Dictionary<string, byte[]>();
			for (var i = 0; i < 20; i++)
			{
				dict.Add(i.ToString(), new byte[] { (byte)i });
			}

			AssertExceptionThrown(typeof(ArgumentOutOfRangeException), delegate
			{ signer.Sign(dict); });
		}

		public void TestErrorIsNoPlaceholder()
		{
			DocumentsDataRegistry.Instance.DigitalSignature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DigitalSignatureTestHelper.GetDigitalSignatureRegistry());

			var signer = new DigitalSignBatchSigner(new DigitalSignClientForTest());

			var excelFile = new XlsFile(1, true);
			excelFile.SetCellValue(1, 1, @"excelFile1 - batch");

			byte[] pdf;
			using (var stream = new MemoryStream())
			using (var pdfExport = new FlexCelPdfExportSafe(excelFile))
			{
				pdfExport.BeginExport(stream);
				pdfExport.ExportSheet();
				pdfExport.EndExport();
				pdf = stream.ToArray();
			}

			var dict = new Dictionary<string, byte[]>();
			for (var i = 0; i < 2; i++)
			{
				dict.Add(i.ToString(), pdf);
			}

			var result = signer.Sign(dict).ToArray();
			AssertEquals("DGS-Invalid Signature Placeholder", result[0].ErrorMessage);
			AssertEquals("0", result[0].TransactionId);
		}

		sealed class DigitalSignClientForTest : DigitalSignClient
		{
			public int SignedBytesSize;
			public JObject DefaultErrorResponse;

			public DigitalSignClientForTest()
			{
				DefaultErrorResponse = CreateDefaultErrorResponse();
			}

			protected override byte[] TryGetCertificateContent(out JObject response)
			{
				response = new JObject();
				var encodedCert = DigitalSignClientTest.CreateTestCertificate();
				return encodedCert;
			}

			public override JObject Sign(Dictionary<string, byte[]> fileHashes)
			{
				var resp = new JObject();
				if (SignedBytesSize > 0)
				{
					var hashSig = Encoding.UTF8.GetBytes(new string('0', SignedBytesSize));

					var signedDocsInfo = new JObject
					{
						{ DigitalSignConstants.Response.SignedDocumentID, "docID" },
						{ DigitalSignConstants.Response.SignedDocumentAlias, "1" },
						{ DigitalSignConstants.Response.SignedDocumentSignatureHash, Convert.ToBase64String(hashSig) }
					};

					var signedDocs = new JArray();
					signedDocs.Add(signedDocsInfo);

					resp.Add(DigitalSignConstants.Response.SignedDocuments, signedDocs);

					return resp;
				}
				return DefaultErrorResponse;
			}

			JObject CreateDefaultErrorResponse()
			{
				var error = new JObject
				{
					{ DigitalSignConstants.Response.ErrorCode, "ErrorCode" },
					{ DigitalSignConstants.Response.ErrorMessage, "ErrorMessage" },
				};
				return error;
			}
		}
	}
}
