using System;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public partial class WarehouseAreaIDsForm : ZChildForm
	{
		public WarehouseAreaIDsForm(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public static void ShowDialog(JobDeclaration declaration)
		{
			ZFormModaliser.ShowDialogAndDispose(new WarehouseAreaIDsForm(declaration));
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Form Caption

		public override string FormVerb
		{
			get { return ""; }
		}

		#endregion

		#region Buttons

		void OnOKButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void OnCloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion
	}
}
