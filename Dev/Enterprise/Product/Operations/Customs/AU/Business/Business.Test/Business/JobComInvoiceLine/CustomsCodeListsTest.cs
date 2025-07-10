namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CustomsCodeListsTest : NUnit.Framework.TestCase
	{
		public void TestCustomsValuationBasisLists()
		{
			AssertEquals("CustomsValuationBasis.AU.Line_List.Count", 5, EDIFICEValuationBasis.Line_List.Count);
		}
	}
}
