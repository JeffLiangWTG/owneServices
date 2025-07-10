using CargoWise.Common;
using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business
{
	public class ImportDeclarationWrapper : IMessageImportDeclaration
	{
		public ImportDeclarationWrapper(CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		}

		public static ImportDeclarationWrapper NewOrNull(CusEntryHeader entryHeader) => entryHeader == null ? null : new ImportDeclarationWrapper(entryHeader);

		IDeclaration IMessageImportDeclaration.Declaration => DeclarationWrapper.NewOrNull(entryHeader);

		IRequestContentHeader IMessageImportDeclaration.RequestContentHeader => RequestContentHeaderWrapper.New();

		readonly CusEntryHeader entryHeader;
	}
}
