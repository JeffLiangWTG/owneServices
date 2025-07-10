using System;
using Enterprise.ZArchitecture.GUI;

#if DEBUG
using Enterprise.ZArchitecture.GUI.Testing;
#endif

namespace Enterprise.Accounting.GUI.XmlExport
{
	public partial class XmlExportForm : ZForm
	{
		public XmlExportForm()
		{
		}

		public XmlExportForm(XmlExportGUIWrapper wrapper)
			: base(wrapper)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, null, CloseButton, null);
			this.Wrapper = wrapper;
			wrapper.ExistingBatchChanged += Wrapper_ExistingBatchChanged;
			SetModlueButtonsGridButtonsCaptions();
#if DEBUG
			MissingResourceStringChecker.ExcludeFromTest(HighWaterMarkLabel);
#endif
		}

		void SetModlueButtonsGridButtonsCaptions()
		{
			OrganisationModuleButtonGrid.AttachButtonText = Res.GetData("ZModuleButtonGrid|Add", "Add");
			OrganisationModuleButtonGrid.DetachButtonText = Res.GetData("ZModuleButtonGrid|Remove", "Remove");
			OrganisationModuleButtonGrid.DetachMessage = Res.GetData("Accounting.XmlExportForm|ModuleButtonGridConfirmationQuestion", "Are you sure you want to remove the selected item?");

			BranchModuleButtonGrid.AttachButtonText = Res.GetData("ZModuleButtonGrid|Add", "Add");
			BranchModuleButtonGrid.DetachButtonText = Res.GetData("ZModuleButtonGrid|Remove", "Remove");
			BranchModuleButtonGrid.DetachMessage = Res.GetData("Accounting.XmlExportForm|ModuleButtonGridConfirmationQuestion", "Are you sure you want to remove the selected item?");

			DepartmentModuleButtonGrid.AttachButtonText = Res.GetData("ZModuleButtonGrid|Add", "Add");
			DepartmentModuleButtonGrid.DetachButtonText = Res.GetData("ZModuleButtonGrid|Remove", "Remove");
			DepartmentModuleButtonGrid.DetachMessage = Res.GetData("Accounting.XmlExportForm|ModuleButtonGridConfirmationQuestion", "Are you sure you want to remove the selected item?");

			JobModuleButtonGrid.AttachButtonText = Res.GetData("ZModuleButtonGrid|Add", "Add");
			JobModuleButtonGrid.DetachButtonText = Res.GetData("ZModuleButtonGrid|Remove", "Remove");
			JobModuleButtonGrid.DetachMessage = Res.GetData("Accounting.XmlExportForm|ModuleButtonGridConfirmationQuestion", "Are you sure you want to remove the selected item?");

			this.OrganisationModuleButtonGrid.NameOfAGridElement = Res.GetData("Accounting|XmlExportForm|OrganisationModuleButtonGrid.NameOfAGridElement", "Organization");
			this.BranchModuleButtonGrid.NameOfAGridElement = Res.GetData("Accounting|XmlExportForm|BranchModuleButtonGrid.NameOfAGridElement", "Branch");
			this.DepartmentModuleButtonGrid.NameOfAGridElement = Res.GetData("Accounting|XmlExportForm|DeparmentModuleButtonGrid.NameOfAGridElement", "Department");
			this.JobModuleButtonGrid.NameOfAGridElement = Res.GetData("Accounting|XmlExportForm|JobModuleButtonGrid.NameOfAGridElement", "Job");
		}

		public override string FormCaption
		{
			get { return Wrapper.FormCaption; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Export

		void ExportButton_Click(object sender, EventArgs e)
		{
			Wrapper.Export(this);
		}

		readonly XmlExportGUIWrapper Wrapper;

		void Wrapper_ExistingBatchChanged(object sender, EventArgs e)
		{
			if (Wrapper != null)
			{
				bool enabled = Wrapper.ExistingBatchNumberToExport == 0;

				OrganisationModuleButtonGrid.Enabled = enabled;
				JobModuleButtonGrid.Enabled = enabled;
				DepartmentModuleButtonGrid.Enabled = enabled;
				BranchModuleButtonGrid.Enabled = enabled;
			}
		}

		#endregion
	}
}

