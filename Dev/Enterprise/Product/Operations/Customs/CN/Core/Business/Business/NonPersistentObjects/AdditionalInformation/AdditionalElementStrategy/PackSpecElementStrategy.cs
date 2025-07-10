using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public class PackSpecElementStrategy : CommonAdditionalElementStrategy
	{
		public static string AdditionalElementCode => "99997"; // 包装规格

		protected override ZString ValidationPattern => (NoResString)@"^[0-9]+\.?[0-9]*[^*×/0-9]+[*×][0-9]+\.?[0-9]*[^*×/0-9]+/[^*×/0-9]+$";

		protected override ZString ValidationPatternMessage => Res.GetString("9cc659a9-356b-4c05-8ebd-0c94facfad18", "The value is in incorrect format. It should look like 10干克*6罐/箱."); // '请正确填写包装规格，如10干克*6罐/箱'
	}
}
