using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.IE.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.IE.H7.Business.UniversalDataTransfer
{
	public class IEH7AsycudaBillEntryInstructionDataObjectWriter<TCountry> : AsycudaBillEntryInstructionDataObjectWriter<TCountry> where TCountry : ASYCUDA.Business.AsycudaBill
	{
		public IEH7AsycudaBillEntryInstructionDataObjectWriter(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper linkHelper) : base(manager, linkHelper)
		{
		}

		protected override EntryInstruction CreateEntryInstruction(TCountry countrySource)
		{
			var entryInstruction = base.CreateEntryInstruction(countrySource);
			entryInstruction.Style = ImportDeclarationTypeList.Codes.H1;
			entryInstruction.SubStyle = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = countrySource.ABL_ShipmentType };

			return entryInstruction;
		}
	}
}
