using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC060RequestedDocumentProvider
	{
		public CC060RequestedDocumentProvider(RequestedDocumentType requestedDocumentType)
		{
			this.requestedDocumentType = Argument.NotNull(requestedDocumentType, nameof(requestedDocumentType));
		}
		readonly RequestedDocumentType requestedDocumentType;

		public ZInt SequenceNumber => CachedValueHelper.GetValue(ref sequenceNumberCached, () => int.TryParse(requestedDocumentType.SequenceNumber, out var tryParseIntResult) ? tryParseIntResult : 0);
		CachedValue<int> sequenceNumberCached;

		public ZString DocumentType => requestedDocumentType.DocumentType;

		public ZString Description => requestedDocumentType.Description;
	}
}
