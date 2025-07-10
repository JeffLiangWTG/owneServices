using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business.Testing
{
	class JobDeclarationDeepCloneStrategyTest : TestCaseWithFactory
	{
		public void TestCountrySpecificJobDocAddresses()
		{
			var declaration = Factory.New<JobDeclaration>();
			var stratety = new JobDeclarationDeepCloneStrategyForTest(declaration, CloneType.TemplateCopy, declaration.Factory);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				declaration.ManufacturerDocumentaryAddress,
				declaration.BuyerDocAddress,
			}, stratety.CountrySpecificJobDocAddressesExposed);
		}

		public void TestCopyCountrySpecificJobDocAddresses()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.SupplierDocumentaryAddress.E2_AddressOverride = true;
			declaration.SupplierDocumentaryAddress.OverseasPartyCodeType = OrgCusCode.ChinaCodeTypes.AEO;
			declaration.SupplierDocumentaryAddress.OverseasPartyCode = "AEO111";
			declaration.ImporterDocumentaryAddress.E2_AddressOverride = true;
			declaration.ImporterDocumentaryAddress.SocialCreditCode = "USC123";
			declaration.BuyerDocAddress.E2_AddressOverride = true;
			declaration.BuyerDocAddress.CustomsCode = "CCD222";
			declaration.ManufacturerDocumentaryAddress.E2_AddressOverride = true;
			declaration.ManufacturerDocumentaryAddress.CIQCode = "CIQ333";
			var clonedShipment = (ForwardingShipment)shipment.TemplateCopy();
			var clonedDeclaration = (JobDeclaration)clonedShipment.Declarations[0];
			AssertEquals("Only the items of CountrySpecificJobDocAddresses should be copied", 2, clonedDeclaration.DocAddresses.Count);
			AssertEquals("BuyerDocAddress should be copied", "CCD222", clonedDeclaration.BuyerDocAddress.CustomsCode);
			AssertEquals("ManufacturerDocumentaryAddress should be copied", "CIQ333", clonedDeclaration.ManufacturerDocumentaryAddress.CIQCode);
		}
	}

	class JobDeclarationDeepCloneStrategyForTest : JobDeclarationDeepCloneStrategy
	{
		public JobDeclarationDeepCloneStrategyForTest(JobDeclaration declarationToClone, CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn) : base(declarationToClone, cloneType, alternativeFactoryToInstantiateCloneIn)
		{
		}

		public IEnumerable<JobDocAddress> CountrySpecificJobDocAddressesExposed => base.CountrySpecificJobDocAddresses;
	}
}
