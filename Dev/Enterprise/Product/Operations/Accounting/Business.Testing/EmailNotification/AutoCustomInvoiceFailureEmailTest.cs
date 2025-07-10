using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Testing;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public class AutoCustomInvoiceFailureEmailTest : MasterFiles.Business.Testing.AccountingEmailDefTest
	{
		public void TestSubject()
		{
			AssertEquals("Email Subject not correctly set.", "Auto-Billing failure for Declaration TST01", TestEmail.GetSubject(true, "TST01", "Declaration"));
			AssertEquals("Email Subject not correctly set.", "Auto-Billing result for Declaration TST01", TestEmail.GetSubject(false, "TST01", "Declaration"));
		}

		public void TestShouldSendEmailIfSomethingToNotify()
		{
			ZGuid staffPK = SetupStaffMemberEmailAddress();
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Guid.Empty);
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CustomsCharge.PK.ToGuid());
			Factory.Save();
			MockCustomsJobProvider customDetail = Factory.New<MockCustomsJobProvider>();
			customDetail.AutoPostingNotification = new AutoPostingNotification(new ZGuid[] { staffPK }, false);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			RaiseTestInvoice(customDetail);
			AssertEquals("Email should have been sent if registry check fail", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(true, Env.OutgoingMailManager.EmailsCreated[0].Body.Contains(CustomsDSBChargePostValidator.GetCustomsDisbursementCreditorNotSet(Env.CurrentCompany.Code)));
		}

		public void TestShouldNotSendEmailIfNothingToNotify()
		{
			SetupStaffMemberEmailAddress();
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CustomsCreditor.PK.ToGuid());
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CustomsCharge.PK.ToGuid());
			Factory.Save();
			MockCustomsJobProvider customDetail = Factory.New<MockCustomsJobProvider>();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			RaiseTestInvoice(customDetail);
			AssertEquals("Email should not have been sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestBodyIfAdditionalErrorMsgPassed()
		{
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CustomsCreditor.PK.ToGuid());
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CustomsCharge.PK.ToGuid());
			CUSDSBPostingValidationResult result = new CUSDSBPostingValidationResult("1", new AutoPostingNotification(Array.Empty<ZGuid>(), false));
			result.AddError("Error 1");
			result.AddError("Error 2");
			List<CUSDSBPostingValidationResult> results = new List<CUSDSBPostingValidationResult>();
			results.Add(result);
			string body = TestEmail.GetBody(true, results, "", ZGuid.NewZGuid(), DummyControllerIDs.Dummy, "Declaration");
			string expected = "The following errors were encountered while trying to automatically process billing charges.";
			AssertEquals(true, body.Contains(expected));
			AssertEquals(true, body.Contains("Error 1"));
			AssertEquals(true, body.Contains("Error 2"));
		}

		AccChargeCode SetChargeCode()
		{
			ZQuery chargeFilter = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			chargeFilter.AddToFilter(AccChargeCodeSchema.AC_AG_CostAccount, SQLComparisonOperator.NotEqual, null);
			chargeFilter.AddToFilter(AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.Equal, Constants.ChargeType.Disbursement);
			return Factory.LoadTop1(typeof(AccChargeCode), chargeFilter) as AccChargeCode;
		}

		public void TestGenerateErrorMessageForExistingAPInvoice()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();
			ZGuid staffPK = SetupStaffMemberEmailAddress();
			AccChargeCode chargeCode = SetChargeCode();
			OrgHeader testCreditor = Factory.LoadTop1<OrgHeader>(new ZQuery());
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, testCreditor.PK.ToGuid());
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				MockCustomsJobProvider customDetail = Factory.New<MockCustomsJobProvider>();
				MockCustomCharge cusCharge = GetCustomCharge(testObjectCreator.CC1, 122, 0, true);
				cusCharge.AddCustomsCharge(new CustomsCharge(testObjectCreator.CC2, "TEST", 200, 0, true, testCreditor.PK));
				customDetail.JobNumber = "TR436877";
				customDetail.AutoPostingNotification = new AutoPostingNotification(new ZGuid[] { staffPK }, false);
				Job job = new Job.Loader(customDetail).TryCreateWithoutMutexForTestOnly();
				job.JH_OA_AgentCollectAddr = testObjectCreator.Agent.MainAddress.PK;
				job.JH_OA_LocalChargesAddr = testObjectCreator.LocalClient.MainAddress.PK;
				testObjectCreator.CreateCharge(job, testObjectCreator.CC3, 100, 200);
				APInvoice existingInvoice = Factory.NewWithValidTestData<APInvoice>();
				existingInvoice.AH_OH = testCreditor.PK;
				existingInvoice.AH_TransactionNum = "ABC123DEF";
				APInvoiceLine lineCharge1 = existingInvoice.Lines.AddNew() as APInvoiceLine;
				lineCharge1.AL_AG = testObjectCreator.GLHeader1.PK;
				lineCharge1.FillWithValidTestData();
				lineCharge1.AL_AC = testObjectCreator.CC1.PK;
				lineCharge1.AL_JH = job.PK;
				lineCharge1.AL_Desc = "DESC1\r\n ADDDESC11\r\n ADDDESC12";
				lineCharge1.AL_OSExTaxAmount = 120m;
				lineCharge1.AL_OSTaxAmount = 0m;
				APInvoiceLine lineCharge2 = existingInvoice.Lines.AddNew() as APInvoiceLine;
				lineCharge2.AL_AG = testObjectCreator.GLHeader1.PK;
				lineCharge2.FillWithValidTestData();
				lineCharge1.AL_AC = testObjectCreator.CC2.PK;
				lineCharge2.AL_Desc = "DESC2\r\n ADDDESC21\r\n";
				lineCharge2.AL_OSExTaxAmount = 200m;
				lineCharge2.AL_OSTaxAmount = 0m;
				lineCharge2.AL_JH = job.PK;
				testObjectCreator.CreateJobCharge(lineCharge1, job, testObjectCreator.CC1, testObjectCreator.AUD);
				testObjectCreator.CreateJobCharge(lineCharge2, job, testObjectCreator.CC1, testObjectCreator.AUD);
				Factory.Save();
				MockCustomsChargesProvider chargeProvider = new MockCustomsChargesProvider();
				chargeProvider.CustomsCharges = new ICustomsCharges[] { cusCharge };
				chargeProvider.InvoiceNumber = "ABC123DEF";
				chargeProvider.InvoiceDate = new ZDateTime(2005, 4, 12);
				chargeProvider.CustomsJob = customDetail;
				chargeProvider.Factory = Factory;
				chargeProvider.AutoPostingNotification = new AutoPostingNotification(new ZGuid[] { staffPK }, false);
				customDetail.DataProviders.Add(chargeProvider);
				Env.OutgoingMailManager.EmailsCreated.Clear();
				ChargePosterBehaviours actions = ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.SendEmail;
				CustomsDisbursementChargePoster poster = new CustomsDisbursementChargePoster(actions, new ZGuid[] { chargeCode.PK });
				poster.RaiseInvoices(customDetail);
				AssertEquals("one email should have been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
				Factory.Save();
				string body = email.Body;
				string expected = "There are differences in the details for these charges, please review the differences below. You may have to reverse the existing invoice manually and re-enter with correct details.";
				AssertEquals("Error message", true, body.Contains(expected));
				expected = @"
<th>CHARGE CODE</th><th>DESCRIPTION</th><th>AP INV. AMOUNT</th><th>CUSTOMS AMOUNT</th><th>DIFFERENCE</th></tr></thead>
<tr><td>ZZCC2</td><td>DESC1</td><td>120.00 *</td><td>200.00 *</td><td>80.00</td></tr>
<tr><td>&nbsp;</td><td> ADDDESC11</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>
<tr><td>&nbsp;</td><td> ADDDESC12</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>
<tr><td>ZZCC1</td><td>&nbsp;</td><td>0.00 *</td><td>122.00 *</td><td>122.00</td></tr>
<tr><td>&nbsp;</td><td>  TEST                                      122.00</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>
<tr><td>TOTAL</td><td>&nbsp;</td><td>120.00 *</td><td>322.00 *</td><td>202.00</td></tr></table>";
				AssertEquals("should contain a table for difference", true, body.Contains(expected.Replace("\r\n", "")));
			}
		}

		public void TestDoNotGenerateErrorMessageForExistingARInvoiceWhenCreditNoteIsIssued()
		{
			ZGuid staffPK = SetupStaffMemberEmailAddress();
			AccChargeCode chargeCode = SetChargeCode();
			OrgHeader testCreditor = Factory.LoadTop1<OrgHeader>(new ZQuery());
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, testCreditor.PK.ToGuid());
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());
			Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).SetCountry(Constants.CountryCodes.SouthAfrica);
			MockCustomsJobProvider customDetail = Factory.New<MockCustomsJobProvider>();
			MockCustomCharge cusCharge = GetCustomCharge(chargeCode, 100, 0, true);
			customDetail.JobNumber = "TR436877";
			customDetail.AutoPostingNotification = new AutoPostingNotification(new ZGuid[] { staffPK }, false);
			var job = new Job.Loader(customDetail).TryCreateWithoutMutexForTestOnly();
			var existingInvoice = Factory.NewWithValidTestData<ARInvoice>();
			existingInvoice.AH_OH = testCreditor.PK;
			existingInvoice.AH_TransactionNum = "ABC123DEF";
			existingInvoice.AH_TransactionType = TransactionTypes.Invoice;
			var lineCharge1 = existingInvoice.Lines.AddNew() as ARInvoiceLine;
			lineCharge1.FillWithValidTestData();
			lineCharge1.AL_AC = chargeCode.PK;
			lineCharge1.AL_JH = job.PK;
			lineCharge1.AL_Desc = "DESC1\r\n ADDDESC11\r\n ADDDESC12";
			lineCharge1.AL_OSExTaxAmount = 120m;
			lineCharge1.AL_OSTaxAmount = 0m;
			var existingInvoice2 = Factory.NewWithValidTestData<ARInvoice>();
			existingInvoice2.AH_OH = testCreditor.PK;
			existingInvoice2.AH_TransactionNum = "ABC123DEF";
			existingInvoice2.AH_TransactionType = TransactionTypes.CreditNote;
			var lineCharge2 = existingInvoice2.Lines.AddNew() as ARInvoiceLine;
			lineCharge2.FillWithValidTestData();
			lineCharge2.AL_AC = chargeCode.PK;
			lineCharge2.AL_Desc = "DESC2\r\n ADDDESC21\r\n";
			lineCharge2.AL_OSExTaxAmount = -20m;
			lineCharge2.AL_OSTaxAmount = 0m;
			lineCharge2.AL_JH = job.PK;
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateJobCharge(lineCharge1, job, testObjectCreator.CC1, testObjectCreator.AUD);
			testObjectCreator.CreateJobCharge(lineCharge2, job, testObjectCreator.CC1, testObjectCreator.AUD);
			Factory.Save();
			MockCustomsChargesProvider chargeProvider = new MockCustomsChargesProvider();
			chargeProvider.CustomsCharges = new ICustomsCharges[] { cusCharge };
			chargeProvider.InvoiceNumber = "ABC123DEF";
			chargeProvider.InvoiceDate = new ZDateTime(2005, 4, 12);
			chargeProvider.CustomsJob = customDetail;
			chargeProvider.Factory = Factory;
			chargeProvider.AutoPostingNotification = new AutoPostingNotification(new ZGuid[] { staffPK }, false);
			customDetail.DataProviders.Add(chargeProvider);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			ChargePosterBehaviours actions = ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.SendEmail;
			CustomsDisbursementChargePoster poster = new CustomsDisbursementChargePoster(actions, new ZGuid[] { chargeCode.PK });
			poster.RaiseInvoices(customDetail);
			AssertEquals("no email should have been sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		MockCustomCharge GetCustomCharge(AccChargeCode chargeCode, ZDecimal amount, ZDecimal gST, ZBool paidByBroker)
		{
			CustomsCharge customCharge1 = new CustomsCharge(chargeCode, "TEST", amount, gST, paidByBroker, CustomsCreditor.PK);
			MockCustomCharge charge1 = new MockCustomCharge();
			charge1.fIsActive = true;
			charge1.fCustomsCharges = new CustomsCharge[] { customCharge1 };
			return charge1;
		}

		ZGuid SetupStaffMemberEmailAddress()
		{
			BusinessObjectFactory staffMemberFactory = new BusinessObjectFactory();
			GlbStaff currentStaffMember = staffMemberFactory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "PM"));
			currentStaffMember.GS_EmailAddress = EmailAddress;
			staffMemberFactory.Save();
			return currentStaffMember.PK;
		}

		void RaiseTestInvoice(MockCustomsJobProvider customDetail)
		{
			var customCharge1 = new CustomsCharge(CustomsCharge.PK, "TEST", 10m, 20m, true, CustomsCreditor.PK);
			var charge1 = new MockCustomCharge();
			charge1.fIsActive = true;
			charge1.fCustomsCharges = new CustomsCharge[] { customCharge1 };
			customDetail.JobNumber = "TR436877";
			var poster = new CustomsDisbursementChargePoster(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.SendEmail, new ZGuid[] { CustomsCharge.PK });
			poster.RaiseInvoices(customDetail);
		}

		protected ZString EmailAddress
		{
			get
			{
				return new ZString("blahblah@whatever.example");
			}
		}

		AutoCustomInvoiceFailureEmail fTestEmail;
		protected AutoCustomInvoiceFailureEmail TestEmail
		{
			get
			{
				if (fTestEmail == null)
				{
					fTestEmail = new AutoCustomInvoiceFailureEmail();
				}

				return fTestEmail;
			}
		}

		protected AccChargeCode CustomsCharge
		{
			get
			{
				if (fCustomsCharge == null)
				{
					fCustomsCharge = Factory.NewWithValidTestData(typeof(AccChargeCode)) as AccChargeCode;
				}

				return fCustomsCharge;
			}
		}

		AccChargeCode fCustomsCharge;
		protected OrgHeader CustomsCreditor
		{
			get
			{
				if (fCustomsCreditor == null)
				{
					fCustomsCreditor = Factory.NewWithValidTestData(typeof(OrgHeader)) as OrgHeader;
				}

				return fCustomsCreditor;
			}
		}

		OrgHeader fCustomsCreditor;
		protected GlbCompany TestCompany
		{
			get
			{
				if (fTestCompany == null)
				{
					fTestCompany = Factory.NewWithValidTestData(typeof(GlbCompany)) as GlbCompany;
					fTestCompany.GC_Code = "DEM";
				}

				return fTestCompany;
			}
		}

		protected GlbBranch TestBranch
		{
			get
			{
				if (fTestBranch == null)
				{
					fTestBranch = Factory.NewWithValidTestData(typeof(GlbBranch)) as GlbBranch;
					fTestBranch.GB_Code = "DEM";
				}

				return fTestBranch;
			}
		}

		protected GlbDepartment TestDepartment
		{
			get
			{
				if (fTestDepartment == null)
				{
					fTestDepartment = Factory.NewWithValidTestData(typeof(GlbDepartment)) as GlbDepartment;
					fTestDepartment.GE_Code = "DEP";
				}

				return fTestDepartment;
			}
		}

		GlbCompany fTestCompany;
		GlbBranch fTestBranch;
		GlbDepartment fTestDepartment;
		protected override Type EmailDefType
		{
			get
			{
				return typeof(AutoCustomInvoiceFailureEmail);
			}
		}
	}
}
