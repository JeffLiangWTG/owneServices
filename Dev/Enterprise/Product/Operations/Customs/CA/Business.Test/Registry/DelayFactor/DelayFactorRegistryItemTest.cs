using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Registry.Testing
{
	[TestedType(typeof(DelayFactorRegistryItem))]
	sealed class DelayFactorRegistryItemTest : StronglyTypedRegistryItemTestCase<DelayFactorRegistryBusinessObject>
	{
		protected override StronglyTypedRegistryItem<DelayFactorRegistryBusinessObject, DelayFactorRegistryBusinessObject> GetNewRegistryItem()
		{
			return new DelayFactorRegistryItem("", null, null, null, new DelayFactorRegistryBusinessObject(5, DelayIntervalTypeCodes.Codes.None, 7, DelayIntervalTypeCodes.Codes.Default));
		}
	}
}
