using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class COLSAdditionalDocumentMessageBuilder : COLSMessageBuilder<COLSAdditionalDocumentMessage>
	{
		public COLSAdditionalDocumentMessageBuilder(QuarantineColsHeader colsHeader, ZString additionalComment)
			: base(colsHeader)
		{
			this.additionalComment = additionalComment;
		}
		readonly ZString additionalComment;

		protected override ZString MessageType => AUCOLSMessageTypeList.Codes.AddAdditionalDocument;

		protected override BusinessObject MessageParent => colsHeader;

		protected override COLSAdditionalDocumentMessage GetMessageData()
		{
			var addAdditionalDocumentMessageData = new COLSAdditionalDocumentMessage()
			{
				additionalComment = additionalComment,
				generalDeclaration = "True"
			};
			return addAdditionalDocumentMessageData;
		}
	}
}
