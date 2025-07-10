using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.GB.GUI.Plugin;

namespace Enterprise.Customs.GB.GUI
{
	public partial class EntryInstructionDetailsUserControl : EU.GUI.EntryInstructionDetailsUserControl
	{
		public EntryInstructionDetailsUserControl()
		{
			InitializeComponent();
		}

		protected override Type GetGridUserControl() => typeof(EntryInstructionGridUserControl);

		protected override Type GetDetailsUserControlType() => typeof(EntryInstructionDetailBasicUserControl);

		protected override ResourceStringData GetAdditionalInfosTabCaption() => Res.GetData("302fcb7d-7a0f-453d-915e-e42b33336d1f", "[UCC 2/2] Additional Info");

		protected override Type GetAdditionalInfosUserControlType() => typeof(GBAdditionalInfosUserControl);

		internal new ZArchitecture.GUI.ZTabControl EntryInstructionTabControl => base.EntryInstructionTabControl;
	}
}
