using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(ActiveCusEntryHeaderCollection))]
sealed class ActiveCusEntryHeaderCollectionTest : Customs.Business.Testing.ActiveCusEntryHeaderCollectionTest<ActiveCusEntryHeaderCollection>
{
	protected override ActiveCusEntryHeaderCollection GetCollectionToTest() => new ActiveCusEntryHeaderCollection((JobDeclaration)Declaration);
}
