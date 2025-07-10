using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	public class ActionDataMenuItem : ZMenuItem
	{
		protected ActionDataMenuItem(IDataBoundControl owner)
			: base(ResString.GetMultilingualString("c3df90ca-f3ad-4835-9949-715c03649728", "&Data"))
		{
			this.owner = owner;
		}

		protected override void OnPopup(EventArgs e)
		{
			base.OnPopup(e);
			Populate();
		}

		internal void Populate()
		{
			MenuItems.Clear();
			AddCustomMenuItems(owner.DataSource as IBusiness);
		}

		public static ActionDataMenuItem New(IDataBoundControl owner)
		{
			var overridden = OverridableNewDelegate.Value;
			if (overridden == null)
			{
				return new ActionDataMenuItem(owner);
			}
			else
			{
				return overridden(owner);
			}
		}

		protected virtual void AddCustomMenuItems(IBusiness businessEntity)
		{
		}

		protected delegate ActionDataMenuItem ConstructorDelegate(IDataBoundControl owner);
		protected static readonly Overridable<ConstructorDelegate> OverridableNewDelegate = new Overridable<ConstructorDelegate>();

		protected IBusiness BusinessEntity
		{
			get { return owner.DataSource as IBusiness; }
		}

		readonly IDataBoundControl owner;
	}
}
