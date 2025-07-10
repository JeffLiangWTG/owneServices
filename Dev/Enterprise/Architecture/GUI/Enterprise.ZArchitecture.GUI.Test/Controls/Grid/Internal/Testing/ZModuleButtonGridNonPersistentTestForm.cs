using CargoWise.EntityFramework;
using Enterprise.Core.Forms;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed partial class ZModuleButtonGridNonPersistentTestForm : ZForm
	{
		public ZCalcEdit CalcEdit;
		public ZTextBox TextBox;
		public ZPostingButtonsUserControl SaveUserControl;
		public ZDummyNonPersistentModuleButtonGrid Grid;
		private readonly System.ComponentModel.Container components;

		public ZModuleButtonGridNonPersistentTestForm()
		{
			InitializeComponent();
		}

		public override string FormCaption
		{
			get { return "ZModuleButtonGridNonPersistentTestForm"; }
		}

		public ZModuleButtonGridNonPersistentTestForm(IBusiness entity)
			: base(entity)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, SaveUserControl);
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
