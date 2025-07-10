using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	/// <summary>
	/// EU-specific cloning strategy, allowing one to do stuff to a declaration after, or indeed before, cloning it,
	/// and allows us to do stuff to the new declaration we've just made. 
	/// </summary>
	public class JobDeclarationDeepCloneStrategy : Customs.Business.JobDeclarationDeepCloneStrategy
	{
		public JobDeclarationDeepCloneStrategy(BaseJobDeclaration declarationToClone, CloneType cloneType)
			: this(declarationToClone, cloneType, declarationToClone.Factory)
		{
		}

		public JobDeclarationDeepCloneStrategy(BaseJobDeclaration declarationToClone, CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
			: base(declarationToClone, cloneType, alternativeFactoryToInstantiateCloneIn)
		{
		}

		protected override Customs.Business.JobComInvoiceHeaderDeepCopyStrategy GetInvoiceDeepCopyStrategy(BaseJobComInvoiceHeader invoiceToClone, BaseJobDeclaration clonedDeclaration)
		{
			return new JobComInvoiceHeaderDeepCopyStrategy((JobComInvoiceHeader)invoiceToClone, cloneType, (JobDeclaration)clonedDeclaration, pkPairsDictionaryCollection);
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clonedByBase = (JobDeclaration)base.CloneInternal(args);
			var oldDec = ((JobDeclaration)this.bizObjToClone);
			var newDec = clonedByBase;
			CloneDeclarationsEuChildren(oldDec, newDec, args); //  DocAddresses, AdditionalReferenceNumbers, DocsAndCartage
			return clonedByBase;
		}

		public override BusinessObject Clone()
		{
			var newDeclaration = (JobDeclaration)base.Clone();
			DeclarationToClone.CleanUpNewDeclarationAfterClone(newDeclaration, cloneType);
			return newDeclaration;
		}

		void CloneDeclarationsEuChildren(JobDeclaration oldDec, JobDeclaration newDec, BusinessObjectCloneArgs args)
		{
			// We are not cloning the real CusEntryNumbers, these are a series of additional references that happen to be of type CusEntryNumber.
			foreach (CusEntryNumber additionalRef in oldDec.AdditionalReferenceNumbers)
			{
				if (additionalRef.SupportsClone())
				{
					CusEntryNumber additionalRefCloned = (CusEntryNumber)additionalRef.Clone(args);
					newDec.AdditionalReferenceNumbers.Add(additionalRefCloned);
				}
			}

			newDec.DocsAndCartage.CopyPersistentValuesFrom(oldDec.DocsAndCartage);
			foreach (JobService service in oldDec.DocsAndCartage.Services)
			{   // Services are things line FUM-igation, etc.
				newDec.DocsAndCartage.Services.Add(service.Clone(args));
			}
			CopyGuarantees(oldDec, newDec, args);
			CopyCustomsOffices(oldDec, newDec, args);
			CopyDV1Details(oldDec, newDec, args);

			if (oldDec.IsUCC6AndIsExport || oldDec.IsUCC6AndIsImport)
			{
				newDec.EUD_AgreedPlaceCode = oldDec.EUD_AgreedPlaceCode;
				CopyGoodsLocation(newDec, oldDec, args);
			}
		}

		protected virtual void CopyGuarantees(JobDeclaration oldDec, JobDeclaration newDec, BusinessObjectCloneArgs args)
		{
			newDec.Guarantees.RemoveAndDeleteAll();

			foreach (GuaranteeForDeclaration guarantee in oldDec.Guarantees)
			{
				var newGuarantee = (GuaranteeForDeclaration)guarantee.Clone();
				newDec.Guarantees.Add(newGuarantee);
				if (!guarantee.EntryInstructionID.IsEmpty && pkPairsDictionaryCollection != null)
				{
					if (pkPairsDictionaryCollection.GetValueOrDefault(JobDeclarationDeepCloneStrategy.CusEntryInstructionPKPairsKey)?.TryGetValue(guarantee.EntryInstructionID, out var newEntryInstructionPK) ?? false)
					{
						newGuarantee.EntryInstructionID = newEntryInstructionPK;
					}
				}
			}
		}

		protected virtual void CopyCustomsOffices(JobDeclaration oldDec, JobDeclaration newDec, BusinessObjectCloneArgs args)
		{
			newDec.CustomsOffices.RemoveAndDeleteAll();

			foreach (var oldCustomOffice in oldDec.CustomsOffices)
			{
				var newCustomsOffice = oldCustomOffice.Clone(args);
				newDec.CustomsOffices.Add(newCustomsOffice);
			}
		}

		protected virtual void CopyDV1Details(JobDeclaration oldDec, JobDeclaration newDec, BusinessObjectCloneArgs args)
		{
			foreach (var oldDv1Detail in oldDec.DV1Details)
			{
				var newDv1Detail = oldDv1Detail.Clone(args);
				newDec.DV1Details.Add(newDv1Detail);
			}
			newDec.DV1Details.RecalculateAllLineNumbers();
		}

		protected override Customs.Business.CusEntryInstructionDeepCloneStrategy GetCusEntryInstructionDeepCloneStrategy(Customs.Business.CusEntryInstruction cusEntryInstructionToClone) => new CusEntryInstructionDeepCloneStrategy(cusEntryInstructionToClone, CloneType.DeepTemplateCopy, alternativeFactoryToInstantiateCloneIn);

		void CopyGoodsLocation(JobDeclaration newDec, JobDeclaration oldDec, BusinessObjectCloneArgs args)
		{
			newDec.GoodsLocation.CopyPersistentValuesFrom(oldDec.GoodsLocation, args);
			newDec.GoodsLocation.CGL_ParentID = newDec.PK;
			newDec.GoodsLocation.Address.CopyPersistentValuesFrom(oldDec.GoodsLocation.Address, args);
			newDec.GoodsLocation.Address.E2_ParentID = newDec.GoodsLocation.PK;
		}
	}
}
