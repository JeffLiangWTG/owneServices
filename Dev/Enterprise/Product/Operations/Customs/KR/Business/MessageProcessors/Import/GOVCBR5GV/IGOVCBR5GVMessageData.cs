using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR5GVMessageData
	{
		ZDate NoticeDate { get; }
		ZString ImportDeclarationNumber { get; }
		ZString ComplementReasonCode { get; }
		ZString ComplementReasonName { get; }
		ZDate ComplementByDate { get; }
		ZString CustomsManagerName { get; }
		ZString CustomsManagerPhoneNumber { get; }
		ZString ComplementDescription { get; }
	}

	public class GOVCBR5GVMessageData : NonPersistentBusinessObject, IGOVCBR5GVMessageData
	{
		public GOVCBR5GVMessageData(BusinessObjectFactory factory) : base(factory)
		{
		}
		public ZDate NoticeDate { get; set; }
		public ZString ImportDeclarationNumber { get; set; }
		public ZString ComplementReasonCode { get; set; }
		public ZString SupplementationType
		{
			get
			{
				return SupplementationCodeList.GetSupplementationType(ComplementReasonCode);
			}
		}
		public ZString SupplementaryCodeDescription
		{
			get
			{
				return Factory.GetCachedValue<SupplementationCodeList>().GetDescriptionFromCode(ComplementReasonCode);
			}
		}
		public ZString ComplementReasonName { get; set; }
		public ZDate ComplementByDate { get; set; }
		public ZString CustomsManagerName { get; set; }
		public ZString CustomsManagerPhoneNumber { get; set; }
		public ZString ComplementDescription { get; set; }

		public ZDate DocumentNoticeDate { get; set; }
		public ZString ComplementNumber { get; set; }
		public ZString DeclarationOffice { get; set; }
		public ZString PrimaryOfficialName { get; set; }
		public ZString DocumentName { get; set; }
		public ZString IssuingPartyName { get; set; }

		public GOVCBR5GVLineMessageDataCollection Lines => lines ?? (lines = new GOVCBR5GVLineMessageDataCollection(Factory));
		GOVCBR5GVLineMessageDataCollection lines;
	}
}
