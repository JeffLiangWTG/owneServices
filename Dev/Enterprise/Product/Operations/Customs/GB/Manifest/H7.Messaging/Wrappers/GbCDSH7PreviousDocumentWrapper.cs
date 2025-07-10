using CargoWise.Types;
using Enterprise.Customs.GB.H7.Business;
using IPreviousDocument = Enterprise.Customs.GB.CDS.Messaging.IPreviousDocument;

namespace Enterprise.Customs.GB.H7.Messaging
{
	public class GbCDSH7PreviousDocumentWrapper : IPreviousDocument
	{
		public GbCDSH7PreviousDocumentWrapper(PreviousDocument previousDocument)
		{
			this.previousDocument = previousDocument;
		}

		public ZString CategoryCode => previousDocument.CSI_SubType;

		public ZString TypeCode => previousDocument.CSI_Code;

		public ZString ID => previousDocument.CSI_ReferenceNumber;

		public ZInt LineNumeric => previousDocument.CSI_LineNo;

		public ZDateTime SystemCreateTime => previousDocument.CSI_SystemCreateTimeUtc;

		readonly PreviousDocument previousDocument;
	}
}
