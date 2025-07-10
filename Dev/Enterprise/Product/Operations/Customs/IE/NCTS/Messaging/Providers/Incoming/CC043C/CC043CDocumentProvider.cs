using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC043CDocumentProvider
	{
		public CC043CDocumentProvider(IIE043Document document)
		{
			documentType = Argument.NotNull(document, nameof(document));
		}
		readonly IIE043Document documentType;

		public ZShort SequenceNumber => CachedValueHelper.GetValue(ref sequenceNumberCached, () => ZShort.ParseSafe(documentType.SequenceNumber, ZShort.Zero));
		CachedValue<ZShort> sequenceNumberCached;

		public ZString Type => documentType.Type ?? ZString.Empty;

		public ZString ReferenceNumber => documentType.ReferenceNumber ?? ZString.Empty;
	}
}
