using System;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	class NCTS5DepartureAndNotificationCommonSendMessageWrapperTest : WrapperHelperTest<NCTS5DepartureAndNotificationCommonSendMessageWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if MovementHeader is null", typeof(ArgumentNullException),
			ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "MovementHeader"), () => GetWrapper(Factory.New<NctsHeader>()));
		}

		public void TestCustomsOfficeOfDeparture()
		{
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.CustomsOffices.RemoveAndDeleteAll();

			var customsOffice1 = movementHeader.CustomsOffices.AddNew();
			customsOffice1.CY_Code = EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
			customsOffice1.CY_Data = "FR008889";

			var customsOffice2 = movementHeader.CustomsOffices.AddNew();
			customsOffice2.CY_Code = EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination;
			customsOffice2.CY_Data = "DE000002";

			wrapper = GetWrapper(nctsHeader);
			AssertEquals("Expected filled CustomsOfficeOfDeparture with NCTSOfficeOfDeparture office code when declared in CustomsOffice list", "FR008889", wrapper.CustomsOfficeOfDeparture);
		}

		public void TestNullRepresentative()
		{
			AssertExceptionThrown<NullReferenceException>(() => wrapper.Representative.ToString());
		}

		public void TestRepresentative()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			nctsHeader.MovementHeader.Representative.OrganisationPK = orgHeader.PK;

			wrapper = GetWrapper(nctsHeader);
			var representative = wrapper.Representative;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Representative", wrapper.Representative);
				AssertSame("Cached Representative", wrapper.Representative, representative);

				nctsHeader.Principal.OrganisationPK = orgHeader.PK;
				wrapper = GetWrapper(nctsHeader);
				AssertNull("Expected null Representative when its the same as the Principal", wrapper.Representative);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			wrapper = GetWrapper(nctsHeader);
		}

		NctsHeader nctsHeader;
		NCTS5DepartureAndNotificationCommonSendMessageWrapper wrapper;

		NCTS5DepartureAndNotificationCommonSendMessageWrapper GetWrapper(NctsHeader nctsHeader) => new NCTS5DepartureAndNotificationCommonSendMessageWrapper(nctsHeader, Certificate);

		protected override NCTS5DepartureAndNotificationCommonSendMessageWrapper GetProvider() => wrapper;
	}
}
