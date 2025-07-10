using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class CostConfirmationDocTypePopupForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public CostConfirmationDocTypePopupForm()
		{
		}

		public CostConfirmationDocTypePopupForm(CostConfirmationDocTypeBizo bo)
			: base(bo)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
			e.Cancel = DialogResult == DialogResult.OK && ValidateAndSave() == ContinueWithSave.No;
		}

		protected override void Save(CargoWise.Integration.ITransactionParticipant[] factories)
		{
			//do nothing
		}
	}
}
