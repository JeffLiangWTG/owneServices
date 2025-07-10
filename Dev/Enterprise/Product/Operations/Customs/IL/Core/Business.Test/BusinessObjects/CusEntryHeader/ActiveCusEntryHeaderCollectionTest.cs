using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(ActiveCusEntryHeaderCollection))]
	class ActiveCusEntryHeaderCollectionTest : Customs.Business.Testing.ActiveCusEntryHeaderCollectionTest<ActiveCusEntryHeaderCollection>
	{
		protected override ActiveCusEntryHeaderCollection GetCollectionToTest() => new ActiveCusEntryHeaderCollection((JobDeclaration)Declaration);
	}
}
