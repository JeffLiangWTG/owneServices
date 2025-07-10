using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.EU.H7.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.ES.Manifest.H7.Business.UniversalDataTransfer
{
	public class ESH7AsycudaForCustomsDeclarationDataObjectWriter : EUH7AsycudaForCustomsDeclarationDataObjectWriter
	{
		public ESH7AsycudaForCustomsDeclarationDataObjectWriter(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper) : base(manager, helper)
		{
		}

		protected override AsycudaBillEntryInstructionDataObjectWriter<EU.H7.Business.AsycudaBill> CreateNewAsycudaBillEntryInstructionDataObjectWriter() => new ESH7AsycudaBillEntryInstructionDataObjectWriter<EU.H7.Business.AsycudaBill>(writeManager, helper);
	}
}
