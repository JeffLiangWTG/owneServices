#if DEBUG
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ManifestBase
{
	partial class AsycudaManifestHeader : IBusinessObjectTestDataHelperPropertiesToExclude
	{
		bool IBusinessObjectTestDataHelperPropertiesToExclude.ShouldExcludeFromFillWithValidTestData(ZString name)
		{
			return ShouldExcludeFromFillWithValidTestData(name);
		}

		protected virtual bool ShouldExcludeFromFillWithValidTestData(ZString name)
		{
			return name == Schema.AMA_ApplicationCode;
		}
	}
}
#endif
