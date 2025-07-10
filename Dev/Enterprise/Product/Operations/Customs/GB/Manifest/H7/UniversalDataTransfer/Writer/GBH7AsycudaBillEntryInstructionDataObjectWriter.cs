using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.GB.H7.Business.UniversalDataTransfer
{
	public class GBH7AsycudaBillEntryInstructionDataObjectWriter<TCountry> : AsycudaBillEntryInstructionDataObjectWriter<TCountry> where TCountry : ASYCUDA.Business.AsycudaBill
	{
		public GBH7AsycudaBillEntryInstructionDataObjectWriter(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper linkHelper) : base(manager, linkHelper)
		{
		}

		protected override EntryInstruction CreateEntryInstruction(TCountry countrySource)
		{
			var entryInstruction = base.CreateEntryInstruction(countrySource);
			entryInstruction.Style = ImportDeclarationTypeList.Codes.DeclarationForReleaseForFreeCirculationOrEndUse;

			return entryInstruction;
		}
	}
}
