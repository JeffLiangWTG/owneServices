using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class DueDateCalculationTest : TestCaseWithFactory
	{
		public readonly int AcceptableTimeLimit = 50;

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		protected TestObjectCreator fTestObjectCreator;

		public void TestGetDueDate()
		{
			var testInvoiceDate = new DateTime(2003, 3, 25);

			AssertEquals("Due Date", ComparisonResult.Equal, CompareDates(ZDateTime.Now, DueDateCalculation.GetDueDate(Factory, new InvoiceTerm("A", "", (ZByte)20), testInvoiceDate), AcceptableTimeLimit));
			AssertEquals("Due Date", new DateTime(2003, 4, 1), DueDateCalculation.GetDueDate(Factory, new InvoiceTerm(Constants.InvoiceTerms.FromInvoiceDate, "", (ZByte)7), testInvoiceDate));
			AssertEquals("Due Date", new DateTime(2003, 4, 30), DueDateCalculation.GetDueDate(Factory, new InvoiceTerm(Constants.InvoiceTerms.FromMonthEnd, "", (ZByte)30), testInvoiceDate));
			AssertEquals("Due Date", testInvoiceDate, DueDateCalculation.GetDueDate(Factory, new InvoiceTerm(Constants.InvoiceTerms.CashOnDelivery, "", (ZByte)0), testInvoiceDate));
			AssertEquals("Due Date", testInvoiceDate, DueDateCalculation.GetDueDate(Factory, new InvoiceTerm(Constants.InvoiceTerms.PaymentInAdvance, "", (ZByte)0), testInvoiceDate));
		}

		public void TestGetCalculateDate()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.CompanyData.OB_APPaymentTerms = Constants.InvoiceTerms.CashOnDelivery;
			organisation.CompanyData.OB_APPaymentTermDays = 0;

			var invoiceDate = new DateTime(2020, 1, 1);
			var documentReceivedDate = new DateTime(2020, 1, 2);

			using (AccountingMasterFilesRegistry.Instance.APInvoiceDueDateCalculationRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(documentReceivedDate, DueDateCalculation.GetCalculateDate(organisation.CompanyData.GetAPTerm(), invoiceDate, documentReceivedDate));

				organisation.CompanyData.OB_APPaymentTerms = Constants.InvoiceTerms.FromShipmentDate;
				AssertEquals(invoiceDate, DueDateCalculation.GetCalculateDate(organisation.CompanyData.GetAPTerm(), invoiceDate, documentReceivedDate));

				organisation.CompanyData.OB_APPaymentTerms = Constants.InvoiceTerms.FromCustomsClearanceDate;
				AssertEquals(invoiceDate, DueDateCalculation.GetCalculateDate(organisation.CompanyData.GetAPTerm(), invoiceDate, ZDateTime.Empty));

				organisation.CompanyData.OB_APPaymentTerms = Constants.InvoiceTerms.LaterOfShipmentOrInvoiceDate;
				AssertEquals(invoiceDate, DueDateCalculation.GetCalculateDate(organisation.CompanyData.GetAPTerm(), invoiceDate, ZDateTime.Empty));
			}

			using (AccountingMasterFilesRegistry.Instance.APInvoiceDueDateCalculationRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(invoiceDate, DueDateCalculation.GetCalculateDate(organisation.CompanyData.GetAPTerm(), invoiceDate, documentReceivedDate));
			}
		}

		public void TestGetDueDateFromEndOfWeek()
		{
			var testInvoiceDate = new DateTime(2019, 4, 17);
			AssertEquals("Default Invoice Terms End of Week should be Sunday", DayOfWeekCodeList.Codes.Sunday,
				OrganisationRegistry.Instance.InvoiceTermsEndOfWeek.Value);
			AssertEquals("Due Date From End of Week", new DateTime(2019, 4, 26), DueDateCalculation.GetDueDate(Factory, new InvoiceTerm(Constants.InvoiceTerms.FromWeekEnd, "", (ZByte)5), testInvoiceDate));

			OrganisationRegistry.Instance.InvoiceTermsEndOfWeek.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				DayOfWeekCodeList.Codes.Monday);
			AssertEquals("Due Date From End of Week", new DateTime(2019, 4, 27), DueDateCalculation.GetDueDate(Factory, new InvoiceTerm(Constants.InvoiceTerms.FromWeekEnd, "", (ZByte)5), testInvoiceDate));

			OrganisationRegistry.Instance.InvoiceTermsEndOfWeek.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				DayOfWeekCodeList.Codes.Wednesday);
			AssertEquals("Due Date From End of Week", new DateTime(2019, 4, 22), DueDateCalculation.GetDueDate(Factory, new InvoiceTerm(Constants.InvoiceTerms.FromWeekEnd, "", (ZByte)5), testInvoiceDate));

			OrganisationRegistry.Instance.InvoiceTermsEndOfWeek.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				DayOfWeekCodeList.Codes.Friday);
			AssertEquals("Due Date From End of Week", new DateTime(2019, 4, 24), DueDateCalculation.GetDueDate(Factory, new InvoiceTerm(Constants.InvoiceTerms.FromWeekEnd, "", (ZByte)5), testInvoiceDate));
		}
		[TestDate(2010, 11, 10)]
		public void TestGetDueDateForMonthsFromInvoiceCycleDate()
		{
			var organisation = Factory.New<OrgHeader>();
			var arTerm = organisation.CompanyData.LoadARTermForAllInvoiceTypes();
			arTerm.PY_InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
			arTerm.PY_InvoiceDays = 2;

			AssertEquals("Due Date if payment date can't be calculated.", ZDateTime.Now, DueDateCalculation.GetDueDate(Factory, organisation.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty), new ZDateTime(2010, 01, 01)));

			var termCycle1 = arTerm.ARTermsCycles.AddNew();
			var termCycle2 = arTerm.ARTermsCycles.AddNew();
			var termCycle3 = arTerm.ARTermsCycles.AddNew();

			termCycle1.P5_ToDay = 10;
			termCycle2.P5_ToDay = 20;

			termCycle1.P5_PaymentDay = 5;
			termCycle2.P5_PaymentDay = 15;
			termCycle3.P5_PaymentDay = 25;

			arTerm.MarkAsNeedingValidationIncludingChildren();
			arTerm.RunPreSaveValidation();
			AssertNoErrors("Precondition: terms were set up correctly.", arTerm);

			AssertEquals("Due Date", new DateTime(2010, 4, 5), DueDateCalculation.GetDueDate(Factory, organisation.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty), new ZDateTime(2010, 01, 01)));
			AssertEquals("Due Date", new DateTime(2010, 4, 5), DueDateCalculation.GetDueDate(Factory, organisation.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty), new ZDateTime(2010, 01, 10)));

			AssertEquals("Due Date", new DateTime(2010, 4, 15), DueDateCalculation.GetDueDate(Factory, organisation.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty), new ZDateTime(2010, 01, 11)));
			AssertEquals("Due Date", new DateTime(2010, 4, 15), DueDateCalculation.GetDueDate(Factory, organisation.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty), new ZDateTime(2010, 01, 20)));

			AssertEquals("Due Date", new DateTime(2010, 4, 25), DueDateCalculation.GetDueDate(Factory, organisation.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty), new ZDateTime(2010, 01, 21)));
			AssertEquals("Due Date", new DateTime(2010, 4, 25), DueDateCalculation.GetDueDate(Factory, organisation.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty), new ZDateTime(2010, 01, 31)));

			arTerm.PY_InvoiceDays = 0;
			arTerm.ARTermsCycles.DeleteAll();
			termCycle1 = arTerm.ARTermsCycles.AddNew();
			termCycle1.P5_PaymentDay = 31;
			arTerm.MarkAsNeedingValidationIncludingChildren();
			arTerm.RunPreSaveValidation();
			AssertNoErrors("Precondition: terms were set up correctly.", arTerm);
			AssertEquals("Due Date", new DateTime(2010, 1, 31), DueDateCalculation.GetDueDate(Factory, organisation.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty), new ZDateTime(2010, 01, 01)));

			termCycle1.P5_PaymentDay = 30;
			arTerm.MarkAsNeedingValidationIncludingChildren();
			arTerm.RunPreSaveValidation();
			AssertNoErrors("Precondition: terms were set up correctly.", arTerm);
			AssertEquals("Due Date", new DateTime(2010, 2, 28), DueDateCalculation.GetDueDate(Factory, organisation.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty), new ZDateTime(2010, 01, 01)));

			termCycle1.P5_PaymentDay = 29;
			arTerm.MarkAsNeedingValidationIncludingChildren();
			arTerm.RunPreSaveValidation();
			AssertNoErrors("Precondition: terms were set up correctly.", arTerm);
			AssertEquals("Due Date", new DateTime(2010, 2, 28), DueDateCalculation.GetDueDate(Factory, organisation.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty), new ZDateTime(2010, 01, 01)));

			termCycle1.P5_PaymentDay = 28;
			arTerm.MarkAsNeedingValidationIncludingChildren();
			arTerm.RunPreSaveValidation();
			AssertNoErrors("Precondition: terms were set up correctly.", arTerm);
			AssertEquals("Due Date", new DateTime(2010, 2, 28), DueDateCalculation.GetDueDate(Factory, organisation.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty), new ZDateTime(2010, 01, 01)));

			arTerm.PY_InvoiceDays = 12;
			termCycle1.P5_PaymentDay = 27;
			arTerm.MarkAsNeedingValidationIncludingChildren();
			arTerm.RunPreSaveValidation();
			AssertNoErrors("Precondition: terms were set up correctly.", arTerm);
			AssertEquals("Due Date", new DateTime(2011, 2, 27), DueDateCalculation.GetDueDate(Factory, organisation.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty), new ZDateTime(2010, 01, 01)));
		}

		[TestDate(2010, 11, 10)]
		public void TestGetDueDateFromPaymentCycleDate()
		{
			var organisation = Factory.New<OrgHeader>();
			var arTerm = organisation.CompanyData.LoadARTermForAllInvoiceTypes();
			arTerm.PY_InvoiceTerm = Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle;
			arTerm.PY_InvoiceDays = 2;

			AssertEquals("Due Date if payment date can't be calculated.", ZDateTime.Now, DueDateCalculation.GetDueDate(Factory, organisation.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty), new ZDateTime(2010, 01, 01)));

			var paymentCycle1 = arTerm.ARPaymentCycles.AddNew();
			var paymentCycle2 = arTerm.ARPaymentCycles.AddNew();
			var paymentCycle3 = arTerm.ARPaymentCycles.AddNew();

			paymentCycle1.P5_PaymentDay = 5;
			paymentCycle2.P5_PaymentDay = 15;
			paymentCycle3.P5_PaymentDay = 25;

			AssertEquals("Cycle #", (ZByte)1, paymentCycle1.P5_Cycle);
			AssertEquals("Cycle #", (ZByte)2, paymentCycle2.P5_Cycle);
			AssertEquals("Cycle #", (ZByte)3, paymentCycle3.P5_Cycle);

			arTerm.MarkAsNeedingValidationIncludingChildren();
			arTerm.RunPreSaveValidation();
			AssertNoErrors("Precondition: terms were set up correctly.", arTerm);

			AssertEquals("Due Date", new DateTime(2010, 1, 5), DueDateCalculation.GetDueDate(Factory, organisation.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty), new ZDateTime(2010, 01, 01)));
			AssertEquals("Due Date", new DateTime(2010, 2, 5), DueDateCalculation.GetDueDate(Factory, organisation.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty), new ZDateTime(2010, 01, 24)));

			AssertEquals("Due Date", new DateTime(2010, 1, 15), DueDateCalculation.GetDueDate(Factory, organisation.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty), new ZDateTime(2010, 01, 04)));
			AssertEquals("Due Date", new DateTime(2010, 1, 15), DueDateCalculation.GetDueDate(Factory, organisation.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty), new ZDateTime(2010, 01, 13)));

			AssertEquals("Due Date", new DateTime(2010, 1, 25), DueDateCalculation.GetDueDate(Factory, organisation.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty), new ZDateTime(2010, 01, 14)));
			AssertEquals("Due Date", new DateTime(2010, 1, 25), DueDateCalculation.GetDueDate(Factory, organisation.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty), new ZDateTime(2010, 01, 23)));

			arTerm.PY_InvoiceDays = 0;
			arTerm.ARPaymentCycles.DeleteAll();
			paymentCycle1 = arTerm.ARPaymentCycles.AddNew();
			paymentCycle1.P5_PaymentDay = 31;
			arTerm.MarkAsNeedingValidationIncludingChildren();
			arTerm.RunPreSaveValidation();
			AssertNoErrors("Precondition: terms were set up correctly.", arTerm);
			AssertEquals("Due Date", new DateTime(2010, 3, 31), DueDateCalculation.GetDueDate(Factory, organisation.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty), new ZDateTime(2010, 02, 01)));

			paymentCycle1.P5_PaymentDay = 30;
			arTerm.MarkAsNeedingValidationIncludingChildren();
			arTerm.RunPreSaveValidation();
			AssertNoErrors("Precondition: terms were set up correctly.", arTerm);
			AssertEquals("Due Date", new DateTime(2010, 3, 30), DueDateCalculation.GetDueDate(Factory, organisation.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty), new ZDateTime(2010, 02, 01)));

			paymentCycle1.P5_PaymentDay = 29;
			arTerm.MarkAsNeedingValidationIncludingChildren();
			arTerm.RunPreSaveValidation();
			AssertNoErrors("Precondition: terms were set up correctly.", arTerm);
			AssertEquals("Due Date", new DateTime(2010, 3, 29), DueDateCalculation.GetDueDate(Factory, organisation.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty), new ZDateTime(2010, 02, 01)));

			paymentCycle1.P5_PaymentDay = 28;
			arTerm.MarkAsNeedingValidationIncludingChildren();
			arTerm.RunPreSaveValidation();
			AssertNoErrors("Precondition: terms were set up correctly.", arTerm);
			AssertEquals("Due Date", new DateTime(2010, 2, 28), DueDateCalculation.GetDueDate(Factory, organisation.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty), new ZDateTime(2010, 02, 01)));

			arTerm.PY_InvoiceDays = 12;
			paymentCycle1.P5_PaymentDay = 27;
			arTerm.MarkAsNeedingValidationIncludingChildren();
			arTerm.RunPreSaveValidation();
			AssertNoErrors("Precondition: terms were set up correctly.", arTerm);
			AssertEquals("Due Date", new DateTime(2010, 3, 27), DueDateCalculation.GetDueDate(Factory, organisation.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty), new ZDateTime(2010, 02, 17)));
		}

		public void TestGetDueDateWithInvalidDateTime()
		{
			var testInvoiceDate = ZDateTime.Invalid;

			AssertEquals("Due Date", ComparisonResult.Equal, CompareDates(ZDateTime.Now, DueDateCalculation.GetDueDate(Factory, new InvoiceTerm("A", "", (ZByte)20), testInvoiceDate), AcceptableTimeLimit));
			AssertEquals("Due Date", ComparisonResult.Equal, CompareDates(ZDateTime.Now, DueDateCalculation.GetDueDate(Factory, new InvoiceTerm(Constants.InvoiceTerms.FromInvoiceDate, "", (ZByte)7), testInvoiceDate), AcceptableTimeLimit));
			AssertEquals("Due Date", ComparisonResult.Equal, CompareDates(ZDateTime.Now, DueDateCalculation.GetDueDate(Factory, new InvoiceTerm(Constants.InvoiceTerms.FromMonthEnd, "", (ZByte)30), testInvoiceDate), AcceptableTimeLimit));
			AssertEquals("Due Date", ComparisonResult.Equal, CompareDates(ZDateTime.Now, DueDateCalculation.GetDueDate(Factory, new InvoiceTerm(Constants.InvoiceTerms.FromWeekEnd, "", (ZByte)10), testInvoiceDate), AcceptableTimeLimit));
			AssertEquals("Due Date", ComparisonResult.Equal, CompareDates(ZDateTime.Now, DueDateCalculation.GetDueDate(Factory, new InvoiceTerm(Constants.InvoiceTerms.CashOnDelivery, "", (ZByte)0), testInvoiceDate), AcceptableTimeLimit));
		}

		public void TestGetDueDateWithEmptyDateTime()
		{
			var testInvoiceDate = ZDateTime.Empty;

			AssertEquals("Due Date", ComparisonResult.Equal, CompareDates(ZDateTime.Now, DueDateCalculation.GetDueDate(Factory, new InvoiceTerm("A", "", (ZByte)20), testInvoiceDate), AcceptableTimeLimit));
			AssertEquals("Due Date", ComparisonResult.Equal, CompareDates(ZDateTime.Now, DueDateCalculation.GetDueDate(Factory, new InvoiceTerm(Constants.InvoiceTerms.FromInvoiceDate, "", (ZByte)7), testInvoiceDate), AcceptableTimeLimit));
			AssertEquals("Due Date", ComparisonResult.Equal, CompareDates(ZDateTime.Now, DueDateCalculation.GetDueDate(Factory, new InvoiceTerm(Constants.InvoiceTerms.FromMonthEnd, "", (ZByte)30), testInvoiceDate), AcceptableTimeLimit));
			AssertEquals("Due Date", ComparisonResult.Equal, CompareDates(ZDateTime.Now, DueDateCalculation.GetDueDate(Factory, new InvoiceTerm(Constants.InvoiceTerms.FromWeekEnd, "", (ZByte)10), testInvoiceDate), AcceptableTimeLimit));
			AssertEquals("Due Date", ComparisonResult.Equal, CompareDates(ZDateTime.Now, DueDateCalculation.GetDueDate(Factory, new InvoiceTerm(Constants.InvoiceTerms.CashOnDelivery, "", (ZByte)0), testInvoiceDate), AcceptableTimeLimit));
		}

		public void TestGetDueDateReturnsInvoiceDateForInvalidPostDate()
		{
			var invoiceDate = ZDateTime.Now;
			var returnedDueDate = DueDateCalculation.GetDueDate(Factory, new InvoiceTerm(Constants.InvoiceTerms.FromPeriodEnd, "", (ZByte)14), invoiceDate);
			AssertEquals("Should return invoice date for post dates that are invalid because of no periods existing for that date", invoiceDate, returnedDueDate);
		}

		#region Implementation

		protected enum ComparisonResult { GreaterThan, LessThan, Equal, None }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		protected ComparisonResult CompareDates(ZDateTime dateToCompare, ZDateTime dateToBeCompared, int comparisonMarginInMilliSeconds)
		{
			var difference = dateToCompare - dateToBeCompared;
			var result = ComparisonResult.None;

			if (Math.Abs(difference.Milliseconds) < comparisonMarginInMilliSeconds)
			{
				result = ComparisonResult.Equal;
			}
			else
			{
				result = (dateToCompare > dateToBeCompared ? ComparisonResult.GreaterThan : ComparisonResult.LessThan);
			}
			return result;
		}

		#endregion

		#region Test Case Delivery and Pickup Term

		JobHeader CreateJobHeader(ForwardingShipment shipment, ForwardingConsol consol, ZDateTime actualDate, ZDateTime estimatedDate)
		{
			var job = TestObjectCreator.CreateJob(shipment);

			if (consol.IsCrossTrade() || consol.IsImport() || consol.IsDomestic())
			{
				shipment.DocsAndCartage.JP_DeliveryCartageCompleted = actualDate;
				shipment.DocsAndCartage.JP_EstimatedDelivery = estimatedDate;
			}
			else if (consol.IsExport())
			{
				shipment.DocsAndCartage.JP_PickupCartageCompleted = actualDate;
				shipment.DocsAndCartage.JP_EstimatedPickup = estimatedDate;
			}

			Factory.Save();
			return job;
		}

		#region Tests when actual and estimated dates given

		public void TestDueDate_GivenInvoiceTermIsDLP_WhenActualDeliveryDateGiven_AndEstimatedDeliveryDateAlsoGiven_ThenDueDateSetFromActualDeliveryDatePlusNDays(ForwardingConsol consol, ForwardingShipment shipment)
		{
			var ndays = 10;
			InvoiceTerm term = new InvoiceTerm(Constants.InvoiceTerms.FromDeliveryOrPickupDate, "DLP", (ZByte)ndays);

			ZDateTime today = ZDateTime.Now;
			ZDateTime estimatedDate = today.AddDays(15);
			ZDateTime actualDate = today.AddDays(5);
			var invoiceDate = today.AddDays(8);
			ZDateTime expectedDueDate = actualDate.AddDays(ndays);

			JobHeader jobHeader = CreateJobHeader(shipment, consol, actualDate, estimatedDate);

			if (consol.IsExport())
			{
				AssertEquals("Preconditions: actual pickup date is given.", shipment.DocsAndCartage.JP_PickupCartageCompleted, actualDate);
				AssertEquals("Preconditions: estimated pickup date is given.", shipment.DocsAndCartage.JP_EstimatedPickup, estimatedDate);
			}
			else
			{
				AssertEquals("Preconditions: actual delivery date is given.", shipment.DocsAndCartage.JP_DeliveryCartageCompleted, actualDate);
				AssertEquals("Preconditions: estimated delivery date is given.", shipment.DocsAndCartage.JP_EstimatedDelivery, estimatedDate);
			}

			var returnedDueDate = DueDateCalculation.GetDueDate(Factory, term, invoiceDate, jobHeader: jobHeader);
			AssertEquals("Due date calculated from actual delivery date plus DLP term days", expectedDueDate.ToBestReadableDateString(), returnedDueDate.ToBestReadableDateString());
		}

		public void TestDueDate_GivenInvoiceTermIsDLP_ForDomesticConsol_WhenActualDeliveryDateGiven_AndEstimatedDeliveryDateAlsoGiven_ThenDueDateSetFromActualDeliveryDatePlusNDays()
		{
			var origin = "AUSYD";
			var destination = "AUSYD";

			var consol = TestObjectCreator.CreateConsol(origin, destination);
			var shipment = TestObjectCreator.CreateShipment("S00001", consol);

			Factory.Save();

			AssertEquals("Preconditions: job is domestic.", true, consol.IsDomestic());

			TestDueDate_GivenInvoiceTermIsDLP_WhenActualDeliveryDateGiven_AndEstimatedDeliveryDateAlsoGiven_ThenDueDateSetFromActualDeliveryDatePlusNDays(consol, shipment);
		}

		public void TestDueDate_GivenInvoiceTermIsDLP_ForImportConsol_WhenActualDeliveryDateGiven_AndEstimatedDeliveryDateAlsoGiven_ThenDueDateSetFromActualDeliveryDatePlusNDays()
		{
			var origin = "USLAX";
			var destination = "AUSYD";

			var consol = TestObjectCreator.CreateConsol(origin, destination);
			var shipment = TestObjectCreator.CreateShipment("S00001", consol);

			Factory.Save();

			AssertEquals("Preconditions: job is import.", true, consol.IsImport());

			TestDueDate_GivenInvoiceTermIsDLP_WhenActualDeliveryDateGiven_AndEstimatedDeliveryDateAlsoGiven_ThenDueDateSetFromActualDeliveryDatePlusNDays(consol, shipment);
		}

		public void TestDueDate_GivenInvoiceTermIsDLP_ForCrossTradeConsol_WhenActualDeliveryDateGiven_AndEstimatedDeliveryDateAlsoGiven_ThenDueDateSetFromActualDeliveryDatePlusNDays()
		{
			var origin = "USLAX";
			var destination = "HKHKG";

			var consol = TestObjectCreator.CreateConsol(origin, destination);
			var shipment = TestObjectCreator.CreateShipment("S00001", consol);

			Factory.Save();

			AssertEquals("Preconditions: job is cross trade.", true, consol.IsCrossTrade());

			TestDueDate_GivenInvoiceTermIsDLP_WhenActualDeliveryDateGiven_AndEstimatedDeliveryDateAlsoGiven_ThenDueDateSetFromActualDeliveryDatePlusNDays(consol, shipment);
		}

		public void TestDueDate_GivenInvoiceTermIsDLP_ForExportConsol_WhenActualPickupDateGiven_AndEstimatedPickupDateAlsoGiven_ThenDueDateSetFromActualPickupDatePlusNDays()
		{
			var origin = "AUSYD";
			var destination = "HKHKG";

			var consol = TestObjectCreator.CreateConsol(origin, destination);
			var shipment = TestObjectCreator.CreateShipment("S00001", consol);

			Factory.Save();

			AssertEquals("Preconditions: job is export.", true, consol.IsExport());

			TestDueDate_GivenInvoiceTermIsDLP_WhenActualDeliveryDateGiven_AndEstimatedDeliveryDateAlsoGiven_ThenDueDateSetFromActualDeliveryDatePlusNDays(consol, shipment);
		}

		void AssertDueDate_GivenInvoiceTermDLP_WithDeclarationJob_DeliveryDateWillBeUse(bool isDrawback, bool isEstimateDate, bool isAcutalDate)
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_RL_NKOrigin = "AUCHI";
			declaration.JE_RL_NKFinalDestination = "ITALL";
			declaration.JE_MessageType = isDrawback ? JobMessageTypeList.Codes.Drawback : JobMessageTypeList.Codes.MiscellaneousCustoms;

			var ndays = 10;
			var term = new InvoiceTerm(Constants.InvoiceTerms.FromDeliveryOrPickupDate, "DLP", (ZByte)ndays);
			var today = ZDateTime.Now;
			var expectedDueDate = ZDateTime.Now;
			var invoiceDate = today.AddDays(8);

			if (isEstimateDate)
			{
				var estimatedDeliveryDate = today.AddDays(15);
				var estimatePickupDate = today.AddDays(20);
				expectedDueDate = estimatedDeliveryDate.AddDays(ndays);
				declaration.DocsAndCartage.JP_EstimatedDelivery = estimatedDeliveryDate;
				declaration.DocsAndCartage.JP_EstimatedPickup = estimatePickupDate;
			}

			if (isAcutalDate)
			{
				var actualDeliveryDate = today.AddDays(5);
				var actualPickupDate = today.AddDays(4);
				expectedDueDate = actualDeliveryDate.AddDays(ndays);
				declaration.DocsAndCartage.JP_DeliveryCartageCompleted = actualDeliveryDate;
				declaration.DocsAndCartage.JP_PickupCartageCompleted = actualPickupDate;
			}

			if (!isEstimateDate && !isAcutalDate)
			{
				expectedDueDate = invoiceDate.AddDays(ndays);
			}

			var brokerageJob = TestObjectCreator.CreateJob(declaration);
			brokerageJob.Parent = declaration;
			Factory.Save();

			var returnedDueDate = DueDateCalculation.GetDueDate(Factory, term, invoiceDate, jobHeader: brokerageJob);
			AssertEquals(expectedDueDate.ToBestReadableDateString(), returnedDueDate.ToBestReadableDateString());
		}

		public void TestDueDate_GivenInvoiceTermDLP_WithDeclarationJob_WhenDeliveryAndPickupEstimateAndActualDateGiven_WithMSCShptType_DeliveryDateWillBeUse()
		{
			AssertDueDate_GivenInvoiceTermDLP_WithDeclarationJob_DeliveryDateWillBeUse(false, true, true);
		}

		public void TestDueDate_GivenInvoiceTermDLP_WithDeclarationJob_WhenDeliveryAndPickupEstimateAndActualDateGiven_WithDRWShptType_DeliveryDateWillBeUse()
		{
			AssertDueDate_GivenInvoiceTermDLP_WithDeclarationJob_DeliveryDateWillBeUse(true, true, true);
		}

		public void TestDueDate_GivenInvoiceTermIsDLP_ForPortTransport_WhenCompletionDateIsGiven_AndEstimateDeliveryDateIsGiven_ThenDueDateSetFromCompletionDatePlusNDays()
		{
			var ndays = 10;
			var term = new InvoiceTerm(Constants.InvoiceTerms.FromDeliveryOrPickupDate, "DLP", (ZByte)ndays);

			var today = ZDateTime.Now;
			var estimatedDate = today.AddDays(15);
			var completionDate = today.AddDays(5);
			var invoiceDate = today.AddDays(8);
			var expectedDueDate = completionDate.AddDays(ndays);

			var portTransport = TestObjectCreator.CreateCartage();
			portTransport.JJ_A_JCL = completionDate;
			portTransport.JJ_EstimatedDelivery = estimatedDate;

			var job = TestObjectCreator.CreateJob(portTransport);

			var returnedDueDate = DueDateCalculation.GetDueDate(Factory, term, invoiceDate, jobHeader: job);
			AssertEquals("Due date calculated from completion date plus DLP term days", expectedDueDate.ToBestReadableDateString(), returnedDueDate.ToBestReadableDateString());
			job.Dispose();
		}

		#endregion

		#region Tests when actual delivery/pickup date is given but estimated date not given

		public void TestDueDate_GivenInvoiceTermIsDLP_WhenActualDeliveryDateGiven_AndEstimatedDeliveryDateNotGiven_ThenDueDateSetFromActualDeliveryDatePlusNDays(ForwardingConsol consol, ForwardingShipment shipment)
		{
			var ndays = 10;
			InvoiceTerm term = new InvoiceTerm(Constants.InvoiceTerms.FromDeliveryOrPickupDate, "DLP", (ZByte)ndays);

			ZDateTime today = ZDateTime.Now;
			ZDateTime estimatedDate = new ZDateTime();
			ZDateTime actualDate = today.AddDays(5);
			var invoiceDate = today.AddDays(8);

			ZDateTime expectedDueDate = actualDate.AddDays(ndays);

			JobHeader jobHeader = CreateJobHeader(shipment, consol, actualDate, estimatedDate);

			if (consol.IsExport())
			{
				AssertEquals("Preconditions: actual delivery date is given.", shipment.DocsAndCartage.JP_PickupCartageCompleted, actualDate);
				AssertEquals("Preconditions: estimated delivery date is not given.", shipment.DocsAndCartage.JP_EstimatedPickup.IsEmpty, estimatedDate.IsEmpty);
			}
			else
			{
				AssertEquals("Preconditions: actual delivery date is given.", shipment.DocsAndCartage.JP_DeliveryCartageCompleted, actualDate);
				AssertEquals("Preconditions: estimated delivery date is not given.", shipment.DocsAndCartage.JP_EstimatedDelivery.IsEmpty, estimatedDate.IsEmpty);
			}

			var returnedDueDate = DueDateCalculation.GetDueDate(Factory, term, invoiceDate, jobHeader: jobHeader);
			AssertEquals("Due date calculated from actual delivery date plus DLP term days", expectedDueDate.ToBestReadableDateString(), returnedDueDate.ToBestReadableDateString());
		}

		public void TestDueDate_GivenInvoiceTermIsDLP_ForDomesticConsol_WhenActualDeliveryDateGiven_AndEstimatedDeliveryDateNotGiven_ThenDueDateSetFromActualDeliveryDatePlusNDays()
		{
			var origin = "AUSYD";
			var destination = "AUSYD";

			var consol = TestObjectCreator.CreateConsol(origin, destination);
			var shipment = TestObjectCreator.CreateShipment("S00001", consol);

			Factory.Save();

			AssertEquals("Preconditions: job is domestic.", true, consol.IsDomestic());

			TestDueDate_GivenInvoiceTermIsDLP_WhenActualDeliveryDateGiven_AndEstimatedDeliveryDateNotGiven_ThenDueDateSetFromActualDeliveryDatePlusNDays(consol, shipment);
		}

		public void TestDueDate_GivenInvoiceTermIsDLP_ForImportConsol_WhenActualDeliveryDateGiven_AndEstimatedDeliveryDateNotGiven_ThenDueDateSetFromActualDeliveryDatePlusNDays()
		{
			var origin = "USLAX";
			var destination = "AUSYD";

			var consol = TestObjectCreator.CreateConsol(origin, destination);
			var shipment = TestObjectCreator.CreateShipment("S00001", consol);

			Factory.Save();

			AssertEquals("Preconditions: job is import.", true, consol.IsImport());

			TestDueDate_GivenInvoiceTermIsDLP_WhenActualDeliveryDateGiven_AndEstimatedDeliveryDateNotGiven_ThenDueDateSetFromActualDeliveryDatePlusNDays(consol, shipment);
		}

		public void TestDueDate_GivenInvoiceTermIsDLP_ForCrossTradeConsol_WhenActualDeliveryDateGiven_AndEstimatedDeliveryDateNotGiven_ThenDueDateSetFromActualDeliveryDatePlusNDays()
		{
			var origin = "USLAX";
			var destination = "HKHKG";

			var consol = TestObjectCreator.CreateConsol(origin, destination);
			var shipment = TestObjectCreator.CreateShipment("S00001", consol);

			Factory.Save();

			AssertEquals("Preconditions: job is cross trade.", true, consol.IsCrossTrade());

			TestDueDate_GivenInvoiceTermIsDLP_WhenActualDeliveryDateGiven_AndEstimatedDeliveryDateNotGiven_ThenDueDateSetFromActualDeliveryDatePlusNDays(consol, shipment);
		}

		public void TestDueDate_GivenInvoiceTermIsDLP_ForExportConsol_WhenActualPickupDateGiven_AndEstimatedPickupDateNotGiven_ThenDueDateSetFromActualPickupDatePlusNDays()
		{
			var origin = "AUSYD";
			var destination = "HKHKG";

			var consol = TestObjectCreator.CreateConsol(origin, destination);
			var shipment = TestObjectCreator.CreateShipment("S00001", consol);

			Factory.Save();

			AssertEquals("Preconditions: job is export.", true, consol.IsExport());

			TestDueDate_GivenInvoiceTermIsDLP_WhenActualDeliveryDateGiven_AndEstimatedDeliveryDateNotGiven_ThenDueDateSetFromActualDeliveryDatePlusNDays(consol, shipment);
		}

		public void TestDueDate_GivenInvoiceTermDLP_WithDeclarationJob_WhenDeliveryAndPickupActualDateGiven_WithMSCShptType_DeliveryDateWillBeUse()
		{
			AssertDueDate_GivenInvoiceTermDLP_WithDeclarationJob_DeliveryDateWillBeUse(false, false, true);
		}

		public void TestDueDate_GivenInvoiceTermDLP_WithDeclarationJob_WhenDeliveryAndPickupActualDateGiven_WithDRWShptType_DeliveryDateWillBeUse()
		{
			AssertDueDate_GivenInvoiceTermDLP_WithDeclarationJob_DeliveryDateWillBeUse(true, false, true);
		}

		#endregion

		#region Tests when actual delivery/pickup date not given but estimated date is given

		public void TestDueDate_GivenInvoiceTermIsDLP_WhenActualDeliveryDateNotGiven_AndEstimatedDeliveryDateGiven_ThenDueDateSetFromEstimatedDeliveryDatePlusNDays(ForwardingConsol consol, ForwardingShipment shipment)
		{
			var ndays = 10;
			InvoiceTerm term = new InvoiceTerm(Constants.InvoiceTerms.FromDeliveryOrPickupDate, "DLP", (ZByte)ndays);

			ZDateTime today = ZDateTime.Now;
			ZDateTime estimatedDate = today.AddDays(5);
			ZDateTime actualDate = new ZDateTime();
			var invoiceDate = today.AddDays(8);

			ZDateTime expectedDueDate = estimatedDate.AddDays(ndays);

			JobHeader jobHeader = CreateJobHeader(shipment, consol, actualDate, estimatedDate);

			if (consol.IsExport())
			{
				AssertEquals("Preconditions: actual delivery date is not given.", shipment.DocsAndCartage.JP_PickupCartageCompleted.IsEmpty, actualDate.IsEmpty);
				AssertEquals("Preconditions: estimated delivery date is given.", shipment.DocsAndCartage.JP_EstimatedPickup, estimatedDate);
			}
			else
			{
				AssertEquals("Preconditions: actual delivery date is not given.", shipment.DocsAndCartage.JP_DeliveryCartageCompleted.IsEmpty, actualDate.IsEmpty);
				AssertEquals("Preconditions: estimated delivery date is given.", shipment.DocsAndCartage.JP_EstimatedDelivery, estimatedDate);
			}

			var returnedDueDate = DueDateCalculation.GetDueDate(Factory, term, invoiceDate, jobHeader: jobHeader);
			AssertEquals("Due date calculated from estimated delivery date plus DLP term days", expectedDueDate.ToBestReadableDateString(), returnedDueDate.ToBestReadableDateString());
		}

		public void TestDueDate_GivenInvoiceTermIsDLP_ForDomesticConsol_WhenActualDeliveryDateNotGiven_AndEstimatedDeliveryDateGiven_ThenDueDateSetFromEstimatedDeliveryDatePlusNDays()
		{
			var origin = "AUSYD";
			var destination = "AUSYD";

			var consol = TestObjectCreator.CreateConsol(origin, destination);
			var shipment = TestObjectCreator.CreateShipment("S00001", consol);

			Factory.Save();

			AssertEquals("Preconditions: job is domestic.", true, consol.IsDomestic());
			TestDueDate_GivenInvoiceTermIsDLP_WhenActualDeliveryDateNotGiven_AndEstimatedDeliveryDateGiven_ThenDueDateSetFromEstimatedDeliveryDatePlusNDays(consol, shipment);
		}

		public void TestDueDate_GivenInvoiceTermIsDLP_ForImportConsol_WhenActualDeliveryDateNotGiven_AndEstimatedDeliveryDateGiven_ThenDueDateSetFromEstimatedDeliveryDatePlusNDays()
		{
			var origin = "USLAX";
			var destination = "AUSYD";

			var consol = TestObjectCreator.CreateConsol(origin, destination);
			var shipment = TestObjectCreator.CreateShipment("S00001", consol);

			Factory.Save();

			AssertEquals("Preconditions: job is import.", true, consol.IsImport());
			TestDueDate_GivenInvoiceTermIsDLP_WhenActualDeliveryDateNotGiven_AndEstimatedDeliveryDateGiven_ThenDueDateSetFromEstimatedDeliveryDatePlusNDays(consol, shipment);
		}

		public void TestDueDate_GivenInvoiceTermIsDLP_ForCrossTradeConsol_WhenActualDeliveryDateNotGiven_AndEstimatedDeliveryDateNotGiven_FromEstimatedDeliveryDatePlusNDays()
		{
			var origin = "USLAX";
			var destination = "HKHKG";

			var consol = TestObjectCreator.CreateConsol(origin, destination);
			var shipment = TestObjectCreator.CreateShipment("S00001", consol);

			Factory.Save();

			AssertEquals("Preconditions: job is cross trade.", true, consol.IsCrossTrade());
			TestDueDate_GivenInvoiceTermIsDLP_WhenActualDeliveryDateNotGiven_AndEstimatedDeliveryDateGiven_ThenDueDateSetFromEstimatedDeliveryDatePlusNDays(consol, shipment);
		}

		public void TestDueDate_GivenInvoiceTermIsDLP_ForExportConsol_WhenActualPickupDateNotGiven_AndEstimatedPickupDateGiven_FromEstimatedPickupDatePlusNDays()
		{
			var origin = "AUSYD";
			var destination = "HKHKG";

			var consol = TestObjectCreator.CreateConsol(origin, destination);
			var shipment = TestObjectCreator.CreateShipment("S00001", consol);

			Factory.Save();

			AssertEquals("Preconditions: job is export.", true, consol.IsExport());
			TestDueDate_GivenInvoiceTermIsDLP_WhenActualDeliveryDateNotGiven_AndEstimatedDeliveryDateGiven_ThenDueDateSetFromEstimatedDeliveryDatePlusNDays(consol, shipment);
		}

		public void TestDueDate_GivenInvoiceTermDLP_WithDeclarationJob_WhenDeliveryAndPickupEstimateDateGiven_WithMSCShptType_DeliveryDateWillBeUse()
		{
			AssertDueDate_GivenInvoiceTermDLP_WithDeclarationJob_DeliveryDateWillBeUse(false, true, false);
		}

		public void TestDueDate_GivenInvoiceTermDLP_WithDeclarationJob_WhenDeliveryAndPickupEstimateDateGiven_WithDRWShptType_DeliveryDateWillBeUse()
		{
			AssertDueDate_GivenInvoiceTermDLP_WithDeclarationJob_DeliveryDateWillBeUse(true, true, false);
		}

		public void TestDueDate_GivenInvoiceTermIsDLP_ForPortTransport_WhenCompletionDateNotGiven_AndEstimateDeliveryDateIsGiven_ThenDueDateSetFromDeliveryDatePlusNDays()
		{
			var ndays = 10;
			var term = new InvoiceTerm(Constants.InvoiceTerms.FromDeliveryOrPickupDate, "DLP", (ZByte)ndays);

			var today = ZDateTime.Now;
			var estimatedDate = today.AddDays(5);
			var completionDate = new ZDateTime();
			var invoiceDate = today.AddDays(8);
			var expectedDueDate = estimatedDate.AddDays(ndays);

			var portTransport = TestObjectCreator.CreateCartage();
			portTransport.JJ_A_JCL = completionDate;
			portTransport.JJ_EstimatedDelivery = estimatedDate;

			var job = TestObjectCreator.CreateJob(portTransport);

			var returnedDueDate = DueDateCalculation.GetDueDate(Factory, term, invoiceDate, jobHeader: job);
			AssertEquals("Due date calculated from estimated delivery date plus DLP term days", expectedDueDate.ToBestReadableDateString(), returnedDueDate.ToBestReadableDateString());
			job.Dispose();
		}

		#endregion

		#region Tests when both actual delivery/pickup and estimated delivery/pickup not given

		public void TestDueDate_GivenInvoiceTermIsDLP_WhenActualDeliveryDateNotGiven_AndEstimatedDeliveryDateNotGiven_ThenDueDateSetFromInvoiceDatePlusNDays(ForwardingConsol consol, ForwardingShipment shipment)
		{
			var ndays = 10;
			InvoiceTerm term = new InvoiceTerm(Constants.InvoiceTerms.FromDeliveryOrPickupDate, "DLP", (ZByte)ndays);

			ZDateTime today = ZDateTime.Now;
			ZDateTime estimatedDate = new ZDateTime();
			ZDateTime actualDate = new ZDateTime();
			var invoiceDate = today.AddDays(8);

			ZDateTime expectedDueDate = invoiceDate.AddDays(ndays);

			JobHeader jobHeader = CreateJobHeader(shipment, consol, actualDate, estimatedDate);

			if (consol.IsExport())
			{
				AssertEquals("Preconditions: actual delivery date is not given.", shipment.DocsAndCartage.JP_PickupCartageCompleted.IsEmpty, actualDate.IsEmpty);
				AssertEquals("Preconditions: estimated delivery date is not given.", shipment.DocsAndCartage.JP_EstimatedPickup.IsEmpty, estimatedDate.IsEmpty);
			}
			else
			{
				AssertEquals("Preconditions: actual delivery date is not given.", shipment.DocsAndCartage.JP_DeliveryCartageCompleted.IsEmpty, actualDate.IsEmpty);
				AssertEquals("Preconditions: estimated delivery date is not given.", shipment.DocsAndCartage.JP_EstimatedDelivery.IsEmpty, estimatedDate.IsEmpty);
			}

			var returnedDueDate = DueDateCalculation.GetDueDate(Factory, term, invoiceDate, jobHeader: jobHeader);
			AssertEquals("Due date calculated from today date plus DLP term days", expectedDueDate.ToBestReadableDateString(), returnedDueDate.ToBestReadableDateString());
		}

		public void TestDueDate_GivenInvoiceTermIsDLP_ForDomesticConsol_WhenActualDeliveryDateNotGiven_AndEstimatedDeliveryDateNotGiven_ThenDueDateSetFromInvoiceDatePlusNDays()
		{
			var origin = "AUSYD";
			var destination = "AUSYD";

			var consol = TestObjectCreator.CreateConsol(origin, destination);
			var shipment = TestObjectCreator.CreateShipment("S00001", consol);

			Factory.Save();

			AssertEquals("Preconditions: job is domestic.", true, consol.IsDomestic());
			TestDueDate_GivenInvoiceTermIsDLP_WhenActualDeliveryDateNotGiven_AndEstimatedDeliveryDateNotGiven_ThenDueDateSetFromInvoiceDatePlusNDays(consol, shipment);
		}

		public void TestDueDate_GivenInvoiceTermIsDLP_ForImportConsol_WhenActualDeliveryDateNotGiven_AndEstimatedDeliveryDateNotGiven_ThenDueDateSetFromInvoiceDatePlusNDays()
		{
			var origin = "USLAX";
			var destination = "AUSYD";

			var consol = TestObjectCreator.CreateConsol(origin, destination);
			var shipment = TestObjectCreator.CreateShipment("S00001", consol);

			Factory.Save();

			AssertEquals("Preconditions: job is import.", true, consol.IsImport());
			TestDueDate_GivenInvoiceTermIsDLP_WhenActualDeliveryDateNotGiven_AndEstimatedDeliveryDateNotGiven_ThenDueDateSetFromInvoiceDatePlusNDays(consol, shipment);
		}

		public void TestDueDate_GivenInvoiceTermIsDLP_ForCrossTradeConsol_WhenActualDeliveryDateNotGiven_AndEstimatedDeliveryDateNotGiven_ThenDueDateSetFromInvoiceDatePlusNDays()
		{
			var origin = "USLAX";
			var destination = "HKHKG";

			var consol = TestObjectCreator.CreateConsol(origin, destination);
			var shipment = TestObjectCreator.CreateShipment("S00001", consol);

			Factory.Save();

			AssertEquals("Preconditions: job is cross trade.", true, consol.IsCrossTrade());
			TestDueDate_GivenInvoiceTermIsDLP_WhenActualDeliveryDateNotGiven_AndEstimatedDeliveryDateNotGiven_ThenDueDateSetFromInvoiceDatePlusNDays(consol, shipment);
		}

		public void TestDueDate_GivenInvoiceTermIsDLP_ForExportConsol_WhenActualPickupDateNotGiven_AndEstimatedPickupDateNotGiven_ThenDueDateSetFromInvoiceDatePlusNDays()
		{
			var origin = "AUSYD";
			var destination = "HKHKG";

			var consol = TestObjectCreator.CreateConsol(origin, destination);
			var shipment = TestObjectCreator.CreateShipment("S00001", consol);

			Factory.Save();

			AssertEquals("Preconditions: job is export.", true, consol.IsExport());
			TestDueDate_GivenInvoiceTermIsDLP_WhenActualDeliveryDateNotGiven_AndEstimatedDeliveryDateNotGiven_ThenDueDateSetFromInvoiceDatePlusNDays(consol, shipment);
		}

		public void TestDueDate_GivenInvoiceTermDLP_WithDeclarationJob_NoDateGiven_WithMSCShptType_DeliveryDateWillBeUse()
		{
			AssertDueDate_GivenInvoiceTermDLP_WithDeclarationJob_DeliveryDateWillBeUse(false, false, false);
		}

		public void TestDueDate_GivenInvoiceTermDLP_WithDeclarationJob_NoDateGiven_WithDRWShptType_DeliveryDateWillBeUse()
		{
			AssertDueDate_GivenInvoiceTermDLP_WithDeclarationJob_DeliveryDateWillBeUse(true, false, false);
		}

		#endregion

		#region Test due date should be greater than or equal to invoice date

		public void TestDueDate_GivenInvoiceTermIsDLP_ForExportConsol_WhenInvoiceDateCalculated_AndWhenDueDateCalculated_ThenDueDateMustBeGreaterThanOrEqualToInvoiceDate()
		{
			var ndays = 10;
			InvoiceTerm term = new InvoiceTerm(Constants.InvoiceTerms.FromDeliveryOrPickupDate, "DLP", (ZByte)ndays);

			var origin = "AUSYD";
			var destination = "HKHKG";

			ZDateTime today = ZDateTime.Now;
			ZDateTime estimatedDate = today;
			ZDateTime actualDate = new ZDateTime();
			var invoiceDate = today.AddDays(18);

			ZDateTime expectedDueDate = invoiceDate;

			var consol = TestObjectCreator.CreateConsol(origin, destination);
			var shipment = TestObjectCreator.CreateShipment("S00001", consol);

			Factory.Save();

			JobHeader jobHeader = CreateJobHeader(shipment, consol, actualDate, estimatedDate);

			AssertEquals("Preconditions: job is export.", true, consol.IsExport());
			AssertEquals("Preconditions: actual delivery date is not given.", shipment.DocsAndCartage.JP_PickupCartageCompleted.IsEmpty, actualDate.IsEmpty);
			AssertEquals("Preconditions: estimated delivery date is given.", shipment.DocsAndCartage.JP_EstimatedPickup, estimatedDate);

			var returnedDueDate = DueDateCalculation.GetDueDate(Factory, term, invoiceDate, jobHeader: jobHeader);
			AssertEquals("Due date is calculated from invoice date plus DLP term days", expectedDueDate.ToBestReadableDateString(), returnedDueDate.ToBestReadableDateString());
		}

		public void TestDueDate_GivenInvoiceTermIsDLP_ForExportConsol_WhenInvoiceDateAndDueDateIsInThePast_ThenDueDateSetToInvoiceDate()
		{
			var ndays = 1;
			InvoiceTerm term = new InvoiceTerm(Constants.InvoiceTerms.FromDeliveryOrPickupDate, "DLP", (ZByte)ndays);

			var origin = "AUSYD";
			var destination = "HKHKG";

			ZDateTime today = ZDateTime.Now;
			ZDateTime estimatedDate = today.AddDays(-10);
			ZDateTime actualDate = today.AddDays(-10);
			var invoiceDate = today.AddDays(-5);
			ZDateTime expectedDueDate = invoiceDate;

			var consol = TestObjectCreator.CreateConsol(origin, destination);
			var shipment = TestObjectCreator.CreateShipment("S00001", consol);

			Factory.Save();

			JobHeader jobHeader = CreateJobHeader(shipment, consol, actualDate, estimatedDate);

			AssertEquals("Preconditions: job is export.", true, consol.IsExport());
			AssertLessThan("Preconditions: actual delivery date is in the past.", shipment.DocsAndCartage.JP_PickupCartageCompleted, today);
			AssertLessThan("Preconditions: estimated delivery date is in the past.", shipment.DocsAndCartage.JP_EstimatedPickup, today);
			AssertLessThan("Preconditions: invoice date is in the past.", invoiceDate, today);

			var returnedDueDate = DueDateCalculation.GetDueDate(Factory, term, invoiceDate, jobHeader: jobHeader);
			AssertEquals("Due date is calculated from invoice date", expectedDueDate.ToBestReadableDateString(), returnedDueDate.ToBestReadableDateString());
		}

		[TestDate(2023, 05, 06)]
		public void TestDueDate_GivenInvoiceTermIsDLP_GivenJobHeaderIsNull()
		{
			var term = new InvoiceTerm(Constants.InvoiceTerms.FromDeliveryOrPickupDate, "DLP", (ZByte)1);
			var today = ZDateTime.Now;
			var dueDate = today.AddDays(2);
			AssertNoExceptionThrown("No Exception when jobHeader is null", () => dueDate = DueDateCalculation.GetDueDate(Factory, term, today.AddDays(-5)));
			AssertEquals("Due date is equal to today", ComparisonResult.Equal, CompareDates(today, dueDate, AcceptableTimeLimit));
		}

		#endregion

		#endregion
	}
}
