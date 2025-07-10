using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EdiCommissionAgreement))]
	public class EdiCommissionAgreementTest : EnterpriseBusinessObjectTestCase
	{
		#region Draft

		public void TestCreateDraft_ShouldNotSetHasChanges()
		{
			var company = Factory.NewWithValidTestData<ClientCompany>();
			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			var agreement = Factory.NewWithValidTestData<EdiCommissionAgreement>();
			var customization = agreement.GetOrCreateCustomization();
			customization.CompanyPivots.AddNew(company);
			customization.CompanyAutoAddDatabases.AddNew(database);

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var agreementInOtherFactory = anotherFactory.Load<EdiCommissionAgreement>(agreement.PK);
			var draft = agreementInOtherFactory.CreateDraft();
			AssertEquals(false, draft.HasChanges);
		}

		#endregion

		#region Customization

		public void TestGetOrCreateCustomization()
		{
			var agreement = Factory.New<EdiCommissionAgreement>();

			var customization = agreement.GetOrCreateCustomization();
			AssertEquals(customization.EZN_CA0, agreement.PK);

			AssertEquals(customization, agreement.GetOrCreateCustomization());

			customization.Delete();

			var customization2 = agreement.GetOrCreateCustomization();
			AssertNotEquals(customization, customization2);

			var query = new ZQuery(EdiCommissionAgreementCustomizationSchema.EZN_CA0, agreement.PK);
			AssertContainsExactElementsInAnyOrder(
				new[] { customization2 },
				Factory.Load<EdiCommissionAgreementCustomization>(query));
		}

		public void TestCustomization_ShouldNotSetHasChangesOnLoad()
		{
			var agreement = Factory.NewWithValidTestData<EdiCommissionAgreement>();
			agreement.GetOrCreateCustomization();

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var agreementInOtherFactory = anotherFactory.Load<EdiCommissionAgreement>(agreement.PK);
			var customizationInOtherFactory = agreementInOtherFactory.Customization;
			AssertEquals(false, customizationInOtherFactory.HasChanges);
			AssertEquals(false, agreementInOtherFactory.HasChanges);
		}

		#endregion

		#region Delete

		public void TestDelete()
		{
			var commissionAgreement = Factory.New<EdiCommissionAgreement>();
			var customization = commissionAgreement.GetOrCreateCustomization();

			commissionAgreement.Delete();

			AssertEquals(true, customization.IsDeleted);
		}

		#endregion

		#region AYC Trigger Type

		public void TestEffectiveDateForAYCTriggerType()
		{
			var primaryCharge = BillingTestHelper.CreateChargeCode(Factory, null, "PRIMARY");
			primaryCharge.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			primaryCharge.AC_ChargeGroup = ChargeCodeGroupList.Codes.NonJobRelated;

			var secondaryCharge = BillingTestHelper.CreateChargeCode(Factory, null, "SECOND");
			secondaryCharge.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			secondaryCharge.AC_ChargeGroup = ChargeCodeGroupList.Codes.NonJobRelated;

			var anotherCharge = BillingTestHelper.CreateChargeCode(Factory, null, "ANOTHER");
			anotherCharge.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			anotherCharge.AC_ChargeGroup = ChargeCodeGroupList.Codes.NonJobRelated;

			Factory.Save();

			var settings = new AYCTriggerTypeSettings();
			settings.PrimaryChargeCode = primaryCharge.AC_Code;
			settings.SecondaryChargeCode = secondaryCharge.AC_Code;
			EDIDataRegistry.Instance.AYCTriggerTypeSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			var organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var fee1 = BillingTestHelper.CreateLicenceFee(organisation.LicCompany, "AAA", 10m);
			fee1.L8_ChargeCode = anotherCharge.AC_Code;
			fee1.L8_StartDate = new ZDateTime(2020, 1, 1);

			Factory.Save();

			var agreement = Factory.NewWithValidTestData<EdiCommissionAgreement>();
			agreement.CA0_OH_Customer = organisation.PK;
			agreement.CA0_CommissionTriggerType = EDICommissionTriggerTypes.Codes.AYCEStartDate;

			AssertEquals("No Primary or Secondary charge", ZDate.Empty, agreement.EffectiveDate);

			var fee2 = BillingTestHelper.CreateLicenceFee(organisation.LicCompany, "AAA", 20m);
			fee2.L8_ChargeCode = secondaryCharge.AC_Code;
			fee2.L8_StartDate = new ZDateTime(2020, 1, 2);

			var fee3 = BillingTestHelper.CreateLicenceFee(organisation.LicCompany, "AAA", 30m);
			fee3.L8_ChargeCode = secondaryCharge.AC_Code;
			fee3.L8_StartDate = new ZDateTime(2020, 1, 3);

			Factory.Save();

			AssertEquals("Secondary charge but no primary", new ZDate(2020, 1, 2), agreement.EffectiveDate);

			var fee4 = BillingTestHelper.CreateLicenceFee(organisation.LicCompany, "AAA", 40m);
			fee4.L8_ChargeCode = primaryCharge.AC_Code;
			fee4.L8_StartDate = ZDateTime.Empty;

			Factory.Save();

			AssertEquals("Secondary charge and only primary has no Start Date", new ZDate(2020, 1, 2), agreement.EffectiveDate);

			var fee5 = BillingTestHelper.CreateLicenceFee(organisation.LicCompany, "AAA", 40m);
			fee5.L8_ChargeCode = primaryCharge.AC_Code;
			fee5.L8_StartDate = new ZDateTime(2020, 2, 2);

			var fee6 = BillingTestHelper.CreateLicenceFee(organisation.LicCompany, "AAA", 50m);
			fee6.L8_ChargeCode = secondaryCharge.AC_Code;
			fee6.L8_StartDate = new ZDateTime(2020, 2, 3);

			Factory.Save();

			AssertEquals("Primary Charge", new ZDate(2020, 2, 2), agreement.EffectiveDate);
		}

		#endregion

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			var bizObj = base.GetNewBusinessObjectForSettingValueCallsRefreshBindingTest() as EdiCommissionAgreement;
			bizObj.CA0_OH_Customer = ZGuid.NewZGuid();
			return bizObj;
		}
	}
}
