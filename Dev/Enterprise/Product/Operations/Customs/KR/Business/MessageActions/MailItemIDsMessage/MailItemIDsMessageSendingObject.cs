using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class MailItemIDsMessageSendingObject : JobDeclarationMessageSendingObject
	{
		public MailItemIDsMessageSendingObject(CusEntryHeader entry)
			: base(entry, ElectronicDocumentTypeList.Codes._5SI)
		{
		}

		[ResourceStringData("4FFAAADD-3FB5-4BA4-9BC2-C2B1EFC5044C", Caption = "Customs Office")]
		public ZString DeclarationCustomsOffice => Header.Declaration.JE_CustomsOffice;

		[ResourceStringData("C2F6FA6D-40A7-418F-BDA5-E3D5C75BB1BD", Caption = "Department")]
		public ZString DeclarationCustomsDivision => Header.Declaration.JE_CustomsDivision;
	}
}
