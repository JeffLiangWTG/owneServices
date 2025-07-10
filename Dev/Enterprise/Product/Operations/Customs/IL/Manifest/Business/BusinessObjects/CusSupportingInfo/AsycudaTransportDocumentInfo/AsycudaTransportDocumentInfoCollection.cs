using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IL;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public interface IAsycudaTransportDocumentInfoCollection : ICusSupportingInfoCollection<AsycudaTransportDocumentInfo>
	{
	}

	public class AsycudaTransportDocumentInfoCollection : CusSupportingInfoCollection<AsycudaTransportDocumentInfo>, IAsycudaTransportDocumentInfoCollection
	{
		public AsycudaTransportDocumentInfoCollection(BusinessObject parent) : base(parent, CusSupportingInfoTypeList.Codes.AdditionalInfo, AdditionalInfoSubTypeList.Codes.TransportDocument)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var asycudaAdditionalInfo = child as AsycudaTransportDocumentInfo;
			asycudaAdditionalInfo.CSI_SubType = CSI_SubType;
		}
	}
}
