using System;

using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public class ModuleShowingEventArgs : EventArgs
	{
		public ModuleShowingEventArgs(ModuleIdentifier moduleID)
		{
			this.moduleID = moduleID;
		}

		public ModuleIdentifier ModuleID
		{
			get { return moduleID; }
			set { moduleID = value; }
		}

		ModuleIdentifier moduleID;
	}
}