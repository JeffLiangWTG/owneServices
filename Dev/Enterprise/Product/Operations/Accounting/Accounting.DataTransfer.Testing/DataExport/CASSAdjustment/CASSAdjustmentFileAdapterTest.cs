using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer.Invoices.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	[TestedType(typeof(CASSAdjustmentFileAdapter))]
	public class CASSAdjustmentFileAdapterTest : ValueObjectDataAdapterTest<CASSBilling, CASSAdjustmentHeader>
	{
		public void TestAdapter()
		{
			var notificationTestHelper = new NotificationTestHelper();
			var notificationBuffer = new NotificationBuffer();
			var adapter = new CASSAdjustmentFileAdapter();
			var billing = CASSAdjustmentFileTestHelper.GetFullyPopulatedCASS(Factory);
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "10203040506";
			var adjHeader = adapter.ExportToValueObject(billing, new ValueObjectExportContext(notificationBuffer));

			AssertEquals("BillingPeriodEnd", billing.BillingPeriodEnd, adjHeader.BillingPeriodEnd);
			AssertEquals("AgentNumber", "10203040506", adjHeader.AgentNumber);
			AssertEquals("Number of Lines", 2, adjHeader.Lines.Count);

			var line = adjHeader.Lines[0];
			AssertEquals("AgentCode", "10203040506", line.AgentCode);
			AssertEquals("AirlinePrefix", "160", line.AirlinePrefix);
			AssertEquals("AWBSerialNumber", "00144154", line.AWBSerialNumber);
			AssertEquals("CCADCNumber", string.Empty, line.CCADCMNumber);
			AssertEquals("ChargesDueAgentCC", 2000M, line.ChargesDueAgentCC);
			AssertEquals("ChargesDueCarrierPP", 180880M, line.ChargesDueCarrierPP);
			AssertEquals("Comment", "I want to test", line.Comment);
			AssertEquals("Commission", 1000M, line.Commission);
			AssertEquals("Incentive", 132090M, line.Incentive);
			AssertEquals("ReasonCode", 10, line.ReasonCode);
			AssertEquals("RecordType", "AW", line.RecordType);
			AssertEquals("ValuationChargePP", 3000M, line.ValuationChargePP);
			AssertEquals("WeightChargePP", 2500025M, line.WeightChargePP);

			line = adjHeader.Lines[1];
			AssertEquals("AgentCode", "10203040506", line.AgentCode);
			AssertEquals("AirlinePrefix", "172", line.AirlinePrefix);
			AssertEquals("AWBSerialNumber", "67828073", line.AWBSerialNumber);
			AssertEquals("CCADCNumber", "808640", line.CCADCMNumber);
			AssertEquals("ChargesDueAgentCC", 200M, line.ChargesDueAgentCC);
			AssertEquals("ChargesDueCarrierPP", 21618M, line.ChargesDueCarrierPP);
			AssertEquals("Comment", "I want to test again", line.Comment);
			AssertEquals("Commission", 300M, line.Commission);
			AssertEquals("Incentive", 400M, line.Incentive);
			AssertEquals("ReasonCode", 11, line.ReasonCode);
			AssertEquals("RecordType", "DO", line.RecordType);
			AssertEquals("ValuationChargePP", 0M, line.ValuationChargePP);
			AssertEquals("WeightChargePP", 4500025M, line.WeightChargePP);

			AssertEquals("HasError", false, notificationBuffer.HasErrors);
		}

		public void TestSpecialCharactersAreRemovedFromIATAAgentCodeWhileExporting()
		{
			var validCode1 = new { IATAcode = "01-1 8844/0116", isValid = true, NormalizedCode = "01188440116", ExpectedErrorMessage = "" };
			var validCode2 = new { IATAcode = "01-1 88-44/01/16", isValid = true, NormalizedCode = "01188440116", ExpectedErrorMessage = "" };
			var invalidCode1 = new { IATAcode = "01-1**8844/0116", isValid = false, NormalizedCode = "", ExpectedErrorMessage = "Invalid Agent Code. Only numbers (0 - 9), '/', '-' and ' ' are allowed in Agent Code. You can modify the code at Registry > Freight > AWB > MAWB > Issuing Carrier Agent > IATA Code" };
			var invalidCode2 = new { IATAcode = "01-1222228844/0116", isValid = false, NormalizedCode = "", ExpectedErrorMessage = "Agent Code is too long. Maximum possible length is 11 excluding special characters ('/', '-', ' '). You can modify the code at Registry > Freight > AWB > MAWB > Issuing Carrier Agent > IATA Code" };

			foreach (var code in new[] { validCode1, validCode2, invalidCode1, invalidCode2 })
			{
				Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = code.IATAcode;

				var notificationTestHelper = new NotificationTestHelper();
				var notificationBuffer = new NotificationBuffer();
				var adapter = new CASSAdjustmentFileAdapter();
				var billing = CASSAdjustmentFileTestHelper.GetFullyPopulatedCASS(Factory);
				var adjHeader = adapter.ExportToValueObject(billing, new ValueObjectExportContext(notificationBuffer));
				AssertEquals("AgentNumber", code.NormalizedCode, adjHeader.AgentNumber);
				AssertEquals("HasError", !code.isValid, notificationBuffer.HasErrors);
				if (!code.isValid)
				{
					AssertContains("Error Message", code.ExpectedErrorMessage, notificationBuffer.AsString);
				}
				else
				{
					AssertEquals("AgentCode", code.NormalizedCode, adjHeader.Lines[0].AgentCode);
				}
			}
		}

		public void TestCCANumberValidation()
		{
			var validCCADCM1 = new { Number = "C12345", IsValid = true, ExpectedErrorMessage = "" };
			var validCCADCM2 = new { Number = "C1234", IsValid = true, ExpectedErrorMessage = "" };
			var validCCADCM3 = new { Number = "", IsValid = true, ExpectedErrorMessage = "" };
			var invalidCCADCM1 = new { Number = "C123456", IsValid = false, ExpectedErrorMessage = "Allowed Maximum Length: 6, Provided Value Length: 7" };

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "01-1 8844/0116";

			foreach (var cCADCM in new[] { validCCADCM1, validCCADCM2, validCCADCM3, invalidCCADCM1 })
			{
				var notificationTestHelper = new NotificationTestHelper();
				var notificationBuffer = new NotificationBuffer();
				var adapter = new CASSAdjustmentFileAdapter();
				var billing = CASSAdjustmentFileTestHelper.GetFullyPopulatedCASS(Factory);
				billing.CostHeader.ExportLines[0].CCADCMNumber = cCADCM.Number;
				var adjHeader = adapter.ExportToValueObject(billing, new ValueObjectExportContext(notificationBuffer));
				if (!cCADCM.IsValid)
				{
					AssertContains("Error Message", cCADCM.ExpectedErrorMessage, notificationBuffer.AsString);
				}
				else
				{
					AssertEquals("CCADCM Number", cCADCM.Number, adjHeader.Lines[0].CCADCMNumber);
				}
			}
		}

		public new void TestRootElementName()
		{
			try
			{
				base.TestRootElementName();
				Fail("Exception expected as RootElementName is not supported by CASS Adjustment File Adapter.");
			}
			catch (NotSupportedException)
			{
				Assert(true);
			}
		}

		#region Implementation

		protected override void AssertExportFromValueObjectNotSupportedException()
		{
			Assert(true);
		}

		protected override bool IsImportFromValueObjectSupported
		{
			get { return false; }
		}

		protected override bool IsExportToValueObjectSupported
		{
			get { return false; }
		}

		protected override bool IsExportToCollectionSupported
		{
			get { return false; }
		}

		protected override bool IsCreateOrUpdateFromValueObjectSupported
		{
			get { return false; }
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return "RootCollectionElementName is not supported by CASS Adjustment File Adapter."; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "RootElementName is not supported by CASS Adjustment File Adapter."; }
		}

		protected override CASSBilling NewBusinessObject()
		{
			return new CASSBilling(Factory);
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var cassBilling = new CASSBilling(Factory);
			cassBilling.CostHeader.InitializeAsExportCASS();

			return new BusinessObjectAndExpectedOutputFileName(cassBilling, "", ValidationKind.None, "XML import/export is not supported");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return null;
		}

		protected override ValueObjectDataAdapter<CASSBilling, CASSAdjustmentHeader> GetNewBizObjXmlDataAdapter()
		{
			return new CASSAdjustmentFileAdapter();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return null;
		}

		#endregion
	}
}
