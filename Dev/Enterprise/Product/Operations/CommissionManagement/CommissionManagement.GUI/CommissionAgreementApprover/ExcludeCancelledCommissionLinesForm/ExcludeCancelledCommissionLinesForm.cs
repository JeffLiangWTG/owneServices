using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.CommissionManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CommissionManagement.GUI
{
	public partial class ExcludeCancelledCommissionLinesForm : ZChildForm
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public ExcludeCancelledCommissionLinesForm(BusinessObjectFactory factory, Dictionary<OrgCommissionAgreement, List<ViewCommissionLine>> agreementsAndCancelledLines)
		{
			InitializeComponent();

			this.Factory = factory;
			this.FormCaptionLabel.Text = FormCaptionLabelText;

			InitializeControlsForAgreements(agreementsAndCancelledLines);
		}

		readonly BusinessObjectFactory Factory;
		protected List<AgreementCancelledCommissionsControl> SubControls = new List<AgreementCancelledCommissionsControl>();

		void InitializeControlsForAgreements(Dictionary<OrgCommissionAgreement, List<ViewCommissionLine>> agreementsAndCancelledLines)
		{
			foreach (var agreementLinesPair in agreementsAndCancelledLines)
			{
				var cancelledLinesCollection = new CancelledLinesCollection(Factory);
				cancelledLinesCollection.AddLines(agreementLinesPair.Value);

				var newControl = new AgreementCancelledCommissionsControl(cancelledLinesCollection, agreementLinesPair.Key);
				newControl.Dock = DockStyle.Top;
				newControl.CaptionRenderingEnabled = true;
				newControl.TabStop = false;
				this.AgreementsWithCancelledCommissionLinesPanel.Controls.Add(newControl);
				SubControls.Add(newControl);
			}
		}

		public void UntickAllSubcontrols()
		{
			foreach (var control in SubControls)
			{
				control.CancelledLines.UntickAll();
			}
		}

		void Cancel_OnClick(object sender, EventArgs e)
		{
			this.Close();
		}

		void ContinueButton_OnClick(object sender, EventArgs e)
		{
			var cancelledLinesToNotReinstate = new List<ViewCommissionLine>();

			SubControls.ForEach(control => cancelledLinesToNotReinstate.AddRange(control.GetCommissionLinesToNotReinstate()));

			foreach (var line in cancelledLinesToNotReinstate)
			{
				var baseLine = Factory.Load<AccCommissionLine>(line.PK);
				baseLine.CL0_ShouldReinstate = false;
			}

			DialogResult = DialogResult.OK;
		}

		string FormCaptionLabelText => Res.GetString("9b9edc13-718c-442f-a419-de1ae0ded1ce", @"For the Commission Agreements being appended, please note the commission transaction lines that are in a canceled state (i.e. have been canceled by the Commission Manager). 
These canceled commission transaction lines will remain as 'Excluded' from this append action by default. If you would like to reinstate any of these canceled transactions as part of this append action, uncheck the 'Exclude' checkbox before selecting the Continue button. 
These canceled commission transactions, if reinstated here, will not be presented in subsequent append actions unless they have been canceled again by the Commission Manager.");
	}
}
