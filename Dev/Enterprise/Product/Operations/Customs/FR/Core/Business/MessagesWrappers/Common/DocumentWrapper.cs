using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class DocumentWrapper : ISupportingDocument
	{
		public DocumentWrapper(EU.NCTS.Business.NctsSupportingDocument nctsSupportingDocument)
		{
			csi = Argument.NotNull(nctsSupportingDocument, nameof(nctsSupportingDocument));
		}

		public DocumentWrapper(PreviousDocument previousDocument)
		{
			csi = Argument.NotNull(previousDocument, nameof(previousDocument));
		}

		public DocumentWrapper(SupportingDocument supportingDocument)
		{
			csi = Argument.NotNull(supportingDocument, nameof(supportingDocument));
		}

		#region Members

		public ZString Code => csi.CSI_Code;

		public ZString Description => csi.CSI_Description;

		public ZString Type => csi.CSI_SubType;

		public ZString RefNumber => csi.CSI_ReferenceNumber;

		public ZDateTime DateIssue => csi.CSI_DateOfIssue;

		public ZString PFAIdentification => ZString.Empty;

		public ZString PFADocument => ZString.Empty;

		#endregion

		protected readonly Customs.Business.CusSupportingInfo csi;
	}
}
