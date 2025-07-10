using System.Collections.Generic;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public partial class ImportSiscomexEntryInstructionDetailsUserControl : BaseCustomsEntryUserControl
	{
		public ImportSiscomexEntryInstructionDetailsUserControl()
		{
			InitializeComponent();
			InitializeCustomizedGrid();

			AFRMMDetailsUserControl.IsAFRMMRateOverriddenCheckBox.AllowOutsideOfParent();
		}

		protected void InitializeCustomizedGrid()
		{
			EntryInstructionsGrid.ReOrderColumns(ReorderedColumnsSequence);
		}

		JobDeclaration Declaration => JobDeclaration as JobDeclaration;

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			AFRMMDetailsUserControl.AFRMMGroupBox.Visible = Declaration?.IsAFRMMApplicable ?? true;
		}

		string[] ReorderedColumnsSequence
		{
			get
			{
				if (reorderedColumnsSequence == null)
				{
					var columnList = new List<string>
					{
						BR.Business.CusEntryInstruction.Schema.CEI_Description,
						BR.Business.CusEntryInstruction.Schema.AdditionalInformationOptionDescription,
						BR.Business.CusEntryInstruction.Schema.AdditionalInformationManual,
						BR.Business.CusEntryInstruction.Schema.AdditionalInformation
					};
					reorderedColumnsSequence = columnList.ToArray();
				}
				return reorderedColumnsSequence;
			}
		}
		string[] reorderedColumnsSequence;
	}
}
