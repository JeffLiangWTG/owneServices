using Enterprise.DocumentEngineCore.Registry;
using FlexCel.Pdf;

namespace Enterprise.DocumentEngine.DigitalSignature
{
	class PlaceholderPdfSigner : TPdfSigner
	{
		public override int EstimateLength() => 1024 * DocumentsDataRegistry.Instance.DocumentSignaturePlaceholderSize.Value;

		public override byte[] GetSignature() => System.Array.Empty<byte>();

		public override void Write(byte[] buffer, int offset, int count) { }
	}
}
