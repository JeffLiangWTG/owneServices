using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public class SpecModelElementStrategy : CommonAdditionalElementStrategy
	{
		public static string AdditionalElementCode => "99998"; // 规格型号

		public override ZString GetDefaultValue(EnteringOrExiting isEnteringOrExiting) => (NoResString)"规格：、型号：";

		protected override ZString ValidationPattern => (NoResString)@"^规格：(\S)+、型号：(\S)+$";

		protected override ZString ValidationPatternMessage => Res.GetString("1406ba8a-4f95-487d-af94-52093d63bbb7", "The value is in incorrect format. It should look like 规格：XXX、型号：YYY."); // '请同时申报货品的“规格”及“型号”，格式为规格：XXX、型号：YYY'
	}
}
