using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(RelatedEntryInstructionGenPivotCollection))]
	internal class RelatedEntryInstructionGenPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new RelatedEntryInstructionGenPivotCollection(Factory.New<CusEntryInstruction>());
	}
}
