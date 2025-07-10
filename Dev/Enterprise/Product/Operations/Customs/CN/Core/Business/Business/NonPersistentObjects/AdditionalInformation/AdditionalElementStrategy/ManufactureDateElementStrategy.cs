using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public class ManufactureDateElementStrategy : CommonAdditionalElementStrategy
	{
		public static string AdditionalElementCode => "ef374a392b7934885636cee2f907884a"; // 生产日期

		const string DateExpression = "[0-9]{4}(((0[13578]|(10|12))(0[1-9]|[1-2][0-9]|3[0-1]))|(02(0[1-9]|[1-2][0-9]))|((0[469]|11)(0[1-9]|[1-2][0-9]|30)))";

		static string DatePattern => $@"^({DateExpression};)*{DateExpression}$";

		protected override ZString ValidationPattern => DatePattern;

		public static bool IsValidManufactureDateString(ZString dateString) => !dateString.IsEmpty && Regex.IsMatch(dateString, DatePattern);

		protected override ZString ValidationPatternMessage => Res.GetString("1406ba8a-4f95-487d-af94-52093d63bbb1", "The value is in incorrect format. The date should in format '{0}'. If there are multiply dates, please join them with ';'.", "yyyyMMdd");
	}
}
