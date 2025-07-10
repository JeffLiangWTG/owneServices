using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public sealed class IM460RequestedDocumentsProvider : DocumentAdditionalInformationProvider
	{
		public IM460RequestedDocumentsProvider(MRequestedDocumentsType02 xmlObject) : base(xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly MRequestedDocumentsType02 xmlObject;

		public ZString SequenceNumber => xmlObject.SequenceNumber;

		public ZString ReferenceNumber => xmlObject.ReferenceNumber;
	}
}
