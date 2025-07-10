using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBRR95MessageData
	{
		ZDate InspectionDate { get; }
		ZString ExportDeclarationNumber { get; }
		ZInt InspectionSequenceNo { get; }
		ZString DeclarantCompanyName { get; }
		ZString CustomsOfficeAndDivision { get; }
		ZString InspectionPersonName { get; }
		ZString CustomsPersonPhoneNumber { get; }
	}

	class GOVCBRR95MessageData : IGOVCBRR95MessageData
	{
		public ZDate InspectionDate { get; set; }
		public ZString ExportDeclarationNumber { get; set; }
		public ZInt InspectionSequenceNo { get; set; }
		public ZString DeclarantCompanyName { get; set; }
		public ZString CustomsOfficeAndDivision { get; set; }
		public ZString InspectionPersonName { get; set; }
		public ZString CustomsPersonPhoneNumber { get; set; }
	}
}
