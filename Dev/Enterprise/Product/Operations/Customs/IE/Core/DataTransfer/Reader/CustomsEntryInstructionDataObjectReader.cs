using CargoWise.Types;
using Enterprise.Customs.EU.DataTransfer.Universal;
using Enterprise.Customs.IE.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.IE.DataTransfer.Reader
{
	public class CustomsEntryInstructionDataObjectReader : EU.DataTransfer.Universal.CustomsEntryInstructionDataObjectReader
	{
		public CustomsEntryInstructionDataObjectReader(EntryInstruction entryInstructionDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, UniversalObjectFactory factory, Customs.Business.BaseJobDeclaration declaration) : base(entryInstructionDataObject, logger, helper, factory, declaration)
		{
		}

		protected override ZString? GetAdditionalIdentifier(LocationOfGoods goodsLocation) => base.GetAdditionalIdentifier(goodsLocation)?.Left(CusGoodsLocation.CGL_AdditionalIdentifierMaxLength);
	}
}
