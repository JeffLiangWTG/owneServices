using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Testing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Client.EDI.DocManager.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(EDIARInvoice))]
	internal class EDIARInvoiceTest : ARInvoiceTest
	{
		public void TestTypeDecidingTheCorrectType()
		{
			EDIARInvoice invoice = Factory.NewWithValidTestData<EDIARInvoice>();
			Factory.Save();

			BusinessObject newInvoice = new BusinessObjectFactory().Load<InvoicingBase>(invoice.PK);
			AssertEquals("If this fails, check the base class. It has to be subclassed from AR invoice", typeof(EDIARInvoice), newInvoice.GetType());
		}

		public void TestOnSaving()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "developer@cargowise.com";
			ProfessionalServicesQuote psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			psq.IM_GS_NKCustServiceContact = staff.GS_Code;
			EDIARInvoice invoice = GetValidInvoice("jobNum1", 20);
			invoice.Job.JH_ParentID = psq.PK;
			invoice.Job.JH_ParentTableCode = IncidentMainSchema.Constants.Prefix;

			AccTransactionMatchLink link = ((IMatching)invoice).CurrentMatchGroup.AddNew();
			link.AP_AH = invoice.PK;
			link.AP_Amount = 10M;
			invoice.AH_OutstandingAmount = 10M;

			AccTransactionHeader headerToMatch = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerToMatch.AH_InvoiceAmount = -10M;

			AccTransactionMatchLink linkToMatch = ((IMatching)invoice).CurrentMatchGroup.AddNew();
			linkToMatch.AP_AH = headerToMatch.PK;
			linkToMatch.AP_Amount = -10M;
			TestObjectCreator.SetupMatchLinkMatchDate(invoice);

			Factory.Save();
			AssertEquals("Emails sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Email sent to:", true, email.Recipients.Contains("developer@cargowise.com"));

			Env.OutgoingMailManager.EmailsCreated.Clear();
			invoice = invoice = GetValidInvoice("jobNum2", 45);
			ProfessionalServicesQuote incident = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			incident.IM_GS_NKCustServiceContact = staff.GS_Code;
			invoice.Job.JH_ParentID = incident.PK;
			invoice.Job.JH_ParentTableCode = IncidentMainSchema.Constants.Prefix;
			Factory.Save();
			AssertEquals("No emails sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		[TestedType(typeof(EDIARInvoice))]
		public class EDIARInvoiceMatchingTest : InvoicingBaseMatchingTest
		{
			protected override InvoicingBase GetNewInvoice()
			{
				return Factory.New<EDIARInvoice>();
			}
		}

		#region Implementation

		EDIARInvoice GetValidInvoice(ZString jobNum, ZDecimal amount)
		{
			EDIARInvoice invoice = Factory.New<EDIARInvoice>();

			ARInvoiceLine line = Factory.New<ARInvoiceLine>();
			invoice.Lines.Add(line);
			line.AL_AG = Factory.LoadTop1<AccGLHeader>(new ZQuery()).PK;
			line.AL_LineType = Enterprise.ZArchitecture.Core.TransactionLineTypes.Revenue;
			line.AL_OSExTaxAmount = amount;

			invoice.AH_FullyPaidDate = ZDateTime.Empty;
			invoice.AH_OutstandingAmount = invoice.AH_InvoiceAmount + invoice.AH_GSTAmount;

			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_JobNum = jobNum;
			invoice.AH_JH = job.PK;

			Factory.Save();

			return invoice;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<EDIARInvoice>();
		}

		protected override Type GetExpectedDocManagerInfoType()
		{
			return typeof(EDIARInvoiceDocManagerInfo);
		}

		protected override Type TypeOfValidation
		{
			get { return typeof(EdiInvoiceValidation); }
		}

		#endregion
	}

	[TestedType(typeof(EDIARInvoice))]
	internal class EDIARInvoiceIUnmatchDateSupporterTest : ARInvoiceIUnmatchDateSupporterTest
	{
	}
}
