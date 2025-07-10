using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import5BFCancel : IImport5BFHeader
	{
		public string ImportDeclarationNumber { get; set; }
		public string ApplicationReason { get; set; }

		ZString IImport5BFHeader.ImportDeclarationNumber => ImportDeclarationNumber;
		ZString IImport5BFHeader.ApplicationReason => ApplicationReason;
	}
}
