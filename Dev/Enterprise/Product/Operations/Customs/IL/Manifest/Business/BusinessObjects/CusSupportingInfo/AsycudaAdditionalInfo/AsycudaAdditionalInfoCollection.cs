using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IL;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public interface IAsycudaAdditionalInfoCollection : ICusSupportingInfoCollection<AsycudaAdditionalInfo>
	{
	}

	public class AsycudaAdditionalInfoCollection : CusSupportingInfoCollection<AsycudaAdditionalInfo>, IAsycudaAdditionalInfoCollection
	{
		public AsycudaAdditionalInfoCollection(BusinessObject parent) : base(parent, CusSupportingInfoTypeList.Codes.AdditionalInfo, AdditionalInfoSubTypeList.Codes.AdditionalInformation)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var asycudaAdditionalInfo = child as AsycudaAdditionalInfo;
			asycudaAdditionalInfo.CSI_SubType = CSI_SubType;
		}
	}
}
