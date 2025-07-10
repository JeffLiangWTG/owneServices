using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.ES.Manifest.H7.Business.UniversalDataTransfer
{
	public class ESH7AsycudaBillEntryInstructionDataObjectWriter<TCountry> : AsycudaBillEntryInstructionDataObjectWriter<TCountry> where TCountry : ASYCUDA.Business.AsycudaBill
	{
		public ESH7AsycudaBillEntryInstructionDataObjectWriter(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper linkHelper) : base(manager, linkHelper)
		{
		}

		protected override EntryInstruction CreateEntryInstruction(TCountry countrySource)
		{
			var entryInstruction = base.CreateEntryInstruction(countrySource);
			entryInstruction.Style = IMPDeclarationTypeList.Codes.IM;
			entryInstruction.SubStyle = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = EntrySubStyleList.Codes.A, Description = EntrySubStyleList.Descriptions.A };

			return entryInstruction;
		}
	}
}
