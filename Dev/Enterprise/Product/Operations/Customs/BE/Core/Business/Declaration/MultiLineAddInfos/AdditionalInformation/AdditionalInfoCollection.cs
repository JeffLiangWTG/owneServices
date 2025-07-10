using CargoWise.EntityFramework;

namespace Enterprise.Customs.BE.Business;

public class AdditionalInfoCollection : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection
{
	public AdditionalInfoCollection(BusinessObject parent) : base(parent)
	{
	}

	public new AdditionalInfo this[int i] => (AdditionalInfo)base[i];

	public new AdditionalInfo AddNew() => (AdditionalInfo)base.AddNew();
}
