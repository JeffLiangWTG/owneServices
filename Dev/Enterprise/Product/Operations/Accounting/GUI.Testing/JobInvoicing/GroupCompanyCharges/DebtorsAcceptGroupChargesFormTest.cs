using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.GlobalChargeCode;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	[TestedType(typeof(DebtorsAcceptGroupChargesForm))]
	public class DebtorsAcceptGroupChargesFormTest : ZFormBasherTest
	{
		[TestDate(2004, 04, 04, 04, 04, 04)]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAcceptButton()
		{
			var shipment = TestObjectCreator.CreateShipment("SHIP100216", "NZAKL", "AUSYD");
			shipment.JS_INCO = ZString.Empty;

			var debtorCompany = TestObjectCreator.CreateCompanyAndBranch("NZAKL");
			var creditorCompany = TestObjectCreator.CreateCompanyAndBranch("AUSYD");

			RatingTestHelper.ChargeCodes.CreateGlobalCharge("GLBFRT");

			Factory.Save();
			var autoRatingCalculationDesc = @"GLBFRT: Base Rate AUD 100.00

GLBFRT Global Charge

Charge located in TESNAMAKL client rate with the following details:

Mode:			LCL
Charge Code Group:	FRT
Start Date:		04 April 2004
End Date:		04 October 2004
Origin:			NZ
Destination:		AU
Commodity Code:		GEN
Currency:		USD
Autorated for:		Shipment SHIP100216
Leg:			NZAKL-AUSYD


User:		CargoWise Support
Time:		04-Apr-04 04:04:04";

			using (Env.SetTemporaryUserContext(TestObjectCreator.GetUserContext(creditorCompany)))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				var debtor = debtorCompany.OrgProxy;
				debtor.OH_IsDebtor = true;
				job.LocalChargesPK = debtor.PK;

				RatingTestHelper.NewClientRateWithSingleRateLine(debtor, "LCL", "LCL", "NZ", "AU", "GLBFRT", 100m);
				Factory.Save();

				new AutoRatingStarter(shipment, new LoggerDecorator()).ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);

				var jobCharge = (Charge)job.Charges.Single();
				var message = "Pre-condition: AutoRating should produce expected charge from client rate";
				AssertEquals(message, "GLBFRT", jobCharge.ChargeCode.AC_Code);
				AssertEquals(message, creditorCompany.PK, jobCharge.ChargeCode.AC_GC);
				AssertEquals(message, 100m, jobCharge.JR_OSSellAmt);
				AssertEquals(message, 1, jobCharge.SellPaymentBases.Count);
				AssertMultilineASCIIEquals(message, autoRatingCalculationDesc, jobCharge.RevenueCalculationDescription.ToUTF8());
			}

			Factory.Save();

			using (Env.SetTemporaryUserContext(TestObjectCreator.GetUserContext(debtorCompany)))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				var groupCompanyChargesForJob = new GroupCompanyChargesForJob(job);
				AssertEquals("Pre-condition", 0, job.Charges.Count);

				using (var form = new DebtorsAcceptGroupChargesForm(groupCompanyChargesForJob))
				{
					form.Show();

					var gridCount = form.DebtorChargesCollectionExposed.List;
					AssertEquals("The Grid should show the single Group Gompany Sell Charges", 1, gridCount.Count);
					AssertEquals("Create", ((GroupCompanyCharge)gridCount[0]).AcceptActionDescription);

					form.DebtorChargesCollectionExposed.SelectAllElements();
					form.AcceptButton.PerformClick();
				}

				var jobCharge = (Charge)job.Charges.Single();
				AssertEquals("Should map to local charge", "GLBFRT", jobCharge.ChargeCode.AC_Code);
				AssertEquals("Should map to local charge", debtorCompany.PK, jobCharge.ChargeCode.AC_GC);
				AssertEquals(100m, jobCharge.JR_OSCostAmt);
				AssertEquals("Debtor owes Creditor Company", creditorCompany.OrgProxy, jobCharge.CostAccount);
				AssertEquals("Should Clone Payment Basis as Cost", 1, jobCharge.CostPaymentBases.Count);
				AssertMultilineASCIIEquals(autoRatingCalculationDesc, jobCharge.CostCalculationDescription.ToUTF8().Trim());

				groupCompanyChargesForJob = new GroupCompanyChargesForJob(job);
				using (var form = new DebtorsAcceptGroupChargesForm(groupCompanyChargesForJob))
				{
					form.Show();

					var gridCount = form.DebtorChargesCollectionExposed.List;
					AssertEquals("The Grid should show the single Group Gompany Sell Charges", 1, gridCount.Count);
					AssertEquals("No Action", ((GroupCompanyCharge)gridCount[0]).AcceptActionDescription);
				}

				job.Charges[0].Delete();
				Factory.Save();

				groupCompanyChargesForJob = new GroupCompanyChargesForJob(job);
				using (var form = new DebtorsAcceptGroupChargesForm(groupCompanyChargesForJob))
				{
					form.Show();

					var gridCount = form.DebtorChargesCollectionExposed.List;
					AssertEquals("The Grid should show the single Group Gompany Sell Charges", 1, gridCount.Count);
					AssertEquals("Create", ((GroupCompanyCharge)gridCount[0]).AcceptActionDescription);

					form.DebtorChargesCollectionExposed.SelectAllElements();
					form.AcceptButton.PerformClick();
				}

				jobCharge = (Charge)job.Charges.Single();
				AssertNotNull("Accepting Charges as Costs should produce a Charge", jobCharge);
			}
		}

		[TestDate(2004, 04, 04, 04, 04, 04)]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAcceptButton_CostBasedCalculator()
		{
			var creditor = TestObjectCreator.Creditor1;
			var consol = TestObjectCreator.CreateConsol();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			consol.JK_OA_ShippingLineAddress = creditor.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.Transports.MostInterestingTransport.CreditorPK = ZGuid.Empty;

			var shipment = TestObjectCreator.CreateShipment("SHIP100216", consol: consol);

			var creditorCompany = TestObjectCreator.CreateCompanyAndBranch("NZAKL");
			var debtorCompany = TestObjectCreator.CreateCompanyAndBranch("AUSYD");

			var chargeCode = RatingTestHelper.ChargeCodes.CreateGlobalCharge("GLBCHG");

			Factory.Save();

			#region Calculation Descriptions
			var costCalculationDescription = @"GLBCHG: Base Rate USD 100.00

GLBCHG Global Charge

Charge located in ZCreditor1 (Consol C001 --> Creditor/Co-Loader) cost with the following details:

Mode:			LCL
Charge Code Group:	FRT
Start Date:		04 April 2004
End Date:		04 October 2004
Origin:			AU
Commodity Code:		GEN
Currency:		USD
Autorated for:		Shipment SHIP100216
Leg:			AUSYD-NZAKL


User:		CargoWise Support
Time:		04-Apr-04 04:04:04";

			var revenueCalculationDescription = @"GLBCHG: 105.00% of (Base Rate USD 100.00)

GLBCHG Global Charge

Charge located in TESNAMAKL1 client rate with the following details:

Payment Term:		Collect
Mode:			LCL
Charge Code Group:	FRT
Start Date:		04 April 2004
End Date:		04 October 2004
Origin:			AU
Commodity Code:		GEN
Currency:		USD
Based On:
	ZCreditor1 (Consol C001 --> Creditor/Co-Loader) cost
Autorated for:		Shipment SHIP100216
Leg:			AUSYD-NZAKL


User:		CargoWise Support
Time:		04-Apr-04 04:04:04";

			#endregion

			using (Env.SetTemporaryUserContext(TestObjectCreator.GetUserContext(creditorCompany)))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				var localClient = TestObjectCreator.CreateOrgHeader("NZCLIENT", true, true, "NZAKL");
				job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;

				var costing = RatingTestHelper.NewCosting(TestObjectCreator.Creditor1);
				costing.AddRateEntryWithFlatRateLine("LCL", "LCL", "AU", "", chargeCode.AC_Code, 100m);

				var clientRate = RatingTestHelper.NewClientRate(localClient);
				var clientRateEntry = clientRate.AddRateEntry("LCL", "LCL", "AU", "");
				var localChargeCode = chargeCode.ChildChargeCodes.Single(x => x.AC_GC == creditorCompany.PK);
				var rateLine = clientRateEntry.AddRateLine(localChargeCode, CompanyTariffOrCostBasedCalculator.CostBasedCode);
				rateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 5;

				Factory.Save();

				new AutoRatingStarter(shipment, new LoggerDecorator()).ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);

				var jobCharge = job.Charges.Cast<Charge>().Single(c => c.ChargeCode.AC_Code == chargeCode.AC_Code);

				CombineAssertions("Pre-condition: AutoRating should produce expected charge from costing", () =>
				{
					AssertEquals(creditorCompany.PK, jobCharge.ChargeCode.AC_GC);
					AssertEquals(100m, jobCharge.JR_OSCostAmt);
					AssertEquals(105m, jobCharge.JR_OSSellAmt);
					AssertEquals(1, jobCharge.CostPaymentBases.Count);
					AssertEquals(1, jobCharge.SellPaymentBases.Count);
					AssertMultilineASCIIEquals(costCalculationDescription, jobCharge.CostCalculationDescription.ToUTF8());
					AssertMultilineASCIIEquals(revenueCalculationDescription, jobCharge.RevenueCalculationDescription.ToUTF8());
				});

				jobCharge.JR_OH_SellAccount = debtorCompany.GC_OH_OrgProxy;
			}

			Factory.Save();

			using (Env.SetTemporaryUserContext(TestObjectCreator.GetUserContext(debtorCompany)))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				AssertEquals("Pre-condition", 0, job.Charges.Count);

				var groupCompanyChargesForJob = new GroupCompanyChargesForJob(job);
				using (var form = new DebtorsAcceptGroupChargesForm(groupCompanyChargesForJob))
				{
					form.Show();

					var gridCount = form.DebtorChargesCollectionExposed.List.Count;
					AssertEquals("The Grid should show the single Group Gompany Sell Charges", 1, gridCount);

					form.DebtorChargesCollectionExposed.SelectAllElements();
					form.AcceptButton.PerformClick();
				}

				var jobCharge = job.Charges.Cast<Charge>().Single(c => c.ChargeCode.AC_Code == chargeCode.AC_Code);

				CombineAssertions("Accepted Group Company Charges as Costs", () =>
				{
					AssertEquals(debtorCompany.PK, jobCharge.ChargeCode.AC_GC);
					AssertEquals(105m, jobCharge.JR_OSCostAmt);
					AssertEquals(1, jobCharge.CostPaymentBases.Count);
					AssertEquals(creditorCompany.OrgProxy, jobCharge.CostAccount);
					AssertMultilineASCIIEquals(revenueCalculationDescription, jobCharge.CostCalculationDescription.ToUTF8());
				});
			}
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAcceptButton_ShowsErrorsForMissingChargeCode()
		{
			var shipment = TestObjectCreator.CreateShipment("SHIP100216", "NZAKL", "AUSYD");
			shipment.JS_INCO = ZString.Empty;
			shipment.JS_ActualWeight = 8.5m;

			var debtorCompany = TestObjectCreator.CreateCompanyAndBranch("NZAKL");
			var creditorCompany = TestObjectCreator.CreateCompanyAndBranch("AUSYD");

			var globalChargeCode = RatingTestHelper.ChargeCodes.CreateGlobalCharge("GLBFRT", UnitCalculator.Code);

			Factory.Save();

			using (Env.SetTemporaryUserContext(TestObjectCreator.GetUserContext(creditorCompany)))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				var debtor = debtorCompany.OrgProxy;
				debtor.OH_IsDebtor = true;
				job.LocalChargesPK = debtor.PK;

				var clientRate = RatingTestHelper.NewClientRate(debtor);
				var rateEntry = clientRate.AddRateEntry("LCL", "LCL", "NZ", "AU");
				rateEntry.RateLines.RemoveAndDeleteAll();

				var rateLine1 = rateEntry.AddRateLine("GLBFRT", FlatCalculator.Code);
				rateLine1.GetCalculator<FlatCalculator>().BaseRate = 500m;

				var rateLine2 = rateEntry.AddRateLine("BAF", FlatCalculator.Code);
				rateLine2.GetCalculator<FlatCalculator>().BaseRate = 80m;

				var rateLine3 = rateEntry.AddRateLine("CAF", UnitCalculator.Code, QuantityUnit.KG);
				rateLine3.GetCalculator<UnitCalculator>().PerUnit = 9m;

				Factory.Save();

				new AutoRatingStarter(shipment, new LoggerDecorator()).ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);

				AssertEquals(3, job.Charges.Count);
			}

			using (Env.SetTemporaryUserContext(TestObjectCreator.GetUserContext(debtorCompany)))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				globalChargeCode.ChildChargeCodes.Single(x => x.AC_GC == debtorCompany.PK).Delete();
				globalChargeCode.Factory.Save();

				var groupCompanyChargesForJob = new GroupCompanyChargesForJob(job);
				using (var form = new DebtorsAcceptGroupChargesForm(groupCompanyChargesForJob))
				{
					form.Show();

					var gridCount = form.DebtorChargesCollectionExposed.List.Count;
					AssertEquals("The Grid should show Group Gompany Sell Charges", 3, gridCount);
					form.DebtorChargesCollectionExposed.SelectAllElements();
					form.AcceptButton.PerformClick();
				}

				AssertEquals("Could not accept any charge due to errors on BAF and CAF not being mapped.", 0, job.Charges.Count);
			}

			var userNotification = UnitTestUserNotification.Instance.LastMessage.Text.Trim();
			var expectedUserNotification = "Please select valid Group Company Charges from the grid.";
			AssertMultilineASCIIEquals(expectedUserNotification, userNotification);
		}

		[TestDate(2004, 04, 04, 04, 04, 04)]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAcceptButton_UsesIntercompanyMappings()
		{
			var shipment = TestObjectCreator.CreateShipment("SHIP100216", "AUSYD", "NZAKL");
			shipment.JS_INCO = ZString.Empty;

			var debtorCompany = TestObjectCreator.CreateCompanyAndBranch("NZAKL");
			var creditorCompany = TestObjectCreator.CreateCompanyAndBranch("AUSYD");

			var globalChargeCode = "GLBCHG";
			RatingTestHelper.ChargeCodes.CreateGlobalCharge(globalChargeCode);

			Factory.Save();

			using (Env.SetTemporaryUserContext(TestObjectCreator.GetUserContext(creditorCompany)))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				var localChargeCode = RatingTestHelper.ChargeCodes.New("NZLOCAL", "AP Mapped Intercompany Charge Code", FlatCalculator.Code);

				var mapping = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
				mapping.YG_Code = globalChargeCode;
				var pivot = mapping.PivotWithOverrideLocalClientCollection.AddNew();
				pivot.YP_TYPE = LedgerTypes.AccountsReceivable;
				pivot.YP_AC = localChargeCode.PK;

				var debtor = debtorCompany.OrgProxy;
				debtor.OH_IsDebtor = true;
				job.LocalChargesPK = debtor.PK;
				RatingTestHelper.NewClientRateWithSingleRateLine(debtor, "LCL", "LCL", "AU", "", globalChargeCode, 100m);

				Factory.Save();

				AssertEquals(globalChargeCode, localChargeCode.GetGlobalChargeCode(LedgerTypes.AccountsReceivable, null).AC_Code);

				new AutoRatingStarter(shipment, new LoggerDecorator()).ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);

				var jobCharge = (Charge)job.Charges.Single();
				var message = "Pre-condition: AutoRating should have rated the local charge code";
				AssertEquals(message, globalChargeCode, jobCharge.ChargeCode.AC_Code);
				AssertEquals(message, creditorCompany.PK, jobCharge.ChargeCode.AC_GC);
			}

			using (Env.SetTemporaryUserContext(TestObjectCreator.GetUserContext(debtorCompany)))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				var mappedLocalChargeCode = RatingTestHelper.ChargeCodes.New("AULOCAL", "AP Mapped Intercompany Charge Code", FlatCalculator.Code);

				var mapping = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
				mapping.YG_Code = globalChargeCode;
				var pivot = mapping.PivotWithOverrideLocalClientCollection.AddNew();
				pivot.YP_TYPE = LedgerTypes.AccountsPayable;
				pivot.YP_AC = mappedLocalChargeCode.PK;

				Factory.Save();

				using (var form = new DebtorsAcceptGroupChargesForm(new GroupCompanyChargesForJob(job)))
				{
					form.Show();
					form.DebtorChargesCollectionExposed.SelectAllElements();
					form.AcceptButton.PerformClick();

					var jobCharge = (Charge)job.Charges.Single();
					AssertEquals("Charge Code should be for the current company", debtorCompany.PK, jobCharge.ChargeCode.AC_GC);
					AssertEquals("NZLOCAL -> GLBCHG -> AULOCAL", mappedLocalChargeCode.AC_Code, jobCharge.ChargeCode.AC_Code);
				}
			}
		}

		public void TestAutoratingOptions_FormDefaults()
		{
			var shipment = TestObjectCreator.CreateShipment("SHIP100216", "AUSYD", "NZAKL");
			Factory.Save();

			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				var groupCompanyChargesForJob = new GroupCompanyChargesForJob(job);
				var expected = GroupCompanyChargesForJob.AutoratingOptionsCode.AutoRateCosts;
				var message = "Job should default to AutoRateCosts rather than no option";
				AssertEquals(message, expected, groupCompanyChargesForJob.AutoratingOption);

				using (var form = new DebtorsAcceptGroupChargesForm(groupCompanyChargesForJob))
				{
					form.Show();

					expected = GroupCompanyChargesForJob.AutoratingOptionsCode.NoAutorating;
					message = "But the field can be set to something else";
					groupCompanyChargesForJob.AutoratingOption = expected;

					AssertEquals(message, expected, groupCompanyChargesForJob.AutoratingOption);
				}

				using (var form = new DebtorsAcceptGroupChargesForm(groupCompanyChargesForJob))
				{
					form.Show();

					message = "even after the form is closed and reopened, as long as they're using the same factory.";
					AssertEquals(message, expected, groupCompanyChargesForJob.AutoratingOption);
				}
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var groupCompanyChargesForJob = new GroupCompanyChargesForJob(job);

			return new DebtorsAcceptGroupChargesForm(groupCompanyChargesForJob);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		TestHelper RatingTestHelper => ratingTestHelper ?? (ratingTestHelper = new TestHelper(Factory));
		TestHelper ratingTestHelper;

		#endregion
	}
}
