using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using Enterprise.DocumentEngine.DigitalSignature.DigitalSign;
using Enterprise.DocumentEngine.DigitalSignature.EMudhra.V1;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.FlexCelInterface.Testing;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentEngineCore.Registry.Testing;
using FlexCel.XlsAdapter;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using static Enterprise.DocumentEngineCore.Registry.DocumentSigningRegistryConstants;

namespace Enterprise.DocumentEngine.DigitalSignature.Testing
{
	sealed class PdfSignatureEditorTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBadFile()
		{
			//8 KB AutoFilter.xlsx
			// 51 KB PDFConversionCrasherWithHiddenSheet.xls
			// 100+ KB IndexOutOfRangeTest.xls
			var xls = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentEngine\FlexCelInterface\Testing\", "AutoFilter.xlsx");
			var bytes = File.ReadAllBytes(xls);

			AssertExceptionThrown(typeof(Exception), delegate
			{ var hash = new PdfSignatureEditor(bytes).PdfSHA256Hash; });
			AssertContains(string.Join(",", bytes), ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
			xls = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentEngine\FlexCelInterface\Testing\", "PDFConversionCrasherWithHiddenSheet.xls");
			bytes = File.ReadAllBytes(xls);
			AssertExceptionThrown(typeof(Exception), delegate
			{ var hash = new PdfSignatureEditor(bytes).PdfSHA256Hash; });
			AssertContains(string.Join(",", bytes), ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
			xls = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentEngine\FlexCelInterface\Testing\", "IndexOutOfRangeTest.xls");
			bytes = File.ReadAllBytes(xls);
			AssertExceptionThrown(typeof(Exception), delegate
			{ var hash = new PdfSignatureEditor(bytes).PdfSHA256Hash; });
			AssertContains(string.Join(",", bytes.Take(60 * 1024)), ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestOverwiteSignature_EMudhra()
		{
			var rego = DigitalSignatureTestHelper.GetDigitalSignatureRegistry();
			DocumentsDataRegistry.Instance.DigitalSignature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rego);

			var excelFile = new XlsFile(1, true);
			excelFile.SetCellValue(1, 1, @"excelFile1 - batch");

			byte[] pdf;
			using (var stream = new MemoryStream())
			using (var pdfExport = new FlexCelPdfExportSafe(excelFile))
			{
				var sig = PdfSignatureFactory.NewPdfSignature(PdfSigningOptionCodes.Placeholder);
				pdfExport.Sign(sig);
				pdfExport.BeginExport(stream);
				pdfExport.ExportSheet();
				pdfExport.EndExport();
				pdf = stream.ToArray();
			}

			var client = new EMudhraClientForTest();
			client.Response = new SignDocResp();
			var signer = new EMudhraBatchSigner(client);

			var pdfContents = new Dictionary<string, byte[]>();
			pdfContents.Add(string.Empty, pdf);
			signer.Sign(pdfContents).ToArray();

			var reason = $@"{Core.Constants.CompanyBrandingName} \(WTG\) has provided the feature to generate and sign this PDF document using the data verified by the application users and available in {Core.Constants.ProductName} at the time of signing. WTG or any of its employees shall not be held responsible for any discrepancies observed in the data contained in this document.";

			var actualSignature = ImageToPDFConverterTest.GetSignatureFromPDF(pdf);
			AssertEquals(reason, actualSignature.First(e => e.key == "Reason").value);

			new PdfSignatureEditor(pdf).OverwriteSignature(new byte[] { 1, 2, 3 });

			actualSignature = ImageToPDFConverterTest.GetSignatureFromPDF(pdf);
		}

		public void TestOverwiteSignature_DigitalSign()
		{
			var rego = DigitalSignatureTestHelper.GetDigitalSignatureRegistry();
			DocumentsDataRegistry.Instance.DigitalSignature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rego);

			var excelFile = new XlsFile(1, true);
			excelFile.SetCellValue(1, 1, @"excelFile1 - batch");

			byte[] pdf;
			using (var stream = new MemoryStream())
			using (var pdfExport = new FlexCelPdfExportSafe(excelFile))
			{
				var sig = PdfSignatureFactory.NewPdfSignature(PdfSigningOptionCodes.Placeholder);
				pdfExport.Sign(sig);
				pdfExport.BeginExport(stream);
				pdfExport.ExportSheet();
				pdfExport.EndExport();
				pdf = stream.ToArray();
			}

			var client = new DigitalSignClientForTest();
			var signer = new DigitalSignBatchSigner(client);

			var pdfContents = new Dictionary<string, byte[]>();
			pdfContents.Add("1", pdf);
			signer.Sign(pdfContents).ToArray();

			var reason = $@"{Core.Constants.CompanyBrandingName} \(WTG\) has provided the feature to generate and sign this PDF document using the data verified by the application users and available in {Core.Constants.ProductName} at the time of signing. WTG or any of its employees shall not be held responsible for any discrepancies observed in the data contained in this document.";

			var actualSignature = ImageToPDFConverterTest.GetSignatureFromPDF(pdf);
			AssertEquals(reason, actualSignature.First(e => e.key == "Reason").value);

			new PdfSignatureEditor(pdf).OverwriteSignature(new byte[] { 1, 2, 3 });

			actualSignature = ImageToPDFConverterTest.GetSignatureFromPDF(pdf);
		}

		public void TestOverwiteSignatureOversize_EMudhra()
		{
			AssertEquals(16, DocumentsDataRegistry.Instance.DocumentSignaturePlaceholderSize.Value);

			var rego = DigitalSignatureTestHelper.GetDigitalSignatureRegistry();
			DocumentsDataRegistry.Instance.DigitalSignature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rego);

			var excelFile = new XlsFile(1, true);
			excelFile.SetCellValue(1, 1, @"excelFile1 - batch");

			byte[] pdf;
			using (var stream = new MemoryStream())
			using (var pdfExport = new FlexCelPdfExportSafe(excelFile))
			{
				var sig = PdfSignatureFactory.NewPdfSignature(PdfSigningOptionCodes.Placeholder);
				pdfExport.Sign(sig);
				pdfExport.BeginExport(stream);
				pdfExport.ExportSheet();
				pdfExport.EndExport();
				pdf = stream.ToArray();
			}

			var client = new EMudhraClientForTest();
			var resp = new SignDocResp();
			resp.status = "1";
			var newSig = resp.DocSignatures.AddNew();
			newSig.id = "1";
			newSig.Value = Encoding.UTF8.GetBytes(new string('x', 50 * 1024));

			client.Response = resp;
			var signer = new EMudhraBatchSigner(client);

			var message = $"PLACEHOLDER_ERROR - The digital signature does not fit into placeholder. Current placeholder size [{DocumentsDataRegistry.Instance.DocumentSignaturePlaceholderSize.Value}], signature size [50KB]. Placeholder size will automatically be adjusted to [52KB], please re-queue failed documents.";

			var pdfContents = new Dictionary<string, byte[]>();
			pdfContents.Add(string.Empty, pdf);
			var res = signer.Sign(pdfContents).ToArray();

			AssertEquals(message, res[0].ErrorMessage);
			AssertEquals(52, DocumentsDataRegistry.Instance.DocumentSignaturePlaceholderSize.Value);
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestOverwiteSignatureTooOversize_EMudhra()
		{
			AssertEquals(16, DocumentsDataRegistry.Instance.DocumentSignaturePlaceholderSize.Value);

			var rego = DigitalSignatureTestHelper.GetDigitalSignatureRegistry();
			DocumentsDataRegistry.Instance.DigitalSignature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rego);

			var excelFile = new XlsFile(1, true);
			excelFile.SetCellValue(1, 1, @"excelFile1 - batch");

			byte[] pdf;
			using (var stream = new MemoryStream())
			using (var pdfExport = new FlexCelPdfExportSafe(excelFile))
			{
				var sig = PdfSignatureFactory.NewPdfSignature(PdfSigningOptionCodes.Placeholder);
				pdfExport.Sign(sig);
				pdfExport.BeginExport(stream);
				pdfExport.ExportSheet();
				pdfExport.EndExport();
				pdf = stream.ToArray();
			}

			var client = new EMudhraClientForTest();
			var resp = new SignDocResp();
			resp.status = "1";
			var newSig = resp.DocSignatures.AddNew();
			newSig.id = "1";
			newSig.Value = Encoding.UTF8.GetBytes(new string('x', 150 * 1024));

			client.Response = resp;
			var signer = new EMudhraBatchSigner(client);
			var message = $"The digital signature does not fit into placeholder. Current placeholder size [{DocumentsDataRegistry.Instance.DocumentSignaturePlaceholderSize.Value}], signature size [150KB]";

			var pdfContents = new Dictionary<string, byte[]>();
			pdfContents.Add(string.Empty, pdf);
			var res = signer.Sign(pdfContents).ToArray();

			AssertEquals("PLACEHOLDER_ERROR - " + message, res[0].ErrorMessage);
			AssertEquals(16, DocumentsDataRegistry.Instance.DocumentSignaturePlaceholderSize.Value);
		}

