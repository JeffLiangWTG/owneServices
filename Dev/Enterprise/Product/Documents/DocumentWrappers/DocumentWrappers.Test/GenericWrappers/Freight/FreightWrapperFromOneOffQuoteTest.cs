using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromOneOffQuote))]
	sealed class FreightWrapperFromOneOffQuoteTest : FreightWrapperTest
	{
		#region Charges, Non Carrier Charges, Carrier Charges

		public void TestCarrierAndNonCarrierCharges()
		{
			var chargeCodeFactory = new TestHelper.ChargeCodeFactory(Factory);
			var testObjectCreator = new TestObjectCreator(Factory);

			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier3 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier4 = Factory.NewWithValidTestData<OrgHeader>();

			var booking = QuotedBooking.New(WrappedBO.PK, ZGuid.Empty, Factory);

			booking.OH_Carrier = carrier4.PK;
			booking.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier1.PK;
			booking.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier2.PK;

			var localClient = Factory.New<OrgHeader>();
			localClient.OH_Code = "Microsoft";
			localClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 2);

			var accChargeCode1 = chargeCodeFactory.New("111", "Charge Code 111 Description", FlatCalculator.Code, showOnQuotation: true, suppressIfZero: false);
			var accChargeCode2 = chargeCodeFactory.New("222", "Charge Code 222 Description", FlatCalculator.Code, showOnQuotation: true, suppressIfZero: true);
			var accChargeCode3 = chargeCodeFactory.New("333", "Charge Code 333 Description", FlatCalculator.Code, showOnQuotation: false, suppressIfZero: false);
			var accChargeCode4 = chargeCodeFactory.New("444", "Charge Code 444 Description", FlatCalculator.Code, showOnQuotation: false, suppressIfZero: true);
			accChargeCode4.AC_ChargeType = "CMT";
			var accChargeCode5 = chargeCodeFactory.New("555", "Charge Code 555 Description", FlatCalculator.Code, showOnQuotation: true, suppressIfZero: true);
			var accChargeCode6 = chargeCodeFactory.New("666", "Charge Code 666 Description", FlatCalculator.Code, showOnQuotation: true, suppressIfZero: true);
			var accChargeCode7 = chargeCodeFactory.New("777", "Charge Code 777 Description", FlatCalculator.Code, showOnQuotation: true, suppressIfZero: true);

			var job = NewJob(booking, jobNum: "123", localClient);

			// possible carriers
			CreateCharge(testObjectCreator, job, accChargeCode1, osSellAmt: 10m, creditor: carrier1, sequence: 1, orgHeader: localClient);
			CreateCharge(testObjectCreator, job, accChargeCode1, osSellAmt: 0m, creditor: carrier2, sequence: 2, orgHeader: localClient);
			CreateCharge(testObjectCreator, job, accChargeCode2, osSellAmt: 20m, creditor: carrier1, sequence: 3, orgHeader: localClient);
			CreateCharge(testObjectCreator, job, accChargeCode2, osSellAmt: 0m, creditor: carrier2, sequence: 4, orgHeader: localClient);
			CreateCharge(testObjectCreator, job, accChargeCode3, osSellAmt: 30m, creditor: carrier1, sequence: 5, orgHeader: localClient);
			CreateCharge(testObjectCreator, job, accChargeCode3, osSellAmt: 0m, creditor: carrier2, sequence: 6, orgHeader: localClient);
			CreateCharge(testObjectCreator, job, accChargeCode4, osSellAmt: 40m, creditor: carrier1, sequence: 7, orgHeader: localClient);
			CreateCharge(testObjectCreator, job, accChargeCode4, osSellAmt: 0m, creditor: carrier2, sequence: 8, orgHeader: localClient);

			// not possible carriers
			CreateCharge(testObjectCreator, job, accChargeCode5, osSellAmt: 50m, creditor: carrier3, sequence: 9, orgHeader: localClient);

			// no creditor
			CreateCharge(testObjectCreator, job, accChargeCode6, osSellAmt: 60m, creditor: null, sequence: 10, orgHeader: localClient);

			// carrier
			CreateCharge(testObjectCreator, job, accChargeCode7, osSellAmt: 70m, creditor: carrier4, sequence: 11, orgHeader: localClient);

			Factory.Save();

			var wrapper = new FreightWrapperFromOneOffQuote(booking.Quote, Factory);

			AssertChargesString
			(
				wrapper.Charges,
				@"111: 10.00 ERN
111: 
222: 20.00 ERN
444: 40.00 ERN
444: 
555: 50.00 ERN
666: 60.00 ERN
777: 70.00 ERN",
				"Charges: should contain all charges");

			AssertChargesString
			(
				wrapper.NonCarrierCharges,
				@"555: 50.00 ERN
666: 60.00 ERN",
				"Non Carrier Charges: should contain non-carrier charges"
			);

			AssertChargesString
			(
				wrapper.CarrierCharges,
				@"111: 10.00 ERN
111: 
222: 20.00 ERN
444: 40.00 ERN
444: 
777: 70.00 ERN",
				"Carrier Charges: should only contain carrier charges");
		}

		public void TestCarrierAndNonCarrierCharges_EmptyCarrier()
		{
			var chargeCodeFactory = new TestHelper.ChargeCodeFactory(Factory);
			var testObjectCreator = new TestObjectCreator(Factory);

			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier3 = Factory.NewWithValidTestData<OrgHeader>();

			var booking = QuotedBooking.New(WrappedBO.PK, ZGuid.Empty, Factory);

			booking.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier1.PK;
			booking.Quote.CurrentOneOffQuote.PossibleCarriers.AddNew().TTC_OH_Carrier = carrier2.PK;

			var localClient = Factory.New<OrgHeader>();
			localClient.OH_Code = "Microsoft";
			localClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 2);

			var accChargeCode1 = chargeCodeFactory.New("111", "Charge Code 111 Description", FlatCalculator.Code, showOnQuotation: true, suppressIfZero: false);
			var accChargeCode2 = chargeCodeFactory.New("222", "Charge Code 222 Description", FlatCalculator.Code, showOnQuotation: true, suppressIfZero: true);
			var accChargeCode3 = chargeCodeFactory.New("333", "Charge Code 333 Description", FlatCalculator.Code, showOnQuotation: false, suppressIfZero: false);
			var accChargeCode4 = chargeCodeFactory.New("444", "Charge Code 444 Description", FlatCalculator.Code, showOnQuotation: false, suppressIfZero: true);
			accChargeCode4.AC_ChargeType = "CMT";
			var accChargeCode5 = chargeCodeFactory.New("555", "Charge Code 555 Description", FlatCalculator.Code, showOnQuotation: true, suppressIfZero: true);
			var accChargeCode6 = chargeCodeFactory.New("666", "Charge Code 666 Description", FlatCalculator.Code, showOnQuotation: true, suppressIfZero: true);

			var job = NewJob(booking, jobNum: "123", localClient);

			// possible carriers
			CreateCharge(testObjectCreator, job, accChargeCode1, osSellAmt: 10m, creditor: carrier1, sequence: 1, orgHeader: localClient);
			CreateCharge(testObjectCreator, job, accChargeCode1, osSellAmt: 0m, creditor: carrier2, sequence: 2, orgHeader: localClient);
			CreateCharge(testObjectCreator, job, accChargeCode2, osSellAmt: 20m, creditor: carrier1, sequence: 3, orgHeader: localClient);
			CreateCharge(testObjectCreator, job, accChargeCode2, osSellAmt: 0m, creditor: carrier2, sequence: 4, orgHeader: localClient);
			CreateCharge(testObjectCreator, job, accChargeCode3, osSellAmt: 30m, creditor: carrier1, sequence: 5, orgHeader: localClient);
			CreateCharge(testObjectCreator, job, accChargeCode3, osSellAmt: 0m, creditor: carrier2, sequence: 6, orgHeader: localClient);
			CreateCharge(testObjectCreator, job, accChargeCode4, osSellAmt: 40m, creditor: carrier1, sequence: 7, orgHeader: localClient);
			CreateCharge(testObjectCreator, job, accChargeCode4, osSellAmt: 0m, creditor: carrier2, sequence: 8, orgHeader: localClient);

			// not possible carriers
			CreateCharge(testObjectCreator, job, accChargeCode5, osSellAmt: 50m, creditor: carrier3, sequence: 9, orgHeader: localClient);

			// no creditor
			CreateCharge(testObjectCreator, job, accChargeCode6, osSellAmt: 60m, creditor: null, sequence: 10, orgHeader: localClient);

			Factory.Save();

			var wrapper = new FreightWrapperFromOneOffQuote(booking.Quote, Factory);

			AssertChargesString
			(
				wrapper.Charges,
				@"111: 10.00 ERN
111: 
222: 20.00 ERN
444: 40.00 ERN
444: 
555: 50.00 ERN
666: 60.00 ERN",
				"Charges: should contain all charges");

			AssertChargesString
			(
				wrapper.NonCarrierCharges,
				@"555: 50.00 ERN
666: 60.00 ERN",
				"Non Carrier Charges: should contain non-carrier charges"
			);

			AssertChargesString
			(
				wrapper.CarrierCharges,
				@"111: 10.00 ERN
111: 
222: 20.00 ERN
444: 40.00 ERN
444: ",
				"Carrier Charges: should only contain carrier charges");
		}

		public void TestCarrierAndNonCarrierCharges_EmptyPossibleCarriers()
		{
			var chargeCodeFactory = new TestHelper.ChargeCodeFactory(Factory);
			var testObjectCreator = new TestObjectCreator(Factory);

			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier3 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier4 = Factory.NewWithValidTestData<OrgHeader>();

			var booking = QuotedBooking.New(WrappedBO.PK, ZGuid.Empty, Factory);

			booking.OH_Carrier = carrier4.PK;

			var localClient = Factory.New<OrgHeader>();
			localClient.OH_Code = "Microsoft";
			localClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 2);

			var accChargeCode1 = chargeCodeFactory.New("111", "Charge Code 111 Description", FlatCalculator.Code, showOnQuotation: true, suppressIfZero: false);
			var accChargeCode2 = chargeCodeFactory.New("222", "Charge Code 222 Description", FlatCalculator.Code, showOnQuotation: true, suppressIfZero: true);
			var accChargeCode3 = chargeCodeFactory.New("333", "Charge Code 333 Description", FlatCalculator.Code, showOnQuotation: false, suppressIfZero: false);
			var accChargeCode4 = chargeCodeFactory.New("444", "Charge Code 444 Description", FlatCalculator.Code, showOnQuotation: false, suppressIfZero: true);
			accChargeCode4.AC_ChargeType = "CMT";
			var accChargeCode5 = chargeCodeFactory.New("555", "Charge Code 555 Description", FlatCalculator.Code, showOnQuotation: true, suppressIfZero: true);
			var accChargeCode6 = chargeCodeFactory.New("666", "Charge Code 666 Description", FlatCalculator.Code, showOnQuotation: true, suppressIfZero: true);
			var accChargeCode7 = chargeCodeFactory.New("777", "Charge Code 777 Description", FlatCalculator.Code, showOnQuotation: true, suppressIfZero: true);

			var job = NewJob(booking, jobNum: "123", localClient);

			// not possible carriers
			CreateCharge(testObjectCreator, job, accChargeCode1, osSellAmt: 10m, creditor: carrier1, sequence: 1, orgHeader: localClient);
			CreateCharge(testObjectCreator, job, accChargeCode1, osSellAmt: 0m, creditor: carrier2, sequence: 2, orgHeader: localClient);
			CreateCharge(testObjectCreator, job, accChargeCode2, osSellAmt: 20m, creditor: carrier1, sequence: 3, orgHeader: localClient);
			CreateCharge(testObjectCreator, job, accChargeCode2, osSellAmt: 0m, creditor: carrier2, sequence: 4, orgHeader: localClient);
			CreateCharge(testObjectCreator, job, accChargeCode3, osSellAmt: 30m, creditor: carrier1, sequence: 5, orgHeader: localClient);
			CreateCharge(testObjectCreator, job, accChargeCode3, osSellAmt: 0m, creditor: carrier2, sequence: 6, orgHeader: localClient);
			CreateCharge(testObjectCreator, job, accChargeCode4, osSellAmt: 40m, creditor: carrier1, sequence: 7, orgHeader: localClient);
			CreateCharge(testObjectCreator, job, accChargeCode4, osSellAmt: 0m, creditor: carrier2, sequence: 8, orgHeader: localClient);
			CreateCharge(testObjectCreator, job, accChargeCode5, osSellAmt: 50m, creditor: carrier3, sequence: 9, orgHeader: localClient);

			// no creditor
			CreateCharge(testObjectCreator, job, accChargeCode6, osSellAmt: 60m, creditor: null, sequence: 10, orgHeader: localClient);

			// carrier
			CreateCharge(testObjectCreator, job, accChargeCode7, osSellAmt: 70m, creditor: carrier4, sequence: 11, orgHeader: localClient);

			Factory.Save();

			var wrapper = new FreightWrapperFromOneOffQuote(booking.Quote, Factory);

			AssertChargesString
			(
				wrapper.Charges,
				@"111: 10.00 ERN
111: 
222: 20.00 ERN
444: 40.00 ERN
444: 
555: 50.00 ERN
666: 60.00 ERN
777: 70.00 ERN",
				"Charges: should contain all charges");

			AssertChargesString
			(
				wrapper.NonCarrierCharges,
				@"111: 10.00 ERN
111: 
222: 20.00 ERN
444: 40.00 ERN
444: 
555: 50.00 ERN
666: 60.00 ERN",
				"Non Carrier Charges: should contain non-carrier charges"
			);

			AssertChargesString
			(
				wrapper.CarrierCharges,
				@"777: 70.00 ERN",
				"Carrier Charges: should only contain carrier charges");
		}

		public void TestCarrierAndNonCarrierCharges_EmptyCarrier_EmptyPossibleCarriers()
		{
			var chargeCodeFactory = new TestHelper.ChargeCodeFactory(Factory);
			var testObjectCreator = new TestObjectCreator(Factory);

			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier3 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier4 = Factory.NewWithValidTestData<OrgHeader>();

			var booking = QuotedBooking.New(WrappedBO.PK, ZGuid.Empty, Factory);

			var localClient = Factory.New<OrgHeader>();
			localClient.OH_Code = "Microsoft";
			localClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 2);

			var accChargeCode1 = chargeCodeFactory.New("111", "Charge Code 111 Description", FlatCalculator.Code, showOnQuotation: true, suppressIfZero: false);
			var accChargeCode2 = chargeCodeFactory.New("222", "Charge Code 222 Description", FlatCalculator.Code, showOnQuotation: true, suppressIfZero: true);
			var accChargeCode3 = chargeCodeFactory.New("333", "Charge Code 333 Description", FlatCalculator.Code, showOnQuotation: false, suppressIfZero: false);
			var accChargeCode4 = chargeCodeFactory.New("444", "Charge Code 444 Description", FlatCalculator.Code, showOnQuotation: false, suppressIfZero: true);
			accChargeCode4.AC_ChargeType = "CMT";
			var accChargeCode5 = chargeCodeFactory.New("555", "Charge Code 555 Description", FlatCalculator.Code, showOnQuotation: true, suppressIfZero: true);
			var accChargeCode6 = chargeCodeFactory.New("666", "Charge Code 666 Description", FlatCalculator.Code, showOnQuotation: true, suppressIfZero: true);
			var accChargeCode7 = chargeCodeFactory.New("777", "Charge Code 777 Description", FlatCalculator.Code, showOnQuotation: true, suppressIfZero: true);

			var job = NewJob(booking, jobNum: "123", localClient);

			// not possible carriers
			CreateCharge(testObjectCreator, job, accChargeCode1, osSellAmt: 10m, creditor: carrier1, sequence: 1, orgHeader: localClient);
			CreateCharge(testObjectCreator, job, accChargeCode1, osSellAmt: 0m, creditor: carrier2, sequence: 2, orgHeader: localClient);
			CreateCharge(testObjectCreator, job, accChargeCode2, osSellAmt: 20m, creditor: carrier1, sequence: 3, orgHeader: localClient);
			CreateCharge(testObjectCreator, job, accChargeCode2, osSellAmt: 0m, creditor: carrier2, sequence: 4, orgHeader: localClient);
			CreateCharge(testObjectCreator, job, accChargeCode3, osSellAmt: 30m, creditor: carrier1, sequence: 5, orgHeader: localClient);
			CreateCharge(testObjectCreator, job, accChargeCode3, osSellAmt: 0m, creditor: carrier2, sequence: 6, orgHeader: localClient);
			CreateCharge(testObjectCreator, job, accChargeCode4, osSellAmt: 40m, creditor: carrier1, sequence: 7, orgHeader: localClient);
			CreateCharge(testObjectCreator, job, accChargeCode4, osSellAmt: 0m, creditor: carrier2, sequence: 8, orgHeader: localClient);
			CreateCharge(testObjectCreator, job, accChargeCode5, osSellAmt: 50m, creditor: carrier3, sequence: 9, orgHeader: localClient);

			// no creditor
			CreateCharge(testObjectCreator, job, accChargeCode6, osSellAmt: 60m, creditor: null, sequence: 10, orgHeader: localClient);

			// non carrier
			CreateCharge(testObjectCreator, job, accChargeCode7, osSellAmt: 70m, creditor: carrier4, sequence: 11, orgHeader: localClient);

			Factory.Save();

			var wrapper = new FreightWrapperFromOneOffQuote(booking.Quote, Factory);

			AssertChargesString
			(
				wrapper.Charges,
				@"111: 10.00 ERN
111: 
222: 20.00 ERN
444: 40.00 ERN
444: 
555: 50.00 ERN
666: 60.00 ERN
777: 70.00 ERN",
				"Charges: should contain all charges");

			AssertChargesString
			(
				wrapper.NonCarrierCharges,
				@"111: 10.00 ERN
111: 
222: 20.00 ERN
444: 40.00 ERN
444: 
555: 50.00 ERN
666: 60.00 ERN
777: 70.00 ERN",
				"Non Carrier Charges: should contain non-carrier charges"
			);

			AssertChargesString
			(
				wrapper.CarrierCharges,
				string.Empty,
				"Carrier Charges: should only contain carrier charges");
		}

		static Charge CreateCharge(TestObjectCreator testObjectCreator, Job job, AccChargeCode chargeCode, decimal osSellAmt, OrgHeader creditor, ZShort sequence, OrgHeader orgHeader)
		{
			var charge = testObjectCreator.CreateCharge(job, chargeCode, osSellAmt: osSellAmt, creditor: creditor);
			charge.JR_DisplaySequence = sequence;
			charge.JR_OH_SellAccount = orgHeader.PK;

			return charge;
		}

		Job NewJob(QuotedBooking booking, string jobNum, OrgHeader localClient)
		{
			booking.TryLoadOrCreateJob();
			var job = (Job)booking.Job;
			job.JH_JobNum = jobNum;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.LocalChargesPK = localClient.PK;

			return job;
		}

		void AssertChargesString(ChargeWrapperCollection charges, string expectedCharges, string assertMessage = default)
		{
			var builder = new StringBuilder();
			builder.AppendLine();

			foreach (ChargeWrapper chargeWrapper in charges)
			{
				builder.Append(chargeWrapper.ChargeCode.Code);
				builder.Append(": ");
				builder.Append(chargeWrapper.LocalSell);
				builder.AppendLine();
			}

			AssertMultilineASCIIEquals(assertMessage, expectedCharges, builder.ToString());
		}

		public void TestCharges()
		{
			QuotedBooking booking = QuotedBooking.New(WrappedBO.PK, ZGuid.Empty, Factory);

			var localClient = Factory.New<OrgHeader>();
			localClient.OH_Code = "Microsoft";
			localClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 2);

			AccChargeCode code1 = Factory.NewWithValidTestData<AccChargeCode>();
			code1.AC_Code = "AAA";
			code1.AC_ShowOnQuotation = true;
			code1.AC_SuppressOnQuoteIfZero = false;

			AccChargeCode code2 = Factory.NewWithValidTestData<AccChargeCode>();
			code2.AC_Code = "BBB";
			code2.AC_ShowOnQuotation = true;
			code2.AC_SuppressOnQuoteIfZero = true;

			AccChargeCode code3 = Factory.NewWithValidTestData<AccChargeCode>();
			code3.AC_Code = "CCC";
			code3.AC_ShowOnQuotation = false;

			AccChargeCode code4 = Factory.NewWithValidTestData<AccChargeCode>();
			code4.AC_Code = "DDD";
			code4.AC_ChargeType = "CMT";
			code4.AC_ShowOnQuotation = false;
			code4.AC_SuppressOnQuoteIfZero = true;

			booking.TryLoadOrCreateJob();
			Job jobHeader = (Job)booking.Job;
			jobHeader.JH_JobNum = "123";
			jobHeader.JH_GC = GlbCompany.CurrentCompany.PK;
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			jobHeader.LocalChargesPK = localClient.PK;

			var accChargeCodes = new[] { code1, code2, code3, code4 };
			var seq = accChargeCodes.Length * 2;

			foreach (AccChargeCode code in accChargeCodes)
			{
				Charge chargeA = jobHeader.Charges.AddNew();
				chargeA.JR_AC = code.PK;
				chargeA.JR_LocalSellAmt = 50;
				chargeA.JR_DisplaySequence = (ZShort)seq--;

				Charge chargeB = jobHeader.Charges.AddNew();
				chargeB.JR_AC = code.PK;
				chargeB.JR_LocalSellAmt = 0;
				chargeB.JR_DisplaySequence = (ZShort)seq--;
			}

			Factory.Save();

			StringBuilder builder = new StringBuilder();
			builder.AppendLine();

			foreach (ChargeWrapper chargeWrapper in Wrapper.Charges)
			{
				builder.Append(chargeWrapper.ChargeCode.Code);
				builder.Append(": ");
				builder.Append(chargeWrapper.LocalSell);
				builder.AppendLine();
			}

			const string expected = @"
