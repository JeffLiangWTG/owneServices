using System;
using CargoWise.Types;
using Enterprise.UniversalCopy.Business;

namespace Enterprise.UniversalCopy.GUI
{
	partial class CopyTemplateDetailsUserControl : EntityNodeDetailsUserControl
	{
		protected CopyTemplateDetailsUserControl()
		{
			InitializeComponent();
		}

		public CopyTemplateDetailsUserControl(UniversalCopyManager copyManager)
			: base(copyManager)
		{
			InitializeComponent();
		}

		CopyTemplateTreeBizo CopyTemplateTreeBizo
		{
			get { return (CopyTemplateTreeBizo)BindingSource.Current; }
		}

		protected override void HookDataSource()
		{
			base.HookDataSource();

			if (CopyTemplateTreeBizo != null)
			{
				nominatedRecordFindBox.ModuleID = CopyTemplateTreeBizo.Parent.GetModuleIdentifier();
				nominatedRecordFindBox.AllowTemplateRecords = true;

				CopyTemplateTreeBizo.ConfigurationSourceInfo.ValueChanged += ConfigurationSourceChanged;
				ConfigurationSourceChanged(this, EventArgs.Empty);
			}
		}

		protected override void UnHookDataSource()
		{
			base.UnHookDataSource();

			if (CopyTemplateTreeBizo != null)
			{
				CopyTemplateTreeBizo.ConfigurationSourceInfo.ValueChanged -= ConfigurationSourceChanged;
			}
		}

		void ConfigurationSourceChanged(object sender, EventArgs e)
		{
			if (CopyTemplateTreeBizo.ConfigurationSource == CargoWise.UniversalCopy.ConfigurationSourceCodes.FilteredRecord)
			{
				ClearRecordFindBoxControl();
				InitializeFilterControl();
			}
			else if (CopyTemplateTreeBizo.ConfigurationSource == CargoWise.UniversalCopy.ConfigurationSourceCodes.NominatedRecord)
			{
				ClearFilterControls();
				nominatedRecordFindBox.Visible = true;
			}
			else
			{
				ClearRecordFindBoxControl();
				ClearFilterControls();
			}
		}

		void ClearRecordFindBoxControl()
		{
			nominatedRecordFindBox.Visible = false;
			CopyTemplateTreeBizo.NominatedRecordPk = ZGuid.Empty;
		}

		protected override string OrderBy
		{
			get { return CopyTemplateTreeBizo != null ? CopyTemplateTreeBizo.OrderBy.ToString() : base.OrderBy; }
		}
	}
}
