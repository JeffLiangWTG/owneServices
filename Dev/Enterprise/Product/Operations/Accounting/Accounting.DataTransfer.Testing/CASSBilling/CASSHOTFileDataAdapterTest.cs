using System;
using System.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer.Invoices.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	[TestedType(typeof(CASSHOTFileDataAdapter))]
	public class CASSHOTFileDataAdapterTest : ValueObjectDataAdapterTest<CASSBilling, CASSCostHeader>
	{
		readonly NotificationTestHelper notificationTestHelper = new NotificationTestHelper();

		[TestDate(2015, 11, 05)]
		public void TestConverter()
		{
			CASSBilling cassBilling = new CASSBilling(Factory);
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("AAADE   08080108083108091001EUR 2347006");
				writer.WriteLine("AWM  16062144154  DUS23470068510R   HKG0808140011900KEUR0000003462900000000030000000001808800000000000000000000000000000000000000000000000000000000020000000000000001000 000000395080            08090400000.000000000001320901000020100000000000180");
				writer.WriteLine("DCO Y17267828073  LEJ23470068510808640EUR00000.00000080607P000000047300C000000000100C000000000000C000000000200P000000021618200000000002000000000300000000000000000000000400 K0002150MEX");
				writer.WriteLine("DCR N17267828073  LEJ23470068510808640EUR00000.00000080607P000000047300C000000000000C000000000000C000000000000P000000054668000005000004000000000000000000000000000000000400 K0002150MEX");
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					LoadCASSBilling(cassBilling, notificationBuffer, reader);

					AssertEquals(true, cassBilling.IsExportBilling);
					AssertEquals(false, cassBilling.IsImportBilling);
					AssertEquals(new ZDateTime(2008, 08, 01), cassBilling.BillingPeriodStart);
					AssertEquals(new ZDateTime(2008, 08, 31), cassBilling.BillingPeriodEnd);
					AssertEquals(new ZDateTime(2008, 09, 10), cassBilling.BillingDate);

					AssertEquals(2, cassBilling.Lines.Count);
					CASSBillingLine cassLine = cassBilling.Lines[0];
					AssertEquals("160", cassLine.AirlinePrefix);
					AssertEquals("62144154", cassLine.AWBNumber);
					AssertEquals("23470068510", cassLine.AgentCode);
					AssertEquals(new ZDateTime(2008, 08, 14), cassLine.IssueDate);
					AssertEquals(ZDateTime.Empty, cassLine.DateOfArrival);
					AssertEquals(ZDateTime.Empty, cassLine.DateOfDelivery);
					AssertEquals("DUS", cassLine.LoadPortIATA);
					AssertEquals("HKG", cassLine.DischargePortIATA);
					AssertEquals(1190.0M, cassLine.CASSWeight);
					AssertEquals("KG", cassLine.CASSWeightUnit);
					AssertEquals("EUR", cassLine.CASSCostCurrencyCode);
					AssertEquals(3950.80M, cassLine.CASSCostValue);
					AssertEquals(0M, cassLine.CASSCostAdjustedValue);
					AssertEquals(false, cassLine.IsGSTApplicable);
					AssertEquals(100002.01M, cassLine.CASSCostTaxValue);
					AssertEquals(0M, cassLine.CASSCostTaxAdjustedValue);

					cassLine = cassBilling.Lines[1];
					AssertEquals("172", cassLine.AirlinePrefix);
					AssertEquals("67828073", cassLine.AWBNumber);
					AssertEquals("23470068510", cassLine.AgentCode);
					AssertEquals(new ZDateTime(2008, 06, 07), cassLine.IssueDate);
					AssertEquals("LEJ", cassLine.LoadPortIATA);
					AssertEquals("MEX", cassLine.DischargePortIATA);
					AssertEquals(215.0M, cassLine.CASSWeight);
					AssertEquals("KG", cassLine.CASSWeightUnit);
					AssertEquals("EUR", cassLine.CASSCostCurrencyCode);
					AssertEquals(1015.68M, cassLine.CASSCostValue);
					AssertEquals(-680.18M, cassLine.CASSCostAdjustedValue);
					AssertEquals(true, cassLine.IsGSTApplicable);
					AssertEquals(50000.04M, cassLine.CASSCostTaxValue);
					AssertEquals(-2000000000.02M, cassLine.CASSCostTaxAdjustedValue);

					Assert(!notificationBuffer.HasErrors);
					Assert(!notificationBuffer.HasWarnings);
				}
			}
		}

		[TestDate(2015, 11, 05)]
		public void TestConverterWithECRRecortIfRejectedClaimLinesIsNotExpected()
		{
			AssertEquals("Precondition: IsRejectedClaimLinesExpected", false, CASSBilling.IsRejectedClaimLinesExpected);

			CASSBilling cassBilling = new CASSBilling(Factory);

			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("AAADE   08080108083108091001EUR 2347006");
				writer.WriteLine("AWM  16062144154  DUS23470068510R   HKG0808140011900KEUR0000003462900000000030000000001808800000000000000000000000000000000000000000000000000000000020000000000000001000 000000395080            08090400000.000000000001320901000020100000000000180");
				writer.WriteLine("ECR Y17267828073  LEJ23470068510808640EUR00000.00000080607P000000047300C000000000000C000000000000C000000000000P000000054668000005000004000000000000000000000000000000000400 K0002150MEX");
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					LoadCASSBilling(cassBilling, notificationBuffer, reader);

					AssertEquals(new ZDateTime(2008, 08, 01), cassBilling.BillingPeriodStart);
					AssertEquals(new ZDateTime(2008, 08, 31), cassBilling.BillingPeriodEnd);
					AssertEquals(new ZDateTime(2008, 09, 10), cassBilling.BillingDate);

					AssertEquals(1, cassBilling.Lines.Count);
					CASSBillingLine cassLine = cassBilling.Lines[0];
					AssertEquals(false, cassLine.IsRejectedClaimLine);
					AssertEquals("160", cassLine.AirlinePrefix);
					AssertEquals("62144154", cassLine.AWBNumber);
					AssertEquals("23470068510", cassLine.AgentCode);
					AssertEquals(new ZDateTime(2008, 08, 14), cassLine.IssueDate);
					AssertEquals("DUS", cassLine.LoadPortIATA);
					AssertEquals("HKG", cassLine.DischargePortIATA);
					AssertEquals(1190.0M, cassLine.CASSWeight);
					AssertEquals("KG", cassLine.CASSWeightUnit);
					AssertEquals("EUR", cassLine.CASSCostCurrencyCode);
					AssertEquals(3950.80M, cassLine.CASSCostValue);
					AssertEquals(0M, cassLine.CASSCostAdjustedValue);
					AssertEquals(false, cassLine.IsGSTApplicable);
					AssertEquals(100002.01M, cassLine.CASSCostTaxValue);
					AssertEquals(0M, cassLine.CASSCostTaxAdjustedValue);

					Assert(!notificationBuffer.HasErrors);
					Assert(!notificationBuffer.HasWarnings);
				}
			}
		}

		[TestDate(2015, 11, 05)]
		public void TestConverterWithECRRecortIfRejectedClaimLinesIsExpected()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("Precondition: IsRejectedClaimLinesExpected", true, CASSBilling.IsRejectedClaimLinesExpected);

			CASSBilling cassBilling = new CASSBilling(Factory);

			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("AAADE   08080108083108091001EUR 2347006");
				writer.WriteLine("AWM  16062144154  DUS23470068510R   HKG0808140011900KEUR0000003462900000000030000000001808800000000000000000000000000000000000000000000000000000000020000000000000001000 000000395080            08090400000.000000000001320901000020100000000000180");
				writer.WriteLine("ECR Y17267828073  LEJ23470068510808640EUR00000.00000080607P000000047300C000000000000C000000000000C000000000000P000000054668000005000004000000000000000000000000000000000400 K0002150MEX");
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					LoadCASSBilling(cassBilling, notificationBuffer, reader);

					AssertEquals(new ZDateTime(2008, 08, 01), cassBilling.BillingPeriodStart);
					AssertEquals(new ZDateTime(2008, 08, 31), cassBilling.BillingPeriodEnd);
					AssertEquals(new ZDateTime(2008, 09, 10), cassBilling.BillingDate);

					AssertEquals(2, cassBilling.Lines.Count);
					CASSBillingLine cassLine = cassBilling.Lines[0];
					AssertEquals("160", cassLine.AirlinePrefix);
					AssertEquals("62144154", cassLine.AWBNumber);
					AssertEquals("23470068510", cassLine.AgentCode);
					AssertEquals(new ZDateTime(2008, 08, 14), cassLine.IssueDate);
					AssertEquals("DUS", cassLine.LoadPortIATA);
					AssertEquals("HKG", cassLine.DischargePortIATA);
					AssertEquals(1190.0M, cassLine.CASSWeight);
					AssertEquals("KG", cassLine.CASSWeightUnit);
					AssertEquals("EUR", cassLine.CASSCostCurrencyCode);
					AssertEquals(3950.80M, cassLine.CASSCostValue);
					AssertEquals(0M, cassLine.CASSCostAdjustedValue);
					AssertEquals(false, cassLine.IsGSTApplicable);
					AssertEquals(100002.01M, cassLine.CASSCostTaxValue);
					AssertEquals(0M, cassLine.CASSCostTaxAdjustedValue);

					cassLine = cassBilling.Lines[1];
					AssertEquals("172", cassLine.AirlinePrefix);
					AssertEquals("67828073", cassLine.AWBNumber);
					AssertEquals("23470068510", cassLine.AgentCode);
					AssertEquals(new ZDateTime(2008, 06, 07), cassLine.IssueDate);
					AssertEquals("LEJ", cassLine.LoadPortIATA);
					AssertEquals("MEX", cassLine.DischargePortIATA);
					AssertEquals(215.0M, cassLine.CASSWeight);
					AssertEquals("KG", cassLine.CASSWeightUnit);
					AssertEquals("EUR", cassLine.CASSCostCurrencyCode);
					AssertEquals(1015.68M, cassLine.CASSCostValue);
					AssertEquals(0M, cassLine.CASSCostAdjustedValue);
					AssertEquals(true, cassLine.IsGSTApplicable);
					AssertEquals(50000.04M, cassLine.CASSCostTaxValue);
					AssertEquals(0M, cassLine.CASSCostTaxAdjustedValue);

					Assert(!notificationBuffer.HasErrors);
					Assert(!notificationBuffer.HasWarnings);
				}
			}
		}

		public void TestConverter_ManyRecordsForOneMAWB()
		{
			CASSBilling cassBilling = new CASSBilling(Factory);

			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("AAADE   08080108083108091001EUR 2347006");
				writer.WriteLine("AWM  08170639310  FRA23470066001R   MEL0812100026910KEUR0000014800500000000000000000002540560000000000000000000000000000000000000000000000000000000000000000000000000000 000000738436            09010800000.000000000009956700000000000000000");
				writer.WriteLine("CCO  08170639310  FRA23470066001F08067EUR00000.00000081210P000001480050C000000000000C000000000000C000000000000P000000254056000000000000000000000000000000000000000000995670 K0026910MEL");
				writer.WriteLine("CCR  08170639310  FRA23470066001F08067EUR00000.00000081210P000001441550C000000000000C000000000000C000000000000P000000249974000000000000000000000000000000000000000000969770 K0026910MEL");
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					LoadCASSBilling(cassBilling, notificationBuffer, reader);

					AssertEquals(1, cassBilling.Lines.Count);
					CASSBillingLine cassLine = cassBilling.Lines[0];
					AssertEquals("081", cassLine.AirlinePrefix);
					AssertEquals("70639310", cassLine.AWBNumber);
					AssertEquals(14601.90M, cassLine.CASSCostValue);
					AssertEquals(-7384.36M, cassLine.CASSCostAdjustedValue);
					AssertEquals(false, cassLine.IsGSTApplicable);
					AssertEquals(0M, cassLine.CASSCostTaxValue);
					AssertEquals(0M, cassLine.CASSCostTaxAdjustedValue);

					Assert(!notificationBuffer.HasErrors);
					Assert(!notificationBuffer.HasWarnings);
				}
			}
		}

		[TestDate(2015, 11, 05)]
		public void TestConverter_RecordsDifferentDatesAndPorts()
		{
			CASSBilling cassBilling = new CASSBilling(Factory);
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("AAADE   08080108083108091001EUR 2347006");
				writer.WriteLine("DCO  02095636343  NUE234700685106K0732EUR00000.00000081127P000000052700C000000000000C000000000000C000000000000P000000009630000000000000000000003162000000000000000000008438 K0001000LOS");
				writer.WriteLine("DCR Y02095636343  NUE234700685106K0732EUR00000.00000081127P000000019500C000000000000C000000000000C000000000000P000000014180000000005823000000001463000000000278000000003038 K0001000FRA");
				writer.WriteLine("DCO N12954710456  CGN23470065382016353EUR00000.00000081130P000000110050C000000000000C000000000000C000000000000P000000063900000000000000000000000000000000000000000000000000 K0007100KGL");
				writer.WriteLine("DCR N12954710456  CGN23470065382016353EUR00000.00000081127P000000110050C000000000000C000000000000C000000000000P000000063900000000000000000000000000000000000000000000000000 K0007100KGL");
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					LoadCASSBilling(cassBilling, notificationBuffer, reader);

					AssertEquals(2, cassBilling.Lines.Count);
					CASSBillingLine cassLine = cassBilling.Lines[0];
					AssertEquals("020", cassLine.AirlinePrefix);
					AssertEquals("95636343", cassLine.AWBNumber);
					AssertEquals("NUE", cassLine.LoadPortIATA);
					AssertEquals("FRA", cassLine.DischargePortIATA);
					AssertEquals(291.79M, cassLine.CASSCostValue);
					AssertEquals(-507.30M, cassLine.CASSCostAdjustedValue);
					AssertEquals(true, cassLine.IsGSTApplicable);
					AssertEquals(55.45M, cassLine.CASSCostTaxValue);
					AssertEquals(0M, cassLine.CASSCostTaxAdjustedValue);

					cassLine = cassBilling.Lines[1];
					AssertEquals("129", cassLine.AirlinePrefix);
					AssertEquals("54710456", cassLine.AWBNumber);
					AssertEquals(new ZDateTime(2008, 11, 27), cassLine.IssueDate);
					AssertEquals(1739.50M, cassLine.CASSCostValue);
					AssertEquals(-1739.50M, cassLine.CASSCostAdjustedValue);
					AssertEquals(false, cassLine.IsGSTApplicable);
					AssertEquals(0M, cassLine.CASSCostTaxValue);
					AssertEquals(0M, cassLine.CASSCostTaxAdjustedValue);

					Assert(!notificationBuffer.HasErrors);
					Assert(notificationBuffer.HasWarnings);
					notificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer, "AWB 02095636343 records have different Discharge Port IATA Codes.");
				}
			}
		}

		public void TestConverter_RecordsDifferentCurrencies()
		{
			CASSBilling cassBilling = new CASSBilling(Factory);
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("AAADE   08080108083108091001EUR 2347006");
				writer.WriteLine("CCO  08170639310  FRA23470066001F08067EUR00000.00000081210P000001480050C000000000000C000000000000C000000000000P000000254056000000000000000000000000000000000000000000995670 K0026910MEL");
				writer.WriteLine("CCR  08170639310  FRA23470066001F08067AUD00000.00000081210P000001441550C000000000000C000000000000C000000000000P000000249974000000000000000000000000000000000000000000969770 K0026910MEL");
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					LoadCASSBilling(cassBilling, notificationBuffer, reader);

					AssertEquals(1, cassBilling.Lines.Count);
					CASSBillingLine cassLine = cassBilling.Lines[0];
					AssertEquals("081", cassLine.AirlinePrefix);
					AssertEquals("70639310", cassLine.AWBNumber);
					AssertEquals("EUR", cassLine.CASSCostCurrencyCode);
					AssertEquals(0M, cassLine.CASSCostValue);
					AssertEquals(-7384.36M, cassLine.CASSCostAdjustedValue);
					AssertEquals(false, cassLine.IsGSTApplicable);
					AssertEquals(0M, cassLine.CASSCostTaxValue);
					AssertEquals(0M, cassLine.CASSCostTaxAdjustedValue);

					Assert(notificationBuffer.HasErrors);
					Assert(!notificationBuffer.HasWarnings);
					notificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer, "AWB 08170639310 records have different currencies.");
				}
			}
		}

		public void TestConverter_ImportRecords()
		{
			CASSBilling cassBilling = new CASSBilling(Factory);
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("AA2JP054000004  10080110081510082301JPY");
				writer.WriteLine("IBI        00533186 NNJP054000004006A00670158700        MGANRTDL06191008041008050001400KUSD0087.67000000000041560000000500000000000230000000000000000000000003000Y000000000077440000000044000000000000  120000000021  670000000067140000000041000000000000");
				writer.WriteLine("IBO        00533186 NNJP054000004006A00670158756        MGANRTDL06191008041008050001400KUSD0087.67000000000041560000000500000000000230000000000000000000000003000Y000000000077440000000044000000000000  120000000021  670000000067140000000041000000000000");
				writer.WriteLine("IBR        00533186 NNJP054000004006A00670158756        MGANRTDL06191008041008050001400KUSD0087.67000000000041560000000500000000000230000000000000000000000003000Y000000000077440000000044000000000000  120000000021  670000000067140000000041000000000000");
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					LoadCASSBilling(cassBilling, notificationBuffer, reader);

					AssertEquals(false, cassBilling.IsExportBilling);
					AssertEquals(true, cassBilling.IsImportBilling);
					AssertEquals(new ZDateTime(2010, 08, 01), cassBilling.BillingPeriodStart);
					AssertEquals(new ZDateTime(2010, 08, 15), cassBilling.BillingPeriodEnd);
					AssertEquals(new ZDateTime(2010, 08, 23), cassBilling.BillingDate);

					AssertEquals(2, cassBilling.Lines.Count);
					CASSBillingLine cassLine = cassBilling.Lines[0];
					AssertEquals("006", cassLine.AirlinePrefix);
					AssertEquals("70158700", cassLine.AWBNumber);
					AssertEquals("JP054000004", cassLine.AgentCode);
					AssertEquals(ZDateTime.Empty, cassLine.IssueDate);
					AssertEquals(new ZDateTime(2010, 08, 04), cassLine.DateOfArrival);
					AssertEquals(new ZDateTime(2010, 08, 05), cassLine.DateOfDelivery);
					AssertEquals("MGA", cassLine.LoadPortIATA);
					AssertEquals("NRT", cassLine.DischargePortIATA);
					AssertEquals(140.0m, cassLine.CASSWeight);
					AssertEquals("KG", cassLine.CASSWeightUnit);
					AssertEquals("JPY", cassLine.CASSCostCurrencyCode);
					AssertEquals(13699997748.10m, cassLine.CASSCostValue);
					AssertEquals(0m, cassLine.CASSCostAdjustedValue);
					AssertEquals(false, cassLine.IsGSTApplicable);
					AssertEquals(0m, cassLine.CASSCostTaxValue);
					AssertEquals(0m, cassLine.CASSCostTaxAdjustedValue);

					cassLine = cassBilling.Lines[1];
					AssertEquals("006", cassLine.AirlinePrefix);
					AssertEquals("70158756", cassLine.AWBNumber);
					AssertEquals("JP054000004", cassLine.AgentCode);
					AssertEquals(ZDateTime.Empty, cassLine.IssueDate);
					AssertEquals(new ZDateTime(2010, 08, 04), cassLine.DateOfArrival);
					AssertEquals(new ZDateTime(2010, 08, 05), cassLine.DateOfDelivery);
					AssertEquals("MGA", cassLine.LoadPortIATA);
					AssertEquals("NRT", cassLine.DischargePortIATA);
					AssertEquals(140.0m, cassLine.CASSWeight);
					AssertEquals("KG", cassLine.CASSWeightUnit);
					AssertEquals("JPY", cassLine.CASSCostCurrencyCode);
					AssertEquals(13699997748.10m, cassLine.CASSCostValue);
					AssertEquals(-13699997748.10m, cassLine.CASSCostAdjustedValue);
					AssertEquals(false, cassLine.IsGSTApplicable);
					AssertEquals(0m, cassLine.CASSCostTaxValue);
					AssertEquals(0m, cassLine.CASSCostTaxAdjustedValue);

					Assert(!notificationBuffer.HasErrors);
					Assert(!notificationBuffer.HasWarnings);
				}
			}
		}

		public void TestConverter_MissingHeader()
		{
			CASSBilling cassBilling = new CASSBilling(Factory);
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("IBI        00533186 NNJP054000004006A00670158700        MGANRTDL06191008041008050001400KUSD0087.67000000000041560000000500000000000230000000000000000000000003000Y000000000077440000000044000000000000  120000000021  670000000067140000000041000000000000");
				writer.WriteLine("IBO        00533186 NNJP054000004006A00670158756        MGANRTDL06191008041008050001400KUSD0087.67000000000041560000000500000000000230000000000000000000000003000Y000000000077440000000044000000000000  120000000021  670000000067140000000041000000000000");
				writer.WriteLine("IBR        00533186 NNJP054000004006A00670158756        MGANRTDL06191008041008050001400KUSD0087.67000000000041560000000500000000000230000000000000000000000003000Y000000000077440000000044000000000000  120000000021  670000000067140000000041000000000000");
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					LoadCASSBilling(cassBilling, notificationBuffer, reader);

					AssertEquals(false, cassBilling.IsExportBilling);
					AssertEquals(true, cassBilling.IsImportBilling);

					AssertEquals(2, cassBilling.Lines.Count);
					CASSBillingLine cassLine = cassBilling.Lines[0];
					AssertEquals("006", cassLine.AirlinePrefix);
					AssertEquals("70158700", cassLine.AWBNumber);
					AssertEquals("JP054000004", cassLine.AgentCode);
					AssertEquals(ZDateTime.Empty, cassLine.IssueDate);
					AssertEquals(new ZDateTime(2010, 08, 04), cassLine.DateOfArrival);
					AssertEquals(new ZDateTime(2010, 08, 05), cassLine.DateOfDelivery);
					AssertEquals("MGA", cassLine.LoadPortIATA);
					AssertEquals("NRT", cassLine.DischargePortIATA);
					AssertEquals(140.0M, cassLine.CASSWeight);
					AssertEquals("KG", cassLine.CASSWeightUnit);
					AssertEquals("", cassLine.CASSCostCurrencyCode);
					AssertEquals(13699997748.10M, cassLine.CASSCostValue);
					AssertEquals(0M, cassLine.CASSCostAdjustedValue);
					AssertEquals(false, cassLine.IsGSTApplicable);
					AssertEquals(0M, cassLine.CASSCostTaxValue);
					AssertEquals(0M, cassLine.CASSCostTaxAdjustedValue);

					cassLine = cassBilling.Lines[1];
					AssertEquals("006", cassLine.AirlinePrefix);
					AssertEquals("70158756", cassLine.AWBNumber);
					AssertEquals("JP054000004", cassLine.AgentCode);
					AssertEquals(ZDateTime.Empty, cassLine.IssueDate);
					AssertEquals(new ZDateTime(2010, 08, 04), cassLine.DateOfArrival);
					AssertEquals(new ZDateTime(2010, 08, 05), cassLine.DateOfDelivery);
					AssertEquals("MGA", cassLine.LoadPortIATA);
					AssertEquals("NRT", cassLine.DischargePortIATA);
					AssertEquals(140.0M, cassLine.CASSWeight);
					AssertEquals("KG", cassLine.CASSWeightUnit);
					AssertEquals("", cassLine.CASSCostCurrencyCode);
					AssertEquals(13699997748.10M, cassLine.CASSCostValue);
					AssertEquals(-13699997748.10M, cassLine.CASSCostAdjustedValue);
					AssertEquals(false, cassLine.IsGSTApplicable);
					AssertEquals(0M, cassLine.CASSCostTaxValue);
					AssertEquals(0M, cassLine.CASSCostTaxAdjustedValue);

					Assert(notificationBuffer.HasErrors);
					Assert(!notificationBuffer.HasWarnings);
					notificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer, "A header record with AA2 or AA3 Record ID is expected before the import records with IBI, IBR or IBO Record ID.");
				}
			}
		}

		public new void TestImportOfLongStrings()
		{
			Assert(true);
		}

		public void TestErrorMessage()
		{
			CASSBilling cassBilling = new CASSBilling(Factory);
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("AAADE   08080108083108091001EUR 2347006");
				writer.WriteLine("AWM  16062144154  DUS23470068510R   HKG0808140011900KEUR0000003462900000000030000000001808800000000000000000000000000000000000000000000000000000000020000000000000001000 000000395080            08090400000.000000000001320901000020100000000000180");
				writer.WriteLine("DCO Y17267828073  LEJ23470068510808640EUR00000.00000080607P000000047300C000000000100C000000000000C000000000200P000000021618200000000002000000000300000000000000000000000400 K0002150MEX");
				writer.WriteLine("DCR N17267828073  LEJ23470068510808640EUR00000.00000080607P000000047300C000000000000C000000000000C000000000000P000000054668000005000004000000000000000000000000000000000400 K0002150MEX");
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					LoadCASSBilling(cassBilling, notificationBuffer, reader);

					Assert(!notificationBuffer.HasErrors);
					Assert(!notificationBuffer.HasWarnings);

					cassBilling.CostHeader.ExportLines[0].WeightChargePP = 250.00M;
					CASSHOTFileDataAdapter adapter = new CASSHOTFileDataAdapter(notificationBuffer);
					adapter.Fill(cassBilling);

					Assert(notificationBuffer.HasErrors);
					AssertContains("Error: Cannot refresh costs, as one or more file line(s) have error.", notificationBuffer.AsString);
				}
			}
		}

		[ExpectNoExceptions("NullReferenceException should not occur")]
		public void TestCASSBillingCostHeaderLinesEmpty_InvalidLineType()
		{
			CASSBilling cassBilling = new CASSBilling(Factory);
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("ABC  16062144154  DUS23470068510R   HKG0808140011900KEUR0000003462900000000030000000001808800000000000000000000000000000000000000000000000000000000020000000000000001000 000000395080            08090400000.000000000001320901000020100000000000180");

				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					LoadCASSBilling(cassBilling, notificationBuffer, reader);

					Assert("No Errors while uploading file", !notificationBuffer.HasErrors);
					Assert("No Warnings while uploading file", !notificationBuffer.HasWarnings);
				}
			}
		}

		public void TestCASSBillingCostHeaderLinesEmpty_ValidLineType()
		{
			CASSBilling cassBilling = new CASSBilling(Factory);
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("AWM  16062144154  DUS23470068510R   HKG0808140011900KEUR0000003462900000000030000000001808800000000000000000000000000000000000000000000000000000000020000000000000001000 000000395080            08090400000.000000000001320901000020100000000000180");

				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					LoadCASSBilling(cassBilling, notificationBuffer, reader);

					Assert("No Errors while uploading file", !notificationBuffer.HasErrors);
					Assert("Expecting Warnings while uploading file", notificationBuffer.HasWarnings);
					notificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer, "A header record with AAA Record ID is expected before the export records with AWM, CCO, CCR, DCO, DCR or ECR Record ID.");
				}
			}
		}

		public void TestCASSBillingCostHeaderLinesEmpty_InvalidLineTypeFollowedByValidLineType()
		{
			CASSBilling cassBilling = new CASSBilling(Factory);
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("ABC  16062144154  DUS23470068510R   HKG0808140011900KEUR0000003462900000000030000000001808800000000000000000000000000000000000000000000000000000000020000000000000001000 000000395080            08090400000.000000000001320901000020100000000000180");
				writer.WriteLine("AWM  16062144154  DUS23470068510R   HKG0808140011900KEUR0000003462900000000030000000001808800000000000000000000000000000000000000000000000000000000020000000000000001000 000000395080            08090400000.000000000001320901000020100000000000180");

				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					LoadCASSBilling(cassBilling, notificationBuffer, reader);

					Assert("No Errors while uploading file", !notificationBuffer.HasErrors);
					Assert("Expecting Warnings while uploading file", notificationBuffer.HasWarnings);
					notificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer, "A header record with AAA Record ID is expected before the export records with AWM, CCO, CCR, DCO, DCR or ECR Record ID.");
				}
			}
		}

		public void TestCASSBillingCostHeaderLinesEmpty_ValidLineTypeFollowedByInvalidLineType()
		{
			CASSBilling cassBilling = new CASSBilling(Factory);
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("AWM  16062144154  DUS23470068510R   HKG0808140011900KEUR0000003462900000000030000000001808800000000000000000000000000000000000000000000000000000000020000000000000001000 000000395080            08090400000.000000000001320901000020100000000000180");
				writer.WriteLine("ABC  16062144154  DUS23470068510R   HKG0808140011900KEUR0000003462900000000030000000001808800000000000000000000000000000000000000000000000000000000020000000000000001000 000000395080            08090400000.000000000001320901000020100000000000180");

				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					LoadCASSBilling(cassBilling, notificationBuffer, reader);

					Assert("No Errors while uploading file", !notificationBuffer.HasErrors);
					Assert("Expecting Warnings while uploading file", notificationBuffer.HasWarnings);
					notificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer, "A header record with AAA Record ID is expected before the export records with AWM, CCO, CCR, DCO, DCR or ECR Record ID.");
				}
			}
		}

		public void TestCASSBillingCostHeaderLinesPresent_InvalidLineType()
		{
			CASSBilling cassBilling = new CASSBilling(Factory);
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("AAADE   08080108083108091001EUR 2347006");
				writer.WriteLine("ABC  16062144154  DUS23470068510R   HKG0808140011900KEUR0000003462900000000030000000001808800000000000000000000000000000000000000000000000000000000020000000000000001000 000000395080            08090400000.000000000001320901000020100000000000180");

				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					LoadCASSBilling(cassBilling, notificationBuffer, reader);

					Assert("No Errors while uploading file", !notificationBuffer.HasErrors);
					Assert("No Warnings while uploading file", !notificationBuffer.HasWarnings);
				}
			}
		}

		[ExpectException(typeof(NotSupportedException))]
		public new void TestRootElementName()
		{
			AssertEquals(ExpectedRootElementName, GetNewBizObjXmlDataAdapter().RootElementName);
		}

		public void TestImportDataWithAdjustedCASSCost()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				AccountingConfigurationRegistry.Instance.CASSCostImportAllowedDiscrepancy.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 400M);
				var creator = new TestObjectCreator(Factory);
				creator.CreateTestPeriods(ZDateTime.Today);

				CASSChargeCodeCollection cASSMaps = new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
				var cassChargeCode = cASSMaps.AddNew();
				cassChargeCode.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				cassChargeCode.CASSType = "ALL";
				cassChargeCode.ChargeCodePK = creator.FRT.PK;
				cassChargeCode.CASSComponentCode = "ALL";
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, cASSMaps);

				var testObjectCreator = new TestObjectCreator(Factory);
				var testAirLinePrefix = "176";
				var airLineOrg = creator.AALSHI;
				var airline = testObjectCreator.CreateAirLine(testAirLinePrefix);
				airLineOrg.MiscServ.OM_RM_Airline = airline.PK;

				OrgContact contact1 = Factory.NewWithValidTestData<OrgContact>();
				contact1.OC_OH = airLineOrg.PK;

				var testAirLinePrefix2 = "695";
				var airLineOrg2 = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "AUSNEW");
				var airline2 = testObjectCreator.CreateAirLine(testAirLinePrefix2);
				airLineOrg2.MiscServ.OM_RM_Airline = airline2.PK;

				OrgContact contact2 = Factory.NewWithValidTestData<OrgContact>();
				contact2.OC_OH = airLineOrg2.PK;

				creator.GLHeader1.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
				creator.GLHeader1.AG_Description = "Test Account";
				AccountingConfigurationRegistry.Instance.CASSGLAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, creator.GLHeader1.PK.ToGuid());

				var registryValue = new CASSFileImportDefaultTaxID();
				registryValue.ZeroRatedTaxID = creator.GSTFREE1.PK;
				registryValue.StandardRatedTaxID = creator.GST1.PK;
				AccountingConfigurationRegistry.Instance.CASSFileImportDefaultTaxID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

				Factory.Save();

				MemoryStream testStream = new MemoryStream();
				using (StreamWriter writer = new StreamWriter(testStream))
				{
					writer.WriteLine("AWM N17686086184  CLT01196080152R   CCU1002250000700KUSD0000000196000000000000000000000034170000000000000000000000000000000000000000000000000000000000000000000000000980                         10030900001.000000000000000000000000000000000            ");
					writer.WriteLine("AWM N69566676912  CLT01196080152R   PEN1002220001500KUSD0000001317000000000000000000000112500000000000000000000000000000000000000000000000000000000000000000000000006585                         10030500001.000000000001086150000000000000000            ");
					writer.Flush();
					testStream.Position = 0;
					using (StreamReader reader = new StreamReader(testStream))
					{
						CASSBilling cassBilling = new CASSBilling(Factory);
						CASSHOTFileDataImporter importer = new CASSHOTFileDataImporter(cassBilling);

						bool result = importer.ImportData(reader, "", new NotificationBuffer(), SourceInfo.EmptySourceInfo);
						var adapter = new CASSHOTFileDataAdapter(new NotificationBuffer());
						adapter.Fill(cassBilling);

						AssertEquals(0, cassBilling.Lines.Count);
						AssertEquals(2, cassBilling.HiddenLines.Count);
						AssertEquals(2, cassBilling.ExportLines.Count);

						var cassCostExportLine = cassBilling.ExportLines[0];
						cassCostExportLine.WeightChargePP = 1000M;
						cassCostExportLine.AdjustmentReasonType = cassCostExportLine.AdjustmentReasonTypeList[0].Code;
						cassCostExportLine.AdjustmentReason = cassCostExportLine.AdjustmentReasonList[0].Code;
						cassCostExportLine.AdjustmentReasonComment = "Test Comment";

						adapter.Fill(cassBilling);

						AssertEquals(1, cassBilling.Lines.Count);
						AssertEquals(1, cassBilling.HiddenLines.Count);

						cassBilling.CreateInvoices();
						AssertEquals(2, cassBilling.APTransactions.Count);
						AssertEquals(1, cassBilling.APTransactions[0].Lines.Count);
						AssertEquals(1, cassBilling.APTransactions[1].Lines.Count);
					}
				}
			}
		}

		#region Implementation

		protected override void AssertExportFromValueObjectNotSupportedException()
		{
			Assert(true);
		}

		protected override bool IsImportFromValueObjectSupported
		{
			get { return true; }
		}

		protected override bool IsExportToValueObjectSupported
		{
			get { return false; }
		}

		protected override bool IsExportToCollectionSupported
		{
			get
			{
				return false;
			}
		}

		protected override bool IsCreateOrUpdateFromValueObjectSupported
		{
			get { return false; }
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return string.Empty; }
		}

		protected override string ExpectedRootElementName
		{
			get { return string.Empty; }
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			return null;
		}

		protected override ValueObjectDataAdapter<CASSBilling, CASSCostHeader> GetNewBizObjXmlDataAdapter()
		{
			var notifications = new NotificationBuffer();
			return new CASSHOTFileDataAdapter(notifications);
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return null;
		}

		protected override IValueObject PopulateValueObject(Type valueType, int fieldPopulateDepth)
		{
			var costheader = new CASSCostHeader();
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("AAADE   08080108083108091001EUR 2347006");
				writer.WriteLine("AWM  16062144154  DUS23470068510R   HKG0808140011900KEUR0000003462900000000030000000001808800000000000000000000000000000000000000000000000000000000020000000000000001000 000000395080            08090400000.000000000001320901000020100000000000180");
				writer.WriteLine("DCO Y17267828073  LEJ23470068510808640EUR00000.00000080607P000000047300C000000000100C000000000000C000000000200P000000021618200000000002000000000300000000000000000000000400 K0002150MEX");
				writer.WriteLine("DCR N17267828073  LEJ23470068510808640EUR00000.00000080607P000000047300C000000000000C000000000000C000000000000P000000054668000005000004000000000000000000000000000000000400 K0002150MEX");
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					var converter = new CASSHOTFileConverter(notificationBuffer, Factory);
					converter.ImportFlatFile(costheader, new CASSHOTFileFormat(), reader);
				}
			}
			return costheader;
		}

		protected override CASSBilling NewBusinessObjectFromIValueObject(IValueObject value)
		{
			var bizo = new CASSBilling(Factory);
			bizo.Initialize(value as CASSCostHeader);
			return bizo;
		}

		protected override CASSBilling NewBusinessObject()
		{
			var cassCost = new CASSCostHeader();
			cassCost.InitializeAsExportCASS();
			var bizo = new CASSBilling(Factory);
			bizo.Initialize(cassCost);
			return bizo;
		}

		void LoadCASSBilling(CASSBilling cassBilling, NotificationBuffer notificationBuffer, StreamReader reader)
		{
			var converter = new CASSHOTFileConverter(notificationBuffer, Factory);
			converter.ImportFlatFile(cassBilling.CostHeader, new CASSHOTFileFormat(), reader);

			CASSHOTFileDataAdapter adapter = new CASSHOTFileDataAdapter(notificationBuffer);
			adapter.Fill(cassBilling);
		}

		#endregion
	}
}
