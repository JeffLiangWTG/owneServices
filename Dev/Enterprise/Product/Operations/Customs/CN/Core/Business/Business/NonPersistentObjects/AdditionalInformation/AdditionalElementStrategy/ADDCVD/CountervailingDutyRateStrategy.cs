using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public class CountervailingDutyRateStrategy : ADDCVDElementStrategy
	{
		public static string AdditionalElementCode => "01117ac2bd2e912a552d8a20a338da71"; // 反补贴税率

		public override void ValidateAdditionalElement(IAdditionalInformationWrapperParent master, ZString elementValue, ZPropertyInfo propertyInfo)
		{
			AddMessageErrorIfNotNumberOrNotGreaterThanZero(elementValue, propertyInfo, master);
		}
	}
}
