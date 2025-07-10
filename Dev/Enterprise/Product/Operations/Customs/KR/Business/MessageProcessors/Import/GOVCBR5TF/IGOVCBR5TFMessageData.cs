using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR5TFMessageData
	{
		ZString ImportDeclarationNumber { get; }
		ZDateTime DeclarationDate { get; }
		ZDateTime EntryReleaseDate { get; }
		ZString DeclarantCompanyName { get; }
		ZString ImportCompanyName { get; }
	}

	class GOVCBR5TFMessageData : IGOVCBR5TFMessageData
	{
		public ZString ImportDeclarationNumber { get; set; }
		public ZDateTime DeclarationDate { get; set; }
		public ZDateTime EntryReleaseDate { get; set; }
		public ZString DeclarantCompanyName { get; set; }
		public ZString ImportCompanyName { get; set; }
	}
}
