using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngineCore.DocWrappers.Testing;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocumentWrapperForTesting))]
	sealed class DocAuthorityToDealTestCase : DocumentWrapperTest
	{
		public void TestMessageLines()
		{
			AssertEquals("Line Count", 0, ATDWrapper.MessageLines.Count);

			SegmentGroup13 group13 = Message.Group6.InstantiateAChildAndAddItToChildrenCollection().Group13.InstantiateAChildAndAddItToChildrenCollection();
			ATD = new AuthorityToDeal(Message);
			ATDWrapper = DocAuthorityToDeal.New(ATD, Factory);
			AssertEquals("Line Count", 1, ATDWrapper.MessageLines.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Message = new CUSRESMessage();
			ATD = new AuthorityToDeal(Message);
			ATDWrapper = DocAuthorityToDeal.New(ATD, Factory);
		}

		AuthorityToDeal ATD;
		DocAuthorityToDeal ATDWrapper;
		CUSRESMessage Message;
	}
}
