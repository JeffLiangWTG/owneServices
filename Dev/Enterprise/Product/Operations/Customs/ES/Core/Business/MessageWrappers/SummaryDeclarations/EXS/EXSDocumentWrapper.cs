using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class EXSDocumentWrapper : DocumentCommonWrapper, IEXSDocument
	{
		public EXSDocumentWrapper(SupportingDocument doc) : base(GetCodeFromDocument(doc), doc.CSI_ReferenceNumber)
		{
			Argument.NotNull(doc, nameof(doc));
			LineNumber = ZString.Empty;
		}

		public EXSDocumentWrapper(PreviousDocument pre) : base(GetCodeFromDocument(pre), GetN337ReferenceNumberToSend(pre))
		{
			Argument.NotNull(pre, nameof(pre));
			LineNumber = pre.CSI_Code == PreviousDocumentHelper.PreviousDocumentCodeN337 && pre.CSI_LineNo > 0 ? pre.CSI_LineNo.ToString() : ZString.Empty;
		}

		public ZString LineNumber { get; }

		static ZString GetCodeFromDocument(CusSupportingInfo doc) => doc.CSI_SubType + doc.CSI_Code;

		static ZString GetN337ReferenceNumberToSend(PreviousDocument doc)
		{
			var referenceNumber = doc.CSI_ReferenceNumber;

			if (doc.CSI_Code != PreviousDocumentHelper.PreviousDocumentCodeN337 && doc.CSI_LineNo > 0)
			{
				referenceNumber += doc.CSI_LineNo.ToString().PadLeft(PreviousDocumentHelper.ImportExportReferenceLineNoLength, '0');
			}

			return referenceNumber;
		}
	}
}
