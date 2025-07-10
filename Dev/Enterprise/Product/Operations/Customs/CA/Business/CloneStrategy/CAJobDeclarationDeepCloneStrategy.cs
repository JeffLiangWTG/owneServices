using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class CAJobDeclarationDeepCloneStrategy : JobDeclarationDeepCloneStrategy
	{
		public CAJobDeclarationDeepCloneStrategy(BaseJobDeclaration declarationToClone, CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
			: base(declarationToClone, cloneType, alternativeFactoryToInstantiateCloneIn)
		{
		}

		protected new JobDeclaration DeclarationToClone
		{
			get { return (JobDeclaration)bizObjToClone; }
		}

		public override BusinessObject Clone()
		{
			var result = (JobDeclaration)base.Clone();
			DeclarationToClone.CleanUpNewDeclarationAfterClone(result, cloneType);
			return result;
		}

		protected override void DeepCopyNotesCore(BaseJobDeclaration clonedResult, StmNoteCollection notes)
		{
			var declaration = clonedResult as IStmNoteParentWithSystemNote;
			if (declaration != null)
			{
				foreach (var note in notes.Cast<StmNote>().Where(x => !declaration.IsSystemNote(x)))
				{
					clonedResult.Notes.Add(note.Clone());
				}
			}
		}

		protected override JobComInvoiceHeaderDeepCopyStrategy GetInvoiceDeepCopyStrategy(BaseJobComInvoiceHeader invoiceToClone, BaseJobDeclaration clonedDeclaration)
		{
			return new CAJobComInvoiceHeaderDeepCloneStrategy(invoiceToClone, cloneType, clonedDeclaration, pkPairsDictionaryCollection);
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			args.AddExcludedColumns(new[] { JobDeclarationSchema.Constants.JE_MessageSubType });
			var newDec = (JobDeclaration)base.CloneInternal(args);

			using (newDec.SuspendMarkApportionmentDirty())
			using (newDec.GetValidationSuspender())
			using (newDec.SuspendSettingHasChanges())
			{
				var subType = DeclarationToClone.JE_MessageSubType;
				newDec.JE_MessageSubType = subType;
			}
			return newDec;
		}
	}
}
