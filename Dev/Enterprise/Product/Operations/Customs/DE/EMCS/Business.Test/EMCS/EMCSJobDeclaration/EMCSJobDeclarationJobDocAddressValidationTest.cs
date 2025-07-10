using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	public class EMCSJobDeclarationJobDocAddressValidationTest
		: EU.EMCS.Business.Testing.EMCSJobDeclarationJobDocAddressValidationTest
	{
		protected override HashSet<ZString> DestinationTypesForDestinationWarehouseTest => new HashSet<ZString> { EU.EMCS.Business.EMCSDestinationTypeList.Codes.DestinationTaxWarehouse, EU.EMCS.Business.EMCSDestinationTypeList.Codes.DestinationDirectDelivery };

		public void TestSupplierDocumentaryAddressHasEIRNumberWithRuleUSEIsAEX()
		{
			const string forAConsolidatedEAdTheConsignorNeedsAnEirAuthorizationWithRuleCodeUseAex = "For a consolidated e-AD the Consignor needs an EIR Authorization with a usage (USE) rule for an accredited exporter (AEX).";

			CombineAssertions(() =>
			{
				var jobDocAddress = declaration.DocAddresses.AddNew();
				jobDocAddress.E2_AddressType = DocAddressTypes.Codes.SupplierDocumentaryAddress;
				jobDocAddress.OrganisationPK = orgHeader.PK;
				AssertNoMessageError("No consolidated Document: no EIR code", jobDocAddress.OrganisationPKInfo, forAConsolidatedEAdTheConsignorNeedsAnEirAuthorizationWithRuleCodeUseAex);

				declaration.SetConsolidatedDocument();
				jobDocAddress.Validation.ValidateOrganisationPK();
				AssertHasMessageError("Is consolidated document: no EIR code", jobDocAddress.OrganisationPKInfo, forAConsolidatedEAdTheConsignorNeedsAnEirAuthorizationWithRuleCodeUseAex);

				var authorization = orgHeader.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "TEST", Core.Constants.CountryCodes.Germany);
				jobDocAddress.Validation.ValidateOrganisationPK();
				AssertHasMessageError("Is consolidated document: has EIR code without Rule USE = AEX", jobDocAddress.OrganisationPKInfo, forAConsolidatedEAdTheConsignorNeedsAnEirAuthorizationWithRuleCodeUseAex);

				var rule = authorization.CusAuthorisationRules.AddNew();
				rule.CPR_RuleCode = DE.Business.CusAuthorisationRuleTypeList.Codes.Usage;
				rule.CPR_ValueFrom = CusAuthorisationUsageRuleList.Codes.AccreditedExporter;
				jobDocAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageError("Is consolidated document: has EIR code with Rule USE = AEX", jobDocAddress.OrganisationPKInfo, forAConsolidatedEAdTheConsignorNeedsAnEirAuthorizationWithRuleCodeUseAex);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			declaration = Factory.New<EMCSJobDeclaration>();
		}
		OrgHeader orgHeader;
		EMCSJobDeclaration declaration;
	}
}
