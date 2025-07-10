using System.Collections.Generic;

namespace Enterprise.DocumentEngine.DigitalSignature
{
	public interface IPdfBatchSigner
	{
		int MaxBatchSize { get; }
		IEnumerable<SignResult> Sign(Dictionary<string, byte[]> pdfContents);
	}
}
