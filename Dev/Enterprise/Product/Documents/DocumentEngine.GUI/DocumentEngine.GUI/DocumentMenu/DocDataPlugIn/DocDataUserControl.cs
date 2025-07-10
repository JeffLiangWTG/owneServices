using System;
using Enterprise.DocumentEngine.GUI.SDF;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu.DocDataPlugIn
{
	public partial class DocDataUserControl : ZUserControl
	{
		public DocDataUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			MainTabControl.PlugIns.Add(ControllerIDs.DocumentSDFPlugIn);
			MainTabControl.PlugIns.Add(ControllerIDs.DocumentUDFPlugIn);
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			var docDataPlugIn = ZTabPagePlugIn.FindParentPlugIn(this);
			var shouldBeReadOnly = docDataPlugIn.ShouldBeReadOnly;

			var udfPlugIn = MainTabControl.PlugIns.GetPlugIn(ControllerIDs.DocumentUDFPlugIn) as DocumentUDFPlugIn;
			if (udfPlugIn != null)
			{
				udfPlugIn.SetHintLabelVisibility(shouldBeReadOnly);
				udfPlugIn.UpdateUDFDefaults();
			}

			var sdfPlugIn = MainTabControl.PlugIns.GetPlugIn(ControllerIDs.DocumentSDFPlugIn) as DocumentSDFPlugIn;
			if (sdfPlugIn != null)
			{
				var documentSDFControl = sdfPlugIn.UserControl as DocumentSDFControl;
				documentSDFControl.SetHinLabelVisibility(shouldBeReadOnly);
			}
		}
	}
}
