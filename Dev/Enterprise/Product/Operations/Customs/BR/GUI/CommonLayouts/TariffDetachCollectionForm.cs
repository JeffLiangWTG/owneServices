using System;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public partial class TariffDetachCollectionForm : ZChildForm
	{
		public TariffDetachCollectionForm(TariffDetachCollection tariffDetachCollection)
			: base(tariffDetachCollection)
		{
		}

		public static void ShowDialog(TariffDetachCollection tariffDetachCollection)
		{
			ZFormModaliser.ShowDialogAndDispose(new TariffDetachCollectionForm(tariffDetachCollection));
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Form Caption

		public override string FormVerb => "";

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
