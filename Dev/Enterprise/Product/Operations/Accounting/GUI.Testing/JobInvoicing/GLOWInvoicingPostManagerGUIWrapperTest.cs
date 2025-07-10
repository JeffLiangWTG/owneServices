using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Accounting.GUI.JobInvoicing.GLOWInvoicingPostManagerGUIWrapper;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	public class GLOWInvoicingPostManagerGUIWrapperTest : PostManagerGUIWrapperTest
	{
		public new void TestBackDateARInvoicesWithRegistryEnabledAndUserAnsweringCancel()
		{
			Assert("There is no such case to say cancel for GLOW web service", true);
		}

		public new void TestPaymentRequisitionStatusOverrideWithRegistryAndSecurityAllowed()
		{
			Assert("The functionality is not implemented here.", true);
		}

		protected override void AssertOverrideTransactionDescription(bool shouldOverride)
		{
			Assert("No implemented here.", true);
		}

		public override void TestPostWithInvoicePostingExchangeRateOption()
		{
			Assert("N/a", true);
		}
		public override void TestBackDatingARAPInvoiceWithInvoicePostingExchangeRateOption()
		{
			Assert("N/a", true);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Create2MonthPeriod();
			Job job = Job1;
			Factory.Save();
		}

		bool shouldPerformBackDating;
		DateTime postDate;
		DateTime invoiceDate;

		protected override void InitializeBackDateARInvoiceWithBackDateDialogResultYes()
		{
			shouldPerformBackDating = true;
			postDate = invoiceDate = ZDateTime.Now.ToDateTime();
		}

		protected override void AssertInvoiceDatesOnPosting(ZDateTime expectedInvoiceDate, ZDateTime expectedPostDate, ZDateTime lastMonthDate, ZDateTime expectedDueDate)
		{
			GUIWrapper.Post();

			AssertEquals("Should have created one invoice", 1, ((GLOWInvoicingPostManagerGUIWrapper)GUIWrapper).PostManager_ForTestOnly.Poster.PostedInvoices.Count);
			InvoicingBase invoice = ((GLOWInvoicingPostManagerGUIWrapper)GUIWrapper).PostManager_ForTestOnly.Poster.PostedInvoices[0];
			AssertNull("Shouldn't have shown question about back dating", ZFormModaliser.LastFormShownDialogForTest);
			if (shouldPerformBackDating)
			{
				AssertEquals("Should have posted invoice correct invoice date", invoiceDate, invoice.AH_InvoiceDate.Date);
				AssertEquals("Should have posted invoice correct post date", postDate, invoice.AH_PostDate.Date);
			}
			else
			{
				AssertEquals("Should have posted invoice correct invoice date", expectedInvoiceDate, invoice.AH_InvoiceDate.Date);
				AssertEquals("Should have posted invoice correct post date", expectedPostDate, invoice.AH_PostDate.Date);
				AssertEquals("Should have posted invoice correct due date", expectedDueDate, invoice.AH_DueDate.Date);
			}
		}

		protected override PostManagerGUIWrapper GUIWrapper
		{
			get
			{
				if (GUIWrapper_inner == null)
				{
					GUIWrapper_inner = new GLOWInvoicingPostManagerGUIWrapper(Job1.PK.ToGuid(), JobInvoicingPostingOption.All, shouldPerformBackDating, postDate, invoiceDate, Factory);
					GUIWrapper.DoTestPostTransactions = true;
				}
				return GUIWrapper_inner;
			}
		}
		GLOWInvoicingPostManagerGUIWrapper GUIWrapper_inner;

		#region NothingPostedHandler

		protected override PostManagerGUIWrapper PrepareTestDataForNothingPostedHandler()
		{
			var shipment = TestObjectCreator.CreateShipment("S001", saveIt: true);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "zero amount charge", TestObjectCreator.AUD, 0m, null, TestObjectCreator.AUD, 0M, TestObjectCreator.LocalClient);

			Factory.Save();

			var wrapper = new GLOWInvoicingPostManagerGUIWrapper(job.PK.ToGuid(), JobInvoicingPostingOption.All, shouldPerformBackDating, postDate, invoiceDate, Factory);
			wrapper.DoTestPostTransactions = true;

			return wrapper;
		}

		protected override void AssertNothingPostedHandlerCore(PostManagerGUIWrapper wrapper)
		{
			AssertExceptionThrown<GlowErrorReportException>("Should contain expected Nothing Posted Message", GetExpectedMessageForNothingPosted(), () => wrapper.Post());
		}

		#endregion

		#endregion
	}
}
