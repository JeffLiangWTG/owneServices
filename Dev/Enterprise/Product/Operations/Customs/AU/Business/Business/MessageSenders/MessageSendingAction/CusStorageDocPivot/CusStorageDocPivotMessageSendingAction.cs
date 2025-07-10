using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class CusStorageDocPivotMessageSendingAction : BaseMessageSendingObject
	{
		public CusStorageDocPivotMessageSendingAction(CusStorageDocPivot docPivot)
		{
			Document = docPivot;
			ShouldSend = docPivot.CSD_MessageStatus.IsEmpty;
		}

		public CusStorageDocPivot Document { get; }

		[ResourceStringData("AF6384D7-67DB-49C5-898A-2F7AA950CDE6", Caption = "eDoc")]
		public ZString DocReference => Document.Document.FileName;

		[ResourceStringData("AB983A6A-BAAF-4F43-8742-B7B8D39DE0AE", Caption = "Document Type")]
		public ZString DocType => Document.CSD_DocType;

		[ResourceStringData("79AB7A5C-4622-4667-9924-EB505929693B", Caption = "Description")]
		public ZString Description => Document.CSD_Description;

		[ResourceStringData("D8413D3D-52EE-4896-A63F-4ECBC41ED3FF", Caption = "Status")]
		public ZString MessageStatus => Document.CSD_MessageStatus;

		[ResourceStringData("6E0D989D-4001-4B07-96C5-EC63FAA13199", Caption = "Status Description")]
		public ZString MessageStatusDescription => Document.MessageStatusDescription;
	}
}
