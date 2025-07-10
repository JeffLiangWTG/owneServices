using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.Payment
{
	public partial class PaymentDocumentsPrintPopup : KForm, ICaptionRenderingSupport
	{
		public PaymentDocumentsPrintPopup(PaymentPrintOptions optionsAvailable = ~PaymentPrintOptions.None)
			: base()
		{
			this.optionsAvailable = optionsAvailable;
			InitializeComponent();
			Text = Res.GetString("92c9aefb-4507-4319-8cc0-a0921970d4e2", "Print Options for Payment");
		}

		public PaymentDocumentsPrintPopup(ZString paymentDescription, ZBool chequeIsAutoPrinted, PaymentPrintOptions optionsAvailable = ~PaymentPrintOptions.None)
			: this(optionsAvailable)
		{
			PaymentDescriptionLabel.Text = Res.GetString("579d7207-1d38-407e-ae24-e5cacc15ae8b", "Payment: {0}", paymentDescription);
			this.ChequeIsAutoPrinted = chequeIsAutoPrinted;
		}

		readonly PaymentPrintOptions optionsAvailable;

		public PaymentPrintOptions GetSelectedOptions()
		{
			var result = CheckBoxOptionsMap
				.Where(x => x.checkbox.Checked)
				.Aggregate(PaymentPrintOptions.None, (x, y) => x |= y.option);

			return result;
		}

		public PaymentPrintOptions GetDefaultPrintOptions()
		{
			var checkedOptions = RegistryOptionsMap.Where(x => x.registry.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK)).ToArray();
			var result = checkedOptions.Aggregate(PaymentPrintOptions.None, (x, y) => x |= y.option);
			return result;
		}

		(ZCheckBox checkbox, PaymentPrintOptions option)[] CheckBoxOptionsMap => new[]
		{
			(PrintChequeCheckBox, PaymentPrintOptions.Cheque),
			(PaymentVoucherCheckBox, PaymentPrintOptions.PaymentVoucher),
			(RemittanceAdviceCheckBox, PaymentPrintOptions.RemittanceAdvice),
			(PrintPaymentBatchListingCheckBox, PaymentPrintOptions.PaymentBatchListing),
		};

		(BooleanRegistryItem registry, PaymentPrintOptions option)[] RegistryOptionsMap => new[]
		{
			(AccountingConfigurationRegistry.Instance.PrintCheque, PaymentPrintOptions.Cheque),
			(AccountingConfigurationRegistry.Instance.PrintPaymentVoucher, PaymentPrintOptions.PaymentVoucher),
			(AccountingConfigurationRegistry.Instance.PrintRemittanceAdvice, PaymentPrintOptions.RemittanceAdvice),
			(AccountingConfigurationRegistry.Instance.PrintPaymentBatchListing, PaymentPrintOptions.PaymentBatchListing),
		};

		protected void SetCheckButtonByDefault()
		{
			var defaultOptions = GetDefaultPrintOptions();
			CheckBoxOptionsMap.ForEach(x => x.checkbox.Checked = defaultOptions.HasFlag(x.option));
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetCheckButtonByDefault();
			DisableCheckBoxes();
			DisableUnavailableCheckBoxes();
		}

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion

		readonly ZBool ChequeIsAutoPrinted;
		protected PaymentPrintOptions fDefaultOptions;
		protected Guid fPaymentPK;

		protected void PrintButton_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			Close();
		}

		protected void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void DisableCheckBoxes()
		{
			if (!IsCheque || ChequeIsAutoPrinted)
			{
				PrintChequeCheckBox.Checked = false;
				PrintChequeCheckBox.Enabled = false;
			}
			else
			{
				PrintChequeCheckBox.Enabled = true;
			}
#if DEBUG
			if (GlbStaff.CurrentUser.IsSupportUser && !Globals.IsTest)
			{
				PrintChequeCheckBox.Enabled = true;
			}
#endif
			ChequeIsAutoPrintedZLabel.Visible = ChequeIsAutoPrinted;
		}

		void DisableUnavailableCheckBoxes()
		{
			foreach (var cbo in CheckBoxOptionsMap)
			{
				if (!optionsAvailable.HasFlag(cbo.option))
				{
					cbo.checkbox.Checked = false;
					cbo.checkbox.Enabled = false;
				}
			}
		}

		public bool IsCheque
		{
			get { return fIsCheque; }
			set { fIsCheque = value; }
		}

		protected bool fIsCheque;

		bool? ICaptionRenderingSupport.CaptionRenderingEnabled
		{
			get { return true; }
		}

		event EventHandler ICaptionRenderingSupport.CaptionRenderingEnabledChanged
		{
			add { }
			remove { }
		}

#if DEBUG
		#region Test Classes

		public class MockPaymentDocumentsPrintPopup : PaymentDocumentsPrintPopup
		{
			public MockPaymentDocumentsPrintPopup(ZString paymentDescription, ZBool chequeIsAutoPrinted, PaymentPrintOptions optionsAvailable = ~PaymentPrintOptions.None)
				: base(paymentDescription, chequeIsAutoPrinted, optionsAvailable) { }

			public MockPaymentDocumentsPrintPopup()
				: base() { }

			public ZCheckBox PrintRemitAdviceCheckBox_Exposed
			{
				get { return base.RemittanceAdviceCheckBox; }
			}

			public ZCheckBox PaymentVoucherCheckBox_Exposed
			{
				get { return base.PaymentVoucherCheckBox; }
			}

			public ZCheckBox PrintChequeCheckBox_Exposed
			{
				get { return base.PrintChequeCheckBox; }
			}

			public ZCheckBox PrintPaymentBatchListingCheckBox_Exposed
			{
				get { return base.PrintPaymentBatchListingCheckBox; }
			}

			public ZLabel ChequeIsAutoPrintLabel_Exposed
			{
				get { return base.ChequeIsAutoPrintedZLabel; }
			}

			public void ExecutePrintButton()
			{
				PrintButton.PerformClick();
			}

			public void ExecuteCancelButton()
			{
				CloseButton.PerformClick();
			}
		}

		public class MockPaymentPrinter : IPaymentPrint
		{
			#region IPaymentPrint Members

			public void PrintPaymentVoucher()
			{
			}

			public void PrintCheque()
			{
			}

			public void PrintRemittanceAdvice()
			{
			}

			public void PrintPaymentBatchListing()
			{
			}

			public void AutoPrintCheque(ZGuid printerPK)
			{
			}

			public ZString PaymentTypeCode => "CHQ";

			public ZString PaymentChequeOrReference => "123456";

			public ZString PaymentOrganisationCode => "AALSHI";

			public string PaymentType
			{
				get
				{
					return PaymentTypeExposed;
				}
			}

			public string PaymentTypeExposed;

			#endregion
		}

		#endregion
#endif
	}
}

