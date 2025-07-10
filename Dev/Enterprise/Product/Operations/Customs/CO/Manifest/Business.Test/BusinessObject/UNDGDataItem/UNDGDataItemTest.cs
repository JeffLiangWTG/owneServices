using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	[TestedType(typeof(UNDGDataItem))]
	sealed class UNDGDataItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidationType()
		{
			var obj = Factory.New<UNDGDataItem>();
			AssertType<UNDGDataItemValidation>(obj.Validation);
		}
	}
}
