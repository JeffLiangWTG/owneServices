using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.DataTransfer.Universal;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.DataTransfer.Universal;

public class CustomsEntryInstructionDataObjectReader : EU.DataTransfer.Universal.CustomsEntryInstructionDataObjectReader
{
	public CustomsEntryInstructionDataObjectReader(EntryInstruction entryInstructionDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, UniversalObjectFactory factory, Customs.Business.BaseJobDeclaration declaration)
		: base(entryInstructionDataObject, logger, helper, factory, declaration)
	{
	}

	protected override void FillCountrySpecificDetails(Customs.Business.CusEntryInstruction targetBO)
	{
		base.FillCountrySpecificDetails(targetBO);
		if (targetBO is CusEntryInstruction itEntryInstruction)
		{
			PopulateSealsInfo(itEntryInstruction);
		}
	}

	void PopulateSealsInfo(CusEntryInstruction itEntryInstruction)
	{
		if (itEntryInstruction.JobDeclaration?.IsExport ?? false)
		{
			itEntryInstruction.ZG_SealsCount = dataObject.SealInfo?.Quantity ?? 0;
			foreach (var sealNumber in GetAllSealNumbers())
			{
				var sealBizObj = itEntryInstruction.Seals.AddNew();
				var sealColumnIndexer = GetColumnIndexer(sealBizObj);
				SetValue(sealColumnIndexer, CusCodeDataSchema.CY_Data, sealNumber);
			}
		}
	}

	ZString[] GetAllSealNumbers()
	{
		return dataObject.SealNumberCollection
			?.Where(sealNumber => sealNumber != null && sealNumber.Number.GetValueOrDefault() != ZString.Empty)
			.Select(sealNumber => sealNumber.Number.Value)
			.ToArray() ?? System.Array.Empty<ZString>();
	}
}
