using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business
{
	public class DeclarationDmExtWrapper : IDeclarationDmExtensions
	{
		DeclarationDmExtWrapper(CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			this.jobDeclaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
			this.cusEntryInstruction = entryHeader.EntryInstruction;
		}
		readonly JobDeclaration jobDeclaration;
		readonly CusEntryHeader entryHeader;
		readonly CusEntryInstruction cusEntryInstruction;

		public static DeclarationDmExtWrapper NewOrNull(CusEntryHeader entryHeader) => entryHeader == null ? null : new DeclarationDmExtWrapper(entryHeader);

		public ICollection<IDeclarationDMExtAdditionalDocument> AdditionalDocument => null;

		public IIDType AgentFileReferenceID => IDTypeWrapper.NewOrNull(jobDeclaration.JE_DeclarationReference);

		public IIDType AutonomyRegionType => IDTypeWrapper.NewOrNull(cusEntryInstruction?.CEI_AutonomyRegionType ?? ZString.Empty);

		public IDeclarationDmExtensionsCustomsValueComponent CustomsValueComponent => null;

		public IExpenseLoadingFactorType ExpenseLoadingFactor => null;

		public IIDType ExternalDeclarationID => IDTypeWrapper.NewOrNull(entryHeader.CH_BGMReference);

		public IDeclarationDmExtPreviousDocument PreviousDocument => DeclarationDmExtPreviousDocumentWrapper.NewOrNull(null);

		public IReleaseDateType ReleaseDateTime => null;

		public string TaxationDateTime => cusEntryInstruction?.CEI_DateForDuty.ToString("yyyy-MM-ddTHH:mm:ss") ?? string.Empty;

		public IIDType TehilaDeclarationID => null;

		public IIDType VersionID => null;
	}
}