DDD: 
DDD: 50.00 ERN
BBB: 50.00 ERN
AAA: 
AAA: 50.00 ERN
";

			AssertMultilineASCIIEquals("", expected, builder.ToString());
		}

		#endregion

		public void TestGoodsDescription()
		{
			QuotedBooking booking = NewBookingWithQuote();
			booking.Booking.JS_GoodsDescription = "Short Description";
			Factory.Save();

			FreightWrapperFromOneOffQuote wrapper = new FreightWrapperFromOneOffQuote(booking.Quote, Factory);
			AssertEquals("Short Description", wrapper.GoodsDescription);

			booking.Booking.DetailedGoodsDescriptionNoteText = "Detailed Description";
			AssertEquals("Detailed Description", wrapper.GoodsDescription);
		}

		public void TestMarksAndNumbers()
		{
			QuotedBooking booking = NewBookingWithQuote();
			booking.Booking.JS_MarksAndNumbers = "Marks And Numbers";
			Factory.Save();

			FreightWrapperFromOneOffQuote wrapper = new FreightWrapperFromOneOffQuote(booking.Quote, Factory);
			AssertEquals("Marks And Numbers", wrapper.MarksAndNumbers);
		}

		public void TestPickupAndDeliveryDates()
		{
			ZDateTime now = ZDateTime.Now;

			QuotedBooking booking = NewBookingWithQuote();
			booking.PickupReady = now.AddDays(1);
			booking.PickupClose = now.AddDays(2);
			booking.DeliveryOpen = now.AddDays(3);
			booking.DeliveryClose = now.AddDays(4);
			Factory.Save();

			FreightWrapperFromOneOffQuote wrapper = new FreightWrapperFromOneOffQuote(booking.Quote, Factory);
			AssertEquals("PickupFrom", now.AddDays(1), wrapper.PickupFrom);
			AssertEquals("PickupRequiredBy", now.AddDays(2), wrapper.PickupRequiredBy);
			AssertEquals("DeliveryFrom", now.AddDays(3), wrapper.DeliveryFrom);
			AssertEquals("DeliveryRequiredBy", now.AddDays(4), wrapper.DeliveryRequiredBy);
		}

		public void TestPickupAndDeliveryAddresses_SpotQuote()
		{
			var pickUpOrg = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>();
			pickUpOrg.MainAddress.OA_Address1 = "PICK UP ADDRESS";
			deliveryOrg.MainAddress.OA_Address1 = "DELIVERY ADDRESS";

			var quote = (Quote)GetNewBusinessObjectToWrap();
			var oneOffQuote = quote.CurrentOneOffQuote;
			oneOffQuote.PickUpDocAddress.OrganisationPK = pickUpOrg.PK;
			oneOffQuote.DeliveryDocAddress.OrganisationPK = deliveryOrg.PK;
			Factory.Save();

			var wrapper = new FreightWrapperFromOneOffQuote(quote, Factory);

			AssertEquals("Expected the wrapper to use the pick up organization", "PICK UP ADDRESS", wrapper.PickupAddress.ToString());
			AssertEquals("Expected the wrapper to use the delivery organization", "DELIVERY ADDRESS", wrapper.DeliveryAddress.ToString());

			oneOffQuote.PickUpDocAddress.E2_AddressOverride = true;
			oneOffQuote.DeliveryDocAddress.E2_AddressOverride = true;
			oneOffQuote.PickUpDocAddress.E2_Address1 = "OVERRIDEN PICKUP";
			oneOffQuote.DeliveryDocAddress.E2_Address1 = "OVERRIDEN DELIVERY";
			Factory.Save();

			wrapper = new FreightWrapperFromOneOffQuote(quote, Factory);

			AssertEquals("Expected to use the overriden address", "OVERRIDEN PICKUP\nERITREA", wrapper.PickupAddress.ToString());
			AssertEquals("Expected to use the overriden address", "OVERRIDEN DELIVERY\nERITREA", wrapper.DeliveryAddress.ToString());
		}

		public void TestPickupAndDeliveryAddresses_BookingWithQuote()
		{
			var pickUpOrg = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			pickUpOrg.MainAddress.OA_Address1 = "PICK UP ADDRESS";
			deliveryOrg.MainAddress.OA_Address1 = "DELIVERY ADDRESS";
			consignor.MainAddress.OA_Address1 = "CONSIGNOR ADDRESS";
			consignee.MainAddress.OA_Address1 = "CONSIGNEE ADDRESS";

			var quotedBooking = NewBookingWithQuote();
			var quote = quotedBooking.Quote;
			var booking = quotedBooking.Booking;
			quote.CurrentOneOffQuote.PickUpDocAddress.OrganisationPK = consignor.PK;
			quote.CurrentOneOffQuote.DeliveryDocAddress.OrganisationPK = consignee.PK;
			Factory.Save();

			var wrapper = new FreightWrapperFromOneOffQuote(quote, Factory);
			AssertEquals("Expected to use quote's consignee", consignee.MainAddress.OA_Address1, wrapper.DeliveryAddress.ToString());
			AssertEquals("Expected to use quote's consignor", consignor.MainAddress.OA_Address1, wrapper.PickupAddress.ToString());

			booking.ConsignorPickupAddress.E2_OA_Address = pickUpOrg.MainAddress.PK;
			booking.ConsigneeDeliveryAddress.E2_OA_Address = deliveryOrg.MainAddress.PK;
			Factory.Save();

			wrapper = new FreightWrapperFromOneOffQuote(quote, Factory);
			AssertEquals("Expected to use pick up org as booking address takes priority", pickUpOrg.MainAddress.OA_Address1, wrapper.PickupAddress.ToString());
			AssertEquals("Expected to use delivery org as booking address takes priority", deliveryOrg.MainAddress.OA_Address1, wrapper.DeliveryAddress.ToString());

			booking.ConsignorPickupAddress.E2_AddressOverride = true;
			booking.ConsigneeDeliveryAddress.E2_AddressOverride = true;
			booking.ConsignorPickupAddress.E2_Address1 = "OVERRIDEN PICKUP ON BOOKING";
			booking.ConsigneeDeliveryAddress.E2_Address1 = "OVERRIDEN DELIVERY ON BOOKING";
			Factory.Save();

			wrapper = new FreightWrapperFromOneOffQuote(quote, Factory);

			AssertEquals("Expected to use the overriden address", booking.ConsignorPickupAddress.E2_Address1, wrapper.PickupAddress.AddressLine1);
			AssertEquals("Expected to use the overriden address", booking.ConsigneeDeliveryAddress.E2_Address1, wrapper.DeliveryAddress.AddressLine1);

			booking.ConsignorPickupAddress.E2_AddressOverride = false;
			booking.ConsigneeDeliveryAddress.E2_AddressOverride = false;
			booking.ConsignorPickupAddress.E2_OA_Address = ZGuid.Empty;
			booking.ConsigneeDeliveryAddress.E2_OA_Address = ZGuid.Empty;
			Factory.Save();

			wrapper = new FreightWrapperFromOneOffQuote(quote, Factory);
			AssertEquals("Expected to fall back to address from quote now booking has no address", consignee.MainAddress.OA_Address1, wrapper.DeliveryAddress.ToString());
			AssertEquals("Expected to fall back to address from quote now booking has no address", consignor.MainAddress.OA_Address1, wrapper.PickupAddress.ToString());
		}

		public void TestInterimReceipt()
		{
			QuotedBooking booking = NewBookingWithQuote();
			booking.Booking.JS_InterimReceipt = "Receipt";
			Factory.Save();

			FreightWrapperFromOneOffQuote wrapper = new FreightWrapperFromOneOffQuote(booking.Quote, Factory);
			AssertEquals("PickupInterimReceipt", "Receipt", wrapper.PickupInterimReceipt);
		}

		public void TestShippersReference()
		{
			QuotedBooking booking = NewBookingWithQuote();
			booking.Booking.JS_BookingReference = "Ref";
			Factory.Save();

			FreightWrapperFromOneOffQuote wrapper = new FreightWrapperFromOneOffQuote(booking.Quote, Factory);
			AssertEquals("ShippersReference", "Ref", wrapper.ShippersReference);
		}

		[SetOrgAllowMixedCase(true)]
		public void TestGetExportReceivingDepotAddress()
		{
			OrgHeader receival = Factory.NewWithValidTestData<OrgHeader>();
			receival.OH_FullName = "Receival";

			QuotedBooking booking = NewBookingWithQuote();
			booking.Booking.JS_OA_ExportReceivingDepot = receival.MainAddress.PK;
			Factory.Save();

			FreightWrapperFromOneOffQuote wrapper = new FreightWrapperFromOneOffQuote(booking.Quote, Factory);
			AssertEquals("GetExportReceivingDepotAddress", "Receival", wrapper.ExportReceivingDepotAddress.CompanyName);
			AssertEquals("PickupCFSAddress", "Receival", wrapper.PickupCFSAddress.CompanyName);
		}

		public void TestIncoTerm()
		{
			Quote quote = (Quote)GetNewBusinessObjectToWrap();
			quote.CurrentOneOffQuote.TT_IncoTerm = Constants.IncoTerms.FreeOnBoard;

			FreightWrapper wrapper = new FreightWrapperFromOneOffQuote(quote, Factory);
			AssertEquals("FOB", wrapper.IncoTerm.Code);
		}

		public void TestAdditionalTerms()
		{
			Quote quote = (Quote)GetNewBusinessObjectToWrap();
			quote.CurrentOneOffQuote.TT_IncoTerm = Constants.IncoTerms.FreeOnBoard;
			quote.CurrentOneOffQuote.TT_AdditionalTerms = "Additional Terms Free Text";

			FreightWrapper wrapper = new FreightWrapperFromOneOffQuote(quote, Factory);
			AssertEquals("Additional Terms Free Text", wrapper.AdditionalTerms);
		}

		public void TestServiceLevel()
		{
			Quote quote = (Quote)GetNewBusinessObjectToWrap();
			quote.CurrentOneOffQuote.TT_RS_NKServiceLevel = "D2D";

			FreightWrapper wrapper = new FreightWrapperFromOneOffQuote(quote, Factory);
			AssertEquals("D2D", wrapper.ServiceLevel.Code);
		}

		public void TestOrderLines()
		{
			var quote = (Quote)GetNewBusinessObjectToWrap();
			quote.CurrentOneOffQuote.TT_IncoTerm = "FOB";

			var wrapper = new FreightWrapperFromOneOffQuote(quote, Factory);
			AssertEquals(0, wrapper.OrderLines.Count);

			var order1 = Factory.NewWithValidTestData<Order>();
			var orderLine1 = order1.OrderLines.AddNew();
			orderLine1.JO_LineNo = 1;
			var orderLine2 = order1.OrderLines.AddNew();
			orderLine2.JO_LineNo = 2;

			var order2 = Factory.NewWithValidTestData<Order>();
			var orderLine3 = order2.OrderLines.AddNew();
			orderLine3.JO_LineNo = 3;

			var booking = NewBookingWithQuote();
			booking.Booking.AttachedOrders.Add(order1);
			booking.Booking.AttachedOrders.Add(order2);

			wrapper = new FreightWrapperFromOneOffQuote(booking.Quote, Factory);
			AssertContainsExactElementsInAnyOrder(new[] { 1, 2, 3 }, wrapper.OrderLines.Cast<OrderLineWrapper>().Select(x => (int)x.LineNumber));
		}

		public void TestValues()
		{
			Quote quote = (Quote)GetNewBusinessObjectToWrap();
			quote.CurrentOneOffQuote.TT_InsureVal = 1500;
			quote.CurrentOneOffQuote.TT_RX_NKInsureValCurr = "USD";
			quote.CurrentOneOffQuote.TT_ValueOfGoods = 1400;
			quote.CurrentOneOffQuote.TT_RX_NKGoodsCurrency = "AUD";

			FreightWrapper wrapper = new FreightWrapperFromOneOffQuote(quote, Factory);
			AssertEquals("1,400.00 AUD", wrapper.GoodsValue.ToString());
			AssertEquals("1,500.00 USD", wrapper.Rating.OneOffShipment.InsuranceValue.ToString());
			AssertEquals("1,500.00 USD", wrapper.InsuranceValue.ToString());
		}

		public void TestContainers()
		{
			//should be no booking
			var quote = (Quote)GetNewBusinessObjectToWrap();
			quote.CurrentOneOffQuote.Containers.AddNew().TC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			quote.CurrentOneOffQuote.Containers.AddNew().TC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;

			var wrapper = new FreightWrapperFromOneOffQuote(quote, Factory);
			AssertEquals(2, wrapper.Containers.Count);
			AssertEquals("20GP", wrapper.Containers[0].Type.Code);
			AssertEquals("20RE", wrapper.Containers[1].Type.Code);

			var bookingWithQuote = NewBookingWithQuote();
			bookingWithQuote.Booking.JS_TransportMode = Constants.TransportModes.Sea;
			bookingWithQuote.Booking.JS_PackingMode = Constants.ContainerModes.FCL;
			bookingWithQuote.QuotedBookingContainers.AddNew().JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			Factory.Save();

			wrapper = new FreightWrapperFromOneOffQuote(bookingWithQuote.Quote, Factory);
			AssertEquals(1, wrapper.Containers.Count);
			AssertEquals("40GP", wrapper.Containers[0].Type.Code);
		}

		public void TestPackages()
		{
			var quote = (Quote)GetNewBusinessObjectToWrap();
			quote.CurrentOneOffQuote.LooseCargo.AddNew().TPL_F3_NKPackType = "PLT";
			quote.CurrentOneOffQuote.LooseCargo.AddNew().TPL_F3_NKPackType = "BOX";

			var wrapper = new FreightWrapperFromOneOffQuote(quote, Factory);
			AssertEquals(2, wrapper.Packages.Count);
			AssertEquals("PLT", wrapper.Packages[0].Packages.Unit.Code);
			AssertEquals("BOX", wrapper.Packages[1].Packages.Unit.Code);

			var bookingWithQuote = NewBookingWithQuote();
			bookingWithQuote.Booking.JS_TransportMode = Constants.TransportModes.Sea;
			bookingWithQuote.Booking.JS_PackingMode = Constants.ContainerModes.FCL;
			bookingWithQuote.Booking.OuterPackLines.AddNew().JL_F3_NKPackType = "BAG";
			Factory.Save();

			wrapper = new FreightWrapperFromOneOffQuote(bookingWithQuote.Quote, Factory);
			AssertEquals(1, wrapper.Packages.Count);
			AssertEquals("BAG", wrapper.Packages[0].Packages.Unit.Code);
		}

		public void TestServices()
		{
			QuotedBooking booking = NewBookingWithQuote();
			JobService service1 = booking.Booking.Services.AddNew();
			JobService service2 = booking.Booking.Services.AddNew();
			Factory.Save();

			FreightWrapperFromOneOffQuote wrapper = new FreightWrapperFromOneOffQuote(booking.Quote, Factory);
			AssertEquals("wrapper.Services.Count", 2, wrapper.Services.Count);
			AssertEquals("wrapper.Services[0]", service1, wrapper.Services[0].WrappedObject);
			AssertEquals("wrapper.Services[1]", service2, wrapper.Services[1].WrappedObject);
		}

		public void TestOrders()
		{
			OrgHeader buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "BUYER";

			QuotedBooking booking = NewBookingWithQuote();

			Order order1 = booking.Booking.AttachedOrders.AddNew();
			order1.BuyerPK = buyer.PK;
			order1.JD_OrderNumber = "X1";

			Order order2 = booking.Booking.AttachedOrders.AddNew();
			order2.BuyerPK = buyer.PK;
			order2.JD_OrderNumber = "X2";

			Factory.Save();

			FreightWrapperFromOneOffQuote wrapper = new FreightWrapperFromOneOffQuote(booking.Quote, Factory);
			AssertEquals("wrapper.Orders.Count", 2, wrapper.Orders.Count);
			AssertEquals("wrapper.Orders[0]", "X1", wrapper.Orders[0].OrderNo);
			AssertEquals("wrapper.Orders[1]", "X2", wrapper.Orders[1].OrderNo);
		}

		public void TestModes()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			quote.CurrentOneOffQuote.TT_TransportMode = Constants.RateMode.ROA;
			quote.CurrentOneOffQuote.TT_ContainerMode = Constants.RateMode.FCL;

			FreightWrapper wrapper = new FreightWrapperFromOneOffQuote(quote, Factory);
			AssertEquals(Constants.TransportModes.Road, wrapper.ShipmentTransportMode.Code);
			AssertEquals(Constants.ContainerModes.FCL, wrapper.ShipmentContainerMode.Code);
		}

		public void TestRouting()
		{
			Quote quote = (Quote)GetNewBusinessObjectToWrap();
			FreightWrapperFromOneOffQuote wrapper = new FreightWrapperFromOneOffQuote(quote, Factory);
			AssertEquals("", wrapper.Origin.Location.UNLOCO);
			AssertEquals("", wrapper.Destination.Location.UNLOCO);
			AssertEquals(0, wrapper.ShipmentRoutes.Count);

			quote.CurrentOneOffQuote.TT_RL_NKReceivalLocation = "NLAMS";
			quote.CurrentOneOffQuote.TT_RL_NKDeliveryLocation = "AUBNE";

			wrapper = new FreightWrapperFromOneOffQuote(quote, Factory);
			AssertEquals("NLAMS", wrapper.Origin.Location.UNLOCO);
			AssertEquals("AUBNE", wrapper.Destination.Location.UNLOCO);
			AssertEquals(1, wrapper.ShipmentRoutes.Count);
			AssertEquals("NLAMS", wrapper.ShipmentRoutes[0].Origin.UNLOCO);
			AssertEquals("AUBNE", wrapper.ShipmentRoutes[0].Destination.UNLOCO);

			quote.CurrentOneOffQuote.TT_RL_NKViaLocation = "SGSIN";

			wrapper = new FreightWrapperFromOneOffQuote(quote, Factory);
			AssertEquals("NLAMS", wrapper.Origin.Location.UNLOCO);
			AssertEquals("AUBNE", wrapper.Destination.Location.UNLOCO);
			AssertEquals(2, wrapper.ShipmentRoutes.Count);
			AssertEquals("NLAMS", wrapper.ShipmentRoutes[0].Origin.UNLOCO);
			AssertEquals("SGSIN", wrapper.ShipmentRoutes[0].Destination.UNLOCO);
			AssertEquals("SGSIN", wrapper.ShipmentRoutes[1].Origin.UNLOCO);
			AssertEquals("AUBNE", wrapper.ShipmentRoutes[1].Destination.UNLOCO);
		}

		[SetOrgAllowMixedCase(true)]
		public void TestOrganisations()
		{
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "Consignor";

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "Consignee";

			OrgHeader carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_FullName = "Carrier";

			OrgHeader broker = Factory.NewWithValidTestData<OrgHeader>();
			broker.OH_FullName = "Broker";

			OrgHeader agent = Factory.NewWithValidTestData<OrgHeader>();
			agent.OH_FullName = "Agent";

			QuotedBooking booking = NewBookingWithQuote();
			booking.Quote.CurrentOneOffQuote.TT_OH_Carrier = carrier.PK;
			booking.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			booking.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			booking.Booking.JS_OH_ExportBroker = broker.PK;
			booking.Booking.JS_OH_DeliveryAgent = agent.PK;
			Factory.Save();

			FreightWrapperFromOneOffQuote wrapper = new FreightWrapperFromOneOffQuote(booking.Quote, Factory);

			AssertEquals("Consignor", wrapper.Consignor.CompanyName);
			AssertEquals("Consignee", wrapper.Consignee.CompanyName);
			AssertEquals("Carrier", wrapper.Carrier.CompanyName);
			AssertEquals("Broker", wrapper.ExportBroker.CompanyName);
			AssertEquals("Agent", wrapper.ImportAgent.CompanyName);
		}

		public void TestJobNumbers()
		{
			CombineAssertions(delegate
			{
				QuotedBooking booking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
				booking.Quote.TH_QuoteNumber = "000X123";
				Factory.Save();

				FreightWrapperFromOneOffQuote wrapper = new FreightWrapperFromOneOffQuote(booking.Quote, Factory);
				AssertEquals("Quote No.", wrapper.JobNumberHeading);
				AssertEquals("X123/A", wrapper.JobNumber);
				AssertEquals("", wrapper.SecondaryHeading);
				AssertEquals("", wrapper.SecondaryNumber);
			});

			CombineAssertions(delegate
			{
				QuotedBooking booking = NewBookingWithQuote();
				booking.Quote.TH_QuoteNumber = "000Y123";
				booking.Booking.JS_UniqueConsignRef = "0002000";
				Factory.Save();

				FreightWrapperFromOneOffQuote wrapper = new FreightWrapperFromOneOffQuote(booking.Quote, Factory);
				AssertEquals("Quote No.", wrapper.JobNumberHeading);
				AssertEquals("Y123/A", wrapper.JobNumber);
				AssertEquals("Shipment No.", wrapper.SecondaryHeading);
				AssertEquals("0002000", wrapper.SecondaryNumber);
			});
		}

		public void TestWeightAndVolume()
		{
			Quote quote = (Quote)GetNewBusinessObjectToWrap();
			quote.CurrentOneOffQuote.TT_ActualWeight = 15;
			quote.CurrentOneOffQuote.TT_UnitOfWeight = Constants.Weight.Tonnes;
			quote.CurrentOneOffQuote.TT_ActualVolume = 10;
			quote.CurrentOneOffQuote.TT_UnitOfVolume = Constants.Volume.CubicMetres;
			quote.CurrentOneOffQuote.TT_Chargeable = 12;

			FreightWrapper wrapper = new FreightWrapperFromOneOffQuote(quote, Factory);
			CombineAssertions(delegate
			{
				AssertEquals("15.000 T", wrapper.Weight.ToString());
				AssertEquals("10.000 M3", wrapper.Volume.ToString());
				AssertEquals("12.000 M3", wrapper.ChargeableWeight.ToString());
			});
		}

		public void TestWeightVolumeAndChargeableWeightDecimalPlaces()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Registry.Business.Module.Freight);

			var defaultNumberOfWeightDecimals = collection.AddNew();
			defaultNumberOfWeightDecimals.UnitOfMeasure = Constants.Weight.Kilograms;
			defaultNumberOfWeightDecimals.TransportMode = Constants.TransportModes.Air;
			defaultNumberOfWeightDecimals.NumberOfDecimals = 1;
			defaultNumberOfWeightDecimals.RoundingMode = RoundingModes.Up;

			var defaultNumberOfVolumeDecimals = collection.AddNew();
			defaultNumberOfVolumeDecimals.UnitOfMeasure = Constants.Volume.CubicMetres;
			defaultNumberOfVolumeDecimals.TransportMode = Constants.TransportModes.Air;
			defaultNumberOfVolumeDecimals.NumberOfDecimals = 2;
			defaultNumberOfVolumeDecimals.RoundingMode = RoundingModes.Up;

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var quote = (Quote)GetNewBusinessObjectToWrap();
			quote.CurrentOneOffQuote.TT_TransportMode = Constants.TransportModes.Air;
			quote.CurrentOneOffQuote.TT_ContainerMode = Constants.ContainerModes.Loose;
			quote.CurrentOneOffQuote.TT_ActualWeight = 127.2;
			quote.CurrentOneOffQuote.TT_UnitOfWeight = Constants.Weight.Kilograms;
			quote.CurrentOneOffQuote.TT_ActualVolume = 1.25;
			quote.CurrentOneOffQuote.TT_UnitOfVolume = Constants.Volume.CubicMetres;
			quote.CurrentOneOffQuote.TT_Chargeable = 208.3;

			FreightWrapper wrapper = new FreightWrapperFromOneOffQuote(quote, Factory);
			CombineAssertions(delegate
			{
				AssertEquals("127.2 KG", wrapper.Weight.ToString());
				AssertEquals("1.25 M3", wrapper.Volume.ToString());
				AssertEquals("208.3 KG", wrapper.ChargeableWeight.ToString());
				AssertEquals(defaultNumberOfWeightDecimals.NumberOfDecimals, wrapper.Weight.DecimalPlaces);
				AssertEquals(defaultNumberOfVolumeDecimals.NumberOfDecimals, wrapper.Volume.DecimalPlaces);
			});

			quote = (Quote)GetNewBusinessObjectToWrap();
			quote.CurrentOneOffQuote.TT_TransportMode = Constants.TransportModes.Courier;
			quote.CurrentOneOffQuote.TT_ContainerMode = Constants.RateMode.COU;
			quote.CurrentOneOffQuote.TT_ActualWeight = 5;
			quote.CurrentOneOffQuote.TT_UnitOfWeight = Constants.Weight.Kilograms;
			quote.CurrentOneOffQuote.TT_ActualVolume = 10;
			quote.CurrentOneOffQuote.TT_UnitOfVolume = Constants.Volume.CubicMetres;
			quote.CurrentOneOffQuote.TT_Chargeable = 20;

			wrapper = new FreightWrapperFromOneOffQuote(quote, Factory);
			CombineAssertions(delegate
			{
				AssertEquals("5.000 KG", wrapper.Weight.ToString());
				AssertEquals("10.000 M3", wrapper.Volume.ToString());
				AssertEquals("20.000 KG", wrapper.ChargeableWeight.ToString());
				AssertEquals(3, wrapper.Weight.DecimalPlaces);
				AssertEquals(3, wrapper.Volume.DecimalPlaces);
				AssertEquals(3, wrapper.ChargeableWeight.DecimalPlaces);
			});
		}

		public void TestCustomFieldValues()
		{
			#region SetupWorkflowCustomFields

			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = "QBK";
			processTaskTemplate.P0_SubType1 = "SEA";
			processTaskTemplate.P0_SubType2 = "QBN";
			processTaskTemplate.P0_LoadPortCountry = "AU";

			var customField1 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "custom1";
			customField1.XC_Type = AddOnColumnDataType.Codes.String;

			var customField2 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField2.XC_Name = "custom2";
			customField2.XC_Type = AddOnColumnDataType.Codes.Integer;

			Factory.Save();

			#endregion

			var quotedBooking = NewBookingWithQuote();
			quotedBooking.Mode = "LCL";
			quotedBooking.Origin = "AUSYD";

			AddCustomFieldValue(quotedBooking.PK, "custom1", AddOnColumnDataType.Codes.String, "AAA");
			AddCustomFieldValue(quotedBooking.PK, "custom2", AddOnColumnDataType.Codes.Integer, "111");

			Factory.Save();

			var wrapper = new FreightWrapperFromOneOffQuote(quotedBooking.Quote, Factory);
			AssertEquals("custom1 updated value", (ZString)"AAA", wrapper.GetCustomField("custom1"));
			AssertEquals("custom1 updated value", (ZInt)111, wrapper.GetCustomField("custom2"));
		}

		public override void TestWrapperNotes()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			Factory.Save();

			FreightWrapperFromOneOffQuote wrapper = new FreightWrapperFromOneOffQuote(quote, Factory);
			AssertEquals(0, wrapper.Notes.Count);
		}

		public override void TestTrackingBusinessObjectPK()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			Factory.Save();
			FreightWrapperFromOneOffQuote emptyWrapper = new FreightWrapperFromOneOffQuote(quote, Factory);

			AssertEquals("TrackingBusinessObjectPK", quote.PK, emptyWrapper.TrackingBusinessObjectPK);
		}

		public override void TestContainerLayoutStyle()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var wrappers = FreightWrapper.New(quote, Factory);

			AssertEquals("NoContainers", wrappers[0].ContainerLayoutStyle);

			AddContainer(quote);
			wrappers = FreightWrapper.New(quote, Factory);

			AssertEquals("SingleContainer", wrappers[0].ContainerLayoutStyle);

			AddContainer(quote);
			wrappers = FreightWrapper.New(quote, Factory);

			AssertEquals("Quote only - 2 Containers", "MultipleContainersSingleDetails", wrappers[0].ContainerLayoutStyle);

			var bookingWithQuote = NewBookingWithQuote();
			wrappers = FreightWrapper.New(bookingWithQuote, Factory);

			AssertEquals("NoContainers", wrappers[0].ContainerLayoutStyle);

			bookingWithQuote.QuotedBookingContainers.AddNew();
			wrappers = FreightWrapper.New(bookingWithQuote, Factory);

			AssertEquals("SingleContainer", wrappers[0].ContainerLayoutStyle);

			bookingWithQuote.QuotedBookingContainers.AddNew();
			wrappers = FreightWrapper.New(bookingWithQuote, Factory);

			AssertEquals("Booking with quote - 2 Containers", "MultipleContainersSingleDetails", wrappers[0].ContainerLayoutStyle);
		}

		public void TestShowOnQuotation_LocalClient()
		{
			QuotedBooking booking = QuotedBooking.New(WrappedBO.PK, ZGuid.Empty, Factory);

			var localClient = Factory.New<OrgHeader>();
			localClient.OH_Code = "Microsoft";
			localClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 2);

			var overseasAgent = Factory.New<OrgHeader>();
			overseasAgent.OH_Code = "Apple";
			overseasAgent.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 2);

			AccChargeCode code1 = Factory.NewWithValidTestData<AccChargeCode>();
			code1.AC_Code = "AAA";
			code1.AC_ShowOnQuotation = true;
			code1.AC_SuppressOnQuoteIfZero = false;

			AccChargeCode code2 = Factory.NewWithValidTestData<AccChargeCode>();
			code2.AC_Code = "BBB";
			code2.AC_ShowOnQuotation = true;
			code2.AC_SuppressOnQuoteIfZero = true;

			AccChargeCode code3 = Factory.NewWithValidTestData<AccChargeCode>();
			code3.AC_Code = "CCC";
			code3.AC_ShowOnQuotation = false;
			code3.AC_SuppressOnQuoteIfZero = false;

			AccChargeCode code4 = Factory.NewWithValidTestData<AccChargeCode>();
			code4.AC_Code = "DDD";
			code4.AC_ChargeType = "CMT";
			code4.AC_ShowOnQuotation = false;
			code4.AC_SuppressOnQuoteIfZero = true;

			booking.TryLoadOrCreateJob();
			booking.Quote.TH_OneTimeQuote = true;
			booking.Quote.CurrentOneOffQuote.TT_OrgRole = Core.Constants.OrgRoles.LocalClient;
			Job jobHeader = (Job)booking.Job;
			jobHeader.JH_JobNum = "123";
			jobHeader.JH_GC = GlbCompany.CurrentCompany.PK;
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			jobHeader.LocalChargesPK = localClient.PK;
			jobHeader.AgentCollectPK = overseasAgent.PK;

			var accChargeCodes = new[] { code1, code2, code3, code4 };
			var seq = accChargeCodes.Length * 2;

			foreach (AccChargeCode code in accChargeCodes)
			{
				Charge chargeA = jobHeader.Charges.AddNew();
				chargeA.JR_AC = code.PK;
				chargeA.JR_LocalSellAmt = 50;
				chargeA.JR_DisplaySequence = (ZShort)seq--;

				Charge chargeB = jobHeader.Charges.AddNew();
				chargeB.JR_AC = code.PK;
				chargeB.JR_LocalSellAmt = 12;
				chargeB.JR_DisplaySequence = (ZShort)seq--;
				chargeB.JR_OH_SellAccount = overseasAgent.PK;
			}

			Factory.Save();

			StringBuilder builder = new StringBuilder();
			builder.AppendLine();

			foreach (ChargeWrapper chargeWrapper in Wrapper.Charges)
			{
				builder.Append(chargeWrapper.ChargeCode.Code);
				builder.Append(": ");
				builder.Append(chargeWrapper.LocalSell);
				builder.AppendLine();
			}

			const string expected = @"
