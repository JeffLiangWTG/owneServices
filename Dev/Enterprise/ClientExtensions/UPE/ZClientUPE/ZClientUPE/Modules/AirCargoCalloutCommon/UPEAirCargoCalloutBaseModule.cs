using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.GUI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.Module
{
	public abstract class UPEAirCargoCalloutBaseModule : ZFilterGridModule
	{
		public new UPEAirCargoCalloutBaseFilterBusinessObject FilterBusinessObject
		{
			get
			{
				return (UPEAirCargoCalloutBaseFilterBusinessObject)base.FilterBusinessObject;
			}
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get
			{
				return Env.Licence.AlwaysAllow;
			}
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get
			{
				return Env.Security.ACAHouse;
			}
		}

		public override bool AllowNew
		{
			get
			{
				return true;
			}
		}

		public override bool AllowDelete
		{
			get
			{
				return false;
			}
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ModuleHAWBCollection(Factory);
		}

		#region ZToolBarButton BulkStatusUpdatingMenuItem;

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewStandardMenuItems());
			if (GlbStaff.CurrentUser.GS_IsController)
			{
				result.Add(BulkStatusUpdatingMenuItem);
			}
			return result.ToArray();
		}

		#region BulkStatusUpdatingMenuItem
		MenuItem BulkStatusUpdatingMenuItem
		{
			get
			{
				if (bulkStatusUpdatingMenuItem == null)
				{
					bulkStatusUpdatingMenuItem = NewBulkStatusUpdatingMenuItem();
					bulkStatusUpdatingMenuItem.Click += new EventHandler(OnBulkStatusUpdatingToolbarButton_Click);
				}
				return bulkStatusUpdatingMenuItem;
			}
		}
		MenuItem bulkStatusUpdatingMenuItem;
		#endregion

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>();
			result.AddRange(base.GetNewActionMenuItems());
			result.Add(new ZMenuItem("View Entry Print", new EventHandler(OnViewEntryPrint_Click)));
			return result.ToArray();
		}

		protected virtual MenuItem NewBulkStatusUpdatingMenuItem()
		{
			return new ZMenuItem("Bulk Status Update");
		}

		void OnBulkStatusUpdatingToolbarButton_Click(object sender, EventArgs e)
		{
			ZFilterStripControl filterControl = (ZFilterStripControl)EmbeddedControl;
			using (BulkStatusUpdatingForm form = NewBulkStatusUpdatingForm(filterControl.FilteredGrid.SelectedElements))
			{
				form.ShowDialog(EmbeddedControl);
			}
		}

		protected virtual BulkStatusUpdatingForm NewBulkStatusUpdatingForm(BusinessObject[] selectedItems)
		{
			return new BulkStatusUpdatingForm(FilterBusinessObject, selectedItems);
		}

		#endregion

		void OnViewEntryPrint_Click(object sender, EventArgs e)
		{
			ZFilterStripControl filterControl = (ZFilterStripControl)EmbeddedControl;
			OnViewEntryPrint_Click(filterControl.FilteredGrid.SelectedElements);
		}

		protected virtual void OnViewEntryPrint_Click(BusinessObject[] selectedGridItems)
		{
			UPECusHAWB hAWB = (selectedGridItems.Length == 1) ? (UPECusHAWB)selectedGridItems[0] : null;
			GetEntryPrintDocumentHelper().Run(hAWB);
		}

		protected virtual EntryPrintDocumentHelper GetEntryPrintDocumentHelper()
		{
			return new EntryPrintDocumentHelper();
		}
	}
}
