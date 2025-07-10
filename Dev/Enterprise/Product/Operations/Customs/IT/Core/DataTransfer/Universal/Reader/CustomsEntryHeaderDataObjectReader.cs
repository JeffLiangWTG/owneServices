using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.IT.DataTransfer.Universal;

public class CustomsEntryHeaderDataObjectReader : EU.DataTransfer.Universal.CustomsEntryHeaderDataObjectReader
{
	public CustomsEntryHeaderDataObjectReader(
		UniversalCustoms.EntryHeader entryHeaderDataObject,
		IXmlImportLogger logger,
		Customs.DataTransfer.Universal.UniversalDataObjectReaderHelper helper,
		BaseJobDeclaration declaration,
		ZGuid primeEntryPK,
		List<ZString> matchingKeys = null) : base(entryHeaderDataObject, logger, helper, declaration, primeEntryPK, matchingKeys)
	{
	}

	protected override void FillCountrySpecificData(CusEntryHeader entryHeader)
	{
		var cusEntryHeader = (Business.Declaration.CusEntryHeader)entryHeader;
		var customsChannel = dataObject.AddInfoCollection == null ? ZString.Empty : dataObject.AddInfoCollection.GetZStringValue(IT.Business.Declaration.CusEntryHeader.Schema.CustomsChannel);
		cusEntryHeader.CustomsChannel = customsChannel ?? ZString.Empty;
	}

	protected override Customs.DataTransfer.Universal.CustomsEntryNumberDataObjectReader<CusEntryHeader> CreateCustomsEntryNumberDataObjectReader(
		UniversalCustoms.EntryNumber entryNumberDataObject,
		IXmlImportLogger logger,
		Customs.DataTransfer.Universal.UniversalDataObjectReaderHelper helper,
		CusEntryHeader entryHeader)
	{
		var cusEntryHeader = (Business.Declaration.CusEntryHeader)entryHeader;
		return new CustomsEntryNumberDataObjectReader(entryNumberDataObject, logger, helper, cusEntryHeader);
	}
}