DDD: 12.00 ERN
DDD: 50.00 ERN
BBB: 12.00 ERN
BBB: 50.00 ERN
AAA: 12.00 ERN
AAA: 50.00 ERN
";

			AssertMultilineASCIIEquals("", expected, builder.ToString());
		}

		public void TestShowOnQuotation_OverseasAgent()
		{
			QuotedBooking booking = QuotedBooking.New(WrappedBO.PK, ZGuid.Empty, Factory);

			var localClient = Factory.New<OrgHeader>();
			localClient.OH_Code = "Microsoft";
			localClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 2);

			var overseasAgent = Factory.New<OrgHeader>();
			overseasAgent.OH_Code = "Apple";
			overseasAgent.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 2);

			AccChargeCode code1 = Factory.NewWithValidTestData<AccChargeCode>();
			code1.AC_Code = "AAA";
			code1.AC_ShowOnQuotation = true;
			code1.AC_SuppressOnQuoteIfZero = false;

			AccChargeCode code2 = Factory.NewWithValidTestData<AccChargeCode>();
			code2.AC_Code = "BBB";
			code2.AC_ShowOnQuotation = true;
			code2.AC_SuppressOnQuoteIfZero = true;

			AccChargeCode code3 = Factory.NewWithValidTestData<AccChargeCode>();
			code3.AC_Code = "CCC";
			code3.AC_ShowOnQuotation = false;
			code3.AC_SuppressOnQuoteIfZero = false;

			AccChargeCode code4 = Factory.NewWithValidTestData<AccChargeCode>();
			code4.AC_Code = "DDD";
			code4.AC_ChargeType = "CMT";
			code4.AC_ShowOnQuotation = false;
			code4.AC_SuppressOnQuoteIfZero = true;

			booking.TryLoadOrCreateJob();
			booking.Quote.TH_OneTimeQuote = true;
			booking.Quote.CurrentOneOffQuote.TT_OrgRole = Core.Constants.OrgRoles.OverseasAgent;
			Job jobHeader = (Job)booking.Job;
			jobHeader.JH_JobNum = "123";
			jobHeader.JH_GC = GlbCompany.CurrentCompany.PK;
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			jobHeader.LocalChargesPK = localClient.PK;
			jobHeader.AgentCollectPK = overseasAgent.PK;

			var accChargeCodes = new[] { code1, code2, code3, code4 };
			var seq = accChargeCodes.Length * 2;

			foreach (AccChargeCode code in accChargeCodes)
			{
				Charge chargeA = jobHeader.Charges.AddNew();
				chargeA.JR_AC = code.PK;
				chargeA.JR_LocalSellAmt = 50;
				chargeA.JR_DisplaySequence = (ZShort)seq--;

				Charge chargeB = jobHeader.Charges.AddNew();
				chargeB.JR_AC = code.PK;
				chargeB.JR_LocalSellAmt = 0;
				chargeB.JR_DisplaySequence = (ZShort)seq--;
				chargeB.JR_OH_SellAccount = overseasAgent.PK;
			}

			Factory.Save();

			StringBuilder builder = new StringBuilder();
			builder.AppendLine();

			foreach (ChargeWrapper chargeWrapper in Wrapper.Charges)
			{
				builder.Append(chargeWrapper.ChargeCode.Code);
				builder.Append(": ");
				builder.Append(chargeWrapper.LocalSell);
				builder.AppendLine();
			}

			const string expected = @"
