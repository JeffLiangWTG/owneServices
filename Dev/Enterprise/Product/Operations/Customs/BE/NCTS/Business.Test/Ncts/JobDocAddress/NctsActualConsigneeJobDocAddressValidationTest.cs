using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

sealed class NctsActualConsigneeJobDocAddressValidationTest : BusinessObjectValidationTestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new NctsActualConsigneeJobDocAddressValidation(Factory.New<JobDocAddress>(), null));
	}

	public void TestCheckRuleTR0021_EntryTypeRNM()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		var sendingObject = new MessageSendingAction(nctsHeader.MovementHeader);

		sendingObject.EntryType = NctsMessageTypeList.Codes.ResponseOnRequestForNonArrivedMovement;

		var jobDocAddress = Factory.New<JobDocAddress>();
		var propertyInfo = jobDocAddress.OrganisationPKInfo;

		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var validation = new NctsActualConsigneeJobDocAddressValidation(jobDocAddress, sendingObject);

		CombineAssertions(() =>
		{
			sendingObject.QueryInformation = "Some query info";
			sendingObject.ActualOfficeOfDestination = "";
			validation.ValidateOrganisationPK();
			AssertHasError("When query info has been entered but no actual destination office or actual consignee", propertyInfo, NctsHeaderValidationHelper.TR0021ValidationMessage);
			validation.ValidateOrganisationPK();
			sendingObject.ActualOfficeOfDestination = "BEANR100";
			validation.ValidateOrganisationPK();
			AssertNoError("When query info and actual destination office are entered but no actual consignee", propertyInfo, NctsHeaderValidationHelper.TR0021ValidationMessage);
			jobDocAddress.OrganisationPK = orgHeader.PK;
			sendingObject.ActualOfficeOfDestination = ZString.Empty;
			validation.ValidateOrganisationPK();
			AssertNoError("When query info and actual consignee are entered but no actual departure office", propertyInfo, NctsHeaderValidationHelper.TR0021ValidationMessage);
		});
	}
}
