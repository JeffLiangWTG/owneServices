using CargoWise.EntityFramework;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaBillCollection : ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>
	{
		public AsycudaBillCollection(AsycudaManifestHeader master) : base(master)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var asycudaBill = (AsycudaBill)child;
			asycudaBill.ABL_BolType = AsycudaBillKindList.Codes.HWB;
			var header = asycudaBill.Header;
			asycudaBill.ABL_RL_NKPortOfDischarge = header.AMA_RL_NKPortOfDischarge;

			if (header.IsRoad)
			{
				asycudaBill.TransportDocuments.EnsureTransportDocumentType(
					GetTransportDocumentType(header),
					header.AMA_ManifestNumber);
				return;
			}
			asycudaBill.TransportDocuments.EnsureTransportDocumentType(TransportDocsTypeList.Codes._704, header.AMA_MasterBill);
		}

		protected override bool AllowRemoveCore => !Master.ShouldSynchroniseWithConsol;

		protected override bool AllowNewCore => !Master.ShouldSynchroniseWithConsol;

		static string GetTransportDocumentType(AsycudaManifestHeader header)
			=> header.IsImport
			? TransportDocsTypeList.Codes.IL3
			: TransportDocsTypeList.Codes.ILF;
	}
}
