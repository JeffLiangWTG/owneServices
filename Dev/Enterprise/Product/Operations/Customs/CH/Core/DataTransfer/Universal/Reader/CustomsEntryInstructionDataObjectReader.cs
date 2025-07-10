using Enterprise.Customs.CH.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using CusEntryInstruction = Enterprise.Customs.CH.Business.CusEntryInstruction;

namespace Enterprise.Customs.CH.DataTransfer;
public class CustomsEntryInstructionDataObjectReader : Customs.DataTransfer.Universal.CustomsEntryInstructionDataObjectReader
{
	public CustomsEntryInstructionDataObjectReader(EntryInstruction entryInstructionDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, UniversalObjectFactory factory, JobDeclaration declaration)
		: base(entryInstructionDataObject, logger, helper, factory, declaration)
	{
	}

	protected override void FillCountrySpecificDetails(Customs.Business.CusEntryInstruction targetBO)
	{
		base.FillCountrySpecificDetails(targetBO);
		if (targetBO is CusEntryInstruction entryInstruction && declaration is JobDeclaration)
		{
			new CusSupplyChainActorReferencesDataObjectReader(logger, factory).Read(entryInstruction.SupplyChainActors, CustomsReferenceGroupByType);
		}
	}
}

