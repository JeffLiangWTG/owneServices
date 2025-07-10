using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Design;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Modules
{
	[SuppressFormDesignerAnalysis]
	[ToolboxItem(false)]
	public class ZEmbeddedModuleControl : ZUserControl, IDialogKeyDown
	{
		public ZEmbeddedModuleControl()
		{
			if (DesignModeFinder.IsDesigning)
			{
				DesignTimeEnvironment.InitializeDesignTimeEarlyWithoutServiceProvider();
			}
		}

		#region IResourceStringParentControl Members

		event KeyEventHandler IDialogKeyDown.DialogKeyDown
		{
			add { dialogKeyDown += value; }
			remove { dialogKeyDown -= value; }
		}
		event KeyEventHandler dialogKeyDown;

		#endregion

		protected override bool ProcessDialogKey(Keys keyData)
		{
			var result = base.ProcessDialogKey(keyData);

			if (dialogKeyDown != null)
			{
				var args = new KeyEventArgs(keyData);
				dialogKeyDown(ActiveControl.GetFrontMostActiveControl(), args);
				result |= args.Handled;
			}

			return result;
		}

		#region Site

		public override ISite Site
		{
			get { return base.Site; }
			set
			{
				base.Site = value;
				DesignTimeEnvironment.InitializeDesignTimeWithServiceProvider(value);
			}
		}

		#endregion
	}
}
