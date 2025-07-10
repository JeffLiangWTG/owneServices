using System;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class LVXSelectionForm : ZChildForm
	{
		public LVXSelectionForm(LVXSelectionCriteriaBO bizObj)
			: base(bizObj)
		{
		}

		LVXSelectionCriteriaBO LVXSelectionCriteriaBO
		{
			get { return (LVXSelectionCriteriaBO)base.DataSource; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormHeading
		{
			get { return Res.GetString("7044a149-af80-4ecb-99be-1a4394aa5916", "Consolidation Selection Criteria"); }
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			LVXSelectionCriteriaBO.RunPreSaveValidation();
			if (LVXSelectionCriteriaBO.HasErrors)
			{
				ShowErrorsDialog();
			}
			else
			{
				DialogResult = System.Windows.Forms.DialogResult.OK;
				Close();
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = System.Windows.Forms.DialogResult.Cancel;
			Close();
		}
	}
}
