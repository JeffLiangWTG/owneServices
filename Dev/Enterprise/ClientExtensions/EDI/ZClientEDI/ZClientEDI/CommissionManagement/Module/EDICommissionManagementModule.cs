using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.CommissionManagement.Business;
using Enterprise.Client.EDI.CommissionManagement.GUI;
using Enterprise.CommissionManagement.GUI;
using Enterprise.CommissionManagement.Module;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Res;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.CommissionManagement.Module
{
	public class EDICommissionManagementModule : CommissionManagementModule
	{
		#region FilterControl

		protected override IFilterControl GetNewFilterControl()
		{
			return new EDICommissionManagementFilterControl(GridCollection, (CommissionManagementFilterBusinessObject)FilterBusinessObject);
		}

		#endregion

		#region Menu Items

		protected override MenuItem[] GetNewAdditionalMenuItems()
		{
			return GetNewAdditionalMenuItemsAsEnumerable().ToArray();
		}

		IEnumerable<MenuItem> GetNewAdditionalMenuItemsAsEnumerable()
		{
			var baseAdditionalMenuItems = base.GetNewAdditionalMenuItems();
			foreach (var menuItem in baseAdditionalMenuItems)
			{
				yield return menuItem;

				if (menuItem.Tag as string == AgreementApprovalMenuItemTag)
				{
					yield return GetNewAmbiguityResolverMenuItem();
				}
			}
		}

		ZMenuItem GetNewAmbiguityResolverMenuItem()
		{
			return new ZMenuItem(ResString.GetMultilingualString("62e4e755-da5f-4b9e-a450-f516f9bd2d82", "Ambiguity Resolver"), OnAmbiguousCommissionResolverMenuItemClick) { Tag = AmbiguityResolverMenuItemTag };
		}

		public const string AmbiguityResolverMenuItemTag = "AmbiguityResolverMenuItemTag";

		protected override void SetupButtonDetailForItem(MenuItem item, ref IconTypes buttonImage, ref IconTypes buttonImageActive, ref string buttonToolTip)
		{
			base.SetupButtonDetailForItem(item, ref buttonImage, ref buttonImageActive, ref buttonToolTip);

			if (item.Tag == (object)AmbiguityResolverMenuItemTag)
			{
				buttonImage = IconTypes.Warning;
				buttonImageActive = IconTypes.None;
				buttonToolTip = Res.GetString("85b29fe3-3575-445e-b5be-5c1ee07260b3", "Open Commission Ambiguity Resolver Form");
			}
		}

		void OnAmbiguousCommissionResolverMenuItemClick(object sender, EventArgs e)
		{
			ShowAmbiguousCommissionResolverIfAllowed();
		}

		void ShowAmbiguousCommissionResolverIfAllowed()
		{
			if (!EDISecurityCheckpoints.CommissionResolveAmbiguity.IsAllowed)
			{
				EDISecurityCheckpoints.CommissionResolveAmbiguity.ShowError();
			}
			else
			{
				var factory = new BusinessObjectFactory();
				var filterBizObj = new AmbiguousCommissionResolverFilterBusinessObject(factory);
				var resolver = new AmbiguousCommissionResolver(filterBizObj);
				var commissionResolverForm = new AmbiguousCommissionResolverForm(resolver);
				commissionResolverForm.Show();
			}
		}

		protected override void OnCommissionFinaliserMenuItemClick(object sender, EventArgs e)
		{
			var unresolvedCommissionsExist = Factory.LoadTop1<AccAmbiguousCommission>(new ZQuery(AccAmbiguousCommissionSchema.AC0_CA0_SelectedAgreement, null)) != null;
			if (unresolvedCommissionsExist)
			{
				var dialogResult = Globals.Message.Show(
						Res.GetString("c7cf01c0-4fad-4118-b4f3-30ee7d2b673b", "Unresolved commissions exist. Do you wish to resolve these before finalizing entity commissions?"),
						Res.GetString("da6358d0-151b-4b8f-9d4e-8f8331b16cbf", "Unresolved Commissions Exist"),
						MessageBoxButtons.YesNoCancel,
						DialogResult.Cancel);

				if (dialogResult == DialogResult.Yes)
				{
					ShowAmbiguousCommissionResolverIfAllowed();
				}
				else if (dialogResult == DialogResult.No)
				{
					base.OnCommissionFinaliserMenuItemClick(sender, e);
				}
				else
				{
					// Do nothing
				}
			}
			else
			{
				base.OnCommissionFinaliserMenuItemClick(sender, e);
			}
		}

		#endregion
	}
}
