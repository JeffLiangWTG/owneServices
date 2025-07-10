using System;
using Enterprise.Registry.Business.Customs.US;

namespace Enterprise.Registry.GUI
{
	public partial class ExportEntryFilerIDControl : RegistryZUserControl
	{
		public ExportEntryFilerIDControl()
		{
			InitializeComponent();
		}

		public new ExportEntryFilerID CurrentDataItem
		{
			get { return (ExportEntryFilerID)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			SetBusinessEntityReadOnly();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			SetBusinessEntityReadOnly();
		}

		void SetBusinessEntityReadOnly()
		{
			if (CurrentDataItem != null)
			{
				CurrentDataItem.ReadOnly = ReadOnly;
			}
		}
	}
}
