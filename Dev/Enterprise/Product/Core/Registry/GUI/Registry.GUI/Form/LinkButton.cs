using System;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Registry.GUI
{
	public class LinkButton : ZButton
	{
		public LinkButton(string moduleName, ModuleIdentifier moduleID, string buttonCaption)
		{
			this.AutoSize = true;
			this.moduleID = moduleID;
			this.Text = buttonCaption;
			this.Click += new EventHandler(LinkButton_Click);
		}

		public LinkButton(string moduleName, ModuleIdentifier moduleID)
			: this(moduleName, moduleID, Res.GetString("b1bbcb6c-6faa-41a9-89f9-c80de06f3c29", "Edit {0}", moduleName))
		{
		}

		public ModuleIdentifier ModuleID
		{
			get { return moduleID; }
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Click -= new EventHandler(LinkButton_Click);
			}
			base.Dispose(disposing);
		}

		void LinkButton_Click(object sender, EventArgs e)
		{
			OpenModule();
		}

		void OpenModule()
		{
			using (ZModule module = ZModuleFactory.Instance.Create(moduleID))
			{
#if DEBUG
				lastCreatedModule = module;
#endif
				ZFilterGridModule filterGridModule = module as ZFilterGridModule;
				if (filterGridModule != null)
				{
					ZFormModaliser.ShowDialogAndDispose(new EmbeddedModulePopup(filterGridModule));
				}
				else
				{
					ZPopupModule popupModule = module as ZPopupModule;
					if (popupModule != null)
					{
						popupModule.ShowModal(FindForm());
					}
					else
					{
						Globals.Message.ShowError(Res.GetString("6f86aee4-efba-4ed5-b714-d527fc4308ea", "The module ({0}) has a type of {1}, which is not supported.", moduleID.ToString(), module.GetType()));
					}
				}
			}
		}

#if DEBUG
		internal ZModule LastCreatedModule
		{
			get { return lastCreatedModule; }
		}

		ZModule lastCreatedModule;
#endif
		readonly ModuleIdentifier moduleID;
	}
}
