using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.CA.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.CAGACPGAHeader)]
	public class GACPGAHeaderAddInfo : AutoGACPGAHeaderAddInfo
	{
		public GACPGAHeaderAddInfo(ZPropertyInfo addInfoProperty) : base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo)
		{
			return base.IsValidationEnabledCore(propertyInfo) && (Parent as ICADeclarationProvider).IsValidationEnabled;
		}
	}
}
