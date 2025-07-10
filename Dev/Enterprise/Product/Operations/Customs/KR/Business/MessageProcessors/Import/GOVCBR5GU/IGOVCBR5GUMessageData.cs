using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR5GUMessageData
	{
		ZString ImportDeclarationNumber { get; }
		ZDate CustomsRegistryDate { get; }
		ZDate CorrectionOrderDeadline { get; }
		ZString DeclarationOffice { get; }
		ZString CustomsPersonName { get; }
		ZString ComplementNumber { get; }
		ZString CustomsPersonPhoneNumber { get; }
	}

	public class GOVCBR5GUMessageData : NonPersistentBusinessObject, IGOVCBR5GUMessageData
	{
		public GOVCBR5GUMessageData(BusinessObjectFactory factory) : base(factory)
		{
		}
		public ZString ImportDeclarationNumber { get; set; }
		public ZDate CustomsRegistryDate { get; set; }
		public ZDate CorrectionOrderDeadline { get; set; }
		public ZString DeclarationOffice { get; set; }
		public ZString CustomsPersonName { get; set; }
		public ZString ComplementNumber { get; set; }
		public ZString CustomsPersonPhoneNumber { get; set; }
		public ZString AmendAcceptResult { get; set; }

		public GOVCBR5GULineMessageDataCollection Corrections => corrections ?? (corrections = new GOVCBR5GULineMessageDataCollection(Factory));
		GOVCBR5GULineMessageDataCollection corrections;

		public ZString CustomsOffice => MessageFunctions.GetCustomsOffice(Factory, DeclarationOffice.SubstringSafe(0, 3));
		public ZString CustomsDivision => MessageFunctions.GetCustomsDepartment(Factory, DeclarationOffice.SubstringSafe(3));
	}
}
