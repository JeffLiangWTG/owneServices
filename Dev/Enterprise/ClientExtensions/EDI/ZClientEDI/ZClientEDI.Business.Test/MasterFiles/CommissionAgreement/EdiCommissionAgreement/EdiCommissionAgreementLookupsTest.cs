using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Client.EDI.MasterFiles.Test
{
	internal class EdiCommissionAgreementLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCustomers()
		{
			var invoiceOrg = Factory.NewWithValidTestData<OrgHeader>();

			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "DDDABCSYD";
			org.CreateAndLoadLicenceForOrg();
			org.LicenceEnterpriseCode = "DDD";
			org.LicCompany.LC_CompanyCode = "ABC";
			var invoiceDelivery = org.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery.L9_OH_InvoiceTo = invoiceOrg.PK;

			var orgParent = Factory.NewWithValidTestData<OrgHeader>();
			var orgSubsidiary = Factory.NewWithValidTestData<OrgHeader>();
			org.AddRelatedParty(orgParent.PK, RelatedPartyTypeList.Codes.ManagementGrouping, RelatedPartyDirectionList.Codes.Forwarder, Core.Constants.TransportModes.All, ZString.Empty, null);
			orgSubsidiary.AddRelatedParty(org.PK, RelatedPartyTypeList.Codes.ManagementGrouping, RelatedPartyDirectionList.Codes.Forwarder, Core.Constants.TransportModes.All, ZString.Empty, null);

			Factory.Save();

			var opportunity = org.SalesOpportunities.AddNew();
			var agreement = opportunity.CommissionAgreements.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { orgParent.PK, org.PK, orgSubsidiary.PK, invoiceOrg.PK }, agreement.Lookups.Customers.Select(c => c.PK));

			opportunity.P8_OH = ZGuid.Empty;
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<ZGuid>(), agreement.Lookups.Customers.Select(c => c.PK));
		}
	}
}
