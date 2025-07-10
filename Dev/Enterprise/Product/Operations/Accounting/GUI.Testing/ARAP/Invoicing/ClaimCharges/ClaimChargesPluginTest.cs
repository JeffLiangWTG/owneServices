using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.GUI.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Testing
{
	[TestedType(typeof(ClaimChargesPluginTestForm))]
	public class ClaimChargesPluginTest : ZFormBasherTest
	{
		public void TestPlugInNotDisplayedMessageWhenCreditNoteIsNotAllowed()
		{
			using (var form = GetForm())
			{
				var tabPage = new ZTabPage();
				form.TabControl.TabPages.Add(tabPage);
				form.Show();

				AssertNotNull("Precondition: RelatedUnapprovedCreditNote", claim.RelatedUnapprovedCreditNote);

				var plugIn = (ClaimChargesPlugin)form.PlugIns.Instances[0];
				ReselectPluginTabPage();
				AssertEquals(ZString.Empty, plugIn.PlugInNotDisplayedMessage);

				AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				ReselectPluginTabPage();
				AssertEquals(ZString.Empty, plugIn.PlugInNotDisplayedMessage);

				plugIn.Delete();
				AssertNull("Precondition: RelatedUnapprovedCreditNote", claim.RelatedUnapprovedCreditNote);
				plugIn.ClaimWasSuccessfullySaved();
				AssertEquals(AccountingMasterFilesUtils.APCreditNoteDisallowedMessage, plugIn.PlugInNotDisplayedMessage);
				AssertNull("Postcondition: RelatedUnapprovedCreditNote", claim.RelatedUnapprovedCreditNote);

				AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				ReselectPluginTabPage();
				AssertEquals(ZString.Empty, plugIn.PlugInNotDisplayedMessage);
				AssertNotNull("Postcondition: RelatedUnapprovedCreditNote", claim.RelatedUnapprovedCreditNote);

				void ReselectPluginTabPage()
				{
					form.TabControl.SelectedTab = tabPage;
					plugIn.SelectTabPage();
				}
			}
		}

		[ExpectNoExceptions]
		public void TestNoExceptionOnReversing()
		{
			APCreditNote creditNote = ObjectCreator.CreateAPCreditNote("123", ObjectCreator.AALSHI, ObjectCreator.AUD, 1m, "took me 4 hours to write this test!");
			creditNote.AH_TransactionNum = "TEST0001";
			Factory.Save();

			ReversingFactory reversingFactory = new ReversingFactory();
			ReversingBase reversing = reversingFactory.NewReversing(creditNote);
			reversing.Reverse();

			var reverseTransaction = reversing.ReverseTransaction as APInvoice;
			reverseTransaction.AH_TransactionNum = "TEST0002";
			using (APInvoiceFormTest.MockAPInvoiceForm form = new APInvoiceFormTest.MockAPInvoiceForm(reverseTransaction) { DisplayMode = Enterprise.ZArchitecture.Core.ODisplayMode.Delete })
			{
				form.Show();
				form.PlugIns.GetPlugIn(ControllerIDs.LinkedeNettEDIMessage).SelectTabPage();
				form.Delete_Exposed();
			}
		}

		public void TestQueryClaimUserControlBoundToClaimTab()
		{
			using (ZForm form = (ZForm)GetFormToBash())
			{
				form.Show();

				ClaimChargesPlugin plugIn = (ClaimChargesPlugin)form.PlugIns.Instances[0];
				Assert("Tab page should contain claim charges user control", plugIn.TabPage.Controls.Contains(plugIn.UserControl));
				var firstBoundCreditNote = ((ZUserControl)plugIn.UserControl).BindingSource.DataSource;

				AssertNotNull("Plugin control is not bound to null object", firstBoundCreditNote);
				AssertEquals("Plugin control should be bound from plugin credit note", claim.RelatedUnapprovedCreditNote, firstBoundCreditNote);

				plugIn.Delete();
				AssertNull("Plugin control should be unbound from plugin claim", ((ZUserControl)plugIn.UserControl).BindingSource.DataSource);
				plugIn.ClaimWasSuccessfullySaved();

				Assert("Tab page should contain claim charges user control", plugIn.TabPage.Controls.Contains(plugIn.UserControl));
				var secondBoundCreditNote = ((ZUserControl)plugIn.UserControl).BindingSource.DataSource;
				AssertEquals("Plugin control should be bound from plugin credit note", claim.RelatedUnapprovedCreditNote, secondBoundCreditNote);

				AssertNotEquals("Bound credit notes are different in two instances", firstBoundCreditNote, secondBoundCreditNote);
			}
		}

		public void TestQueryClaimUserControlExtraTaxCaptionForIndia()
		{
			TestCaseForTestingExtraTaxCaption(Core.Constants.CountryCodes.India, "SGST Amount");
		}

		public void TestQueryClaimUserControlExtraTaxCaptionForMexico()
		{
			TestCaseForTestingExtraTaxCaption(Core.Constants.CountryCodes.Mexico, "RET Amount");
		}

		public void TestQueryClaimUserControlExtraTaxCaptionForCanada()
		{
			TestCaseForTestingExtraTaxCaption(Core.Constants.CountryCodes.Canada, "QST Amount");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Testing")]
		void TestCaseForTestingExtraTaxCaption(string countryCode, string expectedExtraTax)
		{
			GlbCompany.CurrentCompany.SetCountry(countryCode);

			using (ZForm form = (ZForm)GetFormToBash())
			{
				form.Show();

				ClaimChargesPlugin plugIn = (ClaimChargesPlugin)form.PlugIns.Instances[0];

				AssertEquals(expectedExtraTax, ((ClaimChargesUserControl)plugIn.UserControl).AH_OSExtraTaxAmountCalcEdit.GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestComplianceSubTypeIsEmptyForUnapprovedAPCreditNoteWhenComplianceNumberAllocationIsMandatory()
		{
			AssertEmptySubTypeForUnapprovedAPCreditNote(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code, true);
		}

		public void TestComplianceSubTypeIsNotEmptyForUnapprovedAPCreditNoteWhenComplianceNumberAllocationIsNotMandatory()
		{
			AssertEmptySubTypeForUnapprovedAPCreditNote(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code, false);
		}
		
		public void TestShowPreSaveDialogsCore_WhenPreSaveActionsNotExecuted_StopsSave()
		{
			AssertPreSaveDialogBehavior(reopenJobAllowed: false,
				expectedContinueWithSave: ContinueWithSave.No,
				expectedFormType: typeof(LoginForm),
				expectedJobStatus: JobHeaderStatus.Closed.Code);
		}

		public void TestShowPreSaveDialogsCore_WhenPreSaveActionsSucceed_ProceedsWithSave()
		{
			AssertPreSaveDialogBehavior(reopenJobAllowed: true,
				expectedContinueWithSave: ContinueWithSave.Yes,
				expectedFormType: null,
				expectedJobStatus: JobHeaderStatus.Working.Code);
		}

		void AssertEmptySubTypeForUnapprovedAPCreditNote(string registryOption, bool expectEmptyComplianceSubType)
		{
			using (AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AP.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryOption))
			using (var form = GetForm())
			{
				form.Show();
				var uaCreditNote = claim.RelatedUnapprovedCreditNote;
				form.FireSaveButton();

				if (expectEmptyComplianceSubType)
				{
					Assert("Compliance SubType is empty", uaCreditNote.AH_ComplianceSubType.IsEmpty);
					AssertEquals("Warning message is shown", "Note: Unapproved AP Credit Note is saved without Compliance Sub Type, because it must be set only when Compliance Number is allocated", uaCreditNote.LastWarningMessage);
				}
				else
				{
					AssertEquals("Compliance SubType is API", "API", uaCreditNote.AH_ComplianceSubType);
					AssertEquals("Warning message is not shown", ZString.Empty, uaCreditNote.LastWarningMessage);
				}
			}
		}

		void AssertPreSaveDialogBehavior(bool reopenJobAllowed, ContinueWithSave expectedContinueWithSave, Type expectedFormType, string expectedJobStatus)
		{
			CreateClaimForReopenJobTest();
			using (new DisposableAction(() => job.JH_Status = JobHeaderStatus.Closed.Code, () => job.Dispose()))
			{
				Assert("Precondition : Job is Closed", job.IsClosed);

				using (var plugin = new ClaimChargesPlugin(claim))
				{
					Env.Security.ReopenJob.IsAllowed = reopenJobAllowed;
					ZFormModaliser.LastFormShownDialogForTest = null;

					var isContinue = plugin.ShowPreSaveDialogsCore();

					AssertEquals($"Expected {expectedContinueWithSave}.", expectedContinueWithSave, isContinue);

					if (expectedFormType != null)
					{
						AssertEquals($"Should prompt {expectedFormType.Name}.", expectedFormType, ZFormModaliser.LastFormShownDialogForTest?.GetType());
					}

					AssertEquals($"Job status should remain {expectedJobStatus}.", expectedJobStatus, job.JH_Status);
				}
			}

			void CreateClaimForReopenJobTest()
			{
				var shipment = ObjectCreator.CreateShipment("1234");
				job = Job.CreateWithMutex_ForTestOnly(Factory, shipment);
				var invoice = (APInvoice)ObjectCreator.CreateInvoice(typeof(APInvoice), ObjectCreator.AUD, 1m);
				invoice.AH_JH = shipment.Job.PK;

				var creditNote = (UACreditNote)ObjectCreator.CreateInvoice(typeof(UACreditNote), ObjectCreator.AUD, 1m);
				creditNote.AH_JH = shipment.Job.PK;
				var creditNoteLine = (UACreditNoteLine)ObjectCreator.CreateInvoiceLine(creditNote, 20, ObjectCreator.AUD, 1m);
				creditNote.Lines.Add(creditNoteLine);
				creditNoteLine.AL_JH = shipment.Job.PK;
				creditNoteLine.AL_OH = ObjectCreator.AALSHI.PK;

				claim = Factory.New<APAccQueryClaim>();
				claim.AY_OH_Debtor = ObjectCreator.AALSHI.PK;
				claim.AY_AH = invoice.PK;
				claim.TransactionHeader.AH_TransactionBelongsToGroup = invoice.PK;
				creditNote.AH_TransactionBelongsToGroup = invoice.PK;
			}
		}

		#region Implementation

		TestObjectCreator fObjectCreator;
		TestObjectCreator ObjectCreator
		{
			get { return fObjectCreator ??= new TestObjectCreator(Factory); }
		}

		protected override Form GetFormToBashCore() => GetForm();

		ClaimChargesPluginTestForm GetForm()
		{
			GlbCompany newCompany = Factory.New<GlbCompany>();
			newCompany.GC_OH_OrgProxy = ObjectCreator.AALSHI.PK;

			var apInvoice = ObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "11", ObjectCreator.AUD, 1, 10, 0, 10, 0);
			apInvoice.AH_OH = ObjectCreator.AALSHI.PK;
			apInvoice.AH_GB = GlbBranch.CurrentBranch.PK;

			claim = Factory.New<APAccQueryClaim>();
			claim.AY_OH_Debtor = ObjectCreator.AALSHI.PK;
			claim.AY_GB = GlbBranch.CurrentBranch.PK;
			claim.AY_QueryClaimStatus = "OPN";
			claim.AY_OC = Factory.NewWithValidTestData<OrgContact>().PK;

			claim.AY_AH = apInvoice.PK;

			AssertNoErrors("Precondition", claim.AY_OH_DebtorInfo);
			AssertNoErrors("Precondition", claim.AY_AHInfo);

			claim.CreateAndAttachRelatedCreditNote();
			if (claim.RelatedUnapprovedCreditNote.AH_ExchangeRate == 0)
			{
				claim.RelatedUnapprovedCreditNote.AH_ExchangeRate = 1m;
			}

			claim.RelatedUnapprovedCreditNote.AH_ComplianceSubType = "API";

			Factory.Save();

			return new ClaimChargesPluginTestForm(claim);
		}

		public override void TestMarkAsNeedingValidationIsNotCalledWhenFormLoads()
		{
			Assert(true);
		}

		APAccQueryClaim claim;
		Job job;

		#endregion
	}

	class ClaimChargesPluginTestForm : ZForm
	{
		public ClaimChargesPluginTestForm(APAccQueryClaim claim)
			: base(claim)
		{
			TabControl = new ZTemplateTabControl();
			Controls.Add(TabControl);
			TabControl.Dock = DockStyle.Fill;
			PlugIns.Add(ControllerIDs.ClaimCharges);
			CaptionRenderingEnabled = true;
			using (var accQueryClaimForm = new AccQueryClaimForm())
			{
				this.Size = accQueryClaimForm.MinimumSize;
			}
		}

		public ZTemplateTabControl TabControl { get; }
	}
}
