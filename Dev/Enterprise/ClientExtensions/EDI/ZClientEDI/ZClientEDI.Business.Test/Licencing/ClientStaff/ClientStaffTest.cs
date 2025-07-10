using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.Business.Test
{
	[TestedType(typeof(ClientStaff))]
	public class ClientStaffTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewStaff(factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewStaff(Factory);
		}

		static ClientStaff GetNewStaff(BusinessObjectFactory factory)
		{
			var staff = factory.NewWithValidTestData<ClientStaff>();
			var db = factory.NewWithValidTestData<LicenceDatabase>();
			staff.LS_LD = db.PK;
			return staff;
		}
	}
}
