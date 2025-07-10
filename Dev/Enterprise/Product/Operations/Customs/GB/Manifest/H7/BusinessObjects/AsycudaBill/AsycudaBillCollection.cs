using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business;

namespace Enterprise.Customs.GB.H7.Business
{
	public class AsycudaBillCollection : EU.H7.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>
	{
		public AsycudaBillCollection(AsycudaManifestHeader master)
			: base(master)
		{
		}

		protected override void OnAdded(BusinessObject businessObject)
		{
			base.OnAdded(businessObject);

			var bill = (AsycudaBill)businessObject;
			CreateAndDefaultAdditionalInfo(bill);
		}

		void CreateAndDefaultAdditionalInfo(AsycudaBill bill)
		{
			if (!bill.AdditionalInfos.Any())
			{
				var portOfDischarge = bill.Header?.PortOfDischarge;
				if (portOfDischarge?.IsInNorthernIreland == true)
				{
					var portOfLoading = bill.Header?.PortOfLoading;
					if (portOfLoading?.IsInNorthernIreland == false)
					{
						var additionalInfo = bill.AdditionalInfos.AddNew();
						additionalInfo.CSI_Code = portOfLoading.IsInGreatBritain
							? GBCommonConstants.AdditonalInfoCodes.NIDOM
							: GBCommonConstants.AdditonalInfoCodes.NIIMP;
					}
				}
			}
		}
	}
}
