using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public class CusEntryInstructionLookups : Customs.Business.CusEntryInstructionLookups
	{
		public CusEntryInstructionLookups(CusEntryInstruction parent) : base(parent)
		{
		}

		protected new CusEntryInstruction Parent => base.Parent as CusEntryInstruction;

		protected new JobDeclaration JobDeclaration => Parent?.JobDeclaration;

		protected override ZString GetShipmentType()
		{
			ZString result;
			if (JobDeclaration?.WillGenerateBothEntries ?? false)
			{
				result = Parent.WillGenerateEnteringEntry ? Common.Shared.SharedJobMessageTypeList.Codes.Import : Common.Shared.SharedJobMessageTypeList.Codes.Export;
			}
			else
			{
				result = base.GetShipmentType();
			}
			return result;
		}
		public CodeDescriptionPairList LevyTypes => LevyTypeList.GetSupportedLevyTypeList(Factory, JobDeclaration?.IsImport ?? false);

		public ICodeDescriptionPairList CIQRelations => Factory.GetCachedValue<CIQRelation>();

		public PackageType PackageTypeList => Factory.GetCachedValue<PackageType>();

		public CodeDescriptionPairList EntryDocumentSubmissionType => Factory.GetCachedValue<EntryDocumentSubmissionTypes>();

		public CodeDescriptionPairList IntelligentDeclarationTypeList => Factory.GetCachedValue<IntelligentDeclarationTypeList>();

		public SubsetCusEntryInstructionCollection Parents
		{
			get
			{
				SubsetCusEntryInstructionCollection result = null;
				var declaration = JobDeclaration;
				var instruction = Parent;
				if (declaration != null)
				{
					if (instruction != null && !instruction.IsParent)
					{
						result = new SubsetCusEntryInstructionCollection(declaration, x => x.PK == instruction.CEI_CEI_Parent || x.PK != instruction.PK && !x.IsParent && !x.IsChild);
					}
					else
					{
						result = Factory.GetCachedValue("Customs.CN.Business.EmptySubsetCusEntryInstructionCollection", () => new SubsetCusEntryInstructionCollection(declaration, x => false));
					}
				}
				return result;
			}
		}
	}
}
