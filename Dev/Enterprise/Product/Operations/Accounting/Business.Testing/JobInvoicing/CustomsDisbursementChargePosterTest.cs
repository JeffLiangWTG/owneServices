using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class CustomsDisbursementChargePosterTest : TransactionCreatorBaseTest
	{
		public void TestCeateNewChargeWhenDiffIsEmptAndCostAmountIsZero()
		{
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, CC2, Constants.CountryCodes.SouthAfrica))
			{
				RatingDataRegistry.Instance.CustomDeferredChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, CC2.PK.ToGuid());
				RatingDataRegistry.Instance.IncludeCustomDeferredChargeInInvoicing.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).DefaultDebtor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				Factory.Save();

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { GetCustomCharge(CC2, 0, 0, true, testCreditor.PK) },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					APDueDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory,
				};

				customDetail.DataProviders.Add(chargeProvider);

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { CC2.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				var invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);

				AssertEquals("One charge line should have been created", 1, invoicingJob.Charges.Count);
				AssertEquals("It has not been posted for cost", false, invoicingJob.Charges[0].IsCostPosted);
				AssertEquals("It has been posted for revenue", false, invoicingJob.Charges[0].IsRevenuePosted);

				var invoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), GlbCompany.CurrentCompany.LocalCurrency, 1m);
				invoice.AH_OH = TestObjectCreator.ABIGAS.PK;

				var line = (ARInvoiceLine)TestObjectCreator.CreateInvoiceLine(invoice, 0m, invoice.TransactionCurrency, 1m);
				line.AL_AC = CC2.PK;
				line.AL_JH = invoicingJob.PK;
				invoicingJob.Charges[0].JR_AL_ARLine = line.PK;
				AssertEquals("It has been posted for revenue", true, invoicingJob.Charges[0].IsRevenuePosted);

				poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB | ChargePosterBehaviours.APPostDSB, new[] { CC2.PK });
				poster.RaiseInvoices(customDetail);
				AssertEquals("One charge line should have been created", 2, invoicingJob.Charges.Count);
			}
		}

		public void TestCreatingWhenNotAllowedReopenJob_ThrowCustomsInvoiceRaiseException()
		{
			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory,
					APInvoiceNumberAlwaysIncludeChargeCode = true
				};
				customDetail.DataProviders.Add(chargeProvider);

				var job = CreateJob("Z00001000", LocalClient, 5m, Agent, 10M);
				job.JH_ParentID = customDetail.PK;
				job.JH_Status = JobHeaderStatus.Closed.Code;

				Factory.Save();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARAPPostNonDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				Env.Security.ReopenJob.IsAllowed = false;
				Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;

				AssertExceptionThrown(typeof(CustomsInvoiceRaiseException), "You do not have the appropriate security rights to run this function.", delegate
				{ poster.RaiseInvoices(customDetail); }, assertStartsWith: true);
			}
		}

		public void TestCreatingWhenThereIsAlreadyJobHeaderWithRelatedJobs()
		{
			SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var relatedJobParent = Factory.New<MockCustomsJobProvider>();
				relatedJobParent.JobNumber = "TR436878";
				relatedJobParent.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				relatedJobParent.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				var jobForRelatedParent = new Job.Loader(relatedJobParent).TryCreateWithoutMutexForTestOnly();
				jobForRelatedParent.JH_GE = GlbDepartment.CurrentDepartment.PK;

				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.AdditionalJobsToShowChargesFor = new IJobInvoicingPlugIn[] { relatedJobParent };
				var jobForParent = new Job.Loader(customDetail).TryCreateWithoutMutexForTestOnly();
				jobForParent.JH_GE = GlbDepartment.CurrentDepartment.PK;
				// Add some Charge to let Preposting Validation pass
				var jobCharge = jobForParent.Charges.AddNew();
				jobCharge.JR_AC = TestObjectCreator.FRT.PK;

				Factory.Save();

				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.ARPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				AssertNoExceptionThrown(delegate
				{ Factory.Save(); });
			}
		}

		public void TestWhenPrepostingValidationsAreViolated()
		{
			SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var relatedJobParent = Factory.New<MockCustomsJobProvider>();
				relatedJobParent.JobNumber = "TR436878";
				relatedJobParent.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				relatedJobParent.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				var jobForRelatedParent = new Job.Loader(relatedJobParent).TryCreateWithoutMutexForTestOnly();
				jobForRelatedParent.JH_GE = GlbDepartment.CurrentDepartment.PK;

				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.AdditionalJobsToShowChargesFor = new IJobInvoicingPlugIn[] { relatedJobParent };
				var jobForParent = new Job.Loader(customDetail).TryCreateWithoutMutexForTestOnly();
				jobForParent.JH_GE = GlbDepartment.CurrentDepartment.PK;

				Factory.Save();

				Env.OutgoingMailManager.EmailsCreated.Clear();

				try
				{
					var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.ARPostDSB, new[] { chargeCode.PK });
					poster.RaiseInvoices(customDetail);
					Fail("Exception expected");
				}
				catch (CustomsInvoiceRaiseException e)
				{
					AssertContains("The following errors occurred while trying to post ", e.Message);
					AssertContains("transactions on Job TR436877:", e.Message);
				}
			}
		}

		public void TestWhenErrorsConditionsAreDetectedAfterInvoiceIsRaised()
		{
			SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory
				};
				customDetail.DataProviders.Add(chargeProvider);

				Env.OutgoingMailManager.EmailsCreated.Clear();

				LocalClient.MiscServ.OM_ARWHTApplicable = false;
				Agent.MiscServ.OM_ARWHTApplicable = false;

				Job job = CreateJob("Z00001000", LocalClient, 5m, Agent, 10M);
				job.JH_ParentID = customDetail.PK;
				job.JH_ProfitLossReasonCode = ZString.Empty;

				var localCurrency = GlbCompany.CurrentCompany.LocalCurrency;
				var charge1 = CreateCharge(job, CC1, "Charge Code 1", localCurrency, 50m, Creditor1, localCurrency, 100m, LocalClient);
				var charge2 = CreateCharge(job, TestObjectCreator.CC7, "Desc 2", localCurrency, 100M, Creditor1, localCurrency, 200M, TestObjectCreator.LocalClient);

				job.JH_ProfitLossReasonCode = ZString.Empty;
				var plReasonCodes = new JobProfitLossReasonCodeCollection();
				var plReasonCode = plReasonCodes.AddNew();
				plReasonCode.Code = "TST";
				plReasonCode.Description = (NoResString)"Test";
				AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plReasonCodes);

				TestObjectCreator.SetupRegistrySetJobStatusToInvoicedWhenFirstARInvoicePosted(GlbCompany.CurrentCompany.PK.ToGuid(), null);

				var plRequiringReasonParameters = new JobProfitLossRequiringReasonParameters
				{
					ProfitThreshold = 20M,
					LossThreshold = 10m
				};
				plRequiringReasonParameters.JobStatusCollection.AddNew().Code = JobHeaderStatus.JobInvoiced.Code;
				AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plRequiringReasonParameters);

				Factory.Save();

				try
				{
					var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB | ChargePosterBehaviours.ARAPPostNonDSB, new[] { chargeCode.PK });
					poster.RaiseInvoices(customDetail);
					Fail("Exception expected");
				}
				catch (CustomsInvoiceRaiseException e)
				{
					AssertContains("Job TR436877 has errors and transactions posting of AR is canceled.", e.Message);
				}
			}
		}

		public void TestWhenTruckDeclarationIsInRoadShipment()
		{
			var toLevelObjectWithoutDepartmentSupporter = new Mock<IJobInvoicingSupporter>();
			toLevelObjectWithoutDepartmentSupporter.Setup(m => m.OverriddenDepartmentPK).Returns(ZGuid.Empty);
			toLevelObjectWithoutDepartmentSupporter.Setup(m => m.ConsumerType).Returns(JobInvoicingConsumerTypes.Shipment);
			toLevelObjectWithoutDepartmentSupporter.Setup(m => m.TransportMode).Returns((ZString)Constants.TransportModes.Road);
			toLevelObjectWithoutDepartmentSupporter.Setup(m => m.ContainerMode).Returns((ZString)Constants.ContainerModes.LCL);
			toLevelObjectWithoutDepartmentSupporter.Setup(m => m.IsImport).Returns(true);

			var toLevelObjectWithoutDepartment = new Mock<IJobInvoicingPlugIn>();
			toLevelObjectWithoutDepartment.Setup(m => m.InvoicingSupporter).Returns(toLevelObjectWithoutDepartmentSupporter.Object);

			var parent = new Mock<ICustomsJobInfo>();
			parent.Setup(m => m.InvoicingSupporter).Returns(toLevelObjectWithoutDepartmentSupporter.Object);
			parent.Setup(m => m.TopLevelObjectForJobToReference).Returns(toLevelObjectWithoutDepartment.Object);
			parent.Setup(m => m.Factory).Returns(Factory);

			var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, Array.Empty<ZGuid>());
			AssertNotNull("ParentDepartment", poster.GetParentDepartment_ForTestOnly(parent.Object));
		}

		public void TestReturnedIAutoBillingResult()
		{
			var staff = SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false);

				Factory.Save();

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);
				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory,
					AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false)
				};
				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				IAutoBillingResult result = poster.RaiseInvoices(customDetail);

				AssertEquals("no email should have been sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);

				AssertContains("it should have a warning message about unposted AR", "The job does not have an AR invoice posted while there is an AP invoice posted for Customs Disbursement costs.", result.Message);
			}
		}

		public void TestNotifyWhenAPInvoiceIsThereAndARInvoiceIsNotPosted()
		{
			var staff = SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var postingNotificaiton = new AutoPostingNotification(new[] { staff }, false);
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.AutoPostingNotification = postingNotificaiton;

				Factory.Save();

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory,
					AutoPostingNotification = postingNotificaiton
				};
				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.SendEmail, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				AssertEquals("one email should have been sent notifying unposted AR", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertContains("The job does not have an AR invoice posted while there is an AP invoice posted for Customs Disbursement costs.", Env.OutgoingMailManager.EmailsCreated[0].Body);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				postingNotificaiton = new AutoPostingNotification(new[] { staff }, true);
				customDetail.AutoPostingNotification = postingNotificaiton;
				chargeProvider.AutoPostingNotification = postingNotificaiton;
				poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.SendEmail, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				AssertEquals("No email generated because unposted AR notification has been suspended", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		public void TestJR_PaymentDateTakeCorrectValueWithAutoBillingDueDateFromPaymentTermsTrue()
		{
			var dueDateValue = new ZDateTime(2006, 4, 15);
			var invoiceDate = new ZDateTime(2005, 4, 12);
			var invoicingJob = SetUpChargesAndProcessJobForDueDateTesting(dueDateValue, invoiceDate, true);

			AssertEquals("The due date value should equals to the invoice date plus 10 days (CompanyData.OB_APPaymentTermDays)", new ZDateTime(2005, 4, 22), invoicingJob.Charges[0].JR_PaymentDate);
		}

		public void TestJR_PaymentDateTakeCorrectValueWithAutoBillingDueDateFromPaymentTermsFalse()
		{
			var dueDateValue = new ZDateTime(2006, 4, 15);
			var invoiceDate = new ZDateTime(2005, 4, 12);
			var invoicingJob2 = SetUpChargesAndProcessJobForDueDateTesting(dueDateValue, invoiceDate, false);
			AssertEquals("If dueDate is set JR_PaymentDate should have the same value", dueDateValue, invoicingJob2.Charges[0].JR_PaymentDate);
		}

		public Job SetUpChargesAndProcessJobForDueDateTesting(ZDateTime dueDateValue, ZDateTime invoiceDateValue, bool isAutoBillingDueDateFromPaymentTerms)
		{
			var staff = SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;
			testCreditor.OH_IsCreditor = true;
			testCreditor.CompanyData.OB_APPaymentTermDays = 10;
			testCreditor.CompanyData.OB_APPaymentTerms = "INV";

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false);
				((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).DefaultDebtor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				Factory.Save();

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = invoiceDateValue,
					APDueDate = dueDateValue,
					CustomsJob = customDetail,
					Factory = Factory,
					AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false),
					IsAutoBillingDueDateFromPaymentTerms = isAutoBillingDueDateFromPaymentTerms
				};

				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.ARPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();
				return Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
			}
		}

		public void TestPostingARInvoiceThenAPForTwoDataProvidersSharingSameCustomsJob()
		{
			SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).DefaultDebtor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				Factory.Save();

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);
				var cusCharge2 = GetCustomCharge(chargeCode, 22, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					APDueDate = new ZDateTime(2005, 4, 15),
					CustomsJob = customDetail,
					Factory = Factory
				};

				var chargeProvider2 = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge2 },
					InvoiceNumber = "INV124",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					APDueDate = new ZDateTime(2005, 4, 15),
					CustomsJob = customDetail,
					Factory = Factory
				};

				customDetail.DataProviders.Add(chargeProvider);
				customDetail.DataProviders.Add(chargeProvider2);

				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				var invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);

				AssertEquals("Two charge lines should have been created", 2, invoicingJob.Charges.Count);

				AssertEquals("It has not been posted for cost", false, invoicingJob.Charges[0].IsCostPosted);
				AssertEquals("It has not been posted for cost", false, invoicingJob.Charges[1].IsCostPosted);

				AssertEquals("INV123", invoicingJob.Charges[0].JR_APInvoiceNum);
				AssertEquals("InvoiceDate should not be set when AP is not going to be posted", ZDateTime.Empty, invoicingJob.Charges[0].JR_APInvoiceDate);
				AssertEquals("PaymentDate should not be set when AP is not going to be posted", ZDateTime.Empty, invoicingJob.Charges[0].JR_PaymentDate);

				AssertEquals("INV124", invoicingJob.Charges[1].JR_APInvoiceNum);
				AssertEquals("InvoiceDate should not be set when AP is not going to be posted", ZDateTime.Empty, invoicingJob.Charges[1].JR_APInvoiceDate);
				AssertEquals("PaymentDate should not be set when AP is not going to be posted", ZDateTime.Empty, invoicingJob.Charges[1].JR_PaymentDate);

				AssertEquals("It has been posted for revenue", true, invoicingJob.Charges[0].IsRevenuePosted);
				AssertEquals("It has been posted for revenue", true, invoicingJob.Charges[1].IsRevenuePosted);

				AssertEquals("posted into the same AR Invoice", invoicingJob.Charges[0].ARLine.TransactionHeader.PK, invoicingJob.Charges[1].ARLine.TransactionHeader.PK);

				AssertEquals(100m, invoicingJob.Charges[0].ARLine.AL_LineAmount);
				AssertEquals(22m, invoicingJob.Charges[1].ARLine.AL_LineAmount);

				//Post AP as well
				poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);

				AssertEquals("Two charge lines should have been created", 2, invoicingJob.Charges.Count);

				AssertEquals("It has been posted for cost", true, invoicingJob.Charges[0].IsCostPosted);
				AssertEquals("It has been posted for cost", true, invoicingJob.Charges[1].IsCostPosted);

				AssertEquals("It has been posted for revenue", true, invoicingJob.Charges[0].IsRevenuePosted);
				AssertEquals("It has been posted for revenue", true, invoicingJob.Charges[1].IsRevenuePosted);

				AssertNotEquals("posted into the different AP Invoices", invoicingJob.Charges[0].APLine.TransactionHeader.PK, invoicingJob.Charges[1].APLine.TransactionHeader.PK);
			}
		}

		public void TestPostingInWrongCompany()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~AA";
			company.GC_RN_NKCountryCode = "AU";

			var branch = company.Branches.AddNew();
			Factory.Save();

			SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Company = company;//should not be processed in this current company

				Factory.Save();

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);
				var cusCharge2 = GetCustomCharge(chargeCode, 22, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					APDueDate = new ZDateTime(2005, 4, 15),
					CustomsJob = customDetail,
					Factory = Factory
				};

				var chargeProvider2 = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge2 },
					InvoiceNumber = "INV124",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					APDueDate = new ZDateTime(2005, 4, 15),
					CustomsJob = customDetail,
					Factory = Factory
				};

				customDetail.DataProviders.Add(chargeProvider);
				customDetail.DataProviders.Add(chargeProvider2);

				Env.OutgoingMailManager.EmailsCreated.Clear();

				try
				{
					var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB, new[] { chargeCode.PK });
					poster.RaiseInvoices(customDetail);
					Fail("An exception is expected");
				}
				catch (CustomsInvoiceRaiseException e)
				{
					AssertContains("Auto-Billing is attempted in the company ", e.Message);
				}
			}
		}

		[TestDate(2018, 07, 14)]
		public void TestPostNonDSBCharges()
		{
			SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory
				};
				customDetail.DataProviders.Add(chargeProvider);

				Env.OutgoingMailManager.EmailsCreated.Clear();

				LocalClient.MiscServ.OM_ARWHTApplicable = false;
				Agent.MiscServ.OM_ARWHTApplicable = false;
				Job job = CreateJob("Z00001000", LocalClient, 5m, Agent, 10M);
				job.JH_ParentID = customDetail.PK;

				RefCurrency localCurrency = GlbCompany.CurrentCompany.LocalCurrency;

				Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", localCurrency, 150m, Creditor1, localCurrency, 160m, Agent);
				charge1.JR_APInvoiceDate = new ZDateTime(2009, 1, 1);
				charge1.JR_APInvoiceNum = "987432";

				Charge charge2 = CreateCharge(job, CC5, "Charge Code 5", localCurrency, 200m, Creditor2, localCurrency, 250m, Agent);
				charge2.JR_APInvoiceDate = new ZDateTime(2009, 1, 2);
				charge2.JR_APInvoiceNum = "9874387";

				Factory.Save();

				((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).DefaultDebtor = job.AgentCollect;

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB | ChargePosterBehaviours.ARAPPostNonDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				AssertEquals("Three Charges", 3, job.Charges.Count);

				Assert(charge1.IsCostPosted);
				Assert(charge1.IsRevenuePosted);
				AssertEquals(160m, charge1.ARLine.AL_LineAmount);
				AssertEquals(-150m, charge1.APLine.AL_LineAmount);

				Assert(charge2.IsCostPosted);
				Assert(charge2.IsRevenuePosted);
				AssertEquals(250m, charge2.ARLine.AL_LineAmount);
				AssertEquals(-200m, charge2.APLine.AL_LineAmount);

				AssertEquals("Should have been posted into the same AR invoice", charge1.ARLine.AL_AH, charge2.ARLine.AL_AH);

				Charge customsDSBCharge = (Charge)job.Charges.Find(new ZQuery(JobChargeSchema.JR_AC, chargeCode.PK))[0];
				AssertEquals("AP should not have been posted", false, customsDSBCharge.IsCostPosted);
				AssertEquals("However, AR should have been posted", true, customsDSBCharge.IsRevenuePosted);
				AssertEquals(100m, customsDSBCharge.ARLine.AL_LineAmount);
				AssertEquals("AR same invoice as non-DSB charges", charge1.ARLine.AL_AH, customsDSBCharge.ARLine.AL_AH);
			}
		}

		[TestDate(2018, 07, 14)]
		public void TestPostNonDSBCharges_BranchNotDefaultedFromRegistry()
		{
			JobBranchDefaultOrderRule rule = new JobBranchDefaultOrderRule();
			rule.DefaultToBlank = 1;
			rule.DefaultToBranchOfOrganisation = 0;
			rule.DefaultToBranchRelatedToPortOrWarehouseBranch = 0;
			rule.DefaultToLoginUserDefault = 0;
			AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rule);

			Factory.Save();

			SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory
				};
				customDetail.DataProviders.Add(chargeProvider);

				Env.OutgoingMailManager.EmailsCreated.Clear();

				LocalClient.MiscServ.OM_ARWHTApplicable = false;
				Agent.MiscServ.OM_ARWHTApplicable = false;

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB | ChargePosterBehaviours.ARAPPostNonDSB, new[] { chargeCode.PK });
				AssertNoExceptionThrown(() => poster.RaiseInvoices(customDetail));

				var jobBranch = customDetail.DataProviders.First().CustomsJob.Branch;
				AssertNotNull(jobBranch);
			}
		}

		[TestDate(2018, 07, 14)]
		public void TestPostNonCustomsChargeEvenWhenThereIsNoCustomsChargeAutoRated()
		{
			SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var job = CreateJob("Z00001000", LocalClient, 5m, Agent, 10M);
				job.JH_ParentID = customDetail.PK;
				job.JH_ParentTableCode = "XX";  // add this line to avoid "ViewGenericJob should not be queried by empty parent table code" error, XX does not exist is insignificant here.

				var localCurrency = GlbCompany.CurrentCompany.LocalCurrency;

				var charge1 = CreateCharge(job, CC1, "Charge Code 1", localCurrency, 150m, Creditor1, localCurrency, 160m, LocalClient);
				charge1.JR_APInvoiceDate = new ZDateTime(2009, 1, 1);
				charge1.JR_APInvoiceNum = "987432";

				var charge2 = CreateCharge(job, CC5, "Charge Code 5", localCurrency, 200m, Creditor2, localCurrency, 250m, LocalClient);
				charge2.JR_APInvoiceDate = new ZDateTime(2009, 1, 2);
				charge2.JR_APInvoiceNum = "9874387";

				Factory.Save();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB | ChargePosterBehaviours.ARAPPostNonDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				AssertEquals("two Charges", 2, job.Charges.Count);

				Assert(charge1.IsCostPosted);
				Assert(charge1.IsRevenuePosted);
				AssertEquals(160m, charge1.ARLine.AL_LineAmount);
				AssertEquals(-150m, charge1.APLine.AL_LineAmount);

				Assert(charge2.IsCostPosted);
				Assert(charge2.IsRevenuePosted);
				AssertEquals(250m, charge2.ARLine.AL_LineAmount);
				AssertEquals(-200m, charge2.APLine.AL_LineAmount);
			}
		}

		[TestDate(2018, 07, 14)]
		public void TestDoNotFailOnJobLevelErrorsOnlyIfAutoRating()
		{
			SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = null;
				customDetail.Consignor = null;

				// Prepare data to avoid Validation errors on saving Job before posting
				Job expectedJob = Factory.NewJobWithValidTestDataForTesting<Job>();
				expectedJob.JH_ParentID = customDetail.PK;
				expectedJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				expectedJob.JH_JobNum = "TR436877";
				expectedJob.PlugInData = customDetail;

				expectedJob.JH_OA_LocalChargesAddr = ZGuid.Empty;
				expectedJob.JH_OA_AgentCollectAddr = ZGuid.Empty;
				expectedJob.RunPreSaveValidation();

				var localCurrency = GlbCompany.CurrentCompany.LocalCurrency;
				var charge1 = CreateCharge(expectedJob, CC1, "Charge Code 1", localCurrency, 150m, Creditor1, localCurrency, 160m, LocalClient);
				charge1.JR_APInvoiceDate = new ZDateTime(2009, 1, 1);
				charge1.JR_APInvoiceNum = "987432";

				expectedJob.RunPreSaveValidation();
				Assert(expectedJob.HasErrors);

				Factory.Save();
				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory
				};
				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				foreach (Charge charge in expectedJob.Charges)
				{
					charge.JR_CostRatingOverride = false;
					charge.JR_SellRatingOverride = false;
				}

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB, new[] { chargeCode.PK });

				poster.RaiseInvoices(customDetail);
				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
				Assert("Should not be overridden", !expectedJob.Charges.Cast<Charge>().Any(x => x.JR_CostRatingOverride));
				Assert("Should not be overridden", !expectedJob.Charges.Cast<Charge>().Any(x => x.JR_SellRatingOverride));

				poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });

				try
				{
					poster.RaiseInvoices(customDetail);
					Fail("When posting, the job level errors should have been checked");
				}
				catch (CustomsInvoiceRaiseException e)
				{
					AssertContains("You cannot post because job TR436877 has errors. Please fix errors before posting.", e.Message);
				}
				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		public void TestDoNotUpdateAPInvoiceNumIfCostAmountIsZero_CS00253626()
		{
			SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				// Disable WHT to avoid some Validation errors
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());

				var cusCharge = GetCustomCharge(chargeCode, 160m, 0, true, testCreditor.PK);
				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory
				};
				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var job = CreateJob("Z00001000", LocalClient, 5m, Agent, 10M);
				job.JH_ParentID = customDetail.PK;
				job.JH_ParentTableCode = "XX";  // add this line to avoid "ViewGenericJob should not be queried by empty parent table code" error, XX does not exist is insignificant here.

				var localCurrency = GlbCompany.CurrentCompany.LocalCurrency;

				var charge1 = CreateCharge(job, chargeCode, "ChargeCode", localCurrency, 160m, testCreditor, localCurrency, 160m, LocalClient);
				charge1.JR_APInvoiceNum = "";
				charge1.PostARWhenInvokedByCustomInvoiceCreator = true;

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.ARPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				charge1.Reload();
				Assert(charge1.IsRevenuePosted);
				charge1.PostARWhenInvokedByCustomInvoiceCreator = false;

				var charge2 = CreateCharge(job, chargeCode, "ChargeCode", localCurrency, -10m, testCreditor, localCurrency, -10m, LocalClient);
				charge2.JR_APInvoiceNum = "123";
				charge2.JR_APInvoiceDate = ZDateTime.Today;
				charge2.PostARWhenInvokedByCustomInvoiceCreator = true;

				Factory.Save();

				poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.ARPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				charge2.Reload();
				Assert(!charge2.IsRevenuePosted);
				charge2.PostARWhenInvokedByCustomInvoiceCreator = false;

				charge2.JR_APInvoiceNum = "";
				charge2.JR_LocalCostAmt = ZDecimal.Zero;

				charge1.JR_CostRatingOverride = false;
				charge2.JR_SellRatingOverride = false;

				poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Assert(charge2.JR_LocalCostAmt.IsEmpty);
				Assert("AP Invoice Number should not have been populated on zero-amounted charges", charge2.JR_APInvoiceNum.IsEmpty);
				Assert("AP Invoice Number should not have been populated on zero-amounted charges", !charge1.JR_APInvoiceNum.IsEmpty);
				Assert("Should not be overridden", !charge1.JR_CostRatingOverride);
				Assert("Should not be overridden", !charge2.JR_SellRatingOverride);
			}
		}

		public void TestPostInvoiceForNonDSBNegativeAmount_CS00688483_CS00708297()
		{
			SetupStaffMemberEmailAddress();

			var chargeCodeDSB = SetChargeCode();
			var chargeCodeNonDSB = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCodeNonDSB.AC_ChargeType = Constants.ChargeType.Margin;

			Factory.Save();

			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCodeDSB, Constants.CountryCodes.UnitedStates))
			{
				// Disable WHT to avoid some Validation errors
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());

				var cusChargeDSB = GetCustomCharge(chargeCodeDSB, -10m, 0, true, testCreditor.PK);
				var chargeProviderDSB = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusChargeDSB },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory
				};

				var cusChargeNonDSB = GetCustomCharge(chargeCodeNonDSB, 10m, 0, true, testCreditor.PK);
				var chargeProviderNonDSB = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusChargeNonDSB },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory
				};

				customDetail.DataProviders.Add(chargeProviderDSB);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var job = CreateJob("Z00001000", LocalClient, 5m, Agent, 10M);
				job.JH_ParentID = customDetail.PK;
				job.JH_ParentTableCode = "XX"; // add this line to avoid "ViewGenericJob should not be queried by empty parent table code" error, XX does not exist is insignificant here.

				var localCurrency = GlbCompany.CurrentCompany.LocalCurrency;

				var charge1 = CreateCharge(job, chargeCodeDSB, "ChargeCode", localCurrency, 160m, testCreditor, localCurrency, 160m, LocalClient);
				charge1.JR_APInvoiceNum = "";
				charge1.PostARWhenInvokedByCustomInvoiceCreator = true;
				Factory.Save();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.ARPostDSB, new[] { chargeCodeDSB.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				charge1.Reload();
				Assert("Charge 1 should be posted", charge1.IsRevenuePosted);
				charge1.PostARWhenInvokedByCustomInvoiceCreator = false;

				customDetail.DataProviders.Clear();
				customDetail.DataProviders.Add(chargeProviderNonDSB);

				ARInvoice invoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), GlbCompany.CurrentCompany.LocalCurrency, 1m);
				invoice.AH_OH = TestObjectCreator.ABIGAS.PK;

				ARInvoiceLine line = (ARInvoiceLine)TestObjectCreator.CreateInvoiceLine(invoice, 20m, invoice.TransactionCurrency, 1m);
				line.AL_AC = chargeCodeNonDSB.PK;
				line.AL_JH = job.PK;

				var charge2 = CreateCharge(job, chargeCodeNonDSB, "ChargeCode", localCurrency, 20m, testCreditor, localCurrency, 20m, LocalClient);
				charge2.JR_APInvoiceNum = "654";
				charge2.JR_APInvoiceDate = new ZDateTime(2005, 4, 12);
				charge2.PostARWhenInvokedByCustomInvoiceCreator = true;
				charge2.JR_AL_ARLine = line.PK;
				charge2.JR_AT_SellGSTRate = line.AL_AT;
				Factory.Save();

				charge2.JR_SellRatingOverride = false;

				poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.ARPostDSB, new[] { chargeCodeNonDSB.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				charge2.Reload();
				Assert("Charge 2 should be posted", charge2.IsRevenuePosted);
				charge2.PostARWhenInvokedByCustomInvoiceCreator = false;

				ARInvoice invoice2 = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), GlbCompany.CurrentCompany.LocalCurrency, 1m);
				invoice2.AH_OH = TestObjectCreator.ABIGAS.PK;

				ARInvoiceLine line2 = (ARInvoiceLine)TestObjectCreator.CreateInvoiceLine(invoice2, -10m, invoice2.TransactionCurrency, 1m);
				line2.AL_AC = chargeCodeNonDSB.PK;
				line2.AL_JH = job.PK;

				var charge3 = CreateCharge(job, chargeCodeNonDSB, "ChargeCode", localCurrency, -10m, testCreditor, localCurrency, -10m, LocalClient);
				charge3.JR_APInvoiceNum = "123";
				charge3.JR_APInvoiceDate = new ZDateTime(2005, 4, 12);
				charge3.PostARWhenInvokedByCustomInvoiceCreator = true;
				charge3.JR_AL_ARLine = line2.PK;
				charge3.JR_AT_SellGSTRate = line2.AL_AT;
				Factory.Save();

				charge2.JR_CostRatingOverride = false;
				charge3.JR_SellRatingOverride = false;

				poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.ARPostDSB, new[] { chargeCodeNonDSB.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				charge3.Reload();
				Assert("Charge 3 should be posted", charge3.IsRevenuePosted);
			}
		}

		[TestDate(2018, 07, 14)]
		public void TestPostAROnlyForNonCustomsDSB()
		{
			SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				// Disable WHT to avoid some Validation errors
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);
				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory
				};
				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var job = CreateJob("Z00001000", LocalClient, 5m, Agent, 10M);
				job.JH_ParentID = customDetail.PK;

				var localCurrency = GlbCompany.CurrentCompany.LocalCurrency;

				var charge1 = CreateCharge(job, CC1, "Charge Code 1", localCurrency, 150m, Creditor1, localCurrency, 160m, LocalClient);
				charge1.JR_APInvoiceDate = new ZDateTime(2009, 1, 1);
				charge1.JR_APInvoiceNum = "987432";

				var charge2 = CreateCharge(job, CC5, "Charge Code 5", localCurrency, 200m, Creditor2, localCurrency, 250m, LocalClient);
				charge2.JR_APInvoiceDate = new ZDateTime(2009, 1, 2);
				charge2.JR_APInvoiceNum = "9874387";

				Factory.Save();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARAPPostNonDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				AssertEquals("Three Charges", 3, job.Charges.Count);

				Assert(charge1.IsCostPosted && charge1.IsRevenuePosted);
				Assert(charge2.IsCostPosted && charge2.IsRevenuePosted);
				AssertEquals("Should have been posted into the same AR invoice", charge1.ARLine.AL_AH, charge2.ARLine.AL_AH);

				var customsDSBCharge = (Charge)job.Charges.Find(new ZQuery(JobChargeSchema.JR_AC, chargeCode.PK))[0];
				Assert("Should NOT have posted DSB charge", !customsDSBCharge.IsCostPosted);
				Assert("Should NOT have posted DSB charge", !customsDSBCharge.IsRevenuePosted);
				AssertEquals("However should have autorated", 100m, customsDSBCharge.JR_LocalCostAmt);
				AssertEquals("However should have autorated", 100m, customsDSBCharge.JR_LocalSellAmt);
				Assert("Should not be overridden", !customsDSBCharge.JR_CostRatingOverride);
				Assert("Should not be overridden", !customsDSBCharge.JR_SellRatingOverride);
			}
		}

		[TestDate(2018, 07, 14)]
		public void TestPostARForNonCustomsDSBAndCustomsDSB()
		{
			SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				// Disable WHT to avoid some Validation errors
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);
				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory
				};
				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				LocalClient.MiscServ.OM_ARWHTApplicable = false;
				var job = CreateJob("Z00001000", LocalClient, 5m, Agent, 10M);
				job.JH_ParentID = customDetail.PK;

				var localCurrency = GlbCompany.CurrentCompany.LocalCurrency;

				var charge1 = CreateCharge(job, CC1, "Charge Code 1", localCurrency, 150m, Creditor1, localCurrency, 160m, LocalClient);
				charge1.JR_APInvoiceDate = new ZDateTime(2009, 1, 1);
				charge1.JR_APInvoiceNum = "987432";

				var charge2 = CreateCharge(job, CC5, "Charge Code 5", localCurrency, 200m, Creditor2, localCurrency, 250m, LocalClient);
				charge2.JR_APInvoiceDate = new ZDateTime(2009, 1, 2);
				charge2.JR_APInvoiceNum = "9874387";

				var customsCharge = CreateCharge(job, chargeCode, "Charge Code 2", localCurrency, 120m, Creditor2, localCurrency, 120m, LocalClient);
				customsCharge.JR_APInvoiceNum = "WRONG";

				Factory.Save();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARAPPostNonDSB | ChargePosterBehaviours.ARPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				AssertEquals("Four Charges:Another charge line with a correct AP invoice number is autorated", 4, job.Charges.Count);

				Assert(charge1.IsCostPosted && charge1.IsRevenuePosted);
				Assert(charge2.IsCostPosted && charge2.IsRevenuePosted);
				AssertEquals("Should have been posted into the same AR invoice", charge1.ARLine.AL_AH, charge2.ARLine.AL_AH);

				AssertEquals("user-entered charge should not be posted", false, customsCharge.IsCostPosted);
				AssertEquals("user-entered charge should not be posted", false, customsCharge.IsRevenuePosted);

				var query = new ZQuery(JobChargeSchema.JR_AC, chargeCode.PK);
				query.AddToFilter(JobChargeSchema.PK, SQLComparisonOperator.NotEqual, customsCharge.PK);

				var autoRatedDSBCharge = (Charge)job.Charges.Find(query)[0];
				Assert("Should have NOT posted DSB charge for AP", !autoRatedDSBCharge.IsCostPosted);
				Assert("Should have posted DSB charge", autoRatedDSBCharge.IsRevenuePosted);
				AssertEquals("However should have autorated", 100m, autoRatedDSBCharge.JR_LocalCostAmt);
				AssertEquals("However should have autorated", 100m, autoRatedDSBCharge.JR_LocalSellAmt);
			}
		}

		[TestDate(2018, 07, 14)]
		public void TestPostAPForDSBAndNonDSB()
		{
			SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				// Disable WHT to avoid some Validation errors
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);
				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory
				};
				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var job = CreateJob("Z00001000", LocalClient, 5m, Agent, 10M);
				job.JH_ParentID = customDetail.PK;

				var localCurrency = GlbCompany.CurrentCompany.LocalCurrency;

				var charge1 = CreateCharge(job, CC1, "Charge Code 1", localCurrency, 150m, Creditor1, localCurrency, 160m, LocalClient);
				charge1.JR_APInvoiceDate = new ZDateTime(2009, 1, 1);
				charge1.JR_APInvoiceNum = "987432";

				var charge2 = CreateCharge(job, CC5, "Charge Code 5", localCurrency, 200m, Creditor2, localCurrency, 250m, LocalClient);
				charge2.JR_APInvoiceDate = new ZDateTime(2009, 1, 2);
				charge2.JR_APInvoiceNum = "9874387";

				Factory.Save();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARAPPostNonDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				AssertEquals("Three Charges", 3, job.Charges.Count);

				Assert(charge1.IsCostPosted && charge1.IsRevenuePosted);
				Assert(charge2.IsCostPosted && charge2.IsRevenuePosted);
				AssertEquals("Should have been posted into the same AR invoice", charge1.ARLine.AL_AH, charge2.ARLine.AL_AH);

				var customsDSBCharge = (Charge)job.Charges.Find(new ZQuery(JobChargeSchema.JR_AC, chargeCode.PK))[0];
				Assert("Should have AP posted DSB charge", customsDSBCharge.IsCostPosted);
				Assert("Should NOT have posted DSB charge", !customsDSBCharge.IsRevenuePosted);
				AssertEquals(-100m, customsDSBCharge.APLine.AL_LineAmount);
			}
		}

		[TestDate(2018, 07, 14)]
		public void TestWhenAutoRatedWithInformationOnlyChargeCodesDifferentToExistingOnes()
		{
			var staff = SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				RatingDataRegistry.Instance.CustomDeferredChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, CC2.PK.ToGuid());
				RatingDataRegistry.Instance.IncludeCustomDeferredChargeInInvoicing.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				// Disable WHT to avoid some Validation errors
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false);

				var cusCharge = GetCustomCharge(CC2, 100, 0, false, testCreditor.PK);//not paid by broker
				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					CustomsJob = customDetail,
					Factory = Factory,
					AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false)
				};
				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var job = CreateJob("Z00001000", LocalClient, 5m, Agent, 10M);
				job.JH_ParentID = customDetail.PK;

				var localCurrency = GlbCompany.CurrentCompany.LocalCurrency;

				var charge1 = CreateCharge(job, chargeCode, "Charge Code 1", localCurrency, 150m, Creditor1, localCurrency, 160m, LocalClient);

				var charge2 = CreateCharge(job, CC5, "Charge Code 5", localCurrency, 200m, Creditor2, localCurrency, 250m, LocalClient);
				charge2.JR_APInvoiceDate = new ZDateTime(2009, 1, 2);
				charge2.JR_APInvoiceNum = "9874387";

				Factory.Save();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARAPPostNonDSB | ChargePosterBehaviours.ARPostDSB | ChargePosterBehaviours.SendEmail, new[] { chargeCode.PK, CC2.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				AssertEquals("One email should have been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertContains("are not relevant any more. Please remove them. The job potentially needs another review and auto-rate for other charges manually.", Env.OutgoingMailManager.EmailsCreated[0].Body);

				AssertEquals("Autorated for Customs DSB", 3, job.Charges.Count);
				var deferred = job.Charges.Find(new ZQuery(JobChargeSchema.JR_AC, CC2.PK)).OfType<Charge>().Single();
				AssertEquals(@"Charge Code 2
  TEST                                      100.00", deferred.JR_Desc);

				Assert("Not posted", !charge1.IsRevenuePosted && !charge1.IsCostPosted);
				Assert("Not posted", !charge2.IsRevenuePosted && !charge2.IsCostPosted);

				cusCharge = GetCustomCharge(CC2, 120, 0, false, testCreditor.PK);
				chargeProvider.CustomsCharges = new ICustomsCharges[] { cusCharge };
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				deferred = job.Charges.Find(new ZQuery(JobChargeSchema.JR_AC, CC2.PK)).OfType<Charge>().Single();
				AssertEquals(@"Charge Code 2
Current Amounts
  TEST                                      120.00", deferred.JR_Desc);
			}
		}

		[TestDate(2018, 07, 14)]
		public void TestSendEmailIfAutoRatedCustomsDSBChargeIsDifferentFromCurrentCustomsCharges()
		{
			var staff = SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				// Disable WHT to avoid some Validation errors
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false);

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);
				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory,
					AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false)
				};
				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var job = CreateJob("Z00001000", LocalClient, 5m, Agent, 10M);
				job.JH_ParentID = customDetail.PK;

				var localCurrency = GlbCompany.CurrentCompany.LocalCurrency;

				var charge1 = CreateCharge(job, CC1, "Charge Code 1", localCurrency, 150m, Creditor1, localCurrency, 160m, LocalClient);
				charge1.JR_APInvoiceDate = new ZDateTime(2009, 1, 1);
				charge1.JR_APInvoiceNum = "987432";

				var charge2 = CreateCharge(job, CC5, "Charge Code 5", localCurrency, 200m, Creditor2, localCurrency, 250m, LocalClient);
				charge2.JR_APInvoiceDate = new ZDateTime(2009, 1, 2);
				charge2.JR_APInvoiceNum = "9874387";

				var customsCharge = CreateCharge(job, chargeCode, "Charge Code 2", localCurrency, 120m, Creditor2, localCurrency, 120m, LocalClient);//Amount is different

				Factory.Save();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARAPPostNonDSB | ChargePosterBehaviours.SendEmail, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

				Assert("Nothing should have been posted", !charge1.IsCostPosted);
				Assert("Nothing should have been posted", !charge1.IsRevenuePosted);

				Assert("Nothing should have been posted", !charge2.IsCostPosted);
				Assert("Nothing should have been posted", !charge2.IsRevenuePosted);

				Assert("Nothing should have been posted", !customsCharge.IsCostPosted);
				Assert("Nothing should have been posted", !customsCharge.IsRevenuePosted);

				AssertEquals("However, customs Charge is updated", 100m, customsCharge.JR_LocalCostAmt);
				AssertEquals("However, customs Charge is updated", 100m, customsCharge.JR_LocalSellAmt);
			}
		}

		public void TestDoNotPostAgainForUnpostedUserEnteredChargeLine()
		{
			SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).DefaultDebtor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				Factory.Save();

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory
				};
				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.ARPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				var invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);
				AssertEquals("One charge should have been created", 1, invoicingJob.Charges.Count);
				AssertEquals("It has been posted for cost", true, invoicingJob.Charges[0].IsCostPosted);
				AssertEquals("It has been posted for revenue", true, invoicingJob.Charges[0].IsRevenuePosted);
				AssertEquals("INV123", invoicingJob.Charges[0].JR_APInvoiceNum);

				var chargeLine2 = invoicingJob.Charges.AddNew();
				chargeLine2.JR_AC = chargeCode.PK;
				chargeLine2.JR_LocalCostAmt = 20m;

				//Attempt to post again
				poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.ARPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);

				AssertEquals("Two charge lines", 2, invoicingJob.Charges.Count);
				AssertEquals("First line was posted before", true, invoicingJob.Charges[0].IsCostPosted);
				AssertEquals("Second line should not be posted for cost", false, invoicingJob.Charges[1].IsCostPosted);

				AssertEquals("First line was posted before", true, invoicingJob.Charges[0].IsRevenuePosted);
				AssertEquals("Second line should not be posted for revenue", false, invoicingJob.Charges[1].IsRevenuePosted);
			}
		}

		public void TestDoNotPostAgainForUnpostedUserEnteredChargeLine2()
		{
			SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).DefaultDebtor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				Factory.Save();

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);
				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory
				};
				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.ARPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				var invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);
				AssertEquals("One charge should have been created", 1, invoicingJob.Charges.Count);
				AssertEquals("It has been posted for cost", true, invoicingJob.Charges[0].IsCostPosted);
				AssertEquals("It has been posted for revenue", true, invoicingJob.Charges[0].IsRevenuePosted);
				AssertEquals("INV123", invoicingJob.Charges[0].JR_APInvoiceNum);

				//DSB charge line that has a valid data for AR posting & AP Posting
				var chargeLine2 = CreateCharge(invoicingJob, chargeCode, "TEST", GlbCompany.CurrentCompany.LocalCurrency, 100m, testCreditor, GlbCompany.CurrentCompany.LocalCurrency, 100m, customDetail.Consignee);
				chargeLine2.JR_APInvoiceNum = "INV123/1";
				chargeLine2.JR_APInvoiceDate = ZDateTime.Today;
				chargeLine2.JR_PaymentDate = ZDateTime.Today;
				chargeLine2.JR_LocalCostAmt = 20m;

				//Attempt to post again
				poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.ARPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);

				AssertEquals("Two charge lines", 2, invoicingJob.Charges.Count);
				AssertEquals("First line was posted before", true, invoicingJob.Charges[0].IsCostPosted);
				AssertEquals("Second line should not be posted for cost", false, invoicingJob.Charges[1].IsCostPosted);

				AssertEquals("First line was posted before", true, invoicingJob.Charges[0].IsRevenuePosted);
				AssertEquals("Second line should not be posted for revenue", false, invoicingJob.Charges[1].IsRevenuePosted);
			}
		}

		public void TestPostMultipleChargeLinesWithMatchingTotalAmount()
		{
			SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).DefaultDebtor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				Factory.Save();

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);
				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory
				};
				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				var invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);
				AssertEquals("One charge should have been created", 1, invoicingJob.Charges.Count);
				AssertEquals("INV123", invoicingJob.Charges[0].JR_APInvoiceNum);
				AssertEquals("Amount autorated", 100m, invoicingJob.Charges[0].JR_LocalSellAmt);

				//DSB charge line that has a valid data for AR posting & AP Posting
				var chargeLine2 = CreateCharge(invoicingJob, chargeCode, "TEST", GlbCompany.CurrentCompany.LocalCurrency, 100m, testCreditor, GlbCompany.CurrentCompany.LocalCurrency, 100m, customDetail.Consignee);
				chargeLine2.JR_APInvoiceNum = "INV123/1";
				chargeLine2.JR_APInvoiceDate = ZDateTime.Today;
				chargeLine2.JR_PaymentDate = ZDateTime.Today;
				chargeLine2.JR_LocalCostAmt = 20m;

				//amended
				cusCharge = GetCustomCharge(chargeCode, 120, 0, true, testCreditor.PK);
				chargeProvider.CustomsCharges = new ICustomsCharges[] { cusCharge };

				//Attempt to post again
				poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.ARPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);
				AssertEquals("Two charge lines", 2, invoicingJob.Charges.Count);
				AssertEquals("Amounts are retained", 100m, invoicingJob.Charges[0].JR_LocalSellAmt);
				AssertEquals("Amounts are retained", 20m, invoicingJob.Charges[1].JR_LocalSellAmt);

				Assert("Both posted", invoicingJob.Charges[0].IsCostPosted);
				Assert("Both Posted", invoicingJob.Charges[1].IsCostPosted);

				Assert("Both posted", invoicingJob.Charges[0].IsRevenuePosted);
				Assert("Both Posted", invoicingJob.Charges[1].IsRevenuePosted);
			}
		}

		public void TestDoNotAddANewChargeLineWhenAPIsPosted()
		{
			SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).DefaultDebtor = Factory.LoadTop1<OrgHeader>(new ZQuery());

				// Prepare data to avoid Validation errors on saving Job before posting
				Job expectedJob = Factory.NewJobWithValidTestDataForTesting<Job>();
				expectedJob.JH_ParentID = customDetail.PK;
				expectedJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				expectedJob.JH_OA_LocalChargesAddr = TestObjectCreator.Agent.MainAddress.PK;
				expectedJob.JH_JobNum = "TR436877";
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

				expectedJob.PlugInData = customDetail;

				Factory.Save();
				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory
				};
				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.ARPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				Job invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);

				AssertEquals("Two charge lines should have been created", 1, invoicingJob.Charges.Count);

				AssertEquals("It has been posted for cost", true, invoicingJob.Charges[0].IsCostPosted);
				AssertEquals("It has been posted for revenue", true, invoicingJob.Charges[0].IsRevenuePosted);
				AssertEquals("INV123", invoicingJob.Charges[0].JR_APInvoiceNum);

				Factory.Save();

				poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);

				AssertEquals("Still one charge line", 1, invoicingJob.Charges.Count);
			}
		}

		[TestDate(2018, 07, 14)]
		public void TestWhenPostingNonDSBAndDSBWithFullAPDetailsExist()
		{
			var staff = SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				// Disable WHT to avoid some Validation errors
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false);

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory,
					AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false)
				};
				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var job = CreateJob("Z00001000", LocalClient, 5m, Agent, 10M);
				job.JH_ParentID = customDetail.PK;

				var localCurrency = GlbCompany.CurrentCompany.LocalCurrency;

				var charge1 = CreateCharge(job, CC1, "Charge Code 1", localCurrency, 150m, Creditor1, localCurrency, 160m, LocalClient);
				charge1.JR_APInvoiceDate = new ZDateTime(2009, 1, 1);
				charge1.JR_APInvoiceNum = "987432";

				var charge2 = CreateCharge(job, CC5, "Charge Code 5", localCurrency, 200m, Creditor2, localCurrency, 250m, LocalClient);
				charge2.JR_APInvoiceDate = new ZDateTime(2009, 1, 2);
				charge2.JR_APInvoiceNum = "9874387";

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				Factory.Save();

				AssertEquals("Three charges", 3, job.Charges.Count);

				Charge customsCharge = (Charge)job.Charges.Find(new ZQuery(JobChargeSchema.JR_AC, chargeCode.PK))[0];

				Assert("nothing posted", !charge1.IsCostPosted && !charge2.IsCostPosted && !customsCharge.IsCostPosted);
				Assert("nothing posted", !charge1.IsRevenuePosted && !charge2.IsRevenuePosted && !customsCharge.IsRevenuePosted);

				poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARAPPostNonDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				Assert("Only non-DSB should have been posted", charge1.IsCostPosted && charge1.IsRevenuePosted);
				Assert("Only non-DSB should have been posted", charge2.IsCostPosted && charge2.IsRevenuePosted);
				Assert("Only non-DSB should have been posted", !customsCharge.IsCostPosted && !customsCharge.IsRevenuePosted);
			}
		}

		public void TestPostAPInvoicesWhenReversedInvoiceExists()
		{
			var staff = SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false);
				Factory.Save();

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);
				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory,
					AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false)
				};
				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.SendEmail, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				var invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);
				AssertEquals("It has been posted for cost", true, invoicingJob.Charges[0].IsCostPosted);

				var apInvoiceRaised = Factory.LoadTop1<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "INV123"));
				new APInvoiceReversing(apInvoiceRaised).Reverse();
				apInvoiceRaised.ReverseInvoice.AH_TransactionNum = "TEST_TRANSACTIONNUM_REVERSE";
				Factory.Save();
				AssertEquals("It is not posted any more", false, invoicingJob.Charges[0].IsCostPosted);

				//try to post AP again
				invoicingJob.Charges[0].JR_APInvoiceNum = ZString.Empty;
				poster.RaiseInvoices(customDetail);
				AssertEquals("It has been posted for cost", true, invoicingJob.Charges[0].IsCostPosted);
				var apInvoiceRaised2 = invoicingJob.Charges[0].APLine.TransactionHeader;
				AssertNotNull(apInvoiceRaised2);

				AssertEquals("TransactionNumber is set uniquely", "INV123/1", apInvoiceRaised2.AH_TransactionNum);
			}
		}

		public void TestPostingARInvoiceOneAtATimeThenARWithDifferentAmount()
		{
			var staff = SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false);
				((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).DefaultDebtor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				Factory.Save();

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);
				var cusCharge2 = GetCustomCharge(chargeCode, 22, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory,
					AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false)
				};

				var chargeProvider2 = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge2 },
					InvoiceNumber = "INV124",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory,
					AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false)
				};

				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();
				var invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);
				AssertEquals("One charge line should have been created", 1, invoicingJob.Charges.Count);
				AssertEquals("INV123", invoicingJob.Charges[0].JR_APInvoiceNum);
				AssertEquals(-100m, invoicingJob.Charges[0].APLine.AL_LineAmount);
				AssertEquals("It has not been posted for cost", true, invoicingJob.Charges[0].IsCostPosted);
				AssertEquals("It has been posted for revenue", false, invoicingJob.Charges[0].IsRevenuePosted);
				AssertEquals("Customs Disbursement Charges\r\nCurrent Amounts\r\n  TEST                                      100.00", invoicingJob.Charges[0].JR_Desc);

				customDetail.DataProviders.Add(chargeProvider2);
				poster.RaiseInvoices(customDetail);
				AssertEquals("Two charge lines should have been created", 2, invoicingJob.Charges.Count);
				AssertEquals("INV124", invoicingJob.Charges[1].JR_APInvoiceNum);
				AssertEquals("It has not been posted for cost", true, invoicingJob.Charges[1].IsCostPosted);
				AssertEquals(-22m, invoicingJob.Charges[1].APLine.AL_LineAmount);
				AssertEquals("It has been posted for revenue", false, invoicingJob.Charges[1].IsRevenuePosted);

				AssertEquals("AP Already posted should remain same", "INV123", invoicingJob.Charges[0].JR_APInvoiceNum);
				AssertEquals("AP Already posted should remain same", -100m, invoicingJob.Charges[0].APLine.AL_LineAmount);
				AssertEquals("Customs Disbursement Charges\r\nCurrent Amounts\r\n  TEST                                       22.00", invoicingJob.Charges[1].JR_Desc);

				cusCharge = GetCustomCharge(chargeCode, 101, 0, true, testCreditor.PK);
				cusCharge2 = GetCustomCharge(chargeCode, 23, 0, true, testCreditor.PK);
				chargeProvider.CustomsCharges = new ICustomsCharges[] { cusCharge };
				chargeProvider2.CustomsCharges = new ICustomsCharges[] { cusCharge2 };

				Env.OutgoingMailManager.EmailsCreated.Clear();
				poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.SendEmail, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				CombineAssertions(() =>
				{
					AssertEquals("Two charge lines should have been created", 4, invoicingJob.Charges.Count);

					var charge1 = invoicingJob.Charges[0];
					var charge2 = invoicingJob.Charges[1];
					var charge3 = invoicingJob.Charges[2];
					var charge4 = invoicingJob.Charges[3];
					AssertEquals("INV123", charge1.JR_APInvoiceNum);
					AssertEquals(new ZDateTime(2005, 4, 12), charge1.JR_APInvoiceDate);
					AssertEquals(100m, charge1.JR_OSCostAmt);
					AssertEquals(100m, charge1.JR_LocalCostAmt);
					AssertEquals(100m, charge1.JR_EstimatedCost);
					AssertEquals(100m, charge1.JR_OSSellAmt);
					AssertEquals(100m, charge1.JR_LocalSellAmt);
					AssertEquals(100m, charge1.JR_EstimatedRevenue);
					Assert(charge1.IsCostPosted);
					Assert(!charge1.IsRevenuePosted);
					AssertEquals("AP Detail", -100m, charge1.APLine.AL_LineAmount);
					AssertEquals("AR Detail", -100m, charge1.ARLine.AL_LineAmount);
					AssertEquals("Customs Disbursement Charges\r\nCurrent Amounts\r\n  TEST                                      100.00", charge1.JR_Desc);

					AssertEquals("INV124", charge2.JR_APInvoiceNum);
					AssertEquals(new ZDateTime(2005, 4, 12), charge2.JR_APInvoiceDate);
					AssertEquals(22m, charge2.JR_OSCostAmt);
					AssertEquals(22m, charge2.JR_LocalCostAmt);
					AssertEquals(22m, charge2.JR_EstimatedCost);
					AssertEquals(22m, charge2.JR_OSSellAmt);
					AssertEquals(22m, charge2.JR_LocalSellAmt);
					AssertEquals(22m, charge2.JR_EstimatedRevenue);
					Assert(charge2.IsCostPosted);
					Assert(!charge2.IsRevenuePosted);
					AssertEquals("AP Detail", -22m, charge2.APLine.AL_LineAmount);
					AssertEquals("AR Detail", -22m, charge2.ARLine.AL_LineAmount);
					AssertEquals("Customs Disbursement Charges\r\nCurrent Amounts\r\n  TEST                                       22.00", charge2.JR_Desc);

					AssertEquals("INV123/CUSDSB/1", charge3.JR_APInvoiceNum);
					AssertEquals(new ZDateTime(2005, 4, 12), charge3.JR_APInvoiceDate);
					AssertEquals(1m, charge3.JR_OSCostAmt);
					AssertEquals(1m, charge3.JR_LocalCostAmt);
					AssertEquals(1m, charge3.JR_EstimatedCost);
					AssertEquals(1m, charge3.JR_OSSellAmt);
					AssertEquals(1m, charge3.JR_LocalSellAmt);
					AssertEquals(1m, charge3.JR_EstimatedRevenue);
					Assert(charge3.IsCostPosted);
					Assert(!charge3.IsRevenuePosted);
					AssertEquals("AP Detail", -1m, charge3.APLine.AL_LineAmount);
					AssertEquals("AR Detail", -1m, charge3.ARLine.AL_LineAmount);
					AssertEquals("Customs Disbursement Charges\r\nCurrent Amounts\r\n  TEST                                      101.00", charge3.JR_Desc);

					AssertEquals("INV124/CUSDSB/1", charge4.JR_APInvoiceNum);
					AssertEquals(new ZDateTime(2005, 4, 12), charge3.JR_APInvoiceDate);
					AssertEquals(1m, charge4.JR_OSCostAmt);
					AssertEquals(1m, charge4.JR_LocalCostAmt);
					AssertEquals(1m, charge4.JR_EstimatedCost);
					AssertEquals(1m, charge4.JR_OSSellAmt);
					AssertEquals(1m, charge4.JR_LocalSellAmt);
					AssertEquals(1m, charge4.JR_EstimatedRevenue);
					Assert(charge4.IsCostPosted);
					Assert(!charge4.IsRevenuePosted);
					AssertEquals("AP Detail", -1m, charge4.APLine.AL_LineAmount);
					AssertEquals("AR Detail", -1m, charge4.ARLine.AL_LineAmount);
					AssertEquals("Customs Disbursement Charges\r\nCurrent Amounts\r\n  TEST                                       23.00", charge4.JR_Desc);

					AssertEquals("one email should have been sent notifying discrepancy", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				});

				cusCharge = GetCustomCharge(chargeCode, 120, 0, true, testCreditor.PK);
				cusCharge2 = GetCustomCharge(chargeCode, 30, 0, true, testCreditor.PK);
				chargeProvider.CustomsCharges = new ICustomsCharges[] { cusCharge };
				chargeProvider2.CustomsCharges = new ICustomsCharges[] { cusCharge2 };

				Env.OutgoingMailManager.EmailsCreated.Clear();
				//post AR as well
				poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.SendEmail, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				CombineAssertions(() =>
				{
					AssertEquals("Six charge lines should have been created", 6, invoicingJob.Charges.Count);

					var charge1 = invoicingJob.Charges[0];
					var charge2 = invoicingJob.Charges[1];
					var charge3 = invoicingJob.Charges[2];
					var charge4 = invoicingJob.Charges[3];
					var charge5 = invoicingJob.Charges[4];
					var charge6 = invoicingJob.Charges[5];
					AssertEquals("INV123", charge1.JR_APInvoiceNum);
					AssertEquals(new ZDateTime(2005, 4, 12), charge1.JR_APInvoiceDate);
					AssertEquals("Customs Disbursement Charges\r\nCurrent Amounts\r\n  TEST                                      100.00", charge1.JR_Desc);
					AssertEquals(100m, charge1.JR_OSCostAmt);
					AssertEquals(100m, charge1.JR_LocalCostAmt);
					AssertEquals(100m, charge1.JR_EstimatedCost);
					AssertEquals(100m, charge1.JR_OSSellAmt);
					AssertEquals(100m, charge1.JR_LocalSellAmt);
					AssertEquals(100m, charge1.JR_EstimatedRevenue);
					Assert(charge1.IsCostPosted);
					Assert(!charge1.IsRevenuePosted);
					AssertEquals(-100m, charge1.APLine.AL_LineAmount);
					AssertEquals(-100m, charge1.ARLine.AL_LineAmount);

					AssertEquals("INV124", charge2.JR_APInvoiceNum);
					AssertEquals(new ZDateTime(2005, 4, 12), charge1.JR_APInvoiceDate);
					AssertEquals("Customs Disbursement Charges\r\nCurrent Amounts\r\n  TEST                                       22.00", charge2.JR_Desc);
					AssertEquals(22m, charge2.JR_OSCostAmt);
					AssertEquals(22m, charge2.JR_LocalCostAmt);
					AssertEquals(22m, charge2.JR_EstimatedCost);
					AssertEquals(22m, charge2.JR_OSSellAmt);
					AssertEquals(22m, charge2.JR_LocalSellAmt);
					AssertEquals(22m, charge2.JR_EstimatedRevenue);
					Assert(charge2.IsCostPosted);
					Assert(!charge2.IsRevenuePosted);
					AssertEquals(-22m, charge2.APLine.AL_LineAmount);
					AssertEquals(-22m, charge2.ARLine.AL_LineAmount);

					AssertEquals("INV123/CUSDSB/1", charge3.JR_APInvoiceNum);
					AssertEquals(new ZDateTime(2005, 4, 12), charge3.JR_APInvoiceDate);
					AssertEquals("Customs Disbursement Charges\r\nCurrent Amounts\r\n  TEST                                      101.00", charge3.JR_Desc);
					AssertEquals(1m, charge3.JR_OSCostAmt);
					AssertEquals(1m, charge3.JR_LocalCostAmt);
					AssertEquals(1m, charge3.JR_EstimatedCost);
					AssertEquals(1m, charge3.JR_OSSellAmt);
					AssertEquals(1m, charge3.JR_LocalSellAmt);
					AssertEquals(1m, charge3.JR_EstimatedRevenue);
					Assert(charge3.IsCostPosted);
					Assert(!charge3.IsRevenuePosted);
					AssertEquals(-1m, charge3.APLine.AL_LineAmount);
					AssertEquals(-1m, charge3.ARLine.AL_LineAmount);

					AssertEquals("INV124/CUSDSB/1", charge4.JR_APInvoiceNum);
					AssertEquals(new ZDateTime(2005, 4, 12), charge4.JR_APInvoiceDate);
					AssertEquals("Customs Disbursement Charges\r\nCurrent Amounts\r\n  TEST                                       23.00", charge4.JR_Desc);
					AssertEquals(1m, charge4.JR_OSCostAmt);
					AssertEquals(1m, charge4.JR_LocalCostAmt);
					AssertEquals(1m, charge4.JR_EstimatedCost);
					AssertEquals(1m, charge4.JR_OSSellAmt);
					AssertEquals(1m, charge4.JR_LocalSellAmt);
					AssertEquals(1m, charge4.JR_EstimatedRevenue);
					Assert(charge4.IsCostPosted);
					Assert(!charge4.IsRevenuePosted);
					AssertEquals(-1m, charge4.APLine.AL_LineAmount);
					AssertEquals(-1m, charge4.ARLine.AL_LineAmount);

					AssertEquals("INV123/CUSDSB/2", charge5.JR_APInvoiceNum);
					AssertEquals(ZDateTime.Empty, charge5.JR_APInvoiceDate);
					AssertEquals("Customs Disbursement Charges\r\nCurrent Amounts\r\n  TEST                                      120.00", charge5.JR_Desc);
					AssertEquals(19m, charge5.JR_OSCostAmt);
					AssertEquals(19m, charge5.JR_LocalCostAmt);
					AssertEquals(19m, charge5.JR_EstimatedCost);
					AssertEquals(19m, charge5.JR_OSSellAmt);
					AssertEquals(19m, charge5.JR_LocalSellAmt);
					AssertEquals(19m, charge5.JR_EstimatedRevenue);
					Assert(!charge5.IsCostPosted);
					Assert(!charge5.IsRevenuePosted);

					AssertEquals("INV124/CUSDSB/2", charge6.JR_APInvoiceNum);
					AssertEquals(ZDateTime.Empty, charge6.JR_APInvoiceDate);
					AssertEquals("Customs Disbursement Charges\r\nCurrent Amounts\r\n  TEST                                       30.00", charge6.JR_Desc);
					AssertEquals(7m, charge6.JR_OSCostAmt);
					AssertEquals(7m, charge6.JR_LocalCostAmt);
					AssertEquals(7m, charge6.JR_EstimatedCost);
					AssertEquals(7m, charge6.JR_OSSellAmt);
					AssertEquals(7m, charge6.JR_LocalSellAmt);
					AssertEquals(7m, charge6.JR_EstimatedRevenue);
					Assert(!charge6.IsCostPosted);
					Assert(!charge6.IsRevenuePosted);

					AssertEquals("one email should have been sent notifying discrepancy", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				});

				cusCharge = GetCustomCharge(chargeCode, 110, 0, true, testCreditor.PK);
				cusCharge2 = GetCustomCharge(chargeCode, 20, 0, true, testCreditor.PK);
				chargeProvider.CustomsCharges = new ICustomsCharges[] { cusCharge };
				chargeProvider2.CustomsCharges = new ICustomsCharges[] { cusCharge2 };

				Env.OutgoingMailManager.EmailsCreated.Clear();
				poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.SendEmail, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				CombineAssertions(() =>
				{
					AssertEquals("Six charge lines should have been created", 6, invoicingJob.Charges.Count);

					var charge1 = invoicingJob.Charges[0];
					var charge2 = invoicingJob.Charges[1];
					var charge3 = invoicingJob.Charges[2];
					var charge4 = invoicingJob.Charges[3];
					var charge5 = invoicingJob.Charges[4];
					var charge6 = invoicingJob.Charges[5];
					AssertEquals("INV123", charge1.JR_APInvoiceNum);
					AssertEquals(new ZDateTime(2005, 4, 12), charge1.JR_APInvoiceDate);
					AssertEquals("Customs Disbursement Charges\r\nCurrent Amounts\r\n  TEST                                      110.00", charge1.JR_Desc);
					AssertEquals(100m, charge1.JR_OSCostAmt);
					AssertEquals(100m, charge1.JR_LocalCostAmt);
					AssertEquals(100m, charge1.JR_EstimatedCost);
					AssertEquals(100m, charge1.JR_OSSellAmt);
					AssertEquals(100m, charge1.JR_LocalSellAmt);
					AssertEquals(100m, charge1.JR_EstimatedRevenue);
					Assert(charge1.IsCostPosted);
					Assert(charge1.IsRevenuePosted);
					AssertEquals(-100m, charge1.APLine.AL_LineAmount);
					AssertEquals(100m, charge1.ARLine.AL_LineAmount);

					AssertEquals("INV124", charge2.JR_APInvoiceNum);
					AssertEquals(new ZDateTime(2005, 4, 12), charge1.JR_APInvoiceDate);
					AssertEquals("Customs Disbursement Charges\r\nCurrent Amounts\r\n  TEST                                       20.00", charge2.JR_Desc);
					AssertEquals(22m, charge2.JR_OSCostAmt);
					AssertEquals(22m, charge2.JR_LocalCostAmt);
					AssertEquals(22m, charge2.JR_EstimatedCost);
					AssertEquals(22m, charge2.JR_OSSellAmt);
					AssertEquals(22m, charge2.JR_LocalSellAmt);
					AssertEquals(22m, charge2.JR_EstimatedRevenue);
					Assert(charge2.IsCostPosted);
					Assert(charge2.IsRevenuePosted);
					AssertEquals(-22m, charge2.APLine.AL_LineAmount);
					AssertEquals(22m, charge2.ARLine.AL_LineAmount);

					AssertEquals("INV123/CUSDSB/1", charge3.JR_APInvoiceNum);
					AssertEquals(new ZDateTime(2005, 4, 12), charge3.JR_APInvoiceDate);
					AssertEquals("Customs Disbursement Charges\r\nCurrent Amounts\r\n  TEST                                      110.00", charge3.JR_Desc);
					AssertEquals(1m, charge3.JR_OSCostAmt);
					AssertEquals(1m, charge3.JR_LocalCostAmt);
					AssertEquals(1m, charge3.JR_EstimatedCost);
					AssertEquals(1m, charge3.JR_OSSellAmt);
					AssertEquals(1m, charge3.JR_LocalSellAmt);
					AssertEquals(1m, charge3.JR_EstimatedRevenue);
					Assert(charge3.IsCostPosted);
					Assert(charge3.IsRevenuePosted);
					AssertEquals(-1m, charge3.APLine.AL_LineAmount);
					AssertEquals(1m, charge3.ARLine.AL_LineAmount);

					AssertEquals("INV124/CUSDSB/1", charge4.JR_APInvoiceNum);
					AssertEquals(new ZDateTime(2005, 4, 12), charge4.JR_APInvoiceDate);
					AssertEquals("Customs Disbursement Charges\r\nCurrent Amounts\r\n  TEST                                       20.00", charge4.JR_Desc);
					AssertEquals(1m, charge4.JR_OSCostAmt);
					AssertEquals(1m, charge4.JR_LocalCostAmt);
					AssertEquals(1m, charge4.JR_EstimatedCost);
					AssertEquals(1m, charge4.JR_OSSellAmt);
					AssertEquals(1m, charge4.JR_LocalSellAmt);
					AssertEquals(1m, charge4.JR_EstimatedRevenue);
					Assert(charge4.IsCostPosted);
					Assert(charge4.IsRevenuePosted);
					AssertEquals(-1m, charge4.APLine.AL_LineAmount);
					AssertEquals(1m, charge4.ARLine.AL_LineAmount);

					AssertEquals("INV123/CUSDSB/2", charge5.JR_APInvoiceNum);
					AssertEquals(new ZDateTime(2005, 4, 12), charge5.JR_APInvoiceDate);
					AssertEquals("Customs Disbursement Charges\r\nCurrent Amounts\r\n  TEST                                      110.00", charge5.JR_Desc);
					AssertEquals(9m, charge5.JR_OSCostAmt);
					AssertEquals(9m, charge5.JR_LocalCostAmt);
					AssertEquals(9m, charge5.JR_EstimatedCost);
					AssertEquals(9m, charge5.JR_OSSellAmt);
					AssertEquals(9m, charge5.JR_LocalSellAmt);
					AssertEquals(9m, charge5.JR_EstimatedRevenue);
					Assert(charge5.IsCostPosted);
					Assert(charge5.IsRevenuePosted);

					AssertEquals("INV124/CUSDSB/2", charge6.JR_APInvoiceNum);
					AssertEquals(new ZDateTime(2005, 4, 12), charge6.JR_APInvoiceDate);
					AssertEquals("Customs Disbursement Charges\r\nCurrent Amounts\r\n  TEST                                       20.00", charge6.JR_Desc);
					AssertEquals(-3m, charge6.JR_OSCostAmt);
					AssertEquals(-3m, charge6.JR_LocalCostAmt);
					AssertEquals(-3m, charge6.JR_EstimatedCost);
					AssertEquals(-3m, charge6.JR_OSSellAmt);
					AssertEquals(-3m, charge6.JR_LocalSellAmt);
					AssertEquals(-3m, charge6.JR_EstimatedRevenue);
					Assert(!charge6.IsCostPosted);
					Assert(!charge6.IsRevenuePosted);

					AssertEquals("one email should have been sent notifying discrepancy", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				});
			}
		}

		public void TestPostingARInvoiceOneAtATimeThenARWithDifferentAmount_AmountDecreased()
		{
			var staff = SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false);
				((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).DefaultDebtor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				Factory.Save();

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);
				var cusCharge2 = GetCustomCharge(chargeCode, 22, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory,
					AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false)
				};

				var chargeProvider2 = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge2 },
					InvoiceNumber = "INV124",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory,
					AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false)
				};

				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();
				var invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);
				AssertEquals("One charge line should have been created", 1, invoicingJob.Charges.Count);
				AssertEquals("INV123", invoicingJob.Charges[0].JR_APInvoiceNum);
				AssertEquals(-100m, invoicingJob.Charges[0].APLine.AL_LineAmount);
				AssertEquals("It has not been posted for cost", true, invoicingJob.Charges[0].IsCostPosted);
				AssertEquals("It has been posted for revenue", false, invoicingJob.Charges[0].IsRevenuePosted);

				customDetail.DataProviders.Add(chargeProvider2);
				poster.RaiseInvoices(customDetail);
				Factory.Save();
				AssertEquals("Two charge lines should have been created", 2, invoicingJob.Charges.Count);
				AssertEquals("INV124", invoicingJob.Charges[1].JR_APInvoiceNum);
				AssertEquals("It has not been posted for cost", true, invoicingJob.Charges[1].IsCostPosted);
				AssertEquals(-22m, invoicingJob.Charges[1].APLine.AL_LineAmount);
				AssertEquals("It has been posted for revenue", false, invoicingJob.Charges[1].IsRevenuePosted);

				AssertEquals("AP Already posted should remain same", "INV123", invoicingJob.Charges[0].JR_APInvoiceNum);
				AssertEquals("AP Already posted should remain same", -100m, invoicingJob.Charges[0].APLine.AL_LineAmount);

				cusCharge = GetCustomCharge(chargeCode, 90, 0, true, testCreditor.PK);
				cusCharge2 = GetCustomCharge(chargeCode, 10, 0, true, testCreditor.PK);
				chargeProvider.CustomsCharges = new ICustomsCharges[] { cusCharge };
				chargeProvider2.CustomsCharges = new ICustomsCharges[] { cusCharge2 };

				Env.OutgoingMailManager.EmailsCreated.Clear();
				poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.SendEmail, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				CombineAssertions(() =>
				{
					AssertEquals("Two charge lines should have been created", 4, invoicingJob.Charges.Count);

					var charge1 = invoicingJob.Charges[0];
					var charge2 = invoicingJob.Charges[1];
					var charge3 = invoicingJob.Charges[2];
					var charge4 = invoicingJob.Charges[3];
					AssertEquals("INV123", charge1.JR_APInvoiceNum);
					AssertEquals(100m, charge1.JR_OSCostAmt);
					AssertEquals(100m, charge1.JR_LocalCostAmt);
					AssertEquals(100m, charge1.JR_OSSellAmt);
					AssertEquals(100m, charge1.JR_LocalSellAmt);
					Assert(charge1.IsCostPosted);
					Assert(!charge1.IsRevenuePosted);
					AssertEquals("AP Detail", -100m, charge1.APLine.AL_LineAmount);
					AssertEquals("AR Detail", -100m, charge1.ARLine.AL_LineAmount);

					AssertEquals("INV124", charge2.JR_APInvoiceNum);
					AssertEquals(22m, charge2.JR_OSCostAmt);
					AssertEquals(22m, charge2.JR_LocalCostAmt);
					AssertEquals(22m, charge2.JR_OSSellAmt);
					AssertEquals(22m, charge2.JR_LocalSellAmt);
					Assert(charge2.IsCostPosted);
					Assert(!charge2.IsRevenuePosted);
					AssertEquals("AP Detail", -22m, charge2.APLine.AL_LineAmount);
					AssertEquals("AR Detail", -22m, charge2.ARLine.AL_LineAmount);

					AssertEquals("INV123/CUSDSB/1", charge3.JR_APInvoiceNum);
					AssertEquals(-10m, charge3.JR_OSCostAmt);
					AssertEquals(-10m, charge3.JR_LocalCostAmt);
					AssertEquals(-10m, charge3.JR_OSSellAmt);
					AssertEquals(-10m, charge3.JR_LocalSellAmt);
					Assert(!charge3.IsCostPosted);
					Assert(!charge3.IsRevenuePosted);
					AssertEquals("AP Detail", -10m, charge3.APLine.AL_LineAmount);
					AssertEquals("AR Detail", 10m, charge3.ARLine.AL_LineAmount);

					AssertEquals("INV124/CUSDSB/1", charge4.JR_APInvoiceNum);
					AssertEquals(-12m, charge4.JR_OSCostAmt);
					AssertEquals(-12m, charge4.JR_LocalCostAmt);
					AssertEquals(-12m, charge4.JR_OSSellAmt);
					AssertEquals(-12m, charge4.JR_LocalSellAmt);
					Assert(!charge4.IsCostPosted);
					Assert(!charge4.IsRevenuePosted);
					AssertEquals("AP Detail", -12m, charge4.APLine.AL_LineAmount);
					AssertEquals("AR Detail", 12m, charge4.ARLine.AL_LineAmount);

					AssertEquals("one email should have been sent notifying discrepancy", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				});
			}
		}

		public void TestPostingInvoices_PostZeroAmount()
		{
			var staff = SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false);
				((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).DefaultDebtor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				Factory.Save();

				var cusCharge = GetCustomCharge(chargeCode, 0, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory,
					AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false)
				};

				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();
				var invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);
				AssertEquals("No charge line should have been created", 0, invoicingJob.Charges.Count);
			}
		}

		public void TestPostingInvoices_ResetToZeroAmountAfterAPPosted()
		{
			var staff = SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false);
				((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).DefaultDebtor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				Factory.Save();

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					APDueDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory,
					AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false)
				};

				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();
				var invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);
				AssertEquals("One charge line should have been created", 1, invoicingJob.Charges.Count);
				AssertEquals("INV123", invoicingJob.Charges[0].JR_APInvoiceNum);
				AssertEquals(-100m, invoicingJob.Charges[0].APLine.AL_LineAmount);
				AssertEquals("It has not been posted for cost", true, invoicingJob.Charges[0].IsCostPosted);
				AssertEquals("It has been posted for revenue", false, invoicingJob.Charges[0].IsRevenuePosted);

				cusCharge = GetCustomCharge(chargeCode, 0, 0, true, testCreditor.PK);
				chargeProvider.CustomsCharges = new ICustomsCharges[] { cusCharge };
				poster.RaiseInvoices(customDetail);
				Factory.Save();
				var charge1 = invoicingJob.Charges[0];
				var charge2 = invoicingJob.Charges[1];
				AssertEquals("INV123", charge1.JR_APInvoiceNum);
				AssertEquals(100m, charge1.JR_OSCostAmt);
				AssertEquals(100m, charge1.JR_LocalCostAmt);
				AssertEquals(100m, charge1.JR_OSSellAmt);
				AssertEquals(100m, charge1.JR_LocalSellAmt);
				Assert(charge1.IsCostPosted);
				Assert(!charge1.IsRevenuePosted);
				AssertEquals("AP Detail", -100m, charge1.APLine.AL_LineAmount);
				AssertEquals("AR Detail", -100m, charge1.ARLine.AL_LineAmount);

				AssertEquals("INV123/CUSDSB/1", charge2.JR_APInvoiceNum);
				AssertEquals(-100m, charge2.JR_OSCostAmt);
				AssertEquals(-100m, charge2.JR_LocalCostAmt);
				AssertEquals(-100m, charge2.JR_OSSellAmt);
				AssertEquals(-100m, charge2.JR_LocalSellAmt);
				Assert(!charge2.IsCostPosted);
				Assert(!charge2.IsRevenuePosted);
				AssertEquals("AP Detail", -100m, charge2.APLine.AL_LineAmount);
				AssertEquals("AR Detail", 100m, charge2.ARLine.AL_LineAmount);
				Assert(!charge2.JR_APInvoiceDate.IsEmpty);
				Assert(!charge2.JR_PaymentDate.IsEmpty);
				Assert(!charge2.PostARWhenInvokedByCustomInvoiceCreator);
				Assert(charge2.PostAPWhenInvokedByCustomInvoiceCreator);

				cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);
				chargeProvider.CustomsCharges = new ICustomsCharges[] { cusCharge };
				poster.RaiseInvoices(customDetail);
				Factory.Save();
				charge1 = invoicingJob.Charges[0];
				charge2 = invoicingJob.Charges[1];
				AssertEquals("INV123", charge1.JR_APInvoiceNum);
				AssertEquals(100m, charge1.JR_OSCostAmt);
				AssertEquals(100m, charge1.JR_LocalCostAmt);
				AssertEquals(100m, charge1.JR_OSSellAmt);
				AssertEquals(100m, charge1.JR_LocalSellAmt);
				Assert(charge1.IsCostPosted);
				Assert(!charge1.IsRevenuePosted);
				AssertEquals("AP Detail", -100m, charge1.APLine.AL_LineAmount);
				AssertEquals("AR Detail", -100m, charge1.ARLine.AL_LineAmount);

				AssertEquals(ZString.Empty, charge2.JR_APInvoiceNum);
				AssertEquals(0m, charge2.JR_OSCostAmt);
				AssertEquals(0m, charge2.JR_LocalCostAmt);
				AssertEquals(0m, charge2.JR_OSSellAmt);
				AssertEquals(0m, charge2.JR_LocalSellAmt);
				Assert(!charge2.IsCostPosted);
				Assert(!charge2.IsRevenuePosted);
				AssertNull("AP Detail", charge2.APLine);
				AssertNull("AR Detail", charge2.ARLine);
				Assert(!charge2.JR_APInvoiceDate.IsEmpty);
				Assert(!charge2.JR_PaymentDate.IsEmpty);
				Assert(!charge2.PostAPWhenInvokedByCustomInvoiceCreator);
				Assert(!charge2.PostARWhenInvokedByCustomInvoiceCreator);

				cusCharge = GetCustomCharge(chargeCode, 0, 0, true, testCreditor.PK);
				chargeProvider.CustomsCharges = new ICustomsCharges[] { cusCharge };
				poster.RaiseInvoices(customDetail);
				Factory.Save();
				AssertEquals(2, invoicingJob.Charges.Count);
				charge1 = invoicingJob.Charges[0];
				charge2 = invoicingJob.Charges[1];
				AssertEquals("INV123", charge1.JR_APInvoiceNum);
				AssertEquals(100m, charge1.JR_OSCostAmt);
				AssertEquals(100m, charge1.JR_LocalCostAmt);
				AssertEquals(100m, charge1.JR_OSSellAmt);
				AssertEquals(100m, charge1.JR_LocalSellAmt);
				Assert(charge1.IsCostPosted);
				Assert(!charge1.IsRevenuePosted);
				AssertEquals("AP Detail", -100m, charge1.APLine.AL_LineAmount);
				AssertEquals("AR Detail", -100m, charge1.ARLine.AL_LineAmount);

				AssertEquals("INV123/CUSDSB/1", charge2.JR_APInvoiceNum);
				AssertEquals(-100m, charge2.JR_OSCostAmt);
				AssertEquals(-100m, charge2.JR_LocalCostAmt);
				AssertEquals(-100m, charge2.JR_OSSellAmt);
				AssertEquals(-100m, charge2.JR_LocalSellAmt);
				Assert(!charge2.IsCostPosted);
				Assert(!charge2.IsRevenuePosted);
				AssertEquals("AP Detail", -100m, charge2.APLine.AL_LineAmount);
				AssertEquals("AR Detail", 100m, charge2.ARLine.AL_LineAmount);
				Assert(!charge2.JR_APInvoiceDate.IsEmpty);
				Assert(!charge2.JR_PaymentDate.IsEmpty);
				Assert(!charge2.PostARWhenInvokedByCustomInvoiceCreator);
				Assert(charge2.PostAPWhenInvokedByCustomInvoiceCreator);
			}
		}

		public void TestPostingInvocies_CreditNoteCreatedForNegativeCharge()
		{
			var staff = SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false);
				((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).DefaultDebtor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				Factory.Save();

				var cusCharge = GetCustomCharge(chargeCode, -100, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory,
					AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false)
				};

				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.PostNegativeCost, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();
				var invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);
				AssertEquals("One charge line should have been created", 1, invoicingJob.Charges.Count);
				var charge = invoicingJob.Charges[0];
				Assert("Cost is posted", charge.IsCostPosted);
				AssertEquals("AP Detail", 100m, charge.APLine.AL_LineAmount);
				AssertEquals(TransactionTypes.CreditNote, invoicingJob.Charges[0].APLine.TransactionHeader.AH_TransactionType);
			}
		}

		public void TestPostingInvoices_ResetToZeroAmountAfterARAPPosted()
		{
			var staff = SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false);
				((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).DefaultDebtor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				Factory.Save();

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					APDueDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory,
					AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false)
				};

				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.ARPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();
				var invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);
				AssertEquals("One charge line should have been created", 1, invoicingJob.Charges.Count);
				AssertEquals("INV123", invoicingJob.Charges[0].JR_APInvoiceNum);
				AssertEquals(-100m, invoicingJob.Charges[0].APLine.AL_LineAmount);
				AssertEquals("It has not been posted for cost", true, invoicingJob.Charges[0].IsCostPosted);
				AssertEquals("It has been posted for revenue", true, invoicingJob.Charges[0].IsRevenuePosted);

				// Post Zero Amount
				// JR_APInvoiceNum JR_OSCostAmt JR_LocalCostAmt JR_OSSellAmt JR_LocalSellAmt IsCostPosted IsRevenuePosted
				// INV123			100m		100m			100m			100m			true		true
				// INV123/CUSDSB/1	-100m		-100m			-100m			-100m			false		false
				cusCharge = GetCustomCharge(chargeCode, 0, 0, true, testCreditor.PK);
				chargeProvider.CustomsCharges = new ICustomsCharges[] { cusCharge };
				poster.RaiseInvoices(customDetail);
				Factory.Save();
				invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				var charge1 = invoicingJob.Charges[0];
				var charge2 = invoicingJob.Charges[1];
				AssertEquals("INV123", charge1.JR_APInvoiceNum);
				AssertEquals(100m, charge1.JR_OSCostAmt);
				AssertEquals(100m, charge1.JR_LocalCostAmt);
				AssertEquals(100m, charge1.JR_OSSellAmt);
				AssertEquals(100m, charge1.JR_LocalSellAmt);
				Assert(charge1.IsCostPosted);
				Assert(charge1.IsRevenuePosted);
				AssertEquals("AP Detail", -100m, charge1.APLine.AL_LineAmount);
				AssertEquals("AR Detail", 100m, charge1.ARLine.AL_LineAmount);

				AssertEquals("INV123/CUSDSB/1", charge2.JR_APInvoiceNum);
				AssertEquals(-100m, charge2.JR_OSCostAmt);
				AssertEquals(-100m, charge2.JR_LocalCostAmt);
				AssertEquals(-100m, charge2.JR_OSSellAmt);
				AssertEquals(-100m, charge2.JR_LocalSellAmt);
				Assert(!charge2.IsCostPosted);
				Assert(!charge2.IsRevenuePosted);
				AssertEquals("AP Detail", -100m, charge2.APLine.AL_LineAmount);
				AssertEquals("AR Detail", 100m, charge2.ARLine.AL_LineAmount);
				Assert(!charge2.JR_APInvoiceDate.IsEmpty);
				Assert(!charge2.JR_PaymentDate.IsEmpty);
				Assert(!charge2.PostARWhenInvokedByCustomInvoiceCreator);
				Assert(charge2.PostAPWhenInvokedByCustomInvoiceCreator);

				// Post Non-Zero Amount
				// JR_APInvoiceNum JR_OSCostAmt JR_LocalCostAmt JR_OSSellAmt JR_LocalSellAmt IsCostPosted IsRevenuePosted
				// INV123			100m		100m			100m			100m			true		true
				// INV123/CUSDSB/1	0m			0m				0m				0m			false		false
				cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);
				chargeProvider.CustomsCharges = new ICustomsCharges[] { cusCharge };
				poster.RaiseInvoices(customDetail);
				Factory.Save();
				invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				charge1 = invoicingJob.Charges[0];
				charge2 = invoicingJob.Charges[1];
				AssertEquals("INV123", charge1.JR_APInvoiceNum);
				AssertEquals(100m, charge1.JR_OSCostAmt);
				AssertEquals(100m, charge1.JR_LocalCostAmt);
				AssertEquals(100m, charge1.JR_OSSellAmt);
				AssertEquals(100m, charge1.JR_LocalSellAmt);
				Assert(charge1.IsCostPosted);
				Assert(charge1.IsRevenuePosted);
				AssertEquals("AP Detail", -100m, charge1.APLine.AL_LineAmount);
				AssertEquals("AR Detail", 100m, charge1.ARLine.AL_LineAmount);

				AssertEquals(ZString.Empty, charge2.JR_APInvoiceNum);
				AssertEquals(0m, charge2.JR_OSCostAmt);
				AssertEquals(0m, charge2.JR_LocalCostAmt);
				AssertEquals(0m, charge2.JR_OSSellAmt);
				AssertEquals(0m, charge2.JR_LocalSellAmt);
				Assert(!charge2.IsCostPosted);
				Assert(!charge2.IsRevenuePosted);
				AssertNull("AP Detail", charge2.APLine);
				AssertNull("AR Detail", charge2.ARLine);
				Assert(!charge2.JR_APInvoiceDate.IsEmpty);
				Assert(!charge2.JR_PaymentDate.IsEmpty);
				Assert(!charge2.PostAPWhenInvokedByCustomInvoiceCreator);
				Assert(!charge2.PostARWhenInvokedByCustomInvoiceCreator);

				// Post Zero Amount
				// JR_APInvoiceNum JR_OSCostAmt JR_LocalCostAmt JR_OSSellAmt JR_LocalSellAmt IsCostPosted IsRevenuePosted
				// INV123			100m		100m			100m			100m			true		true
				// INV123/CUSDSB/1	-100m		-100m			-100m			-100m			false		false
				cusCharge = GetCustomCharge(chargeCode, 0, 0, true, testCreditor.PK);
				chargeProvider.CustomsCharges = new ICustomsCharges[] { cusCharge };
				poster.RaiseInvoices(customDetail);
				Factory.Save();
				invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertEquals(2, invoicingJob.Charges.Count);
				charge1 = invoicingJob.Charges[0];
				charge2 = invoicingJob.Charges[1];
				AssertEquals("INV123", charge1.JR_APInvoiceNum);
				AssertEquals(100m, charge1.JR_OSCostAmt);
				AssertEquals(100m, charge1.JR_LocalCostAmt);
				AssertEquals(100m, charge1.JR_OSSellAmt);
				AssertEquals(100m, charge1.JR_LocalSellAmt);
				Assert(charge1.IsCostPosted);
				Assert(charge1.IsRevenuePosted);
				AssertEquals("AP Detail", -100m, charge1.APLine.AL_LineAmount);
				AssertEquals("AR Detail", 100m, charge1.ARLine.AL_LineAmount);

				AssertEquals("INV123/CUSDSB/1", charge2.JR_APInvoiceNum);
				AssertEquals(-100m, charge2.JR_OSCostAmt);
				AssertEquals(-100m, charge2.JR_LocalCostAmt);
				AssertEquals(-100m, charge2.JR_OSSellAmt);
				AssertEquals(-100m, charge2.JR_LocalSellAmt);
				Assert(!charge2.IsCostPosted);
				Assert(!charge2.IsRevenuePosted);
				AssertEquals("AP Detail", -100m, charge2.APLine.AL_LineAmount);
				AssertEquals("AR Detail", 100m, charge2.ARLine.AL_LineAmount);
				Assert(!charge2.JR_APInvoiceDate.IsEmpty);
				Assert(!charge2.JR_PaymentDate.IsEmpty);
				Assert(!charge2.PostARWhenInvokedByCustomInvoiceCreator);
				Assert(charge2.PostAPWhenInvokedByCustomInvoiceCreator);
			}
		}

		public void TestPostingInvoices_ReOpenClosedJob_AfterARAPPosted()
		{
			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).DefaultDebtor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				Factory.Save();

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK) },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					APDueDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory,
				};

				customDetail.DataProviders.Add(chargeProvider);

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.ARPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();
				var invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				invoicingJob.JH_Status = JobHeaderStatus.Closed.Code;

				chargeProvider.CustomsCharges = new ICustomsCharges[] { GetCustomCharge(chargeCode, 0, 0, true, testCreditor.PK) };
				poster.RaiseInvoices(customDetail);
				Factory.Save();
				invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertEquals(JobHeaderStatus.Working.Code, invoicingJob.JH_Status);
			}
		}

		public void TestPostingInvoices_ShouldNotReOpenClosedJobIfNoChanges()
		{
			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).DefaultDebtor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				Factory.Save();

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK) },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					APDueDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory,
				};

				customDetail.DataProviders.Add(chargeProvider);

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				chargeProvider.CustomsCharges = new ICustomsCharges[] { GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK) };
				poster.RaiseInvoices(customDetail);
				Factory.Save();
				var invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				invoicingJob.JH_Status = JobHeaderStatus.Closed.Code;

				chargeProvider.CustomsCharges = new ICustomsCharges[] { GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK) };
				poster.RaiseInvoices(customDetail);
				Factory.Save();
				invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertEquals(JobHeaderStatus.Closed.Code, invoicingJob.JH_Status);
			}
		}

		public void TestPostingInvoices_ShouldNotChangeChargeDescriptionIfChargeAmountDoesNotChange()
		{
			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).DefaultDebtor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				Factory.Save();

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK) },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					APDueDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory,
				};

				customDetail.DataProviders.Add(chargeProvider);

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				var invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				//Mimic the description generated from Job Invoicing menu
				var jobCharge = invoicingJob.Charges[0];
				jobCharge.JR_Desc = @"Customs Disbursement Charges
  TEST                                      100.00";
				Factory.Save();

				chargeProvider.CustomsCharges = new ICustomsCharges[] { GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK) };
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				invoicingJob.JH_Status = JobHeaderStatus.Closed.Code;
				AssertEquals("Unchanged", @"Customs Disbursement Charges
  TEST                                      100.00", jobCharge.JR_Desc);
			}
		}

		public void TestPostingInvoices_ReOpenClosedJob_CS00851873_ARPostDSB()
		{
			AssertPostingInvoices_ReOpenClosedJob(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB, JobHeaderStatus.JobInvoiced.Code);
		}

		public void TestPostingInvoices_ReOpenClosedJob_CS00851873_APPostDSB()
		{
			AssertPostingInvoices_ReOpenClosedJob(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, JobHeaderStatus.Working.Code);
		}

		public void TestPostingInvoices_ReOpenClosedJob_CS00851873_AutoRateDSB()
		{
			AssertPostingInvoices_ReOpenClosedJob(ChargePosterBehaviours.AutoRateDSB, JobHeaderStatus.Working.Code);
		}

		void AssertPostingInvoices_ReOpenClosedJob(ChargePosterBehaviours chargePosterBehaviours, ZString expectedJobStatus)
		{
			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).DefaultDebtor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				var job = CreateInvoicingJob(customDetail.JobNumber, customDetail);
				job.JH_Status = JobHeaderStatus.Closed.Code;
				Factory.Save();

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK) },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					APDueDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory,
				};

				customDetail.DataProviders.Add(chargeProvider);

				var poster = new CustomsDisbursementChargePoster(chargePosterBehaviours, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				var invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertEquals(expectedJobStatus, invoicingJob.JH_Status);
			}
		}

		public void TestPostingInvoices_PostAnotherDSB()
		{
			var staff = SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false);
				((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).DefaultDebtor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				Factory.Save();

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					APDueDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory,
					AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false)
				};

				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.ARPostDSB, new[] { chargeCode.PK, CC1.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();
				var invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);
				AssertEquals("One charge line should have been created", 1, invoicingJob.Charges.Count);
				AssertEquals("INV123", invoicingJob.Charges[0].JR_APInvoiceNum);
				AssertEquals(-100m, invoicingJob.Charges[0].APLine.AL_LineAmount);
				AssertEquals("It has not been posted for cost", true, invoicingJob.Charges[0].IsCostPosted);
				AssertEquals("It has been posted for revenue", true, invoicingJob.Charges[0].IsRevenuePosted);

				// Post Another DSB
				// JR_APInvoiceNum JR_OSCostAmt JR_LocalCostAmt JR_OSSellAmt JR_LocalSellAmt IsCostPosted IsRevenuePosted
				// INV123			100m		100m			100m			100m			true		true
				// INV123/ZZCC1		111m		111m			111m			111m			true		true
				var cusCharge2 = GetCustomCharge(CC1, 111, 0, true, testCreditor.PK);
				chargeProvider.CustomsCharges = new ICustomsCharges[] { cusCharge, cusCharge2 };
				poster.RaiseInvoices(customDetail);
				Factory.Save();
				invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				var charge1 = invoicingJob.Charges[0];
				var charge2 = invoicingJob.Charges[1];
				AssertEquals("INV123", charge1.JR_APInvoiceNum);
				AssertEquals(100m, charge1.JR_OSCostAmt);
				AssertEquals(100m, charge1.JR_LocalCostAmt);
				AssertEquals(100m, charge1.JR_OSSellAmt);
				AssertEquals(100m, charge1.JR_LocalSellAmt);
				Assert(charge1.IsCostPosted);
				Assert(charge1.IsRevenuePosted);
				AssertEquals("AP Detail", -100m, charge1.APLine.AL_LineAmount);
				AssertEquals("AR Detail", 100m, charge1.ARLine.AL_LineAmount);

				AssertEquals("INV123/ZZCC1", charge2.JR_APInvoiceNum);
				AssertEquals(111m, charge2.JR_OSCostAmt);
				AssertEquals(111m, charge2.JR_LocalCostAmt);
				AssertEquals(111m, charge2.JR_OSSellAmt);
				AssertEquals(111m, charge2.JR_LocalSellAmt);
				Assert(charge2.IsCostPosted);
				Assert(charge2.IsRevenuePosted);
				AssertEquals("AP Detail", -111m, charge2.APLine.AL_LineAmount);
				AssertEquals("AR Detail", 111m, charge2.ARLine.AL_LineAmount);
				Assert(!charge2.JR_APInvoiceDate.IsEmpty);
				Assert(!charge2.JR_PaymentDate.IsEmpty);
				Assert(charge2.PostARWhenInvokedByCustomInvoiceCreator);
				Assert(charge2.PostAPWhenInvokedByCustomInvoiceCreator);
			}
		}

		public void TestDoNotAttemptToPostARWhenItIsAlreadyPosted()
		{
			SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).DefaultDebtor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				Factory.Save();
				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory
				};
				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				var invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);

				AssertEquals("One charge line should have been created", 1, invoicingJob.Charges.Count);
				AssertEquals("It has not been posted for cost", false, invoicingJob.Charges[0].IsCostPosted);
				AssertEquals("INV123", invoicingJob.Charges[0].JR_APInvoiceNum);
				AssertEquals("It has been posted for revenue", true, invoicingJob.Charges[0].IsRevenuePosted);
				AssertEquals(100m, invoicingJob.Charges[0].ARLine.AL_LineAmount);

				poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				AssertEquals("Still one charge line", 1, invoicingJob.Charges.Count);
				AssertEquals("Cost is posted", true, invoicingJob.Charges[0].IsCostPosted);
				AssertEquals("INV123", invoicingJob.Charges[0].JR_APInvoiceNum);
				AssertEquals("Revenue is posted as before", true, invoicingJob.Charges[0].IsRevenuePosted);
				AssertEquals(100m, invoicingJob.Charges[0].ARLine.AL_LineAmount);
				AssertEquals(-100m, invoicingJob.Charges[0].APLine.AL_LineAmount);
			}
		}

		public void TestSendEmailToMultipleRecipients()
		{
			var staff = SetupStaffMemberEmailAddress();

			var user1 = Factory.NewWithValidTestData<GlbStaff>();
			user1.GS_Code = "US1";
			user1.GS_EmailAddress = "user1@whatever.com";
			Factory.Save();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.AutoPostingNotification = new AutoPostingNotification(new[] { staff, user1.PK }, false);

				var customDetail2 = Factory.New<MockCustomsJobProvider>();
				customDetail2.JobNumber = "TR436878";
				customDetail2.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail2.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail2.AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false);

				Factory.Save();

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);
				var cusCharge2 = GetCustomCharge(chargeCode, 22, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory,
					AutoPostingNotification = new AutoPostingNotification(new[] { staff, user1.PK }, false)
				};
				customDetail.DataProviders.Add(chargeProvider);

				var chargeProvider2 = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge2 },
					InvoiceNumber = "INV124",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail2,
					Factory = Factory,
					AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false)
				};
				customDetail.DataProviders.Add(chargeProvider2);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.SendEmail, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();
				var invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);
				AssertEquals("It has been posted for cost", true, invoicingJob.Charges[0].IsCostPosted);

				poster.RaiseInvoices(customDetail2);
				invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail2));
				AssertNotNull("One invoicing job should have been created", invoicingJob);
				AssertEquals("It has been posted for cost", true, invoicingJob.Charges[0].IsCostPosted);

				cusCharge = GetCustomCharge(chargeCode, 101, 0, true, testCreditor.PK);
				cusCharge2 = GetCustomCharge(chargeCode, 23, 0, true, testCreditor.PK);
				chargeProvider.CustomsCharges = new ICustomsCharges[] { cusCharge };
				chargeProvider2.CustomsCharges = new ICustomsCharges[] { cusCharge2 };

				Env.OutgoingMailManager.EmailsCreated.Clear();

				poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.SendEmail, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				AssertEquals("two emails should have been sent to PM and user1", 2, Env.OutgoingMailManager.EmailsCreated.Count);

				EmailDef emailToUser1 = null;
				EmailDef emailToPM = null;

				foreach (var email in Env.OutgoingMailManager.EmailsCreated)
				{
					if (email.Recipients[0].Email == "user1@whatever.com")
					{
						emailToUser1 = email;
					}
					else
					{
						emailToPM = email;
					}
				}

				AssertEquals(true, emailToUser1.Body.Contains("Processing result for INV123"));
				AssertEquals(false, emailToUser1.Body.Contains("Processing result for INV124"));//user1 is not set as a recepient of the job

				AssertEquals(true, emailToPM.Body.Contains("Processing result for INV123"));
				AssertEquals(true, emailToPM.Body.Contains("Processing result for INV124"));
			}
		}

		public void TestPostARThenAPWithDifferentAmounts()
		{
			SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).DefaultDebtor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				Factory.Save();

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);
				var cusCharge2 = GetCustomCharge(chargeCode, 22, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory
				};

				var chargeProvider2 = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge2 },
					InvoiceNumber = "INV124",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory
				};
				customDetail.DataProviders.Add(chargeProvider);
				customDetail.DataProviders.Add(chargeProvider2);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				var invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);

				AssertEquals("Two charge lines should have been created", 2, invoicingJob.Charges.Count);

				AssertEquals("It has not been posted for cost", false, invoicingJob.Charges[0].IsCostPosted);
				AssertEquals("It has not been posted for cost", false, invoicingJob.Charges[1].IsCostPosted);

				AssertEquals("INV123", invoicingJob.Charges[0].JR_APInvoiceNum);
				AssertEquals("INV124", invoicingJob.Charges[1].JR_APInvoiceNum);

				AssertEquals("It has been posted for revenue", true, invoicingJob.Charges[0].IsRevenuePosted);
				AssertEquals("It has been posted for revenue", true, invoicingJob.Charges[1].IsRevenuePosted);

				AssertEquals("posted into the same AR Invoice", invoicingJob.Charges[0].ARLine.TransactionHeader.PK, invoicingJob.Charges[1].ARLine.TransactionHeader.PK);

				AssertEquals(100m, invoicingJob.Charges[0].ARLine.AL_LineAmount);
				AssertEquals(22m, invoicingJob.Charges[1].ARLine.AL_LineAmount);

				cusCharge = GetCustomCharge(chargeCode, 101, 0, true, testCreditor.PK);
				cusCharge2 = GetCustomCharge(chargeCode, 23, 0, true, testCreditor.PK);
				chargeProvider.CustomsCharges = new ICustomsCharges[] { cusCharge };
				chargeProvider2.CustomsCharges = new ICustomsCharges[] { cusCharge2 };

				//Post AP as well
				poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);

				CombineAssertions(() =>
				{
					AssertEquals("Four charge lines should have been created", 4, invoicingJob.Charges.Count);

					var charge1 = invoicingJob.Charges[0];
					var charge2 = invoicingJob.Charges[1];
					AssertEquals("It has been posted for revenue", true, charge1.IsRevenuePosted);
					AssertEquals("It has been posted for revenue", true, charge2.IsRevenuePosted);

					AssertEquals("AR Amount", 100m, charge1.ARLine.AL_LineAmount);
					AssertEquals("AR Amount", 22m, charge2.ARLine.AL_LineAmount);

					AssertEquals("It should be posted for cost", true, charge1.IsCostPosted);
					AssertEquals("It should be posted for cost", true, charge2.IsCostPosted);

					AssertEquals("AP Amount", -100m, charge1.APLine.AL_LineAmount);
					AssertEquals("AP Amount", -22m, charge2.APLine.AL_LineAmount);

					AssertEquals(100m, charge1.JR_LocalCostAmt);
					AssertEquals(100m, charge1.JR_LocalSellAmt);

					AssertEquals(22m, charge2.JR_LocalCostAmt);
					AssertEquals(22m, charge2.JR_LocalSellAmt);

					var charge3 = invoicingJob.Charges[2];
					var charge4 = invoicingJob.Charges[3];
					AssertEquals("It has been posted for revenue", true, charge3.IsRevenuePosted);
					AssertEquals("It has been posted for revenue", true, charge4.IsRevenuePosted);

					AssertEquals("AR Amount", 1m, charge3.ARLine.AL_LineAmount);
					AssertEquals("AR Amount", 1m, charge4.ARLine.AL_LineAmount);

					AssertEquals("It should be posted for cost", true, charge3.IsCostPosted);
					AssertEquals("It should be posted for cost", true, charge4.IsCostPosted);

					AssertEquals("AP Amount", -1m, charge3.APLine.AL_LineAmount);
					AssertEquals("AP Amount", -1m, charge4.APLine.AL_LineAmount);

					AssertEquals(1m, charge3.JR_LocalCostAmt);
					AssertEquals(1m, charge3.JR_LocalSellAmt);

					AssertEquals(1m, charge4.JR_LocalCostAmt);
					AssertEquals(1m, charge4.JR_LocalSellAmt);
				});
			}
		}

		public void TestKeepUpdatingUntilPosted()
		{
			SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());

				Factory.Save();
				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory
				};
				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				var invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);

				AssertEquals("One charge line should have been created", 1, invoicingJob.Charges.Count);
				AssertEquals("It has not been posted for cost", false, invoicingJob.Charges[0].IsCostPosted);
				AssertEquals("It has been posted for revenue", false, invoicingJob.Charges[0].IsRevenuePosted);
				AssertEquals("INV123", invoicingJob.Charges[0].JR_APInvoiceNum);
				AssertEquals(100m, invoicingJob.Charges[0].JR_LocalCostAmt);
				AssertEquals(100m, invoicingJob.Charges[0].JR_LocalSellAmt);

				cusCharge = GetCustomCharge(chargeCode, 101, 0, true, testCreditor.PK);
				chargeProvider.CustomsCharges = new ICustomsCharges[] { cusCharge };
				poster.RaiseInvoices(customDetail);

				invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertEquals("One charge line should have been created", 1, invoicingJob.Charges.Count);
				AssertEquals("It has not been posted for cost", false, invoicingJob.Charges[0].IsCostPosted);
				AssertEquals("It has been posted for revenue", false, invoicingJob.Charges[0].IsRevenuePosted);
				AssertEquals("INV123", invoicingJob.Charges[0].JR_APInvoiceNum);
				AssertEquals(101m, invoicingJob.Charges[0].JR_LocalCostAmt);
				AssertEquals(101m, invoicingJob.Charges[0].JR_LocalSellAmt);

				cusCharge = GetCustomCharge(chargeCode, 102, 0, true, testCreditor.PK);
				chargeProvider.CustomsCharges = new ICustomsCharges[] { cusCharge };
				poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				AssertEquals("One charge line should have been created", 1, invoicingJob.Charges.Count);
				AssertEquals("It has been posted for cost", true, invoicingJob.Charges[0].IsCostPosted);
				AssertEquals("It has not been posted for revenue", false, invoicingJob.Charges[0].IsRevenuePosted);
				AssertEquals("INV123", invoicingJob.Charges[0].JR_APInvoiceNum);
				AssertEquals(-102m, invoicingJob.Charges[0].APLine.AL_LineAmount);
			}
		}

		public void TestKeepUpdatingWithZeroAmountsUntilPosted()
		{
			SetupStaffMemberEmailAddress();

			#region TestData SetUp
			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());

				Factory.Save();

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory
				};
				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();
				#endregion

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				Job invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);

				AssertEquals("One charge line should have been created", 1, invoicingJob.Charges.Count);
				AssertEquals("It has not been posted for cost", false, invoicingJob.Charges[0].IsCostPosted);
				AssertEquals("It has been posted for revenue", false, invoicingJob.Charges[0].IsRevenuePosted);
				AssertEquals("INV123", invoicingJob.Charges[0].JR_APInvoiceNum);
				AssertEquals(100m, invoicingJob.Charges[0].JR_LocalCostAmt);
				AssertEquals(100m, invoicingJob.Charges[0].JR_LocalSellAmt);

				cusCharge = GetCustomCharge(chargeCode, 0, 0, true, testCreditor.PK);
				chargeProvider.CustomsCharges = new ICustomsCharges[] { cusCharge };
				poster.RaiseInvoices(customDetail);

				invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertEquals("The charge line should have amounts cleared", 0m, invoicingJob.Charges[0].JR_OSCostAmt);
				AssertEquals("The charge line should have amounts cleared", 0m, invoicingJob.Charges[0].JR_OSSellAmt);
				Assert("Should not be overridden", !invoicingJob.Charges[0].JR_CostRatingOverride);
				Assert("Should not be overridden", !invoicingJob.Charges[0].JR_SellRatingOverride);
			}
		}

		public void TestUnmatchedChargeLinesShouldNotBeClearedWithMultipleProviders()
		{
			SetupStaffMemberEmailAddress();

			#region TestData SetUp
			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());

				Factory.Save();

				var cusCharge1 = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);

				var chargeProvider1 = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge1 },
					InvoiceNumber = "INV111",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory
				};
				var chargeProvider2 = new MockCustomsChargesProvider
				{
					CustomsCharges = Array.Empty<ICustomsCharges>(),
					InvoiceNumber = "INV222",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory
				};
				customDetail.DataProviders.Add(chargeProvider1);
				customDetail.DataProviders.Add(chargeProvider2);
				Env.OutgoingMailManager.EmailsCreated.Clear();
				#endregion

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				Job invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);

				AssertEquals("One charge line should have been created", 1, invoicingJob.Charges.Count);
				AssertEquals("It has not been posted for cost", false, invoicingJob.Charges[0].IsCostPosted);
				AssertEquals("It has been posted for revenue", false, invoicingJob.Charges[0].IsRevenuePosted);
				AssertEquals("INV111", invoicingJob.Charges[0].JR_APInvoiceNum);
				AssertEquals("Matched charge lines should be cleared", 0m, invoicingJob.Charges[0].JR_LocalCostAmt);

				chargeProvider1.MatchCustomsChargesToClearFunc = (inv, desc) => inv == "INV111";
				chargeProvider2.MatchCustomsChargesToClearFunc = (inv, desc) => inv == "INV222";
				chargeProvider2.CustomsCharges = Array.Empty<ICustomsCharges>();

				poster.RaiseInvoices(customDetail);

				invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertEquals("Unmatched charge line should not have amounts cleared", 100m, invoicingJob.Charges[0].JR_OSCostAmt);
			}
		}

		public void TestKeepUpdatingWithZeroAmountsWhenOneSideOfInvoiceIsPosted()
		{
			TestObjectCreator.SetupRegistrySetJobStatusToInvoicedWhenFirstARInvoicePosted(Guid.Empty, new JobHeaderStatusList());

			SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).DefaultDebtor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				Factory.Save();

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory
				};
				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				Job invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);

				AssertEquals("One charge line should have been created", 1, invoicingJob.Charges.Count);
				AssertEquals("It has not been posted for cost", false, invoicingJob.Charges[0].IsCostPosted);
				AssertEquals("It has been posted for revenue", true, invoicingJob.Charges[0].IsRevenuePosted);
				AssertEquals("INV123", invoicingJob.Charges[0].JR_APInvoiceNum);
				AssertEquals(100m, invoicingJob.Charges[0].JR_LocalCostAmt);
				AssertEquals(100m, invoicingJob.Charges[0].JR_LocalSellAmt);

				chargeProvider.CustomsCharges = Array.Empty<ICustomsCharges>();
				poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));

				AssertEquals("One charge line should have been created", 1, invoicingJob.Charges.Count);
				AssertEquals("It has not been posted for cost", false, invoicingJob.Charges[0].IsCostPosted);
				AssertEquals("It has been posted for revenue", true, invoicingJob.Charges[0].IsRevenuePosted);
				AssertEquals("INV123", invoicingJob.Charges[0].JR_APInvoiceNum);
				AssertEquals(0m, invoicingJob.Charges[0].JR_LocalCostAmt);
				AssertEquals(100m, invoicingJob.Charges[0].JR_LocalSellAmt);
			}
		}

		public void TestPostingAPInvoiceThenARForTwoDataProvidersSharingSameCustomsJob()
		{
			SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).DefaultDebtor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				Factory.Save();

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);
				var cusCharge2 = GetCustomCharge(chargeCode, 22, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory
				};

				var chargeProvider2 = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge2 },
					InvoiceNumber = "INV124",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory
				};
				customDetail.DataProviders.Add(chargeProvider);
				customDetail.DataProviders.Add(chargeProvider2);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				Job invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);

				AssertEquals("Two charge lines should have been created", 2, invoicingJob.Charges.Count);

				AssertEquals("It has not been posted for cost", true, invoicingJob.Charges[0].IsCostPosted);
				AssertEquals("It has not been posted for cost", true, invoicingJob.Charges[1].IsCostPosted);

				AssertEquals("INV123", invoicingJob.Charges[0].JR_APInvoiceNum);
				AssertEquals("INV124", invoicingJob.Charges[1].JR_APInvoiceNum);

				AssertEquals("It has been posted for revenue", false, invoicingJob.Charges[0].IsRevenuePosted);
				AssertEquals("It has been posted for revenue", false, invoicingJob.Charges[1].IsRevenuePosted);

				//Post AR as well
				poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.ARPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);

				AssertEquals("Two charge lines should have been created", 2, invoicingJob.Charges.Count);

				AssertEquals("It has been posted for cost", true, invoicingJob.Charges[0].IsCostPosted);
				AssertEquals("It has been posted for cost", true, invoicingJob.Charges[1].IsCostPosted);

				AssertEquals("It has been posted for revenue", true, invoicingJob.Charges[0].IsRevenuePosted);
				AssertEquals("It has been posted for revenue", true, invoicingJob.Charges[1].IsRevenuePosted);

				AssertEquals("posted into the same AR Invoice", invoicingJob.Charges[0].ARLine.TransactionHeader.PK, invoicingJob.Charges[1].ARLine.TransactionHeader.PK);
			}
		}

		public void TestParentDepartment()
		{
			GlbDepartment department = Factory.New<GlbDepartment>();

			var toLevelObjectWithoutDepartmentSupporter = new Mock<IJobInvoicingSupporter>();
			toLevelObjectWithoutDepartmentSupporter.Setup(m => m.OverriddenDepartmentPK).Returns(ZGuid.Empty);
			toLevelObjectWithoutDepartmentSupporter.Setup(m => m.ConsumerType).Returns(JobInvoicingConsumerTypes.LocalCartage);
			toLevelObjectWithoutDepartmentSupporter.Setup(m => m.TransportMode).Returns((ZString)Constants.TransportModes.Sea);
			toLevelObjectWithoutDepartmentSupporter.Setup(m => m.ContainerMode).Returns((ZString)Constants.ContainerModes.FCL);
			toLevelObjectWithoutDepartmentSupporter.Setup(m => m.IsImport).Returns(true);

			var toLevelObjectWithoutDepartment = new Mock<IJobInvoicingPlugIn>();
			toLevelObjectWithoutDepartment.Setup(m => m.InvoicingSupporter).Returns(toLevelObjectWithoutDepartmentSupporter.Object);

			var parent = new Mock<ICustomsJobInfo>();
			parent.Setup(m => m.TopLevelObjectForJobToReference).Returns(toLevelObjectWithoutDepartment.Object);
			parent.Setup(m => m.InvoicingSupporter).Returns(toLevelObjectWithoutDepartmentSupporter.Object);
			parent.Setup(m => m.Factory).Returns(Factory);

			var poster1 = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, Array.Empty<ZGuid>());
			AssertNull("ParentDepartment", poster1.GetParentDepartment_ForTestOnly(parent.Object));

			var toLevelObjectWithDepartmentSupporter = new Mock<IJobInvoicingSupporter>();
			toLevelObjectWithDepartmentSupporter.Setup(m => m.OverriddenDepartmentPK).Returns(department.PK);
			var toLevelObjectWithDepartment = new Mock<IJobInvoicingPlugIn>();

			toLevelObjectWithDepartment.Setup(m => m.InvoicingSupporter).Returns(toLevelObjectWithDepartmentSupporter.Object);

			parent.Setup(m => m.TopLevelObjectForJobToReference).Returns(toLevelObjectWithDepartment.Object);

			var poster2 = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, Array.Empty<ZGuid>());
			AssertEquals("ParentDepartment", department.PK, poster2.GetParentDepartment_ForTestOnly(parent.Object).PK);
		}

		public void TestGetJobWhereThereIsNoJob()
		{
			var mockPlugIn = Factory.New<MockCustomsJobProvider>();

			var testPoster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, Array.Empty<ZGuid>());

			using (Job result = testPoster.GetOrCreateJob_ForTestOnly(mockPlugIn))
			{
				AssertNotNull(result);
				Assert(result.PK.IsValid);
			}
		}

		public void TestGetJobWhereThereIsNoJobWithIAutoRatingAndJobInvoicing()
		{
			var mockPlugIn = Factory.New<MockCustomsJobProvider>();

			var testPoster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, Array.Empty<ZGuid>());

			using (Job result = testPoster.GetOrCreateJob_ForTestOnly(mockPlugIn))
			{
				AssertNotNull(result);
				Assert(result.PK.IsValid);
			}
		}

		public void TestGetJobWhereThereIsJob()
		{
			var mockPlugIn = Factory.New<MockCustomsJobProvider>();

			Job expectedJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			expectedJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			expectedJob.JH_GB = GlbBranch.CurrentBranch.PK;
			expectedJob.JH_ParentID = mockPlugIn.PK;
			expectedJob.JH_ParentTableCode = DummyBusinessObject.Schema.TablePrefix;
			Factory.Save();

			var testPoster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, Array.Empty<ZGuid>());
			Job result = testPoster.GetOrCreateJob_ForTestOnly(mockPlugIn);

			AssertNotNull(result);
			AssertEquals(expectedJob.PK, result.PK);
		}

		public void TestDontReplaceApportionedCUSDSBCharge()
		{
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, TestObjectCreator.Creditor1.PK.ToGuid());

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = "SEA";
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value;
			cost.E6_OH_Creditor = RatingDataRegistry.Instance.CustomsDisbursementCreditor.Value;
			cost.E6_OSCostAmount = 100m;
			cost.E6_ApportionmentMethod = "SHP";

			GlbGroup group = Factory.LoadTop1<GlbGroup>(new ZQuery());

			foreach (GlbStaff staff in group.Staff)
			{
				staff.GS_EmailAddress = "test@edi.com.au";
			}

			Factory.Save();

			Job expectedJob = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK));
			AssertNotNull(expectedJob);
			AssertEquals("One charge is created when saving", 1, expectedJob.Charges.Count);

			// Prepare data to avoid Validation errors on saving Job before posting
			expectedJob.JH_OA_LocalChargesAddr = TestObjectCreator.Agent.MainAddress.PK;
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

			Factory.Save();

			var customDetail = Factory.New<MockCustomsJobProvider>();
			customDetail.fTopLevelObjectForJobToReference = shipment;

			var charge1 = GetCustomCharge(122, 0, true, TestObjectCreator.Creditor1.PK);
			var chargeProvider = new MockCustomsChargesProvider
			{
				CustomsCharges = new ICustomsCharges[] { charge1 },
				InvoiceNumber = "XXX",
				InvoiceDate = ZDateTime.Now,
				CustomsJob = customDetail
			};
			customDetail.DataProviders.Add(chargeProvider);
			var testPoster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new ZGuid[] { RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value });
			testPoster.RaiseInvoices(customDetail);

			AssertEquals(expectedJob.PK, new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(shipment));
			AssertEquals("Should now be two charges on the job", 2, expectedJob.Charges.Count);
			AssertEquals("Sell should remain the same", 100m, expectedJob.Charges[0].JR_LocalSellAmt);
			AssertEquals("Cost amount should be same as custom charge rated amount", 100m, expectedJob.Charges[0].JR_LocalCostAmt);
			Assert("Charge's cost should be apportioned", expectedJob.Charges[0].JR_E6.IsValid);

			Charge postingCharge = expectedJob.Charges[1];
			AssertEquals("Should have correct sell value", 122m, postingCharge.JR_LocalSellAmt);
			AssertEquals("Should have correct cost value", 122m, postingCharge.JR_LocalCostAmt);
			AssertEquals("Should have correct sell value", 122m, postingCharge.JR_OSSellAmt);
			AssertEquals("Should have correct cost value", 122m, postingCharge.JR_OSCostAmt);

			AssertEquals("AP is posted", true, postingCharge.IsCostPosted);
			AssertEquals("Amount", -122m, postingCharge.APLine.AL_LineAmount);
		}

		public void TestPostInvoiceWhereThereIsJobAndCUSDSBAlreadyPresent()
		{
			var mockPlugIn = Factory.New<MockCustomsJobProvider>();

			Job expectedJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			expectedJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			expectedJob.JH_GB = GlbBranch.CurrentBranch.PK;
			expectedJob.JH_ParentID = mockPlugIn.PK;
			expectedJob.JH_ParentTableCode = DummyBusinessObject.Schema.TablePrefix;
			// Prepare data to avoid Validation errors on saving Job before posting
			expectedJob.JH_OA_LocalChargesAddr = TestObjectCreator.Agent.MainAddress.PK;
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

			Charge cUSDSBCharge = expectedJob.Charges.AddNew();
			cUSDSBCharge.JR_AC = RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value;
			cUSDSBCharge.JR_OH_CostAccount = RatingDataRegistry.Instance.CustomsDisbursementCreditor.Value;
			cUSDSBCharge.JR_LocalCostAmt = 150m;

			Factory.Save();

			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, TestObjectCreator.Creditor1.PK.ToGuid());

			expectedJob.PlugInData = mockPlugIn;

			var testPoster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { cUSDSBCharge.JR_AC });

			var customDetail = Factory.New<MockCustomsJobProvider>();
			var charge1 = GetCustomCharge(122, 0, true, TestObjectCreator.Creditor1.PK);

			var chargeProvider = new MockCustomsChargesProvider
			{
				CustomsCharges = new ICustomsCharges[] { charge1 },
				InvoiceNumber = "XXX",
				InvoiceDate = ZDateTime.Now,
				CustomsJob = mockPlugIn,
				APDueDate = ZDateTime.Today.AddDays(10)
			};
			customDetail.DataProviders.Add(chargeProvider);
			testPoster.RaiseInvoices(customDetail);

			expectedJob.Charges.Load();
			AssertEquals("Should still only be one charge on the job", 1, expectedJob.Charges.Count);
			AssertEquals("Sell should be same as cost side", 122m, expectedJob.Charges[0].JR_LocalSellAmt);
			AssertEquals("Cost amount should be same as custom charge rated amount", 122m, expectedJob.Charges[0].JR_LocalCostAmt);
			Assert("Charge's cost should be posted", expectedJob.Charges[0].IsCostPosted);

			APInvoice cUSDSBInvoice = Factory.Load<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "XXX").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK))[0];
			AssertNotNull("Should have posted invoice", cUSDSBInvoice);
			AssertEquals("AP Due date is set right", ZDateTime.Today.AddDays(10), cUSDSBInvoice.AH_DueDate);
		}

		public void TestPostInvoiceWhereThereIsJobAndCUSDSBAlreadyPresentWithRevenuePosted()
		{
			var mockPlugIn = Factory.New<MockCustomsJobProvider>();
			mockPlugIn.JobNumber = "S00001000";

			SetupStaffMemberEmailAddress();
			var expectedJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			expectedJob.JH_JobNum = "S00001000";
			expectedJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			expectedJob.JH_GB = GlbBranch.CurrentBranch.PK;
			expectedJob.JH_ParentID = mockPlugIn.PK;
			expectedJob.JH_ParentTableCode = DummyBusinessObject.Schema.TablePrefix;
			// Prepare data to avoid Validation errors on saving Job before posting
			expectedJob.JH_OA_LocalChargesAddr = TestObjectCreator.Agent.MainAddress.PK;
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, TestObjectCreator.Creditor1.PK.ToGuid());

			Charge cUSDSBCharge = expectedJob.Charges.AddNew();

			cUSDSBCharge.JR_AC = RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value;
			cUSDSBCharge.JR_OH_CostAccount = RatingDataRegistry.Instance.CustomsDisbursementCreditor.Value;
			cUSDSBCharge.JR_LocalCostAmt = 100m;

			ARInvoice invoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), GlbCompany.CurrentCompany.LocalCurrency, 1m);
			invoice.AH_OH = TestObjectCreator.ABIGAS.PK;

			ARInvoiceLine line = (ARInvoiceLine)TestObjectCreator.CreateInvoiceLine(invoice, 100m, invoice.TransactionCurrency, 1m);
			line.AL_AC = RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value;
			line.AL_JH = expectedJob.PK;
			cUSDSBCharge.JR_AL_ARLine = line.PK;
			cUSDSBCharge.JR_AT_SellGSTRate = line.AL_AT;

			Factory.Save();

			expectedJob.Charges.Load();
			AssertEquals("Should now be one charge on the job", 1, expectedJob.Charges.Count);
			var cahrge = expectedJob.Charges[0];
			AssertEquals("Customs Disbursement Charges", cahrge.JR_Desc);
			Assert(cahrge.IsRevenuePosted);

			expectedJob.PlugInData = mockPlugIn;

			var customDetail = Factory.New<MockCustomsJobProvider>();
			GlbStaff.CurrentUser.GS_EmailAddress = "test@com.com";
			GlbStaff.CurrentUser.Factory.Save();
			mockPlugIn.AutoPostingNotification = new AutoPostingNotification(new[] { GlbStaff.CurrentUser.PK }, false);

			var charge122 = GetCustomCharge(122, 0, true, TestObjectCreator.Creditor1.PK);
			customDetail.fTopLevelObjectForJobToReference = mockPlugIn;
			var chargeProvider = new MockCustomsChargesProvider
			{
				CustomsCharges = new ICustomsCharges[] { charge122 },
				InvoiceNumber = "XXX",
				InvoiceDate = ZDateTime.Now,
				CustomsJob = mockPlugIn,
				Factory = Factory,
				AutoPostingNotification = new AutoPostingNotification(new[] { GlbStaff.CurrentUser.PK }, false)
			};
			customDetail.DataProviders.Add(chargeProvider);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var testPoster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.SendEmail, new[] { cUSDSBCharge.JR_AC });
			testPoster.RaiseInvoices(customDetail);

			expectedJob.Charges.Load();
			CombineAssertions(() =>
			{
				AssertEquals("Should now be two charge on the job", 2, expectedJob.Charges.Count);
				var charge1 = expectedJob.Charges[0];
				var charge2 = expectedJob.Charges[1];
				AssertEquals("Customs Disbursement Charges", charge1.JR_Desc);
				AssertEquals("XXX", charge1.JR_APInvoiceNum);
				AssertEquals("Sell should remain the same", 100m, charge1.JR_LocalSellAmt);
				AssertEquals("Cost amount should be same as custom charge rated amount", 100m, charge1.JR_LocalCostAmt);
				Assert("Charge's cost should be posted", charge1.IsCostPosted);
				Assert(charge1.IsRevenuePosted);
				AssertEquals("Invoice should be for correct amount", -100m, charge1.APLine.AL_LineAmount);
				AssertEquals("Invoice should be for correct amount", -100m, charge1.APLine.TransactionHeader.AH_InvoiceAmount);

				AssertEquals("Customs Disbursement Charges\r\nCurrent Amounts\r\n  TEST                                      122.00", charge2.JR_Desc);
				AssertEquals("XXX/CUSDSB/1", charge2.JR_APInvoiceNum);
				AssertEquals(22m, charge2.JR_LocalSellAmt);
				AssertEquals(22m, charge2.JR_LocalCostAmt);
				Assert(charge2.IsCostPosted);
				Assert(!charge2.IsRevenuePosted);
				AssertEquals(-22m, charge2.APLine.AL_LineAmount);
				AssertEquals(-22m, charge2.APLine.TransactionHeader.AH_InvoiceAmount);

				AssertEquals("Email should have been sent for AR discrepancy", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			});
		}

		public void TestDontPostAdditionalWhenPayablesCUSDSBAlreadyPosted()
		{
			var mockPlugIn = Factory.New<MockCustomsJobProvider>();
			mockPlugIn.JobNumber = "S00001000";

			var expectedJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			expectedJob.JH_JobNum = "S00001000";
			expectedJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			expectedJob.JH_GB = GlbBranch.CurrentBranch.PK;
			expectedJob.JH_ParentID = mockPlugIn.PK;
			expectedJob.JH_ParentTableCode = DummyBusinessObject.Schema.TablePrefix;
			expectedJob.JH_OA_LocalChargesAddr = TestObjectCreator.Agent.MainAddress.PK;
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

			Factory.Save();

			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, TestObjectCreator.Creditor1.PK.ToGuid());

			Charge cUSDSBCharge = expectedJob.Charges.AddNew();

			cUSDSBCharge.JR_AC = RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value;
			cUSDSBCharge.JR_OH_CostAccount = RatingDataRegistry.Instance.CustomsDisbursementCreditor.Value;
			cUSDSBCharge.JR_LocalCostAmt = 100m;

			GlbStaff.CurrentUser.GS_EmailAddress = "Test@com.com";
			GlbStaff.CurrentUser.Factory.Save();
			Factory.Save();

			APInvoice invoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), GlbCompany.CurrentCompany.LocalCurrency, 1m);
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;
			invoice.AH_TransactionNum = "XXX";
			APInvoiceLine line = (APInvoiceLine)TestObjectCreator.CreateInvoiceLine(invoice, 100m, invoice.TransactionCurrency, 1m);
			cUSDSBCharge.ReverseAccrual(ZDateTime.Now);
			cUSDSBCharge.JR_AL_APLine = line.PK;
			line.AL_AC = cUSDSBCharge.JR_AC;
			line.AL_JH = expectedJob.PK;

			AssertEquals("CostIsPosted", true, cUSDSBCharge.IsCostPosted);
			Factory.Save();

			var customDetail = Factory.New<MockCustomsJobProvider>();
			customDetail.fTopLevelObjectForJobToReference = mockPlugIn;
			customDetail.AutoPostingNotification = new AutoPostingNotification(new[] { GlbStaff.CurrentUser.PK }, false);
			var charge1 = GetCustomCharge(122, 0, true, TestObjectCreator.Creditor1.PK);

			var chargeProvider = new MockCustomsChargesProvider
			{
				CustomsCharges = new ICustomsCharges[] { charge1 },
				InvoiceNumber = "XXX",
				InvoiceDate = ZDateTime.Now,
				CustomsJob = customDetail,
				Factory = Factory,
				AutoPostingNotification = new AutoPostingNotification(new[] { GlbStaff.CurrentUser.PK }, false)
			};
			customDetail.DataProviders.Add(chargeProvider);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var testPoster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.SendEmail, new[] { cUSDSBCharge.JR_AC });
			testPoster.RaiseInvoices(customDetail);

			CombineAssertions(() =>
			{
				AssertEquals("Should be no change to job - should still be only 1 charge", 2, expectedJob.Charges.Count);
				var aPPostedCharge = expectedJob.Charges[0];
				AssertEquals(line.PK, aPPostedCharge.JR_AL_APLine);
				AssertEquals("Should have local cost remain the same", 100m, aPPostedCharge.JR_LocalCostAmt);
				AssertEquals("Should have OS cost remain the same", 100m, aPPostedCharge.JR_OSCostAmt);
				AssertEquals("Should have local sell remain the same", 100m, aPPostedCharge.JR_LocalSellAmt);
				AssertEquals("Should have os sell remain the same", 100m, aPPostedCharge.JR_OSSellAmt);

				var charge2 = expectedJob.Charges[1];
				AssertNotEquals(line.PK, charge2.JR_AL_APLine);
				AssertEquals(22m, charge2.JR_LocalCostAmt);
				AssertEquals(22m, charge2.JR_OSCostAmt);
				AssertEquals(22m, charge2.JR_LocalSellAmt);
				AssertEquals(22m, charge2.JR_OSSellAmt);

				AssertEquals("Should have sent one email as it is trying to auto rate with a different amount", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			});
		}

		public void TestGetCustomsJob()
		{
			var creditor = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var customDetail = Factory.New<MockCustomsJobProvider>();
			var charge1 = GetCustomCharge(122, 0, true, creditor.PK);

			var testPoster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, Array.Empty<ZGuid>());

			ICustomsCharges[] charges = { charge1 };
			new CustomsChargesManager(customDetail).RateCustomsCharges(charges);

			using (Job result = testPoster.GetOrCreateCustomsJob_ForTestOnly(customDetail))
			{
				AssertNotNull(result);
				AssertEquals(0, result.Charges.Count);
			}
		}

		public void TestAPInvoiceDetailHasBeenSet()
		{
			SetupStaffMemberEmailAddress();
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CC3.PK.ToGuid());
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Creditor2.PK.ToGuid());

			var customDetail = Factory.New<MockCustomsJobProvider>();
			customDetail.JobNumber = "TR436877";

			Job testJob = new Job.Loader(customDetail).TryCreateWithoutMutexForTestOnly();
			testJob.LocalChargesPK = LocalClient.PK;
			testJob.AgentCollectPK = Agent.PK;
			testJob.PlugInData = customDetail;

			CreateCharge(testJob, CC1, "TESTCharge1", AUD, 155m, Creditor1, AUD, 155m, LocalClient);
			CreateCharge(testJob, CC2, "TESTCharge2", AUD, 55m, Creditor1, AUD, 55m, LocalClient);
			Charge addedDSBCharge = CreateCharge(testJob, CC3, "TESTCharge2", AUD, 105m, Creditor2, AUD, 105m, LocalClient);

			var charge = GetCustomCharge(CC3, 122, 0, true, Creditor2.PK);

			var chargeProvider = new MockCustomsChargesProvider
			{
				CustomsCharges = new ICustomsCharges[] { charge },
				InvoiceNumber = "TEST4397",
				InvoiceDate = new ZDateTime(2005, 4, 12),
				CustomsJob = customDetail
			};
			customDetail.DataProviders.Add(chargeProvider);
			var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { CC3.PK });
			poster.RaiseInvoices(customDetail);

			AssertEquals(3, testJob.Charges.Count);

			AssertEquals("", testJob.Charges[0].JR_APInvoiceNum);
			AssertEquals(ZDateTime.Empty, testJob.Charges[0].JR_APInvoiceDate);

			AssertEquals("", testJob.Charges[1].JR_APInvoiceNum);
			AssertEquals(ZDateTime.Empty, testJob.Charges[1].JR_APInvoiceDate);

			AssertEquals("TEST4397", testJob.Charges[2].JR_APInvoiceNum);
			AssertEquals(new ZDateTime(2005, 4, 12), testJob.Charges[2].JR_APInvoiceDate);
			AssertEquals(addedDSBCharge.PK, testJob.Charges[2].PK);
		}

		public void TestCustomsJobHasDepartment()
		{
			var creditor = Factory.LoadTop1<OrgHeader>(new ZQuery());

			var customDetail = Factory.New<MockCustomsJobProvider>();
			var charge1 = GetCustomCharge(122, 0, true, creditor.PK);

			var chargePosterCreator = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, Array.Empty<ZGuid>());
			ICustomsCharges[] charges = { charge1 };
			new CustomsChargesManager(customDetail).RateCustomsCharges(charges);

			using (Job result = chargePosterCreator.GetOrCreateCustomsJob_ForTestOnly(customDetail))
			{
				Assert(!result.JH_GE.IsEmpty);
			}
		}

		public void TestCustomJobBranch()
		{
			var creditor = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var customDetail = Factory.New<MockCustomsJobProvider>();

			Assert(NonCurrentBranch.PK != GlbBranch.CurrentBranch.PK);
			customDetail.fBranch = NonCurrentBranch;
			var charge1 = GetCustomCharge(122, 0, true, creditor.PK);

			var chargePosterCreator = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, Array.Empty<ZGuid>());
			ICustomsCharges[] charges = { charge1 };
			new CustomsChargesManager(customDetail).RateCustomsCharges(charges);

			using (Job result = chargePosterCreator.GetOrCreateCustomsJob_ForTestOnly(customDetail))
			{
				AssertEquals(result.Branch.PK, NonCurrentBranch.PK);
			}
		}

		public void TestRaiseInvoice()
		{
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CC1.PK.ToGuid());

			var customDetail = Factory.New<MockCustomsJobProvider>();
			customDetail.JobNumber = "kje789";

			var testPoster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { CC1.PK });

			Job testJob = CreateJob("S001", LocalClient, 0m, Agent, 0m);
			testJob.PlugInData = customDetail;
			Charge addedCharge = CreateCharge(testJob, CC1, "TESTCharge", AUD, 155m, Creditor1, AUD, 155m, LocalClient);

			addedCharge.JR_APInvoiceNum = "R49837";
			addedCharge.JR_APInvoiceDate = ZDateTime.Now;
			addedCharge.PostAPWhenInvokedByCustomInvoiceCreator = true;

			Factory.Save();

			Assert(testPoster.PostInvoice_ForTestOnly(testJob, JobInvoicingPostingOption.CustomsDSBChargeAPOnly));

			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, addedCharge.JR_APInvoiceNum);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
			APInvoice actualInvoice = Factory.LoadTop1(typeof(APInvoice), filter) as APInvoice;

			AssertNotNull(actualInvoice);
		}

		public void TestDoNotRaiseInvoiceForExistingCharge()
		{
			var creditor = Factory.LoadTop1<OrgHeader>(new ZQuery());
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CC1.PK.ToGuid());

			var customDetail = Factory.New<MockCustomsJobProvider>();
			var charge1 = GetCustomCharge(122, 0, true, creditor.PK);
			customDetail.JobNumber = "TR436877";

			var chargeProvider = new MockCustomsChargesProvider
			{
				CustomsCharges = new ICustomsCharges[] { charge1 },
				InvoiceNumber = "XXX",
				InvoiceDate = ZDateTime.Now,
				CustomsJob = customDetail
			};

			var testPoster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { CC1.PK });

			Job testJob = CreateJob("S001", LocalClient, 0m, Agent, 0m);
			Charge addedCharge = CreateCharge(testJob, CC1, "TESTCharge", AUD, 155m, Creditor1, AUD, 155m, LocalClient);

			addedCharge.JR_APInvoiceNum = "R49837";
			addedCharge.JR_APInvoiceDate = ZDateTime.Now;

			Factory.Save();

			testPoster.RaiseInvoices(customDetail);

			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, addedCharge.JR_APInvoiceNum);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			APInvoice actualInvoice = Factory.LoadTop1(typeof(APInvoice), filter) as APInvoice;

			AssertNull(actualInvoice);
		}

		public void TestDoNotRaiseInvoiceWhenChargePosterBehavioursWasSetNotTo()
		{
			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;
			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				var charge1 = GetCustomCharge(122, 0, true, testCreditor.PK);
				customDetail.JobNumber = "TR436877";

				// Prepare data to avoid Validation errors on saving Job before posting
				Job expectedJob = Factory.NewJobWithValidTestDataForTesting<Job>();
				expectedJob.JH_ParentID = customDetail.PK;
				expectedJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				expectedJob.JH_OA_LocalChargesAddr = TestObjectCreator.Agent.MainAddress.PK;
				expectedJob.JH_JobNum = "TR436877";
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { charge1 },
					InvoiceNumber = "XXX",
					InvoiceDate = ZDateTime.Now,
					CustomsJob = customDetail
				};
				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var testPoster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB, new[] { chargeCode.PK });
				testPoster.RaiseInvoices(customDetail);

				AssertEquals("PreCondition:No errors expected", 0, Env.OutgoingMailManager.EmailsCreated.Count);

				Job createdJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("job is created", createdJob);
				AssertEquals("One charge", 1, createdJob.Charges.Count);
				AssertEquals("Cost is not posted", false, createdJob.Charges[0].IsCostPosted);
				AssertEquals("Revenue is not posted", false, createdJob.Charges[0].IsRevenuePosted);
			}
		}

		public void TestCustomsChargesWorksWithChargePoster()
		{
			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";

				// Prepare data to avoid Validation errors on saving Job before posting
				Job expectedJob = Factory.NewJobWithValidTestDataForTesting<Job>();
				expectedJob.JH_ParentID = customDetail.PK;
				expectedJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				expectedJob.JH_OA_LocalChargesAddr = TestObjectCreator.Agent.MainAddress.PK;
				expectedJob.JH_JobNum = "TR436877";
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

				expectedJob.PlugInData = customDetail;

				var charge1 = GetCustomCharge(122, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { charge1 },
					InvoiceNumber = "TR436877",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail
				};
				customDetail.DataProviders.Add(chargeProvider);
				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				Factory.Save();

				ZQuery filter = GetInvoiceFilter();
				APInvoice actualInvoice = Factory.LoadTop1(typeof(APInvoice), filter) as APInvoice;

				AssertNotNull(actualInvoice);
				Assert(actualInvoice.PK.IsValid);
			}
		}

		public void TestCustomsChargeWorksWithTwoCustomsCharge()
		{
			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";

				// Prepare data to avoid Validation errors on saving Job before posting
				Job expectedJob = Factory.NewJobWithValidTestDataForTesting<Job>();
				expectedJob.JH_ParentID = customDetail.PK;
				expectedJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				expectedJob.JH_OA_LocalChargesAddr = TestObjectCreator.Agent.MainAddress.PK;
				expectedJob.JH_JobNum = "TR436877";
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

				expectedJob.PlugInData = customDetail;

				var charge1 = GetCustomCharge(122, 0, true, testCreditor.PK);
				var charge2 = GetCustomCharge(132, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { charge1, charge2 },
					InvoiceNumber = "TR436877",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail
				};
				customDetail.DataProviders.Add(chargeProvider);
				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				Factory.Save();

				ZQuery filter = new ZQuery();
				filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

				APInvoice[] actualInvoice = Factory.Load(typeof(APInvoice), filter) as APInvoice[];

				AssertNotNull(actualInvoice);
				AssertEquals(1, actualInvoice.Length);
				AssertEquals(-254m, actualInvoice[0].AH_InvoiceAmount);
				AssertEquals(1, actualInvoice[0].Lines.Count);
				AssertEquals(-254m, actualInvoice[0].Lines[0].AL_LineAmount);
				AssertEquals(chargeCode.PK, actualInvoice[0].Lines[0].AL_AC);
			}
		}

		public void TestCustomsChargesUpdateUsingAdditionalMatchingCondition()
		{
			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();

				// Prepare data to avoid Validation errors on saving Job before posting
				Job expectedJob = Factory.NewJobWithValidTestDataForTesting<Job>();
				expectedJob.JH_ParentID = customDetail.PK;
				expectedJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				expectedJob.JH_OA_LocalChargesAddr = TestObjectCreator.Agent.MainAddress.PK;
				expectedJob.JH_JobNum = "TR436877";
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

				expectedJob.PlugInData = customDetail;

				var charge1 = GetCustomCharge(122, 0, true, testCreditor.PK);
				customDetail.JobNumber = "TR436877";

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { charge1 },
					InvoiceNumber = "TR436877",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail
				};
				customDetail.DataProviders.Add(chargeProvider);

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				Job invoicingJob = Factory.LoadTop1(typeof(Job), new ZQuery(JobHeaderSchema.JH_JobNum, customDetail.JobNumber)) as Job;
				CombineAssertions(() =>
				{
					AssertEquals("One line", 1, invoicingJob.Charges.Count);
					AssertEquals("Cost is posted", false, invoicingJob.Charges[0].IsCostPosted);
					AssertEquals("No revenue is posted", false, invoicingJob.Charges[0].IsRevenuePosted);
					AssertEquals("AP line Amount", 122m, invoicingJob.Charges[0].APLine.AL_LineAmount);
					AssertEquals("AP Invoice Num", "TR436877", invoicingJob.Charges[0].JR_APInvoiceNum);
				});

				var charge2 = GetCustomCharge(200, 0, true, testCreditor.PK);
				chargeProvider.InvoiceNumber = "PR436877";
				chargeProvider.CustomsCharges = new ICustomsCharges[] { charge2 };
				var secondPoster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB, new[] { chargeCode.PK });
				secondPoster.RaiseInvoices(customDetail);
				CombineAssertions(() =>
				{
					AssertEquals("two line", 2, invoicingJob.Charges.Count);
					AssertNotNull(invoicingJob.Charges.OfType<Charge>().FirstOrDefault((x) => x.JR_APInvoiceNum == "PR436877" && x.APLine.AL_LineAmount == 200m));
					AssertNotNull(invoicingJob.Charges.OfType<Charge>().FirstOrDefault((x) => x.JR_APInvoiceNum == "TR436877" && x.APLine.AL_LineAmount == 122m));
				});

				var charge3 = GetCustomCharge(200, 0, true, testCreditor.PK);
				chargeProvider.InvoiceNumber = "CR436877";
				chargeProvider.PreviousInvoiceNumber = "TR";
				chargeProvider.CustomsCharges = new ICustomsCharges[] { charge3 };
				var thirsPoster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB, new[] { chargeCode.PK });
				thirsPoster.RaiseInvoices(customDetail);
				CombineAssertions(() =>
				{
					AssertEquals("two line", 2, invoicingJob.Charges.Count);
					AssertNotNull(invoicingJob.Charges.OfType<Charge>().FirstOrDefault((x) => x.JR_APInvoiceNum == "PR436877" && x.APLine.AL_LineAmount == 200m));
					AssertNotNull(invoicingJob.Charges.OfType<Charge>().FirstOrDefault((x) => x.JR_APInvoiceNum == "CR436877/CUSDSB" && x.APLine.AL_LineAmount == 200m));
				});
			}
		}

		public void TestCustomsChargesRunningTwice()
		{
			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();

				// Prepare data to avoid Validation errors on saving Job before posting
				Job expectedJob = Factory.NewJobWithValidTestDataForTesting<Job>();
				expectedJob.JH_ParentID = customDetail.PK;
				expectedJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				expectedJob.JH_OA_LocalChargesAddr = TestObjectCreator.Agent.MainAddress.PK;
				expectedJob.JH_JobNum = "TR436877";
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

				expectedJob.PlugInData = customDetail;

				var charge1 = GetCustomCharge(122, 0, true, testCreditor.PK);
				customDetail.JobNumber = "TR436877";

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { charge1 },
					InvoiceNumber = "TR436877",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail
				};
				customDetail.DataProviders.Add(chargeProvider);
				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				Job invoicingJob = Factory.LoadTop1(typeof(Job), new ZQuery(JobHeaderSchema.JH_JobNum, customDetail.JobNumber)) as Job;
				AssertNotNull(invoicingJob);
				AssertEquals("One line is added", 1, invoicingJob.Charges.Count);
				AssertEquals("No cost or revenue is posted", false, invoicingJob.Charges[0].IsCostPosted);
				AssertEquals("No cost or revenue is posted", false, invoicingJob.Charges[0].IsRevenuePosted);
				AssertEquals("Amount", 122m, invoicingJob.Charges[0].JR_LocalSellAmt);

				var charge2 = GetCustomCharge(200, 0, true, testCreditor.PK);
				chargeProvider.CustomsCharges = new ICustomsCharges[] { charge2 };
				var secondPoster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				secondPoster.RaiseInvoices(customDetail);

				AssertNotNull(invoicingJob);
				AssertEquals("One line", 1, invoicingJob.Charges.Count);
				AssertEquals("Cost is posted", true, invoicingJob.Charges[0].IsCostPosted);
				AssertEquals("No revenue is posted", false, invoicingJob.Charges[0].IsRevenuePosted);
				AssertEquals("AP line Amount", -200m, invoicingJob.Charges[0].APLine.AL_LineAmount);
			}
		}

		public void TestClearUnpostedChargeWhenWithdrawn()
		{
			SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).DefaultDebtor = Factory.LoadTop1<OrgHeader>(new ZQuery());

				Factory.Save();

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges =
							new ICustomsCharges[] { GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK) },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory
				};
				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				Job invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);

				AssertEquals("One charge line should have been created", 1, invoicingJob.Charges.Count);
				AssertEquals("It has not been posted for cost", false, invoicingJob.Charges[0].IsCostPosted);
				AssertEquals("It has been posted for revenue", true, invoicingJob.Charges[0].IsRevenuePosted);

				Charge unpostedCharge = invoicingJob.Charges.AddNew();
				unpostedCharge.JR_AC = chargeCode.PK;
				unpostedCharge.JR_OSCostAmt = 100m;
				unpostedCharge.JR_OSSellAmt = 100m;
				chargeProvider.HasBeenWithdrawn = true;
				chargeProvider.DisbursementChargeCodes = new[] { chargeCode.PK };
				chargeProvider.CustomsCharges = Array.Empty<ICustomsCharges>();//CusEntryHeader returns an empty array when withdrawn
				poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				AssertEquals(2, invoicingJob.Charges.Count);
				unpostedCharge.Reload();
				AssertEquals("Unposted disbursement charge should have zero amounts", 0m, unpostedCharge.JR_OSSellAmt);
				AssertEquals("Unposted disbursement charge should have zero amounts", 0m, unpostedCharge.JR_OSCostAmt);
			}
		}

		public void TestRaiseAPInvoiceForNewCustomChargeOnly()
		{
			ZGuid staff = SetupStaffMemberEmailAddress();
			RefCurrency localCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			using (SetCustomsDisbursementDetailsInRegistry(Creditor1, CC1, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "OIa897";
				customDetail.AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false);
				Job testJob = CreateJob("S001", LocalClient, 0m, Agent, 0m);
				testJob.JH_ParentID = customDetail.PK;
				testJob.PlugInData = customDetail;
				// Prepare data to avoid Validation errors on saving Job before posting
				var exRate = testJob.ExchangeRates.AddNew();
				exRate.JF_RX_NKRateCurrency = TestObjectCreator.AUD.RX_Code;
				exRate.JF_CFXPercent = exRate.JF_BaseRate = 1m;

				Factory.Save();

				Charge addedCharge = CreateCharge(testJob, CC1, "TESTCharge", localCurrency, 155m, Creditor1, localCurrency, 155m, LocalClient);
				addedCharge.JR_LocalCostAmt = 155m;
				addedCharge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				addedCharge.JR_OSCostExRate = 1m;
				addedCharge.JR_LocalCostAmt = 155m;
				addedCharge.JR_APInvoiceNum = "R49837";
				addedCharge.JR_APInvoiceDate = ZDateTime.Now;
				addedCharge.JR_OH_CostAccount = Creditor1.PK;
				addedCharge.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;
				addedCharge.JR_OSSellExRate = 1m;
				Factory.Save();

				var testPoster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { CC1.PK });
				Job result = testPoster.GetOrCreateJob_ForTestOnly(customDetail);
				AssertNotNull(result);

				var charge1 = GetCustomCharge(122, 0, true, Creditor1.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { charge1 },
					InvoiceNumber = "TR436877",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory,
					AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false)
				};
				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.SendEmail, new[] { CC1.PK });
				poster.RaiseInvoices(customDetail);

				Factory.Save();

				ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "R49837");
				filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
				APInvoice invoice = Factory.LoadTop1(typeof(APInvoice), filter) as APInvoice;

				AssertNull(invoice);

				filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "TR436877");
				filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
				APInvoice actualInvoice = Factory.LoadTop1(typeof(APInvoice), filter) as APInvoice;

				AssertNotNull(actualInvoice);
				AssertEquals("TR436877", actualInvoice.AH_TransactionNum);
			}
		}

		public void TestInformationOnlyChargeShowsEntryNumber()
		{
			var informationOnlyCharge = Factory.NewWithValidTestData<AccChargeCode>();
			informationOnlyCharge.AC_ChargeType = Core.Constants.ChargeType.Comment;
			informationOnlyCharge.AC_Code = "FORINFO";
			informationOnlyCharge.AC_Desc = "Customs Deferred Charge (For information only)";
			SetCustomsDeferredChargeInRegistry(informationOnlyCharge.PK.ToGuid(), true);

			var disbursementCharge = Factory.NewWithValidTestData<AccChargeCode>();
			disbursementCharge.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			disbursementCharge.AC_Code = "DSBCHG";
			disbursementCharge.AC_Desc = "Customs Disbursement Charges";
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, disbursementCharge.PK.ToGuid());

			var customDetail = Factory.New<MockCustomsJobProvider>();
			customDetail.JobNumber = "TR436877";
			var expectedJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			expectedJob.JH_ParentID = customDetail.PK;
			expectedJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			expectedJob.JH_JobNum = "TR436877";
			var chargeNotPaidByBroker = GetCustomCharge(CC1, 100, 0, false, Creditor2.PK, "ENT1");
			var chargePaidByBroker = GetCustomCharge(disbursementCharge, 200, 0, true, Creditor2.PK, "ENT2");
			var chargeProvider = new MockCustomsChargesProvider
			{
				CustomsCharges = new ICustomsCharges[] { chargeNotPaidByBroker, chargePaidByBroker },
				CustomsJob = customDetail
			};
			customDetail.DataProviders.Add(chargeProvider);
			Factory.Save();

			var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.ARPostDSB, new[] { CC3.PK });
			poster.RaiseInvoices(customDetail);

			var invoicingJob = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.PK, expectedJob.PK));
			AssertNotNull(invoicingJob);
			AssertEquals("Two charge lines are added", 2, invoicingJob.Charges.Count);

			var invoiceJobCharges = invoicingJob.Charges.Cast<Charge>();
			AssertContains("Customs Deferred Charge (For information only) - ENT1", invoiceJobCharges.Single(x => x.ChargeCode == informationOnlyCharge).JR_Desc);
			AssertContains("Customs Disbursement Charges - ENT2", invoiceJobCharges.Single(x => x.ChargeCode == disbursementCharge).JR_Desc);

			chargeNotPaidByBroker = GetCustomCharge(CC1, 120, 0, false, Creditor2.PK, "ENT1");
			chargePaidByBroker = GetCustomCharge(disbursementCharge, 220, 0, true, Creditor2.PK, "ENT2");
			chargeProvider.CustomsCharges = new ICustomsCharges[] { chargeNotPaidByBroker, chargePaidByBroker };
			poster.RaiseInvoices(customDetail);
			Factory.Save();

			AssertEquals("Two charge lines are added", 2, invoicingJob.Charges.Count);
			AssertContains("Customs Deferred Charge (For information only) - ENT1", invoiceJobCharges.Single(x => x.ChargeCode == informationOnlyCharge).JR_Desc);
			AssertContains("Customs Disbursement Charges - ENT2", invoiceJobCharges.Single(x => x.ChargeCode == disbursementCharge).JR_Desc);
		}

		public void TestInformationOnlyChargeLineWhenImporterPays()
		{
			SetupStaffMemberEmailAddress();
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CC3.PK.ToGuid());
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Creditor2.PK.ToGuid());

			var informationOnlyCharge = Factory.NewWithValidTestData<AccChargeCode>();
			informationOnlyCharge.AC_ChargeType = Core.Constants.ChargeType.Comment;

			SetCustomsDeferredChargeInRegistry(informationOnlyCharge.PK.ToGuid(), true);

			var customDetail = Factory.New<MockCustomsJobProvider>();
			customDetail.JobNumber = "TR436877";

			// Prepare data to avoid Validation errors on saving Job before posting
			Job expectedJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			expectedJob.JH_ParentID = customDetail.PK;
			expectedJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			expectedJob.JH_OA_LocalChargesAddr = TestObjectCreator.Agent.MainAddress.PK;
			expectedJob.JH_JobNum = "TR436877";
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

			var charge = GetCustomCharge(CC1, 100, 0, false, Creditor2.PK);//not paid by broker

			var chargeProvider = new MockCustomsChargesProvider
			{
				CustomsCharges = new ICustomsCharges[] { charge },
				InvoiceNumber = "TEST4397",
				InvoiceDate = new ZDateTime(2005, 4, 12),
				CustomsJob = customDetail
			};
			customDetail.DataProviders.Add(chargeProvider);
			Factory.Save();

			var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.ARPostDSB, new[] { CC3.PK });
			poster.RaiseInvoices(customDetail);

			Job invoicingJob = Factory.LoadTop1(typeof(Job), new ZQuery(JobHeaderSchema.JH_JobNum, customDetail.JobNumber)) as Job;
			AssertNotNull(invoicingJob);
			AssertEquals("One line is added", 1, invoicingJob.Charges.Count);
			AssertEquals("No cost or revenue is posted", false, invoicingJob.Charges[0].IsCostPosted);
			AssertEquals("No cost or revenue is posted", false, invoicingJob.Charges[0].IsRevenuePosted);

			//However AP invoice number should be set to an entry number. When there are many entries, system needs to know which charge line corresponds to which entry and Charge.JR_APInvoiceNum serves as a reference
			AssertEquals("TEST4397", invoicingJob.Charges[0].JR_APInvoiceNum);
			AssertContains("Description should contain breakdown of charges", "TEST", invoicingJob.Charges[0].JR_Desc);
			AssertContains("Description should contain breakdown of charges", "100", invoicingJob.Charges[0].JR_Desc);
			AssertEquals("No Cost or Revenue details", 0m, invoicingJob.Charges[0].JR_LocalCostAmt);
			AssertEquals("No Cost or Revenue details", 0m, invoicingJob.Charges[0].JR_LocalSellAmt);
			AssertEquals(Constants.ChargeType.Comment, invoicingJob.Charges[0].ChargeType);

			invoicingJob.Charges[0].RunPreSaveValidation();
			AssertNoErrors(invoicingJob.Charges[0].JR_APInvoiceNumInfo);
		}

		public void TestAPInvoiceDetailWithLongDescription()
		{
			SetupStaffMemberEmailAddress();
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CC3.PK.ToGuid());
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Creditor2.PK.ToGuid());

			var customDetail = Factory.New<MockCustomsJobProvider>();
			customDetail.JobNumber = "TR436877";

			Job testJob = new Job.Loader(customDetail).TryCreateWithoutMutexForTestOnly();
			testJob.LocalChargesPK = LocalClient.PK;
			testJob.AgentCollectPK = Agent.PK;
			testJob.PlugInData = customDetail;

			Charge addedDSBCharge = CreateCharge(testJob, CC3, "TESTCharge2", AUD, 105m, Creditor2, AUD, 105m, LocalClient);

			var charges = new List<MockCustomCharge>();
			for (int i = 0; i < 1000; i++)
			{
				var charge = GetCustomCharge(CC3, 122, 0, true, Creditor2.PK);
				charges.Add(charge);
			}

			var chargeProvider = new MockCustomsChargesProvider
			{
				CustomsCharges = charges.ToArray<ICustomsCharges>(),
				InvoiceNumber = "TEST4397",
				InvoiceDate = new ZDateTime(2005, 4, 12),
				CustomsJob = customDetail
			};
			customDetail.DataProviders.Add(chargeProvider);
			var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { CC3.PK });
			poster.RaiseInvoices(customDetail);

			AssertEquals(AutoJobCharge.Schema.JR_DescMaxLength, testJob.Charges[0].JR_DescInfo.MaxLength);
			AssertEquals(1, testJob.Charges.Count);
			AssertEquals("Truncated to 1024 characters, but any trailing whitespace is removed too, so this is a 1009 character string.", @"Charge Code 3
Current Amounts
  TEST                                      122.00
  TEST                                      122.00
  TEST                                      122.00
  TEST                                      122.00
  TEST                                      122.00
  TEST                                      122.00
  TEST                                      122.00
  TEST                                      122.00
  TEST                                      122.00
  TEST                                      122.00
  TEST                                      122.00
  TEST                                      122.00
  TEST                                      122.00
  TEST                                      122.00
  TEST                                      122.00
  TEST                                      122.00
  TEST                                      122.00
  TEST                                      122.00
  TEST                                      122.00
  TE", testJob.Charges[0].JR_Desc);
		}

		public void TestErrorHandlingAndNotCreatingInvoiceJobIfDepartmentDefaultCannotDetermined()
		{
			var creditor = Factory.LoadTop1<OrgHeader>(new ZQuery());

			var customDetail = Factory.New<MockCustomsJobProvider>();
			customDetail.InvDataProviderCandidates.OfType<MockCustomsChargesProvider>().FirstOrDefault().HasBeenWithdrawn = false;
			customDetail.JobNumber = "TR436877";
			((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).TransportMode = "";

			var charge1 = GetCustomCharge(122, 0, true, creditor.PK);
			var chargeProvider = customDetail.InvDataProviderCandidates.OfType<MockCustomsChargesProvider>().FirstOrDefault();
			chargeProvider.HasBeenWithdrawn = false;
			chargeProvider.CustomsCharges = new ICustomsCharges[] { charge1 };
			chargeProvider.InvoiceNumber = "TR436877";
			chargeProvider.InvoiceDate = new ZDateTime(2005, 4, 12);
			chargeProvider.CustomsJob = customDetail;

			AssertNull(new Job.Loader(customDetail.TopLevelObjectForJobToReference).Load());

			var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, Array.Empty<ZGuid>());
			try
			{
				poster.RaiseInvoices(customDetail);
			}
			catch (Exception e)
			{
				AssertEquals("Incorrect Type", typeof(CustomsInvoiceRaiseException), e.GetType());
				AssertEquals("Message", "Failed to load/create Invoicing Job, please manually create a valid Invoicing Job in 'Billing' tab.", e.Message);
			}
			finally
			{
				AssertNull("Still no Invoicing Job been created", new Job.Loader(customDetail.TopLevelObjectForJobToReference).Load());
			}
		}

		public void TestErrorHandlingAndNotCreatingInvoiceJobIfBranchDefaultCannotDetermined()
		{
			var creditor = Factory.LoadTop1<OrgHeader>(new ZQuery());

			var customDetail = Factory.New<MockCustomsJobProvider>();
			customDetail.fBranch = null;
			customDetail.JobNumber = "TR436877";
			((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).TransportMode = "OTH";

			var charge1 = GetCustomCharge(122, 0, true, creditor.PK);
			var chargeProvider = customDetail.InvDataProviderCandidates.OfType<MockCustomsChargesProvider>().FirstOrDefault();
			chargeProvider.HasBeenWithdrawn = false;
			chargeProvider.CustomsCharges = new ICustomsCharges[] { charge1 };
			chargeProvider.InvoiceNumber = "TR436877";
			chargeProvider.InvoiceDate = new ZDateTime(2005, 4, 12);
			chargeProvider.CustomsJob = customDetail;

			AssertNull(new Job.Loader(customDetail.TopLevelObjectForJobToReference).Load());

			var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, Array.Empty<ZGuid>());
			try
			{
				poster.RaiseInvoices(customDetail);
			}
			catch (Exception e)
			{
				AssertEquals("Incorrect Type", typeof(CustomsInvoiceRaiseException), e.GetType());
				AssertEquals("Message", "Failed to load/create Invoicing Job, please manually create a valid Invoicing Job in 'Billing' tab.", e.Message);
			}
			finally
			{
				AssertNull("Still no Invoicing Job been created", new Job.Loader(customDetail.TopLevelObjectForJobToReference).Load());
			}
		}

		void SetCustomsDeferredChargeInRegistry(Guid chargeCode, bool enabled)
		{
			RatingDataRegistry.Instance.CustomDeferredChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeCode);
			RatingDataRegistry.Instance.IncludeCustomDeferredChargeInInvoicing.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, enabled);
		}

		public void TestEmailNotSentIfDeferredChargeDisbledAndRegistryNotSet()
		{
			SetCustomsDeferredChargeInRegistry(Guid.Empty, false);

			SetupStaffMemberEmailAddress();
			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				RaiseTestInvoice(customDetail, testCreditor.PK);

				AssertEquals("Should not have sent email", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		public void TestEmailNotSent()
		{
			var cusDef = Factory.NewWithValidTestData(typeof(AccChargeCode)) as AccChargeCode;
			cusDef.AC_ChargeType = Constants.ChargeType.Comment;
			SetCustomsDeferredChargeInRegistry(cusDef.PK.ToGuid(), true);

			SetupStaffMemberEmailAddress();
			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				RaiseTestInvoice(customDetail, testCreditor.PK);

				AssertEquals("Should not have sent email", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		public void TestExceptionNotCaught()
		{
			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				try
				{
					var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
					poster.RaiseInvoices(null);

					Assert("Exception should have been thrown", false);
				}
				catch (NullReferenceException)
				{
					Assert(true);
				}
			}
		}

		public void TestNoJobIsCreatedIfNothingPaidByBroker()
		{
			SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = Array.Empty<ICustomsCharges>(), //No charge
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory
				};
				Env.OutgoingMailManager.EmailsCreated.Clear();

				Factory.Save();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				Job job = new Job.Loader(customDetail).Load();
				AssertNull(job);
			}
		}

		public void TestChargesAddedIfSomePaidByBroker()
		{
			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();

				Job expectedJob = Factory.NewJobWithValidTestDataForTesting<Job>();
				expectedJob.JH_ParentID = customDetail.PK;
				expectedJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				// Prepare data to avoid Validation errors on saving Job before posting
				expectedJob.JH_OA_LocalChargesAddr = TestObjectCreator.Agent.MainAddress.PK;
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;
				Factory.Save();

				customDetail.JobNumber = "jklfsd897";

				expectedJob.PlugInData = customDetail;

				CustomsCharge customCharge1 = new CustomsCharge(null, "TEST", 110.0m, 0m, true, testCreditor.PK);
				CustomsCharge customCharge2 = new CustomsCharge(null, "TES2", 120.0m, 0m, false, testCreditor.PK);
				var charge1 = new MockCustomCharge { fIsActive = true, fCustomsCharges = new CustomsCharge[] { customCharge1, customCharge2 } };

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { charge1 },
					InvoiceNumber = "TR436877",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail
				};
				customDetail.DataProviders.Add(chargeProvider);
				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				AssertEquals(1, expectedJob.Charges.Count);
				AssertEquals(110.0m, expectedJob.Charges[0].JR_LocalCostAmt);
			}
		}

		public void TestAutoRatingReturnsNonZeroAmount()
		{
			var customDetail = Factory.New<MockCustomsJobProvider>();
			var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, Array.Empty<ZGuid>());

			AutoRateInfoCollection infoCollection = new AutoRateInfoCollection(Factory);
			Assert(poster.NoCustomsDisbursementChargeToAdd_ForTestOnly(infoCollection));

			infoCollection.AddNew(CC1, AUD.RX_Code, 0);
			Assert(poster.NoCustomsDisbursementChargeToAdd_ForTestOnly(infoCollection));

			infoCollection.AddNew(CC1, AUD.RX_Code, 120);
			Assert(!poster.NoCustomsDisbursementChargeToAdd_ForTestOnly(infoCollection));

			infoCollection = new AutoRateInfoCollection(Factory);
			infoCollection.AddNew(CC1, AUD.RX_Code, 120);
			Assert(!poster.NoCustomsDisbursementChargeToAdd_ForTestOnly(infoCollection));
		}

		public void TestNoInvoiceIsRaisedIfRatingReturnedZeroAmount()
		{
			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				var charge1 = GetCustomCharge(122, 0, false, testCreditor.PK);
				customDetail.JobNumber = "TR436877";

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { charge1 },
					InvoiceNumber = "TR436877",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail
				};
				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				Factory.Save();

				ZQuery filter = GetInvoiceFilter();
				APInvoice actualInvoice = Factory.LoadTop1(typeof(APInvoice), filter) as APInvoice;

				AssertNull(actualInvoice);
			}
		}

		public void TestIfCustomsInvoiceExistWithSameNumberButDifferentCompany()
		{
			SetupStaffMemberEmailAddress();

			string aPInvoiceNum = "ABC123DEF";
			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				APInvoice existingInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
				existingInvoice.AH_OH = testCreditor.PK;
				existingInvoice.AH_TransactionNum = "ABC123DEF";
				existingInvoice.AH_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;

				Factory.Save();

				var customDetail = Factory.New<MockCustomsJobProvider>();
				var charge1 = GetCustomCharge(122, 0, true, testCreditor.PK);
				customDetail.JobNumber = "TR436877";

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { charge1 },
					InvoiceNumber = aPInvoiceNum,
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail
				};

				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				AssertEquals("Should not have sent email because the AP invoice is created in a differernt company", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		public void TestBranchAndDeparmentIfDeclarationAttachedToShipment()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			ZGuid shipmentPK = ZGuid.NewZGuid();

			var customDetail = Factory.New<MockCustomsJobProvider>();
			customDetail.fBranch = NonCurrentBranch;
			customDetail.isImport = true;
			customDetail.isExport = false;

			var paymentTerm = new PaymentTermInfos();
			paymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, Constants.IncoTerms.FreeOnBoard));

			var invoicingSupporter = (MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter;
			invoicingSupporter.ConsumerType = JobInvoicingConsumerTypes.Brokerage;
			invoicingSupporter.Origin = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "USLAX"));
			invoicingSupporter.Destination = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			invoicingSupporter.PaymentTerm = paymentTerm;
			invoicingSupporter.ConsumerType = JobInvoicingConsumerTypes.Shipment;
			invoicingSupporter.TransportMode = Constants.TransportModes.Sea;
			invoicingSupporter.ContainerMode = Constants.ContainerModes.FCL;
			invoicingSupporter.ConsolType = "NCN";

			var creditor = Factory.LoadTop1<OrgHeader>(new ZQuery());

			var mockShipment = new Mock<IJobInvoicingPlugIn>();

			mockShipment.Setup(m => m.PK).Returns(shipmentPK);
			mockShipment.Setup(m => m.TableName).Returns("JobShipment");
			mockShipment.Setup(m => m.Factory).Returns(Factory);
			mockShipment.Setup(m => m.InvoicingSupporter).Returns(customDetail.InvoicingSupporter);
			mockShipment.Setup(m => m.IsInDatabase).Returns(true);
			mockShipment.Setup(m => m.IsDeleted).Returns(false);
			customDetail.fTopLevelObjectForJobToReference = mockShipment.Object;

			Assert(NonCurrentBranch.PK != GlbBranch.CurrentBranch.PK);
			customDetail.fBranch = NonCurrentBranch;
			var charge1 = GetCustomCharge(122, 0, true, creditor.PK);

			var chargePosterCreator = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, Array.Empty<ZGuid>());
			ICustomsCharges[] charges = { charge1 };
			new CustomsChargesManager(customDetail).RateCustomsCharges(charges);

			using (Job result = chargePosterCreator.GetOrCreateCustomsJob_ForTestOnly(customDetail))
			{
				AssertEquals("Job's Foreign Key", shipmentPK, result.JH_ParentID);
				AssertEquals("Job's Foreign Table", "JS", result.JH_ParentTableCode);

				AssertEquals("Branch", customDetail.Branch.PK, result.JH_GB);
			}
		}

		public void TestBranchAndDeparmentOfChargeLineForImport()
		{
			GlbBranch nonCurrentBranch = Factory.NewWithValidTestData<GlbBranch>();
			nonCurrentBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			ForwardingShipment realShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			realShipment.JS_INCO = Constants.IncoTerms.FreeOnBoard;
			realShipment.JS_RL_NKDestination = "AUSYD";
			realShipment.JS_RL_NKOrigin = "USLAX";
			realShipment.JS_TransportMode = Constants.TransportModes.Sea;
			realShipment.JS_PackingMode = Constants.ContainerModes.FCL;
			realShipment.JS_UniqueConsignRef = "S0000001";

			// Prepare data to avoid Validation errors on saving Job before posting
			Job shipmentJob = new Job.Loader(realShipment).TryCreateWithoutMutexForTestOnly();
			shipmentJob.JH_OA_LocalChargesAddr = TestObjectCreator.Agent.MainAddress.PK;
			shipmentJob.JH_GB = nonCurrentBranch.PK;
			shipmentJob.JH_GE = TestObjectCreator.FISDepartment.PK;
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

			var customDetail = Factory.New<MockCustomsJobProvider>();
			customDetail.fBranch = nonCurrentBranch;
			((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).ConsumerType = JobInvoicingConsumerTypes.Brokerage;
			customDetail.fTopLevelObjectForJobToReference = realShipment;
			customDetail.isImport = true;
			customDetail.isExport = false;
			((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).TransportMode = "SEA";
			((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).ContainerMode = "FCL";
			Factory.Save();

			Assert(nonCurrentBranch.PK != GlbBranch.CurrentBranch.PK);

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;
			var charge1 = GetCustomCharge(122, 0, true, testCreditor.PK);

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.Australia))
			{
				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { charge1 },
					InvoiceNumber = "TR436877",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail
				};
				customDetail.DataProviders.Add(chargeProvider);
				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				Factory.Save();

				ZQuery filter = GetInvoiceFilter();
				APInvoice actualInvoice = Factory.LoadTop1(typeof(APInvoice), filter) as APInvoice;

				AssertNotNull(actualInvoice);
				AssertEquals(1, actualInvoice.Lines.Count);

				AssertEquals("Header Department", AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingImportSeaFcl.Value.GetDefaultDepartment(), actualInvoice.AH_GE.ToGuid());
				AssertEquals("Line Department", AccountingConfigurationRegistry.Instance.CustomsImportSeaFcl.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty), actualInvoice.Lines[0].AL_GE.ToGuid());
				AssertEquals("Header Branch", nonCurrentBranch.PK, actualInvoice.AH_GB);
				AssertEquals("LIne Branch", nonCurrentBranch.PK, actualInvoice.Lines[0].AL_GB);
			}
		}

		public void TestBranchAndDeparmentOfChargeLineForExport()
		{
			GlbBranch nonCurrentBranch = Factory.NewWithValidTestData<GlbBranch>();
			nonCurrentBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			ForwardingShipment realShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			realShipment.JS_INCO = Constants.IncoTerms.FreeOnBoard;
			realShipment.JS_RL_NKDestination = "AUSYD";
			realShipment.JS_RL_NKOrigin = "USLAX";
			realShipment.JS_TransportMode = Constants.TransportModes.Sea;
			realShipment.JS_PackingMode = Constants.ContainerModes.FCL;
			realShipment.JS_UniqueConsignRef = "S0000001";

			// Prepare data to avoid Validation errors on saving Job before posting
			Job shipmentJob = new Job.Loader(realShipment).TryCreateWithoutMutexForTestOnly();
			shipmentJob.JH_OA_LocalChargesAddr = TestObjectCreator.Agent.MainAddress.PK;
			shipmentJob.JH_GB = nonCurrentBranch.PK;
			shipmentJob.JH_GE = TestObjectCreator.FESDepartment.PK;
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

			var customDetail = Factory.New<MockCustomsJobProvider>();
			customDetail.fBranch = nonCurrentBranch;
			((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).ConsumerType = JobInvoicingConsumerTypes.Brokerage;
			customDetail.fTopLevelObjectForJobToReference = realShipment;
			customDetail.isExport = true;
			customDetail.isImport = false;
			((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).TransportMode = "SEA";
			customDetail.JobNumber = "S0000001";
			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.Australia))
			{
				Assert(nonCurrentBranch.PK != GlbBranch.CurrentBranch.PK);

				var charge1 = GetCustomCharge(122, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { charge1 },
					InvoiceNumber = "TR436877",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail
				};
				customDetail.DataProviders.Add(chargeProvider);
				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				Factory.Save();

				ZQuery filter = GetInvoiceFilter();
				APInvoice actualInvoice = Factory.LoadTop1(typeof(APInvoice), filter) as APInvoice;

				AssertNotNull(actualInvoice);
				AssertEquals(1, actualInvoice.Lines.Count);

				//AssertEquals("Header Department", Env.Registry.Accounting.DefaultDepartments.ForwardingExportSeaFcl, ActualInvoice.AH_GE.ToGuid());
				AssertEquals("Line Department", AccountingConfigurationRegistry.Instance.CustomsExportSeaFcl.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty), actualInvoice.Lines[0].AL_GE.ToGuid());
				AssertEquals("Header Branch", nonCurrentBranch.PK, actualInvoice.AH_GB);
				AssertEquals("LIne Branch", nonCurrentBranch.PK, actualInvoice.Lines[0].AL_GB);
			}
		}

		public void TestBranchAndDeparmentCorrectlyDefaults()
		{
			var mockShipment = Factory.New<MockCustomsJobProvider>();
			mockShipment.IncoTermExposed = Constants.IncoTerms.FreeOnBoard;
			mockShipment.isImport = false;
			mockShipment.isExport = true;
			mockShipment.isDomestic = false;
			((MockCustomsJobProviderInvoicingSupporter)mockShipment.InvoicingSupporter).ConsumerType = JobInvoicingConsumerTypes.Shipment;
			((MockCustomsJobProviderInvoicingSupporter)mockShipment.InvoicingSupporter).TransportMode = Constants.TransportModes.Sea;
			((MockCustomsJobProviderInvoicingSupporter)mockShipment.InvoicingSupporter).ContainerMode = Constants.ContainerModes.FCL;
			((MockCustomsJobProviderInvoicingSupporter)mockShipment.InvoicingSupporter).EditSecurityLock = false;
			mockShipment.JobNumber = "HKLad8970";

			var customDetail = Factory.New<MockCustomsJobProvider>();
			customDetail.fBranch = NonCurrentBranch;
			((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).ConsumerType = JobInvoicingConsumerTypes.Brokerage;
			customDetail.fTopLevelObjectForJobToReference = mockShipment;
			customDetail.isExport = true;
			customDetail.isImport = false;
			((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).TransportMode = "SEA";
			//CustomDetail.ContainerMode = "FCL";
			customDetail.JobNumber = "IYUwer78";

			Assert(NonCurrentBranch.PK != GlbBranch.CurrentBranch.PK);

			var creditor = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var charge1 = GetCustomCharge(122, 0, true, creditor.PK);

			var chargePosterCreator = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, Array.Empty<ZGuid>());
			ICustomsCharges[] charges = new ICustomsCharges[] { charge1 };
			new CustomsChargesManager(customDetail).RateCustomsCharges(charges);

			using (Job result = chargePosterCreator.GetOrCreateCustomsJob_ForTestOnly(customDetail))
			{
				AssertEquals("Job's Foreign Key", mockShipment.PK, result.JH_ParentID);
				AssertEquals("Job's Foreign Table", "Z0", result.JH_ParentTableCode);

				AssertEquals("Branch", customDetail.Branch.PK, result.JH_GB);
				//AssertEquals("Department set according to the shipment one", Env.Registry.Accounting.DefaultDepartments.ForwardingExportSeaFcl, Result.JH_GE);
			}
		}

		public void TestExistingJobDepartmentAndBranchDoesNotChange()
		{
			//Shipment Detail
			ForwardingShipment realShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			realShipment.JS_INCO = Constants.IncoTerms.FreeOnBoard;
			realShipment.JS_RL_NKDestination = "AUSYD";
			realShipment.JS_RL_NKOrigin = "USLAX";
			realShipment.JS_TransportMode = Constants.TransportModes.Sea;
			realShipment.JS_PackingMode = Constants.ContainerModes.FCL;
			realShipment.JS_UniqueConsignRef = "S0000001";

			// Job Detail
			Job expectedJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			expectedJob.JH_ParentID = realShipment.PK;
			expectedJob.JH_ParentTableCode = "JS";
			expectedJob.JH_JobNum = realShipment.JS_UniqueConsignRef;
			expectedJob.JH_GB = GlbBranch.CurrentBranch.PK;
			expectedJob.JH_GE = AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingExportRail.Value.GetDefaultDepartment();

			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_Code = "~B2";

			Factory.Save();

			ZGuid declarationPK = ZGuid.NewZGuid();

			// Declaration Detail
			var customDetail = Factory.New<MockCustomsJobProvider>();
			customDetail.fBranch = NonCurrentBranch;
			((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).ConsumerType = JobInvoicingConsumerTypes.Brokerage;
			customDetail.fTopLevelObjectForJobToReference = realShipment;

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;
			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.Australia))
			{
				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				Factory.Save();

				Job retrievedJob = Factory.Load(typeof(Job), expectedJob.PK) as Job;
				AssertEquals("Table Name", "JS", retrievedJob.JH_ParentTableCode);
				AssertEquals("Job Foreign Key", realShipment.PK, retrievedJob.JH_ParentID);
				AssertEquals("Job Number", realShipment.JS_UniqueConsignRef, retrievedJob.JH_JobNum);
				AssertEquals("Job Branch", GlbBranch.CurrentBranch.PK, retrievedJob.JH_GB);
				AssertEquals("Job Department", AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingExportRail.Value.GetDefaultDepartment(), retrievedJob.JH_GE);
			}
		}

		[TestDate(2018, 07, 14)]
		public void TestRunningAutoRatingAgainDoesNotChangeInvoiceTypeWhenDebtorIsSame()
		{
			ZGuid staff = SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();

			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				// Disable WHT to avoid some Validation errors
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false);

				var cusCharge = GetCustomCharge(chargeCode, 120m, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory,
					AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false)
				};
				Env.OutgoingMailManager.EmailsCreated.Clear();

				LocalClient.MiscServ.OM_ARTreatDisbursementsAsStandardValue = 0m;// DSB amount is 120m below. So the default type is DSB, however once users set the type to FIN, system should not change

				LocalClient.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
				OrgInvoiceRollupOrGroup group = LocalClient.CompanyData.InvoiceRollupOrGroups.AddNew();
				group.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
				group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
				group.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
				group.PG_InvoicePostingStyle = InvoicePostingOptionsList.Codes.DisbursementAndFinal;
				Factory.Save();

				Job job = CreateJob("Z00001000", LocalClient, 5m, Agent, 10m);
				job.JH_ParentID = customDetail.PK;

				RefCurrency localCurrency = GlbCompany.CurrentCompany.LocalCurrency;

				Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", localCurrency, 150m, Creditor1, localCurrency, 160m, LocalClient);
				charge1.JR_APInvoiceDate = new ZDateTime(2009, 1, 1);
				charge1.JR_APInvoiceNum = "987432";
				charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

				Charge charge2 = CreateCharge(job, CC5, "Charge Code 5", localCurrency, 200m, Creditor2, localCurrency, 250m, LocalClient);
				charge2.JR_APInvoiceDate = new ZDateTime(2009, 1, 2);
				charge2.JR_APInvoiceNum = "9874387";
				charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

				Charge customsCharge = job.Charges.AddNew();
				customsCharge.JR_AC = chargeCode.PK;
				customsCharge.JR_Desc = "Charge Code 2";
				customsCharge.JR_OH_CostAccount = Creditor2.PK;
				customsCharge.JR_RX_NKCostCurrency = localCurrency.RX_Code;
				customsCharge.JR_OSCostAmt = 120m;
				customsCharge.JR_OH_SellAccount = LocalClient.PK;
				customsCharge.JR_RX_NKSellCurrency = localCurrency.RX_Code;
				customsCharge.JR_OSSellAmt = 120m;
				AssertEquals("PreCondition", InvoiceTypesList.Codes.DisbursementInvoice, customsCharge.JR_InvoiceType);

				//But users have overriden and this should not be overriden to DSB when disbursement charge is auto-rated again
				customsCharge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				Factory.Save();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB | ChargePosterBehaviours.ARAPPostNonDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				Factory.Save();

				ARInvoice arInvoice1 = new ARInvoice.Loader(Factory).LoadTop1NotReversed(job.PK, new[] { CC1.PK });
				ARInvoice arInvoice2 = new ARInvoice.Loader(Factory).LoadTop1NotReversed(job.PK, new[] { CC5.PK });
				ARInvoice arInvoice3 = new ARInvoice.Loader(Factory).LoadTop1NotReversed(job.PK, new[] { chargeCode.PK });

				AssertEquals("AR should have been posted into one invoice", arInvoice1, arInvoice2);
				AssertEquals("AR should have been posted into one invoice", arInvoice2, arInvoice3);
			}
		}

		public void TestSettingGSTAmountsOnAPInvoice()
		{
			AccTaxRate newRate = Factory.NewWithValidTestData<AccTaxRate>();
			newRate.SetRate_ForTestOnly(125, 10);

			var chargeCode = SetChargeCode();
			chargeCode.AC_AT_GSTRate = newRate.PK;
			var testCreditor = Factory.LoadTop1<OrgHeader>(new ZQuery());
			testCreditor.CompanyData.SetAPTaxApplicable(true);

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				var charge1 = GetCustomCharge(123, 45, true, testCreditor.PK);
				customDetail.JobNumber = "TR436877";

				// Prepare data to avoid Validation errors on saving Job before posting
				Job expectedJob = Factory.NewJobWithValidTestDataForTesting<Job>();
				expectedJob.JH_ParentID = customDetail.PK;
				expectedJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				expectedJob.JH_OA_LocalChargesAddr = TestObjectCreator.Agent.MainAddress.PK;
				expectedJob.JH_JobNum = "TR436877";
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

				expectedJob.PlugInData = customDetail;

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { charge1 },
					InvoiceNumber = "TR436877",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail
				};
				customDetail.DataProviders.Add(chargeProvider);
				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				Factory.Save();

				ZQuery filter = GetInvoiceFilter();
				APInvoice actualInvoice = Factory.LoadTop1(typeof(APInvoice), filter) as APInvoice;

				AssertNotNull(actualInvoice);
				Assert(actualInvoice.PK.IsValid);

				AssertEquals("Invoice Lines", 1, actualInvoice.Lines.Count);
				AssertEquals("Invoice Line 1 Amount", 123M, actualInvoice.Lines[0].AL_OSExTaxAmount);
				AssertEquals("Invoice Line 1 GST, ignoring customs override", 15.38m, actualInvoice.Lines[0].AL_OSTaxAmount);
			}
		}

		public void TestSettingGSTAmountsOnAPInvoice_IsGSTNotApplicable()
		{
			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				var charge1 = GetCustomCharge(123, 45, true, testCreditor.PK);
				customDetail.JobNumber = "TR436877";

				// Prepare data to avoid Validation errors on saving Job before posting
				Job expectedJob = Factory.NewJobWithValidTestDataForTesting<Job>();
				expectedJob.JH_ParentID = customDetail.PK;
				expectedJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				expectedJob.JH_OA_LocalChargesAddr = TestObjectCreator.Agent.MainAddress.PK;
				expectedJob.JH_JobNum = "TR436877";
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

				expectedJob.PlugInData = customDetail;

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { charge1 },
					InvoiceNumber = "TR436877",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail
				};
				customDetail.DataProviders.Add(chargeProvider);
				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				Factory.Save();

				var charges = expectedJob.Charges;
				CombineAssertions(() =>
				{
					AssertEquals("one charge", 1, charges.Count);
					AssertEquals("Charge amount", 168m, charges[0].JR_LocalCostAmt);
					AssertEquals("Invoice No", "TR436877", charges[0].JR_APInvoiceNum);
					Assert("Cost should be posted", charges[0].IsCostPosted);
					Assert("Revenue should be not posted", !charges[0].IsRevenuePosted);
					AssertEquals(@"Customs Disbursement Charges
Current Amounts
  TEST                                      123.00
  + VAT                                      45.00", charges[0].JR_Desc);
				});
			}
		}

		public void TestSettingGSTAmountsOnAPInvoice_ZeroAmount_IsGSTNotApplicable()
		{
			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				var charge1 = GetCustomCharge(0, 45, true, testCreditor.PK);
				customDetail.JobNumber = "TR436877";

				// Prepare data to avoid Validation errors on saving Job before posting
				Job expectedJob = Factory.NewJobWithValidTestDataForTesting<Job>();
				expectedJob.JH_ParentID = customDetail.PK;
				expectedJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				expectedJob.JH_OA_LocalChargesAddr = TestObjectCreator.Agent.MainAddress.PK;
				expectedJob.JH_JobNum = "TR436877";
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

				expectedJob.PlugInData = customDetail;

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { charge1 },
					InvoiceNumber = "TR436877",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail
				};
				customDetail.DataProviders.Add(chargeProvider);
				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);

				Factory.Save();

				var charges = expectedJob.Charges;
				CombineAssertions(() =>
				{
					AssertEquals("one charge", 1, charges.Count);
					AssertEquals("Charge amount", 0m, charges[0].JR_LocalCostAmt);
					AssertEquals("Invoice No", "", charges[0].JR_APInvoiceNum);
					Assert("Cost should be not posted", !charges[0].IsCostPosted);
					Assert("Revenue should be not posted", !charges[0].IsRevenuePosted);
					AssertEquals(@"Customs Disbursement Charges
  + VAT                                      45.00", charges[0].JR_Desc);
				});
			}
		}

		public void TestSettingGSTAmountsOnAPInvoiceWhenThereIsAnAdditionalJob()
		{
			var job1Reference = "111111";
			var job2Reference = "222222";

			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.SetRate_ForTestOnly(125, 10);

			var chargeCodeDuty = TestObjectCreator.CC1;
			chargeCodeDuty.AC_AT_GSTRate = TestObjectCreator.GSTFREE1.PK;

			var chargeCodeGST = TestObjectCreator.CC2;
			chargeCodeGST.AC_AT_GSTRate = TestObjectCreator.GSTFREE1.PK;

			var testCreditor = TestObjectCreator.Creditor1;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCodeDuty, Constants.CountryCodes.Canada))
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

				var charge1Duty = GetCustomCharge(chargeCodeDuty, 100, 0, true, testCreditor.PK);
				var charge1GST = GetCustomCharge(chargeCodeGST, 10, 0, true, testCreditor.PK);
				var customDetail1 = Factory.New<MockCustomsJobProvider>();
				customDetail1.JobNumber = job1Reference;
				var job1 = CreateInvoicingJob(job1Reference, customDetail1);
				var chargeProvider1 = CreateCustomsChargesProvider(customDetail1, new[] { charge1Duty, charge1GST });
				customDetail1.DataProviders.Add(chargeProvider1);
				var poster1 = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.SendEmail, new[] { chargeCodeDuty.PK, chargeCodeGST.PK });
				poster1.RaiseInvoices(customDetail1);
				Factory.Save();

				AssertEquals("Job1 charges count", 2, job1.Charges.Count);

				var job1DutyCharge = job1.Charges.Cast<Charge>().First(x => x.JR_JH == job1.PK && x.JR_AC == chargeCodeDuty.PK);
				AssertEquals("job1 duty charge: Cost Amount", 100m, job1DutyCharge.JR_LocalCostAmt);
				//AssertEquals("job1 duty charge: AP Invoice #", string.Empty, job1DutyCharge.JR_APInvoiceNum);

				var job1GSTCharge = job1.Charges.Cast<Charge>().First(x => x.JR_JH == job1.PK && x.JR_AC == chargeCodeGST.PK);
				AssertEquals("job1 GST charge: Cost Amount", 10m, job1GSTCharge.JR_LocalCostAmt);
				//AssertEquals("job1 GST charge: AP Invoice #", string.Empty, job1DutyCharge.JR_APInvoiceNum);

				job1DutyCharge.JR_APInvoiceNum = string.Empty; // manual user intervention?
				job1GSTCharge.JR_APInvoiceNum = string.Empty;
				Factory.Save();

				var charge2Duty = GetCustomCharge(chargeCodeDuty, 200, 0, true, testCreditor.PK);
				var charge2GST = GetCustomCharge(chargeCodeGST, 20, 0, true, testCreditor.PK);
				var customDetail2 = Factory.New<MockCustomsJobProvider>();
				customDetail2.JobNumber = job2Reference;
				customDetail2.AdditionalJobsToShowChargesFor = new IJobInvoicingPlugIn[] { customDetail1 };
				var job2 = CreateInvoicingJob(job2Reference, customDetail2);
				var chargeProvider2 = CreateCustomsChargesProvider(customDetail2, new[] { charge2Duty, charge2GST });
				customDetail2.DataProviders.Add(chargeProvider2);
				var poster2 = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.SendEmail, new[] { chargeCodeDuty.PK, chargeCodeGST.PK });
				poster2.RaiseInvoices(customDetail2);
				Factory.Save();

				AssertEquals("Job1 charges count", 2, job1.Charges.Count);

				AssertEquals("Job2 charges count (including those from job1 as blue charges)", 4, job2.Charges.Count);

				AssertEquals("job1 duty charge: Cost Amount", 100m, job1DutyCharge.JR_LocalCostAmt);
				//AssertEquals("job1 duty charge: AP Invoice #", job1Reference, job1DutyCharge.JR_APInvoiceNum);

				AssertEquals("job1 GST charge: Cost Amount", 10m, job1GSTCharge.JR_LocalCostAmt);
				//AssertEquals("job1 GST charge: AP Invoice #", job1Reference, job1GSTCharge.JR_APInvoiceNum);

				var job2DutyCharge = job2.Charges.Cast<Charge>().First(x => x.JR_JH == job2.PK && x.JR_AC == chargeCodeDuty.PK);
				AssertEquals("job2 duty charge: Cost Amount", 200m, job2DutyCharge.JR_LocalCostAmt);
				//AssertEquals("job2 duty charge: AP Invoice #", job2Reference, job2DutyCharge.JR_APInvoiceNum);

				var job2GSTCharge = job2.Charges.Cast<Charge>().First(x => x.JR_JH == job2.PK && x.JR_AC == chargeCodeGST.PK);
				AssertEquals("job2 GST charge: Cost Amount", 20m, job2GSTCharge.JR_LocalCostAmt);
				//AssertEquals("job2 GST charge: AP Invoice #", job2Reference, job2GSTCharge.JR_APInvoiceNum);
			}
		}

		public void TestPostNegativeDSBCharges()
		{
			var staff = SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false);
				((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).DefaultDebtor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				Factory.Save();

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					APDueDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory,
					AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false)
				};

				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.PostNegativeCost, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();
				var invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);
				AssertEquals("One charge line should have been created", 1, invoicingJob.Charges.Count);
				AssertEquals("INV123", invoicingJob.Charges[0].JR_APInvoiceNum);
				AssertEquals(-100m, invoicingJob.Charges[0].APLine.AL_LineAmount);
				AssertEquals("It has not been posted for cost", true, invoicingJob.Charges[0].IsCostPosted);
				AssertEquals("It has been posted for revenue", false, invoicingJob.Charges[0].IsRevenuePosted);

				cusCharge = GetCustomCharge(chargeCode, 50, 0, true, testCreditor.PK);
				chargeProvider.CustomsCharges = new ICustomsCharges[] { cusCharge };
				poster.RaiseInvoices(customDetail);
				Factory.Save();
				var charge1 = invoicingJob.Charges[0];
				var charge2 = invoicingJob.Charges[1];
				AssertEquals("INV123", charge1.JR_APInvoiceNum);
				AssertEquals(100m, charge1.JR_OSCostAmt);
				AssertEquals(100m, charge1.JR_LocalCostAmt);
				AssertEquals(100m, charge1.JR_OSSellAmt);
				AssertEquals(100m, charge1.JR_LocalSellAmt);
				Assert(charge1.IsCostPosted);
				Assert(!charge1.IsRevenuePosted);
				AssertEquals("AP Detail", -100m, charge1.APLine.AL_LineAmount);
				AssertEquals("AR Detail", -100m, charge1.ARLine.AL_LineAmount);

				AssertEquals("INV123/CUSDSB/1", charge2.JR_APInvoiceNum);
				AssertEquals(-50m, charge2.JR_OSCostAmt);
				AssertEquals(-50m, charge2.JR_LocalCostAmt);
				AssertEquals(-50m, charge2.JR_OSSellAmt);
				AssertEquals(-50m, charge2.JR_LocalSellAmt);
				Assert(charge2.IsCostPosted);
				Assert(!charge2.IsRevenuePosted);
				AssertEquals("AP Detail", 50m, charge2.APLine.AL_LineAmount);
				AssertEquals("AR Detail", 50m, charge2.ARLine.AL_LineAmount);
			}
		}

		public void TestAutoRateDSBWhenUniqueNumberOnChargeProviderIsEmpty()
		{
			var staff = SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false);
				((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).DefaultDebtor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				Factory.Save();

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = ZString.Empty,
					InvoiceDate = new ZDateTime(2005, 4, 12),
					APDueDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory,
					AutoPostingNotification = new AutoPostingNotification(new[] { staff }, false)
				};

				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();
				var invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertNotNull("One invoicing job should have been created", invoicingJob);
				AssertEquals("One charge line should have been created", 1, invoicingJob.Charges.Count);

				var charge1 = invoicingJob.Charges[0];
				AssertEquals("JR_APInvoiceNum should be empty", ZString.Empty, charge1.JR_APInvoiceNum);
				AssertEquals("JR_OSCostAmt should be 100", 100m, charge1.JR_OSCostAmt);
				AssertEquals("Charge is not posted for cost", false, charge1.IsCostPosted);

				charge1.JR_APInvoiceNum = "INV123";
				charge1.JR_APInvoiceDate = new ZDateTime(2005, 4, 12);
				Factory.Save();

				cusCharge = GetCustomCharge(chargeCode, 50, 0, true, testCreditor.PK);
				chargeProvider.CustomsCharges = new ICustomsCharges[] { cusCharge };
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertEquals("No more charge is created", 1, invoicingJob.Charges.Count);

				var charge2 = invoicingJob.Charges[0];
				AssertEquals("Charge PK should be identical with charge1", charge1.PK, charge2.PK);
				AssertEquals("JR_APInvoiceNum is not changed", "INV123", charge2.JR_APInvoiceNum);
				AssertEquals("JR_APInvoiceDate is not changed", new ZDateTime(2005, 4, 12), charge2.JR_APInvoiceDate);
				AssertEquals("JR_OSCostAmt updated to 50", 50m, charge2.JR_OSCostAmt);
				AssertEquals("Charge is not posted for cost", false, charge2.IsCostPosted);

				poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertEquals("No more charge is created", 1, invoicingJob.Charges.Count);

				charge2 = invoicingJob.Charges[0];
				AssertEquals("Charge is posted for cost", true, charge2.IsCostPosted);

				cusCharge = GetCustomCharge(chargeCode, 150, 0, true, testCreditor.PK);
				chargeProvider.CustomsCharges = new ICustomsCharges[] { cusCharge };
				poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				invoicingJob = Factory.Load<Job>(new JobInvoicingDataAccessor(Factory).GetJobFromForeignKey(customDetail));
				AssertEquals("No more charge is created", 2, invoicingJob.Charges.Count);

				var charge3 = invoicingJob.Charges[0];
				AssertEquals("Charge PK should be identical with charge1", charge1.PK, charge3.PK);
				AssertEquals("JR_APInvoiceNum is not changed", "INV123", charge3.JR_APInvoiceNum);
				AssertEquals("JR_APInvoiceDate is not changed", new ZDateTime(2005, 4, 12), charge3.JR_APInvoiceDate);
				AssertEquals("JR_OSCostAmt updated to 50", 50m, charge3.JR_OSCostAmt);
				AssertEquals("Charge is posted for cost", true, charge3.IsCostPosted);

				var charge4 = invoicingJob.Charges[1];
				AssertEquals("JR_APInvoiceNum is empty for new disbursement charge", ZString.Empty, charge4.JR_APInvoiceNum);
				AssertEquals("JR_APInvoiceDate is empty for new disbursement charge", ZDateTime.Empty, charge4.JR_APInvoiceDate);
				AssertEquals("JR_OSCostAmt is the discrepancy amount", 100m, charge4.JR_OSCostAmt);
				AssertEquals("Charge is not posted for cost", false, charge4.IsCostPosted);
			}
		}

		MockCustomsChargesProvider CreateCustomsChargesProvider(MockCustomsJobProvider customDetail, ICustomsCharges[] charges)
		{
			var chargeProvider = new MockCustomsChargesProvider
			{
				CustomsCharges = charges,
				CustomsJob = customDetail,
				InvoiceNumber = customDetail.JobNumber,
				InvoiceDate = new ZDateTime(2005, 4, 12),
				APDueDate = new ZDateTime(2005, 4, 12)
			};
			return chargeProvider;
		}

		Job CreateInvoicingJob(string jobNumber, MockCustomsJobProvider customDetail)
		{
			var result = Factory.NewJobWithValidTestDataForTesting<Job>();
			result.JH_ParentID = customDetail.PK;
			result.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			result.JH_OA_LocalChargesAddr = TestObjectCreator.Agent.MainAddress.PK;
			result.JH_JobNum = jobNumber;
			//result.PlugInData = customDetail;
			return result;
		}

		public void TestRaiseInvoicesThrowsConvertsCriticalValidationToCustomsInvoiceRaiseException()
		{
			var chargeCode = SetChargeCode();
			var customDetail = setupCustomDetail(chargeCode);

			Job job = CreateJob("Z00001000", LocalClient, 5m, Agent, 10m);
			job.JH_ParentID = customDetail.PK;

			customsCharge2 = job.Charges.AddNew();
			customsCharge2.JR_AC = chargeCode.PK;
			customsCharge2.JR_OSCostAmt = 100m;
			customsCharge2.JR_LocalSellAmt = 100m;
			customsCharge2.JR_AW_SellWHTRate = TestObjectCreator.WHT1.PK;
			Factory.Saving += Factory_Saving;

			try
			{
				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Fail("Should throw CustomsInvoiceRaiseException in RaiseInvoices");
			}
			catch (CustomsInvoiceRaiseException ex)
			{
				AssertContains("Raise Invoices should throw CustomsInvoiceRaiseException when a OnSavingCriticalCheckException is thrown", "Unable to Save:", ex.Message);
			}
			catch (Exception ex)
			{
				Fail(string.Format("A CustomsInvoiceRaiseException was expected but {0} with the following Message was caught\r\n{1}", ex.GetType(), ex.Message));
			}
			finally
			{
				Factory.Saving -= Factory_Saving;
			}
		}
		Charge customsCharge2;

		void Factory_Saving(BusinessObjectFactory factory)
		{
			throw new OnSavingCriticalCheckException<Charge>(customsCharge2, CriticalValidationErrorType.DummyErrorKeyForTest, "Unable to Save:", "E=MC2");
		}

		MockCustomsJobProvider setupCustomDetail(AccChargeCode chargeCode)
		{
			SetupStaffMemberEmailAddress();

			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());

				Factory.Save();
				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory
				};
				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();

				return customDetail;
			}
		}

		public void TestMockCustomsJobProviderEditSecurity()
		{
			IJobInvoicingPlugIn testJob = Factory.New<MockCustomsJobProvider>();
			AssertEquals("EditSecurityCheckPoint should be Empty", Env.Security.None, testJob.InvoicingSupporter.EditSecurityCheckpoint);
			AssertEquals("EditSecuritytMessage should be Empty", ZString.Empty, testJob.InvoicingSupporter.EditSecurityMessage);
			AssertEquals("EditSecurityLock should be False", ZBool.False, testJob.InvoicingSupporter.EditSecurityLock);
		}

		public void TestPostWhenDebtorUseForeignCurrency()
		{
			var mockPlugIn = Factory.New<MockCustomsJobProvider>();
			mockPlugIn.JobNumber = "S00001000";

			SetupStaffMemberEmailAddress();
			var expectedJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			expectedJob.JH_JobNum = "S00001000";
			ExchangeRate rate = expectedJob.ExchangeRates.AddNew();
			rate.JF_RX_NKRateCurrency = TestObjectCreator.USD.RX_Code;
			rate.JF_BaseRate = 2m;
			expectedJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			expectedJob.JH_GB = GlbBranch.CurrentBranch.PK;
			expectedJob.JH_ParentID = mockPlugIn.PK;
			expectedJob.JH_ParentTableCode = DummyBusinessObject.Schema.TablePrefix;
			expectedJob.JH_OA_LocalChargesAddr = TestObjectCreator.Agent.MainAddress.PK;
			TestObjectCreator.Agent.CompanyData.OB_RX_NKARDDefltCurrency = TestObjectCreator.USD.RX_Code;
			var group = TestObjectCreator.Agent.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_JobType = "ALL";
			group.PG_ServiceDirection = "ALL";
			group.PG_TransportMode = "ALL";
			group.PG_InvoicePostingStyle = InvoicePostingOptionsList.Codes.ForeignCurrencyInvoiceAndFinal;
			var option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = true;
			option.APPostDSB = true;
			option.ARPostDSB = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, option);
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, TestObjectCreator.Creditor1.PK.ToGuid());

			var cUSDSBCharge = expectedJob.Charges.AddNew();

			cUSDSBCharge.JR_AC = RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value;
			cUSDSBCharge.JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			cUSDSBCharge.JR_OSSellExRate = 2m;
			cUSDSBCharge.JR_LocalSellAmt = 100m;

			var invoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.USD, 2m);
			invoice.AH_OH = TestObjectCreator.Agent.PK;

			var line = (ARInvoiceLine)TestObjectCreator.CreateInvoiceLine(invoice, 200m, invoice.TransactionCurrency, 2m);
			line.AL_AC = RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value;
			line.AL_JH = expectedJob.PK;
			cUSDSBCharge.JR_AL_ARLine = line.PK;
			cUSDSBCharge.JR_AT_SellGSTRate = line.AL_AT;

			Factory.Save();

			expectedJob.PlugInData = mockPlugIn;

			var customDetail = Factory.New<MockCustomsJobProvider>();
			GlbStaff.CurrentUser.GS_EmailAddress = "test@wisetechglobal.com";
			mockPlugIn.AutoPostingNotification = new AutoPostingNotification(new[] { GlbStaff.CurrentUser.PK }, false);

			var charge = GetCustomCharge(100, 0, true, TestObjectCreator.Agent.PK);
			customDetail.fTopLevelObjectForJobToReference = mockPlugIn;
			var chargeProvider = new MockCustomsChargesProvider
			{
				CustomsCharges = new ICustomsCharges[] { charge },
				InvoiceNumber = "XXX",
				InvoiceDate = ZDateTime.Now,
				CustomsJob = mockPlugIn,
				Factory = Factory,
				AutoPostingNotification = new AutoPostingNotification(new[] { GlbStaff.CurrentUser.PK }, false)
			};
			customDetail.DataProviders.Add(chargeProvider);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var testPoster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB | ChargePosterBehaviours.SendEmail, new[] { cUSDSBCharge.JR_AC });
			testPoster.RaiseInvoices(customDetail);

			expectedJob.Charges.Load();
			AssertEquals("Should now be one charge on the job", 1, expectedJob.Charges.Count);
			AssertEquals("Local amount", 100m, expectedJob.Charges[0].JR_LocalSellAmt);
			AssertEquals("OS amount", 200m, expectedJob.Charges[0].JR_OSSellAmt);
			AssertEquals("Line amount", 100m, expectedJob.Charges[0].ARLine.AL_LineAmount);
			AssertEquals("Invoice amount", 100m, expectedJob.Charges[0].ARLine.TransactionHeader.AH_InvoiceAmount);

			AssertEquals("Shouldn't send any email.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		[TestDate(2018, 07, 14)]
		public void TestThrowExceptionWhenChargeCodeAGAcountIsEmpty()
		{
			SetupStaffMemberEmailAddress();

			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				chargeCode.AC_AG_WIPAccount = new Guid();
				chargeCode.AC_AG_AccrualAccount = new Guid();

				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);

				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory
				};
				customDetail.DataProviders.Add(chargeProvider);

				Env.OutgoingMailManager.EmailsCreated.Clear();

				LocalClient.MiscServ.OM_ARWHTApplicable = false;
				Agent.MiscServ.OM_ARWHTApplicable = false;
				Job job = CreateJob("Z00001000", LocalClient, 5m, Agent, 10M);
				job.JH_ParentID = customDetail.PK;

				RefCurrency localCurrency = GlbCompany.CurrentCompany.LocalCurrency;

				Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", localCurrency, 150m, Creditor1, localCurrency, 160m, Agent);
				charge1.JR_APInvoiceDate = new ZDateTime(2009, 1, 1);
				charge1.JR_APInvoiceNum = "987432";

				Charge charge2 = CreateCharge(job, CC5, "Charge Code 5", localCurrency, 200m, Creditor2, localCurrency, 250m, Agent);
				charge2.JR_APInvoiceDate = new ZDateTime(2009, 1, 2);
				charge2.JR_APInvoiceNum = "9874387";

				Factory.Save();

				((MockCustomsJobProviderInvoicingSupporter)customDetail.InvoicingSupporter).DefaultDebtor = job.AgentCollect;

				try
				{
					var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB | ChargePosterBehaviours.ARAPPostNonDSB, new[] { chargeCode.PK });
					poster.RaiseInvoices(customDetail);
					Fail("Should throw CustomsInvoiceRaiseException in RaiseInvoices");
				}
				catch (CustomsInvoiceRaiseException ex)
				{
					AssertContains("Raise Invoices should throw CustomsInvoiceRaiseException when chargeCode's AC_AG_AccrualAccount is empty", "Charge Code 'CUSDSB' must have Accrual GL Account entered.", ex.Message);
					AssertContains("Raise Invoices should throw CustomsInvoiceRaiseException when chargeCode's AC_AG_WIPAccount is empty", "Charge Code 'CUSDSB' must have WIP GL Account entered.", ex.Message);
				}
			}
		}

		[TestDate(2018, 07, 14)]
		public void TestAPInvoiceNumberAlwaysIncludeChargeCodeForDifferentCharge()
		{
			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);
				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory,
					APInvoiceNumberAlwaysIncludeChargeCode = true
				};
				customDetail.DataProviders.Add(chargeProvider);

				var job = CreateJob("Z00001000", LocalClient, 5m, Agent, 10M);
				job.JH_ParentID = customDetail.PK;

				Factory.Save();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARAPPostNonDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				AssertEquals("1 Charge", 1, job.Charges.Count);

				var customsDSBCharge = (Charge)job.Charges.Find(new ZQuery(JobChargeSchema.JR_AC, chargeCode.PK))[0];
				AssertEquals("INV123/CUSDSB", customsDSBCharge.JR_APInvoiceNum);
			}
		}

		[TestDate(2018, 07, 14)]
		public void TestAPInvoiceNumberDontIncludeChargeCodeForDifferentCharge()
		{
			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);
				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory,
					APInvoiceNumberAlwaysIncludeChargeCode = false
				};
				customDetail.DataProviders.Add(chargeProvider);

				var job = CreateJob("Z00001000", LocalClient, 5m, Agent, 10M);
				job.JH_ParentID = customDetail.PK;

				Factory.Save();

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARAPPostNonDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				AssertEquals("1 Charge", 1, job.Charges.Count);

				var customsDSBCharge = (Charge)job.Charges.Find(new ZQuery(JobChargeSchema.JR_AC, chargeCode.PK))[0];
				AssertEquals("INV123", customsDSBCharge.JR_APInvoiceNum);
			}
		}

		[TestDate(2018, 07, 14)]
		public void TestAPInvoiceNumberAlwaysIncludeChargeCodeForExistingCharge()
		{
			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);
				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory,
					APInvoiceNumberAlwaysIncludeChargeCode = true
				};
				customDetail.DataProviders.Add(chargeProvider);

				var job = CreateJob("Z00001000", LocalClient, 5m, Agent, 10M);
				job.JH_ParentID = customDetail.PK;

				var localCurrency = GlbCompany.CurrentCompany.LocalCurrency;

				var charge1 = CreateCharge(job, chargeCode, "CUSDSB", localCurrency, 300m, Creditor3, localCurrency, 350m, LocalClient);
				charge1.JR_APInvoiceDate = new ZDateTime(2009, 1, 3);

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARAPPostNonDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				AssertEquals("1 Charge", 1, job.Charges.Count);

				var customsDSBCharge = (Charge)job.Charges.Find(new ZQuery(JobChargeSchema.JR_AC, chargeCode.PK))[0];
				AssertEquals("INV123/CUSDSB", customsDSBCharge.JR_APInvoiceNum);
			}
		}

		[TestDate(2018, 07, 14)]
		public void TestAPInvoiceNumberDontIncludeChargeCodeForExistingCharge()
		{
			var chargeCode = SetChargeCode();
			var testCreditor = TestObjectCreator.ABIGAS;

			using (SetCustomsDisbursementDetailsInRegistry(testCreditor, chargeCode, Constants.CountryCodes.SouthAfrica))
			{
				GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;

				var customDetail = Factory.New<MockCustomsJobProvider>();
				customDetail.JobNumber = "TR436877";
				customDetail.Consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
				customDetail.Consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());

				var cusCharge = GetCustomCharge(chargeCode, 100, 0, true, testCreditor.PK);
				var chargeProvider = new MockCustomsChargesProvider
				{
					CustomsCharges = new ICustomsCharges[] { cusCharge },
					InvoiceNumber = "INV123",
					InvoiceDate = new ZDateTime(2005, 4, 12),
					CustomsJob = customDetail,
					Factory = Factory,
					APInvoiceNumberAlwaysIncludeChargeCode = false
				};
				customDetail.DataProviders.Add(chargeProvider);

				var job = CreateJob("Z00001000", LocalClient, 5m, Agent, 10M);
				job.JH_ParentID = customDetail.PK;

				var localCurrency = GlbCompany.CurrentCompany.LocalCurrency;

				var charge1 = CreateCharge(job, chargeCode, "CUSDSB", localCurrency, 300m, Creditor3, localCurrency, 350m, LocalClient);
				charge1.JR_APInvoiceDate = new ZDateTime(2009, 1, 3);

				var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARAPPostNonDSB | ChargePosterBehaviours.APPostDSB, new[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				Factory.Save();

				AssertEquals("1 Charge", 1, job.Charges.Count);

				var customsDSBCharge = (Charge)job.Charges.Find(new ZQuery(JobChargeSchema.JR_AC, chargeCode.PK))[0];
				AssertEquals("INV123", customsDSBCharge.JR_APInvoiceNum);
			}
		}

		void RaiseTestInvoice(MockCustomsJobProvider customDetail, ZGuid creditorPK)
		{
			var charge1 = GetCustomCharge(122, 0, true, creditorPK);
			customDetail.JobNumber = "TR436877";

			var chargeProvider = new MockCustomsChargesProvider
			{
				CustomsCharges = new ICustomsCharges[] { charge1 },
				InvoiceNumber = "TR436877",
				InvoiceDate = new ZDateTime(2005, 4, 12),
				CustomsJob = customDetail
			};

			var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB, new ZGuid[] { RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value });
			poster.RaiseInvoices(customDetail);
		}

		ZGuid SetupStaffMemberEmailAddress()
		{
			BusinessObjectFactory staffMemberFactory = new BusinessObjectFactory();
			GlbStaff currentStaffMember = staffMemberFactory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "PM"));
			currentStaffMember.GS_EmailAddress = EmailAddress;

			staffMemberFactory.Save();
			return currentStaffMember.PK;
		}

		protected ZString EmailAddress
		{
			get { return new ZString("blahblah@whatever.example"); }
		}

		ZQuery GetInvoiceFilter()
		{
			return GetInvoiceFilter("TR436877");
		}

		ZQuery GetInvoiceFilter(ZString transactionNumber)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, transactionNumber);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_InvoiceDate, new ZDateTime(2005, 4, 12));
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			return filter;
		}

		protected IDisposable SetCustomsDisbursementDetailsInRegistry(OrgHeader testCreditor, AccChargeCode chargeCode, string countryCode)
		{
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, testCreditor.PK.ToGuid());
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());

			return GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode);
		}

		protected AccChargeCode SetChargeCode()
		{
			AccTaxRate.LoadExistingOrCreateNewTaxRate(new BusinessObjectFactory(), "EXEMPT", AccTaxRate.Types.Exempt, 0).Factory.Save();

			ZQuery chargeFilter = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			chargeFilter.AddToFilter(AccChargeCodeSchema.AC_AG_CostAccount, SQLComparisonOperator.NotEqual, null);
			chargeFilter.AddToFilter(AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.Equal, Constants.ChargeType.Disbursement);
			return Factory.LoadTop1(typeof(AccChargeCode), chargeFilter) as AccChargeCode;
		}

		protected MockCustomCharge GetCustomCharge(AccChargeCode chargeCode, ZDecimal amount, ZDecimal gST, ZBool paidByBroker, ZGuid creditorPK, string entryReference = "")
		{
			CustomsCharge customCharge1 = new CustomsCharge(chargeCode, "TEST", amount, gST, paidByBroker, creditorPK);
			var charge1 = new MockCustomCharge();
			charge1.fIsActive = true;
			charge1.fCustomsCharges = new CustomsCharge[] { customCharge1 };
			customCharge1.EntryReference = entryReference;
			return charge1;
		}

		protected MockCustomCharge GetCustomCharge(ZDecimal amount, ZDecimal gST, ZBool paidByBroker, ZGuid creditorPK)
		{
			return GetCustomCharge(null, amount, gST, paidByBroker, creditorPK);
		}

		#region Non-Current Branch

		GlbBranch fNonCurrentBranch;
		protected GlbBranch NonCurrentBranch
		{
			get
			{
				if (fNonCurrentBranch == null)
				{
					fNonCurrentBranch = Factory.LoadTop1(typeof(GlbBranch), new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK)) as GlbBranch;
				}
				return fNonCurrentBranch;
			}
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();

			new AccountingPeriodTestHelper().SetupPeriods();
			TestObjectCreator.ABIGAS.OH_IsCreditor = true;
		}
	}
}
