using System;

namespace Enterprise.ZArchitecture.GUI
{
	[SuppressFormsLocalizedTest]
	public partial class CustomPropertiesCollectionDetailsControl : ZUserControl
	{
		public CustomPropertiesCollectionDetailsControl()
		{
			InitializeComponent();
		}

		#region InitializeComponent

		protected CustomPropertiesControl customPropertiesControl;

		protected virtual CustomPropertiesControl GetCustomPropertiesControl()
		{
			return new CustomPropertiesControl();
		}

		public void SetNothingSetupMessageLabelText(string message)
		{
			customPropertiesControl.NothingSetupMessageLabelText = message;
		}

		#endregion

		#region OnCurrentDataItemChanged

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			customPropertiesControl.SetDataBinding(CurrentDataItem, "");
		}

		#endregion
	}
}
