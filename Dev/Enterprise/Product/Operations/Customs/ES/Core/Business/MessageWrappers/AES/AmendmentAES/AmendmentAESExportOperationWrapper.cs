using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class AmendmentAESExportOperationWrapper : DeclarationAESExportOperationWrapper, IAmendmentAESExportOperation
	{
		public AmendmentAESExportOperationWrapper(CusEntryHeader entryHeader, ZString securityCode) : base(entryHeader, securityCode)
		{
		}

		public ZString MRN => entryHeader.MovementReferenceNumber;
		protected override ZString DeclarationSubTypeCore => entryHeader.CH_EntryStatus == EntryStatusCodes.PreDeclarationAccepted ? entryHeader.GetMappedSubTypeForExportPreDeclaration() : entryInstruction.CEI_SubStyle;
	}
}
