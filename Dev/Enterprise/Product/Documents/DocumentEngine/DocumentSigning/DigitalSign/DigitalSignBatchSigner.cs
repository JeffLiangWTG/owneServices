using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.ZArchitecture.Core;
using static Enterprise.DocumentEngine.DigitalSignature.PdfSignatureEditor;

namespace Enterprise.DocumentEngine.DigitalSignature.DigitalSign
{
	class DigitalSignBatchSigner : IPdfBatchSigner
	{
		public int MaxBatchSize { get; } = 10;

		public DigitalSignBatchSigner(DigitalSignClient client)
		{
			Client = client;
		}

		public IEnumerable<SignResult> Sign(Dictionary<string, byte[]> pdfContents)
		{
			if (pdfContents == null || !pdfContents.Any() || pdfContents.Count > MaxBatchSize)
			{
				throw new ArgumentOutOfRangeException();
			}

			var candidates = pdfContents.Select(x => new Candidate() { TransactionId = x.Key, Editor = new PdfSignatureEditor(x.Value) }).ToArray();
			var candidateID = 1;

			var finalizeSignRequestErrorMessage = string.Empty;

			if (!Client.InitializeCertificate(out var certResponse))
			{
				finalizeSignRequestErrorMessage = certResponse.GetValue(DigitalSignConstants.Response.ErrorCode).ToString()
					+ " - " + certResponse.GetValue(DigitalSignConstants.Response.ErrorMessage).ToString();
				return candidates.Select(x => new SignResult(DigitalSignConstants.DGS_InternalErrorPrefix + finalizeSignRequestErrorMessage, x.TransactionId));
			}

			candidates.ForEach(x =>
			{
				if (!x.Editor.PdfSHA256Hash.IsEmpty)
				{
					x.CandidateID = candidateID.ToString();
					candidateID++;
				}
				else
				{
					x.Message = DigitalSignConstants.DGS_InternalErrorPrefix + (NoResString)"Invalid Signature Placeholder";
				}
			});

			var fileHashes = new Dictionary<string, byte[]>();
			foreach (var candidate in candidates)
			{
				fileHashes.Add(candidate.TransactionId, candidate.Editor.GetPKCS7SignableHash(SignerType.BytesToSignedSHA256Hash, Client.CertificateContent));
			}

			var finalizeSignRequestResponse = Client.Sign(fileHashes);
			var signedDocuments = finalizeSignRequestResponse.SelectToken(DigitalSignConstants.Response.SignedDocuments);

			if (signedDocuments == null || !signedDocuments.Any())
			{
				finalizeSignRequestErrorMessage = DigitalSignConstants.DGS_InternalErrorPrefix
					+ finalizeSignRequestResponse.GetValue(DigitalSignConstants.Response.ErrorCode).ToString()
					+ " - " + finalizeSignRequestResponse.GetValue(DigitalSignConstants.Response.ErrorMessage).ToString();
			}
			else
			{
				foreach (var signedDocument in signedDocuments)
				{
					var signedDocumentAlias = signedDocument.SelectToken(DigitalSignConstants.Response.SignedDocumentAlias).ToString();
					var signature = signedDocument.SelectToken(DigitalSignConstants.Response.SignedDocumentSignatureHash).ToString();
					var signatureByte = Convert.FromBase64String(signature);
					var candidate = candidates.FirstOrDefault(x => x.TransactionId == signedDocumentAlias);

					try
					{
						if (candidate.Editor.OverwriteSignature(signatureByte, SignerType.BytesToSignedSHA256Hash))
						{
							candidate.Message = "";
						}
						else
						{
							candidate.Message = DigitalSignConstants.DGS_InternalErrorPrefix + (NoResString)"Failed - (ReplaceSignaturePlaceholder)";
						}
					}
					catch (InvalidSignatureSizeForPKCS7Exception ex)
					{
						candidate.Message = $"{DigitalSignConstants.DGS_InternalErrorPrefix}{DocumentSigningConstants.PlaceholderSizeErrorPrefix} - {ex.Message}";
					}
				}
			}

			return candidates.Select(x => new SignResult(x.Message ?? finalizeSignRequestErrorMessage, x.TransactionId));
		}

		class Candidate
		{
			public string CandidateID;
			public PdfSignatureEditor Editor;
			public string Message;
			public string TransactionId;
		}

#if DEBUG
		internal
#endif
		readonly DigitalSignClient Client;
	}
}
