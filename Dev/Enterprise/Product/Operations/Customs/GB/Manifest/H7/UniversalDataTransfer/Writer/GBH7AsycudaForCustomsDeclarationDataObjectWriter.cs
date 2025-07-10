using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.EU.H7.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.GB.H7.Business.UniversalDataTransfer
{
	public class GBH7AsycudaForCustomsDeclarationDataObjectWriter : EUH7AsycudaForCustomsDeclarationDataObjectWriter
	{
		public GBH7AsycudaForCustomsDeclarationDataObjectWriter(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper) : base(manager, helper)
		{
		}

		protected override AsycudaBillEntryInstructionDataObjectWriter<EU.H7.Business.AsycudaBill> CreateNewAsycudaBillEntryInstructionDataObjectWriter() => new GBH7AsycudaBillEntryInstructionDataObjectWriter<EU.H7.Business.AsycudaBill>(writeManager, helper);
	}
}
