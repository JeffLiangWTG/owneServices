using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.Business.Test
{
	[TestedType(typeof(ClientLicenceUsage))]
	public class ClientLicenceUsageTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewUsage(factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewUsage(Factory);
		}

		static ClientLicenceUsage GetNewUsage(BusinessObjectFactory factory)
		{
			var usage = factory.NewWithValidTestData<ClientLicenceUsage>();
			var db = factory.NewWithValidTestData<LicenceDatabase>();
			usage.Staff.LS_LD = db.PK;
			return usage;
		}
	}
}
