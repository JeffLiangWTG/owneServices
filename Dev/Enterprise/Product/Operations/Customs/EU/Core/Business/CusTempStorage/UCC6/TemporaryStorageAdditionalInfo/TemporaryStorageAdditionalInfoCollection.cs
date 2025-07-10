using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public interface ITemporaryStorageAdditionalInfoCollection<out T> : Customs.Business.ICusSupportingInfoCollection<T>
		where T : TemporaryStorageAdditionalInfo
	{
	}

	public class TemporaryStorageAdditionalInfoCollection<T> : Customs.Business.CusSupportingInfoCollection<T>, ITemporaryStorageAdditionalInfoCollection<T>
		where T : TemporaryStorageAdditionalInfo
	{
		public TemporaryStorageAdditionalInfoCollection(BusinessObject parent)
			: base(parent, Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			if (Master is TemporaryStorageBill temporaryStorageBill)
			{
				if (temporaryStorageBill.ABL_BolType == TemporaryStorageBill.ChildBolCode)
				{
					((T)child).CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				}
			}
		}
	}
}
