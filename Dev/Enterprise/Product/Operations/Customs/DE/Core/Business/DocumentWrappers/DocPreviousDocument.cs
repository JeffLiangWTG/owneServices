using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.DE.Business.DocumentWrappers
{
	public class DocPreviousDocument : DocBaseWrapper
	{
		DocPreviousDocument(PreviousDocument previousDocument, BusinessObjectFactory factoryToWrap)
			: base(previousDocument, factoryToWrap)
		{
		}

		public static DocPreviousDocument New(PreviousDocument previousDocument, BusinessObjectFactory factoryToWrap) => previousDocument == null ? null : new DocPreviousDocument(previousDocument, factoryToWrap);

		//Reg. Nr.
		public ZString RegistrationNumber => PreviousDocument.CSI_ReferenceNumber;

		//Pos. Nr.
		public ZInt LineNumber => PreviousDocument.CSI_LineNo;

		//Stück
		public ZString Quantity => PreviousDocument.CSI_Quantity.ToStringRounded(3);

		PreviousDocument PreviousDocument => previousDocument ??= (PreviousDocument)WrappedObject;
		PreviousDocument previousDocument;
	}
}
