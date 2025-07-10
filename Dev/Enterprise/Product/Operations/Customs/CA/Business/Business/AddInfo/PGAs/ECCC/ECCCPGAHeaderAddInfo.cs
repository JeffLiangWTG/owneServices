using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.CA.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.CAECCCPGAHeader)]
	public class ECCCPGAHeaderAddInfo : AutoECCCPGAHeaderAddInfo
	{
		public ECCCPGAHeaderAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo)
		{
			return base.IsValidationEnabledCore(propertyInfo) && (Parent as ICADeclarationProvider).IsValidationEnabled;
		}
	}
}
