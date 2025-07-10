using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(ClearanceCollection))]
	class ClearanceCollectionTest : CusSupportingInfoCollectionTest<Clearance>
	{
		protected override Customs.Business.CusSupportingInfoCollection<Clearance> GetCusSupportingInfoCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			return new ClearanceCollection(entryInstruction);
		}
	}
}
