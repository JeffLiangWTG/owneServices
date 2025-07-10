using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	class DocManagerDataFileSizeThresholdGbRegistryDataType : IntRegistryDataType
	{
		public DocManagerDataFileSizeThresholdGbRegistryDataType(int lowerBound, int upperBound) : base(lowerBound, upperBound)
		{
		}
	}
}
