using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ImportPreviousExpDecLineWrapper : NonPersistentBusinessObject
	{
		public ImportPreviousExpDecLineWrapper(ZInt importEntryLineNo, IImportPreviousExpDecLine importPreviousExpDecLine)
		{
			ImportEntryLineNo = importEntryLineNo;
			ImportPreviousExpDecLine = importPreviousExpDecLine;
		}

		public ZInt ImportEntryLineNo { get; }
		public ZString FormattedDeclarationNumber => MessageFunctions.DeclarationNumberFormat(ImportPreviousExpDecLine.DeclarationNumber);
		public IImportPreviousExpDecLine ImportPreviousExpDecLine { get; }
	}
}
