using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class TIRSendMessageWrapperTest : WrapperHelperTest<TIRSendMessageWrapper>
	{
		public void TestCountryOfOrigin()
		{
			nctsHeader.BH_RL_NKImportLoadPort = HeaderDataNCTS.OriginCountry;
			AssertEquals("Expected filled CountryOfOrigin", HeaderDataNCTS.OriginCountry, wrapper.CountryOfOrigin);
		}

		public void TestCustomsOfficeOfTransit()
		{
			CombineAssertions(() =>
			{
				nctsHeader.DestinationCustomsOfficeCodeForDeparture = HeaderDataNCTS.DestinationCustomsOffice;
				var customsOfficeOfTransit = wrapper.CustomsOfficeOfTransit;

				AssertNotNull("Expected not null CustomsOfficeOfTransit", customsOfficeOfTransit);
				AssertSame("Cached CustomsOfficeOfTransit", customsOfficeOfTransit, wrapper.CustomsOfficeOfTransit);
			});
		}

		public void TestTIRCarnetNumber()
		{
			nctsHeader.MovementHeader.TirCarnetNumber = HeaderDataNCTS.TIRNumber;
			AssertEquals(HeaderDataNCTS.TIRNumber, wrapper.TIRCarnetNumber);
		}

		public void TestTIRCarnetExpiryDate()
		{
			var expiryDate = new ZDateTime(2020, 7, 28);
			nctsHeader.MovementHeader.TirCarnetExpiryDate = expiryDate;
			AssertEquals(expiryDate, wrapper.TIRCarnetExpiryDate);
		}

		public void TestLoadingTransport()
		{
			nctsHeader.MovementHeader.BM_TransportAtDeparture = NctsTransportData1.Id;
			nctsHeader.MovementHeader.BM_RN_NKTransportAtDepartureCountry = NctsTransportData1.Nationality;
			var loadingTransport = wrapper.LoadingTransport;

			CombineAssertions(() =>
			{
				AssertNotNull("LoadingTransport wrapped", loadingTransport);
				AssertSame("Cached LoadingTransport", wrapper.LoadingTransport, loadingTransport);
			});
		}

		public void TestNullHolder()
		{
			AssertExceptionThrown<NullReferenceException>(() => wrapper.Holder.ToString());
		}

		public void TestHolder()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			nctsHeader.Principal.OrganisationPK = orgHeader.PK;
			var holder = wrapper.Holder;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Holder", holder);
				AssertSame("Cached Holder", wrapper.Holder, holder);
			});
		}

		public void TestLines()
		{
			CombineAssertions(() =>
			{
				nctsHeader.MovementHeader.TirCarnetNumber = HeaderDataNCTS.TIRNumber;
				AssertEquals("Expected 1 Line (mandatory at least one)", 1, wrapper.Lines.Count);

				nctsHeader.Bills[0].GoodsItems.AddNew();
				nctsHeader.Bills[0].GoodsItems.AddNew();

				wrapper = new TIRSendMessageWrapper(nctsHeader, Certificate);

				var lines = wrapper.Lines;

				AssertEquals("Expected 3 Lines", 3, lines.Count);
				AssertSame("Cached Lines", wrapper.Lines, lines);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			wrapper = new TIRSendMessageWrapper(nctsHeader, Certificate);
		}

		NctsHeader nctsHeader;
		TIRSendMessageWrapper wrapper;

		protected override TIRSendMessageWrapper GetProvider() => wrapper;
	}
}
