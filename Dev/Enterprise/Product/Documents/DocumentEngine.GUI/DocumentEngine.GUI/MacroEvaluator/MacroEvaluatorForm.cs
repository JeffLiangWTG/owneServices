using System;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngine.MacroEvaluator;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI
{
	public partial class MacroEvaluatorForm : ZChildForm
	{
		MacroEvaluatorForm()
		{
			InitializeComponent();
		}

		public MacroEvaluatorForm(EvaluatorManager manager)
			: base(manager)
		{
			Argument.NotNull(manager, "manager");

			InitializeComponent();

			Manager.Output = ZString.Empty;
			DataContextZDropEdit.Visible = manager.HasDataContext;
			DataContextLabel.Visible = !manager.HasDataContext;
			MacroZTextBox.CaptionResourceString = manager.MacroDescription;
			CaptionResourceString = manager.Name;

#if DEBUG
			TypeDescriptor.AddAttributes(DataContextZDropEdit, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		public EvaluatorManager Manager
		{
			get { return BusinessEntity as EvaluatorManager; }
		}

		#region Button Actions

		void CloseZButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void EvaluateZButton_Click(object sender, EventArgs e)
		{
			try
			{
				EvaluateZButton.Enabled = false;
				ZString result = Manager.Evaluate();
				if (!result.IsEmpty)
				{
					Globals.Message.ShowError(result);
				}
			}
			finally
			{
				EvaluateZButton.Enabled = true;
				EvaluateZButton.Focus();
			}
		}

#if DEBUG
		internal
#endif
		void CopyToClipboard(object sender, EventArgs e)
		{
			if (!Manager.Macro.IsEmpty)
			{
				SafeClipboard.SetText(Manager.Macro);
			}
		}

		#endregion

		#region Implementation

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		#endregion
	}
}
