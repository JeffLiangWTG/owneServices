using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.ARAP.AutoAllocationAndPrinting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.ARAP.HotCheque
{
	public partial class AccHotChequeForm : ZForm, IButtonDeleteTextOverride, IDoDisplayModeDeleteOverride
	{
		public AccHotChequeForm(AccHotCheque bO)
			: base(bO)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);

			ZFormMenuStrategy.SetMenuItemText(this, ZFormMenuStrategy.FileSaveAndCloseMenuItemName, Res.GetString("Accounting|AccHotChequeForm|FileSaveAndCloseMenuItemName", "&Post"));
		}

		#region ZForm Overrides

		public override string FormVerb
		{
			get
			{
				string verb = base.FormVerb;

				if (BusinessEntityForHasChanges != null && DisplayMode == ODisplayMode.Delete)
				{
					verb = Res.GetString("9DE94CB6-9BA3-4e74-9D0C-F17B84259736", "Cancel");
				}

				return verb;
			}
		}

		string IButtonDeleteTextOverride.DeleteButtonText
		{
			get { return Res.GetString("Accounting|AccHotChequeForm|DeleteButton", "Cancel"); }
		}

		void IDoDisplayModeDeleteOverride.DoDisplayModeDelete()
		{
			ZFormStrategy.DoDisplayModeDelete(this);
			PostingButtonsUserControl.CloseButton.Text = ZFormPostingButtonsStrategy.DefaultCloseButtonText;
		}

		#region HandleSaveException

		protected override void HandleSaveException(Exception e)
		{
			// This hack resolves a problem whereby the user is able to edit a posted a hot cheque.
			//
			// To reproduce, comment out this method and:
			// 1. Make a new hot cheque. Save, but leave this form (AccHotChequeForm) open.
			// 2. Create a new payment using the hot cheque from (1), and post it.
			// 3. Return to this form. It is still editable even though posted.
			//
			// This is because the Payment form is an OForm, whilst this is from Z;
			// there is no data refresh bus between the two.
			if (e is ZSaveConcurrencyException)
			{
				var castEntity = BusinessEntity as AccHotCheque;
				var savedCheque = castEntity != null ? (new BusinessObjectFactory()).Load<AccHotCheque>(castEntity.PK) : null;

				if (savedCheque != null && savedCheque.AQ_Calc_ChequeStatus == AccHotCheque.POSTED)
				{
					// Do NOT 'handle' the concurrency issue by merging; it shouldn't be editable in the first place.
					Globals.Message.ShowInformation(Res.GetString("7c58d23e-120d-4d3a-ba57-a3f3623a031a", "This check has just been posted; it can no longer be edited."), Res.GetString("01019975-d89e-42a0-bf96-64764c6454b4", "This Check Was Posted"));
					((BusinessObject)BusinessEntity).Reload();
				}
				else
				{
					base.HandleSaveException(e);
				}
			}
			else if (e is AllocationSaveException)
			{
				Globals.Message.ShowError(((AllocationSaveException)e).UserFriendlyMessage, Res.GetString("f85da81e-9d2c-4e12-9f6a-cd15b6122a09", "Check Book Busy"));
			}
			else if (e is AllocationChequeBookException)
			{
				Globals.Message.ShowError(((AllocationChequeBookException)e).UserFriendlyMessage, Res.GetString("6ee634d8-b749-4f33-980f-ef3b1af0a0c1", "Check Book Full"));
			}
			else
			{
				base.HandleSaveException(e);
			}
		}

		#endregion

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			bool allowMaximumAmount = AccountingConfigurationRegistry.Instance.AccountingAllowUserToEnterMaximumAmount.Value;
			AQ_Calc_ActualAmountIndicatorBoundRadioButton.Enabled = allowMaximumAmount;
			AQ_Calc_MaximumAmountIndicatorBoundRadioButton.Enabled = allowMaximumAmount;
		}

		protected override void DeleteCore()
		{
			if (Cheque != null)
			{
				Cheque.AQ_Cancelled = true;
			}
		}

		AccHotCheque Cheque
		{
			get
			{
				return BusinessEntity as AccHotCheque;
			}
		}

		protected override void Save(ITransactionParticipant[] factories)
		{
			if (((IChequeNumberAutoAllocation)Cheque).IsAutoAllocationEnabled && !Cheque.IsInDatabase)
			{
#if DEBUG
				if (Globals.IsTest)
				{
					Test_Allocator = new PaymentChequeNumberAllocator.DummyPaymentChequeNumberAllocator(Cheque, PaymentChequeNumberAllocator.PrintingMode.HotCheque, Cheque.Factory);
					Test_Allocator.SetChequeBookToInactiveOnSaving = Test_DeactivateChequeBookOnAllocation;
					base.Save(Test_Allocator.GetFactoriesForTest());
				}
				else
				{
#endif
					var allocator = new PaymentChequeNumberAllocator(Cheque, PaymentChequeNumberAllocator.PrintingMode.HotCheque, Cheque.Factory);
					base.Save(allocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving(factories));
#if DEBUG
				}
#endif
			}
			else
			{
				base.Save(factories);
			}
		}

		protected override DialogResult ShowConfirmationForDelete()
		{
			string question = Res.GetString("1503c97c-bb25-404f-852f-6808eb261237", "You are about to cancel this check. Do you want to proceed?");
			return Globals.Message.Show(question, Res.GetString("656044e7-d0b2-466c-8444-96cf55cde563", "Cancel Confirmation"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#endregion

		#region For Testing
#if DEBUG

		public Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl_Exposed
		{
			get
			{
				if (!Globals.IsTest)
				{
					throw new ZException("This property is for testing only.");
				}

				return PostingButtonsUserControl;
			}
		}

		public ZTextBox AQ_DescriptionBoundTextBox_Exposed
		{
			get
			{
				if (!Globals.IsTest)
				{
					throw new ZException("This property is for testing only.");
				}

				return AQ_DescriptionBoundTextBox;
			}
		}

		public ZBool Test_DeactivateChequeBookOnAllocation;
		public PaymentChequeNumberAllocator.DummyPaymentChequeNumberAllocator Test_Allocator;

#endif
		#endregion
	}
}

