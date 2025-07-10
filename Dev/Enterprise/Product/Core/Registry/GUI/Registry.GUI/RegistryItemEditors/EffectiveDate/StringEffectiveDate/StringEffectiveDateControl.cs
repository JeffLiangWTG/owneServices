using System;
using Enterprise.Registry.Business;

namespace Enterprise.Registry.GUI
{
	public partial class StringEffectiveDateControl : RegistryZUserControl
	{
		public StringEffectiveDateControl()
		{
			InitializeComponent();
		}

		public new StringEffectiveDate CurrentDataItem
		{
			get { return (StringEffectiveDate)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			SetBusinessEntityReadOnly();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			PreviousValueTextBox.ReadOnly = readOnly;
			NewValueTextBox.ReadOnly = readOnly;
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
