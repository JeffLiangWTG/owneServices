using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal class CustomsQueueCodeDescriptionPairListTest : TestCaseWithFactory
	{
		public void TestCompletedQueueNames()
		{
			AssertEquals(DefaultQueueCodeDescriptionPairList.Codes.Completed, CustomsQueueCodeDescriptionPairList.CompletedQueueNames[0]);
			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding, CustomsQueueCodeDescriptionPairList.CompletedQueueNames[1]);
		}
	}
}
