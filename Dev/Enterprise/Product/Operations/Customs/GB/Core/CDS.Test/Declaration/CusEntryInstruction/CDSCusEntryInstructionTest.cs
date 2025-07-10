using CargoWise.EntityFramework;
using Enterprise.Customs.GB.CDS.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryInstruction))]
	class CDSCusEntryInstructionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCreateInvoiceLineSupportingDoc()
		{
			// Reset Invoice data
			dec.Invoices.RemoveAll();
			var inv = dec.Invoices.AddNew();
			var invLine = inv.InvoiceLines.AddNew();

			var fromWH = Factory.New<OrgHeader>();
			var fromAddress = fromWH.Addresses.AddNew(OrgAddressType.Office, true);
			fromAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			fromWH.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "WH1CCP111111GB", Core.Constants.CountryCodes.UnitedKingdom);
			fromWH.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "WH1AEO22222/02", Core.Constants.CountryCodes.UnitedKingdom);

			var toWH = Factory.New<OrgHeader>();
			var toAddress = toWH.Addresses.AddNew(OrgAddressType.Office, true);
			toAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			toWH.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "WH2CCP333333GB", Core.Constants.CountryCodes.UnitedKingdom);
			toWH.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "WH2AEO44444/04", Core.Constants.CountryCodes.UnitedKingdom);

			cei.CEI_OA_Warehouse2 = fromAddress.PK;
			AssertEquals(2, invLine.SupportingDocuments.Count);

			cei.CEI_OA_Warehouse = toAddress.PK;
			AssertEquals(4, invLine.SupportingDocuments.Count);

			AssertCollectionContains(new ZQuery(CusSupportingInfoSchema.CSI_Code, CDS.Constants.DocumentCodes.C517).AddToFilter(CusSupportingInfoSchema.CSI_ReferenceNumber, "GBCWPWH1CCP111111GB"), invLine.SupportingDocuments);
			AssertCollectionContains(new ZQuery(CusSupportingInfoSchema.CSI_Code, CDS.Constants.DocumentCodes.Y027).AddToFilter(CusSupportingInfoSchema.CSI_ReferenceNumber, "GBWH1AEO22222/02"), invLine.SupportingDocuments);
			AssertCollectionContains(new ZQuery(CusSupportingInfoSchema.CSI_Code, CDS.Constants.DocumentCodes.C517).AddToFilter(CusSupportingInfoSchema.CSI_ReferenceNumber, "GBCWPWH2CCP333333GB"), invLine.SupportingDocuments);
			AssertCollectionContains(new ZQuery(CusSupportingInfoSchema.CSI_Code, CDS.Constants.DocumentCodes.Y027).AddToFilter(CusSupportingInfoSchema.CSI_ReferenceNumber, "GBWH2AEO44444/04"), invLine.SupportingDocuments);
		}

		public void TestCreateMiscAuthorisations()
		{
			var fromWH = Factory.New<OrgHeader>();
			var fromAddress = fromWH.Addresses.AddNew(OrgAddressType.Office, true);
			fromWH.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "WH1EORI11111111", Core.Constants.CountryCodes.UnitedKingdom);

			var toWH = Factory.New<OrgHeader>();
			var toAddress = toWH.Addresses.AddNew(OrgAddressType.Office, true);
			toWH.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "WH2EORI22222222", Core.Constants.CountryCodes.UnitedKingdom);

			cei.CusAuthorizationUsages.RemoveAll();

			cei.CEI_OA_Warehouse2 = fromAddress.PK;
			AssertEquals(1, cei.CusAuthorizationUsages.Count);

			cei.CEI_OA_Warehouse = toAddress.PK;
			AssertEquals(2, cei.CusAuthorizationUsages.Count);

			AssertCollectionContains(new ZQuery(CusAuthorizationUsageSchema.AGC_Code, CDSAuthorisationHeaderTypeList.Codes.CustomsWarehousingCWP)
										.AddToFilter(CusAuthorizationUsageSchema.AGC_Number, "GBWH1EORI11111111")
										.AddToFilter(CusAuthorizationUsageSchema.AGC_OH_Owner, fromWH.PK), (BusinessObjectCollection)cei.CusAuthorizationUsages);
			AssertCollectionContains(new ZQuery(CusAuthorizationUsageSchema.AGC_Code, CDSAuthorisationHeaderTypeList.Codes.CustomsWarehousingCWP)
										.AddToFilter(CusAuthorizationUsageSchema.AGC_Number, "GBWH2EORI22222222")
										.AddToFilter(CusAuthorizationUsageSchema.AGC_OH_Owner, toWH.PK), (BusinessObjectCollection)cei.CusAuthorizationUsages);
		}

		protected override void SetUp()
		{
			base.SetUp();
			dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var ceh = dec.CustomsEntryHeaders.AddNew();
			dec.CustomsEntryInstructions.RemoveAndDeleteAll();
			cei = dec.CustomsEntryInstructions.AddNew();
			cei.CEI_PackageCount = 30;
		}

		JobDeclaration dec;
		CusEntryInstruction cei;
	}
}
