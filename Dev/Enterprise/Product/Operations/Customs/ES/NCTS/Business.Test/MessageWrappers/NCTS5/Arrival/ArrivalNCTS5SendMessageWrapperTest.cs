using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class ArrivalNCTS5SendMessageWrapperTest : WrapperHelperTest<ArrivalNCTS5SendMessageWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if ArrivalMovementHeader is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "ArrivalMovementHeader"), () => GetWrapper(Factory.New<NctsHeader>()));
			});
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

		public void TestCustomsOfficeOfDestinationActual()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled empty in DestinationCustomsOfficeCodeForArrival", ZString.Empty, wrapper.CustomsOfficeOfDestinationActual);
				nctsHeader.ArrivalMovementHeader.DestinationCustomsOfficeCodeForArrival = "FR008889";
				wrapper = GetWrapper(nctsHeader);
				AssertEquals("Expected filled DestinationCustomsOfficeCodeForArrival", "FR008889", wrapper.CustomsOfficeOfDestinationActual);
			});
		}

		public void TestTraderAtDestination()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			nctsHeader.DestinationTrader.OrganisationPK = orgHeader.PK;

			wrapper = GetWrapper(nctsHeader);
			var representative = wrapper.TraderAtDestination;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Destination Trader", wrapper.TraderAtDestination);
				AssertSame("Cached Destination Trader", wrapper.TraderAtDestination, representative);
			});
		}

		public void TestNullTraderAtDestination()
		{
			AssertExceptionThrown<NullReferenceException>(() => wrapper.TraderAtDestination.ToString());
		}

		public void TestRepresentativeAtDestination()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			nctsHeader.ArrivalMovementHeader.Representative.OrganisationPK = orgHeader.PK;

			wrapper = GetWrapper(nctsHeader);
			var representative = wrapper.RepresentativeAtDestination;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Representative", wrapper.RepresentativeAtDestination);
				AssertSame("Cached Representative", wrapper.RepresentativeAtDestination, representative);
			});
		}

		public void TestNullRepresentativeAtDestination()
		{
			AssertExceptionThrown<NullReferenceException>(() => wrapper.RepresentativeAtDestination.ToString());
		}

		public void TestIndicators()
		{
			var indicators = wrapper.Indicators;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Indicators", indicators);
				AssertSame("Cached Indicators", wrapper.Indicators, indicators);
			});
		}

		public void TestConsignment()
		{
			var consignment = wrapper.Consignment;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Consignment", consignment);
				AssertSame("Cached Consignment", wrapper.Consignment, consignment);
			});
		}

		public void TestAuthorisations()
		{
			CombineAssertions(() =>
			{
				var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
				var eun = helper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
				_ = helper.CreateCusCodeType("AUTH", "Authorisation");
				_ = helper.CreateCusCodeList("EUN", "AUTH", "SAS", "Self-Assessment", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
				_ = helper.CreateCusMapType("EUNAU", "OUT", "Authorisation Codes to Document Type Code", isReadonly: true);
				_ = helper.CreateCusMap("EUNAU", "ACT", "C520", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "EUN");
				Factory.Save();
				AssertEquals("Expected empty Authorisations", 0, wrapper.Authorisations.Count);

				var auth1 = nctsHeader.CusAuthorizationUsages.AddNew();
				auth1.AGC_Code = "ACT";
				var auth2 = nctsHeader.CusAuthorizationUsages.AddNew();
				auth2.AGC_Code = "AAA";

				wrapper = new ArrivalNCTS5SendMessageWrapper(nctsHeader, Certificate);
				var authorisations = wrapper.Authorisations;

				AssertEquals("Expected filled Authorisations with count 2 (all expected codes)", 2, authorisations.Count);
				AssertSame("Cached Authorisations", wrapper.Authorisations, authorisations);

				AssertEquals("Expected filled Authorisations in position 0", "C520", authorisations.ToArray()[0].Type);
				AssertEquals("Expected filled Authorisations in position 1", "AAA", authorisations.ToArray()[1].Type);
				auth1.AGC_Code = "ACE";
				wrapper = new ArrivalNCTS5SendMessageWrapper(nctsHeader, Certificate);
				authorisations = wrapper.Authorisations;
				AssertEquals("Expected filled Authorisations in position 0", "ACE", authorisations.ToArray()[0].Type);
				AssertEquals("Expected filled Authorisations in position 1", "AAA", authorisations.ToArray()[1].Type);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			wrapper = GetWrapper(nctsHeader);
		}

		NctsHeader nctsHeader;
		ArrivalNCTS5SendMessageWrapper wrapper;

		ArrivalNCTS5SendMessageWrapper GetWrapper(NctsHeader header) => new ArrivalNCTS5SendMessageWrapper(header, Certificate);

		protected override ArrivalNCTS5SendMessageWrapper GetProvider() => wrapper;
	}
}
