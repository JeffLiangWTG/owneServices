using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Duimp.Outgoing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business.Duimp
{
	public class DocumentProvider : IDocument
	{
		DocumentProvider(CusEntryInstruction entryInstruction)
		{
			this.entryInstruction = Argument.NotNull(entryInstruction, nameof(entryInstruction));
			declaration = Argument.NotNull(entryInstruction.JobDeclaration, nameof(entryInstruction.JobDeclaration));
		}

		public static DocumentProvider New(CusEntryInstruction entryInstruction) => entryInstruction?.JobDeclaration == null ? null : new DocumentProvider(entryInstruction);

		readonly CusEntryInstruction entryInstruction;
		readonly JobDeclaration declaration;

		public IEnumerable<IInstructionDocument> InstructionDocuments => fInstructionDocuments ??
			(fInstructionDocuments = declaration.DispatchInstructionNumbers.Cast<DispatchInstructionNumber>()
			.Where(x => x.CE_EntryType == DispatchInstructionDocumentTypes.Codes._01 || x.CE_EntryType == DispatchInstructionDocumentTypes.Codes._28)
			.Select(tariffDetach => InstructionDocumentProvider.New(tariffDetach)).ToArray());
		IInstructionDocument[] fInstructionDocuments;

		public IEnumerable<IProcess> Processes => fProcesses ??= declaration.ProcessRelatedNumbers.Cast<ProcessRelatedNumber>()
			.Select(x => ProcessProvider.New(x)).ToArray();
		IProcess[] fProcesses;

		public IEnumerable<IForeignExportDeclaration> ForeignExportDeclarations => fForeignExportDeclarations ??= entryInstruction.MercosulForeignDeclarations.Cast<MercosulForeignDeclaration>()
			.Select(x => ForeignExportDeclarationProvider.New(x)).ToArray();
		IForeignExportDeclaration[] fForeignExportDeclarations;
	}
}
