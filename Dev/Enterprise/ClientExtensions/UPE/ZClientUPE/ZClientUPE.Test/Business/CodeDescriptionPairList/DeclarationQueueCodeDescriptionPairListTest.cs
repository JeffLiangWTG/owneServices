using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.UPE.Business.Testing
{
	public class DeclarationQueueCodeDescriptionPairListTest : TestCaseWithFactory
	{
		public void TestOrder()
		{
			DeclarationQueueCodeDescriptionPairList list = new DeclarationQueueCodeDescriptionPairList();
			AssertEquals(11, list.Count);
			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.Classification, list[0].Code);
			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.Compiling, list[1].Code);
			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.Lodgement, list[2].Code);
			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.Submitted, list[3].Code);
			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.Unknown, list[4].Code);
			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.BCA, list[5].Code);
			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.BCO, list[6].Code);
			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.EIR, list[7].Code);
			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.Pending, list[8].Code);
			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding, list[9].Code);
			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.Completed, list[10].Code);
		}
	}
}
