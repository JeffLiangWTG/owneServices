using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using CusEntryInstruction = Enterprise.Customs.Business.CusEntryInstruction;

namespace Enterprise.Customs.CH.DataTransfer;

public class CustomsEntryInstructionDataObjectWriter : Customs.DataTransfer.Universal.CustomsEntryInstructionDataObjectWriter
{
	public CustomsEntryInstructionDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper) : base(manager, helper)
	{
	}

	protected override EntryInstruction PopulateDataObject(CusEntryInstruction sourceBO)
	{
		var dataObject = base.PopulateDataObject(sourceBO);
		var entryInstruction = (Business.CusEntryInstruction)sourceBO;
		PopulateAdditionalSupplyChainActors(entryInstruction, dataObject);
		return dataObject;
	}

	void PopulateAdditionalSupplyChainActors(Business.CusEntryInstruction entryInstruction, EntryInstruction dataObject)
	{
		var references = dataObject.CustomsReferenceCollection ?? new List<CustomsReference>();
		PopulateCustomsReference(references, entryInstruction.SupplyChainActors.Cast<CusSupplyChainActorReference>(), helper, writeManager);
		dataObject.SetCustomsReferenceCollection(() => references);
	}

	void PopulateCustomsReference(List<CustomsReference> customsReferenceCollection, IEnumerable<CusSupplyChainActorReference> cusSupplyChainActorReferences, UniversalCommonHelper helper, IDataWritingManager writeManager)
	{
		var addEmpty = true;
		foreach (var actor in cusSupplyChainActorReferences.OrderBy(x => x.CFR_SystemCreateTimeUtc).ThenBy(x => x.CFR_Code))
		{
			addEmpty = false;
			var reference = new CustomsReference()
			{
				Type = new CodeDescriptionPair() { Code = Customs.Business.CusReferenceTypeList.Codes.SupplyChainActor, Description = Customs.Business.CusReferenceTypeList.Descriptions.SupplyChainActor },
				Owner = helper.CreateOrganizationAddressFromAddress(writeManager, actor.Owner, AddressTypes.Owner),
				Reference = actor.CFR_Reference,
				SubType = ListHelper.GetWithDescription<CodeDescriptionPair35Char>(actor.CFR_Code, actor.Lookups.CodeList),
			};
			customsReferenceCollection.Add(reference);
		}
		if (addEmpty)
		{
			customsReferenceCollection.Add(new CustomsReference()
			{
				Type = new CodeDescriptionPair() { Code = Customs.Business.CusReferenceTypeList.Codes.SupplyChainActor, Description = Customs.Business.CusReferenceTypeList.Descriptions.SupplyChainActor }
			});
		}
	}
}
