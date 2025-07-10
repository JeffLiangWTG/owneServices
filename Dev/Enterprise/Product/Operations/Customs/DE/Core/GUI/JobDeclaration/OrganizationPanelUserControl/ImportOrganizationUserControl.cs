using CargoWise.Windows.UI;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.GUI
{
	public partial class ImportOrganizationUserControl : EU.GUI.ImportOrganizationUserControl
	{
		public ImportOrganizationUserControl()
		{
			InitializeComponent();
			paymentPartyCaption = Res.GetData("7fd0748d-d9f1-4ff2-804a-2f6063b95647", "Payment Party").Caption;
			defermentPartyCaption = DefermentPartyDocAddressControl.CaptionResourceString.Caption;
		}
		readonly string paymentPartyCaption;
		readonly string defermentPartyCaption;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (JobDeclaration != null)
			{
				JobDeclaration.ZG_MethodOfPaymentInfo.ValueChanged -= ZG_MethodOfPaymentInfo_ValueChanged;
			}
			base.SetDataBinding(dataSource, dataMember);
			if (JobDeclaration != null)
			{
				JobDeclaration.ZG_MethodOfPaymentInfo.ValueChanged += ZG_MethodOfPaymentInfo_ValueChanged;
				UpdateDefermentPartyCaption();
			}
		}

		void ZG_MethodOfPaymentInfo_ValueChanged(object sender, System.EventArgs e) => UpdateDefermentPartyCaption();

		void UpdateDefermentPartyCaption()
		{
			DefermentPartyDocAddressControl.GetExtension<ILabelCaptionRenderer>().Caption = JobDeclaration.IsDefermentAllowed ? defermentPartyCaption : paymentPartyCaption;
		}

		JobDeclaration JobDeclaration => (JobDeclaration)BindingSource.DataSource;
	}
}
