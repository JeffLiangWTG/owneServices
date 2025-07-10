using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.DigitalSignature.EMudhra.V1
{
	class EMudhraBatchSigner : IPdfBatchSigner
	{
		public int MaxBatchSize { get; } = 10;

		public EMudhraBatchSigner(EMudhraClient client)
		{
			Client = client;
		}

		public IEnumerable<SignResult> Sign(Dictionary<string, byte[]> pdfContents)
		{
			if (pdfContents == null || !pdfContents.Any() || pdfContents.Count > MaxBatchSize)
			{
				throw new ArgumentOutOfRangeException();
			}

			var candidates = pdfContents.Values.Select(x => new Candidate() { Editor = new PdfSignatureEditor(x) }).ToArray();
			var candidateID = 1;

			candidates.ForEach(x =>
			{
				if (!x.Editor.PdfSHA256Hash.IsEmpty)
				{
					x.CandidateID = candidateID.ToString();
					candidateID++;
				}
				else
				{
					x.Message = (NoResString)"Invalid Signature Placeholder";
				}
			});

			var resp = Client.Sign(candidates.Where(x => !string.IsNullOrEmpty(x.CandidateID)).Select(x => x.Editor.PdfSHA256Hash.ToArray()));
			if (resp.status == "1")
			{
				foreach (SignDocRespDocSignature sig in resp.DocSignatures)
				{
					var candidate = candidates.FirstOrDefault(x => x.CandidateID == sig.id);
					if (candidate != null)
					{
						if (sig.Value?.Any() ?? false)
						{
							try
							{
								if (candidate.Editor.OverwriteSignature(sig.Value))
								{
									candidate.Message = "";
								}
								else
								{
									candidate.Message = (NoResString)"Failed - (ReplaceSignaturePlaceholder)";
								}
							}
							catch (PlaceholderSizeExceededException ex)
							{
								candidate.Message = $"{DocumentSigningConstants.PlaceholderSizeErrorPrefix} - {ex.Message}";
							}
						}
						else
						{
							candidate.Message = $"{sig.docErrorCode} - {sig.docErrorMessage}";
						}
					}
				}
			}
			else
			{
				candidates.ForEach(x =>
				{
					x.Message = x.Message ?? $"{resp.errorCode} - {resp.errorMessage}";
				});
			}

			candidates.ForEach(x =>
			{
				x.Message = x.Message ?? (NoResString)"Failed - (Missed by Signer)";
				x.TransactionId = resp.txn;
			});

			return candidates.Select(x => new SignResult(x.Message, x.TransactionId));
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
		readonly EMudhraClient Client;
	}
}
