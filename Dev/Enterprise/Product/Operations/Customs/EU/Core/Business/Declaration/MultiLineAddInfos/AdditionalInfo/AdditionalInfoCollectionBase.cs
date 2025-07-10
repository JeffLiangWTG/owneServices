using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

public class AdditionalInfoCollection<TAdditionalInfo> : Customs.Business.CusSupportingInfoCollection<TAdditionalInfo>, IAdditionalInfoCollection<TAdditionalInfo> where TAdditionalInfo : AdditionalInfo
{
	public AdditionalInfoCollection(BusinessObject parent) : base(parent, Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo)
	{
		if (Master is CusExitDetail)
		{
			MaxCountValidationEnable(99);
		}
	}

	protected override void SetDefaultsForNewChild(BusinessObject child)
	{
		base.SetDefaultsForNewChild(child);

		if (Master is CusExitDetail)
		{
			((AdditionalInfo)child).CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
		}
	}

	public TAdditionalInfo AddNew(ZString code, ZString referenceNumber)
	{
		var additionalInfo = AddNew();
		additionalInfo.CSI_Code = code;
		additionalInfo.CSI_ReferenceNumber = referenceNumber;
		return additionalInfo;
	}
}
