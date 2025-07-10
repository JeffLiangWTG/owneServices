using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Testing
{
	public class BaseChargeCalculateDepartmentFromJobTypeAndChargeCodeTest : TestCaseWithFactory
	{
		public void TestCalculateDepartmentFromJobTypeAndChargeCodeType()
		{
			var shipment = TestObjectCreator.CreateShipment("S001", true);
			shipment.JS_IsForwardRegistered = true;
			var department1 = TestObjectCreator.CreateDepartment("ZZ1");
			var department2 = TestObjectCreator.CreateDepartment("ZZ2");
			TestObjectCreator.CreateDepartment("ZZC");

			AccountingConfigurationRegistry.Instance.InvoicingDepartmentMapping.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "ZZ1|ZZC");

			using (var shipmentJob = TestObjectCreator.CreateJob(shipment))
			{
				Factory.Save();

				var customsChargeCode = TestObjectCreator.CC1;
				customsChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Brokerage;

				var filteredChargeCode = TestObjectCreator.CC2;
				filteredChargeCode.AC_DepartmentFilterList = department2.GE_Code;

				AssertNull("null current deparment => null", BaseCharge.CalculateDepartmentFromJobTypeAndChargeCodeType(Factory, shipmentJob, TestObjectCreator.FRT, null));
				AssertEquals("null chargecode => given department", "ZZ1", BaseCharge.CalculateDepartmentFromJobTypeAndChargeCodeType(Factory, shipmentJob, null, department1)?.GE_Code);
				AssertEquals("standard setup => given department", "ZZ1", BaseCharge.CalculateDepartmentFromJobTypeAndChargeCodeType(Factory, shipmentJob, TestObjectCreator.FRT, department1)?.GE_Code);
				AssertEquals("forwarding shipment and customs charge => mapped department", "ZZC", BaseCharge.CalculateDepartmentFromJobTypeAndChargeCodeType(Factory, shipmentJob, customsChargeCode, department1)?.GE_Code);
				AssertEquals("charge with one department filter => filtered department", "ZZ2", BaseCharge.CalculateDepartmentFromJobTypeAndChargeCodeType(Factory, shipmentJob, filteredChargeCode, department1)?.GE_Code);
			}
		}

		void AssertCalculateDepartmentFromJobTypeAndChargeCodeType_CustomsChargesOnFreight(Job job, ZString forwardingDepartmentCode, ZString cacheMapping, Dictionary<ZString, ZString> chargeCodeDepartmentFilterToExpected)
		{
			var jobCompanyGuid = job.JH_GE.ToGuid();
			var forwardingDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, forwardingDepartmentCode)) ?? TestObjectCreator.CreateDepartment(forwardingDepartmentCode);
			forwardingDepartment.GE_GE = jobCompanyGuid;

			CombineAssertions("The expected result is the same for BON, BRK, CDS regardless of the department filter. See WI00279605", () =>
			{
				foreach (var entry in chargeCodeDepartmentFilterToExpected)
				{
					foreach (var chargeGroup in new string[] { ChargeCodeGroupList.Codes.BrokerageOnly, ChargeCodeGroupList.Codes.Brokerage, ChargeCodeGroupList.Codes.CustomsDuty, ChargeCodeGroupList.Codes.OriginBrokerage, ChargeCodeGroupList.Codes.OriginBrokerageOnly })
					{
						var chargeCodeDepartmentFilterList = entry.Key;
						var expectedDepartmentCode = entry.Value;

						var newMapping = AccountingConfigurationRegistry.Instance.InvoicingDepartmentMapping.DefaultValue + ";" + cacheMapping;
						using (AccountingConfigurationRegistry.Instance.InvoicingDepartmentMapping.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, newMapping))
						{
							var gst = TestObjectCreator.CreateTaxRate("GST", "GSTRate", 10);
							var chargeCode = TestObjectCreator.CreateChargeCode("CUSTST1", "customs test", Core.Constants.ChargeType.Margin, 100, gst, null, chargeCodeDepartmentFilterList);

							// chargeCode.IsCustomsCharge must be true for BaseCharge to be considered for the mapping under test. This line makes it meet that criteria.
							chargeCode.AC_ChargeGroup = chargeGroup;

							Factory.Save();

							var actualCalculatedDepartmentCode = BaseCharge.CalculateDepartmentFromJobTypeAndChargeCodeType(Factory, job, chargeCode, forwardingDepartment)?.GE_Code;
							AssertEquals("expected customs department to be selected", expectedDepartmentCode, actualCalculatedDepartmentCode);

							chargeCode.Delete();
							gst.Delete();
							Factory.Save();
						}
					}
				}
			});
		}

		void AssertCalculateDepartmentFromJobTypeAndChargeCodeType_Shipment(ZString forwardingDepartmentCode, ZString cacheMapping, ZString expectedResult)
		{
			var chargeCodeDepartmentFilterToExpected = new Dictionary<ZString, ZString>();
			chargeCodeDepartmentFilterToExpected.Add("CIA", expectedResult);
			chargeCodeDepartmentFilterToExpected.Add("ASI, CIS", expectedResult);
			chargeCodeDepartmentFilterToExpected.Add("ASI, CIA", expectedResult);
			chargeCodeDepartmentFilterToExpected.Add("CIS, CIA", expectedResult);
			chargeCodeDepartmentFilterToExpected.Add("CIA, CEA", expectedResult);
			chargeCodeDepartmentFilterToExpected.Add("ASI", expectedResult);
			chargeCodeDepartmentFilterToExpected.Add("CIS", expectedResult);

			var shipment = TestObjectCreator.CreateShipment("S0001");
			using (var shipmentJob = TestObjectCreator.CreateJob(shipment))
			{
				Factory.Save();
				AssertCalculateDepartmentFromJobTypeAndChargeCodeType_CustomsChargesOnFreight(shipmentJob, forwardingDepartmentCode, cacheMapping, chargeCodeDepartmentFilterToExpected);
			}
		}

		public void TestCalculateDepartmentFromJobType_Shipment_WhenForwardingDepartment_MapsTo_CustomsDept()
		{
			AssertCalculateDepartmentFromJobTypeAndChargeCodeType_Shipment(
				forwardingDepartmentCode: "ASI",
				cacheMapping: "ASI|CIS",
				expectedResult: "CIS"
			);
		}

		public void TestCalculateDepartmentFromJobType_Shipment_WhenCustomsDept_MapsTo_Itself()
		{
			AssertCalculateDepartmentFromJobTypeAndChargeCodeType_Shipment(
				forwardingDepartmentCode: "CIS",
				cacheMapping: "ASI|CIS",
				expectedResult: "CIS"
			);
		}

		public void TestCalculateDepartmentFromJobType_Shipment_WhenUnmappedDept_MapsTo_Itself()
		{
			AssertCalculateDepartmentFromJobTypeAndChargeCodeType_Shipment(
				forwardingDepartmentCode: "CES",
				cacheMapping: "ASI|CIS",
				expectedResult: "CES"
			);
		}

		void AssertCalculateDepartmentFromJobTypeAndChargeCodeType_OneOffQuote(ZString forwardingDepartmentCode, ZString cacheMapping, ZString expectedResult)
		{
			var chargeCodeDepartmentFilterToExpected = new Dictionary<ZString, ZString>();
			chargeCodeDepartmentFilterToExpected.Add("CIA", expectedResult);
			chargeCodeDepartmentFilterToExpected.Add("ASI, CIS", expectedResult);
			chargeCodeDepartmentFilterToExpected.Add("CIA, CEA", expectedResult);
			chargeCodeDepartmentFilterToExpected.Add("ASI, CIA", expectedResult);
			chargeCodeDepartmentFilterToExpected.Add("CIS, CIA", expectedResult);
			chargeCodeDepartmentFilterToExpected.Add("ASI", expectedResult);
			chargeCodeDepartmentFilterToExpected.Add("CIS", expectedResult);

			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			using (var oneOffQuoteJob = TestObjectCreator.CreateJob(quotedBooking))
			{
				Factory.Save();
				AssertCalculateDepartmentFromJobTypeAndChargeCodeType_CustomsChargesOnFreight(oneOffQuoteJob, forwardingDepartmentCode, cacheMapping, chargeCodeDepartmentFilterToExpected);
			}
		}

		public void TestCalculateDepartmentFromJobType_OneOffQuote_WhenForwardingDepartment_MapsTo_CustomsDept()
		{
			AssertCalculateDepartmentFromJobTypeAndChargeCodeType_OneOffQuote(
				forwardingDepartmentCode: "ASI",
				cacheMapping: "ASI|CIS",
				expectedResult: "CIS"
			);
		}

		public void TestCalculateDepartmentFromJobType_OneOffQuote_WhenCustomsDept_MapsTo_Itself()
		{
			AssertCalculateDepartmentFromJobTypeAndChargeCodeType_Shipment(
				forwardingDepartmentCode: "CIS",
				cacheMapping: "ASI|CIS",
				expectedResult: "CIS"
			);
		}

		public void TestCalculateDepartmentFromJobType_OneOffQuote_WhenUnmappedDept_MapsTo_Itself()
		{
			AssertCalculateDepartmentFromJobTypeAndChargeCodeType_Shipment(
				forwardingDepartmentCode: "CES",
				cacheMapping: "ASI|CIS",
				expectedResult: "CES"
			);
		}

		protected TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
