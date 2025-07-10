using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.DocumentEngine.DigitalSignature.EMudhra.V1;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentEngineCore.Registry.Testing;
using FlexCel.XlsAdapter;
using NUnit.Framework;
using static Enterprise.DocumentEngineCore.Registry.DocumentSigningRegistryConstants;

namespace Enterprise.DocumentEngine.DigitalSignature.Testing
{
	sealed class EMudhraBatchSignerTest : TransactionedTestCase
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

			var client = new EMudhraClientForTest();
			client.SignedBytesSize = 5000;
			client.ResponseOverride = new SignDocResp() { status = "1", txn = "123" };
			var signer = new EMudhraBatchSigner(client);

			var pdfContents = new Dictionary<string, byte[]>();
			pdfContents.Add(string.Empty, pdfs.ToArray()[0]);

			var result = signer.Sign(pdfContents).ToArray();
			AssertEquals("", result[0].ErrorMessage);
		}

		public void TestApplyingSignature_OverSize()
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

			var client = new EMudhraClientForTest();
			client.SignedBytesSize = 1024 * 50;
			client.ResponseOverride = new SignDocResp() { status = "1", txn = "123" };
			var signer = new EMudhraBatchSigner(client);

			var pdfContents = new Dictionary<string, byte[]>();
			pdfContents.Add(string.Empty, pdfs.ToArray()[0]);

			var result = signer.Sign(pdfContents).ToArray();
			AssertEquals("PLACEHOLDER_ERROR - The digital signature does not fit into placeholder. Current placeholder size [16], signature size [50KB]. Placeholder size will automatically be adjusted to [52KB], please re-queue failed documents.", result[0].ErrorMessage);
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

			var signer = new EMudhraBatchSigner(new EMudhraClientForTest());
			var pdfContents = new Dictionary<string, byte[]>();
			pdfContents.Add("1", pdfs.ToArray()[0]);
			pdfContents.Add("2", pdfs.ToArray()[1]);

			var result = signer.Sign(pdfContents).ToArray();

			AssertEquals("x - error", result[0].ErrorMessage);
			AssertEquals("x - error", result[1].ErrorMessage);
			AssertEquals("123", result[0].TransactionId);
			AssertEquals("123", result[1].TransactionId);
		}

		public void TestThrowsIncorrectInput()
		{
			DocumentsDataRegistry.Instance.DigitalSignature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DigitalSignatureTestHelper.GetDigitalSignatureRegistry());

			var signer = new EMudhraBatchSigner(new EMudhraClientForTest());

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
			DocumentsDataRegistry.Instance.CloudSigningServiceProviderAPIEndpoint.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "antongorlin.com");
			DocumentsDataRegistry.Instance.DigitalSignature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DigitalSignatureTestHelper.GetDigitalSignatureRegistry());

			var signer = new EMudhraBatchSigner(new EMudhraClient());

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
			AssertEquals("Invalid Signature Placeholder", result[0].ErrorMessage);
			AssertEquals("Invalid Signature Placeholder", result[1].ErrorMessage);
			AssertEquals("", result[0].TransactionId);
			AssertEquals("", result[1].TransactionId);
		}

		public void TestSignatureDateForInvariantCulture()
		{
			var oldCulture = CultureInfo.CurrentCulture;
			CultureInfo.CurrentCulture = new CultureInfo("hi-IN");
			var date = new ZDateTime(2023, 01, 01);

			var result = PdfSignatureFactory.GetSignatureDate(date);

			try
			{
				AssertEquals("2023-01-01", result);
			}
			finally
			{
				CultureInfo.CurrentCulture = oldCulture;
			}
		}

		sealed class EMudhraClientForTest : EMudhraClient
		{
			public int SignedBytesSize;
			public SignDocResp ResponseOverride = new SignDocResp() { status = "0", errorCode = "x", errorMessage = "error", txn = "123" };

			public override SignDocResp Sign(IEnumerable<byte[]> fileHashes)
			{
				var resp = ResponseOverride;

				if (SignedBytesSize > 0)
				{
					var sig = resp.DocSignatures.AddNew();
					sig.Value = Encoding.UTF8.GetBytes(new string('0', SignedBytesSize));
					sig.id = "1";
				}

				return resp;
			}
		}
	}
}
