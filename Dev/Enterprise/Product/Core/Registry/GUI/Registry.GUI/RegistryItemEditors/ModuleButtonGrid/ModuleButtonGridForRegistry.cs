using System;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Registry.GUI
{
	class ModuleButtonGridForRegistry<T> : ZModuleButtonGrid
		where T : RegistryProxyBusinessObject, new()
	{
		#region Attach

		protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
		{
			return new RecordAttacherForRegistry<T>((RegistryZUserControl)Parent, destinationCollection, findBoxList, moduleID);
		}

		#endregion

		#region Detach

		protected override void DetachButton_Click(object sender, EventArgs e)
		{
			base.DetachButton_Click(sender, e);
			Parent.Focus(); // This is necessary as Committing of the Registry item occurs on Leaving the Registry Control.
		}

		#endregion

		#region Testing
#if DEBUG
		internal bool ButtonsReadOnlyExposedForTesting
		{
			get { return ButtonsReadOnly; }
		}
#endif
		#endregion
	}
}