		public void TestOverwiteSignatureInvalid_DigitalSign()
		{
			AssertEquals(16, DocumentsDataRegistry.Instance.DocumentSignaturePlaceholderSize.Value);

			var rego = DigitalSignatureTestHelper.GetDigitalSignatureRegistry();
			DocumentsDataRegistry.Instance.DigitalSignature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rego);

			var excelFile = new XlsFile(1, true);
			excelFile.SetCellValue(1, 1, @"excelFile1 - batch");

			byte[] pdf;
			using (var stream = new MemoryStream())
			using (var pdfExport = new FlexCelPdfExportSafe(excelFile))
			{
				var sig = PdfSignatureFactory.NewPdfSignature(PdfSigningOptionCodes.Placeholder);
				pdfExport.Sign(sig);
				pdfExport.BeginExport(stream);
				pdfExport.ExportSheet();
				pdfExport.EndExport();
				pdf = stream.ToArray();
			}

			var client = new DigitalSignClientForTest();

			var resp = new JObject();
			var hashSig = Encoding.UTF8.GetBytes(new string('x', 512));
			var signedDocsInfo = new JObject
			{
				{ DigitalSignConstants.Response.SignedDocumentID, "docID" },
				{ DigitalSignConstants.Response.SignedDocumentAlias, "1" },
				{ DigitalSignConstants.Response.SignedDocumentSignatureHash, Convert.ToBase64String(hashSig) }
			};
			var signedDocs = new JArray();
			signedDocs.Add(signedDocsInfo);
			resp.Add(DigitalSignConstants.Response.SignedDocuments, signedDocs);
			client.Response = resp;

			var signer = new DigitalSignBatchSigner(client);

			var pdfContents = new Dictionary<string, byte[]>();
			pdfContents.Add("1", pdf);
			var res = signer.Sign(pdfContents).ToArray();

			AssertEquals("DGS-PLACEHOLDER_ERROR - The signature size is invalid and cannot be copied in the PKCS7.", res[0].ErrorMessage);
		}
	}
}
