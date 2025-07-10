using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class DetailWithoutLegalDocListTest : TestCase
	{
		public void TestRequiresDetailWithoutLegalDocInJustification()
		{
			Assert("3020", DetailWithoutLegalDocList.RequiresDetailWithoutLegalDocInJustification("3020"));
			Assert("3021", DetailWithoutLegalDocList.RequiresDetailWithoutLegalDocInJustification("3021"));
			Assert("3026", DetailWithoutLegalDocList.RequiresDetailWithoutLegalDocInJustification("3026"));
			Assert("3024", !DetailWithoutLegalDocList.RequiresDetailWithoutLegalDocInJustification("3024"));
			Assert("3004", !DetailWithoutLegalDocList.RequiresDetailWithoutLegalDocInJustification("3004"));
			Assert("XXXX", !DetailWithoutLegalDocList.RequiresDetailWithoutLegalDocInJustification("XXXX"));
			Assert("3001", !DetailWithoutLegalDocList.RequiresDetailWithoutLegalDocInJustification("3001"));
		}
	}
}
