using CargoWise.Types;

namespace Enterprise.Registry.Business
{
	static class BillOfLadingNumberCustomizationHelper
	{
		public static bool IsMacro(this ZString text)
		{
			var leftSignIndex = text.IndexOf('<');
			var rightSignIndex = text.IndexOf('>');
			return leftSignIndex >= 0 && rightSignIndex > 0 && leftSignIndex < rightSignIndex;
		}
	}
}
