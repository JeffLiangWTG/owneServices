using CargoWise.EntityFramework;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(EdiPriceUsageMapping))]
	internal class EdiPriceUsageMappingTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return CreateMapping(factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return CreateMapping(Factory);
		}

		static EdiPriceUsageMapping CreateMapping(BusinessObjectFactory factory)
		{
			var org = factory.New<EDIOrgHeader>();
			org.OH_Code = "DDDSYD";
			org.CreateAndLoadLicenceForOrg();
			var header = org.LicCompany.PriceHeaders.AddNew();
			var bizo = header.UsageMaps.AddNew();
			return bizo;
		}
	}
}
