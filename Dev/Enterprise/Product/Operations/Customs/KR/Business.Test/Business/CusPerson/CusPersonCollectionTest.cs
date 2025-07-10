using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusPersonCollection))]
	sealed class CusPersonCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => Factory.New<JobDeclaration>().Persons;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var cusPerson = Factory.New<CusPerson>();
			var glbPerson = Factory.New<GlbPerson>();
			glbPerson.PER_FullName = "Kenny G";
			cusPerson.CPN_PER_Person = glbPerson.PK;
			return cusPerson;
		}
	}
}
