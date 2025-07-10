using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class CancelNCTS5SendMessageWrapperTest : WrapperHelperTest<CancelNCTS5SendMessageWrapper>
	{
		public void TestConstructor()
		{
			reasonForCancellation = ZString.Empty;
			AssertExceptionThrown("Constructor Throws Exception if ReasonForCancellation is empty string", typeof(ArgumentException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be empty string (\"\").", "reasonForCancellation"), () => GetWrapper(nctsHeader));
		}

		public void TestTransitOperation()
		{
			var transitOperation = wrapper.TransitOperation;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled TransitOperation", transitOperation);
				AssertSame("Cached TransitOperation", wrapper.TransitOperation, transitOperation);
			});
		}

		public void TestInvalidation()
		{
			var invalidation = wrapper.Invalidation;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Invalidation", invalidation);
				AssertSame("Cached Invalidation", wrapper.Invalidation, invalidation);
			});
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

		public void TestNullHolderOfTheTransitProcedure()
		{
			AssertExceptionThrown<NullReferenceException>(() => wrapper.HolderOfTheTransitProcedure.ToString());
		}

		public void TestHolderOfTheTransitProcedure()
		{
			CombineAssertions(() =>
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				nctsHeader.Principal.OrganisationPK = orgHeader.PK;
				wrapper = GetWrapper(nctsHeader);
				var holderOfTheTransitProcedure = wrapper.HolderOfTheTransitProcedure;

				AssertNotNull("Expected filled HolderOfTheTransitProcedure", holderOfTheTransitProcedure);
				AssertSame("Cached HolderOfTheTransitProcedure", wrapper.HolderOfTheTransitProcedure, holderOfTheTransitProcedure);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			wrapper = GetWrapper(nctsHeader);
		}

		NctsHeader nctsHeader;
		CancelNCTS5SendMessageWrapper wrapper;
		ZString reasonForCancellation = "ReasonForCancellation";

		CancelNCTS5SendMessageWrapper GetWrapper(NctsHeader header) => new CancelNCTS5SendMessageWrapper(header, Certificate, reasonForCancellation);

		protected override CancelNCTS5SendMessageWrapper GetProvider() => wrapper;
	}
}
