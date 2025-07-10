using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class COLSEnquiryMessageBuilder : COLSMessageBuilder<COLSEnquiryMessage>
	{
		public COLSEnquiryMessageBuilder(QuarantineColsHeader colsHeader, COLSEnquiryAdditionalInformation additionalInformation, ZString additionalComment) : base(colsHeader)
		{
			this.additionalInformation = Argument.NotNull(additionalInformation, "additionalInformation");
			this.additionalComment = additionalComment;
		}
		readonly COLSEnquiryAdditionalInformation additionalInformation;
		readonly ZString additionalComment;

		protected override ZString MessageType => AUCOLSMessageTypeList.Codes.MakeAnEnquiry;

		protected override BusinessObject MessageParent => colsHeader;

		protected override COLSEnquiryMessage GetMessageData()
		{
			return new COLSEnquiryMessage
			{
				enquiryType = additionalInformation.EnquiryType,
				lodgementReferenceNumber = colsHeader.LRN,
				fullImportDeclarationNumber = colsHeader.IMPNumber,
				contactName = additionalInformation.ContactName,
				contactPhone = GetLocalFormattedPhoneNumber(additionalInformation.ContactPhone),
				contactEmail = additionalInformation.ContactEmail,
				additionalComments = additionalComment,
				generalDeclaration = "True",
				documentationRequired = additionalInformation.DocumentRequired
			};
		}
	}
}
