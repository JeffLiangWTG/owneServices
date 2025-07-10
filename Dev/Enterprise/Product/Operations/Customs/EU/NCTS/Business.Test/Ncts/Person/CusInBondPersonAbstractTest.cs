using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestsSubclassesOf(typeof(CusInBondPerson))]
	public abstract class CusInBondPersonAbstractTest<TMaster> : EnterpriseBusinessObjectTestCase
		where TMaster : NctsHeader
	{
		public void TestCorrectTypeForLoad()
		{
			var locationContact = GetNewBusinessObject() as CusInBondPerson;
			Factory.Save();

			AssertType(locationContact.NctsHeader.CusInBondPersonType, new BusinessObjectFactory().Load<CusInBondPerson>(locationContact.PK));
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<TMaster>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return header.LocationContact;
		}
	}
}
