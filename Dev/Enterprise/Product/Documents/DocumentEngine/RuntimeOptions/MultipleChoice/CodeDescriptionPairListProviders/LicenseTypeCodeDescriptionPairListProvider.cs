using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class LicenseTypeCodeDescriptionPairListProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return SystemDataRegistry.Instance.StaffCertificateTypes.Value.GetCodeDescriptionPairList();
		}
	}
}
