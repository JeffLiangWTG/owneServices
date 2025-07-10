using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class RefundDeclarationForm : ZTemplateForm
	{
		public RefundDeclarationForm(CusReconDeclaration cusReconDeclaration)
			: base(cusReconDeclaration)
		{
			this.cusReconDeclaration = cusReconDeclaration;
		}
		readonly CusReconDeclaration cusReconDeclaration;

		public override string FormCaption
		{
			get
			{
				var caption = ZString.Empty;
				if (!this.IsDesignMode() && cusReconDeclaration != null)
				{
					var additionalCaption = string.IsNullOrEmpty(cusReconDeclaration.CRD_JobReferenceNumber) ? ZString.Empty : (ZString)(" - " + cusReconDeclaration.CRD_JobReferenceNumber);
					caption = string.Format(Res.GetString("86F70F23-2DC8-47D0-9AB3-DAAF6A5AB466", "Refund Declaration {0}"), additionalCaption);
				}
				return caption;
			}
		}

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			base.InitialiseForm();
		}
	}
}
