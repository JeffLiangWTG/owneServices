using System.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public interface IAdditionalInfoCollection<out T> : Customs.Business.ICusSupportingInfoCollection<T>, IBindingList
		where T : AdditionalInfo
	{
		new T this[int index] { get; }
		new T AddNew();
	}

	public class AdditionalInfoCollection<T> : Customs.Business.CusSupportingInfoCollection<T>, IAdditionalInfoCollection<T>
		where T : AdditionalInfo
	{
		public AdditionalInfoCollection(BusinessObject parent) : base(parent, Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo)
		{
		}
	}
}
