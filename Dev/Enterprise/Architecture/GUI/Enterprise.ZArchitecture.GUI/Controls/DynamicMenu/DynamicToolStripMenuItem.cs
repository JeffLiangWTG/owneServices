using System;
using System.Collections;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.GUI
{
	public class DynamicToolStripMenuItem : ToolStripMenuItem, IDynamicMenu
	{
		public DynamicToolStripMenuItem()
			: base()
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		public DynamicToolStripMenuItem(string text)
			: base(text)
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);
			this.Name = text;
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}
			base.Dispose(isNotFinalizing);
		}

		#region IMenuItem

		IList IMenuItem.MenuItems
		{
			get { return DropDownItems; }
		}

		event EventHandler IDynamicMenu.Opening
		{
			add { DropDownOpening += value; }
			remove { DropDownOpening -= value; }
		}

		#endregion
	}
}