DDD: 
AAA: 
";

			AssertMultilineASCIIEquals("", expected, builder.ToString());
		}

		protected override string ExpectedBarcodeText()
		{
			return "È^QU1=1000;CAD;|iÊ";
		}

		public override void TestFormattedTotalCO2e()
		{
			var oneOffQuote = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			var wrapper = new FreightWrapperFromOneOffQuote(oneOffQuote.Quote, Factory);
			AssertEquals(ZString.Empty, wrapper.FormattedTotalCO2e);

			oneOffQuote.SetTotalCO2e(9.07244m);
			oneOffQuote.SetCO2eStatus(CO2eStatusList.Codes.Current);
			wrapper = new FreightWrapperFromOneOffQuote(oneOffQuote.Quote, Factory);
			AssertEquals("9.072", wrapper.FormattedTotalCO2e);

			oneOffQuote.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
			wrapper = new FreightWrapperFromOneOffQuote(oneOffQuote.Quote, Factory);
			AssertEquals(ZString.Empty, wrapper.FormattedTotalCO2e);
		}

		public override void TestCO2eCalculationDate()
		{
			var oneOffQuote = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			var wrapper = new FreightWrapperFromOneOffQuote(oneOffQuote.Quote, Factory);
			AssertEquals(ZDateTime.Empty, wrapper.CO2eCalculationDate);

			oneOffQuote.SetTotalCO2e(200m);
			oneOffQuote.SetCO2eStatus(CO2eStatusList.Codes.Current);
			Factory.Save();

			wrapper = new FreightWrapperFromOneOffQuote(oneOffQuote.Quote, Factory);
			AssertEquals(((JobCO2e)oneOffQuote.GetOrCreateJobCO2e()).JCO_SystemLastEditTimeUtc, wrapper.CO2eCalculationDate);
		}

		#region Implementation

		QuotedBooking NewBookingWithQuote()
		{
			return QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
		}

		protected override CommonContainer AddContainer(BusinessObject parent)
		{
			Quote quote = (Quote)parent;
			quote.CurrentOneOffQuote.Containers.AddNew();
			return null;
		}

		protected override FreightWrapper GetNewDocumentWrapperWithCarrier()
		{
			((Quote)WrappedBO).CurrentOneOffQuote.TT_OH_Carrier = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			return new FreightWrapperFromOneOffQuote((Quote)WrappedBO, Factory);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			Factory.Save();
			return quote;
		}

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "DeliveryEstimated", ZDateTime.Today.AddMonths(1).ToString() },
					{ "DeliveryFrom", ZDateTime.Today.AddMonths(1).ToString() },
					{ "DeliveryRequiredBy", ZDateTime.Today.AddMonths(1).ToString() },
					{ "JobNumber", "1000" },
					{ "JobNumberBarcodeText", "^QU1=1000;;|" },
					{ "JobNumberBarcodeTextForFont", "È^QU1=1000;;|,Ê" },
					{ "JobNumberBarcodeTextWithoutDocManagerCodes", "È1000[Ê" },
					{ "JobNumberHeading", "Quote No." },
					{ "PickupFrom", ZDateTime.Today.ToString() },
					{ "PickupRequiredBy", ZDateTime.Today.ToString() },
				};
			}
		}

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
Rating : (No Default Field Value Available on Rating Information)";
			}
		}

		protected override Base.GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			Factory.Save();

			return new FreightWrapperFromOneOffQuote(quote, Factory);
		}

		void AddCustomFieldValue(ZGuid quotedBookingId, ZString customValueName, ZString customValueType, ZString value)
		{
			GenCustomAddOnValue customAddOnValue = Factory.New<GenCustomAddOnValue>();
			customAddOnValue.XV_ParentID = quotedBookingId;
			customAddOnValue.XV_ParentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(ViewQuotedBookingSchema.Constants.TableName);
			customAddOnValue.XV_Name = customValueName;
			customAddOnValue.XV_Type = customValueType;
			customAddOnValue.XV_Data = value;
		}

		#endregion
	}
}
