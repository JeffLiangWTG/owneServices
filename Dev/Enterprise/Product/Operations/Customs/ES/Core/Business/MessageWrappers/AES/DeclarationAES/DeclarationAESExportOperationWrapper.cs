using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.EU.Business;
using EntrySubStyleList = Enterprise.Customs.ES.Business.Declaration.EntrySubStyleList;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DeclarationAESExportOperationWrapper : IDeclarationAESExportOperation
	{
		public DeclarationAESExportOperationWrapper(CusEntryHeader entryHeader, ZString securityCode)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
			entryInstruction = entryHeader.EntryInstruction;
			invoiceHeader = entryHeader.RandomHeader;

			SecurityFlag = securityCode;
		}

		public DeclarationAESExportOperationWrapper(CusEntryHeader entryHeader, ZString securityCode, ZString messageType, bool isComplementaryCWithMRN) : this(entryHeader, securityCode)
		{
			this.messageType = Argument.NotNullOrEmpty(messageType, nameof(messageType));
			this.isComplementaryCWithMRN = isComplementaryCWithMRN;
		}

		protected readonly CusEntryHeader entryHeader;
		readonly JobDeclaration declaration;
		protected readonly CusEntryInstruction entryInstruction;
		readonly JobComInvoiceHeader invoiceHeader;
		readonly ZString messageType;
		readonly bool isComplementaryCWithMRN;

		const string ComplYSuffix = "_Y";

		public ZString LRN => entryHeader.CH_BGMReference + (isComplementaryCWithMRN ? (ZString)ComplYSuffix : ZString.Empty);

		public ZString DeclarationType => declaration.JE_MessageSubType;

		public ZString DeclarationSubType => DeclarationSubTypeCore;

		protected virtual ZString DeclarationSubTypeCore => messageType == DeclarationMessageTypeList.Codes.ExportPreDeclaration
																	? entryHeader.GetMappedSubTypeForExportPreDeclaration()
																	: (isComplementaryCWithMRN
																			? (ZString)EntrySubStyleList.Codes.Y
																			: entryInstruction.CEI_SubStyle);

		public ZDateTime RecapitulationDate => declaration.ZG_LCPDepart;

		public ZBool RecapitulationDateSpecified => (entryInstruction.IsSubStyleYOrZ || isComplementaryCWithMRN) && !declaration.ZG_LCPDepart.IsEmpty;

		public ZString SecurityFlag { get; }

		public ZString SpecificCircumstance => declaration.ZG_SpecificCircumstanceIndicator == SpecificCircumstanceIndicatorForUCCList.Codes.A20 ? declaration.ZG_SpecificCircumstanceIndicator : ZString.Empty;

		public ZDecimal TotalAmount
		{
			get
			{
				if (totalAmount == null)
				{
					totalAmount = new CachedProperty<ZDecimal>(entryHeader.Factory, () =>
					{
						return AESWrappersHelper.GetTotalAmount(invoiceHeader, entryHeader);
					});
				}
				return totalAmount.Value;
			}
		}
		CachedProperty<ZDecimal> totalAmount;

		public ZString Currency => AESWrappersHelper.GetCurrency(invoiceHeader);
	}
}
