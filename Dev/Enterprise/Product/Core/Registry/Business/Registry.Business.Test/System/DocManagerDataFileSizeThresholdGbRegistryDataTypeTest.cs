using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DocManagerDataFileSizeThresholdGbRegistryDataType))]
	sealed class DocManagerDataFileSizeThresholdGbRegistryDataTypeTest : IntRegistryDataTypeTest
	{
		#region Implementation

		protected override IntRegistryDataType GetNewDataType()
		{
			var result = new DocManagerDataFileSizeThresholdGbRegistryDataType(-1000, 2000);
			return result;
		}

		#endregion
	}
}
