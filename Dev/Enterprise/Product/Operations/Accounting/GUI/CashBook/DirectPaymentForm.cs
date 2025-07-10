using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.GUI.ARAP.AutoAllocationAndPrinting;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.GUI.CashBook
{
	public partial class DirectPaymentForm : DirectCashBookBaseForm
	{
		public DirectPaymentForm(DirectPayment directPaymentBizO)
			: base(directPaymentBizO)
		{
			fDirectPaymentBizO = directPaymentBizO;
		}

		#region Print Remittance

		protected virtual void PrintReport()
		{
			PrintManager.Print();
		}

		#endregion

		#region Override

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (this.DisplayMode == ODisplayMode.ReadOnly)
			{
				HideChequeControl();
			}
		}

		protected override void HandleSaveException(Exception e)
		{
			if (e is AllocationSaveException)
			{
				Globals.Message.ShowError(((AllocationSaveException)e).UserFriendlyMessage, Res.GetString("ecd8ccd3-9eca-462c-bf90-b0e324c168aa", "Check Book Busy"));
			}
			else if (e is AllocationChequeBookException)
			{
				Globals.Message.ShowError(((AllocationChequeBookException)e).UserFriendlyMessage, Res.GetString("398c595c-cc17-4439-a812-9b2de6dd3e6a", "Check Book Full"));
			}
			else
			{
				base.HandleSaveException(e);
			}
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			ContinueWithSave result;
			bool isNew = !fDirectPaymentBizO.IsInDatabase;
			ValidateAll(ValidationType.Light);
			if (BusinessEntity.HasErrors())
			{
				result = ContinueWithSave.No;
				ShowErrorsDialog();
			}
			else
			{
				result = isNew && MessageHelper.ShowMessageIfChequeBookUsesSamePrinterReturnsCancel(((DirectPayment)BusinessEntity).ChequeBook) ? ContinueWithSave.No : ContinueWithSave.Yes;
			}

			if (result == ContinueWithSave.Yes)
			{
				result = base.ValidateAndSave();
			}

			if (result == ContinueWithSave.Yes && isNew)
			{
				if (Globals.Message.Show(Res.GetString("d9c4519d-9621-4185-90a3-da7e442326ba", "Do you want to print Remittance Advice?"), Res.GetString("6baaa1c5-489d-470e-9214-750002c7f6f6", "Direct Payment"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					PrintReport();
				}
			}
			return result;
		}

		ZLabel AutoPrintZLabel;
		ZLabel AutoAllocateZLabel;

		AccountingMessageHelper MessageHelper
		{
			get
			{
				if (fMessageHelper == null)
				{
					fMessageHelper = new AccountingMessageHelper();
				}
				return fMessageHelper;
			}
		}
		AccountingMessageHelper fMessageHelper;

		protected override void SaveCore(ITransactionParticipant[] factories)
		{
			if (!fDirectPaymentBizO.IsInDatabase && ((IChequeNumberAutoAllocation)fDirectPaymentBizO).IsAutoAllocationEnabled)
			{
#if DEBUG
				if (Globals.IsTest && !Globals.GetIsUnitTestingProductionFunctionality())
				{
					Test_Allocator = new PaymentChequeNumberAllocator.DummyPaymentChequeNumberAllocator(fDirectPaymentBizO, PaymentChequeNumberAllocator.PrintingMode.DirectPayment, fDirectPaymentBizO.Factory);
					Test_Allocator.SetChequeBookToInactiveOnSaving = Test_DeactivateChequeBookOnAllocation;
					base.SaveCore(Test_Allocator.GetFactoriesForTest());
				}
				else
				{
#endif
					var allocator = new PaymentChequeNumberAllocator(fDirectPaymentBizO, PaymentChequeNumberAllocator.PrintingMode.DirectPayment, fDirectPaymentBizO.Factory);
					base.SaveCore(allocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving(factories));
#if DEBUG
				}
#endif
			}
			else
			{
				base.SaveCore(factories);
			}
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			CashBookTransactionTabPage1.RunWhenBindingOrFirstShown(delegate
			{
				CashBookLineBoundGrid.ColumnLayoutContext = CashBookLineGridContext.Payment;
			});
		}

		#endregion

		#region Implementation

		readonly DirectPayment fDirectPaymentBizO;

		void HideChequeControl()
		{
			ChequeBookFindBox.Visible = false;
			ControlDpiScalingHelper.SetTop(ref ExchangeRateControl, ChequeBookFindBox.Top, false);
		}

		protected virtual PaymentPrintManager PrintManager
		{
			get
			{
				if (fPrintManager == null)
				{
					fPrintManager = GetPaymentPrintManager();
				}

				return fPrintManager;
			}
		}
		PaymentPrintManager fPrintManager;

		PaymentPrintManager GetPaymentPrintManager()
		{
			PaymentPrintManager printManager = new PaymentPrintManager(fDirectPaymentBizO.PK.ToGuid(), TransactionTypes.DirectPayment, new BusinessObjectFactory());
			if (((IChequeNumberAutoAllocation)fDirectPaymentBizO).ChequeIsAutoPrinted)
			{
				printManager.SetChequeIsAutoPrinted();
			}
			return printManager;
		}

		#endregion

		#region TestCase

#if DEBUG
		internal ZBool Test_DeactivateChequeBookOnAllocation = ZBool.False;
		internal PaymentChequeNumberAllocator.DummyPaymentChequeNumberAllocator Test_Allocator;
#endif
		#endregion

	}
}

