using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public class AntiDumpingDutyRateStrategy : ADDCVDElementStrategy
	{
		public static string AdditionalElementCode => "33f1e2ce12b0de03b26a92c409c1f8f0"; // 反倾销税率

		public override void ValidateAdditionalElement(IAdditionalInformationWrapperParent master, ZString elementValue, ZPropertyInfo propertyInfo)
		{
			AddMessageErrorIfNotNumberOrNotGreaterThanZero(elementValue, propertyInfo, master);
		}
	}
}
