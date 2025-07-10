using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	[TestedType(typeof(StandaloneOrganisationValueObjectDataAdapter))]
	sealed class StandardManualAndBatchImportOrganisationValueObjectDataAdapterTest : StandaloneOrganisationValueObjectDataAdapterTest
	{
		public void TestImportOrganisationTypes()
		{
			Xsd.Organisation value = new Xsd.Organisation();
			value.OrganisationDetails = new Xsd.OrganisationDetail();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			value.EDICode = "edicode";
			value.OrganisationDetails.Name = "splaty";

			value.OrganisationDetails.OrganisationTypes = new Xsd.OrganisationDetailOrganisationTypeCollection();

			Xsd.OrganisationDetailOrganisationType orgType1 = value.OrganisationDetails.OrganisationTypes.AddNew();
			Xsd.OrganisationDetailOrganisationType orgType2 = value.OrganisationDetails.OrganisationTypes.AddNew();
			Xsd.OrganisationDetailOrganisationType orgType3 = value.OrganisationDetails.OrganisationTypes.AddNew();
			Xsd.OrganisationDetailOrganisationType orgType4 = value.OrganisationDetails.OrganisationTypes.AddNew();

			orgType1.Value = Xsd.OrganisationTypes.NAT;
			orgType2.Value = Xsd.OrganisationTypes.FWD;
			orgType3.Value = Xsd.OrganisationTypes.BRK;
			orgType4.Value = Xsd.OrganisationTypes.CNR;

			OrgHeader importToOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			DataAdapter.ImportFromValueObject(importToOrganisation, value, context);

			Assert(importToOrganisation.OH_IsNationalAccount);
			Assert(importToOrganisation.OH_IsForwarder);
			Assert(importToOrganisation.OH_IsBroker);
			Assert(importToOrganisation.OH_IsConsignor);

			Factory.Save();

			orgType2.Status = false;
			orgType2.StatusSpecified = true;
			orgType4.Status = false;
			orgType4.StatusSpecified = true;

			DataAdapter.ImportFromValueObject(importToOrganisation, value, context);

			Assert(importToOrganisation.OH_IsNationalAccount);
			Assert(!importToOrganisation.OH_IsForwarder);
			Assert(importToOrganisation.OH_IsBroker);
			Assert(!importToOrganisation.OH_IsConsignor);
		}

		Xsd.Organisation value;
		ValueObjectImportContext context;
		OrgHeader ImportToOrganisation;

		void SetupARAPDefaultingData(bool? isARApplicable, bool isARSpecified, bool? isAPApplicable, bool isAPSpecified)
		{
			value = new Xsd.Organisation();
			value.OrganisationDetails = new Xsd.OrganisationDetail();
			context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			OrgDebtorGroup debtorGroup = context.Factory.New<OrgDebtorGroup>();
			OrgCreditorGroup creditorGroup = context.Factory.New<OrgCreditorGroup>();

			debtorGroup.OJ_Code = "UTS";
			debtorGroup.OJ_Desc = "UUUUUUUUTTTTTTTTSSSSSSS";

			creditorGroup.OG_Code = "UTV";
			creditorGroup.OG_Desc = "UUUUUUUUTTTTTTTTvvvvvvv";

			value.EDICode = "edicode";
			value.OrganisationDetails.Name = "splaty";

			Xsd.AccountsReceivable receivable1 = value.OrganisationDetails.AccountsReceivables.AddNew();
			receivable1.AccountGroup = "UTS";
			if (isARApplicable != null)
			{
				receivable1.GSTIsApplicable = (bool)isARApplicable;
			}
			receivable1.GSTIsApplicableSpecified = isARSpecified;

			Xsd.AccountsPayable payable1 = value.OrganisationDetails.AccountsPayables.AddNew();
			payable1.AccountGroup = "UTV";
			if (isAPApplicable != null)
			{
				payable1.GSTIsApplicable = (bool)isAPApplicable;
			}
			payable1.GSTIsApplicableSpecified = isAPSpecified;
		}

		void AssertGSTApplicable(bool isCreditor, bool isDebtor, bool aPTaxApplicable, bool aRTaxApplicable)
		{
			ImportToOrganisation = Factory.New<OrgHeader>();
			ImportToOrganisation.OH_IsCreditor = isCreditor;
			ImportToOrganisation.OH_IsDebtor = isDebtor;
			ImportToOrganisation.CompanyData.SetAPTaxApplicable(aPTaxApplicable);
			ImportToOrganisation.CompanyData.SetARTaxApplicable(aRTaxApplicable);
			AssertEquals(aPTaxApplicable, ImportToOrganisation.CompanyData.IsAPTaxApplicable);
			AssertEquals(aRTaxApplicable, ImportToOrganisation.CompanyData.IsARTaxApplicable);
		}

		public void TestImportAPARNotApplicableBeforeARAPApplicable()
		{
			SetupARAPDefaultingData(false, true, false, true);
			AssertGSTApplicable(true, true, true, true);

			DataAdapter.ImportFromValueObject(ImportToOrganisation, value, context);

			AssertEquals(false, ImportToOrganisation.CompanyData.IsARTaxApplicable);
			AssertEquals(false, ImportToOrganisation.CompanyData.IsAPTaxApplicable);
		}

		public void TestImportAPARNotApplicableBeforeAPApplicable()
		{
			SetupARAPDefaultingData(false, true, false, true);
			AssertGSTApplicable(true, true, true, false);

			DataAdapter.ImportFromValueObject(ImportToOrganisation, value, context);

			AssertEquals(false, ImportToOrganisation.CompanyData.IsARTaxApplicable);
			AssertEquals(false, ImportToOrganisation.CompanyData.IsAPTaxApplicable);
		}

		public void TestImportAPARNotApplicableBeforeARApplicable()
		{
			SetupARAPDefaultingData(false, true, false, true);
			AssertGSTApplicable(true, true, false, true);

			DataAdapter.ImportFromValueObject(ImportToOrganisation, value, context);

			AssertEquals(false, ImportToOrganisation.CompanyData.IsARTaxApplicable);
			AssertEquals(false, ImportToOrganisation.CompanyData.IsAPTaxApplicable);
		}

		public void TestImportAPARApplicableBeforeARAPApplicable()
		{
			SetupARAPDefaultingData(true, true, true, true);
			AssertGSTApplicable(true, true, true, true);

			DataAdapter.ImportFromValueObject(ImportToOrganisation, value, context);

			AssertEquals(true, ImportToOrganisation.CompanyData.IsARTaxApplicable);
			AssertEquals(true, ImportToOrganisation.CompanyData.IsAPTaxApplicable);
		}

		public void TestImportAPARApplicableBeforeAPApplicable()
		{
			SetupARAPDefaultingData(true, true, true, true);
			AssertGSTApplicable(true, true, true, false);

			DataAdapter.ImportFromValueObject(ImportToOrganisation, value, context);

			AssertEquals(true, ImportToOrganisation.CompanyData.IsARTaxApplicable);
			AssertEquals(true, ImportToOrganisation.CompanyData.IsAPTaxApplicable);
		}

		public void TestImportAPARApplicableBeforeARApplicable()
		{
			SetupARAPDefaultingData(true, true, true, true);
			AssertGSTApplicable(true, true, false, true);

			DataAdapter.ImportFromValueObject(ImportToOrganisation, value, context);

			AssertEquals(true, ImportToOrganisation.CompanyData.IsARTaxApplicable);
			AssertEquals(true, ImportToOrganisation.CompanyData.IsAPTaxApplicable);
		}

		public void TestImportAPARNotSpecifiedBeforeARAPApplicable()
		{
			SetupARAPDefaultingData(null, false, null, false);
			AssertGSTApplicable(true, true, true, true);

			DataAdapter.ImportFromValueObject(ImportToOrganisation, value, context);

			AssertEquals("AR Tax Applicable should stay true when importing blank tag", true, ImportToOrganisation.CompanyData.IsARTaxApplicable);
			AssertEquals("AP Tax Applicable should stay true when importing blank tag", true, ImportToOrganisation.CompanyData.IsAPTaxApplicable);
		}

		public void TestImportAPARNotSpecifiedBeforeApplicable()
		{
			SetupARAPDefaultingData(null, false, null, false);
			AssertGSTApplicable(true, true, false, false);

			DataAdapter.ImportFromValueObject(ImportToOrganisation, value, context);

			AssertEquals("AR Tax Applicable should stay true when importing blank tag", false, ImportToOrganisation.CompanyData.IsARTaxApplicable);
			AssertEquals("AP Tax Applicable should stay true when importing blank tag", false, ImportToOrganisation.CompanyData.IsAPTaxApplicable);
		}

		public void TestImportAPandARDetails()
		{
			Xsd.Organisation value = new Xsd.Organisation();
			value.OrganisationDetails = new Xsd.OrganisationDetail();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			GlbCompany otherCompany = context.Factory.New<GlbCompany>();
			otherCompany.GC_Code = "OTH";

			OrgDebtorGroup debtorGroup = context.Factory.New<OrgDebtorGroup>();
			OrgCreditorGroup creditorGroup = context.Factory.New<OrgCreditorGroup>();

			debtorGroup.OJ_Code = "UTS";
			debtorGroup.OJ_Desc = "UUUUUUUUTTTTTTTTSSSSSSS";

			creditorGroup.OG_Code = "UTV";
			creditorGroup.OG_Desc = "UUUUUUUUTTTTTTTTvvvvvvv";

			value.EDICode = "edicode";
			value.OrganisationDetails.Name = "splaty";

			Xsd.AccountsReceivable receivable1 = value.OrganisationDetails.AccountsReceivables.AddNew();

			receivable1.DefaultCurrency = "UAH";
			receivable1.CreditLimit = 12.12f;
			receivable1.ExternalDebtorCode = "ACME";
			receivable1.CreditOnHold = false;
			receivable1.CreditApproved = true;
			receivable1.GSTIsApplicable = true;
			receivable1.GSTIsApplicableSpecified = true;
			receivable1.WithholdingTaxIsApplicable = false;
			receivable1.AccountGroup = "UTS";
			receivable1.SettlementDetails = new Xsd.SettlementDetails();
			receivable1.SettlementDetails.StandardInvoiceTerms = "INV";
			receivable1.SettlementDetails.StandardInvoiceDays = 4;
			receivable1.SettlementDetails.DisbursementInvoiceTerms = "MTH";
			receivable1.SettlementDetails.DisbursementInvoiceDays = 6;

			Xsd.AccountsReceivable receivable2 = value.OrganisationDetails.AccountsReceivables.AddNew();

			receivable2.CompanyCode = otherCompany.GC_Code;
			receivable2.DefaultCurrency = "USD";
			receivable2.CreditLimit = 13.13f;
			receivable2.ExternalDebtorCode = "Harley Davidson Co";
			receivable2.CreditOnHold = false;
			receivable2.CreditApproved = true;
			receivable2.GSTIsApplicable = true;
			receivable2.GSTIsApplicableSpecified = true;
			receivable2.WithholdingTaxIsApplicable = false;
			receivable2.AccountGroup = "UTS";
			receivable2.SettlementDetails = new Xsd.SettlementDetails();
			receivable2.SettlementDetails.StandardInvoiceTerms = "INV";
			receivable2.SettlementDetails.StandardInvoiceDays = 5;
			receivable2.SettlementDetails.DisbursementInvoiceTerms = "MTH";
			receivable2.SettlementDetails.DisbursementInvoiceDays = 7;

			Xsd.AccountsPayable payable1 = value.OrganisationDetails.AccountsPayables.AddNew();

			payable1.DefaultCurrency = "AUD";
			payable1.CreditLimit = 10.10f;
			payable1.ExternalCreditorCode = "Vegemite";
			payable1.GSTIsApplicable = true;
			payable1.GSTIsApplicableSpecified = true;
			payable1.WithholdingTaxIsApplicable = true;
			payable1.PaymentDays = 5;
			payable1.PaymentTerms = "SHP";
			payable1.AccountGroup = "UTV";

			Xsd.AccountsPayable payable2 = value.OrganisationDetails.AccountsPayables.AddNew();

			payable2.CompanyCode = otherCompany.GC_Code;
			payable2.DefaultCurrency = "NZD";
			payable2.CreditLimit = 11.11f;
			payable2.ExternalCreditorCode = "PLANET EXPRESS";
			payable2.GSTIsApplicable = true;
			payable2.GSTIsApplicableSpecified = true;
			payable2.WithholdingTaxIsApplicable = true;
			payable2.PaymentDays = 6;
			payable2.PaymentTerms = "SHP";
			payable2.AccountGroup = "UTV";

			OrgHeader importToOrganisation = Factory.New<OrgHeader>();
			DataAdapter.ImportFromValueObject(importToOrganisation, value, context);

			AssertEquals(true, importToOrganisation.OH_IsDebtor);
			AssertEquals("UAH", importToOrganisation.CompanyData.OB_RX_NKARDDefltCurrency);
			AssertEquals((decimal)12.12, importToOrganisation.MiscServ.OM_ARCreditLimit);
			AssertEquals("ACME", importToOrganisation.CompanyData.OB_ARExternalDebtorCode);
			AssertEquals(false, importToOrganisation.MiscServ.OM_AROnCreditHold);
			AssertEquals(true, importToOrganisation.CompanyData.IsARTaxApplicable);
			AssertEquals(false, importToOrganisation.MiscServ.OM_ARWHTApplicable);
			AssertEquals("INV", importToOrganisation.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Term);
			AssertEquals((byte)4, importToOrganisation.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Days);
			AssertEquals("MTH", importToOrganisation.CompanyData.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Term);
			AssertEquals((byte)6, importToOrganisation.CompanyData.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Days);

			AssertEquals(true, importToOrganisation.OH_IsCreditor);
			AssertEquals("AUD", importToOrganisation.CompanyData.OB_RX_NKAPDefltCurrency);
			AssertEquals((decimal)10.10, importToOrganisation.MiscServ.OM_APCreditLimit);
			AssertEquals("Vegemite", importToOrganisation.CompanyData.OB_APExternalCreditorCode);
			AssertEquals(true, importToOrganisation.CompanyData.IsAPTaxApplicable);
			AssertEquals(true, importToOrganisation.MiscServ.OM_APWHTApplicable);
			AssertEquals("SHP", importToOrganisation.CompanyData.GetAPTerm().Term);
			AssertEquals((byte)5, importToOrganisation.CompanyData.GetAPTerm().Days);

			AssertEquals(debtorGroup.PK, importToOrganisation.MiscServ.OM_OJ_ARDebtorGroup);
			AssertEquals(creditorGroup.PK, importToOrganisation.MiscServ.OM_OG_APCreditorGroup);

			OrgCompanyData orgCompanyDataForOtherCompany = importToOrganisation.GetCompanyDataForGlbCompany(otherCompany);

			AssertNotNull(orgCompanyDataForOtherCompany);
			AssertEquals(true, orgCompanyDataForOtherCompany.OB_IsDebtor);
			AssertEquals("USD", orgCompanyDataForOtherCompany.OB_RX_NKARDDefltCurrency);
			AssertEquals((decimal)13.13, orgCompanyDataForOtherCompany.OB_ARCreditLimit);
			AssertEquals("Harley Davidson Co", orgCompanyDataForOtherCompany.OB_ARExternalDebtorCode);
			AssertEquals(false, orgCompanyDataForOtherCompany.OB_AROnCreditHold);
			AssertEquals(true, orgCompanyDataForOtherCompany.IsARTaxApplicable);
			AssertEquals(false, orgCompanyDataForOtherCompany.OB_ARWHTApplicable);
			AssertEquals("INV", orgCompanyDataForOtherCompany.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Term);
			AssertEquals((byte)5, orgCompanyDataForOtherCompany.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Days);
			AssertEquals("MTH", orgCompanyDataForOtherCompany.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Term);
			AssertEquals((byte)7, orgCompanyDataForOtherCompany.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Days);

			AssertEquals(true, orgCompanyDataForOtherCompany.OB_IsCreditor);
			AssertEquals("NZD", orgCompanyDataForOtherCompany.OB_RX_NKAPDefltCurrency);
			AssertEquals((decimal)11.11, orgCompanyDataForOtherCompany.OB_APCreditLimit);
			AssertEquals("PLANET EXPRESS", orgCompanyDataForOtherCompany.OB_APExternalCreditorCode);
			AssertEquals(true, orgCompanyDataForOtherCompany.IsAPTaxApplicable);
			AssertEquals(true, orgCompanyDataForOtherCompany.OB_APWHTApplicable);
			AssertEquals("SHP", orgCompanyDataForOtherCompany.GetAPTerm().Term);
			AssertEquals((byte)6, orgCompanyDataForOtherCompany.GetAPTerm().Days);
			AssertEquals(debtorGroup.PK, orgCompanyDataForOtherCompany.OB_OJ_ARDebtorGroup);
			AssertEquals(creditorGroup.PK, orgCompanyDataForOtherCompany.OB_OG_APCreditorGroup);
		}

		public void TestUpdateAPandARDetails()
		{
			Xsd.Organisation value = new Xsd.Organisation();
			value.OrganisationDetails = new Xsd.OrganisationDetail();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			GlbCompany otherCompany = context.Factory.New<GlbCompany>();
			otherCompany.GC_Code = "OTH";

			value.EDICode = "edicode";
			value.OrganisationDetails.Name = "splaty";

			Xsd.AccountsReceivable receivable1 = value.OrganisationDetails.AccountsReceivables.AddNew();

			receivable1.CreditLimit = 12.12f;

			Xsd.AccountsPayable payable1 = value.OrganisationDetails.AccountsPayables.AddNew();

			payable1.CreditLimit = 10.10f;

			OrgHeader importToOrganisation = Factory.New<OrgHeader>();

			ZGuid debtorGroupPK = ZGuid.NewZGuid();
			ZGuid creditorGroupPK = ZGuid.NewZGuid();

			importToOrganisation.CompanyData.OB_OJ_ARDebtorGroup = debtorGroupPK;
			importToOrganisation.CompanyData.OB_OG_APCreditorGroup = creditorGroupPK;

			DataAdapter.ImportFromValueObject(importToOrganisation, value, context);

			AssertEquals(true, importToOrganisation.OH_IsDebtor);
			AssertEquals((decimal)12.12, importToOrganisation.CompanyData.OB_ARCreditLimit);
			AssertEquals(debtorGroupPK, importToOrganisation.CompanyData.OB_OJ_ARDebtorGroup);

			AssertEquals(true, importToOrganisation.OH_IsCreditor);
			AssertEquals((decimal)10.10, importToOrganisation.CompanyData.OB_APCreditLimit);
			AssertEquals(creditorGroupPK, importToOrganisation.CompanyData.OB_OG_APCreditorGroup);
		}

		public void TestEmptyOrganisationCodeForLegacyCodeMatching()
		{
			Xsd.Organisation xsdOrg = new Xsd.Organisation();
			Xsd.XmlInterchange interchange = new Xsd.XmlInterchange();
			xsdOrg.EDICode = "";
			xsdOrg.OrganisationDetails.Name = "-";
			xsdOrg.OrganisationDetails.Location.City = "";
			xsdOrg.OrganisationDetails.Location.Value = "";

			Xsd.OrgAddress address = xsdOrg.OrganisationDetails.Addresses.AddNew();
			address.AddressLine1 = "1 Address";

			Xsd.RegistrationNumber legecyNumber = xsdOrg.OrganisationDetails.RegistrationNumbers.AddNew();
			legecyNumber.NumberType = Xsd.RegistrationNumberTypes.LSC;
			legecyNumber.Number = "legecy number";
			legecyNumber.CountryOfRegistration = "AU";

			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, interchange, buffer);
			SystemDataRegistry.Instance.OrganisationMatching = OrganisationImportMatchingType.LegacyCodeMatching;
			BusinessObject bizO = DataAdapter.CreateOrUpdateFromValueObject(xsdOrg, context);

			AssertContains("Error Notification produced", Res.GetString("b7469631-aa20-4f2b-880b-21f421a358f9", "ERROR: Organization cannot be imported. Organization Code cannot be generated. Organization Name : '{0}'", "-"), buffer.AsString);
		}

		public void TestEmptyLegacyCodeForLegacyCodeMatching()
		{
			Xsd.Organisation xsdOrg = new Xsd.Organisation();
			Xsd.XmlInterchange interchange = new Xsd.XmlInterchange();
			xsdOrg.EDICode = "edicode";
			xsdOrg.OrganisationDetails.Name = "organisation";
			xsdOrg.OrganisationDetails.Location.City = "";
			xsdOrg.OrganisationDetails.Location.Value = "AUSYD";

			Xsd.OrgAddress address = xsdOrg.OrganisationDetails.Addresses.AddNew();
			address.AddressLine1 = "1 Address";

			Xsd.RegistrationNumber legecyNumber = xsdOrg.OrganisationDetails.RegistrationNumbers.AddNew();
			legecyNumber.NumberType = Xsd.RegistrationNumberTypes.LSC;
			legecyNumber.Number = "";
			legecyNumber.CountryOfRegistration = "AU";

			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, interchange, buffer);
			SystemDataRegistry.Instance.OrganisationMatching = OrganisationImportMatchingType.LegacyCodeMatching;
			BusinessObject bizO = DataAdapter.CreateOrUpdateFromValueObject(xsdOrg, context);

			AssertContains("Error Notification produced", Res.GetString("da91a509-dfd2-4c8f-9f07-c254c3ad2cc5", "Organization Legacy Code is empty for Organization Name : '{0}'. No data has been imported.", "organisation"), buffer.AsString);
		}

		public void TestEmptyOrganisationCodeForEnterpriseCodeMatching()
		{
			Xsd.Organisation xsdOrg = new Xsd.Organisation();
			Xsd.XmlInterchange interchange = new Xsd.XmlInterchange();

			xsdOrg.EDICode = "";
			xsdOrg.OrganisationDetails.Name = "-";
			xsdOrg.OrganisationDetails.Location.City = "";
			xsdOrg.OrganisationDetails.Location.Value = "";

			Xsd.OrgAddress address = xsdOrg.OrganisationDetails.Addresses.AddNew();
			address.AddressLine1 = "1 Address";

			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, interchange, buffer);
			SystemDataRegistry.Instance.OrganisationMatching = OrganisationImportMatchingType.OrganisationCodeMatching;
			BusinessObject bizO = DataAdapter.CreateOrUpdateFromValueObject(xsdOrg, context);

			AssertContains("Error Notification produced", Res.GetString("0b215ecf-75f6-4153-a4cb-7d0bca983089", "Organization {0} Code is empty for Organization Name : '{1}'. No data has been imported.", Core.Constants.ProductName, "-"), buffer.AsString);
		}

		public void TestFindBusinessObject()
		{
			Xsd.Organisation value = new Xsd.Organisation();
			value.OrganisationDetails = new Xsd.OrganisationDetail();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			value.EDICode = "edicode";
			value.OrganisationDetails.Name = "splaty";
			Xsd.RegistrationNumber legecyNumber = value.OrganisationDetails.RegistrationNumbers.AddNew();
			legecyNumber.NumberType = Xsd.RegistrationNumberTypes.LSC;
			legecyNumber.Number = "legecy number";
			legecyNumber.CountryOfRegistration = "AU";

			OrgHeader importToMach1 = context.Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader importToMach2 = context.Factory.NewWithValidTestData<OrgHeader>();

			OrgCusCode legecyCode = importToMach1.CustomsCodes.AddNew();
			legecyCode.OK_CodeType = "LSC";
			legecyCode.OK_CustomsRegNo = "legecy number";
			legecyCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			importToMach2.OH_Code = "edicode";

			context.Factory.Save();

			SystemDataRegistry.Instance.OrganisationMatching = OrganisationImportMatchingType.LegacyCodeMatching;
			AssertEquals(importToMach1, DataAdapter.FindBusinessObject(value, context));

			SystemDataRegistry.Instance.OrganisationMatching = OrganisationImportMatchingType.OrganisationCodeMatching;
			AssertEquals(importToMach2, DataAdapter.FindBusinessObject(value, context));
		}

		StandardManualAndBatchImportOrganisationValueObjectDataAdapterTestClass DataAdapter;

		protected override void SetUp()
		{
			base.SetUp();
			DataAdapter = new StandardManualAndBatchImportOrganisationValueObjectDataAdapterTestClass();
		}

		class StandardManualAndBatchImportOrganisationValueObjectDataAdapterTestClass : StandardManualAndBatchImportOrganisationValueObjectDataAdapter
		{
			public StandardManualAndBatchImportOrganisationValueObjectDataAdapterTestClass()
				: base()
			{ }

			public new OrgHeader FindBusinessObject(Xsd.Organisation value, IValueObjectImportContext context)
			{
				return base.FindBusinessObject(value, context);
			}
		}
	}
}
