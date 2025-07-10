using System;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public partial class CustomsBrokerageUserControl : BaseCustomsBrokerageUserControl
	{
		public CustomsBrokerageUserControl()
		{
			InitializeComponent();
		}

		protected override BaseCustomsEntryUserControl GetDeclarationUserControl()
		{
			return new JobDeclarationUserControl();
		}

		#region Entry Instruction Details Tab Page

		protected override BaseCustomsEntryUserControl GetEntryInstructionUserControl()
		{
			if (JobDeclaration.IsImport)
			{
				return new ImportEntryInstructionUserControl();
			}
			else
			{
				return new ExportEntryInstructionUserControl();
			}
		}

		public override bool EntryInstructionsTabVisibleForCountry => true;

		#endregion

		#region Create New User Controls for each tab

		protected override BaseCustomsCusContainersUserControl GetContainerUserControl() => new CustomsCusContainersWithTrackingAndAdditionalSealUserControl();

		protected override void LazyCreateControlsFired(object sender, EventArgs e)
		{
			if (sender == ContainerTabPage)
			{
				LoadContainerTabPage();
			}
			else
			{
				base.LazyCreateControlsFired(sender, e);
			}
		}

		protected override Customs.GUI.BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControl()
		{
			Customs.GUI.BaseCustomsSupplierHeaderUserControl result;

			if (JobDeclaration.IsImport)
			{
				result = new ImportSupplierHeaderUserControl();
			}
			else
			{
				result = new ExportSupplierHeaderUserControl();
			}

			return result;
		}

		protected override Customs.GUI.BaseInvoiceLineUserControl GetInvoiceLinesUserControl()
		{
			Customs.GUI.BaseInvoiceLineUserControl result;

			if (JobDeclaration.IsImport)
			{
				result = new ImportInvoiceLineUserControl();
			}
			else
			{
				result = new ExportInvoiceLineUserControl();
			}

			return result;
		}

		protected override void RemoveUserControlOfEachTabPage()
		{
			base.RemoveUserControlOfEachTabPage();
			RemoveControl(EntryInstructionDetailsTabPage);
		}

		protected override BaseCustomsEntryUserControl GetMessageUserControl()
		{
			return new EntriesTabUserControl();
		}

		#endregion
	}
}
