using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.EU.H7.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.IE.H7.Business.UniversalDataTransfer
{
	public class IEH7AsycudaForCustomsDeclarationDataObjectWriter : EUH7AsycudaForCustomsDeclarationDataObjectWriter
	{
		public IEH7AsycudaForCustomsDeclarationDataObjectWriter(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper) : base(manager, helper)
		{
		}

		protected override AsycudaBillEntryInstructionDataObjectWriter<EU.H7.Business.AsycudaBill> CreateNewAsycudaBillEntryInstructionDataObjectWriter() => new IEH7AsycudaBillEntryInstructionDataObjectWriter<EU.H7.Business.AsycudaBill>(writeManager, helper);
	}
}
