using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.PlugIn.Testing
{
	sealed class DummyPlugInNoTabControl : ZPlugIn
	{
		public DummyPlugInNoTabControl(IBusiness hostBusinessEntity)
			: base(hostBusinessEntity)
		{
		}

		protected override void HookFormEventsCore()
		{
			base.HookFormEventsCore();
			Form.Load += new EventHandler(Form_Load);
		}

		protected override void UnHookFormEventsCore()
		{
			base.UnHookFormEventsCore();
			Form.Load -= new EventHandler(Form_Load);
		}

		void Form_Load(object sender, EventArgs e)
		{
			OnForm_LoadCount++;
		}
		internal int OnForm_LoadCount;

		public override ContinueWithSave ShowPreSaveDialogsCore()
		{
			ShowPreSaveDialogsCoreCount++;
			return base.ShowPreSaveDialogsCore();
		}
		internal int ShowPreSaveDialogsCoreCount;

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return (LicenceCheckpoint)EnvProxy.Instance.Licence.AlwaysAllow; }
		}

		protected internal override ZBool HasUserControl
		{
			get { return false; }
		}

		public override string Name
		{
			get { return "PlugInNoTabControl"; }
		}

		protected internal override ZBool IsActive
		{
			get { return true; }
		}
	}
}
