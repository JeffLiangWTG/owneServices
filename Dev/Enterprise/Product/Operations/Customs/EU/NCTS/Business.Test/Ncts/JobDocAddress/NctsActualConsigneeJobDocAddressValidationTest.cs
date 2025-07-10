using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsActualConsigneeJobDocAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new NctsActualConsigneeJobDocAddressValidation(Factory.New<JobDocAddress>(), null));
		}

		public void TestCheckRuleTR0021ForOrg()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);

			var jobDocAddress = Factory.New<JobDocAddress>();
			var propertyInfo = jobDocAddress.OrganisationPKInfo;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var validation = new NctsActualConsigneeJobDocAddressValidation(jobDocAddress, sendingObject);

			var error = "[TR0021] Actual consignee or actual office of destination must be entered if query information is filled in. ";
			using (var deciderTestContext = new NctsHeaderMessageSendingObjectValidationDeciderTestContext<INctsHeaderMessageSendingObjectValidationDecider>(Factory))
			{
				deciderTestContext.DisableRule(x => x.IsRuleTR0021Active);
				sendingObject.QueryInformation = "Some query info";
				sendingObject.ActualOfficeOfDestination = "";
				validation.ValidateOrganisationPK();
				AssertNoMessageError("When rule TR0021 is disabled and query info has been entered but no actual destination office or actual consignee", propertyInfo, error);

				deciderTestContext.EnableRule(x => x.IsRuleTR0021Active);
				CombineAssertions("When rule TR0021 is Enabled", () =>
				{
					sendingObject.QueryInformation = "Some query info";
					sendingObject.ActualOfficeOfDestination = "";
					validation.ValidateOrganisationPK();
					AssertHasMessageError("When query info has been entered but no actual destination office or actual consignee", propertyInfo, error);
					validation.ValidateOrganisationPK();
					sendingObject.ActualOfficeOfDestination = "BEANR100";
					validation.ValidateOrganisationPK();
					AssertNoMessageError("When query info and actual destination office are entered but no actual consignee", propertyInfo, error);
					jobDocAddress.OrganisationPK = orgHeader.PK;
					sendingObject.ActualOfficeOfDestination = ZString.Empty;
					validation.ValidateOrganisationPK();
					AssertNoMessageError("When query info and actual consignee are entered but no actual departure office", propertyInfo, error);
				});
			}
		}

		public void TestCheckRuleC0215()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);

			var jobDocAddress = Factory.New<JobDocAddress>();
			var propertyInfo = jobDocAddress.OrganisationPKInfo;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var validation = new NctsActualConsigneeJobDocAddressValidation(jobDocAddress, sendingObject);

			var error = "[C0215] You have not selected Actual Consignee.";
			CombineAssertions(() =>
			{
				using (NctsConfigurationTestHelper.TemporarilySetValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleC0215Active), value: false))
				{
					sendingObject.ActualOfficeOfDestination = "";
					sendingObject.AdditionalText = "sd";
					validation.ValidateOrganisationPK();
					AssertNoMessageError("The RuleC0215 is inactive", propertyInfo, error);
				}

				using (NctsConfigurationTestHelper.TemporarilySetValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleC0215Active), value: true))
				{
					sendingObject.ActualOfficeOfDestination = ZString.Empty;
					sendingObject.AdditionalText = "sd";
					validation.ValidateOrganisationPK();
					AssertHasMessageError("The RuleC0215 is active and Additional Text gets value but Actual Office of Destinantion is null", propertyInfo, error);

					sendingObject.ActualOfficeOfDestination = "BEANR100";
					validation.ValidateOrganisationPK();
					AssertNoMessageError("The Actual Office Of Destination is not null", propertyInfo, error);

					sendingObject.ActualOfficeOfDestination = ZString.Empty;
					sendingObject.AdditionalText = ZString.Empty;
					validation.ValidateOrganisationPK();
					AssertNoMessageError("The additional text is not null", propertyInfo, error);
				}
			});
		}
	}
}
