using System;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.GUI
{
	public partial class EntryInstructionDetailsUserControl : EU.GUI.EntryInstructionDetailsUserControl
	{
		public EntryInstructionDetailsUserControl()
		{
			InitializeComponent();
		}

		protected override Type GetGridUserControl() => typeof(EntryInstructionGridUserControl);

		protected override Type GetDetailsUserControlType() => typeof(EntryInstructionDetailBasicUserControl);

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (JobDeclaration is JobDeclaration declaration)
				{
					foreach (CusEntryInstruction instruction in declaration.CustomsEntryInstructions)
					{
						instruction.DisposeMutex();
					}
				}
			}
			base.Dispose(disposing);
		}

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();

			AdjustDependentControlVisibility();
		}

		void AdjustDependentControlVisibility()
		{
			if (DetailsUserControl.HostedControl is null)
			{
				return;
			}

			var isVisible = ((JobDeclaration)CurrentDataItem)?.IsImport ?? false;

			((EntryInstructionDetailBasicUserControl)DetailsUserControl.HostedControl).HandleDeclarationControlVisibilityChangedCore(isVisible);
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			AdjustDependentControlVisibility();
		}

		protected override Type GetGuaranteesUserControlType() => typeof(EntryInstructionGuaranteesUserControl);

		protected override Type GetAuthorisationsUserControlType() => ((JobDeclaration)JobDeclaration).IsUCC6 ? base.GetAuthorisationsUserControlType() : typeof(DeltaGEntryInstructionAuthorisationsUserControl);
	}
}
