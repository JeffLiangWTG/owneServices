using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC
{
	public class JXCMandatoryValidation : MandatoryValidation
	{
		public new static void WarnIfNotEntered(ZPropertyInfo propertyInfo)
		{
			WarnIfNotEntered(propertyInfo, ZString.Empty, JXCConstants.JXCWarningPrefix);
		}

		public new static void WarnIfNotEntered(ZPropertyInfo propertyInfo, ZString propertyDescriptor)
		{
			WarnIfNotEntered(propertyInfo, propertyDescriptor, JXCConstants.JXCWarningPrefix);
		}
	}
}
