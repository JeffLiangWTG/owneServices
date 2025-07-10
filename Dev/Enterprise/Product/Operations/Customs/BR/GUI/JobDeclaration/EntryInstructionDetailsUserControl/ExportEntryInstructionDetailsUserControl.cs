using System.Collections.Generic;
using CargoWise.Windows.UI;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public partial class ExportEntryInstructionDetailsUserControl : BaseCustomsEntryUserControl
	{
		public ExportEntryInstructionDetailsUserControl()
		{
			InitializeComponent();
			InitializeCustomizedGrid();
		}

		protected void InitializeCustomizedGrid()
		{
			EntryInstructionsGrid.ReOrderColumns(ReorderedColumnsSequence);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			const string isVisibleForBinding = "IsVisibleForBinding";

			JustificationGroupBox.DataBindings.RemoveBinding(isVisibleForBinding);

			if (DataSource != null)
			{
				JustificationGroupBox.DataBindings.Add(new KBinding(isVisibleForBinding, DataSource, $"{nameof(JobDeclaration.CustomsEntryInstructions)}.{nameof(CusEntryInstruction.IsJustificationVisible)}", false, System.Windows.Forms.DataSourceUpdateMode.Never));
			}
		}

		string[] ReorderedColumnsSequence
		{
			get
			{
				if (reorderedColumnsSequence == null)
				{
					var columnList = new List<string>
					{
						CusEntryInstruction.Schema.CEI_Description,
						CusEntryInstruction.Schema.CEI_SpecialCustomsClearance,
						CusEntryInstruction.Schema.CEI_LegalDocument,
					};
					reorderedColumnsSequence = columnList.ToArray();
				}
				return reorderedColumnsSequence;
			}
		}
		string[] reorderedColumnsSequence;
	}
}
