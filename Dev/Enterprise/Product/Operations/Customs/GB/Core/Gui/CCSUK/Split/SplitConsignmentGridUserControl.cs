using System;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class SplitConsignmentGridUserControl : ZUserControl
	{
		public SplitConsignmentGridUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			AddCcsukMessagingMenuForSplits();
		}

		protected virtual ModuleIdentifier ModuleId
		{
			get { return ModuleIDs.Customs.EU.GB.CcsukSplitHouse; }
		}

		void AddCcsukMessagingMenuForSplits()
		{
			var innerGrid = splitConsignmentModuleButtonGrid.InnerGrid;
			var managerForSplits = new CusAwbDelegateProvider(delegate
				{
					var split = innerGrid.SelectedElements.Length == 1 ? (SplitConsignment)innerGrid.SelectedElements[0] : null;
					if (split != null && split.AWB.HasChanges)
					{
						split = null; // force the "please save first..." message
					}
					return split;
				});
			var ccsukMessagingMenuForSplits = new CcsukMenu(managerForSplits, (ZForm)ParentForm, false, true);
			splitConsignmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.Add(0, ccsukMessagingMenuForSplits);
		}
	}
}
