using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing;

[TestedType(typeof(FROfficeCodeCollection))]
sealed class FROfficeCodeCollectionTest : EuOfficeCodeCollectionTest
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var parent = Factory.New<JobDeclaration>();
		return new FROfficeCodeCollection(parent);
	}
}
