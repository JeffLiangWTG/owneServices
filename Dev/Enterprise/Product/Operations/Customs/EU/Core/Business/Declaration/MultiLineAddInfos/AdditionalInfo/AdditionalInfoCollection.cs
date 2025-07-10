using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public interface IAdditionalInfoCollection<TAdditionalInfo> : Customs.Business.ICusSupportingInfoCollection<TAdditionalInfo> where TAdditionalInfo : AdditionalInfo
	{
		new TAdditionalInfo this[int index] { get; }
		new TAdditionalInfo AddNew();
		TAdditionalInfo AddNew(ZString code, ZString referenceNumber);
	}

	public class AdditionalInfoCollection : AdditionalInfoCollection<AdditionalInfo>
	{
		public AdditionalInfoCollection(BusinessObject parent) : base(parent)
		{
		}
	}
}
