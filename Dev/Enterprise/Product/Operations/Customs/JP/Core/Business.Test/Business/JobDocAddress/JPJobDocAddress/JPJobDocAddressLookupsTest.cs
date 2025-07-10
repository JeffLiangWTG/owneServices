using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(JPJobDocAddressLookups))]
	public class JPJobDocAddressLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestGovRegNumTypes()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			AssertEquals("LPC, CIE, JAS", declaration.AttorneyForCustomsProceduresAddress.Lookups.GovRegNumTypes.CodesAsString);
			AssertEquals("FSB, CIE", declaration.SupplierDocumentaryAddress.Lookups.GovRegNumTypes.CodesAsString);
			AssertEquals("LPC, CIE, JAS", declaration.ImporterDocumentaryAddress.Lookups.GovRegNumTypes.CodesAsString);
			AssertEquals("FSB, CIE", declaration.DeclarationConsignorAddress.Lookups.GovRegNumTypes.CodesAsString);
			AssertEquals("LPC, CIE, JAS", declaration.DeclarationConsigneeAddress.Lookups.GovRegNumTypes.CodesAsString);
			AssertEquals("NUC", declaration.AirCargoAgent.Lookups.GovRegNumTypes.CodesAsString);
			AssertEquals("NUC", declaration.ExternalBrokerAddress.Lookups.GovRegNumTypes.CodesAsString);
			AssertEquals("NUC", declaration.InspectionWitness.Lookups.GovRegNumTypes.CodesAsString);

			declaration.JE_MessageType = "EXP";
			AssertEquals("LPC, CIE, JAS", declaration.AttorneyForCustomsProceduresAddress.Lookups.GovRegNumTypes.CodesAsString);
			AssertEquals("LPC, CIE, JAS", declaration.SupplierDocumentaryAddress.Lookups.GovRegNumTypes.CodesAsString);
			AssertEquals("FSB, CIE", declaration.ImporterDocumentaryAddress.Lookups.GovRegNumTypes.CodesAsString);
			AssertEquals("LPC, CIE, JAS", declaration.DeclarationConsignorAddress.Lookups.GovRegNumTypes.CodesAsString);
			AssertEquals("FSB, CIE", declaration.DeclarationConsigneeAddress.Lookups.GovRegNumTypes.CodesAsString);
			AssertEquals("NUC", declaration.AirCargoAgent.Lookups.GovRegNumTypes.CodesAsString);
			AssertEquals("NUC", declaration.ExternalBrokerAddress.Lookups.GovRegNumTypes.CodesAsString);
			AssertEquals("NUC", declaration.InspectionWitness.Lookups.GovRegNumTypes.CodesAsString);
		}
	}
}
