using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class NctsGoodsItemsUserControl : ZUserControl
	{
		public NctsGoodsItemsUserControl()
		{
			InitializeComponent();
			InitGoodsItemsGridUserControl();
			ItemPreviousDocumentsTabPage.RunWhenBindingOrFirstShown((s, args) => InitPreviousDocumentsUserControl());
			ItemPackagesTabPage.RunWhenBindingOrFirstShown((s, args) => InitPackagesUserControl());
			ItemSecurityTabPage.RunWhenBindingOrFirstShown((s, args) => InitItemsSecurityUserControl());
			ItemAdditionalInfosTabPage.RunWhenBindingOrFirstShown((s, args) => InitAdditionalInfosUserControl());
			ItemDetailsTabPage.RunWhenBindingOrFirstShown((s, args) => InitItemDetailsUserControl());
			SupportingDocumentsTabPage.RunWhenBindingOrFirstShown((s, args) => InitSupportingDocumentsUserControl());
			ItemContainersTabPage.RunWhenBindingOrFirstShown((s, args) => InitGoodsItemContainersUserControl());
		}

		public virtual void ShowHideSecurityTab(bool isSafetyAndSecurity)
		{
			var control = GoodsItemsGridDynamicUserControl.HostedControl as GoodsItemsGridUserControl;

			if (isSafetyAndSecurity)
			{
				if (!GoodsItemsTabControl.TabPages.Contains(ItemSecurityTabPage))
				{
					GoodsItemsTabControl.TabPages.Insert(ItemSecurityTabPage, 6);
				}

				if (control != null)
				{
					control.GoodsItemsGrid.AddToAvailableColumns(NctsCommonCargoDesc.Schema.BY_CommercialReferenceNumber);
					control.GoodsItemsGrid.AddToAvailableColumns(NctsCommonCargoDesc.Schema.BY_TransportChargesMethodOfPayment);
				}
			}
			else
			{
				if (GoodsItemsTabControl.TabPages.Contains(ItemSecurityTabPage))
				{
					GoodsItemsTabControl.TabPages.Remove(ItemSecurityTabPage);
				}
				if (control != null)
				{
					control.GoodsItemsGrid.RemoveFromAvailableColumns(NctsCommonCargoDesc.Schema.BY_CommercialReferenceNumber);
					control.GoodsItemsGrid.RemoveFromAvailableColumns(NctsCommonCargoDesc.Schema.BY_TransportChargesMethodOfPayment);
				}
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			header = dataSource as NctsHeader;
			if (header != null)
			{
				header.BH_FTZMoveInfo.ValueChanged -= IsSafetyAndSecurityInfo_ValueChanged;
				header.BH_FTZMoveInfo.ValueChanged += IsSafetyAndSecurityInfo_ValueChanged;
				ShowHideSecurityTab(header.BH_FTZMove);
				InitTabsVisibility();
			}
		}
		protected NctsHeader header;

		protected override void Dispose(bool disposing)
		{
			if (header != null)
			{
				header.BH_FTZMoveInfo.ValueChanged -= IsSafetyAndSecurityInfo_ValueChanged;
			}

			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
			DisposeItemSecurityTabPage();
		}

		void DisposeItemSecurityTabPage()
		{
			if (ItemSecurityTabPage != null && !ItemSecurityTabPage.IsDisposed)
			{
				ItemSecurityTabPage.Dispose();
			}
		}

		protected virtual Type GetGoodsItemContainersUserControl() => typeof(GoodsItemContainersUserControl);

		protected virtual Type GetSupportingDocumentsUserControl() => typeof(SupportingDocumentsUserControl);

		protected virtual Type GetPreviousDocumentsUserControlType() => typeof(PreviousDocumentsUserControl);

		protected virtual Type GetPackagesUserControlType() => typeof(ItemPackagesTabUserControl);

		protected virtual Type GetItemsSecurityUserControlType() => typeof(ItemSecurityTabUserControl);

		protected virtual Type GetAdditionalInfosUserControlType() => typeof(AdditionalInfosUserControl);

		protected virtual Type GetItemDetailsUserControlType() => typeof(ItemDetailsUserControl);

		protected virtual Type GetGoodsItemsGridUserControlType() => typeof(GoodsItemsGridUserControl);

		void IsSafetyAndSecurityInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowHideSecurityTab(header.BH_FTZMove);
		}

		void InitPreviousDocumentsUserControl()
		{
			NctsPreviousDocumentsDynamicUserControl.UserControlType = GetPreviousDocumentsUserControlType();
		}

		void InitPackagesUserControl()
		{
			NctsPackagesDynamicUserControl.UserControlType = GetPackagesUserControlType();
		}

		void InitItemsSecurityUserControl()
		{
			ItemSecurityDynamicUserControl.UserControlType = GetItemsSecurityUserControlType();
		}

		void InitAdditionalInfosUserControl()
		{
			AdditionalInfosDynamicUserControl.UserControlType = GetAdditionalInfosUserControlType();
		}

		void InitItemDetailsUserControl()
		{
			GoodsItemLineDetailDynamicUserControl.UserControlType = GetItemDetailsUserControlType();
		}

		void InitGoodsItemsGridUserControl()
		{
			GoodsItemsGridDynamicUserControl.UserControlType = GetGoodsItemsGridUserControlType();
		}

		void InitSupportingDocumentsUserControl()
		{
			SupportingDocumentsDynamicUserControl.UserControlType = GetSupportingDocumentsUserControl();
		}

		void InitGoodsItemContainersUserControl()
		{
			GoodsItemContainersDynamicUserControl.UserControlType = GetGoodsItemContainersUserControl();
		}

		void InitTabsVisibility()
		{
			var configuration = !DesignModeFinder.IsDesigning ? NctsConfiguration.GetConfiguration(header.Factory, header.BrokerageCountryCode) : null;
			if (configuration != null)
			{
				SupportingDocumentsTabPage.TabVisible = configuration.GoodsItemsConfiguration.SupportingDocumentsSupport(header);
				ItemAdditionalInfosTabPage.TabVisible = configuration.GoodsItemsConfiguration.AdditionalInfosSupport(header);
				ItemPreviousDocumentsTabPage.TabVisible = configuration.GoodsItemsConfiguration.PreviousDocumentsSupport(header);
			}
		}
	}
}
