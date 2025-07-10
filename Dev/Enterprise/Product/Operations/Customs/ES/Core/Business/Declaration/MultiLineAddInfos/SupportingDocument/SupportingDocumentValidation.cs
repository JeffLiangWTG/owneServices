using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using static Enterprise.Customs.ES.Business.Declaration.ReadOnlySupportingDocumentCollection;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
namespace Enterprise.Customs.ES.Business.Declaration
{
	public class SupportingDocumentValidation : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentValidation
	{
		public SupportingDocumentValidation(SupportingDocument parent)
			: base(parent)
		{
		}

		protected new SupportingDocument Parent => (SupportingDocument)base.Parent;

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();

			ValidateSupportingDocumentNotPersistedForC44AndH1();
		}

		protected override void CheckCSI_Procedure()
		{
			base.CheckCSI_Procedure();
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_ProcedureInfo);
		}

		void ValidateSupportingDocumentNotPersistedForC44AndH1()
		{
			var document = Parent;

			if (!(document.Parent is JobDeclaration || document.Parent is CusEntryInstruction)) { return; }

			var entryHeaders = GetEntryHeadersFromDocument(document);
			if (entryHeaders.Any() && !entryHeaders.Any(entry => entry.GetPreviouslySentSupportingDocuments()
				.Any(entryDocs => entryDocs.Cast<EU.Business.Declaration.MultiLineAddInfos.ISupportingDocumentEqualityKey>()
					.Any(entryDoc => new AcceptedDocumentModificationAllButProcedureComparer().Equals(entryDoc, document)))))
			{
				document.AddRowWarning(Res.GetString("B1B9BFCA-E297-4FF8-96A7-A83954416AC4", "For C44 declaration, NEW Supporting Documents added to Entry Instruction or Misc. will not be sent."));
			}
		}

		IEnumerable<CusEntryHeader> GetEntryHeadersFromDocument(SupportingDocument document)
		{
			return document.Parent switch
			{
				JobDeclaration declaration => declaration.CustomsEntryHeaders.Where(IsEntryHeaderEntryInstructionABCStatusCDPAndUCC6),
				CusEntryInstruction entryInstruction => IsEntryHeaderEntryInstructionABCStatusCDPAndUCC6(entryInstruction.EntryHeader, entryInstruction)
					? new List<CusEntryHeader> { entryInstruction.EntryHeader }
					: Enumerable.Empty<CusEntryHeader>(),
				_ => null
			};
		}

		bool IsEntryHeaderEntryInstructionABCStatusCDPAndUCC6(CusEntryHeader entryHeader) => IsEntryHeaderEntryInstructionABCStatusCDPAndUCC6(entryHeader, entryHeader.EntryInstruction);

		bool IsEntryHeaderEntryInstructionABCStatusCDPAndUCC6(CusEntryHeader entryHeader, CusEntryInstruction entryInstruction) => entryHeader != null
				&& (entryInstruction?.IsSubStyleAOrBOrC ?? false)
				&& entryHeader.CH_EntryStatus == EntryStatusCodes.ClearedWithPendingDocuments
				&& entryHeader.IsUCC6;
	}
}
