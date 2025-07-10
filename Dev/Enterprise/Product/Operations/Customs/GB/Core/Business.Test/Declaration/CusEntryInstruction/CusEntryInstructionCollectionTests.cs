using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryInstructionCollection))]
	class CusEntryInstructionCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			return new CusEntryInstructionCollection(testDeclaration);
		}
	}
}
