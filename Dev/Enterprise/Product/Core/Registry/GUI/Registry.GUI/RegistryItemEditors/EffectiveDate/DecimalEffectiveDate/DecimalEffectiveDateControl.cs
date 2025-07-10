using System;
using Enterprise.Registry.Business;

namespace Enterprise.Registry.GUI
{
	public partial class DecimalEffectiveDateControl : RegistryZUserControl
	{
		public DecimalEffectiveDateControl()
		{
			InitializeComponent();
		}

		public new DecimalEffectiveDate CurrentDataItem
		{
			get { return (DecimalEffectiveDate)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			SetBusinessEntityReadOnly();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			PreviousValueCalcEdit.ReadOnly = readOnly;
			NewValueCalcEdit.ReadOnly = readOnly;
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
