using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Common;
using Enterprise.DocumentEngine.DocumentSigning;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using PdfSharp.Pdf;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.IO;

namespace Enterprise.DocumentEngine.DigitalSignature
{
	public class PdfSignatureEditor
	{
		public PdfSignatureEditor(byte[] pdfContent)
		{
			PdfContent = pdfContent;
		}

		public ReadOnlyMemory<byte> PdfSHA256Hash
		{
			get
			{
				Parse();
				return PdfHashBytes;
			}
		}

		public bool OverwriteSignature(byte[] newSignature, SignerType? signerType = null)
		{
			Parse();
			if (SignatureSection != null)
			{
				if (signerType != null && signerType == SignerType.BytesToSignedSHA256Hash)
				{
					SHA256PKCS7Signable.Sign((SignerType)signerType, newSignature);
					newSignature = SHA256PKCS7Signable.ToByteArray();
				}

				var newSignatureHexBytes = Encoding.UTF8.GetBytes(BitConverter.ToString(newSignature).Replace("-", string.Empty));

				if (newSignatureHexBytes.Length > SignatureSection.Length)
				{
					var sizeInKB = newSignatureHexBytes.Length / 2048;
					var newSize = sizeInKB + 2;

					string error;
					if (newSize <= (DocumentsDataRegistry.Instance.DocumentSignaturePlaceholderSize.DataType as IntRegistryDataType).UpperBound)
					{
						error = $"The digital signature does not fit into placeholder. Current placeholder size [{DocumentsDataRegistry.Instance.DocumentSignaturePlaceholderSize.Value}], signature size [{sizeInKB}KB]. Placeholder size will automatically be adjusted to [{newSize}KB], please re-queue failed documents.";
						DocumentsDataRegistry.Instance.DocumentSignaturePlaceholderSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newSize);
					}
					else
					{
						error = $"The digital signature does not fit into placeholder. Current placeholder size [{DocumentsDataRegistry.Instance.DocumentSignaturePlaceholderSize.Value}], signature size [{sizeInKB}KB]";
					}

					throw new PlaceholderSizeExceededException(error);
				}

				var fillerHexBytes = Encoding.UTF8.GetBytes(new string('0', 1000));

				SignatureSection.Position = 0;
				SignatureSection.Write(newSignatureHexBytes, 0, newSignatureHexBytes.Length);
				while (SignatureSection.Position != SignatureSection.Length)
				{
					SignatureSection.Write(fillerHexBytes, 0, (int)Math.Min(fillerHexBytes.Length, SignatureSection.Length - SignatureSection.Position));
				}
				return true;
			}
			return false;
		}

		void Parse()
		{
			//sample:
			//Enterprise/Product/Documents/DocumentEngine/Imaging/WaterMarkOnExistingPfdAdder.cs
			if (!IsParsed)
			{
				try
				{
					using (var ms = new MemoryStream(PdfContent))
					{
						using (var pdfDoc = PdfReader.Open(ms))
						{
							//   /ByteRange[0 5956 38726 479]
							var byteRange = pdfDoc.Internals.GetAllObjects()
								.OfType<PdfDictionary>()
								.Where(x => x.Elements[PdfSignatureField.Keys.Type]?.ToString() == "/Sig")
								.Select(x => x.Elements[PdfSignatureField.Keys.ByteRange])
								.OfType<PdfArray>()
								.LastOrDefault()?
								.Elements.OfType<PdfInteger>()
								.Select(x => x.Value).ToArray();

							if (byteRange != null && byteRange.Length == 4)
							{
								using (var sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256))
								{
									sha256.AppendData(PdfContent, byteRange[0], byteRange[1]);
									sha256.AppendData(PdfContent, byteRange[2], byteRange[3]);
									PdfHashBytes = sha256.GetHashAndReset();
								}

								var signatureStartOffset = byteRange[0] + byteRange[1] + 1;
								var signatureEndOffset = byteRange[2] - 1;
								SignatureSection = new MemoryStream(PdfContent, signatureStartOffset, signatureEndOffset - signatureStartOffset);
							}
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					const int maxSize = 60 * 1024;
					string message = ex.Message;
					var bytes = PdfContent.Length > maxSize ? PdfContent.Take(maxSize) : PdfContent;
					message += (NoResString)" First 60 KB: [" + string.Join((NoResString)",", bytes) + (NoResString)"]";
					ErrorReporter.ReportOnce("ErrorPdfParsing", message);

					throw;
				}
				IsParsed = true;
			}
		}

		public byte[] GetPKCS7SignableHash(SignerType signerType, byte[] signingCertificate)
		{
			switch (signerType)
			{
				case SignerType.PdfSHA256HashToPKCS7:
				{
					return PdfSHA256Hash.ToArray();
				}
				case SignerType.BytesToSignedSHA256Hash:
				{
					if (SHA256PKCS7SignableContent == null && !PdfSHA256Hash.IsEmpty)
					{
						SHA256PKCS7Signable = new SignableSHA256PKCS7(PdfSHA256Hash.ToArray());
						SHA256PKCS7SignableContent = SHA256PKCS7Signable.GetSignableContent(signerType, signingCertificate);
					}
					return SHA256PKCS7SignableContent;
				}
				default:
				{
					throw new ArgumentOutOfRangeException(nameof(signerType), signerType, "Invalid Signer type");
				}
			}
		}

		public enum SignerType
		{
			PdfSHA256HashToPKCS7 = 0,
			BytesToSignedSHA256Hash = 1,
		}

		MemoryStream SignatureSection;
		readonly byte[] PdfContent;
		bool IsParsed;
		byte[] PdfHashBytes;
		byte[] SHA256PKCS7SignableContent;
		SignableSHA256PKCS7 SHA256PKCS7Signable;
	}
}
