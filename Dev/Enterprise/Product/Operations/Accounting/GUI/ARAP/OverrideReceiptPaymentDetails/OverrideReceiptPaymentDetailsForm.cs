using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP
{
	public abstract partial class OverrideReceiptPaymentDetailsForm : ZForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		protected OverrideReceiptPaymentDetailsForm()
		{
		}

		protected OverrideReceiptPaymentDetailsForm(BusinessObject bo)
			: base(bo)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			if (BizO is OverrideReceiptPaymentDetailsHelper)
			{
				if ((BizO as OverrideReceiptPaymentDetailsHelper).CanBizOBeSaved)
				{
					DisplayMode = ODisplayMode.NewSaved;
					DisableNewAction();
				}
				else
				{
					DisplayMode = ODisplayMode.Undefined;
				}
			}
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			HideUnrelatedButtons();
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

#if DEBUG
		public ZGrid ReceiptPaymentsGridForTest
		{
			get { return this.ReceiptPaymentsGrid; }
		}
#endif

		BusinessObject BizO
		{
			get { return (BusinessObject)BusinessEntity; }
		}

		void ContinueButton_Click(object sender, EventArgs e)
		{
			if (BizO is OverrideReceiptPaymentDetailsHelper)
			{
				(BizO as OverrideReceiptPaymentDetailsHelper).UpdateRelatedParentBizO();
				ZForm parentForm = (ZForm)ZFormModaliser.GetParentFormForModalForm(this);
				this.Close();
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		void HideUnrelatedButtons()
		{
			if (BizO == null)
			{
				return; //For designer it is possible that form does not have business object
			}

			if (BizO is OverrideReceiptPaymentDetailsHelper)
			{
				if ((BizO as OverrideReceiptPaymentDetailsHelper).CanBizOBeSaved)
				{
					ContinueButton.Visible = false;
					CloseButton.Visible = false;

					PostingButtonsUserControl.Visible = true;
				}
				else
				{
					ContinueButton.Visible = true;
					CloseButton.Visible = true;

					PostingButtonsUserControl.Visible = false;
				}
			}
		}
	}
}
