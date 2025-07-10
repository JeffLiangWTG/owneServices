using System;
using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.ZArchitecture.GUI;

#if DEBUG

using Enterprise.ZArchitecture.GUI.Testing;

#endif

namespace Enterprise.Accounting.GUI.ARAP.PaymentApproval
{
	public partial class CreditCardSecurityCodeForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public CreditCardSecurityCodeForm()
		{
			InitializeComponent();
		}

		public CreditCardSecurityCodeForm(PaymentCreditCardSecurityCode bo)
			: base(bo)
		{
			InitializeComponent();

#if DEBUG
			MissingResourceStringChecker.ExcludeFromTest(MessageLabel);
			TypeDescriptor.AddAttributes(MessageLabel, new SuppressFormsLocalizedTestAttribute());
			MissingResourceStringChecker.ExcludeFromTest(CardSecurityCodeTextBox);
#endif
			MessageLabel.GetExtension<ILabelCaptionRenderer>().Caption = bo.Message;
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			PaymentCreditCardSecurityCode code = BusinessEntity as PaymentCreditCardSecurityCode;
			if (code != null)
			{
				code.RunPreSaveValidation();
				if (!code.HasErrors)
				{
					code.Continue = true;
					Hide();
				}
			}
		}

		void Cancel_Button_Click(object sender, EventArgs e)
		{
			PaymentCreditCardSecurityCode code = BusinessEntity as PaymentCreditCardSecurityCode;
			if (code != null)
			{
				code.Continue = false;
			}
			Hide();
		}
	}
}
