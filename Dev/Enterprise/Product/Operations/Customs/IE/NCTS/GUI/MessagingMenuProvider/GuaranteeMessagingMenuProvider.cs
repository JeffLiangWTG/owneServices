using System;
using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.Customs.IE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.NCTS.GUI
{
	public class GuaranteeMessagingMenuProvider : Customs.GUI.GuaranteeMessagingMenuProvider
	{
		public GuaranteeMessagingMenuProvider(BaseCusGuaranteeHeader header) : base(header)
		{
		}

		public new IE.Business.CusGuaranteeHeader Header => (IE.Business.CusGuaranteeHeader)base.Header;

		protected override IEnumerable<ZMenuItem> CreateMenuItemsCore()
		{
			yield return GuaranteeVoucherSoldMenuItem;
			yield return GuaranteeAccessCodesMenuItem;
		}

		#region Guarantee Voucher Sold

		ZMenuItem GuaranteeVoucherSoldMenuItem => guaranteeVoucherSoldMenuItem ?? (guaranteeVoucherSoldMenuItem = CreateNewGuaranteeVoucherSoldMenuItem());
		ZMenuItem guaranteeVoucherSoldMenuItem;

		ZMenuItem CreateNewGuaranteeVoucherSoldMenuItem() => new ZMenuItem(ResString.GetMultilingualString("72450ECD-67A5-420F-A732-54AB82D02CA5", "Guarantee Voucher Sold"), GuaranteeVoucherSoldClick);

		void GuaranteeVoucherSoldClick(object sender, EventArgs e)
		{
			var menuItem = (ZMenuItem)sender;
			if (CheckBeforeSending(menuItem))
			{
				GuaranteeVoucherSoldMessageSendingForm.ShowForm(Header);
			}
		}

		#endregion

		#region Guarantee Access Codes

		ZMenuItem GuaranteeAccessCodesMenuItem => fGuaranteeAccessCodesMenuItem ?? (fGuaranteeAccessCodesMenuItem = CreateNewGuaranteeAccessCodesMenuItem());
		ZMenuItem fGuaranteeAccessCodesMenuItem;

		ZMenuItem CreateNewGuaranteeAccessCodesMenuItem() => new ZMenuItem(ResString.GetMultilingualString("9A9B6270-ABF1-4831-841B-0DDB2BDBE44F", "Update Access Code"), GuaranteeAccessCodesClick);

		void GuaranteeAccessCodesClick(object sender, EventArgs e)
		{
			if (CheckBeforeSending((ZMenuItem)sender))
			{
				GuaranteeAccessCodesSendingForm.ShowForm(Header);
			}
		}

		#endregion

		bool CheckBeforeSending(ZMenuItem menuItem) => PreSaveHeader(menuItem) && GlbCompany.CurrentCompany.CheckValidROSCredentialAndSetInvalidIfCredentialHasExpired();

		bool PreSaveHeader(ZMenuItem menuItem)
		{
			var topLevelBizObj = Header;
			return CustomsPlugIn.FormPreSaved(topLevelBizObj, GetForm(menuItem));
		}

		protected ZForm GetForm(ZMenuItem menuItem) => (ZForm)menuItem.GetMainMenu().GetForm();
	}
}
