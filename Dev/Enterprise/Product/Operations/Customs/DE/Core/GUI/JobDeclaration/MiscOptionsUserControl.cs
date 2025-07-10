using System;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.GUI
{
	public partial class MiscOptionsUserControl : EU.GUI.MiscOptionsUserControl
	{
		public MiscOptionsUserControl()
		{
			InitializeComponent();
		}
		JobDeclaration declaration;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (declaration != null)
			{
				declaration.JE_MessageTypeInfo.ValueChanged -= MessageTypeInfoOnValueChanged;
			}

			base.SetDataBinding(dataSource, dataMember);

			declaration = dataSource as JobDeclaration;

			if (declaration != null)
			{
				declaration.JE_MessageTypeInfo.ValueChanged += MessageTypeInfoOnValueChanged;
				MessageTypeInfoOnValueChanged(declaration, EventArgs.Empty);
			}
		}

		void MessageTypeInfoOnValueChanged(object sender, EventArgs eventArgs)
		{
			if (declaration != null && !declaration.IsDeleted)
			{
				var isImport = declaration.IsImport;
				var isExport = declaration.IsExport;
				JE_StatisticStatusDropEdit.Visible = isImport;
				VATClaimBackDropEdit.Visible = isImport;
				RepresentationDropEdit.Visible = !isExport;
				DeferralGroupBox.Visible = isImport;
			}
		}

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();
			LCPInspectDateEdit.Visible = false;
			LCPDepartDateEdit.Visible = false;
			JE_EntryAuthorisationDateDateEdit.Visible = false;
			JE_RouteFRequestedCheckBox.Visible = false;
			CheckBoxTraining.Visible = false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (declaration != null)
				{
					declaration.JE_MessageTypeInfo.ValueChanged -= MessageTypeInfoOnValueChanged;
				}
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
