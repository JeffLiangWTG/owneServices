using CargoWise.Types;

namespace Enterprise.Customs.JP.Common
{
	public static class DocumentWrapperHelper
	{
		public static ZString FormatBarCode(string id) => ZString.Format("*{0}*", id);
	}
}
