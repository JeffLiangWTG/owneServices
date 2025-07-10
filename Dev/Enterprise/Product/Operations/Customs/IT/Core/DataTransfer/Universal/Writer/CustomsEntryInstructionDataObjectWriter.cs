using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.IT.DataTransfer.Universal;

public class CustomsEntryInstructionDataObjectWriter : EU.DataTransfer.Universal.CustomsEntryInstructionDataObjectWriter
{
	public CustomsEntryInstructionDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper) : base(manager, helper)
	{
	}

	protected override EntryInstruction PopulateDataObject(Customs.Business.CusEntryInstruction sourceBO)
	{
		var universalEntryInstruction = base.PopulateDataObject(sourceBO);
		if (sourceBO is CusEntryInstruction itEntryInstruction)
		{
			PopulateSealInfo(universalEntryInstruction, itEntryInstruction);
		}
		return universalEntryInstruction;
	}

	void PopulateSealInfo(EntryInstruction universalEntryInstruction, CusEntryInstruction itEntryInstruction)
	{
		RemoveSealsCountFromAddInfo(universalEntryInstruction);

		if (itEntryInstruction.JobDeclaration?.IsExport ?? false)
		{
			universalEntryInstruction.SealInfo = new SealInfo() { Quantity = itEntryInstruction.ZG_SealsCount };
			universalEntryInstruction.SetSealNumberCollection(() => GetAllSealsNumbers(itEntryInstruction));
		}
	}

	List<SealNumber> GetAllSealsNumbers(CusEntryInstruction itEntryInstruction)
	{
		return itEntryInstruction
			.Seals
			.OfType<EU.Business.SealNumber>()
			.Where(euSeal => !euSeal.CY_Data.IsEmpty)
			.Select(euSeal => new SealNumber() { Number = euSeal.CY_Data })
			.ToList();
	}

	void RemoveSealsCountFromAddInfo(EntryInstruction universalEntryInstruction)
	{
		var addInfoSealsCount = universalEntryInstruction.AddInfoCollection?.Find(x => x.Key.GetValueOrDefault() == EU.Business.Declaration.AutoCusEntryInstruction.Schema.ZG_SealsCount.Substring(3));
		if (addInfoSealsCount != null)
		{
			universalEntryInstruction.AddInfoCollection.Remove(addInfoSealsCount);
		}
	}
}
