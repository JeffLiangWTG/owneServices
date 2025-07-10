using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EES.Testing
{
	[TestedType(typeof(EESDataRegistry))]
	public class EESDataRegistryTest : RegistryItemSetTestCase<EESDataRegistry>
	{
		public void TestNumberOfOriginalBillsToBePrintedOnDotMatrix()
		{
			AssertEquals("NumberOfOriginalBillsToBePrintedOnDotMatrix", 1, ItemSet.NumberOfOriginalBillsToBePrintedOnDotMatrix);
			ItemSet.NumberOfOriginalBillsToBePrintedOnDotMatrix = 33;
			AssertEquals("NumberOfOriginalBillsToBePrintedOnDotMatrix", 33, ItemSet.NumberOfOriginalBillsToBePrintedOnDotMatrix);
		}
	}
}
