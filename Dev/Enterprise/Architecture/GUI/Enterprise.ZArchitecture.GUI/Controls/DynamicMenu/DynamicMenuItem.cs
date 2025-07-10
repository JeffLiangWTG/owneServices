using System;
using System.Collections;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.GUI
{
	public class DynamicMenuItem : MenuItem, IDynamicMenu
	{
		public DynamicMenuItem()
			: base()
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		public DynamicMenuItem(string text)
			: base(text)
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}
			base.Dispose(isNotFinalizing);
		}

		#region IDynamicMenu

		IList IMenuItem.MenuItems
		{
			get { return MenuItems; }
		}

		event EventHandler IDynamicMenu.Opening
		{
			add { Popup += value; }
			remove { Popup -= value; }
		}

		#endregion
	}
}
