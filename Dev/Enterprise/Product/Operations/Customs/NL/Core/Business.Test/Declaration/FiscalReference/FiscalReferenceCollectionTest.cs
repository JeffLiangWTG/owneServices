using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(FiscalReferenceCollection))]
sealed class FiscalReferenceCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<FiscalReference>
{
	protected override CusSupportingInfoCollection<FiscalReference> GetCusSupportingInfoCollection()
	{
		var declaration = Factory.New<JobDeclaration>();
		return new FiscalReferenceCollection(declaration);
	}
}
