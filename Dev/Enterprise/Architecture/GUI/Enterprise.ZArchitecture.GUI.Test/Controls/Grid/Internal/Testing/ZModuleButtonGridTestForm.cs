using CargoWise.EntityFramework;
using CargoWise.Windows.UI.Testing;
using Enterprise.Core.Forms;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[SuppressFormDesignerAnalysis]
	partial class ZModuleButtonGridTestForm : ZForm
	{
		public ZCalcEdit CalcEdit;
		public ZTextBox TextBox;
		public ZPostingButtonsUserControl SaveUserControl;
		public ZDummyModuleButtonGrid Grid;
		private readonly System.ComponentModel.Container components;

		public ZModuleButtonGridTestForm()
		{
			InitializeComponent();
		}

		public override string FormCaption
		{
			get { return "ZButtonGridTestForm"; }
		}

		public ContinueWithSave ExposedContinueWithSave = ContinueWithSave.Yes;

		protected internal override ContinueWithSave ShowPreSaveDialogs()
		{
			return ExposedContinueWithSave;
		}

		protected virtual ZDummyModuleButtonGrid GetModuleButtonGrid()
		{
			return new ZDummyModuleButtonGrid();
		}

		public ZModuleButtonGridTestForm(IBusiness entity)
			: base(entity)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, SaveUserControl);
			this.CaptionRenderingEnabled = true;
		}

		public ZModuleButtonGridTestForm(IBusiness entity, bool useAlternativeEditObject)
			: base(entity)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, SaveUserControl);
			Grid.UseAlternativeObjectForEdit = useAlternativeEditObject;
		}

		#region Auto

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}
