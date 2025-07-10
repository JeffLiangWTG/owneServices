using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.Client.JAS.Business.JXC
{
	public class JXCListValidation : ListValidation
	{
		public new static void WarnIfInvalidCode(ZPropertyInfo codePropertyInfo, ICodeDescriptionPairList list)
		{
			WarnIfInvalidCode(codePropertyInfo, list, JXCConstants.JXCWarningPrefix);
		}
	}
}
