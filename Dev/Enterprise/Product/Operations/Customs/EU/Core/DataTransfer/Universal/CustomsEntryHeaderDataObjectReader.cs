using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public class CustomsEntryHeaderDataObjectReader : Customs.DataTransfer.Universal.CustomsEntryHeaderDataObjectReader
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

		protected override Customs.DataTransfer.Universal.CustomsEntryNumberDataObjectReader<CusEntryHeader> CreateCustomsEntryNumberDataObjectReader(
			UniversalCustoms.EntryNumber entryNumberDataObject,
			IXmlImportLogger logger,
			Customs.DataTransfer.Universal.UniversalDataObjectReaderHelper helper,
			CusEntryHeader entryHeader)
		{
			var cusEntryHeader = (Business.Declaration.CusEntryHeader)entryHeader;
			return new CustomsEntryNumberDataObjectReader(entryNumberDataObject, logger, helper, cusEntryHeader);
		}

		protected override bool ShouldPopulatePaymentInformationData => true;
	}
}
