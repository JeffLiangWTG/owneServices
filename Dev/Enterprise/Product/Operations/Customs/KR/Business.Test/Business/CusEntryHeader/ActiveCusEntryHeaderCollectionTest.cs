using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ActiveCusEntryHeaderCollection))]
	sealed class ActiveCusEntryHeaderCollectionTest : Customs.Business.Testing.ActiveCusEntryHeaderCollectionTest<ActiveCusEntryHeaderCollection>
	{
		protected override ActiveCusEntryHeaderCollection GetCollectionToTest()
		{
			return new ActiveCusEntryHeaderCollection((JobDeclaration)Declaration);
		}
	}
}
