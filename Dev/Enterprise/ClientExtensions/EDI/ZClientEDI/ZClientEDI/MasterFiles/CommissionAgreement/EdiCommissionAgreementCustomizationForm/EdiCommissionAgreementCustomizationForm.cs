using System;
using CargoWise.Common;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	public partial class EdiCommissionAgreementCustomizationForm : ZChildForm, IReadOnlyToggleControl
	{
		public EdiCommissionAgreementCustomizationForm(EdiCommissionAgreementCustomization commissionAgreementCustomization)
			: base(commissionAgreementCustomization)
		{
			Argument.NotNull(commissionAgreementCustomization, "commissionAgreementCustomization");

			this.BackColor = SystemDataRegistry.Instance.ColorTheme.TabBackgroundColor;

			InitializeComponent();
		}

		#region Button Handlers

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion

		#region Form Caption

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		#endregion

		#region ReadOnly

		public bool ReadOnly
		{
			get { return readOnly; }
			set
			{
				if (readOnly != value)
				{
					readOnly = value;
					customizationTreeControl.ReadOnly = value;
				}
			}
		}
		bool readOnly;

		#endregion

		#region Dispose

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
