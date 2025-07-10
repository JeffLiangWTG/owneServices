using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.IT.DataTransfer.Universal;

public class JobDeclarationDataObjectReader : EU.DataTransfer.Universal.JobDeclarationDataObjectReader
{
	public JobDeclarationDataObjectReader(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment forwardingShipment = null)
		: base(declarationDataObject, logger, factory, forwardingShipment)
	{
	}

	protected override Customs.DataTransfer.Universal.CustomsEntryInstructionDataObjectReader CreateCustomsEntryInstructionDataObjectReader(EntryInstruction entryInstructionDataObject, EU.Business.Declaration.JobDeclaration declaration)
	{
		return new CustomsEntryInstructionDataObjectReader(entryInstructionDataObject, logger, Helper, factory, declaration);
	}

	protected override Customs.DataTransfer.Universal.CustomsEntryHeaderDataObjectReader CreateCustomsEntryHeaderDataObjectReader(EntryHeader entryHeaderDataObject, EU.Business.Declaration.JobDeclaration declaration, List<ZString> matchingKeys = null)
	{
		return new CustomsEntryHeaderDataObjectReader(entryHeaderDataObject, logger, Helper, declaration, ZGuid.Empty, matchingKeys);
	}
}
