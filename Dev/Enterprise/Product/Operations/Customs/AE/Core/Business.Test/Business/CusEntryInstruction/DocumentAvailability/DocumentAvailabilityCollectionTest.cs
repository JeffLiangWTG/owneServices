using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(DocumentAvailabilityCollection))]
sealed class DocumentAvailabilityCollectionTest : CusSupportingInfoCollectionTest<DocumentAvailability>
{
	protected override CusSupportingInfoCollection<DocumentAvailability> GetCusSupportingInfoCollection()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		return new DocumentAvailabilityCollection(instruction);
	}
}
