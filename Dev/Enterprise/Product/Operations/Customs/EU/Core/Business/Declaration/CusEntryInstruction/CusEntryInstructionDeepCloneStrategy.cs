using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusEntryInstructionDeepCloneStrategy : Customs.Business.CusEntryInstructionDeepCloneStrategy
	{
		public CusEntryInstructionDeepCloneStrategy(Customs.Business.CusEntryInstruction entryInstructionToClone, CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
			: base(entryInstructionToClone, cloneType, alternativeFactoryToInstantiateCloneIn)
		{
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clonedResult = (CusEntryInstruction)base.CloneInternal(args);
			var entryInstructionToClone = (CusEntryInstruction)bizObjToClone;
			CopyCusAuthorizationUsages(clonedResult, args);
			CopyCusFiscalReferences(clonedResult, args);
			CopyCusSupplyChainActorReferences(clonedResult, args);
			CopyGuarantees(clonedResult, args);
			CopyGoodsLocation(clonedResult, args);
			clonedResult.SupportingDocuments.AddCloneFrom(entryInstructionToClone.SupportingDocuments, args);
			clonedResult.AdditionalInfos.AddCloneFrom(entryInstructionToClone.AdditionalInfos, args);

			return clonedResult;
		}

		void CopyCusAuthorizationUsages(CusEntryInstruction clonedResult, BusinessObjectCloneArgs args)
		{
			CusEntryInstruction entryInstructionToClone = (CusEntryInstruction)bizObjToClone;

			foreach (CusAuthorizationUsage authorisation in entryInstructionToClone.CusAuthorizationUsages)
			{
				var newCusAuthorisation = (CusAuthorizationUsage)authorisation.Clone(args);
				clonedResult.CusAuthorizationUsages.Add(newCusAuthorisation);
				newCusAuthorisation.AGC_ParentID = clonedResult.PK;
				newCusAuthorisation.AGC_ParentTableCode = CusEntryInstructionSchema.Constants.Prefix;
			}
		}

		void CopyCusFiscalReferences(CusEntryInstruction clonedResult, BusinessObjectCloneArgs args)
		{
			CusEntryInstruction entryInstructionToClone = (CusEntryInstruction)bizObjToClone;
			foreach (CusFiscalReference fiscalReference in entryInstructionToClone.FiscalReferences)
			{
				var newFiscalReference = (CusFiscalReference)fiscalReference.Clone(args);
				clonedResult.FiscalReferences.Add(newFiscalReference);
				newFiscalReference.CFR_ParentID = clonedResult.PK;
				newFiscalReference.CFR_ParentTableCode = CusEntryInstructionSchema.Constants.Prefix;
			}
		}

		void CopyCusSupplyChainActorReferences(CusEntryInstruction clonedResult, BusinessObjectCloneArgs args)
		{
			CusEntryInstruction entryInstructionToClone = (CusEntryInstruction)bizObjToClone;

			foreach (CusSupplyChainActorReference supplyChainActorRef in entryInstructionToClone.CusSupplyChainActorReferences)
			{
				var newSupplyChainActorRef = (CusSupplyChainActorReference)supplyChainActorRef.Clone(args);
				clonedResult.CusSupplyChainActorReferences.Add(newSupplyChainActorRef);
				newSupplyChainActorRef.CFR_ParentID = clonedResult.PK;
				newSupplyChainActorRef.CFR_ParentTableCode = CusEntryInstructionSchema.Constants.Prefix;
			}
		}

		void CopyGuarantees(CusEntryInstruction clonedResult, BusinessObjectCloneArgs args)
		{
			CusEntryInstruction entryInstructionToClone = (CusEntryInstruction)bizObjToClone;

			foreach (GuaranteeForEntryInstruction guarantee in entryInstructionToClone.Guarantees)
			{
				var newGuarantee = (GuaranteeForEntryInstruction)guarantee.Clone(args);
				clonedResult.Guarantees.Add(newGuarantee);
				newGuarantee.PW_ParentID = clonedResult.PK;
				newGuarantee.PW_ParentTableCode = CusEntryInstructionSchema.Constants.Prefix;
			}
		}

		public override BusinessObject Clone()
		{
			var newEntryInstruction = (CusEntryInstruction)base.Clone();
			newEntryInstruction.ZG_SealsCount = ZInt.Zero;
			return newEntryInstruction;
		}

		void CopyGoodsLocation(CusEntryInstruction newEntryInstruction, BusinessObjectCloneArgs args)
		{
			var entryInstructionToClone = (CusEntryInstruction)bizObjToClone;
			var goodsLocation = entryInstructionToClone.GoodsLocation;
			if (goodsLocation != null)
			{
				var newGoodsLocation = (CusGoodsLocation)goodsLocation.Clone(args);
				var newAddress = (CusGoodsLocationAddress)goodsLocation.Address.Clone(args);
				newGoodsLocation.CGL_ParentID = newEntryInstruction.PK;
				newAddress.E2_ParentID = newGoodsLocation.PK;
				newAddress.E2_ParentTableCode = CusGoodsLocationSchema.Constants.Prefix;
				newAddress.E2_AddressType = DocAddressTypes.Codes.Location;
			}
		}
	}
}
