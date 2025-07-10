using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class CommonAnnexSendMessageWrapper : EntryHeaderCommonSendMessageWrapper, ICommonAnnexMessageDataProvider
	{
		public CommonAnnexSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData, CusStorageDocPivot docPivot, ZString requestDispatch) : base(cusEntryHeader, certificateData)
		{
			this.docPivot = Argument.NotNull(docPivot, nameof(docPivot));
			Argument.NotNullOrEmpty(requestDispatch, nameof(requestDispatch));
			DispatchRequest = requestDispatch == Customs.Business.YesNoList.Codes.Yes ? (ZString)YesCodeES : requestDispatch;
		}
		readonly CusStorageDocPivot docPivot;
		const string YesCodeES = "S";
		const string OperationCodeT2L = "04";
		const string OperationCodeT2C = "05";
		const string ReqDispatchTagName = "SolicitudDespacho";

		public ZString Operation => entryHeader.IsT2L ? OperationCodeT2L : entryHeader.IsT2C ? OperationCodeT2C : string.Empty;

		public ZString Reference => declaration.IsExport ? entryHeader.MovementReferenceNumber : entryHeader.T2CMovementReferenceNumber;

		public ZString RequestDispatchTagName => ReqDispatchTagName;

		public ZString DispatchRequest { get; }

		public IAnnexDocCommon Document => document ?? (document = new AnnexDocCommonWrapper(docPivot.Document, docPivot.CSD_Description));

		public ZString AdministrationCode => null;

		AnnexDocCommonWrapper document;
	}
}
