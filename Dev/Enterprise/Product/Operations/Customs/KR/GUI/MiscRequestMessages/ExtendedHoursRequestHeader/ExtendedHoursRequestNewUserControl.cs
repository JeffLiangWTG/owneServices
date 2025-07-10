using System;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class ExtendedHoursRequestNewUserControl : ZUserControl
	{
		public ExtendedHoursRequestNewUserControl()
		{
			InitializeComponent();
		}

		public new ExtendedHoursRequestHeader CurrentDataItem => base.CurrentDataItem as ExtendedHoursRequestHeader;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			SetExtendedHoursRequestHeaderLayouts();
		}

		public void SetExtendedHoursRequestHeaderLayouts()
		{
			ExtendedHoursRequestHeaderPanel.UpdateLayout(new ExtendedHoursRequestNewLayout());
		}
	}
}
