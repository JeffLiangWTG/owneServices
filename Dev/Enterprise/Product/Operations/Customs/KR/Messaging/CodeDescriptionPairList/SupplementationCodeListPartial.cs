using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Messaging
{
	public partial class SupplementationCodeList
	{
		public static string GetSupplementationType(string code)
		{
			ZString result;
			if (code == Codes.Guidance)
			{
				result = (NoResString)"보완사항 안내";
			}
			else
			{
				result = (NoResString)"보완사항 통보";
			}
			return result;
		}
	}
}
